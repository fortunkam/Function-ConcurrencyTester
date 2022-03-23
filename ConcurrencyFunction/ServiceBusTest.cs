using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace ConcurrencyFunction
{
    public record LogData(int Iterator, string BatchId);

    public class ServiceBusTest
    {
        private readonly TelemetryClient _telemetryClient;
        private readonly IConfiguration _configuration;

        public ServiceBusTest(TelemetryConfiguration telemetryConfiguration, IConfiguration configuration)
        {
            this._telemetryClient = new TelemetryClient(telemetryConfiguration);
            _configuration = configuration;
        }


        [FunctionName("ServiceBusTest")]
        [return: ServiceBus("processedmessages", Connection = "processed_connection_string")]
        public string Run([ServiceBusTrigger("incomingmessages", Connection = "incoming_connection_string")]ServiceBusReceivedMessage message, 
            ILogger log)
        {
            var body = System.Text.ASCIIEncoding.ASCII.GetString(message.Body);
            var jsonObj = JsonSerializer.Deserialize<LogData>(body);

            var lockWait = _configuration.GetValue<int>("LockWait");

            dynamic messageDebug = new JObject();
            messageDebug["body"] = body;
            messageDebug["machine"] = Environment.MachineName;
            messageDebug["wait"] = lockWait;

            log.LogInformation($"C# ServiceBus queue trigger function started message: {messageDebug.body} on {messageDebug.machine} with wait of {messageDebug.wait}");

            
            _telemetryClient.TrackEvent("ProcessBatchMessage", new Dictionary<string, string>()
            {
                {"machine",Environment.MachineName},
                {"wait",lockWait.ToString()},
                {"batch",jsonObj.BatchId},
                {"iterator",jsonObj.Iterator.ToString()},
            });

            ConcurrencyLibrary.ConcurrencyTester.RunTest(body, (int)messageDebug.wait);

            log.LogInformation($"C# ServiceBus queue trigger function completed message: {messageDebug.body} on {messageDebug.machine}");

            _telemetryClient.TrackEvent("ProcessBatchMessageComplete", new Dictionary<string, string>()
            {
                {"machine",Environment.MachineName},
                {"wait",lockWait.ToString()},
                {"batch",jsonObj.BatchId},
                {"iterator",jsonObj.Iterator.ToString()},
            });
            return messageDebug.ToString();
        }
    }
}
