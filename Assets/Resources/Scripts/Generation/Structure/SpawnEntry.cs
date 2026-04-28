using System;
using UnityEngine;

[Serializable]
public struct SpawnEntry
{
    public GameObject prefab;
    [Tooltip("Чем выше вес, тем чаще выбирается данная сущность")]
    public int weight;
}