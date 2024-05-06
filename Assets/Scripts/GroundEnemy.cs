using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LavaJump
{
    public class GroundEnemy : MonoBehaviour
    {
        [SerializeField] private int movementSpeed;
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

        private static readonly int AnimatorDieTrigger = Animator.StringToHash("Die");
        private static readonly int AnimatorHitTrigger = Animator.StringToHash("Hit");
        private static readonly int AnimatorIsAttacking = Animator.StringToHash("IsAttacking");
        private static readonly int AnimatorIsWalking = Animator.StringToHash("IsWalking");
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
            _animator.SetBool(AnimatorIsWalking, true);
        }

        public void Update()
        {
            HandleTargeting();
            HandleAnimation();
        }

        private void HandleTargeting()
        {
            if (_isDead)
                return;

            if (_isAttacking)
            {
                _rigidBody.velocity = new Vector2(0, _rigidBody.velocity.y);
                return;
            }

            if (IsPatrolling)
            {
                _rigidBody.velocity = _isFacingLeft
                    ? new Vector2(-movementSpeed, _rigidBody.velocity.y)
                    : new Vector2(movementSpeed, _rigidBody.velocity.y);
                return;
            }

            var targetPosition = _target.transform.position;
            Vector2 targetVelocity;

            if (targetPosition.x < transform.position.x)
            {
                targetVelocity = new Vector2(-movementSpeed, _rigidBody.velocity.y);

                if (!_isFacingLeft)
                {
                    transform.Rotate(HorizontalRotation);
                    _isFacingLeft = true;
                }
            }
            else
            {
                targetVelocity = new Vector2(movementSpeed, _rigidBody.velocity.y);

                if (_isFacingLeft)
                {
                    transform.Rotate(HorizontalRotation);
                    _isFacingLeft = false;
                }
            }

            // _rigidBody.velocity = IsAtCliff ? new Vector2(0, _rigidBody.velocity.y) : targetVelocity;
            _rigidBody.velocity = targetVelocity;
        }

        private void HandleAnimation()
        {
            _animator.SetBool(AnimatorIsAttacking, _isAttacking);
            _animator.SetBool(AnimatorIsWalking, Math.Abs(_rigidBody.velocity.x) > float.Epsilon);
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

            if (IsPatrolling && other.CompareTag("Floor") && !_feetCollider.IsTouching(other))
            {
                transform.Rotate(HorizontalRotation);
                _isFacingLeft = !_isFacingLeft;
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
            if (_isDead)
                return;

            _damageActions.Add(damageAction);
            _isAttacking = true;
        }

        public void UnSubscribeDamage(Action<int> damageAction)
        {
            if (_isDead)
                return;

            _damageActions.Remove(damageAction);

            if (_damageActions.Count < 1)
                _isAttacking = false;
        }

        public void OnDealDamage(int attack)
        {
            foreach (var damageAction in _damageActions)
            {
                damageAction(hitDamage);
            }
        }

        public void SubscribeAggro(Player target)
        {
            if (_isDead)
                return;

            _target ??= target;
        }

        public void UnSubscribeAggro(Player target)
        {
            if (_isDead || _target != target)
                return;

            _target = null;
        }
    }
}