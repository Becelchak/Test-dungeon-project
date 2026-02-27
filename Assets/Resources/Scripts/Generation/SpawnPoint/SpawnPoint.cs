using System;
using Unity.VisualScripting;
using UnityEngine;

public class SpawnPoint : MonoBehaviour, ISpawnPoint
{
    [SerializeField] private SpawnType _spawnType;
    [SerializeField] private Vector3 _orientation;

    public SpawnType spawnType => _spawnType;
    public Vector3 orientation => _orientation;

    public Vector3 EffectiveOrientation => _orientation != Vector3.zero ? _orientation : transform.forward;

    public void SpawnStart()
    {
        
    }

    public Vector3 GetSpawnPosition() => transform.position;
    public Quaternion GetSpawnRotation() => Quaternion.LookRotation(EffectiveOrientation);
}
