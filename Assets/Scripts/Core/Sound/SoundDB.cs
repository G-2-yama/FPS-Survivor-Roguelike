using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Audio/SoundDB")]
public class SoundDB : ScriptableObject
{
    public List<SoundData> sounds;
}