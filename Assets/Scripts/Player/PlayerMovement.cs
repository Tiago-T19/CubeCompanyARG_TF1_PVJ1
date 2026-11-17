using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 5f;
    public float Speed
    {
        get { return speed; }
        set { speed = value; }
    }

    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private bool isStop = false;
    public bool IsStop
    {
        get { return isStop; }
        set { isStop = value; }
    }

    [Header("Pasos en asfalto")]
    [SerializeField] private float footstepInterval = 0.4f;
    private float footstepTimer = 0f;

    private float moveHorizontal;
    private float moveVertical;
    private Rigidbody rb;
    private Animator animator;

    private bool estaRalentizado = false;
    private float velocidadBaseGuardada;
    private Coroutine corutinaRalentizacion;

    [Header("Raycast Settings")]
    [SerializeField] private bool isGrounded;
    [SerializeField] private float rayDistance = 2f;
    [SerializeField] private LayerMask groundLayer;

    // Stats del jugador (ScriptableObject). Si no hay, usa los valores del Inspector.
    private PlayerStats Stats => GameManager.Instance != null ? GameManager.Instance.Stats : null;
    private float JumpReal => Stats != null ? Stats.fuerzaSalto : jumpForce;
    private float velocidadInspector;

    void Awake()
    {
        velocidadInspector = speed;
    }

    // Toma la velocidad del PlayerStats y escucha si se compra una mejora en la tienda
    void OnEnable()
    {
        RestaurarVelocidad();
        if (Stats != null) Stats.OnStatsChanged += RestaurarVelocidad;
    }

    void OnDisable()
    {
        if (Stats != null) Stats.OnStatsChanged -= RestaurarVelocidad;
    }

    // Vuelve a la velocidad normal (la usa EnemigoEspecial después de ralentizar)
    public void RestaurarVelocidad()
    {
        speed = Stats != null ? Stats.velocidad : velocidadInspector;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        rb.freezeRotation = true;
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.estadoActual != EstadoJuego.Jugando)
        {
            moveHorizontal = 0;
            moveVertical = 0;
            UpdateAnimator(0);
            return;
        }

        if (isStop)
        {
            UpdateAnimator(0);
            return;
        }

        moveHorizontal = Input.GetAxis("Horizontal");
        moveVertical = Input.GetAxis("Vertical");
        CheckGround();

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.AddForce(new Vector3(0, JumpReal, 0), ForceMode.Impulse);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayJump();
        }

        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        float currentSpeed = horizontalVelocity.magnitude;

        bool estaMoviendose = Mathf.Abs(moveHorizontal) > 0.1f || Mathf.Abs(moveVertical) > 0.1f;
        if (isGrounded && estaMoviendose && currentSpeed > 0.5f)
        {
            footstepTimer -= Time.deltaTime;
            if (footstepTimer <= 0f)
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayFootstep();
                footstepTimer = footstepInterval;
            }
        }
        else
        {
            footstepTimer = 0f;
        }

        UpdateAnimator(currentSpeed);
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance != null && GameManager.Instance.estadoActual != EstadoJuego.Jugando)
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            return;
        }
        if (isStop)
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            return;
        }
        Vector3 movement = (transform.forward * moveVertical + transform.right * moveHorizontal).normalized;
        rb.linearVelocity = new Vector3(movement.x * speed, rb.linearVelocity.y, movement.z * speed);
    }

    private void CheckGround()
    {
        Vector3 origin = transform.position;
        isGrounded = Physics.Raycast(origin, Vector3.down, rayDistance, groundLayer);
        Color colorRay = isGrounded ? Color.green : Color.red;
        Debug.DrawRay(origin, Vector3.down * rayDistance, colorRay);
    }

    public void RotateCharacter(float rotateY)
    {
        transform.rotation = Quaternion.Euler(0f, rotateY, 0f);
    }

    private void UpdateAnimator(float speedValue)
    {
        if (animator != null) animator.SetFloat("Speed", speedValue);
    }

    // Lo usa EnemigoEspecial: no se apila y reinicia el timer si te vuelven a pegar
    public void AplicarRalentizacion(float factor, float duracion)
    {
        if (!estaRalentizado)
        {
            velocidadBaseGuardada = speed;
            speed = velocidadBaseGuardada * factor;
            estaRalentizado = true;
        }
        if (corutinaRalentizacion != null) StopCoroutine(corutinaRalentizacion);
        corutinaRalentizacion = StartCoroutine(RutinaRalentizacion(duracion));
    }

    private IEnumerator RutinaRalentizacion(float duracion)
    {
        yield return new WaitForSeconds(duracion);
        speed = velocidadBaseGuardada;
        estaRalentizado = false;
        corutinaRalentizacion = null;
    }
}
