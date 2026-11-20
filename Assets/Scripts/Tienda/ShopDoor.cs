using UnityEngine;

// Puerta de la tienda. Sirve para ENTRAR a la tienda (en Level1)
// o para SALIR de ella (en la escena Tienda).
// - Detecta al jugador con Physics.CheckSphere filtrado por LayerMask.
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

    [Header("Cartel 'Presioná E' (opcional)")]
    [SerializeField] private GameObject cartel;

    private bool jugadorCerca;

    private void Awake()
    {
        if (capaJugador.value == 0) capaJugador = LayerMask.GetMask("Character");
        DoorTeleport viejo = GetComponent<DoorTeleport>();
        if (viejo != null) viejo.enabled = false;
        if (cartel != null) cartel.SetActive(false);
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

    // Depuración visual en el editor
    private void OnDrawGizmos()
    {
        Gizmos.color = jugadorCerca ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radioDeteccion);
    }
}
