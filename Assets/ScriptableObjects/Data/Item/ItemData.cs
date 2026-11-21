using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/ItemData")]
public class ItemData : ScriptableObject
{
    public string itemID;
    public string itemName;
    public Sprite icon;
    public string description;
    public int maxStackSize = 3;

    public GameObject prefab;

    [Header("Armas")]
    public bool isWeapon;

    public float weaponDamage;
    public int weaponMaxAmmo;
    public float weaponRange;

    public float reloadTime;

    public AudioClip shootSound;
    public AudioClip reloadSound;

    // Consumible botiquin 
    public bool isConsumable;
    public int healAmount; 
}