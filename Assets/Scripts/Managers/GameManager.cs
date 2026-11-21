using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public enum GameState { Start, Playing, Paused, Victory, GameOver }
public enum EstadoJuego { Inicio, Jugando, Pausa, Victoria, Derrota }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Estado")]
    public EstadoJuego estadoActual = EstadoJuego.Inicio;
    public GameState currentState = GameState.Start;

    //Arreglo para que el inventario y el sonido del disparo no choquen
    public bool inventarioAbierto = false;
    public bool resolviendoPuzzle = false;
    [SerializeField] private InventoryController inventoryController;

    public event Action<GameState> OnChangeState;
    public event Action<int> OnPuntajeCambiado;

    [Header("Datos Persistentes")]
    public float vidaActual = 100f;
    public float vidaMax = 100f;
    public int puntajeAcumulado = 0;
    public int itemsRecolectados = 0;
    public bool jefeFinalMuerto = false;

    [Header("Stats del jugador (ScriptableObject)")]
    [Tooltip("Si queda vacío se carga solo desde Resources/PlayerStats")]
    [SerializeField] private PlayerStats playerStats;
    public PlayerStats Stats => playerStats;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            if (playerStats == null) playerStats = Resources.Load<PlayerStats>("PlayerStats");
            ResetDatos();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start() => CambiarEstado(EstadoJuego.Inicio);

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape))
        {
            if (estadoActual == EstadoJuego.Jugando) PausarJuego();
            else if (estadoActual == EstadoJuego.Pausa) ReanudarJuego();
        }
    }

    public void CambiarEstado(EstadoJuego nuevo)
    {
        // Evita llamar 2 veces al mismo estado (Derrota 3 veces en el mismo frame)
        if (estadoActual == nuevo && nuevo != EstadoJuego.Inicio) return;

        estadoActual = nuevo;
        currentState = (GameState)nuevo;
        Debug.Log($"[GameManager] Estado cambiado a: {estadoActual}");

        // Solo GameManager maneja TimeScale y cursor. UIManager ya no lo toca.
        if (nuevo == EstadoJuego.Pausa || nuevo == EstadoJuego.Derrota || nuevo == EstadoJuego.Victoria)
            Time.timeScale = 0f;
        else
            Time.timeScale = 1f;

        if (nuevo == EstadoJuego.Jugando)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        OnChangeState?.Invoke(currentState);
    }

    public void IniciarJuego()
    {
        ResetDatos();
        PlayerPrefs.SetString("NextScene", "Level1");
        SceneManager.LoadScene("PantallaDeCarga");
        CambiarEstado(EstadoJuego.Jugando);
    }

    public void EntrarComisaria()
    {
        PlayerPrefs.SetString("NextScene", "Level2");
        SceneManager.LoadScene("PantallaDeCarga");
        CambiarEstado(EstadoJuego.Jugando);
    }

    public void CargarNivel2() => EntrarComisaria();

    public void SalirComisaria()
    {
        PlayerPrefs.SetString("NextScene", "Level1");
        SceneManager.LoadScene("PantallaDeCarga");
        CambiarEstado(EstadoJuego.Jugando);
    }

    public void VolverMenu()
    {
        ShopTransition.ReiniciarEstado();
        LaberintoTransition.ReiniciarEstado();
        PlayerPrefs.SetString("NextScene", "MainMenu");
        SceneManager.LoadScene("PantallaDeCarga");
        CambiarEstado(EstadoJuego.Inicio);
    }

    public void ReanudarJuego() => CambiarEstado(EstadoJuego.Jugando);
    //Arreglo del inventario para que no se muestre en el menu de pausa y los puzzles
    public void PausarJuego()
    {
        InventoryController inventory =
            FindFirstObjectByType<InventoryController>();

        if (inventory != null)
            inventory.CloseInventory();

        PasswordDoor passwordPuzzle =
            FindFirstObjectByType<PasswordDoor>();

        if (passwordPuzzle != null)
            passwordPuzzle.ClosePasswordPanel();

        WirePuzzle wirePuzzle =
            FindFirstObjectByType<WirePuzzle>();

        if (wirePuzzle != null)
            wirePuzzle.ClosePuzzle();

        CambiarEstado(EstadoJuego.Pausa);
    }

    public void Victoria() => CambiarEstado(EstadoJuego.Victoria);
    public void Derrota() => CambiarEstado(EstadoJuego.Derrota);

    public void ReiniciarNivel()
    {
        Time.timeScale = 1f;
        ResetDatos();
        // Si estás en la Tienda o el Laberinto (se cargan encima del nivel), reinicia desde el inicio del Level1
        string escenaActual = SceneManager.GetActiveScene().name;
        if (escenaActual == "Tienda" || escenaActual == "Laberinto") escenaActual = "Level1";
        ShopTransition.ReiniciarEstado();
        LaberintoTransition.ReiniciarEstado();
        PlayerPrefs.SetString("NextScene", escenaActual);
        SceneManager.LoadScene("PantallaDeCarga");
        CambiarEstado(EstadoJuego.Jugando);
    }

    public void SalirDelJuego()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void ResetDatos()
    {
        // Partida nueva: el PlayerStats vuelve a sus valores base (se pierden las compras)
        if (playerStats != null)
        {
            playerStats.ResetStats();
            vidaMax = playerStats.vidaMax;
        }
        vidaActual = vidaMax;
        puntajeAcumulado = 0;
        itemsRecolectados = 0;
        jefeFinalMuerto = false;
        OnPuntajeCambiado?.Invoke(puntajeAcumulado);
    }

    public void SetVida(float v) { vidaActual = v; }
    public float GetVida() => vidaActual;

    public void AddPuntos(int p)
    {
        puntajeAcumulado += p;
        OnPuntajeCambiado?.Invoke(puntajeAcumulado);
    }

    // Tienda: descuenta puntos solo si alcanzan
    public bool GastarPuntos(int cantidad)
    {
        if (cantidad < 0 || puntajeAcumulado < cantidad) return false;
        puntajeAcumulado -= cantidad;
        OnPuntajeCambiado?.Invoke(puntajeAcumulado);
        return true;
    }

    public void MatarJefeFinal()
    {
        jefeFinalMuerto = true;
        Debug.Log("Jefe Final derrotado, ya puedes escapar!");
    }

    public void AddItem(int c = 1) { itemsRecolectados += c; }
}
