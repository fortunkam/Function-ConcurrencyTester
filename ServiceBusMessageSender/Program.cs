using System;

var connectionString = "<YOUR SB CONNECTION STRING HERE>";

var queueName = "myqueue";

var sb = new Azure.Messaging.ServiceBus.ServiceBusClient(connectionString);

var sender = sb.CreateSender(queueName);

for (int i = 1; i <= 20; i++)
{
    var messageBody = $"{i}::{Guid.NewGuid().ToString()}";

    sender.SendMessageAsync(new Azure.Messaging.ServiceBus.ServiceBusMessage
    {
        ContentType = "text/plain",
        Body = new BinaryData(messageBody)
    }).Wait();
}




