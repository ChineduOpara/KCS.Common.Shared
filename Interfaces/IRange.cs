using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KCS.Common.Shared
{
	/// <summary>
	/// Interface for Generic range classes.
	/// </summary>
	/// <typeparam name="T"></typeparam>
    public interface IRange<T>
    {
        T Start { get; }
        T End { get; }
        //bool Overlaps(T value, bool inclusive);
        bool Includes(T value, bool inclusive);
        bool Overlaps(IRange<T> range, bool inclusive);
    }
}
