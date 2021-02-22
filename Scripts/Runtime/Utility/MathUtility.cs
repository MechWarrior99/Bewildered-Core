using System;
using UnityEngine;

namespace Bewildered
{
    public enum Rounding { ToEven, AwayFromZero }

    public struct MathfUtility
    {
        /// <summary>
        /// Returns <paramref name="f"/> rounded to the nearest <paramref name="n"/>
        /// </summary>
        /// <remarks>If <paramref name="f"/> is 13.5f and <paramref name="n"/> is 5.0f, then will return 15.0f.</remarks>
        /// <param name="f">The <see cref="float"/> value to round.</param>
        /// <param name="n">The <see cref="float"/> value to round to.</param>
        public static float RoundToNearest(float f, float n)
        {
            return Mathf.Round(f / n) * n;
        }

        public static int RoundIntToNearest(int i, int n)
        {
            return (int)Mathf.Round(i / (float)n) * n;
        }

        /// <summary>
        /// Rounds a <see cref="float"/> to the nearest integer, and uses the specified rounding convention for midpoint values.
        /// </summary>
        /// <param name="f">The <see cref="float"/> value to be rounded.</param>
        /// <param name="rounding">Specification for how to round <paramref name="f"/> if it is midway between two other numbers.</param>
        /// <returns>The integer neastest to <paramref name="f"/>. If <paramref name="f"/> is halfway between two integers, one which is even and the other odd,
        /// then <paramref name="rounding"/> determines which of the two is returned. Note that this method returns a <see cref="float"/> instead of an <see cref="int"/> type.</returns>
        public static float Round(float f, Rounding rounding)
        {
            return (float)Math.Round((float)f, rounding == Rounding.AwayFromZero ? MidpointRounding.AwayFromZero : MidpointRounding.ToEven);
        }

        /// <summary>
        /// Rounds a <see cref="float"/> value to a specified number of fractional digits, and rounds midpoint values to the nearest even number.
        /// </summary>
        /// <param name="f">The <see cref="float"/> value to be rounded.</param>
        /// <param name="digits">The number of fractional digits in the return value.</param>
        /// <returns>The number nearest to <paramref name="f"/> that contains a number of fractional digits equal to <paramref name="digits"/>.  If <paramref name="f"/> has fewer fractional digits than <paramref name="digits"/>, <paramref name="f"/> is returned unchanged.</returns></returns>
        public static float Round(float f, int digits)
        {
            return (float)Math.Round(f, digits);
        }

        /// <summary>
        /// Rounds a <see cref="float"/> value to a specified number of fractional digits, and uses the specified rounding convention for midpoint values.
        /// </summary>
        /// <param name="f">The <see cref="float"/> value to be rounded.</param>
        /// <param name="digits">The number of fractional digits in the return value.</param>
        /// <param name="rounding">Specification for how to round <paramref name="f"/> if it is midway between two other numbers.</param>
        /// <returns>The number nearest to <paramref name="f"/> that has a number of fractional digits equal to <paramref name="digits"/>. If <paramref name="f"/> has fewer fractional digits than <paramref name="digits"/>, <paramref name="f"/> is returned unchanged.</returns>
        public static float Round(float f, int digits, Rounding rounding)
        {
            return (float)Math.Round((float)f, digits, rounding == Rounding.AwayFromZero ? MidpointRounding.AwayFromZero : MidpointRounding.ToEven);
        }

        // https://stackoverflow.com/questions/929103/convert-a-number-range-to-another-range-maintaining-ratio

        public static float Remap(float from, float fromMax, float toMax)
        {
            var normal = from / fromMax;

            var toAbs = toMax * normal;

            return toAbs;
        }

        public static float Remap(float from, float fromMin, float fromMax, float toMin, float toMax)
        {
            var fromAbs = from - fromMin;
            var fromMaxAbs = fromMax - fromMin;

            var normal = fromAbs / fromMaxAbs;

            var toMaxAbs = toMax - toMin;
            var toAbs = toMaxAbs * normal;

            var to = toAbs + toMin;

            return to;
        }
    } 
}
