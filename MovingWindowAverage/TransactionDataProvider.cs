using System;

namespace MovingWindowAverage
{
    public class TransactionDataProvider : ITransactionDataProvider
    {
        public TransactionCountData GetTransactionCount(DateTime start, DateTime end)
        {
            //Gets the transactons from DB or any other source in between specified time
            return new TransactionCountData()
            {
                Count = 300,
                StartDate = start,
                EndDate = end
            };
        }
    }
}
