using UnityEngine;
using UnityEngine.UIElements;

//oculto por el momento hasta arreglar el inventario

public class HotBarUIController : MonoBehaviour
{
    private UIDocument document;

    private Image slot1;
    private Image slot2;
    private Image slot3;

    private void Start()
    {
        document = GetComponent<UIDocument>();

        VisualElement root = document.rootVisualElement;

        slot1 = root.Q<Image>("slot01");
        slot2 = root.Q<Image>("slot02");
        slot3 = root.Q<Image>("slot03");

        Debug.Log("slot01: " + slot1);
        Debug.Log("slot02: " + slot2);
        Debug.Log("slot03: " + slot3);

        InventoryManager.Instance.OnInventoryChanged += UpdateUI;

        UpdateUI();
    }

    private void OnDestroy()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged -= UpdateUI;
    }

    private void UpdateUI()
    {
        if (InventoryManager.Instance == null)
            return;

        InventorySlot[] slots =
            InventoryManager.Instance.GetSlots();

        slot1.image = null;
        slot2.image = null;
        slot3.image = null;

        if (slots.Length > 0 && slots[0] != null)
        {
            slot1.image = slots[0].itemData.icon.texture;
        }

        if (slots.Length > 1 && slots[1] != null)
        {
            slot2.image = slots[1].itemData.icon.texture;
        }

        if (slots.Length > 2 && slots[2] != null)
        {
            slot3.image = slots[2].itemData.icon.texture;
        }
    }
}

// Arreglo de hotBarUIcontroller temporal
/*public class HotBarUIController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private int hotbarSize = 3;

    private VisualElement[] _slotElements;

    // probar

    private void Awake()
    {
        // Si no lo asignaste en el Inspector, lo busca en el mismo GameObject
        if (uiDocument == null)
        {
            uiDocument = GetComponent<UIDocument>();
        }
    }

    private void OnEnable()
    {
        if (uiDocument == null || uiDocument.rootVisualElement == null)
        
        {
            Debug.LogError("Falta asignar el UIDocument en HotBarUIController.");
            return;
        }

        // Obtenemos la raíz directamente desde el UIDocument
        VisualElement rootElement = uiDocument.rootVisualElement;
        VisualElement grid = rootElement.Q<VisualElement>("hotbar-grid");

        if (grid == null)
        {
            Debug.LogError("No se encontró el elemento 'hotbar-grid' en el UXML.");
            return;
        }

        _slotElements = new VisualElement[hotbarSize];
        int count = Mathf.Min(hotbarSize, grid.childCount);

        for (int i = 0; i < count; i++)
        {
            _slotElements[i] = grid[i];
        }

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged += RefreshHotbar;
            RefreshHotbar();
        }
    }

    private void OnDisable()
    {
        // Muy importante desuscribirse del evento al desactivar el objeto
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged -= RefreshHotbar;
        }
    }

    private void RefreshHotbar()
    {
        if (_slotElements == null || InventoryManager.Instance == null) return;

        for (int i = 0; i < _slotElements.Length; i++)
        {
            if (_slotElements[i] == null) continue;

            InventorySlot slotData = InventoryManager.Instance.GetSlot(i);
            VisualElement slotElement = _slotElements[i];

            Image icon = slotElement.Q<Image>("slot-icon");
            Label quantityLabel = slotElement.Q<Label>("slot-quantity");

            if (slotData != null && slotData.itemData != null)
            {
                if (icon != null)
                {
                    icon.sprite = slotData.itemData.icon;
                    icon.style.visibility = Visibility.Visible;
                }

                if (quantityLabel != null)
                {
                    quantityLabel.text = slotData.quantity > 1 ? slotData.quantity.ToString() : "";
                }
            }
            else
            {
                if (icon != null)
                {
                    icon.sprite = null;
                    icon.style.visibility = Visibility.Hidden;
                }

                if (quantityLabel != null)
                {
                    quantityLabel.text = "";
                }
            }
        }
    }
    
    // probando codigo finalizado

}
*/

// segundo intento




