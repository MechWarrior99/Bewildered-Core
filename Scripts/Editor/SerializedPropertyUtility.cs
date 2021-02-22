using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace Bewildered.Editor
{
    public static class SerializedPropertyUtility
    {
        static Dictionary<SerializedPropertyType, PropertyInfo> _propertyValueAccessors = new Dictionary<SerializedPropertyType, PropertyInfo>();

        public static void SetPropertyValue(this SerializedProperty property, object value)
        {

        }

        private static void SetGenericPropertyValue(this SerializedProperty property, object value)
        {
            if (value != null)
            {
                Type valueType = value.GetType();
                if (valueType.IsAssignableFrom(property.GetPropertyType()))
                {
                    FieldInfo[] fields = valueType.GetFields(ReflectionUtility.CommonFlags);
                    Dictionary<string, object> fieldValues = GetDictionaryFromFields(fields, value);

                    SerializedProperty iterator = property.Copy();
                    if (iterator.Next(true))
                    {
                        SerializedProperty end = iterator.GetEndProperty();
                        do
                        {
                            iterator.SetPropertyValue(fieldValues[iterator.name]);
                        } while (iterator.Next(false) && !SerializedProperty.EqualContents(iterator, end));
                    }
                }
            }
        }

        private static Dictionary<string, object> GetDictionaryFromFields(FieldInfo[] fields, object parentInstance)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();

            foreach (var field in fields)
            {
                dict.Add(field.Name, field.GetValue(parentInstance));
            }

            return dict;
        }

        private static void Initialize()
        {
            if (_propertyValueAccessors.Count == 0)
            {

                Dictionary<SerializedPropertyType, string> serializedPropertyValueAccessorsNameDict = new Dictionary<SerializedPropertyType, string>() {
                    { SerializedPropertyType.Integer, "intValue" },
                    { SerializedPropertyType.Boolean, "boolValue" },
                    { SerializedPropertyType.Float, "floatValue" },
                    { SerializedPropertyType.String, "stringValue" },
                    { SerializedPropertyType.Color, "colorValue" },
                    { SerializedPropertyType.ObjectReference, "objectReferenceValue" },
                    { SerializedPropertyType.LayerMask, "intValue" },
                    { SerializedPropertyType.Enum, "intValue" },
                    { SerializedPropertyType.Vector2, "vector2Value" },
                    { SerializedPropertyType.Vector3, "vector3Value" },
                    { SerializedPropertyType.Vector4, "vector4Value" },
                    { SerializedPropertyType.Rect, "rectValue" },
                    { SerializedPropertyType.ArraySize, "intValue" },
                    { SerializedPropertyType.Character, "intValue" },
                    { SerializedPropertyType.AnimationCurve, "animationCurveValue" },
                    { SerializedPropertyType.Bounds, "boundsValue" },
                    { SerializedPropertyType.Quaternion, "quaternionValue" }
                };
                Type serializedPropertyType = typeof(SerializedProperty);

                foreach (var kvp in serializedPropertyValueAccessorsNameDict)
                {
                    PropertyInfo propertyInfo = serializedPropertyType.GetProperty(kvp.Value, BindingFlags.Instance | BindingFlags.Public);
                    _propertyValueAccessors.Add(kvp.Key, propertyInfo);
                }
            }
        }
    } 
}
