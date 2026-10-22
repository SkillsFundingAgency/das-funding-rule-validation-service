# Flex Consumption hosting (DASD-16646)

The validation service can be deployed as a second, separate function app on the Azure Functions
[Flex Consumption plan](https://learn.microsoft.com/azure/azure-functions/flex-consumption-plan).
Azure doesn't support moving an existing app to Flex in place, so this runs side by side with the
existing App Service plan app until cutover. None of the existing resources, templates or pipelines are changed.

| File | Purpose |
|---|---|
| `azure/template.flex.json` | Subscription-level ARM template for the Flex app and its supporting resources |
| `pipeline-templates/job/deploy-flex.yml` | ARM deploy, then `AzureFunctionApp@2` with `isFlexConsumption: true` |
| `azure-pipelines-flex.yml` | Manual pipeline: build, then deploy to AT, TEST, TEST2 and DEMO |

## What gets created per environment

| Resource | Name |
|---|---|
| Resource group | `das-<env>-fnrvs-flex-rg` |
| Flex Consumption plan (FC1, Linux) | `das-<env>-fnrvs-flex-asp` |
| Function app (.NET 10 isolated) | `das-<env>-fnrvs-flex-fa` |
| Storage account (host storage, Durable task hub, deployment package) | `das<env>fnrvsflexstr` |
| Application Insights | `das-<env>-fnrvs-flex-fa` |
| Subnet in the shared vnet, delegated to `Microsoft.App/environments` | `fnrvs-flex-sn` |
| Role assignment: Service Bus Data Owner on the shared namespace | |

The funding rules table stays in the existing `das<env>fnrvsstr` account. The template reads that account's key at deploy
time, but doesn't redeploy or change the account. The Durable task hub lives in the Flex app's own storage account, as
[Microsoft recommends for Durable Functions](https://learn.microsoft.com/azure/azure-functions/storage-considerations#storage-account-guidance).

## Before the first run

1. **Register the `Microsoft.App` resource provider** in each subscription. It's currently `NotRegistered` in SFA-DAS-Dev/Test,
   and Flex VNet integration needs it.
2. **Confirm the subnet range** with the platform team. `azure-pipelines-flex.yml` proposes `<prefix>.234.0/24`,
   which was free in the AT, TEST, TEST2 and DEMO vnets on 22/10/2026.
3. **Create the pipeline** in Azure DevOps from `azure-pipelines-flex.yml`. It has no CI trigger.

## Scaling settings

All settings are template parameters. Each can be overridden with a pipeline variable of the same name.

| Parameter | Default | Notes |
|---|---|---|
| `flexMaximumInstanceCount` | 100 | Per function scale group. TBC on the ticket |
| `flexInstanceMemoryMB` | 2048 | 512, 2048 or 4096 |
| `flexServiceBusMaxConcurrentCalls` | 16 | Messages per instance. TBC on the ticket |
| `flexDurablePartitionCount` | 16 | Caps orchestrator scale-out. Fixed once the task hub exists |
| `flexDurableMaxQueuePollingInterval` | `00:00:10` | Microsoft recommends 10 seconds or less on Flex |
| `flexAlwaysReadyDurableInstances` | 0 | Microsoft recommends 1 for Durable Functions; 0 keeps lower environments scale-to-zero |
| `flexTriggersEnabled` | `false` | See cutover below |

The host.json values are applied as `AzureFunctionsJobHost__*` app settings on the Flex app only. That leaves `host.json`, and the
existing task hub's partition count, unchanged.

**Where the load lands:** `FundingRuleServiceBusTrigger` only starts an orchestration, so the `validate-learner-requests` queue
drains quickly. The actual work then scales in the `durable` group, based on the Durable storage queues rather than Service Bus.
Orchestrator throughput is capped by `flexDurablePartitionCount` (maximum 16). Load test with real volumes before fixing the
concurrency and instance numbers; if 16 partitions isn't enough, consider Durable Task Scheduler.

## Cutover

The Flex app is deployed with `FundingRuleServiceBusTrigger` and `FundingRuleHttpTrigger` **disabled**, so it doesn't compete with the
existing app for `validate-learner-requests` messages. Each request carries the Bridge's waiting instance id, so this app can be
switched independently of the Bridge:

1. Stop `das-<env>-fnrvs-fa`.
2. Run the Flex pipeline with `flexTriggersEnabled` set to `true`.

Requests still mid-orchestration in the old app's task hub are lost when it stops, and time out on the Bridge side after
10 minutes, so cut over when the queue is idle. Roll back by reversing the steps.

## Known follow-ups

- PP, PROD and MO stages are deliberately not included yet.
- The DACPAC and SQL database aren't used at runtime (rules are read from table storage), so the Flex pipeline doesn't deploy them.
