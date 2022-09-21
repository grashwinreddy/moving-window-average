using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovingWindowAverage
{
    public interface ITransactionDataProvider
    {
        TransactionCountData GetTransactionCount(DateTime startDate, DateTime endDate);
    }
}
