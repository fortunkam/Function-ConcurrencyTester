#  Function Concurrency Tester

Consists of several parts

1. a set of bicep files for deploying the required files to a resource group
2. a function app (ConcurrencyFunction) that contains a Service bus triggered function.  The function is designed to only permit 1 execution on a machine at a time (using the max concurrent calls parameter)
The function contains a parameter/appSetting called LockWait, this is the amount of time the function will sleep before completing (lets the scale controller add additional compute) )
3. A test app for sending messages to the service bus.