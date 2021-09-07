# Bewildered-Core
 Common use data types, and utilities for Unity that are high-quality and look and feel like a native part of Unity.
 
 ## Runtime
 ### UDictionary
 A serializable dictionary with reordering elements, supports prefabs, and allows for lists as keys and values.
 It also provides indication of which elements have duplicate keys and will not be added to the dictionary. Duplicates are removed on domain reload (enter playmode, or recompile scripts).
 UDictionarys are used just like Lits are!
```csharp
[serializeField] private UDictionary<string, float> _example = new UDictionary<string, float>();
```
 ![image](https://user-images.githubusercontent.com/8076495/132381819-94d52bcc-fee1-493f-8f18-4dec2a778b31.png)

## Editor
### SerializedPropertyExtensions
```csharp
object GetValue()  
```
Gets the value of the field that the SerializedProperty is associated with. It may be different than the value of the SerializedProperty if the SerializedProperty has changed without calling `serializedObject.ApplyModifiedProperties()`, or the field's value has changed without having called `serializedObject.Update()`.  

```csharp
void SetValue(object value)
```
Sets the value of the field that the SerializedProperty is associated with. `serializedObject.Update()` will need to be called for the value of the SerializedProperty of update the new value.
```csharp
Type GetPropertyFieldType()
```
Gets the declared `System.Type` of the field that the SerializedProperty is associated with.
If the field is `[SerializedField] private ExampleClassA _exampleA = new ExampleClassA();`, then `ExampleClassA` will be the type returned, and if the field is `[SerializedReference private ExampleClassA _exampleA = new ExampleClassB();`, it will also return `ExampleClassA`.  
(In this example, `ExampleClassB` inherits from `ExampleClassA`)

```csharp
Type GetPropertyValueType()
```
Gets the value `System.Type` of the field that the SerializedProperty is associated with.
If the field is `[SerializedField] private ExampleClassA _exampleA = new ExampleClassA();`, then `ExampleClassA` will be the type returned, and if the field is `[SerializedReference private ExampleClassA _exampleA = new ExampleClassB();`, it will also return `ExampleClassB`, since `ExampleClassB` is the *value* assinged to the field.  
(In this example, `ExampleClassB` inherits from `ExampleClassA`)

```csharp
FieldInfo GetFieldInfo()
```
Gets the `FieldInfo` of the field that the SerializedProperty is associated with.

