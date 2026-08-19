using UnityEngine;

/// <summary>
/// Контроллер анимаций NPC. Тонкая обёртка над Animator.
/// </summary>
public class NpcAnimationController : MonoBehaviour
{
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void SetMoving(bool isMoving)
    {
        if (_animator != null)
            _animator.SetBool("IsMoving", isMoving);
    }

    public void SetSpeed(float normalizedSpeed)
    {
        if (_animator != null)
            _animator.SetFloat("Speed", normalizedSpeed);
    }

    public void TriggerAttack()
    {
        if (_animator != null)
            _animator.SetTrigger("Attack");
    }

    public void TriggerHit()
    {
        if (_animator != null)
            _animator.SetTrigger("Hit");
    }

    public void TriggerDeath()
    {
        if (_animator != null)
        {
            _animator.SetTrigger("Die");
            _animator.SetBool("IsAlive", false);
        }
    }

    public void SetStagger(bool status)
    {
        if (_animator != null)
        {
            if(status)
                _animator.SetTrigger("Stagger");
            _animator.SetBool("IsStun", status);
        }
    }

    public void SetBlocking(bool status)
    {
        if (_animator != null)
        {
            if (status)
                _animator.SetTrigger("Block");
            _animator.SetBool("IsBlocking", status);
        }
    }
}
