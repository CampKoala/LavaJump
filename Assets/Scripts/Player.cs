using LavaJump;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private float movementSpeed;
    [SerializeField] private float jumpSpeed;
    [SerializeField] [Min(1)] private int hitDamage;
    [SerializeField] private int health;
    [SerializeField] private HealthBar healthBar;

    private Rigidbody2D _rigidBody;
    private Collider2D _collider;
    private Collider2D _feetCollider;
    private Animator _animator;
    private Vector2 _currentInput;
    private int _floorMask;
    private bool _isJumping;
    private bool _isFacingLeft;
    private bool _isAttacking;
    private bool _isDead;
    private int _currentHealth;
    private readonly HashSet<Action<int>> _damageActions = new();

    private static readonly Vector3 HorizontalRotation = new(0, 180, 0);
    private static readonly int AnimatorIsRunning = Animator.StringToHash("IsRunning");
    private static readonly int AnimatorIsGrounded = Animator.StringToHash("IsGrounded");
    private static readonly int AnimatorIsAttacking = Animator.StringToHash("IsAttacking");
    private static readonly int AnimatorJumpTrigger = Animator.StringToHash("Jump");
    private static readonly int AnimatorHitTrigger = Animator.StringToHash("Hit");
    private static readonly int AnimatorDieTrigger = Animator.StringToHash("Die");

    private bool IsGrounded => _feetCollider.IsTouchingLayers(_floorMask);

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
        _currentHealth = health;
        healthBar.SetMaxHealth(health);
    }

    public void Update()
    {
        HandleMovement();
        HandleJumping();
    }

    private void HandleMovement()
    {
        if (_isDead) return;

        _rigidBody.velocity = new Vector2(_currentInput.x * movementSpeed * (_isAttacking && IsGrounded ? 0.0f : 1.0f),
            _rigidBody.velocity.y);
        _animator.SetBool(AnimatorIsRunning, Math.Abs(_currentInput.x) > float.Epsilon);

        if (!_isFacingLeft && _rigidBody.velocity.x < -float.Epsilon)
        {
            transform.Rotate(HorizontalRotation);
            _isFacingLeft = true;
        }

        if (_isFacingLeft && _rigidBody.velocity.x > float.Epsilon)
        {
            transform.Rotate(HorizontalRotation);
            _isFacingLeft = false;
        }
    }

    private void HandleJumping()
    {
        _animator.SetBool(AnimatorIsGrounded, IsGrounded);

        if (_isJumping && IsGrounded)
        {
            _isJumping = false;
        }
    }

    public void OnAttack(InputValue value)
    {
        _isAttacking = value.isPressed;
        _animator.SetBool(AnimatorIsAttacking, _isAttacking);
    }

    public void OnMove(InputValue value)
    {
        _currentInput = value.Get<Vector2>();
    }

    public void OnJump()
    {
        if (!IsGrounded || _isDead) return;

        _rigidBody.velocity = new Vector2(_rigidBody.velocity.x, jumpSpeed);
        _animator.SetTrigger(AnimatorJumpTrigger);
        _isJumping = true;
    }

    public void OnDealDamage(int attack)
    {
        foreach (var damageAction in _damageActions)
        {
            damageAction(hitDamage);
        }
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("AttackHitBox") && _collider.IsTouching(other))
        {
            var enemy = other.GetComponentInParent<GroundEnemy>();
            enemy?.SubscribeDamage(OnTakeDamage);
        }

        if (other.CompareTag("Aggro") && _collider.IsTouching(other))
        {
            var enemy = other.GetComponentInParent<GroundEnemy>();
            enemy?.SubscribeAggro(this);
        }
    }

    public void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("AttackHitBox") && !_collider.IsTouching(other))
        {
            var enemy = other.GetComponentInParent<GroundEnemy>();
            enemy?.UnSubscribeDamage(OnTakeDamage);
        }

        if (other.CompareTag("Aggro") && !_collider.IsTouching(other))
        {
            var enemy = other.GetComponentInParent<GroundEnemy>();
            enemy?.UnSubscribeAggro(this);
        }
    }

    private void OnTakeDamage(int amount)
    {
        if (_isDead) return;

        _currentHealth -= amount;

        healthBar.SetHealth(_currentHealth);

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
    }

    public void UnSubscribeDamage(Action<int> damageAction)
    {
        _damageActions.Remove(damageAction);
    }
}