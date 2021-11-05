
namespace ConcurrencyLibrary;

using System;
using System.Diagnostics;
using System.Threading;

public static class ConcurrencyTester
{

    private static object _syncLock = new object();
    public static string RunTest(string input, int sleep)
    {
        Console.WriteLine($"{input}: before lock");

        lock (_syncLock)
        {
            Console.WriteLine($"{input}: in lock");
            Thread.Sleep(sleep);
        }

        Console.WriteLine($"{input}: after lock");

        return input;
    }
}
