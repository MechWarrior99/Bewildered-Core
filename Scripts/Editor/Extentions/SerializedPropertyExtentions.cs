using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;

using Object = UnityEngine.Object;

namespace Bewildered.Editor
{
    public static class SerializedPropertyExtentions
    {
        private static Dictionary<SerializedPropertyType, PropertyInfo> _serializedPropertyValueAccessors = new Dictionary<SerializedPropertyType, PropertyInfo>();

        /// <summary>
        /// Set the element at the specified index to null if it is a <see cref="Object"/> type field and delete the element in the array.
        /// </summary>
        /// <remarks>
        /// By default <see cref="SerializedProperty.DeleteArrayElementAtIndex(int)"/> will set the field to null instead of deleting it if it is a <see cref="Object"/>. 
        /// </remarks>
        /// <param name="index">The index of the element in the array to delete.</param>
        public static void ClearAndDeleteArrayElementAtIndex(this SerializedProperty serializedProperty, int index)
        {
            if (serializedProperty.GetArrayElementAtIndex(index).propertyType == SerializedPropertyType.ObjectReference)
                serializedProperty.GetArrayElementAtIndex(index).objectReferenceValue = null;

            serializedProperty.DeleteArrayElementAtIndex(index);
        }

        /// <summary>
        /// Add an empty element to the end of the array and return it.
        /// </summary>
        /// <returns>The element that was added.</returns>
        public static SerializedProperty AddAndGetArrayElement(this SerializedProperty serializedProperty)
        {
            serializedProperty.InsertArrayElementAtIndex(serializedProperty.arraySize);
            return serializedProperty.GetArrayElementAtIndex(serializedProperty.arraySize - 1);
        }

        public static SerializedProperty GetLastArrayElement(this SerializedProperty property)
        {
            return property.GetArrayElementAtIndex(property.arraySize - 1);
        }

        /// <summary>
        /// Determines whether the <see cref="SerializedProperty"/> array contains elements that match the conditions defined by the specified <see cref="Predicate{T}"/>.
        /// </summary>
        /// <param name="match">The <see cref="Predicate{T}"/> delegate that defines the conditions of the elements to search for.</param>
        /// <returns><c>true</c> if the <see cref="SerializedProperty"/> array contains one or more elements that match the conditions defined by the specified <see cref="Predicate{T}"/>; otherwise, <c>false</c>.</returns>
        public static bool ExistsInArray(this SerializedProperty serializedProperty, Predicate<SerializedProperty> match)
        {
            for (int i = 0; i < serializedProperty.arraySize; i++)
            {
                if (match(serializedProperty.GetArrayElementAtIndex(i)))
                    return true;
            }

            return false;
        }

        public static bool ArrayContainsElementValue(this SerializedProperty property, SerializedProperty element)
        {
            for (int i = 0; i < property.arraySize; i++)
            {
                if (SerializedProperty.DataEquals(property.GetArrayElementAtIndex(i), element))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Gets all children of `SerializedProperty` at 1 level depth.
        /// </summary>
        /// <param name="serializedProperty">Parent `SerializedProperty`.</param>
        /// <returns>Collection of `SerializedProperty` children.</returns>
        // https://forum.unity.com/threads/loop-through-serializedproperty-children.435119/#post-5333913
        public static IEnumerable<SerializedProperty> GetChildren(this SerializedProperty property)
        {
            SerializedProperty currentProperty = property.Copy();
            SerializedProperty nextSiblingProperty = property.Copy();
            nextSiblingProperty.Next(false);

            if (currentProperty.Next(true))
            {
                do
                {
                    if (SerializedProperty.EqualContents(currentProperty, nextSiblingProperty))
                        break;

                    yield return currentProperty; // Possibly need to use .Copy() to work with Linq ToArray().
                }
                while (currentProperty.Next(false));
            }
        }

        // Works except for when the mangedReferenceValue is null, then it returns null.
        //public static string GetValueType(this SerializedProperty property)
        //{
        //    string unityTypeString = "PPtr";
        //    string managedReferenceString = "managedReference";
        //    string typeString = property.type;

        //    Match unityTypeMatch = Regex.Match(typeString, @"\$(.+)>");
        //    Match managedRefMatch = Regex.Match(typeString, @"<(.+)>");

        //    if (typeString.Contains(unityTypeString))
        //        typeString = unityTypeMatch.Success ? unityTypeMatch.Groups[1].Value : managedRefMatch.Groups[1].Value;
        //    else if (typeString.Contains(managedReferenceString))
        //        typeString = managedRefMatch.Groups[1].Value;

        //    if (string.IsNullOrEmpty(typeString))
        //    {
        //        return null;
        //    }

        //    return property.type;
        //}

        /// <summary>
        /// Doesn't work when in a nested type. It will return the parent type instead.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public static Type GetPropertyType(this SerializedProperty property)
        {
            object obj = property.serializedObject.targetObject;
            string path = property.propertyPath.Replace(".Array.data", "");
            string[] fieldStructure = path.Split('.');
            Regex rgx = new Regex(@"\[\d+\]");

            for (int i = 0; i < fieldStructure.Length; i++)
            {
                if (fieldStructure[i].Contains("["))
                {
                    int index = System.Convert.ToInt32(new string(fieldStructure[i].Where(c => char.IsDigit(c)).ToArray()));
                    FieldInfo field = obj.GetType().GetField(rgx.Replace(fieldStructure[i], ""), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                    object collection = field.GetValue(obj);

                    if (collection.GetType().IsArray)
                    {
                        return ((object[])collection)[index].GetType();
                    }
                    else if (collection is IEnumerable)
                    {
                        object item = ((IList)collection)[index];
                        if (item == null)
                            return collection.GetType().GetGenericArguments().FirstOrDefault();
                        else
                            return item.GetType();
                    }
                }
                else
                {
                    return obj.GetType().GetField(property.propertyPath, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public).FieldType;
                }
            }

            return default;
        }


        public static T GetValue<T>(this SerializedProperty property)
        {
            return (T)property.GetValue();
        }

        public static object GetValue(this SerializedProperty property)
        {
            object obj = property.serializedObject.targetObject;
            string path = property.propertyPath.Replace(".Array.data", "");
            string[] fieldStructure = path.Split('.');
            Regex rgx = new Regex(@"\[\d+\]");
            for (int i = 0; i < fieldStructure.Length; i++)
            {
                if (fieldStructure[i].Contains("["))
                {
                    int index = System.Convert.ToInt32(new string(fieldStructure[i].Where(c => char.IsDigit(c)).ToArray()));
                    obj = GetFieldValueWithIndex(rgx.Replace(fieldStructure[i], ""), obj, index);
                }
                else
                {
                    obj = GetFieldValue(fieldStructure[i], obj);
                }
            }
            return obj;
        }

        public static bool SetValue<T>(this SerializedProperty property, T value) where T : class
        {
            object obj = property.serializedObject.targetObject;
            string path = property.propertyPath.Replace(".Array.data", "");
            string[] fieldStructure = path.Split('.');
            Regex rgx = new Regex(@"\[\d+\]");
            for (int i = 0; i < fieldStructure.Length - 1; i++)
            {
                if (fieldStructure[i].Contains("["))
                {
                    int index = System.Convert.ToInt32(new string(fieldStructure[i].Where(c => char.IsDigit(c)).ToArray()));
                    obj = GetFieldValueWithIndex(rgx.Replace(fieldStructure[i], ""), obj, index);
                }
                else
                {
                    obj = GetFieldValue(fieldStructure[i], obj);
                }
            }

            string fieldName = fieldStructure.Last();
            if (fieldName.Contains("["))
            {
                int index = System.Convert.ToInt32(new string(fieldName.Where(c => char.IsDigit(c)).ToArray()));
                return SetFieldValueWithIndex(rgx.Replace(fieldName, ""), obj, index, value);
            }
            else
            {
                Debug.Log(value);
                return SetFieldValue(fieldName, obj, value);
            }
        }

        private static object GetFieldValue(string fieldName, object obj, BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
        {
            FieldInfo field = obj.GetType().GetField(fieldName, bindings);
            if (field != null)
            {
                return field.GetValue(obj);
            }
            return default(object);
        }

        private static object GetFieldValueWithIndex(string fieldName, object obj, int index, BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
        {
            FieldInfo field = obj.GetType().GetField(fieldName, bindings);
            if (field != null)
            {
                object list = field.GetValue(obj);
                if (list.GetType().IsArray)
                {
                    return ((object[])list)[index];
                }
                else if (list is IEnumerable)
                {
                    return ((IList)list)[index];
                }
            }
            return default(object);
        }

        public static bool SetFieldValue(string fieldName, object obj, object value, bool includeAllBases = false, BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
        {
            FieldInfo field = obj.GetType().GetField(fieldName, bindings);
            if (field != null)
            {
                field.SetValue(obj, value);
                return true;
            }
            return false;
        }

        public static bool SetFieldValueWithIndex(string fieldName, object obj, int index, object value, bool includeAllBases = false, BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
        {
            FieldInfo field = obj.GetType().GetField(fieldName, bindings);
            if (field != null)
            {
                object list = field.GetValue(obj);
                if (list.GetType().IsArray)
                {
                    ((object[])list)[index] = value;
                    return true;
                }
                else if (value is IEnumerable)
                {
                    ((IList)list)[index] = value;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Set the field value of the <see cref="SerializedProperty"/> to it's default value.
        /// </summary>
        public static void ResetValueToDefault(this SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Generic:
                    // Not sure what to do here.
                    break;
                case SerializedPropertyType.Integer:
                    property.intValue = default;
                    break;
                case SerializedPropertyType.Boolean:
                    property.boolValue = default;
                    break;
                case SerializedPropertyType.Float:
                    property.floatValue = default;
                    break;
                case SerializedPropertyType.String:
                    property.stringValue = default;
                    break;
                case SerializedPropertyType.Color:
                    property.colorValue = default;
                    break;
                case SerializedPropertyType.ObjectReference:
                    property.objectReferenceValue = null;
                    break;
                case SerializedPropertyType.LayerMask:
                    property.intValue = default;
                    break;
                case SerializedPropertyType.Enum:
                    property.enumValueIndex = default;
                    break;
                case SerializedPropertyType.Vector2:
                    property.vector2Value = default;
                    break;
                case SerializedPropertyType.Vector3:
                    property.vector3Value = default;
                    break;
                case SerializedPropertyType.Vector4:
                    property.vector4Value = default;
                    break;
                case SerializedPropertyType.Rect:
                    property.rectValue = default;
                    break;
                case SerializedPropertyType.ArraySize:
                    property.arraySize = default;
                    break;
                case SerializedPropertyType.Character:
                    property.intValue = default;
                    break;
                case SerializedPropertyType.AnimationCurve:
                    property.animationCurveValue = default;
                    break;
                case SerializedPropertyType.Bounds:
                    property.boundsValue = default;
                    break;
                case SerializedPropertyType.Gradient:
                    // Not sure what value to set for this one.
                    break;
                case SerializedPropertyType.Quaternion:
                    property.quaternionValue = default;
                    break;
                case SerializedPropertyType.ExposedReference:
                    break;
                case SerializedPropertyType.FixedBufferSize:
                    break;
                case SerializedPropertyType.Vector2Int:
                    property.vector2IntValue = default;
                    break;
                case SerializedPropertyType.Vector3Int:
                    property.vector3IntValue = default;
                    break;
                case SerializedPropertyType.RectInt:
                    property.rectIntValue = default;
                    break;
                case SerializedPropertyType.BoundsInt:
                    property.boundsIntValue = default;
                    break;
                case SerializedPropertyType.ManagedReference:
                    property.managedReferenceValue = default;
                    break;
                default:
                    break;
            }
        }
    } 
}
