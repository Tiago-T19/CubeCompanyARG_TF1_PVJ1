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

    private float moveHorizontal;
    private float moveVertical;
    private Rigidbody rb;
    private Animator animator;

    // --- Ralentizacion ---
    private bool estaRalentizado = false;
    private float velocidadBaseGuardada;
    private Coroutine corutinaRalentizacion;

    [Header("Raycast Settings")]
    [SerializeField] private bool isGrounded;
    [SerializeField] private float rayDistance = 2f;
    [SerializeField] private LayerMask groundLayer;

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
            UpdateAnimator();
            return;
        }

        if (isStop)
        {
            UpdateAnimator();
            return;
        }

        moveHorizontal = Input.GetAxis("Horizontal");
        moveVertical = Input.GetAxis("Vertical");
        CheckGround();

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.AddForce(new Vector3(0, jumpForce, 0), ForceMode.Impulse);
        }

        UpdateAnimator();
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

    private void UpdateAnimator()
    {
        if (animator != null)
        {
            float currentSpeed = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude;
            animator.SetFloat("Speed", currentSpeed);
        }
    }

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