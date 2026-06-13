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

    [Header("Способность: Удар об землю")]
    [SerializeField] private GameObject stonePrefab;        // Префаб камня
    [SerializeField] private Transform stoneSpawnPoint; // новая точка спавна
    [SerializeField] private float downDashSpeed = 22f;      // Скорость падения вниз
    [SerializeField] private float stoneLaunchForce = 20f;   // Сила, с которой меч пинает камень

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
    private PlayerHealth _playerHealth; 
    private PlayerAbilities _abilities;
    
    private bool _doubleJumpUsed;
    private float _coyoteTimeCounter;
    private float _attackCooldownCounter;
    private float _dashTimer; 
    private float _dashAnticipationTimer; 
    private bool _isGrounded;
    private bool _facingRight = true;
    private Vector2 _moveInput;

    private bool _isDownDashing;
    private float _currentDashDirection; 

    private readonly RaycastHit2D[] _dashObstacleHits = new RaycastHit2D[1];

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _input = new InputSystem_Actions();
        _playerHealth = GetComponent<PlayerHealth>(); 
        _abilities = GetComponent<PlayerAbilities>();
    }

    private void Start()
    {
        CheckGround(); 
        if (_playerHealth != null) _playerHealth.onDamaged.AddListener(OnHurt);
    }

    private void OnDestroy()
    {
        if (_playerHealth != null) _playerHealth.onDamaged.RemoveListener(OnHurt);
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
                _rb.linearVelocity = new Vector2(_currentDashDirection * attackDashForce, _rb.linearVelocity.y);
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
        bool wasGrounded = _isGrounded;
        _isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        _animator.SetBool("IsGrounded", _isGrounded);

        // // ДОБАВЛЕНО: Передаем вертикальную скорость в аниматор (нужно для выхода из дэша в падение)
        // _animator.SetFloat("VerticalVelocity", _rb.linearVelocity.y);

        if (_isGrounded && !wasGrounded)
        {
            _doubleJumpUsed = false;
            playerSounds?.PlayLand();
            

            if (_isDownDashing)
            {
                _isDownDashing = false;
                SpawnPoundStone();
            }
        }
    }

    private void HandleMovement()
    {
        if (_isDownDashing)
        {
            _rb.linearVelocity = new Vector2(0f, -downDashSpeed);
            return;
        }

        if (_dashAnticipationTimer > 0f)
        {
            _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
            return;
        }

        if (_dashTimer > 0f && _currentDashDirection != 0f)
        {
            Vector2 directionVec = new Vector2(_currentDashDirection, 0f);
            int hitCount = _rb.Cast(directionVec, _dashObstacleHits, 0.15f);

            if (hitCount > 0 && ((1 << _dashObstacleHits[0].collider.gameObject.layer) & enemyLayer) != 0)
            {
                _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
                _dashTimer = 0f; 
                return;
            }

            _rb.linearVelocity = new Vector2(_currentDashDirection * attackDashForce, _rb.linearVelocity.y);
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
        if (_isDownDashing) return;

        if (_coyoteTimeCounter > 0f)
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpForce);
            _coyoteTimeCounter = 0f;
            _doubleJumpUsed = false;
            _animator.SetTrigger("Jump");
            playerSounds?.PlayJump(); 
            return;
        }

        // Двойной прыжок (Крылья сокола)
        if (_abilities != null && _abilities.HasWings && !_doubleJumpUsed)
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, doubleJumpForce);
            _doubleJumpUsed = true;
            
            _animator.SetTrigger("DoubleJump");
            
            playerSounds?.PlayJump(); 
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
        if (_attackCooldownCounter > 0f || _dashTimer > 0f || _dashAnticipationTimer > 0f || _isDownDashing) return;

        _attackCooldownCounter = attackCooldown;
        playerSounds?.PlayAttack();

        // А. УДАР ВНИЗ
        if (!_isGrounded && _moveInput.y < -0.5f)
        {
            _isDownDashing = true;
            _animator.SetTrigger("AttackDown");
            _rb.linearVelocity = new Vector2(0f, -downDashSpeed);
            return;
        }

        // Б. УДАР ВВЕРХ
        if (_moveInput.y > 0.5f)
        {
            _animator.SetTrigger("AttackUp");
            _rb.linearVelocity = new Vector2(0f, jumpForce * 0.8f);

            Collider2D[] hitsUp = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius);
            foreach (var hit in hitsUp)
            {
                if (((1 << hit.gameObject.layer) & enemyLayer) != 0)
                    hit.GetComponent<IDamageable>()?.TakeDamage(attackDamage);
            }
            return; // ← этот return был, но убедимся что он есть
        }

        // В. ГОРИЗОНТАЛЬНЫЙ ДЭШ
        if (Mathf.Abs(_moveInput.x) > 0.1f)
        {
            _currentDashDirection = _moveInput.x > 0f ? 1f : -1f;

            if (_currentDashDirection > 0f && !_facingRight) DirectFlip();
            else if (_currentDashDirection < 0f && _facingRight) DirectFlip();

            _dashAnticipationTimer = attackDashDelay;
            _animator.SetTrigger("Attack");
            return; // ← не было return! код падал в Г
        }

        // Г. ОБЫЧНЫЙ УДАР (только если нет направления)
        _currentDashDirection = 0f;
        _animator.SetTrigger("Attack");
        _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius);
        foreach (var hit in hits)
        {
            if (((1 << hit.gameObject.layer) & enemyLayer) != 0)
                hit.GetComponent<IDamageable>()?.TakeDamage(attackDamage);

            PoundStone stone = hit.GetComponent<PoundStone>();
            if (stone != null)
            {
                Vector2 launchDir = _facingRight ? Vector2.right : Vector2.left;
                stone.Launch(launchDir, stoneLaunchForce);
            }
        }
    }
    private void OnHurt()
    {
        _animator.SetTrigger("Hurt");
        playerSounds?.PlayHurt();
        _dashTimer = 0f;
        _dashAnticipationTimer = 0f;
        _isDownDashing = false;

        
    }

    private void Flip()
    {
        if (_dashTimer > 0f || _dashAnticipationTimer > 0f || _isDownDashing) return;
        DirectFlip();
    }

    private void DirectFlip()
    {
        _facingRight = !_facingRight;
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
    }

    private void SpawnPoundStone()
    {
        if (stonePrefab != null && groundCheck != null)
        {
            Instantiate(stonePrefab, stoneSpawnPoint.position, Quaternion.identity);
        }
    }

    // ВОЗВРАЩЕНО И УЛУЧШЕНО: Метод экстренной остановки для менеджера респауна
    public void StopAllMovement()
    {
        _rb.linearVelocity = Vector2.zero;
        _dashTimer = 0f;
        _dashAnticipationTimer = 0f;
        _isDownDashing = false; // Страховка от бесконечного падения после респауна

        
        _moveInput = Vector2.zero;
        _currentDashDirection = 0f;
        
        if (_input != null)
        {
            _input.Player.Disable();
        }
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