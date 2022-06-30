using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace ConcurrencyFunction
{
    public record LogData(int Iterator, string BatchId);

    public class ServiceBusTest
    {
        private readonly TelemetryClient _telemetryClient;
        private readonly ILogger<ServiceBusTest> _logger;
        private readonly IConfiguration _configuration;

        public ServiceBusTest(TelemetryClient telemetryClient, ILogger<ServiceBusTest> logger, IConfiguration configuration)
        {
            _telemetryClient = telemetryClient;
            _logger = logger;
            _configuration = configuration;
        }


        [Function("ServiceBusTest")]
        [ServiceBusOutput("processedmessages", Connection = "processed_connection_string")]
        public string Run([ServiceBusTrigger("incomingmessages", Connection = "incoming_connection_string")]string message)
        {
            var body = message;
            var jsonObj = JsonSerializer.Deserialize<LogData>(body);

            var lockWait = _configuration.GetValue<int>("LockWait");

            var machineName = Environment.MachineName;
            var processId = Process.GetCurrentProcess().Id.ToString();


            dynamic messageDebug = new JObject();
            messageDebug["body"] = body;
            messageDebug["machine"] = machineName;
            messageDebug["wait"] = lockWait;
            messageDebug["process"] = processId;

            _logger.LogInformation($"C# ServiceBus queue trigger function started message: {messageDebug.body} on {messageDebug.machine} (Process: {messageDebug.process}) with wait of {messageDebug.wait}");

            
            _telemetryClient.TrackEvent("ProcessBatchMessage", new Dictionary<string, string>()
            {
                {"machine",machineName},
                {"wait",lockWait.ToString()},
                {"batch",jsonObj.BatchId},
                {"process",  processId},
                {"iterator",jsonObj.Iterator.ToString()},
            });

            ConcurrencyLibrary.ConcurrencyTester.RunTest(body, (int)messageDebug.wait);

            _logger.LogInformation($"C# ServiceBus queue trigger function completed message: {messageDebug.body} on {messageDebug.machine} (Process: {messageDebug.process})");

            _telemetryClient.TrackEvent("ProcessBatchMessageComplete", new Dictionary<string, string>()
            {
                {"machine",machineName},
                {"wait",lockWait.ToString()},
                {"batch",jsonObj.BatchId},
                {"process",  processId},
                {"iterator",jsonObj.Iterator.ToString()},
            });
            return messageDebug.ToString();
        }
    }
}
