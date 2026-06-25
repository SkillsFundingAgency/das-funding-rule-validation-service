using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.Configuration;
using SFA.DAS.FundingRuleValidation.Jobs.Core;
using SFA.DAS.FundingRuleValidation.Jobs.Domain;

var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
var adminConnectionString = config["AdminServiceBusConnection"];
var connectionString = config[GlobalConstants.ServiceBusConnectionName];

// create the queue if it doesn't exist
var adminClient = new ServiceBusAdministrationClient(adminConnectionString);
if (await adminClient.QueueExistsAsync(GlobalConstants.IncomingQueueName) == false)
{
    await adminClient.CreateQueueAsync(new CreateQueueOptions(GlobalConstants.IncomingQueueName));
}

if (await adminClient.QueueExistsAsync(GlobalConstants.OutgoingQueueName) == false)
{
    await adminClient.CreateQueueAsync(new CreateQueueOptions(GlobalConstants.OutgoingQueueName));
}

// send a message
await using var client = new ServiceBusClient(connectionString);
await using var sender = client.CreateSender(GlobalConstants.IncomingQueueName);
await using var receiver = client.CreateReceiver(GlobalConstants.OutgoingQueueName);

const long ukprn = 10000;
const long uln = 1234567890;
var courses = new List<Course>
{
    new()
    {
        Id = "Course1",
        AgeAtStartOfCourse = 27,
        StartDate = DateTime.Now.AddMonths(6),
        EndDate = DateTime.UtcNow.AddMonths(12),
        PlannedEndDate = DateTime.UtcNow.AddMonths(12),
        Status = LearnerCourseStatus.InLearning,
        TrainingType = TrainingType.Apprenticeship,
        Type = CourseType.Apprenticeship,
    }
};

var command = new ValidateLearnerCommand(Guid.NewGuid(), ukprn, uln, courses);
await sender.SendMessageAsync(new ServiceBusMessage(JsonSerializer.Serialize(command)));

Console.WriteLine("Waiting for result");
var message = await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(300));
await receiver.CompleteMessageAsync(message);

Console.WriteLine($"Received message: {message.Body}");
Console.ReadLine();