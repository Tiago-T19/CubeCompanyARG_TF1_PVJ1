using System.Collections;
using UnityEngine;
using TMPro;

// Power-up físico de la tienda (estilo tycoon).
// El objeto gira sobre su mesa como vitrina. NO pasa nada si se lo toca:
// la compra se hace parándose sobre la PLATAFORMA (el cubo negro).
// Al comprar: la plataforma se pone verde un momento, el objeto desaparece
// y la mejora queda guardada en PlayerStats (persiste en los niveles).
// Va en el objeto del power-up (corazon, botiquin, botaSalto...).
public class ShopPowerUp : MonoBehaviour
{
    [Header("Qué se vende")]
    [SerializeField] private ShopItemData item;

    [Header("Plataforma de compra (el cubo negro)")]
    [SerializeField] private Renderer plataforma;
    [Tooltip("Qué tan alto sobre la plataforma se detecta al jugador")]
    [SerializeField] private float alturaDeteccion = 2f;
    [SerializeField] private LayerMask capaJugador;
    [SerializeField] private Color colorCompra = new Color(0.1f, 0.9f, 0.2f);
    [SerializeField] private Color colorError = new Color(0.9f, 0.1f, 0.1f);
    [SerializeField] private float duracionColor = 1f;

    [Header("Visual del objeto")]
    [Tooltip("El objeto que gira. Si lo dejás vacío usa este mismo objeto")]
    [SerializeField] private Transform modelo;
    [SerializeField] private float velocidadGiro = 60f;
    [SerializeField] private float alturaFlotar = 0.1f;
    [SerializeField] private float duracionDesaparecer = 0.4f;

    [Header("Cartel con nombre y precio (TextMeshPro 3D, opcional)")]
    [SerializeField] private TextMeshPro cartel;
    [SerializeField] private Color colorAlcanza = new Color(1f, 0.85f, 0.2f);
    [SerializeField] private Color colorNoAlcanza = new Color(1f, 0.3f, 0.3f);
    [SerializeField] private float duracionAviso = 1.5f;

    private bool jugadorEnPlataforma;
    private bool comprado;
    private Vector3 posModeloInicial;
    private Vector3 escalaModeloInicial;
    private Color colorOriginal = Color.black;
    private MaterialPropertyBlock bloque;
    private Coroutine rutinaAviso;
    private Coroutine rutinaColor;

    private static readonly int IdBaseColor = Shader.PropertyToID("_BaseColor");
    private static readonly int IdColor = Shader.PropertyToID("_Color");

    private void Start()
    {
        // Si no se asignó la capa en el Inspector, usa la capa "Character"
        if (capaJugador.value == 0) capaJugador = LayerMask.GetMask("Character");

        // Si ya se compró el máximo en visitas anteriores, no aparece
        if (item == null || ShopManager.Agotado(item))
        {
            gameObject.SetActive(false);
            return;
        }

        if (modelo == null) modelo = transform;
        posModeloInicial = modelo.position;
        escalaModeloInicial = modelo.localScale;

        bloque = new MaterialPropertyBlock();
        if (plataforma != null)
        {
            Material m = plataforma.sharedMaterial;
            if (m != null)
                colorOriginal = m.HasProperty(IdBaseColor) ? m.GetColor(IdBaseColor) : m.color;
        }

        ActualizarCartel();
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnPuntajeCambiado += AlCambiarPuntos;
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnPuntajeCambiado -= AlCambiarPuntos;
    }

    private void Update()
    {
        if (comprado) return;

        Girar();

        if (cartel != null && Camera.main != null)
            cartel.transform.rotation = Quaternion.LookRotation(cartel.transform.position - Camera.main.transform.position);

        // Solo cuenta la plataforma: tocar el objeto no hace nada
        bool estabaEnPlataforma = jugadorEnPlataforma;
        jugadorEnPlataforma = JugadorSobrePlataforma();

        if (jugadorEnPlataforma && !estabaEnPlataforma) Comprar();
    }

    // Physics.CheckBox + LayerMask: caja sobre la plataforma
    private bool JugadorSobrePlataforma()
    {
        if (plataforma == null) return false;
        Bounds b = plataforma.bounds;
        Vector3 centro = b.center + Vector3.up * (b.extents.y + alturaDeteccion * 0.5f);
        Vector3 mitad = new Vector3(b.extents.x, alturaDeteccion * 0.5f, b.extents.z);
        return Physics.CheckBox(centro, mitad, Quaternion.identity, capaJugador, QueryTriggerInteraction.Collide);
    }

    private void Comprar()
    {
        if (ShopManager.IntentarComprar(item))
        {
            comprado = true;
            StartCoroutine(RutinaCompra());
        }
        else
        {
            ShopManager.PuedeComprar(item, out string motivo);
            MostrarAviso(motivo);
            PintarPlataforma(colorError);
        }
    }

    // El objeto gira y flota suavemente sobre su mesa
    private void Girar()
    {
        if (modelo == null) return;
        modelo.Rotate(Vector3.up, velocidadGiro * Time.deltaTime, Space.World);
        modelo.position = posModeloInicial + Vector3.up * (Mathf.Sin(Time.time * 2f) * alturaFlotar);
    }

    // Corrutina: plataforma en verde, el objeto se achica, y al final se apaga
    private IEnumerator RutinaCompra()
    {
        if (cartel != null) cartel.gameObject.SetActive(false);
        SetColorPlataforma(colorCompra);

        float t = 0f;
        while (t < duracionDesaparecer)
        {
            t += Time.deltaTime;
            modelo.localScale = Vector3.Lerp(escalaModeloInicial, Vector3.zero, t / duracionDesaparecer);
            yield return null;
        }

        // Espera lo que falte para completar el tiempo en verde
        float resto = duracionColor - duracionDesaparecer;
        if (resto > 0f) yield return new WaitForSeconds(resto);

        SetColorPlataforma(colorOriginal);
        gameObject.SetActive(false);
    }

    // Color temporal (error): vuelve al original solo
    private void PintarPlataforma(Color c)
    {
        if (plataforma == null) return;
        if (rutinaColor != null) StopCoroutine(rutinaColor);
        rutinaColor = StartCoroutine(RutinaColor(c));
    }

    private IEnumerator RutinaColor(Color c)
    {
        SetColorPlataforma(c);
        yield return new WaitForSeconds(duracionColor);
        SetColorPlataforma(colorOriginal);
        rutinaColor = null;
    }

    // Cambia solo el color de ESTA plataforma sin tocar el material compartido
    private void SetColorPlataforma(Color c)
    {
        if (plataforma == null) return;
        plataforma.GetPropertyBlock(bloque);
        bloque.SetColor(IdBaseColor, c);
        bloque.SetColor(IdColor, c);
        plataforma.SetPropertyBlock(bloque);
    }

    private void AlCambiarPuntos(int puntos)
    {
        if (!comprado) ActualizarCartel();
    }

    private void ActualizarCartel()
    {
        if (cartel == null || item == null) return;
        cartel.text = $"{item.nombre}\n{item.precio} pts";
        cartel.color = ShopManager.AlcanzaElSaldo(item) ? colorAlcanza : colorNoAlcanza;
    }

    private void MostrarAviso(string texto)
    {
        if (cartel == null) return;
        if (rutinaAviso != null) StopCoroutine(rutinaAviso);
        rutinaAviso = StartCoroutine(RutinaAviso(texto));
    }

    // Corrutina: muestra el motivo un rato y vuelve al cartel normal
    private IEnumerator RutinaAviso(string texto)
    {
        cartel.text = texto;
        cartel.color = colorNoAlcanza;
        yield return new WaitForSeconds(duracionAviso);
        ActualizarCartel();
    }

    private void OnDrawGizmosSelected()
    {
        if (plataforma == null) return;
        Bounds b = plataforma.bounds;
        Gizmos.color = jugadorEnPlataforma ? Color.green : Color.cyan;
        Gizmos.DrawWireCube(b.center + Vector3.up * (b.extents.y + alturaDeteccion * 0.5f),
                            new Vector3(b.size.x, alturaDeteccion, b.size.z));
    }
}
