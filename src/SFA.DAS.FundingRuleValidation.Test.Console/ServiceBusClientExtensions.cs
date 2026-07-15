using System.Text;
using System.Text.Json;
using Azure.Messaging.ServiceBus;

namespace SFA.DAS.FundingRuleValidation.Test.Console;

public static class ServiceBusClientExtensions
{
    public static async Task SendJobAsync(this ServiceBusClient client, long jobId, string ukprn, string container, string filename, CancellationToken token = default)
    {
        var job = new ProcessJobMessage
        {
            JobId = jobId,
            KeyValuePairs = new ProcessJobKeyValues
            {
                Ukprn = ukprn,
                Container = container,
                Filename = filename
            }
        };
        
        await using var sender = client.CreateSender("ASFundingValidation");
        await sender.SendMessageAsync(new ServiceBusMessage(JsonSerializer.Serialize(job)), token);
    }
    
    public static async Task<List<string>> PeekQueueAsync(this ServiceBusClient client, string queueName, CancellationToken token = default)
    {
        await using var receiver = client.CreateReceiver(queueName);
        var messages = await receiver.PeekMessagesAsync(1000, cancellationToken: token);
        return messages
            .Select(x => Encoding.UTF8.GetString(x.Body.ToArray()))
            .ToList();
    }
    
    public static async Task ClearQueueAsync(this ServiceBusClient client, string queueName, CancellationToken token = default)
    {
        await using var receiver = client.CreateReceiver(queueName);
        var messages = await receiver.ReceiveMessagesAsync(1000, TimeSpan.FromSeconds(0.5), token);
        foreach (var message in messages)
        {
            await receiver.CompleteMessageAsync(message, token);
        }
    }
}