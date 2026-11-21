using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Maneja el viaje Nivel 1 <-> Laberinto. No hay que ponerlo en ninguna escena: se crea solo cuando
// un PortalLaberinto lo necesita. Carga Laberinto de forma ADITIVA (Level1 queda apagado, no se descarga)
// y al volver el jugador reaparece donde estaba.
public class LaberintoTransition : MonoBehaviour
{
    private const string EscenaLaberinto = "Laberinto";

    private static LaberintoTransition instance;

    public static LaberintoTransition Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("LaberintoTransition");
                instance = go.AddComponent<LaberintoTransition>();
            }
            return instance;
        }
    }

    public static bool Ocupado => (instance != null && instance.enTransicion) || ShopTransition.HayTiendaAbierta;

    [SerializeField] private float duracionFade = 0.5f;

    private bool enTransicion;
    private bool enLaberinto;
    private CanvasGroup fade;
    private Scene escenaOrigen;
    private Transform jugador;
    private Vector3 posicionVuelta;
    private Quaternion rotacionVuelta;
    private readonly List<GameObject> objetosApagados = new List<GameObject>();

    // ---------- Viaje ----------

    // Lo llama el GameManager al reiniciar: limpia el estado para que no quede "dentro del laberinto"
    public static void ReiniciarEstado()
    {
        if (instance == null) return;
        instance.StopAllCoroutines();
        instance.enTransicion = false;
        instance.enLaberinto = false;
        instance.objetosApagados.Clear();
        if (instance.fade != null) { instance.fade.alpha = 0f; instance.fade.blocksRaycasts = false; }
    }

    public void IrAlLaberinto()
    {
        if (enTransicion || enLaberinto || ShopTransition.HayTiendaAbierta) return;
        StartCoroutine(RutinaEntrar());
    }

    public void VolverAlNivel()
    {
        if (enTransicion || !enLaberinto) return;
        StartCoroutine(RutinaSalir());
    }

    private void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject);
        CrearFade();
    }

    private IEnumerator RutinaEntrar()
    {
        enTransicion = true;
        fade.blocksRaycasts = true;
        yield return Fade(0f, 1f);

        escenaOrigen = SceneManager.GetActiveScene();
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        jugador = playerGO != null ? playerGO.transform : null;
        if (jugador != null)
        {
            posicionVuelta = jugador.position;
            rotacionVuelta = jugador.rotation;
        }

        ApagarNivel();

        AsyncOperation carga = SceneManager.LoadSceneAsync(EscenaLaberinto, LoadSceneMode.Additive);
        if (carga == null)
        {
            Debug.LogError("[Laberinto] No se pudo cargar 'Laberinto'. ¿Está en File > Build Profiles > Scene List?");
            PrenderNivel();
            yield return Fade(1f, 0f);
            fade.blocksRaycasts = false;
            enTransicion = false;
            yield break;
        }
        while (!carga.isDone) yield return null;

        Scene laberinto = SceneManager.GetSceneByName(EscenaLaberinto);
        SceneManager.SetActiveScene(laberinto);
        ApagarCamarasDe(laberinto);
        yield return null; // que se registren los colliders del laberinto

        // El jugador aparece al lado del portal de vuelta que pusiste en la escena Laberinto
        PortalLaberinto vuelta = BuscarPortal(laberinto, PortalLaberinto.Destino.VolverAlNivel);
        Vector3 spawn;
        if (vuelta != null)
        {
            if (!BuscarPuntoLibre(vuelta.transform.position, 2f, 8f, jugador != null ? jugador.root : null, out spawn))
                spawn = vuelta.transform.position + Vector3.up * 1f;
        }
        else
        {
            Debug.LogWarning("[Laberinto] La escena Laberinto no tiene un PortalLaberinto con Destino = VolverAlNivel.");
            spawn = new Vector3(0f, 2f, 0f);
        }
        Teletransportar(spawn, Quaternion.identity);

        enLaberinto = true;
        yield return Fade(1f, 0f);
        fade.blocksRaycasts = false;
        enTransicion = false;
    }

    private IEnumerator RutinaSalir()
    {
        enTransicion = true;
        fade.blocksRaycasts = true;
        yield return Fade(0f, 1f);

        PrenderNivel();
        Teletransportar(posicionVuelta, rotacionVuelta);

        AsyncOperation descarga = SceneManager.UnloadSceneAsync(EscenaLaberinto);
        while (descarga != null && !descarga.isDone) yield return null;

        enLaberinto = false;
        yield return Fade(1f, 0f);
        fade.blocksRaycasts = false;
        enTransicion = false;
    }

    // ---------- Nivel apagado / prendido ----------

    private void ApagarNivel()
    {
        objetosApagados.Clear();
        Transform raizCamara = Camera.main != null ? Camera.main.transform.root : null;
        Transform raizJugador = jugador != null ? jugador.root : null;

        foreach (GameObject go in escenaOrigen.GetRootGameObjects())
        {
            if (!go.activeSelf) continue;
            if (go.transform == raizJugador || go.transform == raizCamara) continue;

            // Se apaga lo físico (edificios, terreno, enemigos, luces). Managers y HUD quedan prendidos.
            bool esFisico =
                go.GetComponentInChildren<Renderer>(true) != null ||
                go.GetComponentInChildren<Collider>(true) != null ||
                go.GetComponentInChildren<Light>(true) != null;
            if (!esFisico) continue;

            go.SetActive(false);
            objetosApagados.Add(go);
        }
    }

    private void PrenderNivel()
    {
        if (escenaOrigen.IsValid() && escenaOrigen.isLoaded) SceneManager.SetActiveScene(escenaOrigen);
        foreach (GameObject go in objetosApagados) if (go != null) go.SetActive(true);
        objetosApagados.Clear();
    }

    private void ApagarCamarasDe(Scene escena)
    {
        if (Camera.main == null) return;
        foreach (GameObject root in escena.GetRootGameObjects())
        {
            foreach (Camera cam in root.GetComponentsInChildren<Camera>(true)) cam.gameObject.SetActive(false);
            foreach (AudioListener al in root.GetComponentsInChildren<AudioListener>(true)) al.enabled = false;
            foreach (EventSystem es in root.GetComponentsInChildren<EventSystem>(true)) es.gameObject.SetActive(false);
        }
    }

    private void Teletransportar(Vector3 posicion, Quaternion rotacion)
    {
        if (jugador == null) return;
        Rigidbody rb = jugador.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = posicion;
        }
        CharacterController cc = jugador.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        jugador.SetPositionAndRotation(posicion, rotacion);
        if (cc != null) cc.enabled = true;
        Physics.SyncTransforms();
    }

    // ---------- Búsqueda de lugar libre ----------

    // Busca (en anillos desde 'centro') un punto con piso y sin paredes encima. radioMin/radioMax en metros.
    private static bool BuscarPuntoLibre(Vector3 centro, float radioMin, float radioMax, Transform ignorar, out Vector3 resultado)
    {
        resultado = centro;
        float paso = 1.5f;
        for (float r = radioMin; r <= radioMax; r += paso)
        {
            int pasos = r < 0.1f ? 1 : Mathf.Max(8, Mathf.CeilToInt(2f * Mathf.PI * r / paso));
            for (int i = 0; i < pasos; i++)
            {
                float ang = i * Mathf.PI * 2f / pasos;
                Vector3 desde = centro + new Vector3(Mathf.Cos(ang) * r, 200f, Mathf.Sin(ang) * r);

                RaycastHit[] hits = Physics.RaycastAll(desde, Vector3.down, 400f, ~0, QueryTriggerInteraction.Ignore);
                // el piso más bajo con la cara hacia arriba
                float mejorY = float.MaxValue;
                bool ok = false;
                foreach (RaycastHit h in hits)
                {
                    if (ignorar != null && h.collider.transform.IsChildOf(ignorar)) continue;
                    if (h.normal.y < 0.7f) continue;
                    if (h.point.y < mejorY) { mejorY = h.point.y; ok = true; }
                }
                if (!ok) continue;

                Vector3 p = new Vector3(desde.x, mejorY, desde.z);
                bool bloqueado = Physics.CheckCapsule(p + Vector3.up * 0.4f, p + Vector3.up * 1.8f, 0.5f, ~0, QueryTriggerInteraction.Ignore);
                if (bloqueado) continue;

                resultado = p + Vector3.up * 0.1f;
                return true;
            }
        }
        return false;
    }

    private static PortalLaberinto BuscarPortal(Scene escena, PortalLaberinto.Destino destino)
    {
        foreach (GameObject root in escena.GetRootGameObjects())
            foreach (PortalLaberinto p in root.GetComponentsInChildren<PortalLaberinto>(true))
                if (p.DestinoDelPortal == destino) return p;
        return null;
    }

    // ---------- Fundido a negro ----------

    private IEnumerator Fade(float desde, float hasta)
    {
        float t = 0f;
        while (t < duracionFade)
        {
            t += Time.unscaledDeltaTime;
            fade.alpha = Mathf.Lerp(desde, hasta, t / duracionFade);
            yield return null;
        }
        fade.alpha = hasta;
    }

    private void CrearFade()
    {
        GameObject canvasGO = new GameObject("FadeCanvas", typeof(Canvas), typeof(CanvasGroup));
        canvasGO.transform.SetParent(transform, false);
        Canvas canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        fade = canvasGO.GetComponent<CanvasGroup>();
        fade.alpha = 0f;
        fade.blocksRaycasts = false;
        fade.interactable = false;

        GameObject negro = new GameObject("Negro", typeof(RectTransform), typeof(Image));
        negro.transform.SetParent(canvasGO.transform, false);
        RectTransform rt = negro.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        negro.GetComponent<Image>().color = Color.black;
    }
}
