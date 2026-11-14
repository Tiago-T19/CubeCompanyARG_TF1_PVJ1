using UnityEngine;

// oculto por el momento, hasta solucionar el inventario y la interaccion con los objetos


/*public class PickAble : MonoBehaviour, IInteractable
{
    [SerializeField] private ItemData itemData;
    [SerializeField] private int quantity = 1;
    [SerializeField] private bool destroyOnPickUp = true;

    private bool pickedUp = false;

    public ItemData ItemData => itemData;
    public int Quantity => quantity;

    public bool canInteract()
    {
        return !pickedUp;
    }

    public void Interact()
    {
        TryPickUp();
    }

    public bool TryPickUp()
    {
        if (pickedUp || itemData == null)
            return false;

        pickedUp = true;

        if (destroyOnPickUp)
            Destroy(gameObject);

        return true;
    }
}*/


// Arreglo de inventario temporal

public class Pickable: MonoBehaviour, IInteractable
{
    [Header("Item")]
    [SerializeField] private ItemData itemData;
    [SerializeField] private int quantity = 1;

    [Header("Behaviour")]
    [SerializeField] private bool destroyOnPickUp = true;
    private bool _pickedUp = false;

    public ItemData ItemData => itemData;

    public int Quantity => quantity;

    // Métodos requeridos por la interfaz IInteractable
    public bool canInteract()
    {
        return ! _pickedUp;
    }

    public void Interact()
    {
        TryPickUp();
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


