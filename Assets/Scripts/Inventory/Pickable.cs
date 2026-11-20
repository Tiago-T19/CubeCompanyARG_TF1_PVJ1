using UnityEngine;
public class Pickable : MonoBehaviour, IInteractable
{
    [Header("Item")]
    [SerializeField] private ItemData itemData;
    [SerializeField] private int quantity = 1;

    [Header("Behaviour")]
    [SerializeField] private bool destroyOnPickUp = true;
    private bool _pickedUp = false;

    public ItemData ItemData => itemData;

    public int Quantity => quantity;
    private Outline outline; // Componente de Outline para resaltar el objeto

    // Métodos requeridos por la interfaz IInteractable

    public bool canInteract()
    {
        return !_pickedUp;
    }

    void Awake()
    {
        outline = gameObject.AddComponent<Outline>();
        outline.OutlineMode = Outline.Mode.OutlineVisible;
        outline.OutlineColor = Color.yellow;
        outline.OutlineWidth = 5f;
        outline.enabled = false;
    }

    public void Interact()
    {
        TryPickUp();
    }

    public void OnFocusGained()
    {
        outline.enabled = true; // Activamos el Outline cuando el jugador mira el objeto
    }

    public void OnFocusLost()
    {
        outline.enabled = false; // Desactivamos el Outline cuando el jugador deja de mirar el objeto
    }

    public bool TryPickUp()
    {
        if (_pickedUp || itemData == null) return false;
        _pickedUp = true;
        OnPickedUp();
        return true;
    }

    private void OnPickedUp()
    {
        bool added = InventoryManager.Instance.AddItem(itemData, quantity);

        if (added)
        {
            Debug.Log($"Recogido {quantity} x {itemData.itemName} x {quantity}");
            if (destroyOnPickUp)
                Destroy(gameObject);
        }
    }
}


