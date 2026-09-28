using UnityEngine;

/// <summary>
/// Простой маркер, который идентифицирует объект как щит игрока.
/// Вешается на коллайдер щита для детекции попаданий в WeaponDamageSource.
/// </summary>
public class ShieldMarker : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Владелец щита")]
    [SerializeField] private Transform _owner;

    /// <summary>Владелец щита для проверки того, что это свой щит.</summary>
    public Transform Owner => _owner;

    private void Awake()
    {
        if (_owner == null)
            _owner = transform.root;
    }
}
