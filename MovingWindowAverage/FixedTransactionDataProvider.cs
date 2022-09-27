using System;

namespace MovingWindowAverage
{
    public class FixedTransactionDataProvider : ITransactionDataProvider
    {
        public TransactionCountData GetTransactionCount(DateTime start, DateTime end)
        {
            var rand = new Random();
            //Gets the transactons from DB or any other source in between specified time
            //For demo here always gives fixed count of 300
            return new TransactionCountData()
            {
                Count = 300,
                StartDate = start,
                EndDate = end
            };
        }
    }
}
