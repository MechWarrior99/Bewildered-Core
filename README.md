# Bewildered-Core
 Common use data types, and utilities for Unity that are high-quality and look and feel like a native part of Unity.
 
 ## Runtime
 ### UDictionary
A serializable Dictionary with reordering elements, supports prefabs, and allows for lists as keys and values.
It also provides indication of which elements have duplicate keys and will not be added to the Dictionary. Duplicates are removed on domain reload (enter playmode, or recompile scripts).  
UDictionaries are as easy to use like Lists, no bolierplate! 
```csharp
[serializeField] private UDictionary<string, float> _example = new UDictionary<string, float>();
```
 ![image](https://user-images.githubusercontent.com/8076495/132381819-94d52bcc-fee1-493f-8f18-4dec2a778b31.png)
 ### UHashSet
 A serializable HashSet with reordering elements, and support for prefabs.
 It also provides indication of which elements are duplicates and will not be saved to the HashSet. Duplicates are removed on domain reload (enter playmode, or recompile scripts).  
UHashSets are as easy to use like Lists, no bolierplate! 
```csharp
[serializeField] private UHashset<string> _example = new UHashset<string>();
```
![image](https://user-images.githubusercontent.com/8076495/133187043-4557e207-b244-4fcf-a142-73358e7eca37.png)

### TimeValue
Represents a point in time or a duration, provides a more user freindly way to set time than a float.
```csharp
TimeValue duration = new TimeValue()
{
    Minutes = 2,
    Seconds = 35
};
```
vs
```csharp
float duration = 155.0f;
```
Usage is simple as it is in effectively a wrapper around a float,
```csharp
private TimeValue _duration = new TimeValue(2, 35);
private float effectActiveFor = 0

private void Update()
{
    effectActiveFor += Time.deltaTime;
    
    // You can use the implicit conversion.
    if (effectActiveFor < duration)
    {
        // Do something...
    }
    
    // You can also use the time field directly.
    if (effectActiveFor > duration.time)
    {
       // Do something..
    }
}
```
It is fully integrated in to the editor with a clean property drawer.
![image](https://user-images.githubusercontent.com/8076495/137603646-9e0d43e7-a882-4d85-95c7-b79e89047676.png)


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

