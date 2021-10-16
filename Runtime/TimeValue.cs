using System;

namespace Bewildered
{
    /// <summary>
    /// Represents a point or duration of time in real-world seconds for easier configuration than float.
    /// </summary>
    [Serializable]
    public struct TimeValue
    {
        /// <summary>
        /// The full real-world time in seconds that the <see cref="TimeValue"/> represents.
        /// </summary>
        public float time;

        /// <summary>
        /// The number of real-world minutes that the <see cref="TimeValue"/> represents.
        /// </summary>
        public int Minutes
        {
            get { return (int)(time / 60.0f); }
            set
            {
                // We add the difference between the new minutes and the current minutes, multiplying by 60 to convert to seconds.
                // We do this so we keep the decimal places in the time field.
                time += (value - Minutes) * 60.0f;
            }
        }

        /// <summary>
        /// The number of real-world seconds over <see cref="Minutes"/> that the <see cref="TimeValue"/> represents.
        /// </summary>
        public int Seconds
        {
            get { return (int)(time % 60.0f); }
            set
            {
                // We add the difference between the new and current seconds.
                // We do this so we keep the decimal places in the time field.
                time += value - Seconds;
            }
        }

        public TimeValue(float time)
        {
            this.time = time;
        }

        public static implicit operator TimeValue(float time)
        {
            return new TimeValue(time);
        }

        public static implicit operator float(TimeValue timeVariable)
        {
            return timeVariable.time;
        }

        // We override Equals and GethashCode because the default struct implementation has less than ideal performance.
        public override bool Equals(object obj)
        {
            if (obj is TimeValue duration)
                return duration.time == time;
            else
                return false;
        }

        public override int GetHashCode()
        {
            return time.GetHashCode();
        }
    }
}
