using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// Maneja la entrada y salida de la tienda (no hay que ponerlo en ninguna escena, se crea solo).
//
// La escena Tienda se carga de forma ADITIVA: Level1 no se descarga.
// Se apaga lo físico del nivel (edificios, enemigos, terreno) MENOS el jugador y la cámara; el jugador se
// teletransporta al ShopSpawnPoint de la tienda. Al salir vuelve al mismo lugar
// y el nivel sigue igual (enemigos, objetos de misión, etc.).
public class ShopTransition : MonoBehaviour
{
    private static ShopTransition instance;

    public static ShopTransition Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("ShopTransition");
                instance = go.AddComponent<ShopTransition>();
            }
            return instance;
        }
    }

    public static bool HayTiendaAbierta => instance != null && instance.EnTienda;

    // Eventos para quien quiera enterarse (ej. el AudioManager para cambiar la música)
    public static event Action OnEntroATienda;
    public static event Action OnSalioDeTienda;

    public bool EnTransicion { get; private set; }
    public bool EnTienda { get; private set; }

    [SerializeField] private float duracionFade = 0.5f;

    private CanvasGroup fade;
    private Scene escenaOrigen;
    private string escenaTienda;
    private readonly List<GameObject> objetosApagados = new List<GameObject>();

    private Transform jugador;
    private Vector3 posicionVuelta;
    private Quaternion rotacionVuelta;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        CrearFade();
    }

    public void EntrarATienda(string escena)
    {
        if (EnTransicion || EnTienda) return;
        StartCoroutine(RutinaEntrar(escena));
    }

    public void SalirDeTienda()
    {
        if (EnTransicion || !EnTienda) return;
        StartCoroutine(RutinaSalir());
    }

    private IEnumerator RutinaEntrar(string escena)
    {
        EnTransicion = true;
        fade.blocksRaycasts = true;
        yield return Fade(0f, 1f);

        escenaOrigen = SceneManager.GetActiveScene();
        escenaTienda = escena;

        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        jugador = playerGO != null ? playerGO.transform : null;
        if (jugador != null)
        {
            posicionVuelta = jugador.position;
            rotacionVuelta = jugador.rotation;
        }

        // Apagamos el nivel, menos jugador, cámara y HUD
        ApagarNivel();

        AsyncOperation carga = SceneManager.LoadSceneAsync(escena, LoadSceneMode.Additive);
        if (carga == null)
        {
            Debug.LogError($"[ShopTransition] No se pudo cargar '{escena}'. ¿Está agregada en Build Profiles > Scene List?");
            PrenderNivel();
            yield return Fade(1f, 0f);
            fade.blocksRaycasts = false;
            EnTransicion = false;
            yield break;
        }
        while (!carga.isDone) yield return null;

        Scene tienda = SceneManager.GetSceneByName(escena);
        SceneManager.SetActiveScene(tienda); // usa la iluminación de la tienda
        ApagarCamarasDeLaTienda(tienda);

        ShopSpawnPoint spawn = BuscarEnEscena<ShopSpawnPoint>(tienda);
        if (spawn != null) Teletransportar(spawn.transform.position, spawn.transform.rotation);
        else Debug.LogWarning("[ShopTransition] La escena Tienda no tiene un ShopSpawnPoint.");

        EnTienda = true;
        OnEntroATienda?.Invoke();

        yield return Fade(1f, 0f);
        fade.blocksRaycasts = false;
        EnTransicion = false;
    }

    private IEnumerator RutinaSalir()
    {
        EnTransicion = true;
        fade.blocksRaycasts = true;
        yield return Fade(0f, 1f);

        // Primero prendemos el nivel y volvemos a la puerta, después sacamos la tienda
        PrenderNivel();
        Teletransportar(posicionVuelta, rotacionVuelta);

        AsyncOperation descarga = SceneManager.UnloadSceneAsync(escenaTienda);
        while (descarga != null && !descarga.isDone) yield return null;

        EnTienda = false;
        if (GameManager.Instance != null)
            GameManager.Instance.CambiarEstado(EstadoJuego.Jugando);

        OnSalioDeTienda?.Invoke();

        yield return Fade(1f, 0f);
        fade.blocksRaycasts = false;
        EnTransicion = false;
    }

    private void ApagarNivel()
    {
        objetosApagados.Clear();
        Transform raizCamara = Camera.main != null ? Camera.main.transform.root : null;
        Transform raizJugador = jugador != null ? jugador.root : null;

        foreach (GameObject go in escenaOrigen.GetRootGameObjects())
        {
            if (!go.activeSelf) continue;

            // Quedan prendidos el jugador, la cámara y todo lo que no se ve ni choca
            // (managers, HUD, inventario). Se apaga lo físico: edificios, terreno, enemigos, luces...
            bool esJugadorOCamara = go.transform == raizJugador || go.transform == raizCamara;
            bool esFisico =
                go.GetComponentInChildren<Renderer>(true) != null ||
                go.GetComponentInChildren<Collider>(true) != null ||
                go.GetComponentInChildren<Light>(true) != null;

            if (esJugadorOCamara || !esFisico) continue;

            go.SetActive(false);
            objetosApagados.Add(go);
        }
    }

    private void PrenderNivel()
    {
        if (escenaOrigen.IsValid() && escenaOrigen.isLoaded)
            SceneManager.SetActiveScene(escenaOrigen);

        foreach (GameObject go in objetosApagados)
            if (go != null) go.SetActive(true);

        objetosApagados.Clear();
    }

    // La tienda puede tener su propia cámara para editarla; en juego se usa la del jugador
    private void ApagarCamarasDeLaTienda(Scene tienda)
    {
        if (Camera.main == null) return;
        foreach (GameObject root in tienda.GetRootGameObjects())
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
        jugador.SetPositionAndRotation(posicion, rotacion);
        Physics.SyncTransforms();
    }

    private static T BuscarEnEscena<T>(Scene escena) where T : Component
    {
        foreach (GameObject root in escena.GetRootGameObjects())
        {
            T c = root.GetComponentInChildren<T>(true);
            if (c != null) return c;
        }
        return null;
    }

    // Corrutina de fundido a negro (tiempo real por si el juego está pausado)
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

    // Crea por código un Canvas negro para el fundido (no hay que armarlo en el editor)
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
