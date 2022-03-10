using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace ConcurrencyFunction
{
    public record LogData(int Iterator, string BatchId);

    public class ServiceBusTest
    {
        private readonly TelemetryClient telemetryClient;

        public ServiceBusTest(TelemetryConfiguration telemetryConfiguration)
        {
            this.telemetryClient = new TelemetryClient(telemetryConfiguration);
        }


        [FunctionName("ServiceBusTest")]
        [return: ServiceBus("processedmessages", Connection = "processed_connection_string")]
        public string Run([ServiceBusTrigger("incomingmessages", Connection = "incoming_connection_string")]ServiceBusReceivedMessage message, 
            ILogger log)
        {
            var body = System.Text.ASCIIEncoding.ASCII.GetString(message.Body);
            var jsonObj = JsonSerializer.Deserialize<LogData>(body);

            dynamic messageDebug = new JObject();
            messageDebug["body"] = body;
            messageDebug["machine"] = Environment.MachineName;
            messageDebug["wait"] = int.Parse(Environment.GetEnvironmentVariable("LockWait"));

            log.LogInformation($"C# ServiceBus queue trigger function started message: {messageDebug.body} on {messageDebug.machine} with wait of {messageDebug.wait}");

            telemetryClient.TrackEvent("ProcessBatchMessage", new Dictionary<string, string>()
            {
                {"machine",Environment.MachineName},
                {"wait",Environment.GetEnvironmentVariable("LockWait")},
                {"batch",jsonObj.BatchId},
                {"iterator",jsonObj.Iterator.ToString()},
            });

            ConcurrencyLibrary.ConcurrencyTester.RunTest(body, (int)messageDebug.wait);

            log.LogInformation($"C# ServiceBus queue trigger function completed message: {messageDebug.body} on {messageDebug.machine}");

            telemetryClient.TrackEvent("ProcessBatchMessageComplete", new Dictionary<string, string>()
            {
                {"machine",Environment.MachineName},
                {"wait",Environment.GetEnvironmentVariable("LockWait")},
                {"batch",jsonObj.BatchId},
                {"iterator",jsonObj.Iterator.ToString()},
            });
            return messageDebug.ToString();
        }
    }
}
