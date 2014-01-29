using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KCS.Common.Shared
{
    /// <summary>
    /// DateTime.
    /// </summary>
    public class DateRange : IRange<DateTime>
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        /// <summary>
        /// Duration.
        /// </summary>
        public TimeSpan Duration
        {
            get
            {
                return End - Start;
            }
        }

        ///// <summary>
        ///// Duration, in seconds.
        ///// </summary>
        //public int DurationSeconds
        //{
        //    get
        //    {
        //        var diff = End - Start;
        //        return Convert.ToInt32(diff.TotalSeconds);
        //    }
        //}

        public DateRange()
        {
        }

        public DateRange(DateTime start, DateTime end)
        {
            Start = start;
            End = end;
        }

        //public bool Overlaps(DateTime value, bool inclusive = true)
        //{
        //    if (inclusive)
        //    {
        //        return (value > Start) || (value < End);
        //    }
        //    else
        //    {
        //        return (value >= Start) || (value <= End);
        //    }
        //}

        public bool Includes(DateTime value, bool inclusive = true)
        {
            if (inclusive)
            {
                return (value >= Start) && (value < End);
            }
            else
            {
                return (value >= Start) && (value <= End);
            }
        }

        public bool Overlaps(IRange<DateTime> range, bool inclusive = true)
        {
            bool overlap = Start < range.End && range.Start < End;
            return overlap;
        }
    }
}
