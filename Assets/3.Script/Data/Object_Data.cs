using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="ObjectData", menuName ="ScriptableObject/Object")]
public class Object_Data : ScriptableObject
{
    [SerializeField] public string Behaviour;
    [SerializeField] public string Caution;

    [SerializeField] public string SoundEventPath;
}
