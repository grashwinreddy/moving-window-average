using MovingWindowAverage;
using System;
try
{
    Console.WriteLine("Transaction Count Moving Average");

    var timer = new System.Timers.Timer(60 * 1000);
    // Hook up the Elapsed event for the timer. 
    timer.Elapsed += (a, b) =>
    {
        ITransactionDataProvider dataProvider = new TransactionDataProvider();
        var movingAverageCalculator = new TransactionCountMovingWindowAverage(Window.Hour, Frequency.Minute, dataProvider);
        Console.WriteLine(movingAverageCalculator.GetLatestAverage());
    };
    timer.AutoReset = true;
    timer.Enabled = true;
    Console.ReadLine();
}
catch (Exception ex)
{

    Console.WriteLine(ex.ToString());
}
