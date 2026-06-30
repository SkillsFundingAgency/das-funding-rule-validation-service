## ⛔Never push sensitive information such as client id's, secrets or keys into repositories including in the README file⛔

# Funding Rules Validation

<img src="https://avatars.githubusercontent.com/u/9841374?s=200&v=4" align="right" alt="UK Government logo">


[![Build Status](https://dev.azure.com/sfa-gov-uk/Digital%20Apprenticeship%20Service/_apis/build/status/4377?branchName=master)](https://dev.azure.com/sfa-gov-uk/Digital%20Apprenticeship%20Service/_build/latest?definitionId=4377&branchName=master)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=_projectId_&metric=alert_status)](https://sonarcloud.io/dashboard?id=_projectId_)
[![License](https://img.shields.io/badge/license-MIT-lightgrey.svg?longCache=true&style=flat-square)](https://en.wikipedia.org/wiki/MIT_License)

This repository contains the Funding Rules Validation function app that runs the core learner funding validation rules.
1. Receives funding validation requests from the [Funding Bridge](https://github.com/SkillsFundingAgency/das-sfa-funding-rule-service-bridge)
2. Processes the received information against the necessary rules
3. Returns the validation result to the [Funding Bridge](https://github.com/SkillsFundingAgency/das-sfa-funding-rule-service-bridge)


## How It Works

The validation engine is built as an [Azure Durable Function](https://learn.microsoft.com/en-us/azure/durable-task/durable-functions/durable-functions-overview).

The app receives a validation request from the [Bridge Application](https://github.com/SkillsFundingAgency/das-sfa-funding-rule-service-bridge) via a Azure ServiceBus queue. Each request represents a single learner with one or more courses that need to be checked, to see if there are any reasons the learner does not qualify for funding.

Once a message is received, an orchestration is scheduled that performs the following tasks:
1. Fetches the applicable validation rules to run
2. Schedules each rule to run, passing the learner information and the course(s) to validate
3. Schedules the result of the validation to be sent back onto a ServiceBus queue


## 🚀 Installation

### Pre-Requisites

* A clone of this repository
* [Azure Storage emulator](https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite)
* [Azure ServiceBus emulator](https://learn.microsoft.com/en-us/azure/service-bus-messaging/test-locally-with-service-bus-emulator?tabs=automated-script) (or optionally SQL Server)

### Config

You can find the latest config file in [das-employer-config](https://github.com/SkillsFundingAgency/das-employer-config/blob/master/das-funding-rule-validation-service) repository.

The function app project is configured for User Secrets for local development, so add `ServiceBusConnectionString` with the correct value. For local development when using the emulator this is normally:
```
Endpoint=sb://localhost:5672;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;
```

#### Azure Table Storage config

Row Key: SFA.DAS.FundingRuleValidation.Jobs_1.0
Partition Key: LOCAL

Depending on the storage mechanism used, you will either want to set the `SqlConnectionString` or the `TableStorageConnectionString`, and configure the dependency container to inject the correct repository.

Data:

```json
{
    "ConnectionStrings": {
        "SqlConnectionString": "...",
        "TableStorageConnectionString": "UseDevelopmentStorage=true"
    }
}
```

## Technologies

* .Net10
* Azure Functions V4
* SQL Server
* Azure Table Storage
* Azure ServiceBus
* NUnit
* Moq
* FluentAssertions


## 🐛 Known Issues

None