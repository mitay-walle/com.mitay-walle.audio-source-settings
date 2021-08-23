using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Reflection;
using UnityEditor;

[Serializable]
public class AudioSourceAndDataBulk
{
    public AudioSourceAndData[] data;

    public void Apply()
    {
        #if UNITY_EDITOR
        if (!Application.isPlaying) for (int i = 0; i < data.Length; i++) if (data[i].AS) Undo.RecordObject(data[i].AS,"apply");
        #endif
        
        for (int i = 0; i < data.Length; i++)
            data[i].Apply();
        
#if UNITY_EDITOR
        if (!Application.isPlaying) for (int i = 0; i < data.Length; i++) if (data[i].AS) EditorUtility.SetDirty(data[i].AS);
#endif
    }

#if UNITY_EDITOR
    
    public void Reset(GameObject holder)
    {
        var ASes = holder.GetComponentsInChildren<AudioSource>().ToList();
        var oldASes = new List<AudioSource>();
        var oldData = data?.ToList();
        
        if (data != null && data.Length > 0)
        {
            oldASes = data.Select(d => d.AS).ToList();
        }
        
        if (data == null || data.Length != ASes.Count) data = new AudioSourceAndData[ASes.Count];

        for (int i = 0; i < ASes.Count; i++)
        {
            if (oldASes.Contains(ASes[i]))
            {
                var index = oldASes.FindIndex(a => a == ASes[i]);

                if (oldData[index].data)
                {
                    data[i].Reset(ASes[i],oldData[index].data);
                }
                
            }
            else
            {
                if (data[i] == null) data[i] = new AudioSourceAndData();
                
                data[i].Reset(ASes[i]);
            }
        }    
    }
    
    
[CustomPropertyDrawer(typeof(AudioSourceAndDataBulk), true)]
public class AudioSourceAndDataBulkEditor : PropertyDrawer
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
            var size = pos.size;
            size.x = 50f;

            pos.x += pos.width - size.x * 2f;
            size.y = 17f;
            pos.size = size;


            if (GUI.Button(pos, "apply"))
            {
                var parentObj = GetParent(property);

                if (isArray)
                {
                    //GUIUtility.systemCopyBuffer = JsonUtility.ToJson(fInfo.GetFieldValueWithIndex(parentObj,index));
                }
                else
                {
                    (fInfo.GetValue(parentObj) as AudioSourceAndDataBulk).Apply();  
                }
            }

            pos.x += size.x;

            if (GUI.Button(pos, "reset"))
            {
                Undo.RecordObject(property.serializedObject.targetObject,"Вставить Custom");
                var parentObj = GetParent(property);

                if (isArray)
                {
//                    var newInst = JsonUtility.FromJson(GUIUtility.systemCopyBuffer, 
//                        fInfo.GetFieldValueWithIndex(parentObj,index).GetType());
//                    
//                    fInfo.SetFieldValueWithIndex(parentObj,index,newInst);
//
//                    EditorUtility.SetDirty(property.serializedObject.targetObject);
//                    
//                    Debug.Log(JsonUtility.ToJson(fInfo.GetFieldValueWithIndex(parentObj,index)));
                }
                else
                {
                    (fInfo.GetValue(parentObj) as AudioSourceAndDataBulk).Reset((property.serializedObject.targetObject as Component).gameObject);
                    
                    //Debug.Log(fInfo.GetValue(parentObj));

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
#endif
}