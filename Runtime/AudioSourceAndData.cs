using System;
using UnityEngine;

[Serializable]
public class AudioSourceAndData
{
    [SerializeField] public AudioSourceDataContainer data;
    [SerializeField] public AudioSource AS;

    public void Apply()
    {
        data.data.Apply(AS);
    }

    public void Reset(Component holder)
    {
        AS = holder.GetComponentInChildren<AudioSource>();
    }
    
    public void Reset(AudioSource newAS,AudioSourceDataContainer newData = null)
    {
        AS = newAS;
        data = newData;
    }
}
