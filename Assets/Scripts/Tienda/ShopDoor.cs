using System.Collections;
using UnityEngine;

// Puerta que brilla. Sirve para ENTRAR a la tienda (en Level1)
// o para SALIR de ella (en la escena Tienda).
// - Detecta al jugador con Physics.CheckSphere filtrado por LayerMask.
// - Brilla con un pulso (corrutina) que se intensifica cuando el jugador está cerca.
// - Con la tecla E entra o sale.
public class ShopDoor : MonoBehaviour
{
    public enum Accion { EntrarATienda, SalirDeTienda }

    [Header("Qué hace esta puerta")]
    [SerializeField] private Accion accion = Accion.EntrarATienda;
    [SerializeField] private string escenaTienda = "Tienda";

    [Header("Detección (CheckSphere + LayerMask)")]
    [SerializeField] private float radioDeteccion = 3f;
    [SerializeField] private LayerMask capaJugador;
    [SerializeField] private KeyCode teclaInteraccion = KeyCode.E;

    [Header("Brillo")]
    [SerializeField] private Renderer[] renderersBrillo;
    [SerializeField] private Light luz;
    [SerializeField] private Color colorBrillo = new Color(1f, 0.7f, 0.15f);
    [SerializeField] private float intensidadMin = 1f;
    [SerializeField] private float intensidadMax = 3f;
    [SerializeField] private float velocidadPulso = 2.5f;
    [Tooltip("Cuánto más brilla cuando el jugador está cerca")]
    [SerializeField] private float multiplicadorCerca = 2f;

    [Header("Cartel 'Presioná E' (opcional)")]
    [SerializeField] private GameObject cartel;

    private bool jugadorCerca;
    private float intensidadLuzBase = 1f;
    private MaterialPropertyBlock bloque;
    private static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");

    private void Awake()
    {
        bloque = new MaterialPropertyBlock();
        if (luz != null) intensidadLuzBase = luz.intensity;
        if (cartel != null) cartel.SetActive(false);
    }

    // OnEnable (no Start) para que el brillo vuelva a arrancar al salir de la tienda
    private void OnEnable()
    {
        StartCoroutine(PulsoBrillo());
    }

    private void Update()
    {
        bool estabaCerca = jugadorCerca;
        jugadorCerca = Physics.CheckSphere(transform.position, radioDeteccion, capaJugador, QueryTriggerInteraction.Collide)
                       || CercaDelColliderDeLaPuerta();

        // Mensaje solo UNA vez, cuando el jugador se acerca a la puerta
        if (jugadorCerca && !estabaCerca)
        {
            string texto = accion == Accion.EntrarATienda ? "entrar a la TIENDA" : "salir de la tienda";
            Debug.Log($"<color=white>Presiona <color=purple>{teclaInteraccion}</color> para {texto}</color>");
        }

        if (cartel != null)
        {
            cartel.SetActive(jugadorCerca);
            if (jugadorCerca && Camera.main != null)
                cartel.transform.rotation = Quaternion.LookRotation(cartel.transform.position - Camera.main.transform.position);
        }

        // Ayuda para depurar: E apretada lejos de la puerta o sin tienda abierta
        if (Input.GetKeyDown(teclaInteraccion) && !jugadorCerca && accion == Accion.SalirDeTienda)
        {
            Transform p = BuscarJugador();
            float d = p != null ? Vector3.Distance(p.position, transform.position) : -1f;
            Debug.Log($"[ShopDoor] Apretaste {teclaInteraccion} pero estás lejos de la puerta de salida (distancia {d:0.0}, radio {radioDeteccion}). Acercate o subí el Radio Deteccion.");
        }

        if (jugadorCerca && Input.GetKeyDown(teclaInteraccion) && !EstaJugando())
            Debug.LogWarning($"[ShopDoor] El juego no está en estado Jugando ({GameManager.Instance.estadoActual}), por eso no hace nada.");

        if (jugadorCerca && EstaJugando() && Input.GetKeyDown(teclaInteraccion))
        {
            if (accion == Accion.EntrarATienda)
                ShopTransition.Instance.EntrarATienda(escenaTienda);
            else if (ShopTransition.HayTiendaAbierta)
                ShopTransition.Instance.SalirDeTienda();
            else
                Debug.LogWarning("[ShopDoor] No hay tienda abierta: entrá desde la puerta de Level1.");
        }
    }

    private Transform jugadorTr;
    private Transform BuscarJugador()
    {
        if (jugadorTr == null)
        {
            GameObject go = GameObject.FindGameObjectWithTag("Player");
            if (go != null) jugadorTr = go.transform;
        }
        return jugadorTr;
    }

    // La puerta es grande: además del radio desde su centro, cuenta estar cerca de cualquier parte de su collider
    private bool CercaDelColliderDeLaPuerta()
    {
        Collider col = GetComponent<Collider>();
        Transform p = BuscarJugador();
        if (col == null || p == null) return false;
        Vector3 punto = col.ClosestPoint(p.position);
        return Vector3.Distance(punto, p.position) <= radioDeteccion;
    }

    // Cartel en pantalla "Presioná E..." (sin armar nada en el editor)
    private GUIStyle estiloCartel;
    private void OnGUI()
    {
        if (!jugadorCerca || !EstaJugando()) return;
        if (ShopTransition.Instance.EnTransicion) return;
        if (accion == Accion.SalirDeTienda && !ShopTransition.HayTiendaAbierta) return;

        if (estiloCartel == null)
        {
            estiloCartel = new GUIStyle(GUI.skin.box) { fontSize = 26, alignment = TextAnchor.MiddleCenter };
            estiloCartel.normal.textColor = Color.white;
        }
        string texto = accion == Accion.EntrarATienda
            ? $"Presioná {teclaInteraccion} para entrar a la TIENDA"
            : $"Presioná {teclaInteraccion} para salir de la tienda";
        float w = 520f, h = 60f;
        GUI.Box(new Rect((Screen.width - w) * 0.5f, Screen.height - h - 80f, w, h), texto, estiloCartel);
    }

    private bool EstaJugando()
    {
        return GameManager.Instance == null || GameManager.Instance.estadoActual == EstadoJuego.Jugando;
    }

    // Corrutina: pulso de brillo infinito mientras la puerta está activa
    private IEnumerator PulsoBrillo()
    {
        while (true)
        {
            float t = (Mathf.Sin(Time.time * velocidadPulso) + 1f) * 0.5f;
            float intensidad = Mathf.Lerp(intensidadMin, intensidadMax, t);
            if (jugadorCerca) intensidad *= multiplicadorCerca;

            foreach (Renderer r in renderersBrillo)
            {
                if (r == null) continue;
                r.GetPropertyBlock(bloque);
                bloque.SetColor(EmissionId, colorBrillo * intensidad);
                r.SetPropertyBlock(bloque);
            }

            if (luz != null)
            {
                luz.color = colorBrillo;
                luz.intensity = intensidadLuzBase * intensidad;
            }

            yield return null;
        }
    }

    // Depuración visual en el editor
    private void OnDrawGizmos()
    {
        Gizmos.color = jugadorCerca ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radioDeteccion);
    }
}
