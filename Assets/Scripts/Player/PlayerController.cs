using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [Header("Движение")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float coyoteTime = 0.15f;

    [Header("Проверка земли")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.1f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Атака")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRadius = 0.5f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private float attackCooldown = 0.4f;

    private Rigidbody2D _rb;
    private Animator _animator;
    private InputSystem_Actions _input;

    private float _coyoteTimeCounter;
    private float _attackCooldownCounter;
    private bool _isGrounded;
    private bool _facingRight = true;
    private Vector2 _moveInput;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _input = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        _input.Player.Enable();
        _input.Player.Jump.performed += OnJump;
        _input.Player.Jump.canceled += OnJumpCanceled;
        _input.Player.Attack.performed += OnAttack;
    }

    private void OnDisable()
    {
        _input.Player.Jump.performed -= OnJump;
        _input.Player.Jump.canceled -= OnJumpCanceled;
        _input.Player.Attack.performed -= OnAttack;
        _input.Player.Disable();
    }

    private void Update()
    {
        _moveInput = _input.Player.Move.ReadValue<Vector2>();
        CheckGround();
        HandleCoyoteTime();
        _attackCooldownCounter -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        HandleMovement(); // Теперь бег работает одинаково на любых мониторах![cite: 8]
    }

    private void CheckGround()
    {
        _isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        _animator.SetBool("IsGrounded", _isGrounded);
    }

    private void HandleMovement()
    {
        float input = _moveInput.x;
        _rb.linearVelocity = new Vector2(input * moveSpeed, _rb.linearVelocity.y);

        if (input > 0 && !_facingRight) Flip();
        else if (input < 0 && _facingRight) Flip();

        _animator.SetFloat("Speed", Mathf.Abs(input));
    }

    private void HandleCoyoteTime()
    {
        if (_isGrounded)
            _coyoteTimeCounter = coyoteTime;
        else
            _coyoteTimeCounter -= Time.deltaTime;
    }

    private void OnJump(InputAction.CallbackContext ctx)
    {
        if (_coyoteTimeCounter > 0f)
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpForce);
            _coyoteTimeCounter = 0f;
        }
    }

    private void OnJumpCanceled(InputAction.CallbackContext ctx)
    {
        // Переменная высота прыжка — отпустил раньше, прыгнул ниже
        if (_rb.linearVelocity.y > 0f)
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _rb.linearVelocity.y * 0.5f);
    }

    private void OnAttack(InputAction.CallbackContext ctx)
    {
        if (_attackCooldownCounter > 0f) return;

        _attackCooldownCounter = attackCooldown;
        _animator.SetTrigger("Attack");

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, enemyLayer);
        foreach (var hit in hits)
            hit.GetComponent<IDamageable>()?.TakeDamage(attackDamage);
    }

    private void Flip()
    {
        _facingRight = !_facingRight;
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }
    }
}