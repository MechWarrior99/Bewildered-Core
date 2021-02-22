using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Bewildered
{
    public static class EnumExtentions
    {
        // https://stackoverflow.com/a/20408913
        public static IEnumerable<Enum> GetFlags(this Enum e)
        {
            return Enum.GetValues(e.GetType()).Cast<Enum>().Where(v => !Equals((int)(object)v, 0) && e.HasFlag(v));
        }
    }

}