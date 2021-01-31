using System;
using UnityEngine;

namespace Bewildered
{
    /// <summary>
    /// Makes a variable not be editable in the inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ReadOnlyAttribute : PropertyAttribute
    {
        
    }
}