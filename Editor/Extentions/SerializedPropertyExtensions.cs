using UnityEditor;

namespace Bewildered.Editor
{
    /// <summary>
    /// General use extensions for <see cref="SerializedProperty"/>.
    /// </summary>
    public static class SerializedPropertyExtensions
    {
        /// <summary>
        /// Determines whether the <see cref="SerializedProperty"/> is an element of an array.
        /// </summary>
        /// <returns><c>true</c> if the <see cref="SerializedProperty"/> is an element in an array; otherwise, <c>false</c>.</returns>
        public static bool IsArrayElement(this SerializedProperty property)
        {
            return property.propertyPath.EndsWith("]");
        }

        /// <summary>
        /// Reterns the last <see cref="SerializedProperty"/> element in the <see cref="SerializedProperty"/> if it is an array.
        /// </summary>
        /// <returns>The last <see cref="SerializedProperty"/> element in the <see cref="SerializedProperty"/>; <c>null</c> if there are no elements in the array.</returns>
        public static SerializedProperty GetLastArrayElement(this SerializedProperty property)
        {
            if (property.arraySize == 0)
                return null;
            return property.GetArrayElementAtIndex(property.arraySize - 1);
        }
    }
}
