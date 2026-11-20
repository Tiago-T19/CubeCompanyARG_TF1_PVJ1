using UnityEngine;

public enum PowerUpType
{
    VidaMaxima,     // suma vida máxima (y la misma cantidad de vida actual)
    Velocidad,      // suma velocidad de movimiento
    FuerzaSalto,    // suma fuerza de salto
    CuracionTotal   // recupera toda la vida
}

// Datos de un power-up de la tienda (un asset por cada producto).
// La lógica de compra está en ShopManager: acá solo hay datos.
[CreateAssetMenu(fileName = "ShopItem", menuName = "TFI/Tienda/ShopItemData")]
public class ShopItemData : ScriptableObject
{
    [Header("Info que se muestra")]
    public string nombre = "Power-up";
    [TextArea] public string descripcion;
    public Sprite icono;

    [Header("Compra")]
    public int precio = 50;
    [Tooltip("Cuántas veces se puede comprar por partida")]
    public int maxCompras = 3;

    [Header("Efecto")]
    public PowerUpType tipo;
    public float valor = 20f;

    // Identificador único: usamos el nombre del asset
    public string Id => name;
}
