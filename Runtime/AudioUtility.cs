using System;
using UnityEditor;
using UnityEngine;

public static class AudioUtility
{
    public static void RealisticRolloff(this AudioSource AS)
    {
        var animCurve = new AnimationCurve(
            new Keyframe(AS.minDistance, 1f),
            new Keyframe(AS.minDistance + (AS.maxDistance - AS.minDistance) / 4f, .35f),
            new Keyframe(AS.maxDistance, 0f));

        AS.spatialBlend = 1f;
        AS.rolloffMode = AudioRolloffMode.Custom;
        AS.dopplerLevel = 0f;
        AS.spread = 60f;
        animCurve.SmoothTangents(1, .025f);
        AS.SetCustomCurve(AudioSourceCurveType.CustomRolloff, animCurve);
    }
    
    #if UNITY_EDITOR

    [MenuItem("CONTEXT/AudioSource/copy data")]
    public static void CreateSourceData(MenuCommand command)
    {
        var data = JsonUtility.ToJson(new AudioSourceData((AudioSource) command.context));
        
        Debug.Log(data);
        
        EditorGUIUtility.systemCopyBuffer = data;
    }
    
    [MenuItem("CONTEXT/AudioSource/apply data")]
    public static void ApplySourceData(MenuCommand command)
    {
        try
        {
            var AS = (AudioSource) command.context;
            
            Undo.RecordObject(AS, "применение настроек к AudioSource");

            var data = JsonUtility.FromJson<AudioSourceData>(EditorGUIUtility.systemCopyBuffer);
            data.Apply(AS);
            
            EditorUtility.SetDirty(AS);
        }
        catch(Exception e)
        {
            Debug.LogError(e);
        }
    }
    
    [MenuItem("CONTEXT/AudioSource/реалистичные настройки")]
    public static void RalisticRolloff(MenuCommand command)
    {
        Undo.RecordObject(command.context, "AudioSource реалистичные настройки");
        ((AudioSource) command.context).RealisticRolloff();
        EditorUtility.SetDirty(command.context);
    }
    
    #endif
}
