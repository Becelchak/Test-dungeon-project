using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Компонент, который прикрепляется к щиту игрока и детектирует попадания в него.
/// Используется для направленного блока - атака блокируется только если попала в коллайдер щита.
/// </summary>
public class ShieldCollider : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Коллайдер щита для детекции попаданий")]
    [SerializeField] private Collider _shieldCollider;

    [Tooltip("Слой, который будет детектировать коллайдер щита")]
    [SerializeField] private LayerMask _hitLayer;

    [Tooltip("Время после попадания, когда попадание считается актуальным")]
    [SerializeField] private float _hitValidityWindow = 0.2f;

    private HashSet<GameObject> _recentHits = new HashSet<GameObject>();
    private Dictionary<GameObject, float> _hitTimes = new Dictionary<GameObject, float>();

    private void Awake()
    {
        if (_shieldCollider == null)
            _shieldCollider = GetComponent<Collider>();

        if (_shieldCollider != null)
            _shieldCollider.isTrigger = true;
    }

    private void Update()
    {
        float currentTime = Time.time;
        var expiredHits = new List<GameObject>();

        foreach (var kvp in _hitTimes)
        {
            if (currentTime - kvp.Value > _hitValidityWindow)
            {
                expiredHits.Add(kvp.Key);
            }
        }

        foreach (var hit in expiredHits)
        {
            _recentHits.Remove(hit);
            _hitTimes.Remove(hit);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsInHitLayer(other.gameObject.layer))
            return;

        // Регистрируем попадание
        _recentHits.Add(other.gameObject);
        _hitTimes[other.gameObject] = Time.time;

        Debug.Log($"[ShieldCollider] Зафиксировано попадание в щит от: {other.gameObject.name}");
    }

    /// <summary>
    /// Проверяет, было ли попадание от указанного источника в коллайдер щита.
    /// </summary>
    /// <param name="source">Источник атаки (обычно оружие или атакующий объект)</param>
    /// <returns>true, если было попадание в щит от этого источника</returns>
    public bool WasHitBySource(GameObject source)
    {
        if (source == null)
            return false;

        // Проверяем прямой удар
        if (_recentHits.Contains(source))
            return true;

        // Проверяем, является ли source частью иерархии попавшего объекта
        foreach (var hitObject in _recentHits)
        {
            if (hitObject != null && IsSameOrChild(source, hitObject))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Проверяет, является ли target тем же объектом или дочерним по отношению к source.
    /// </summary>
    private bool IsSameOrChild(GameObject source, GameObject target)
    {
        if (source == target)
            return true;

        Transform current = target.transform;
        while (current != null)
        {
            if (current.gameObject == source)
                return true;
            current = current.parent;
        }

        return false;
    }

    private bool IsInHitLayer(int layer)
    {
        return (_hitLayer.value & (1 << layer)) != 0;
    }

    /// <summary>
    /// Очищает историю попаданий (вызывается при смене оружия/щита)
    /// </summary>
    public void ClearHits()
    {
        _recentHits.Clear();
        _hitTimes.Clear();
    }

    private void OnDisable()
    {
        ClearHits();
    }
}
