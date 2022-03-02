using System;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace ConcurrencyFunction
{
    public static class ServiceBusTest
    {
        [FunctionName("ServiceBusTest")]
        [return: ServiceBus("processedmessages", Connection = "processed_connection_string")]
        public static async Task<string> Run([ServiceBusTrigger("incomingmessages", Connection = "incoming_connection_string")]Message message, 
            ILogger log, MessageReceiver messageReceiver)
        {
            var body = System.Text.ASCIIEncoding.ASCII.GetString(message.Body);

            dynamic messageDebug = new JObject();
            messageDebug["body"] = body;
            messageDebug["machine"] = Environment.MachineName;
            messageDebug["wait"] = int.Parse(Environment.GetEnvironmentVariable("LockWait"));

            log.LogInformation($"C# ServiceBus queue trigger function started message: {messageDebug.body} on {messageDebug.machine} with wait of {messageDebug.wait}");

           // await messageReceiver.CompleteAsync(message.SystemProperties.LockToken);


            ConcurrencyLibrary.ConcurrencyTester.RunTest(body, (int)messageDebug.wait);

            log.LogInformation($"C# ServiceBus queue trigger function completed message: {messageDebug.body} on {messageDebug.machine}");

            return messageDebug.ToString();
        }
    }
}
