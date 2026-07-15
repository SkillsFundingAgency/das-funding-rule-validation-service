using System.Diagnostics.CodeAnalysis;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using SFA.DAS.FundingRuleValidation.Jobs.Core;
using SystemConsole = System.Console;

[assembly: ExcludeFromCodeCoverage]
namespace SFA.DAS.FundingRuleValidation.Test.Console;

[ExcludeFromCodeCoverage]
public class Program
{
    public static async Task Main(string[] args)
    {
        var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
        var connectionString = config[GlobalConstants.ServiceBusConnectionName];

        // send a message
        await using var client = new ServiceBusClient(connectionString);
        await using var sender = client.CreateSender(GlobalConstants.IncomingQueueName);
        await using var receiver = client.CreateReceiver(GlobalConstants.OutgoingQueueName);

        using var cts = new CancellationTokenSource();
        var token = cts.Token;
        var running = true;

        while (running)
        {
            SystemConsole.WriteLine("1 = process-job, Ctrl+1 to send job message");
            SystemConsole.WriteLine($"2 = {GlobalConstants.IncomingQueueName}");
            SystemConsole.WriteLine($"3 = {GlobalConstants.OutgoingQueueName}");
            SystemConsole.WriteLine("4 = job-complete");
            SystemConsole.WriteLine();
            SystemConsole.WriteLine("Shift+<n> to clear the queue messages");

            var key = SystemConsole.ReadKey();
            SystemConsole.Clear();

            switch (key.Key)
            {
                case ConsoleKey.D1:
                    if (key.Modifiers.HasFlag(ConsoleModifiers.Control))
                    {
                        SystemConsole.WriteLine("Sending job 1 message");
                        await client.SendJobAsync(229500, "10034309", "ilr2526-files", "10034309/ILR-10034309-2526-20260514-111603-01-Valid.XML", token);
                        break;
                    }

                    if (key.Modifiers.HasFlag(ConsoleModifiers.Shift))
                    {
                        SystemConsole.WriteLine("Clearing job queue");
                        await client.ClearQueueAsync("ASFundingValidation", token);
                        break;
                    }

                    SystemConsole.WriteLine("Peeking job messages");
                    var jobMessages = await client.PeekQueueAsync("ASFundingValidation", token);
                    if (jobMessages is not { Count: > 0 })
                    {
                        SystemConsole.WriteLine("No job messages");
                        break;
                    }

                    foreach (var jobMessage in jobMessages)
                    {
                        SystemConsole.WriteLine($"Job message: {jobMessage}");
                    }

                    break;
                case ConsoleKey.D2:
                    if (key.Modifiers.HasFlag(ConsoleModifiers.Shift))
                    {
                        SystemConsole.WriteLine("Clearing validation requests");
                        await client.ClearQueueAsync(GlobalConstants.IncomingQueueName, token);
                        break;
                    }

                    SystemConsole.WriteLine("Peeking validation requests");
                    var validationRequests = await client.PeekQueueAsync(GlobalConstants.IncomingQueueName, token);
                    if (validationRequests is not { Count: > 0 })
                    {
                        SystemConsole.WriteLine("No validation requests");
                        break;
                    }

                    foreach (var validationRequest in validationRequests)
                    {
                        SystemConsole.WriteLine($"Validation request: {validationRequest}");
                    }

                    break;
                case ConsoleKey.D3:
                    if (key.Modifiers.HasFlag(ConsoleModifiers.Shift))
                    {
                        SystemConsole.WriteLine("Clearing validation results");
                        await client.ClearQueueAsync(GlobalConstants.OutgoingQueueName, token);
                        break;
                    }

                    SystemConsole.WriteLine("Peeking validation results");
                    var validationResults = await client.PeekQueueAsync(GlobalConstants.OutgoingQueueName, token);
                    if (validationResults is not { Count: > 0 })
                    {
                        SystemConsole.WriteLine("No validation results");
                        break;
                    }

                    foreach (var validationResult in validationResults)
                    {
                        SystemConsole.WriteLine($"Validation result: {validationResult}");
                    }

                    break;
                case ConsoleKey.D4:
                    if (key.Modifiers.HasFlag(ConsoleModifiers.Shift))
                    {
                        SystemConsole.WriteLine("Clearing job results");
                        await client.ClearQueueAsync("jobstatusqueue", token);
                        break;
                    }

                    SystemConsole.WriteLine("Peeking job results");
                    var jobResults = await client.PeekQueueAsync("jobstatusqueue", token);
                    if (jobResults is not { Count: > 0 })
                    {
                        SystemConsole.WriteLine("No job results");
                        break;
                    }

                    foreach (var jobResult in jobResults)
                    {
                        SystemConsole.WriteLine($"Job result: {jobResult}");
                    }

                    break;
                case ConsoleKey.Q:
                case ConsoleKey.Escape:
                    await cts.CancelAsync();
                    SystemConsole.Clear();
                    running = false;
                    break;
                default: continue;
            }

            SystemConsole.WriteLine();
        }
    }
}