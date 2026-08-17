using UnityEngine;

/// <summary>
/// Сервис боевки NPC. Управляет кулдауном атаки и активацией хитбокса оружия.
/// </summary>
public class NpcCombatService : MonoBehaviour
{
    [Tooltip("Коллайдер оружия NPC с компонентом WeaponDamageSource")]
    [SerializeField] private WeaponDamageSource _weaponDamageSource;

    public float CurrentAttackDamage { get; private set; }
    public bool IsAttacking { get; private set; }

    private float _lastAttackTime = -999f;
    private float _attackTimer;
    private float _attackWindup;
    private float _attackDuration;

    private NpcData _data;

    public void Initialize(NpcData data)
    {
        _data = data;
    }

    public bool CanAttack()
    {
        if (_data == null) return false;
        return !IsAttacking && Time.time >= _lastAttackTime + _data.attackCooldown;
    }

    public void StartAttack(float damage, float duration, float windup)
    {
        if (!CanAttack()) return;

        IsAttacking = true;
        CurrentAttackDamage = damage;
        _attackDuration = duration;
        _attackWindup = windup;
        _attackTimer = 0f;
        _lastAttackTime = Time.time;

        if (_weaponDamageSource != null)
        {
            _weaponDamageSource.BaseDamage = damage;
            _weaponDamageSource.ResetHits();
        }
        SetWeaponDamageActive(false);
    }

    public void EndAttack()
    {
        IsAttacking = false;
        CurrentAttackDamage = 0f;
        SetWeaponDamageActive(false);
    }

    private void Update()
    {
        if (!IsAttacking) return;

        _attackTimer += Time.deltaTime;

        bool inActiveWindow = _attackTimer >= _attackWindup && _attackTimer <= _attackDuration * 0.85f;
        SetWeaponDamageActive(inActiveWindow);

        if (_attackTimer >= _attackDuration)
            EndAttack();
    }

    private void SetWeaponDamageActive(bool active)
    {
        if (_weaponDamageSource != null)
            _weaponDamageSource.IsActive = active;
    }
}
