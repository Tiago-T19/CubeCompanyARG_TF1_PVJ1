using System;
using UnityEngine;
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [SerializeField] private int hotbarSize = 3;
    // Modificado para que el inventario tenga un tamaño de 6 en la mochila
    [SerializeField] private int backpackSize = 6;

    private InventorySlot[] hotbarSlots;
    private InventorySlot[] backpackSlots;

    private int selectedSlot = 0;
    public int SelectedSlot => selectedSlot;

    public InventorySlot[] GetHotbarSlots()
    {
        return hotbarSlots;
    }

    public InventorySlot[] GetBackpackSlots()
    {
        return backpackSlots;
    }

  
    public void EquipFromBackpack(int backpackIndex)
    {
        if (backpackSlots[backpackIndex] == null)
            return;

        // Si había algo en la hotbar vuelve a la mochila
        InventorySlot oldHotbarItem =
            hotbarSlots[selectedSlot];

        hotbarSlots[selectedSlot] =
            backpackSlots[backpackIndex];

        backpackSlots[backpackIndex] =
            oldHotbarItem;

        EquipmentManager.Instance.Equip(
            hotbarSlots[selectedSlot].itemData
        );

        OnInventoryChanged?.Invoke();
    }

    public Action OnInventoryChanged;

    public void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        hotbarSlots = new InventorySlot[hotbarSize];
        backpackSlots = new InventorySlot[backpackSize];
    }

    private void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll > 0f)
        {
            ChangeSelectedSlot(-1);
        }

        if (scroll < 0f)
        {
            ChangeSelectedSlot(1);
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            UseSelectedItem();
        }
    }
    public bool AddItem(ItemData itemData, int quantity)
    {
        Debug.Log("Agregando: " + itemData.itemName);

        for (int i = 0; i < backpackSlots.Length; i++)
        {
            if (backpackSlots[i] != null && backpackSlots[i].itemData == itemData)
            {
                backpackSlots[i].quantity += quantity;
                OnInventoryChanged?.Invoke();
                return true;
            }
        }

        for (int i = 0; i < backpackSlots.Length; i++)
        {
            if (backpackSlots[i] == null)
            {
              
                backpackSlots[i] = new InventorySlot(itemData, quantity);

                OnInventoryChanged?.Invoke();

                return true;
            }
        }
        return false; // No hay espacio en el inventario
    }

    public InventorySlot[] GetSlots()
    {
        return hotbarSlots;
    }


    public void ChangeSelectedSlot(int direction)
    {
        selectedSlot += direction;

        if (selectedSlot >= hotbarSlots.Length)
            selectedSlot = 0;

        if (selectedSlot < 0)
            selectedSlot = hotbarSlots.Length - 1;

        EquipSelectedItem();

        OnInventoryChanged?.Invoke();
    }

    private void EquipSelectedItem()
    {
        if (hotbarSlots[selectedSlot] == null)
            return;

        EquipmentManager.Instance.Equip(
            hotbarSlots[selectedSlot].itemData
        );
    }

    private void UseSelectedItem()
    {
        if (hotbarSlots[selectedSlot] == null)
            return;

        ItemData item = hotbarSlots[selectedSlot].itemData;

        if (!item.isConsumable)
            return;

        GameObject playerObj =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObj == null)
            return;

        HealthSystem player =
            playerObj.GetComponent<HealthSystem>();

        if (player != null)
        {
            player.Heal(item.healAmount);

            Debug.Log("Curado +" + item.healAmount);
        }

        hotbarSlots[selectedSlot].quantity--;

        if (hotbarSlots[selectedSlot].quantity <= 0)
        {
            hotbarSlots[selectedSlot] = null;
        }

        OnInventoryChanged?.Invoke();
    }
}

