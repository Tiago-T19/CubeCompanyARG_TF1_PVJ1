using System;
using UnityEngine;

// Lógica de compra de la tienda (clase estática: no hace falta ponerla en ningún objeto).
// Valida el saldo, descuenta los puntos y aplica la mejora sobre el ScriptableObject PlayerStats.
// No toca objetos, UI ni audio: avisa por eventos (Patrón Observador).
public static class ShopManager
{
    public static event Action<ShopItemData> OnCompraRealizada;
    public static event Action<ShopItemData, string> OnCompraFallida;

    private static GameManager GM => GameManager.Instance;
    private static PlayerStats Stats => GM != null ? GM.Stats : null;

    // El saldo de la tienda son los puntos acumulados del GameManager
    public static int Saldo => GM != null ? GM.puntajeAcumulado : 0;

    public static int Compradas(ShopItemData item) => Stats != null ? Stats.CantidadComprada(item.Id) : 0;
    public static bool Agotado(ShopItemData item) => Compradas(item) >= item.maxCompras;
    public static bool AlcanzaElSaldo(ShopItemData item) => Saldo >= item.precio;

    // Validación antes de comprar. Devuelve el motivo si no se puede.
    public static bool PuedeComprar(ShopItemData item, out string motivo)
    {
        motivo = "";

        if (item == null) { motivo = "Producto inválido"; return false; }
        if (GM == null || Stats == null) { motivo = "Falta GameManager o PlayerStats"; return false; }
        if (Agotado(item)) { motivo = "Agotado"; return false; }
        if (!AlcanzaElSaldo(item)) { motivo = $"Te faltan {item.precio - Saldo} pts"; return false; }

        if (item.tipo == PowerUpType.CuracionTotal && GM.GetVida() >= Stats.vidaMax)
        {
            motivo = "Ya tenés la vida llena";
            return false;
        }

        return true;
    }

    // Devuelve true si se compró
    public static bool IntentarComprar(ShopItemData item)
    {
        if (!PuedeComprar(item, out string motivo))
        {
            Debug.Log($"[Tienda] No se pudo comprar '{(item != null ? item.nombre : "?")}': {motivo} (saldo: {Saldo})");
            OnCompraFallida?.Invoke(item, motivo);
            return false;
        }

        // Primero se descuenta y recién después se otorga la mejora
        if (!GM.GastarPuntos(item.precio))
        {
            OnCompraFallida?.Invoke(item, "No te alcanzan los puntos");
            return false;
        }

        Stats.RegistrarCompra(item.Id);
        AplicarMejora(item);

        Debug.Log($"[Tienda] COMPRADO '{item.nombre}' por {item.precio} pts. Saldo: {Saldo} | Vida max: {Stats.vidaMax} | Velocidad: {Stats.velocidad} | Salto: {Stats.fuerzaSalto}");
        OnCompraRealizada?.Invoke(item);
        return true;
    }

    private static void AplicarMejora(ShopItemData item)
    {
        switch (item.tipo)
        {
            case PowerUpType.VidaMaxima:
                GM.SetVida(GM.GetVida() + item.valor); // también suma vida actual
                Stats.SumarVidaMax(item.valor);        // dispara OnStatsChanged
                GM.vidaMax = Stats.vidaMax;
                break;

            case PowerUpType.Velocidad:
                Stats.SumarVelocidad(item.valor);
                break;

            case PowerUpType.FuerzaSalto:
                Stats.SumarFuerzaSalto(item.valor);
                break;

            case PowerUpType.CuracionTotal:
                GM.SetVida(Stats.vidaMax);
                Stats.NotificarCambio();
                break;
        }

        Debug.Log($"[Tienda] Compraste {item.nombre}. Vida máx: {Stats.vidaMax} | Velocidad: {Stats.velocidad} | Salto: {Stats.fuerzaSalto} | Puntos: {Saldo}");
    }
}
