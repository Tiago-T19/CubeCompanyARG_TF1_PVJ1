using System;
using System.Collections.Generic;
using UnityEngine;

// ScriptableObject con los atributos del jugador.
// La tienda modifica estos valores y, como el asset vive fuera de las escenas,
// las mejoras se mantienen al pasar de nivel (Level1 -> Tienda -> Level2).
[CreateAssetMenu(fileName = "PlayerStats", menuName = "TFI/PlayerStats")]
public class PlayerStats : ScriptableObject
{
    [Header("Valores base (con los que arranca una partida nueva)")]
    [SerializeField] private float vidaMaxBase = 100f;
    [SerializeField] private float velocidadBase = 5f;
    [SerializeField] private float fuerzaSaltoBase = 7f;

    [Header("Valores actuales (los modifica la tienda en juego)")]
    public float vidaMax = 100f;
    public float velocidad = 5f;
    public float fuerzaSalto = 7f;

    // Compras de la partida actual (id del item). No se guarda en el asset.
    [NonSerialized] private List<string> compras = new List<string>();

    // Patrón Observador: avisa cuando cambia cualquier stat
    public event Action OnStatsChanged;

    // Se llama al empezar una partida nueva (GameManager.ResetDatos)
    public void ResetStats()
    {
        vidaMax = vidaMaxBase;
        velocidad = velocidadBase;
        fuerzaSalto = fuerzaSaltoBase;
        compras.Clear();
        OnStatsChanged?.Invoke();
    }

    public void SumarVidaMax(float cantidad)
    {
        vidaMax += cantidad;
        OnStatsChanged?.Invoke();
    }

    public void SumarVelocidad(float cantidad)
    {
        velocidad += cantidad;
        OnStatsChanged?.Invoke();
    }

    public void SumarFuerzaSalto(float cantidad)
    {
        fuerzaSalto += cantidad;
        OnStatsChanged?.Invoke();
    }

    // Avisa que cambió algo del jugador (por ejemplo, se curó en la tienda)
    public void NotificarCambio() => OnStatsChanged?.Invoke();

    public void RegistrarCompra(string idItem) => compras.Add(idItem);

    public int CantidadComprada(string idItem)
    {
        int total = 0;
        foreach (string id in compras)
            if (id == idItem) total++;
        return total;
    }
}
