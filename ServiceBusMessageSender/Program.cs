using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServiceBusMessageSender;
using System;
using System.Text.Json;

var builder = new ConfigurationBuilder();
builder.AddJsonFile("local.settings.json", optional: true);

var configuration = builder.Build();

var connectionString = configuration.GetSection("Values")["incoming_connection_string"];

var queueName = "incomingmessages";

var sb = new Azure.Messaging.ServiceBus.ServiceBusClient(connectionString);

var sender = sb.CreateSender(queueName);

var batchId = Guid.NewGuid().ToString();

Console.WriteLine(batchId);


for (int i = 1; i <= 1; i++)
{
    LogData data = new(i, batchId);
    var messageBody = JsonSerializer.Serialize(data);

    sender.SendMessageAsync(new Azure.Messaging.ServiceBus.ServiceBusMessage
    {
        ContentType = "text/plain",
        Body = new BinaryData(messageBody)
    }).Wait();
}




