using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;


var builder = new ConfigurationBuilder();
builder.AddJsonFile("local.settings.json", optional: true);

var configuration = builder.Build();

var connectionString = configuration.GetSection("Values")["incoming_connection_string"];

var queueName = "incomingmessages";

var sb = new Azure.Messaging.ServiceBus.ServiceBusClient(connectionString);

var sender = sb.CreateSender(queueName);

for (int i = 1; i <= 25; i++)
{
    var messageBody = $"{i}::{Guid.NewGuid().ToString()}";

    sender.SendMessageAsync(new Azure.Messaging.ServiceBus.ServiceBusMessage
    {
        ContentType = "text/plain",
        Body = new BinaryData(messageBody)
    }).Wait();
}




