using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject PausePanel;
    [SerializeField] private GameObject VictoryPanel;
    [SerializeField] private GameObject GameOverPanel;
    [SerializeField] private GameObject InicioPanel;
    [SerializeField] private GameObject HUDPanel;
    [SerializeField] private TextMeshProUGUI puntajeText;
    [SerializeField] private Slider healthBar;
    [SerializeField] private HealthSystem playerHealth;
    [SerializeField] private GameObject creditosPanel;
    [SerializeField] private GameObject opcionesPanel;

    [Header("Mision - solo Level1")]
    [SerializeField] private TextMeshProUGUI ammoText;

    void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnChangeState += HandleState;
            GameManager.Instance.OnPuntajeCambiado += UpdatePuntaje;
            HandleState(GameManager.Instance.currentState);
            UpdatePuntaje(GameManager.Instance.puntajeAcumulado);
        }
        if (playerHealth != null) playerHealth.OnChangeHealthPlayer += UpdateHealth;

        // --- BALAS: Ahora escuchamos al evento ESTATICO ---
        // Asi anda con cualquier arma que te equipes, no solo con una
        GunController.OnAmmoProgressChanged += UpdateAmmoText;

        // Si ya hay un arma equipada, pedimos sus balas actuales para no arrancar en 0/0
        if (EquipmentManager.Instance != null && EquipmentManager.Instance.GetCurrentGun() != null)
        {
            var g = EquipmentManager.Instance.GetCurrentGun();
            UpdateAmmoText(g.CurrentAmmo, g.MaxAmmo);
        }
    }

    void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnChangeState -= HandleState;
            GameManager.Instance.OnPuntajeCambiado -= UpdatePuntaje;
        }
        if (playerHealth != null) playerHealth.OnChangeHealthPlayer -= UpdateHealth;

        GunController.OnAmmoProgressChanged -= UpdateAmmoText;
    }

    void HandleState(GameState state)
    {
        // Siempre arrancamos con creditos apagado.
        // Asi si volves al menu desde Pausa o GameOver no queda abierto.
        if (creditosPanel) creditosPanel.SetActive(false);
        if (opcionesPanel) opcionesPanel.SetActive(false);

        if (InicioPanel) InicioPanel.SetActive(state == GameState.Start);
        if (HUDPanel) HUDPanel.SetActive(state == GameState.Playing);
        if (PausePanel) PausePanel.SetActive(state == GameState.Paused);
        if (VictoryPanel) VictoryPanel.SetActive(state == GameState.Victory);
        if (GameOverPanel) GameOverPanel.SetActive(state == GameState.GameOver);
    }

    void UpdatePuntaje(int p) { if (puntajeText) puntajeText.text = p.ToString(); }
    void UpdateHealth(float p) { if (healthBar) healthBar.value = p; }

    // --- Balas HUD ---
    private void UpdateAmmoText(int recovered, int total)
    {
        if (ammoText) ammoText.text = $"{recovered} / {total}";
    }

    public void TogglePanel(GameObject panel) { panel.SetActive(!panel.activeSelf); }

    // --- CREDITOS: Panel_Inicio - Panel_Creditos ---
    public void MostrarCreditos()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        if (creditosPanel == null)
        {
            Debug.LogWarning("[UIManager] CreditosPanel no asignado.");
            return;
        }
        if (InicioPanel) InicioPanel.SetActive(false);
        creditosPanel.SetActive(true);
    }

    public void VolverDeCreditos()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        if (creditosPanel) creditosPanel.SetActive(false);
        if (InicioPanel) InicioPanel.SetActive(true);
    }

    public void MostrarOpciones()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        if (opcionesPanel == null)
        {
            Debug.LogWarning("[UIManager] OpcionesPanel no asignado.");
            return;
        }
        if (InicioPanel) InicioPanel.SetActive(false);
        opcionesPanel.SetActive(true);
    }

    public void VolverDeOpciones()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        if (opcionesPanel) opcionesPanel.SetActive(false);
        if (InicioPanel) InicioPanel.SetActive(true);
    }

    // Alias para conectar directo desde los botones
    public void Btn_Creditos() => MostrarCreditos();
    public void Btn_VolverCreditos() => VolverDeCreditos();

    public void Btn_Opciones() => MostrarOpciones();
    public void Btn_VolverOpciones() => VolverDeOpciones();

    public void Btn_Reanudar() { if (AudioManager.Instance != null) AudioManager.Instance.PlayClick(); GameManager.Instance.ReanudarJuego(); }
    public void Btn_Reiniciar() { if (AudioManager.Instance != null) AudioManager.Instance.PlayClick(); GameManager.Instance.ReiniciarNivel(); }
    public void Btn_MenuPrincipal() { if (AudioManager.Instance != null) AudioManager.Instance.PlayClick(); GameManager.Instance.VolverMenu(); }
    public void Btn_Salir() { if (AudioManager.Instance != null) AudioManager.Instance.PlayClick(); GameManager.Instance.SalirDelJuego(); }
}