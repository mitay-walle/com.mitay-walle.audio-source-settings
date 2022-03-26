using UnityEditor;
using UnityEngine;

public class AudioSourceSettings : MonoBehaviour
{
    [SerializeField] private AudioSourceAndDataBulk data = new AudioSourceAndDataBulk();

    private void Start()
    {
        Apply();
    }

    [ContextMenu("Apply")]
    public void Apply()
    {
        data.Apply();
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            for (int i = 0; i < data.data.Length; i++)
                if (data.data[i].AS)
                    EditorUtility.SetDirty(data.data[i].AS);
        }
#endif
    }

#if UNITY_EDITOR

    [ContextMenu("Reset")]
    private void Reset()
    {
        Undo.RecordObject(this, "reset");

        data.Reset(gameObject);

        EditorUtility.SetDirty(this);
    }

#endif
}