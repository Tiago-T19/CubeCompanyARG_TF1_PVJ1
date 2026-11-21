using UnityEngine;
using UnityEngine.UIElements;

public class BagInventoryUIController : MonoBehaviour
{
    private UIDocument document;

    private Image slot4;
    private Image slot5;
    private Image slot6;
    private Image slot7;
    private Image slot8;
    private Image slot9;

    private void Start()
    {
        document = GetComponent<UIDocument>();

        VisualElement root =
            document.rootVisualElement;

        slot4 = root.Q<Image>("slot04");

        slot4.RegisterCallback<ClickEvent>(evt =>
        {
            evt.StopPropagation();
            InventoryManager.Instance.EquipFromBackpack(0);
        });

        slot5 = root.Q<Image>("slot05");

        slot5.RegisterCallback<ClickEvent>(evt =>
        {
            evt.StopPropagation();
            InventoryManager.Instance.EquipFromBackpack(1);
        });

        slot6 = root.Q<Image>("slot06");

        slot6.RegisterCallback<ClickEvent>(evt =>
        {
            evt.StopPropagation();
            InventoryManager.Instance.EquipFromBackpack(2);
        });

        slot7 = root.Q<Image>("slot07");

        slot7.RegisterCallback<ClickEvent>(evt =>
        {
            evt.StopPropagation();
            InventoryManager.Instance.EquipFromBackpack(3);
        });

        slot8 = root.Q<Image>("slot08");

        slot8.RegisterCallback<ClickEvent>(evt =>
        {
            evt.StopPropagation();
            InventoryManager.Instance.EquipFromBackpack(4);
        });

        slot9 = root.Q<Image>("slot09");

        slot9.RegisterCallback<ClickEvent>(evt =>
        {
            evt.StopPropagation();
            InventoryManager.Instance.EquipFromBackpack(5);
        });

        InventoryManager.Instance.OnInventoryChanged +=
            UpdateUI;

        UpdateUI();
    }

    private void OnDestroy()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged -=
                UpdateUI;
        }
    }

    private void UpdateUI()
    {
        if (InventoryManager.Instance == null)
            return;

        InventorySlot[] slots =
            InventoryManager.Instance.GetBackpackSlots();

        slot4.image = null;
        slot5.image = null;
        slot6.image = null;
        slot7.image = null;
        slot8.image = null;
        slot9.image = null;

        if (slots.Length > 0 && slots[0] != null)
            slot4.image =
                slots[0].itemData.icon.texture;

        if (slots.Length > 1 && slots[1] != null)
            slot5.image =
                slots[1].itemData.icon.texture;

        if (slots.Length > 2 && slots[2] != null)
            slot6.image =
                slots[2].itemData.icon.texture;

        if (slots.Length > 3 && slots[3] != null)
            slot7.image =
                slots[3].itemData.icon.texture;

        if (slots.Length > 4 && slots[4] != null)
            slot8.image =
                slots[4].itemData.icon.texture;

        if (slots.Length > 5 && slots[5] != null)
            slot9.image =
                slots[5].itemData.icon.texture;
    }
}

  
