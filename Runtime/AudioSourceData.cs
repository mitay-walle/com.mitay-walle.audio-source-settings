using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

[Serializable]
public class AudioSourceData : SaveAsCopyPaste
{
    public static string SaveFieldName = "data";
    public static string SaveType = "AudioSourceDataContainer";

    [Header("Основное")]
    public AudioClip clip;
    public AudioMixerGroup outputAudioMixerGroup;
    [Range(0f,1f)]public float volume;
    [Range(-3f,3f)]public float pitch;

    public bool loop;
    public bool playOnAwake;
    [Range(0f,1f)]public float spatialBlend;
    public float minDistance = 2f;
    public float maxDistance = 30f;
    public AnimationCurve Rolloff;
    public AnimationCurve SpreadCurve;
    public AnimationCurve SpatialBlendСurve;
    public AnimationCurve ReverbZoneMixСurve;

    [Header("Прочее")]
    public AudioRolloffMode rolloffMode = AudioRolloffMode.Custom;
    public float panStereo;
    public float spread = 60f;
    public float dopplerLevel;
    public bool spatializePostEffects;
    public AudioVelocityUpdateMode velocityUpdateMode = AudioVelocityUpdateMode.Dynamic;
    public bool spatialize;
    public float reverbZoneMix;
    public bool bypassEffects;
    public bool bypassListenerEffects;
    public bool bypassReverbZones;
    public int priority = 128;
    public bool mute;


    public void Apply(AudioSource AS)
    {
        AS.clip = clip;
        AS.volume = volume;
        AS.pitch = pitch;
        AS.outputAudioMixerGroup = outputAudioMixerGroup;
        AS.loop = loop;
        AS.playOnAwake = playOnAwake;
        AS.spatialBlend = spatialBlend;
        AS.rolloffMode = rolloffMode;
        AS.minDistance = minDistance;
        AS.maxDistance = maxDistance;
        AS.panStereo = panStereo;
        AS.spread = spread;
        AS.dopplerLevel = dopplerLevel;
        AS.spatializePostEffects = spatializePostEffects;
        AS.velocityUpdateMode = velocityUpdateMode;
        AS.spatialize = spatialize;
        AS.reverbZoneMix = reverbZoneMix;
        AS.bypassEffects = bypassEffects;
        AS.bypassListenerEffects = bypassListenerEffects;
        AS.bypassReverbZones = bypassReverbZones;
        AS.priority = priority;
        AS.mute = mute;
        
        if (rolloffMode == AudioRolloffMode.Custom) AS.SetCustomCurve(AudioSourceCurveType.CustomRolloff,Rolloff);
        AS.SetCustomCurve(AudioSourceCurveType.Spread,SpreadCurve);
        AS.SetCustomCurve(AudioSourceCurveType.SpatialBlend,SpatialBlendСurve);
        AS.SetCustomCurve(AudioSourceCurveType.ReverbZoneMix,ReverbZoneMixСurve);
        
        #if UNITY_EDITOR
        if (Application.isPlaying) EditorUtility.SetDirty(AS);
        #endif
    }

    public AudioSourceData(){}
    public AudioSourceData(AudioSource AS)
    {
        clip = AS.clip;
        volume = AS.volume;
        pitch = AS.pitch;
        outputAudioMixerGroup = AS.outputAudioMixerGroup;
        loop = AS.loop;
        playOnAwake = AS.playOnAwake;
        spatialBlend = AS.spatialBlend;
        rolloffMode = AS.rolloffMode;
        minDistance = AS.minDistance;
        maxDistance = AS.maxDistance;
        panStereo = AS.panStereo;
        spread = AS.spread;
        dopplerLevel = AS.dopplerLevel;
        spatializePostEffects = AS.spatializePostEffects;
        velocityUpdateMode = AS.velocityUpdateMode;
        spatialize = AS.spatialize;
        reverbZoneMix = AS.reverbZoneMix;
        bypassEffects = AS.bypassEffects;
        bypassListenerEffects = AS.bypassListenerEffects;
        bypassReverbZones = AS.bypassReverbZones;
        priority = AS.priority;
        mute = AS.mute;
        
        Rolloff = AS.GetCustomCurve(AudioSourceCurveType.CustomRolloff);
        SpreadCurve = AS.GetCustomCurve(AudioSourceCurveType.Spread);
        SpatialBlendСurve = AS.GetCustomCurve(AudioSourceCurveType.SpatialBlend);
        ReverbZoneMixСurve = AS.GetCustomCurve(AudioSourceCurveType.ReverbZoneMix);
    }
}