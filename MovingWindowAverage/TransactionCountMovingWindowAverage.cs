using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace MovingWindowAverage
{
    public class TransactionCountMovingWindowAverage
    {
        private readonly Window _window;
        private readonly Frequency _frequency;
        private readonly TransactionCountData _transactionCountDataOfCurrentWindow;
        ITransactionDataProvider _transactionDataProvider;
        private readonly int _windowBlocksCount;

        private static System.Timers.Timer timer;



        public TransactionCountMovingWindowAverage(Window window, Frequency frequency, ITransactionDataProvider transactionDataProvider)
        {
            _transactionDataProvider = transactionDataProvider;
            _window = window;
            _frequency = frequency;
            _windowBlocksCount = GetWindowBlockCount(_window, _frequency);
            (DateTime start, DateTime end) initialWindowDates = GetInitialWindowDates();
            _transactionCountDataOfCurrentWindow = transactionDataProvider.GetTransactionCount(initialWindowDates.start, initialWindowDates.end);
            ScheduleLiveWindowMovement();
        }

        /// <summary>
        /// The method returns moving window average of live window. 
        /// As soon as TransactionCountMovingWindowAverage is initialized, program starts calaculating moving average from time strated. 
        /// Calculates the average at selected frequency for selected window size
        /// </summary>
        /// <returns></returns>
        public AverageResponse GetLiveAverage()
        {
            var average =  (double)_transactionCountDataOfCurrentWindow.Count / _windowBlocksCount;
            return new AverageResponse() { WindowStartTime = _transactionCountDataOfCurrentWindow.StartDate, WindowEndTime = _transactionCountDataOfCurrentWindow.EndDate, Average=average };

        }

        /// <summary>
        /// Gets moving window averages between specified time
        /// Calculates the average at selected frequency for selected window size
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public List<AverageResponse> GetAllAverages(DateTime startDate, DateTime endDate)
        {
            List<AverageResponse> averages = new List<AverageResponse>();
            (DateTime start, DateTime end) initialWindowDates = GetInitialWindowDates(startDate);
            var currentWindowTransactionData = _transactionDataProvider.GetTransactionCount(initialWindowDates.start, initialWindowDates.end);
            var initialAverage = (double)currentWindowTransactionData.Count / _windowBlocksCount;
            averages.Add(new AverageResponse() { WindowStartTime= currentWindowTransactionData.StartDate, WindowEndTime = currentWindowTransactionData.EndDate, Average= initialAverage});
            do
            {
                var currentWindowTransactionDataAfterMove = MoveWindow(currentWindowTransactionData.StartDate, currentWindowTransactionData.EndDate, currentWindowTransactionData.Count);
                currentWindowTransactionData.EndDate = currentWindowTransactionDataAfterMove.currentWindowEndDate;
                currentWindowTransactionData.StartDate = currentWindowTransactionDataAfterMove.currentWindowStartDate;
                currentWindowTransactionData.Count = currentWindowTransactionDataAfterMove.currentWindowCount;
                var average = (double)currentWindowTransactionData.Count / _windowBlocksCount;
                averages.Add(new AverageResponse() { WindowStartTime = currentWindowTransactionData.StartDate, WindowEndTime = currentWindowTransactionData.EndDate, Average = average });
            } while (currentWindowTransactionData.EndDate <= endDate);
            return averages;
        }

        /// <summary>
        /// This method initializes the timer that moves window for live data
        /// </summary>
        private void ScheduleLiveWindowMovement()
        {
            timer = new System.Timers.Timer(WindowMovementTimeInSeconds() * 1000);
            // Hook up the Elapsed event for the timer. 
            timer.Elapsed += MoveLiveWindow;
            timer.AutoReset = true;
            timer.Enabled = true;
        }
        private (DateTime start, DateTime end) GetInitialWindowDates(DateTime? start = null)
        {
            start = start.HasValue? start: DateTime.Now;
            DateTime end = DateTime.Now;
            switch (this._window)
            {
                case Window.Hour:
                    end = start.Value.AddHours(1);
                    break;
                case Window.Day:
                    end = start.Value.AddDays(1);
                    break;
                case Window.Week:
                    end = start.Value.AddDays(7);
                    break;
                case Window.Month:
                    end = start.Value.AddMonths(1);
                    break;
                case Window.Year:
                    end = start.Value.AddYears(1);
                    break;
                case Window.Custom:
                    break;
                default:
                    break;
            }
            return (start.Value, end);
        }

        private void MoveLiveWindow(Object source, ElapsedEventArgs e)
        {
            var currentWindowData = MoveWindow(_transactionCountDataOfCurrentWindow.StartDate, _transactionCountDataOfCurrentWindow.EndDate, _transactionCountDataOfCurrentWindow.Count);

            //Assign values of current window after move to Live window
            _transactionCountDataOfCurrentWindow.Count = currentWindowData.currentWindowCount;
            _transactionCountDataOfCurrentWindow.StartDate = currentWindowData.currentWindowStartDate;
            _transactionCountDataOfCurrentWindow.EndDate = currentWindowData.currentWindowEndDate;

        }
        /// <summary>
        /// Here is the core logic 
        /// </summary>
        /// <param name="currentWindowEndDate"></param>
        /// <param name="currentWindowCount"></param>
        /// <returns></returns>
        private (DateTime currentWindowStartDate, DateTime currentWindowEndDate, int currentWindowCount) MoveWindow(DateTime currentWindowStartDate, DateTime currentWindowEndDate, int currentWindowCount)
        {
            DateTime startTimeOfTransactionBlockToBeExcluded = currentWindowStartDate;
            DateTime endTimeOfTransactionBlockToBeExcluded = currentWindowStartDate.AddSeconds(WindowMovementTimeInSeconds());
            var transactionCountDataOfBlockToBeExcluded = _transactionDataProvider.GetTransactionCount(startTimeOfTransactionBlockToBeExcluded, endTimeOfTransactionBlockToBeExcluded);

            DateTime startTimeOfTransactionBlockToBeIncluded = currentWindowEndDate;
            DateTime endTimeOfTransactionBlockToBeIncluded = currentWindowEndDate.AddSeconds(WindowMovementTimeInSeconds());
            var transactionCountDataOfBlockToBeIncluded = _transactionDataProvider.GetTransactionCount(startTimeOfTransactionBlockToBeIncluded, endTimeOfTransactionBlockToBeIncluded);

            var currentWindowCountAfterMove = currentWindowCount - transactionCountDataOfBlockToBeExcluded.Count + transactionCountDataOfBlockToBeIncluded.Count;
            var currentWindowStartDateAfterMove = endTimeOfTransactionBlockToBeExcluded;
            var currentWindowEndDateAfterMove = endTimeOfTransactionBlockToBeIncluded;
            return (currentWindowStartDateAfterMove, currentWindowEndDateAfterMove, currentWindowCountAfterMove);
        }

        private int WindowMovementTimeInSeconds()
        {
            int response;
            switch (this._frequency)
            {
                case Frequency.Minute:
                    response = 60;
                    break;
                case Frequency.Hour:
                    response = 60 * 60;
                    break;
                case Frequency.Day:
                    response = 60 * 60 * 24;
                    break;
                case Frequency.Month:
                    response = 60 * 60 * 24 * GetDaysInMonth();
                    break;
                case Frequency.Year:
                    response = 60 * 60 * 24 * GetDaysInMonth() * 12;
                    break;
                default:
                    throw new ApplicationException();
                    break;
            }
            return response;
        }

        private int GetDaysInMonth()
        {
            return 30;
        }
        private int GetWindowBlockCount(Window window, Frequency transactionRateUnit)
        {
            int response = 0;
            switch (window)
            {
                case Window.Hour:
                    response = GetWindowBlockCountForHourWindow(transactionRateUnit);
                    break;
                case Window.Day:
                    response = GetWindowBlockCountForDayWindow(transactionRateUnit);
                    break;
                case Window.Week:

                    break;
                case Window.Month:
                    break;
                case Window.Year:
                    break;
                case Window.Custom:
                    break;
                default:
                    break;
            }
            return response;
        }

        private int GetWindowBlockCountForHourWindow(Frequency transactionRateUnit)
        {
            int response = 0;
            if (transactionRateUnit == Frequency.Minute)
                response = 60;
            else if (transactionRateUnit == Frequency.Hour)
                response = 1;
            else
                throw new ApplicationException("In-valid transaction rate for selected window");
            return response;
        }

        private int GetWindowBlockCountForDayWindow(Frequency transactionRateUnit)
        {
            int response = 0;
            if (transactionRateUnit == Frequency.Minute)
                response = 60;
            else if (transactionRateUnit == Frequency.Hour)
                response = 24;
            else if (transactionRateUnit == Frequency.Day)
                response = 1;
            else
                throw new ApplicationException("In-valid transaction rate for selected window");
            return response;
        }
    }
}
