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

    [Header("Божественный Дэш при атаке")]
    [SerializeField] private float attackDashForce = 18f;    
    [SerializeField] private float attackDashDuration = 0.1f; 
    [SerializeField] private float attackDashDelay = 0.06f;   

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

    [Header("Звуки")]
    [SerializeField] private PlayerSounds playerSounds;


    [Header("Двойной прыжок")]
    [SerializeField] private float doubleJumpForce = 10f;

    private Rigidbody2D _rb;
    private Animator _animator;
    private InputSystem_Actions _input;
    private PlayerHealth _playerHealth; // Ссылка для отслеживания боли

    private PlayerAbilities _abilities;
    private int _jumpsRemaining;
    private bool _doubleJumpUsed;


    private float _coyoteTimeCounter;
    private float _attackCooldownCounter;
    private float _dashTimer; 
    private float _dashAnticipationTimer; 
    private bool _isGrounded;
    private bool _facingRight = true;
    private Vector2 _moveInput;

    private readonly RaycastHit2D[] _dashObstacleHits = new RaycastHit2D[1];

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _input = new InputSystem_Actions();
        _playerHealth = GetComponent<PlayerHealth>(); // Кешируем здоровье
        _abilities = GetComponent<PlayerAbilities>();

    }

    private void Start()
    {
        CheckGround(); 
        
        // Подписываемся на событие получения урона
        if (_playerHealth != null)
        {
            _playerHealth.onDamaged.AddListener(OnHurt);
        }
    }

    private void OnDestroy()
    {
        // Обязательно отписываемся при уничтожении, чтобы избежать утечек памяти
        if (_playerHealth != null)
        {
            _playerHealth.onDamaged.RemoveListener(OnHurt);
        }
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
        
        if (_dashAnticipationTimer > 0f)
        {
            _dashAnticipationTimer -= Time.deltaTime;
            if (_dashAnticipationTimer <= 0f)
            {
                _dashTimer = attackDashDuration;
                float attackDirection = _facingRight ? 1f : -1f;
                _rb.linearVelocity = new Vector2(attackDirection * attackDashForce, _rb.linearVelocity.y);
            }
        }
        else if (_dashTimer > 0f)
        {
            _dashTimer -= Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        HandleMovement(); 
    }

    private void CheckGround()
    {
        // _isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        // _animator.SetBool("IsGrounded", _isGrounded);
        bool wasGrounded = _isGrounded;
        _isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        _animator.SetBool("IsGrounded", _isGrounded);

        if (_isGrounded && !wasGrounded)
        {
            _doubleJumpUsed = false;
            playerSounds?.PlayLand();
        }
    }

    private void HandleMovement()
    {
        if (_dashAnticipationTimer > 0f)
        {
            _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
            return;
        }

        if (_dashTimer > 0f)
        {
            float dashDirection = _facingRight ? 1f : -1f;
            Vector2 directionVec = new Vector2(dashDirection, 0f);

            int hitCount = _rb.Cast(directionVec, _dashObstacleHits, 0.15f);

            if (hitCount > 0 && ((1 << _dashObstacleHits[0].collider.gameObject.layer) & enemyLayer) != 0)
            {
                _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
                _dashTimer = 0f; 
                return;
            }

            _rb.linearVelocity = new Vector2(dashDirection * attackDashForce, _rb.linearVelocity.y);
            return; 
        }

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
        // Обычный прыжок — coyote time
        if (_coyoteTimeCounter > 0f)
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpForce);
            _coyoteTimeCounter = 0f;
            _doubleJumpUsed = false;
            _animator.SetTrigger("Jump");
            playerSounds?.PlayJump(); // звук обычного прыжка
            return;
        }

        // Двойной прыжок — только если есть крылья и ещё не использован
        if (_abilities != null && _abilities.HasWings && !_doubleJumpUsed)
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, doubleJumpForce);
            _doubleJumpUsed = true;
            _animator.SetTrigger("DoubleJump"); // триггер для аниматора
            playerSounds?.PlayJump(); // звук обычного прыжка
            return;
        }
    }

    private void OnJumpCanceled(InputAction.CallbackContext ctx)
    {
        if (_rb.linearVelocity.y > 0f)
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _rb.linearVelocity.y * 0.5f);
    }

    private void OnAttack(InputAction.CallbackContext ctx)
    {
        if (_attackCooldownCounter > 0f || _dashTimer > 0f || _dashAnticipationTimer > 0f) return;

        _attackCooldownCounter = attackCooldown;
        _animator.SetTrigger("Attack");
        playerSounds?.PlayAttack();
        _dashAnticipationTimer = attackDashDelay; 
        _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, enemyLayer);
        foreach (var hit in hits)
            hit.GetComponent<IDamageable>()?.TakeDamage(attackDamage);
    }

    // ЛОГИКА ПОЛУЧЕНИЯ УРОНА ИГРОКОМ
    private void OnHurt()
    {
        // 1. Активируем триггер боли в Аниматоре
        _animator.SetTrigger("Hurt");
        playerSounds?.PlayHurt();

        // 2. СБРОС СОСТОЯНИЙ: если бога ударили в момент замаха или рывка — прерываем атаку,
        // чтобы враги могли "перебить" наше действие. Это делает боевку честной и тактичной.
        _dashTimer = 0f;
        _dashAnticipationTimer = 0f;
    }

    private void Flip()
    {
        if (_dashTimer > 0f || _dashAnticipationTimer > 0f) return;

        _facingRight = !_facingRight;
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
    }

    public void StopAllMovement()
    {
        _rb.linearVelocity = Vector2.zero;
        _dashTimer = 0f;
        _dashAnticipationTimer = 0f;
        _moveInput = Vector2.zero;
        _input.Player.Disable();
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