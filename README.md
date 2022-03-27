# Unity AudioSource Settings
Allow to save all AudioSource values to ScriptableObject, to: 
- setup runtime-created AudioSources
- keep settings of multiplie AudioSources in sync
- create RingBuffer of AudioSources

![alt text](https://github.com/mitay-walle/AudioSourceSettings/blob/master/Documentation/inspector_preview.png?raw=true)

# Problem

## I. Multiplie AS + same settings = Impossible
- [built-in Presets-system](https://docs.unity3d.com/Manual/Presets.html) is Editor-Only 

## II. No way to setup Runtime-created/prespawned AS in different Prefabs/Scenes
You have to:
- open each context (prefab / scene)
- select certain AudioSources
- setup source
- repeat for each context
- repeat each time, to maintain AudioSources values in sync

or

- runtime - hardcode generated settings in code
- write your own Presets-system

# Solution
Presets system based on C#-plain class and ScriptableObject as Containers
<br>I. ScriptableObject-based preset can be shared between multiplie AS and contexts
<br>II. preset can be applied at runtime with Preset.Apply(AudioSource)

# Summary
- UPM package
- Ready-to-use MonoBehavior - AudioSourceSettings
- small API to customize behaviour
- Presets can be ScriptableObjects or plain C# class in your code
- create preset with ProjectWindow/RMB/Create/Audio/AudioSourceData container 

![alt text](https://github.com/mitay-walle/AudioSourceSettings/blob/master/Documentation/create_menu.png?raw=true)
