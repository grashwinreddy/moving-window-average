using MovingWindowAverage;
using System;
try
{
    //Moving average example for fixed count. 
    Console.WriteLine("\n");
    var start1 = DateTime.Now.AddDays(-2);
    var end1 = DateTime.Now.AddDays(-1);
    Console.WriteLine($"Calculating moving averages of transaction Count between {start1} and {end1}. The data provider used generates fixed transaction count of 300");
    ITransactionDataProvider dataProvider1 = new FixedTransactionDataProvider();
    var movingAverageCalculator1 = new TransactionCountMovingWindowAverage(Window.Day, Frequency.Hour, dataProvider1);
    var pastAvergaes1 = movingAverageCalculator1.GetAllAverages(start1, end1);
    foreach (var item in pastAvergaes1)
    {
        Console.WriteLine($"Transaction count moving average between: {item.WindowStartTime}: {item.WindowEndTime}: {item.Average}");
    }

    //Moving average for random count. 
    var start = DateTime.Now.AddHours(-10);
    var end = DateTime.Now.AddHours(-8);
    ITransactionDataProvider dataProvider = new RandomTransactionDataProvider();
    var movingAverageCalculator = new TransactionCountMovingWindowAverage(Window.Hour, Frequency.Minute, dataProvider);
    var pastAvergaes = movingAverageCalculator.GetAllAverages(start,end);
    Console.WriteLine("\n");
    Console.WriteLine($"Calculating moving averages of transaction Count between {start} and {end}. The data provider used generates a random transaction count between 250 to 301");
    foreach (var item in pastAvergaes)
    {
        Console.WriteLine($"Transaction count moving average between: {item.WindowStartTime}: {item.WindowEndTime}: {item.Average}");
    }
    //Live data example
    var timer = new System.Timers.Timer(60 * 1000);
    Console.WriteLine("\n");
    Console.WriteLine($"For every minute emits moving average of live data. Wait for 2 to 3 minutes to see");
    // Hook up the Elapsed event for the timer. 
    timer.Elapsed += (a, b) =>
    {
        var response = movingAverageCalculator.GetLiveAverage();
        Console.WriteLine($"Transaction Count of Live Moving Average: {response.WindowStartTime}: {response.WindowEndTime}: {response.Average}");
    };
    timer.AutoReset = true;
    timer.Enabled = true;
    Console.ReadLine();
}
catch (Exception ex)
{

    Console.WriteLine(ex.ToString());
}
