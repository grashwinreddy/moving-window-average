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
            (DateTime start, DateTime end) initialWindowDates = GetInitialDates();
            _transactionCountDataOfCurrentWindow = transactionDataProvider.GetTransactionCount(initialWindowDates.start, initialWindowDates.end);
            ScheduleWindowMovement();
        }


        public double GetLatestAverage()
        {
            return (double)_transactionCountDataOfCurrentWindow.Count / _windowBlocksCount;
        }

        private void ScheduleWindowMovement()
        {
            timer = new System.Timers.Timer(WindowMovementTimeInSeconds() * 1000);
            // Hook up the Elapsed event for the timer. 
            timer.Elapsed += MoveWindow;
            timer.AutoReset = true;
            timer.Enabled = true;
        }
        private (DateTime start, DateTime end) GetInitialDates()
        {
            DateTime start = DateTime.Now;
            DateTime end = DateTime.Now;
            switch (this._window)
            {
                case Window.Hour:
                    end = start.AddHours(1);
                    break;
                case Window.Day:
                    end = start.AddDays(1);
                    break;
                case Window.Week:
                    end = start.AddDays(7);
                    break;
                case Window.Month:
                    end = start.AddMonths(1);
                    break;
                case Window.Year:
                    end = start.AddYears(1);
                    break;
                case Window.Custom:
                    break;
                default:
                    break;
            }
            return (start, end);
        }

        private void MoveWindow(Object source, ElapsedEventArgs e)
        {
            DateTime startTimeOfTransactionBlockToBeExcluded = _transactionCountDataOfCurrentWindow.EndDate.AddSeconds(-WindowMovementTimeInSeconds());
            DateTime endTimeOfTransactionBlockToBeExcluded = _transactionCountDataOfCurrentWindow.EndDate;
            var transactionCountDataOfBlockToBeExcluded = _transactionDataProvider.GetTransactionCount(startTimeOfTransactionBlockToBeExcluded, endTimeOfTransactionBlockToBeExcluded);

            DateTime startTimeOfTransactionBlockToBeIncluded = _transactionCountDataOfCurrentWindow.EndDate.AddSeconds(-WindowMovementTimeInSeconds());
            DateTime endTimeOfTransactionBlockToBeIncluded = _transactionCountDataOfCurrentWindow.EndDate;
            var transactionCountDataOfBlockToBeIncluded = _transactionDataProvider.GetTransactionCount(startTimeOfTransactionBlockToBeIncluded, endTimeOfTransactionBlockToBeIncluded);

            _transactionCountDataOfCurrentWindow.Count = _transactionCountDataOfCurrentWindow.Count - transactionCountDataOfBlockToBeExcluded.Count + transactionCountDataOfBlockToBeIncluded.Count;
            _transactionCountDataOfCurrentWindow.StartDate = _transactionCountDataOfCurrentWindow.StartDate.AddSeconds(WindowMovementTimeInSeconds());
            _transactionCountDataOfCurrentWindow.EndDate = _transactionCountDataOfCurrentWindow.EndDate.AddSeconds(WindowMovementTimeInSeconds());

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
                    response = 60 * 60 * 24 * GetDaysInMonth()*12;
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
