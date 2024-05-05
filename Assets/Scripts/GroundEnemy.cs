using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GroundEnemy : MonoBehaviour
{
    [SerializeField] private int hitDamage;
    [SerializeField] private int health;

    private Rigidbody2D _rigidBody;
    private Collider2D _collider;
    private Collider2D _feetCollider;
    private Animator _animator;
    private Player _target;
    private bool _isFacingLeft;
    private bool _isAttacking;
    private int _floorMask;
    private bool _isDead;
    private int _currentHealth;

    private static readonly int AnimatorHitTrigger = Animator.StringToHash("Hit");
    private static readonly int AnimatorIsAttacking = Animator.StringToHash("IsAttacking");
    private static readonly int AnimatorIsWalking = Animator.StringToHash("IsWalking");
    private static readonly int AnimatorDieTrigger = Animator.StringToHash("Die");
    private static readonly Vector3 HorizontalRotation = new(0, 180, 0);
    private readonly HashSet<Action<int>> _damageActions = new();

    private bool IsAtCliff => !_feetCollider.IsTouchingLayers(_floorMask);
    private bool IsPatrolling => _target is null;

    public void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
        _feetCollider = GetComponentsInChildren<Collider2D>().First(c => c.CompareTag("Feet"));
        _animator = GetComponent<Animator>();
        _floorMask = LayerMask.GetMask("Floor");
    }

    public void Start()
    {
        _rigidBody.velocity = Vector3.right;
        _currentHealth = health;
    }

    public void Update()
    {
        HandleTargeting();
    }

    private void HandleTargeting()
    {
        if (_isAttacking || _isDead)
            return;

        if (IsPatrolling)
        {
            if (IsAtCliff)
            {
                transform.Rotate(HorizontalRotation);
                _isFacingLeft = !_isFacingLeft;
            }

            _rigidBody.velocity = _isFacingLeft ? Vector3.left : Vector3.right;
            return;
        }

        var targetPosition = _target.transform.position;
        Vector2 targetVelocity;

        if (targetPosition.x < transform.position.x)
        {
            targetVelocity = Vector3.left;

            if (!_isFacingLeft)
            {
                transform.Rotate(HorizontalRotation);
                _isFacingLeft = true;
            }
        }
        else
        {
            targetVelocity = Vector3.right;

            if (_isFacingLeft)
            {
                transform.Rotate(HorizontalRotation);
                _isFacingLeft = false;
            }
        }

        if (IsAtCliff)
        {
            _rigidBody.velocity = Vector3.zero;
            _animator.SetBool(AnimatorIsWalking, false);
        }
        else
        {
            _rigidBody.velocity = targetVelocity;
            _animator.SetBool(AnimatorIsWalking, true);
        }
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("AttackHitBox") && _collider.IsTouching(other))
        {
            var player = other.GetComponentInParent<Player>();
            player?.SubscribeDamage(OnTakeDamage);
        }
    }

    public void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("AttackHitBox") && !_collider.IsTouching(other))
        {
            var player = other.GetComponentInParent<Player>();
            player?.UnSubscribeDamage(OnTakeDamage);
        }
    }

    private void OnTakeDamage(int amount)
    {
        if (_isDead) return;

        _currentHealth -= amount;

        if (_currentHealth > 0)
        {
            _animator.SetTrigger(AnimatorHitTrigger);
        }
        else
        {
            _animator.SetTrigger(AnimatorDieTrigger);
            _isDead = true;
        }
    }

    public void SubscribeDamage(Action<int> damageAction)
    {
        _damageActions.Add(damageAction);

        if (!_isAttacking)
            OnStartAttacking();
    }

    public void UnSubscribeDamage(Action<int> damageAction)
    {
        _damageActions.Remove(damageAction);

        if (_damageActions.Count < 1 && _isAttacking)
            OnStopAttacking();
    }

    public void OnDealDamage(int attack)
    {
        foreach (var damageAction in _damageActions)
        {
            damageAction(hitDamage);
        }
    }

    private void OnStartAttacking()
    {
        _animator.SetBool(AnimatorIsAttacking, true);
        _animator.SetBool(AnimatorIsWalking, false);
        _rigidBody.velocity = Vector3.zero;
        _isAttacking = true;
    }

    private void OnStopAttacking()
    {
        _animator.SetBool(AnimatorIsAttacking, false);
        _animator.SetBool(AnimatorIsWalking, true);
        _isAttacking = false;
    }

    public void SubscribeAggro(Player target)
    {
        _target ??= target;
    }

    public void UnSubscribeAggro(Player target)
    {
        if (_target == target)
            _target = null;
    }
}