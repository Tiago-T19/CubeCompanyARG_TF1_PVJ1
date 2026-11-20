using UnityEngine;
using System;

public class HealthSystem : MonoBehaviour
{
    [SerializeField] private EnemyData datosEnemigo; // asset enemigo.
    [SerializeField] private bool isPlayer = false;

    private float maxHealth;
    private float currentHealth;

    public float CurrentHealth => currentHealth;
    public float vidaMax => maxHealth;
    public bool IsDead => currentHealth <= 0;

    public event Action<float> OnChangeHealthPlayer;
    public event Action OnDeath;

    void Awake()
    {
        maxHealth = datosEnemigo != null ? datosEnemigo.vidaMax : 100f;

        if (isPlayer)
            RefrescarVidaJugador();
        else
            currentHealth = maxHealth;
    }

    // Al volver de la tienda el jugador se reactiva: releemos la vida (puede haber comprado mejoras)
    void OnEnable()
    {
        if (!isPlayer) return;
        RefrescarVidaJugador();
        // Si se compra vida en la tienda, el jugador se entera por evento
        if (GameManager.Instance != null && GameManager.Instance.Stats != null)
            GameManager.Instance.Stats.OnStatsChanged += RefrescarVidaJugador;
    }

    void OnDisable()
    {
        if (isPlayer && GameManager.Instance != null && GameManager.Instance.Stats != null)
            GameManager.Instance.Stats.OnStatsChanged -= RefrescarVidaJugador;
    }

    // El jugador toma su vida máxima del PlayerStats y su vida actual del GameManager
    void RefrescarVidaJugador()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null)
        {
            currentHealth = maxHealth;
            return;
        }

        if (gm.Stats != null) maxHealth = gm.Stats.vidaMax;
        currentHealth = Mathf.Clamp(gm.GetVida(), 0, maxHealth);
        OnChangeHealthPlayer?.Invoke(currentHealth / maxHealth);
    }

    public void TakeDamage(float dmg)
    {
        currentHealth -= dmg;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        Debug.Log($"{gameObject.name} vida: {currentHealth}");

        if (isPlayer)
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayHurt();
            OnChangeHealthPlayer?.Invoke(currentHealth / maxHealth);
            if (GameManager.Instance) GameManager.Instance.SetVida(currentHealth);
            if (currentHealth <= 0) GameManager.Instance.Derrota();
        }
        else
        {
            if (currentHealth <= 0)
            {
                Die();
            }
            else
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayZombieHurt(transform);
            }
        }
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        if (isPlayer)
        {
            OnChangeHealthPlayer?.Invoke(currentHealth / maxHealth);
            if (GameManager.Instance) GameManager.Instance.SetVida(currentHealth);
        }
    }

    void Die()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayZombieDeath(transform);
        OnDeath?.Invoke();
        int puntos = datosEnemigo != null ? datosEnemigo.puntosAlMorir : 50;
        if (GameManager.Instance) GameManager.Instance.AddPuntos(puntos);
        Destroy(gameObject);
    }
}