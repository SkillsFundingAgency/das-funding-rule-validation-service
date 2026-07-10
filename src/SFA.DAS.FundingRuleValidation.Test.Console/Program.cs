using Azure.Messaging.ServiceBus;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SFA.DAS.FundingRuleValidation.Jobs.Core;
using SFA.DAS.FundingRuleValidation.Jobs.Domain;
using SFA.DAS.FundingRuleValidation.Test.Console;

var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
var connectionString = config[GlobalConstants.ServiceBusConnectionName];

// send a message
await using var client = new ServiceBusClient(connectionString);
await using var sender = client.CreateSender(GlobalConstants.IncomingQueueName);
await using var receiver = client.CreateReceiver(GlobalConstants.OutgoingQueueName);

const string ukprn = "10000";
const string uln = "1234567890";
var courses = new List<Course>
{
    new()
    {
        Id = "090D5B59-3839-49CE-8A32-038A312B1F4A",
        AgeAtStartOfCourse = 27,
        StartDate = new DateTime(2026, 5, 31),
        EndDate = DateTime.UtcNow.AddMonths(6),
        PlannedEndDate = DateTime.UtcNow.AddMonths(6),
        Status = LearnerCourseStatus.InLearning,
        TrainingType = TrainingType.Standard,
        Type = CourseType.Apprenticeship,
    }
};

var cts = new CancellationTokenSource();
var token = cts.Token;

while (true)
{
    Console.WriteLine("1 = process-job, Ctrl+1 to send job message");
    Console.WriteLine($"2 = {GlobalConstants.IncomingQueueName}");
    Console.WriteLine($"3 = {GlobalConstants.OutgoingQueueName}");
    Console.WriteLine("4 = job-complete");
    Console.WriteLine();
    Console.WriteLine("Shift+<n> to clear the queue messages");
    
    var key = Console.ReadKey();
    Console.Clear();
    
    switch (key.Key)
    {
        case ConsoleKey.D1:
            if (key.Modifiers.HasFlag(ConsoleModifiers.Control))
            {
                Console.WriteLine("Sending job 1 message");
                await client.SendJobAsync("job-1.xml", token);
                break;    
            }
            
            if (key.Modifiers.HasFlag(ConsoleModifiers.Shift))
            {
                Console.WriteLine("Clearing job queue");
                await client.ClearQueueAsync("process-job", token);
                break;
            }

            Console.WriteLine("Peeking job messages");
            var jobMessages = await client.PeekQueueAsync("process-job", token);
            if (jobMessages is not { Count: > 0 })
            {
                Console.WriteLine("No job messages");
                break;
            }

            foreach (var jobMessage in jobMessages)
            {
                Console.WriteLine($"Job message: {jobMessage}");
            }

            break;
        case ConsoleKey.D2:
            if (key.Modifiers.HasFlag(ConsoleModifiers.Shift))
            {
                Console.WriteLine("Clearing validation requests");
                await client.ClearQueueAsync(GlobalConstants.IncomingQueueName, token);
                break;
            }

            Console.WriteLine("Peeking validation requests");
            var validationRequests = await client.PeekQueueAsync(GlobalConstants.IncomingQueueName, token);
            if (validationRequests is not { Count: > 0 })
            {
                Console.WriteLine("No validation requests");
                break;
            }

            foreach (var validationRequest in validationRequests)
            {
                Console.WriteLine($"Validation request: {validationRequest}");
            }

            break;
        case ConsoleKey.D3:
            if (key.Modifiers.HasFlag(ConsoleModifiers.Shift))
            {
                Console.WriteLine("Clearing validation results");
                await client.ClearQueueAsync(GlobalConstants.OutgoingQueueName, token);
                break;
            }

            Console.WriteLine("Peeking validation results");
            var validationResults = await client.PeekQueueAsync(GlobalConstants.OutgoingQueueName, token);
            if (validationResults is not { Count: > 0 })
            {
                Console.WriteLine("No validation results");
                break;
            }

            foreach (var validationResult in validationResults)
            {
                Console.WriteLine($"Validation result: {validationResult}");
            }

            break;
        case ConsoleKey.D4:
            if (key.Modifiers.HasFlag(ConsoleModifiers.Shift))
            {
                Console.WriteLine("Clearing job results");
                await client.ClearQueueAsync("job-complete", token);
                break;
            }

            Console.WriteLine("Peeking job results");
            var jobResults = await client.PeekQueueAsync("job-complete", token);
            if (jobResults is not { Count: > 0 })
            {
                Console.WriteLine("No job results");
                break;
            }

            foreach (var jobResult in jobResults)
            {
                Console.WriteLine($"Job result: {jobResult}");
            }

            break;
        case ConsoleKey.Q:
        case ConsoleKey.Escape:
            cts.Cancel();
            Console.Clear();
            break;
        default: continue;
    }
    
    Console.WriteLine();
}