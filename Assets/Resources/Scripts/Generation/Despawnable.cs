using UnityEngine;
using System;

public class Despawnable : MonoBehaviour
{
    [SerializeField] private float lifetime = 10f;
    [SerializeField] private SpawnType spawnType;
    [SerializeField] private bool destroyOnCollect = true;

    private SpawnPoint _spawnPoint;
    private bool _collected = false;

    public event Action<SpawnPoint, SpawnType> OnDespawned;

    public void SetSpawnPoint(SpawnPoint point)
    {
        _spawnPoint = point;
    }

    private void Start()
    {
        if (lifetime > 0)
        {
            Invoke(nameof(Despawn), lifetime);
        }
    }

    public void Collect()
    {
        if (_collected) return;
        _collected = true;
        if (destroyOnCollect)
        {
            Despawn();
        }
    }

    private void Despawn()
    {
        OnDespawned?.Invoke(_spawnPoint, spawnType);
        Destroy(gameObject);
        GetComponent<Collider>().enabled = false;
    }

    private void OnDestroy()
    {
        if (!_collected)
        {
            OnDespawned?.Invoke(_spawnPoint, spawnType);
        }
    }
}