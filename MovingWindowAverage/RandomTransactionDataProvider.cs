using System;

namespace MovingWindowAverage
{
    public class RandomTransactionDataProvider : ITransactionDataProvider
    {
        public TransactionCountData GetTransactionCount(DateTime start, DateTime end)
        {
            var rand = new Random();
            //Gets the transactons from DB or any other source in between specified time
            //For demo here I am using a random number geneartor that generates number  between 250 to 301
            return new TransactionCountData()
            {
                Count = rand.Next(250,301),
                StartDate = start,
                EndDate = end
            };
        }
    }
}
