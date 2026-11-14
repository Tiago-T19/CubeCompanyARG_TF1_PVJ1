//using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityCursor = UnityEngine.Cursor;


// Ocutlo por el momento hasta arreglar el inventario

public class InventoryController : MonoBehaviour
{
    private VisualElement root;
    private bool inventoryOpen = false;

    // Slots visuales
    private VisualElement slot1;
    private VisualElement slot2;
    private VisualElement slot3;

    private void Start()
    {
        Debug.Log("InventoryController iniciado");

        root = GetComponent<UIDocument>().rootVisualElement;
        // Buscamos los contenedores de los slots
        slot1 = root.Q<VisualElement>("slot1");
        slot2 = root.Q<VisualElement>("slot2");
        slot3 = root.Q<VisualElement>("slot3");


        InventoryManager.Instance.OnInventoryChanged += UpdateSelectedVisual;

        // Resaltamos el primer slot
        UpdateSelectedVisual();

        root.style.display = DisplayStyle.None;

        UnityCursor.lockState = CursorLockMode.Locked;
        UnityCursor.visible = false;
    }

    private void OnDestroy()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged -= UpdateSelectedVisual;
        }
    }

    private void UpdateSelectedVisual()
    {
        slot1.RemoveFromClassList("_slot_Style--Selected");
        slot2.RemoveFromClassList("_slot_Style--Selected");
        slot3.RemoveFromClassList("_slot_Style--Selected");

        switch (InventoryManager.Instance.SelectedSlot)
        {
            case 0:
                slot1.AddToClassList("_slot_Style--Selected");
                break;

            case 1:
                slot2.AddToClassList("_slot_Style--Selected");
                break;

            case 2:
                slot3.AddToClassList("_slot_Style--Selected");
                break;
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Debug.Log("TAB PRESIONADO");

            inventoryOpen = !inventoryOpen;

            root.style.display = inventoryOpen
                ? DisplayStyle.Flex
                : DisplayStyle.None;

            if (inventoryOpen)
            {
                UnityCursor.lockState = CursorLockMode.None;
                UnityCursor.visible = true;
            }
            else
            {
                UnityCursor.lockState = CursorLockMode.Locked;
                UnityCursor.visible = false;
            }
        }
    }

}