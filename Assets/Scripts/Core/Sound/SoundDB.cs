using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Audio/SoundDB")]
public class SoundDB : ScriptableObject
{
    [Tooltip("バースト中、弾を発射するたびに発射音を再生する")]
    public bool playFireSoundPerShot = false;

    public List<SoundData> sounds;
}
