using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.Configuration;
using SFA.DAS.FundingRuleValidation.Jobs.Core;
using SFA.DAS.FundingRuleValidation.Jobs.Domain;

var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
var adminConnectionString = config["AdminServiceBusConnection"];
var connectionString = config[GlobalConstants.ServiceBusConnectionName];
const string queueName = "validate-learner";

// create the queue if it doesn't exist
var adminClient = new ServiceBusAdministrationClient(adminConnectionString);
if (await adminClient.QueueExistsAsync(GlobalConstants.IncomingQueueName) == false)
{
    await adminClient.CreateQueueAsync(new CreateQueueOptions(queueName));
}

// send a message
await using var client = new ServiceBusClient(connectionString);
await using var sender = client.CreateSender(queueName);

const long ukprn = 10000;
const long uln = 1234567890;
var courses = new List<Course>();

var command = new ValidateLearnerCommand(Guid.NewGuid(), ukprn, uln, courses);
await sender.SendMessageAsync(new ServiceBusMessage(JsonSerializer.Serialize(command)));