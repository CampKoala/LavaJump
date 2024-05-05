using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private float movementSpeed;
    [SerializeField] private float jumpSpeed;

    private Rigidbody2D _rb;
    private Collider2D _collider;
    private Vector2 _currentInput;
    private int _floorMask;

    private bool CanJump => _collider.IsTouchingLayers(_floorMask);

    public void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
        _floorMask = LayerMask.GetMask("Floor");
    }

    public void Update()
    {
        _rb.velocity = new Vector2(_currentInput.x * movementSpeed, _rb.velocity.y);
    }

    public void OnMove(InputValue value)
    {
        _currentInput = value.Get<Vector2>();
    }

    public void OnJump()
    {
        if (CanJump)
            _rb.velocity = new Vector2(_rb.velocity.x, jumpSpeed);
    }
}