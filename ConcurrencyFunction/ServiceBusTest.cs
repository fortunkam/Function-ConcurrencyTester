using System;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace ConcurrencyFunction
{
    public static class ServiceBusTest
    {
        [FunctionName("ServiceBusTest")]
        public static async Task Run([ServiceBusTrigger("myqueue", Connection = "demo1_connection_string")]Message message, ILogger log, MessageReceiver messageReceiver)
        {
            var body = System.Text.ASCIIEncoding.ASCII.GetString(message.Body);
            log.LogInformation($"C# ServiceBus queue trigger function started message: {body} on {Environment.MachineName}");

            //if (message.SystemProperties.IsLockTokenSet)
            //{
            //    await messageReceiver.CompleteAsync(message.SystemProperties.LockToken);
            //}

            await messageReceiver.CompleteAsync(message.SystemProperties.LockToken);


            ConcurrencyLibrary.ConcurrencyTester.RunTest(body, int.Parse(Environment.GetEnvironmentVariable("LockWait")));

            log.LogInformation($"C# ServiceBus queue trigger function completed message: {body} on {Environment.MachineName}");
        }
    }
}
