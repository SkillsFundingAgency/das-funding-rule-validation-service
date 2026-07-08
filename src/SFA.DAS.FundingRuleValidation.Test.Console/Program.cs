using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using SFA.DAS.FundingRuleValidation.Jobs.Core;
using SFA.DAS.FundingRuleValidation.Jobs.Domain;

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
        TrainingType = TrainingType.Apprenticeship,
        Type = CourseType.Apprenticeship,
    }
};

var command = new ValidateLearnerCommand(Guid.NewGuid().ToString(), ukprn, uln, courses);

var cts = new CancellationTokenSource();
var token = cts.Token;
var readerTask = Task.Run(ReceiveMonitor);

while (true)
{
    var key = Console.ReadKey();
    switch (key.Key)
    {
        case ConsoleKey.Spacebar:
            var cursorPos = Console.GetCursorPosition();
            Console.SetCursorPosition(cursorPos.Left-1, cursorPos.Top);
            Console.WriteLine("Sending message...");
            await sender.SendMessageAsync(new ServiceBusMessage(JsonSerializer.Serialize(command)));
            break;
        case ConsoleKey.Escape:
            cts.Cancel();
            await readerTask;
            Environment.Exit(0);
            break;
    }
}

async Task ReceiveMonitor()
{
    while (true)
    {
        try
        {
            var message = await receiver.ReceiveMessageAsync(cancellationToken: token);
            if (token.IsCancellationRequested) break;
            await receiver.CompleteMessageAsync(message, token);
            Console.WriteLine($"Received message: {message.Body}");
        }
        catch (TaskCanceledException)
        {
            break;
        }
    }
}
