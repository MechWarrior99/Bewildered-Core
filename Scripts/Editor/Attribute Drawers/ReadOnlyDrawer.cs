using UnityEngine;
using UnityEditor;

namespace Bewildered.Editor
{
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    internal class ReadOnlyDrawer : DecoratorDrawer
    {
        public override void OnGUI(Rect position)
        {
            bool cachedEnabledState = GUI.enabled;
            GUI.enabled = false;
            base.OnGUI(position);
            GUI.enabled = cachedEnabledState;
        }
    } 
}
