using System;
using System.Collections.Generic;
using UnityEngine;

public class GroundEnemy : MonoBehaviour
{
    [SerializeField] private int hitDamage;

    private Collider2D _collider;
    private Animator _animator;
    private static readonly int AnimatorHitTrigger = Animator.StringToHash("Hit");
    private static readonly int AnimatorIsAttacking = Animator.StringToHash("IsAttacking");

    private readonly HashSet<Action<int>> _damageActions = new();

    public void Awake()
    {
        _animator = GetComponent<Animator>();
        _collider = GetComponent<Collider2D>();
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("AttackHitBox") && _collider.IsTouching(other))
        {
            var player = other.GetComponentInParent<Player>();
            player.SubscribeDamage(OnTakeDamage);
        }
    }

    public void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("AttackHitBox") && !_collider.IsTouching(other))
        {
            var player = other.GetComponentInParent<Player>();
            player.UnSubscribeDamage(OnTakeDamage);
        }
    }

    public void OnTakeDamage(int damage)
    {
        _animator.SetTrigger(AnimatorHitTrigger);
    }

    public void SubscribeDamage(Action<int> damageAction)
    {
        _damageActions.Add(damageAction);
        _animator.SetBool(AnimatorIsAttacking, true);
    }

    public void UnSubscribeDamage(Action<int> damageAction)
    {
        _damageActions.Remove(damageAction);

        if (_damageActions.Count < 1)
            _animator.SetBool(AnimatorIsAttacking, false);
    }

    public void OnDealDamage(int attack)
    {
        foreach (var damageAction in _damageActions)
        {
            damageAction(hitDamage);
        }
    }
}