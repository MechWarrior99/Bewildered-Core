using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;

namespace Bewildered.Editor
{
    /// <summary>
    /// Extensions for <see cref="SerializedProperty"/> that involve getting information about the field associated with a <see cref="SerializedProperty"/>.
    /// </summary>
    public static class SerializedPropertyValueExtensions
    {
        private const string _arrayDataPattern = @"Array\.data\[[0-9]+\]";
        private const string _arrayDataReplacement = "__ArrayElement__";

        private static PropertyInfo _isReferencingAManagedReferenceFieldInfo;
        private static MethodInfo _getFullyQualifiedTypenameForCurrentTypeTreeInternalInfo;
        private static MethodInfo _getPropertyPathInCurrentManagedTypeTreeInternalInfo;

        /// <summary>
        /// Returns the in value in memory of the field associated with the <see cref="SerializedProperty"/>. 
        /// </summary>
        /// /// <remarks>The returned value may be different than the value of the <see cref="SerializedProperty"/>.</remarks>
        /// <typeparam name="T">The type of field value.</typeparam>
        /// <returns>The value in memory of the field.</returns>
        public static T GetValue<T>(this SerializedProperty property)
        {
            return (T)property.GetValue();
        }

        /// <summary>
        /// Returns the in value in memory of the field associated with the <see cref="SerializedProperty"/>. 
        /// </summary>
        /// <remarks>The returned value may be different than the value of the <see cref="SerializedProperty"/>.</remarks>
        /// <returns>The value in memory of the field.</returns>
        public static object GetValue(this SerializedProperty property)
        {
            // Checks if the targetObject has been denstoryed but the property is still valid.
            if (property.serializedObject.targetObject == null)
                return null;

            return GetValue(property.serializedObject.targetObject, property.propertyPath);
        }

        private static object GetValue(object obj, string propertyPath)
        {
            // Replacing array.data leaves ".[0]".
            propertyPath = propertyPath.Replace("Array.data", "");
            string[] fieldStructure = propertyPath.Split('.');

            foreach (string fieldName in fieldStructure)
            {
                if (fieldName.StartsWith("["))
                {
                    obj = GetElementValue(obj, fieldName);
                }
                else
                {
                    obj = GetFieldValue(obj, fieldName);
                }
            }

            return obj;
        }

        private static object GetFieldValue(object obj, string fieldName)
        {
            var fieldInfo = AccessUtility.Field(obj.GetType(), fieldName);
            if (fieldInfo != null)
                return fieldInfo.GetValue(obj);
            else
                return default;
        }

        private static object GetElementValue(object obj, string indexer)
        {
            // The provided string is formatted as "[0]".
            int index = Convert.ToInt32(indexer.Substring(1, indexer.Length - 2));
            // Both array[] and List<> implement IList, and those are the only types of collections Unity serializes.
            return ((IList)obj)[index];
        }

        /// <summary>
        /// Directly sets the value of the field that the <see cref="SerializedProperty"/> is associated with.
        /// </summary>
        /// <remarks>
        /// After setting the value, <see cref="SerializedObject.Update"/> or <see cref="SerializedObject.UpdateIfRequiredOrScript"/> should be called to update
        /// the <see cref="SerializedObject"/> to match the newly set value.
        /// </remarks>
        /// <param name="value">The value to set.</param>
        public static void SetValue(this SerializedProperty property, object value)
        {
            object obj = property.serializedObject.targetObject;
            string propertyPath = property.propertyPath;
            string basePropertyPath = GetBasePropertyPath(property);

            if (!string.IsNullOrEmpty(basePropertyPath))
                obj = GetValue(obj, basePropertyPath);

            if (property.IsArrayElement())
            {
                SetElementValue(obj, propertyPath.Substring(propertyPath.LastIndexOf('[')), value);
            }
            else
            {
                SetFieldValue(obj, propertyPath.Substring(propertyPath.LastIndexOf('.') + 1), value);
            }
        }

        private static string GetBasePropertyPath(SerializedProperty property)
        {
            string propertyPath = property.propertyPath;
            int startIndex;

            if (property.IsArrayElement())
                startIndex = propertyPath.LastIndexOf(".Array.data");
            else
                startIndex = propertyPath.LastIndexOf('.');

            if (startIndex > -1)
                return propertyPath.Remove(startIndex);
            else
                return string.Empty;
        }

        private static void SetFieldValue(object obj, string name, object value)
        {
            var fieldInfo = AccessUtility.Field(obj.GetType(), name);

            if (fieldInfo != null)
            {
                ValidateAssignment(fieldInfo, name, value);
                fieldInfo.SetValue(obj, value);
            }
        }

        private static void ValidateAssignment(FieldInfo fieldInfo, string name, object value)
        {
            if (!fieldInfo.FieldType.IsAssignableFrom(value.GetType()))
            {
                throw new ArgumentException($"Cannot assign value of type '{value.GetType()}', to field '{name}', of type '{fieldInfo.FieldType}'");
            }
        }

        private static void SetElementValue(object obj, string indexer, object value)
        {
            // The provided string is formatted as "[0]".
            int index = Convert.ToInt32(indexer.Substring(1, indexer.Length - 2));
            // Both array[] and List<> implement IList, and those are the only types of collections Unity serializes.
            ((IList)obj)[index] = value;
        }

        /// <summary>
        /// Returns the <see cref="Type"/> for the field associated with the <see cref="SerializedProperty"/>.
        /// </summary>
        /// <remarks>For managed reference types, it will return the declared field <see cref="Type"/>, not the <see cref="Type"/> of the instance assigned to the field.</remarks>
        /// <returns>The <see cref="Type"/> of the <see cref="SerializedProperty"/> field.</returns>
        public static Type GetPropertyFieldType(this SerializedProperty property)
        {
            return property.GetFieldInfo().FieldType;
        }


        /// <summary>
        /// Returns the <see cref="Type"/> for the value assigned to the field associated with the <see cref="SerializedProperty"/>.
        /// </summary>
        /// <remarks>For managed reference types, it will return the <see cref="Type"/> of the instance assigned to the field, not the field's declared <see cref="Type"/>.</remarks>
        /// <returns>The <see cref="Type"/> of the <see cref="SerializedProperty"/> field's value.</returns>
        public static Type GetPropertyValueType(this SerializedProperty property)
        {
            FieldInfo fieldInfo = property.GetFieldInfo();
            if (fieldInfo == null)
                return null;

            if (property.propertyType == SerializedPropertyType.ManagedReference)
            {
                if (TryGetTypeFromManagedReferenceName(property.managedReferenceFullTypename, out Type managedReferenceValueType))
                {
                    return managedReferenceValueType;
                }
            }

            return fieldInfo.FieldType;
        }

        /// <summary>
        /// Returns the <see cref="FieldInfo"/> for the field that the <see cref="SerializedProperty"/> is associated with.
        /// </summary>
        public static FieldInfo GetFieldInfo(this SerializedProperty property)
        {
            Type classType = GetScriptTypeFromProperty(property);
            string fieldPath = property.propertyPath;

            // Managed Reference support based on: https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/ScriptAttributeGUI/ScriptAttributeUtility.cs#L216

            // We check if the property is a a 'child' of a property that is a managed reference.
            if (property.IsReferencingAManagedReferenceField())
            {
                // When the field we are trying to access is a dynamic instance (different type than the field type), it gets more complex
                // since we cannot get the acutal "classType" from just looking at the parent class field type.

                // We get the type of the value of the managed reference property.
                // We then set the "classType" to it and get the field path to be reletive to the managed reference value.
                // All further operations are done reletive to this managed reference class now instead of the targetObject now.

                string objectTypename = property.GetFullyQualifiedTypenameForCurrentTypeTreeInternal();
                TryGetTypeFromManagedReferenceName(objectTypename, out classType);
                
                // Get the path of the property reletive to the managed reference class instance.
                fieldPath = property.GetPropertyPathInCurrentManagedTypeTreeInternal();
            }

            // We replace the array part because it has the extra '.' and we don't need the index info
            // because an element can only be one type, and the path is already reletive to the managed reference if there is one.
            fieldPath = Regex.Replace(fieldPath, _arrayDataPattern, _arrayDataReplacement);

            string[] fieldStructure = fieldPath.Split('.');
            Type type = classType;
            FieldInfo fieldInfo = null;

            foreach (string fieldName in fieldStructure)
            {
                if (fieldName == _arrayDataReplacement)
                {
                    type = GetElementType(type);
                }
                else
                {
                    fieldInfo = AccessUtility.Field(type, fieldName);
                    type = fieldInfo.FieldType;
                }
            }

            return fieldInfo;
        }

        /// <summary>
        /// A property can reference any element in the parent SerializedObject.
        /// In the context of polymorphic serialization, those elements might be dynamic instances
        /// not statically discoverable from the class type.
        /// We need to take a very specific code path when we try to get the type of a field
        /// inside such a dynamic instance through a SerializedProperty.
        /// </summary>
        private static bool IsReferencingAManagedReferenceField(this SerializedProperty property)
        {
            if (_isReferencingAManagedReferenceFieldInfo == null)
                _isReferencingAManagedReferenceFieldInfo = AccessUtility.Property(typeof(SerializedProperty), "isReferencingAManagedReferenceField");

            return (bool)_isReferencingAManagedReferenceFieldInfo.GetValue(property);
        }

        /// <summary>
        /// Returns the FQN in the format "'assembly name' 'full class name'" for the current dynamic managed reference.
        /// </summary>
        private static string GetFullyQualifiedTypenameForCurrentTypeTreeInternal(this SerializedProperty property)
        {
            if (_getFullyQualifiedTypenameForCurrentTypeTreeInternalInfo == null)
                _getFullyQualifiedTypenameForCurrentTypeTreeInternalInfo = AccessUtility.Method(typeof(SerializedProperty), "GetFullyQualifiedTypenameForCurrentTypeTreeInternal");

            return (string)_getFullyQualifiedTypenameForCurrentTypeTreeInternalInfo.Invoke(property, null);
        }

        /// <summary>
        /// Returns the path of the current field reletive to the managed reference class.
        /// </summary>
        private static string GetPropertyPathInCurrentManagedTypeTreeInternal(this SerializedProperty property)
        {
            if (_getPropertyPathInCurrentManagedTypeTreeInternalInfo == null)
                _getPropertyPathInCurrentManagedTypeTreeInternalInfo = AccessUtility.Method(typeof(SerializedProperty), "GetPropertyPathInCurrentManagedTypeTreeInternal");

            return (string)_getPropertyPathInCurrentManagedTypeTreeInternalInfo.Invoke(property, null);
        }

        /// <summary>
        /// Gets the <see cref="Type"/> from a managed reference full type name.
        /// The expected format for the typename string is the one returned by <see cref="SerializedProperty.managedReferenceFullTypename"/>.
        /// </summary>
        /// <param name="fullTypename">The full managed reference typename.</param>
        /// <param name="managedReferenceType">The <see cref="Type"/> of the managed reference.</param>
        /// <returns><c>true</c> if it was able to get a <see cref="Type"/> from <paramref name="fullTypename"/>; otherwise, <c>false</c>.</returns>
        private static bool TryGetTypeFromManagedReferenceName(string fullTypename, out Type managedReferenceType)
        {
            managedReferenceType = null;

            var parts = fullTypename.Split(' ');
            if (parts.Length == 2)
            {
                var assemblyPart = parts[0];
                var nsClassnamePart = parts[1];
                managedReferenceType = Type.GetType($"{nsClassnamePart}, {assemblyPart}");
            }

            return managedReferenceType != null;
        }

        /// <summary>
        /// Get the <see cref="Type"/> of the class the <see cref="SerializedProperty"/> is for targeting.
        /// </summary>
        private static Type GetScriptTypeFromProperty(SerializedProperty property)
        {
            if (property.serializedObject.targetObject != null)
                return property.serializedObject.targetObject.GetType();

            // Fallback in case the targetObject has been destroyed but the property is still valid.
            SerializedProperty scriptProperty = property.serializedObject.FindProperty("m_Script");

            if (scriptProperty == null)
                return null;

            MonoScript script = scriptProperty.objectReferenceValue as MonoScript;

            return script?.GetClass();
        }

        private static Type GetElementType(Type type)
        {
            if (type.IsArray)
                return type.GetElementType();
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                return type.GetGenericArguments()[0];
            else
                return null;

        }
    }
}
