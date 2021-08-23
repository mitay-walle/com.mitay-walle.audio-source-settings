using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public abstract class SaveAsCopyPaste : CopyPaste
{
}

[Serializable]
public abstract class CopyPaste
{
    public void CopyToClipboard<T>() where T : CopyPaste
    {
        GUIUtility.systemCopyBuffer = JsonUtility.ToJson(this as T);
    }
    public void PasteFromClipboard<T>(out T container) where T : CopyPaste
    {
        container = JsonUtility.FromJson<T>(GUIUtility.systemCopyBuffer);
    }
}


#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(CopyPaste), true)]
public class CopyPasteAttributeEditor : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, property.isExpanded);
    }

    public static void DrawButtons(Rect position, SerializedProperty property, FieldInfo fInfo,bool useExpanded = true)
    {
        var check = !useExpanded;
        if (useExpanded) check |= property.isExpanded;
        
        if (check)
        {
            var isArray = property.propertyPath[property.propertyPath.Length - 1] == ']';

            var index = -1;
        
            if (isArray)
            {
                var splitted = property.propertyPath.Split('[');
                var indexString = splitted[splitted.Length - 1];
                index = int.Parse(indexString.Split(']')[0]);
            }
            
            var pos = position;
            pos.x += pos.width - 75f;
            var size = pos.size;
            size.x = 25f;
            size.y = 15f;
            pos.size = size;


            pos.x += size.x;

            
            
            if (GUI.Button(pos, "C"))
            {
                var parentObj = GetParent(property);

                if (isArray)
                {
                    GUIUtility.systemCopyBuffer = JsonUtility.ToJson(GetFieldValueWithIndex(fInfo,parentObj,index));
                }
                else
                {
                    GUIUtility.systemCopyBuffer = JsonUtility.ToJson(fInfo.GetValue(parentObj));    
                }
                
                Debug.Log(GUIUtility.systemCopyBuffer);
            }

            pos.x += size.x;

            if (GUI.Button(pos, "P"))
            {
                Undo.RecordObject(property.serializedObject.targetObject,"Вставить Custom");
                var parentObj = GetParent(property);

                if (isArray)
                {
                    var newInst = JsonUtility.FromJson(GUIUtility.systemCopyBuffer, 
                        GetFieldValueWithIndex(fInfo,parentObj,index).GetType());
                    
                    SetFieldValueWithIndex(fInfo,parentObj,index,newInst);

                    EditorUtility.SetDirty(property.serializedObject.targetObject);
                    
                    Debug.Log(JsonUtility.ToJson(GetFieldValueWithIndex(fInfo,parentObj,index)));
                }
                else
                {
                    var newInst = JsonUtility.FromJson(GUIUtility.systemCopyBuffer, fInfo.FieldType);
                    
                    fInfo.SetValue(parentObj, newInst);
                    
                    EditorUtility.SetDirty(property.serializedObject.targetObject);
                    
                    Debug.Log(fInfo.GetValue(parentObj));

                }

            }
        }
    }
    
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.isArray)
            EditorGUI.BeginProperty(position, label, property);

        DrawButtons(position, property,fieldInfo);

        EditorGUI.PropertyField(position, property, label, property.isExpanded);

        if (property.isArray)
            EditorGUI.EndProperty();
    }
    
      
    public static T GetFieldValueWithIndex<T>(FieldInfo field, object obj, int index,
        BindingFlags bindings =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
    {
        if (field != null)
        {
            object list = field.GetValue(obj);
            if (list.GetType().IsArray)
            {
                return (T) ((object[]) list)[index];
            }
            else if (list is IEnumerable)
            {
                return (T) ((IList) list)[index];
            }
        }

        return default;
    }

    public static object GetFieldValueWithIndex(FieldInfo field, object obj, int index,
        BindingFlags bindings =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
    {
        if (field != null)
        {
            object list = field.GetValue(obj);
            if (list.GetType().IsArray)
            {
                return ((object[]) list)[index];
            }
            else if (list is IEnumerable)
            {
                return ((IList) list)[index];
            }
        }

        return default(object);
    }
    
    public static bool SetFieldValueWithIndex(FieldInfo field, object obj, int index, object value,
        bool includeAllBases = false,
        BindingFlags bindings =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
    {
        if (field != null)
        {
            object list = field.GetValue(obj);
            if (list.GetType().IsArray)
            {
                //Debug.Log(value.GetType());
                //Debug.Log(((object[]) list).GetType());
                ((object[]) list)[index] = value;
                return true;
            }
            else if (value is IEnumerable)
            {
                ((IList) list)[index] = value;
                return true;
            }
        }

        return false;
    }
    
    public static object GetParent(SerializedProperty prop)
    {
        var path = prop.propertyPath.Replace(".Array.data[", "[");
        object obj = prop.serializedObject.targetObject;
        var elements = path.Split('.');
        foreach (var element in elements.Take(elements.Length - 1))
        {
            if (element.Contains("["))
            {
                var elementName = element.Substring(0, element.IndexOf("["));
                var index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                obj = GetValue(obj, elementName, index);
            }
            else
            {
                obj = GetValue(obj, element);
            }
        }

        return obj;
    }
    
    public static object GetValue(object source, string name, int index)
    {
        var enumerable = GetValue(source, name) as IEnumerable;
        var enm = enumerable.GetEnumerator();
        while (index-- >= 0)
            enm.MoveNext();
        return enm.Current;
    }
    
    public static object GetValue(object source, string name)
    {
        if (source == null)
            return null;
        var type = source.GetType();
        var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        if (f == null)
        {
            var p = type.GetProperty(name,
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (p == null)
                return null;
            return p.GetValue(source, null);
        }

        return f.GetValue(source);
    }
}

[CustomPropertyDrawer(typeof(SaveAsCopyPaste), true)]
class SaveAsCopyPasteAttributeEditor : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, property.isExpanded);
    }

    public void DrawButtons(Rect position, SerializedProperty property,FieldInfo fInfo)
    {

        var isArray = property.propertyPath[property.propertyPath.Length - 1] == ']';

        var index = -1;
        
        if (isArray)
        {
            var splitted = property.propertyPath.Split('[');
            var indexString = splitted[splitted.Length - 1];
            index = int.Parse(indexString.Split(']')[0]);
        }
        
        
        if (property.isExpanded)
        {
            var pos = position;
            pos.x += pos.width - 100f;
            var size = pos.size;
            size.x = 25f;
            size.y = 15f;
            pos.size = size;

            if (GUI.Button(pos, "S"))
                Save(property,isArray,index);

            pos.x += size.x;

            if (GUI.Button(pos, "C"))
            {
                if (isArray)
                {
                    GUIUtility.systemCopyBuffer = JsonUtility.ToJson(GetFieldValueWithIndex(fInfo,property.serializedObject.targetObject,index));
                }
                else
                {
                    GUIUtility.systemCopyBuffer =
                        JsonUtility.ToJson(fInfo.GetValue(property.serializedObject.targetObject));    
                }
                
                Debug.Log(GUIUtility.systemCopyBuffer);
            }

            pos.x += size.x;

            if (GUI.Button(pos, "P"))
            {
                Undo.RecordObject(property.serializedObject.targetObject,"Вставить Custom");
                
                if (isArray)
                {
                    var parentObj = GetParent(property);
                    var newInst = JsonUtility.FromJson(GUIUtility.systemCopyBuffer, 
                        GetFieldValueWithIndex(fInfo,parentObj,index).GetType());
                    
                    SetFieldValueWithIndex(fInfo,parentObj,index,newInst);
                }
                else
                {
                    var newInst = JsonUtility.FromJson(GUIUtility.systemCopyBuffer, fInfo.FieldType);
                    fInfo.SetValue(property.serializedObject.targetObject, newInst);
                }
            }
        }
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        DrawButtons(position, property,fieldInfo);

        EditorGUI.PropertyField(position, property, label, property.isExpanded);
    }

    void Save(SerializedProperty property,bool isArray, int index)
    {
        var assetType = string.Empty;

        Type userFieldType = fieldInfo.FieldType;

        if (userFieldType.IsArray)
        {
            userFieldType = GetFieldValueWithIndex(fieldInfo,property.serializedObject.targetObject,index).GetType();
        }



        
        Debug.Log($"user Field Type: { userFieldType.Name}");

        assetType = (string) userFieldType.GetField("SaveType", BindingFlags.Public | BindingFlags.Static)
            .GetValue(null);

        Debug.Log($"asset Type string: {assetType}");
        
        var unityObj = ScriptableObject.CreateInstance(assetType);

        if (!unityObj)
        {
            Debug.LogError("незнакомый тип ассета!");
            return;
        } 
        Debug.Log($"assetType parsed name: {unityObj.GetType()}");
        
        Debug.Log($"userField for saving: {(string) userFieldType.GetField("SaveFieldName", BindingFlags.Public | BindingFlags.Static).GetValue(null)}");
        
        var dataFieldInfo = unityObj.GetType().GetField((string) userFieldType.GetField("SaveFieldName", BindingFlags.Public | BindingFlags.Static).GetValue(null));

        if (isArray)
        {
            dataFieldInfo.SetValue(unityObj,GetFieldValueWithIndex(fieldInfo,property.serializedObject.targetObject,index));
        }
        else
        {
            dataFieldInfo.SetValue(unityObj,fieldInfo.GetValue(property.serializedObject.targetObject));
        }
        
        var path = EditorUtility.SaveFilePanelInProject($"Сохранить {property.name} в {assetType}", assetType,
            "asset",
            $"сохраните данные в контейнере, типа {assetType}");

        AssetDatabase.CreateAsset(unityObj, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    
    
    public static object GetParent(SerializedProperty prop)
    {
        var path = prop.propertyPath.Replace(".Array.data[", "[");
        object obj = prop.serializedObject.targetObject;
        var elements = path.Split('.');
        foreach (var element in elements.Take(elements.Length - 1))
        {
            if (element.Contains("["))
            {
                var elementName = element.Substring(0, element.IndexOf("["));
                var index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                obj = GetValue(obj, elementName, index);
            }
            else
            {
                obj = GetValue(obj, element);
            }
        }

        return obj;
    }
    
    public static object GetValue(object source, string name, int index)
    {
        var enumerable = GetValue(source, name) as IEnumerable;
        var enm = enumerable.GetEnumerator();
        while (index-- >= 0)
            enm.MoveNext();
        return enm.Current;
    }
    
    public static object GetValue(object source, string name)
    {
        if (source == null)
            return null;
        var type = source.GetType();
        var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        if (f == null)
        {
            var p = type.GetProperty(name,
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (p == null)
                return null;
            return p.GetValue(source, null);
        }

        return f.GetValue(source);
    }
    
    public static T GetFieldValueWithIndex<T>(FieldInfo field, object obj, int index,
        BindingFlags bindings =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
    {
        if (field != null)
        {
            object list = field.GetValue(obj);
            if (list.GetType().IsArray)
            {
                return (T) ((object[]) list)[index];
            }
            else if (list is IEnumerable)
            {
                return (T) ((IList) list)[index];
            }
        }

        return default;
    }

    public static object GetFieldValueWithIndex(FieldInfo field, object obj, int index,
        BindingFlags bindings =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
    {
        if (field != null)
        {
            object list = field.GetValue(obj);
            if (list.GetType().IsArray)
            {
                return ((object[]) list)[index];
            }
            else if (list is IEnumerable)
            {
                return ((IList) list)[index];
            }
        }

        return default(object);
    }
    
    public static bool SetFieldValueWithIndex(FieldInfo field, object obj, int index, object value,
        bool includeAllBases = false,
        BindingFlags bindings =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
    {
        if (field != null)
        {
            object list = field.GetValue(obj);
            if (list.GetType().IsArray)
            {
                //Debug.Log(value.GetType());
                //Debug.Log(((object[]) list).GetType());
                ((object[]) list)[index] = value;
                return true;
            }
            else if (value is IEnumerable)
            {
                ((IList) list)[index] = value;
                return true;
            }
        }

        return false;
    }
}
#endif