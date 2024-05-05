using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private float movementSpeed;
    [SerializeField] private float jumpSpeed;

    private Rigidbody2D _rigidBody;
    private Collider2D _collider;
    private Vector2 _currentInput;
    private int _floorMask;

    private bool CanJump => _collider.IsTouchingLayers(_floorMask);

    public void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
        _floorMask = LayerMask.GetMask("Floor");
    }

    public void Update()
    {
        _rigidBody.velocity.Set(_currentInput.x * movementSpeed, _rigidBody.velocity.y);
    }

    public void OnMove(InputValue value)
    {
        _currentInput = value.Get<Vector2>();
    }

    public void OnJump()
    {
        if (CanJump)
            _rigidBody.velocity.Set(_rigidBody.velocity.x, jumpSpeed);
    }
}