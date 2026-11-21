using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance;
    public Transform handPoint;
    private GameObject currentObject;

    private void Awake() => Instance = this;

    public void Equip(ItemData item)
    {
        if (currentObject != null)
        {
            GunController viejo = currentObject.GetComponent<GunController>();
            if (viejo != null) viejo.Desequipar();
            Destroy(currentObject);
        }

        if (item.prefab != null)
        {
            currentObject = Instantiate(item.prefab, handPoint);
            currentObject.transform.localPosition = Vector3.zero;
            currentObject.transform.localRotation = Quaternion.identity;

            Pickable pick = currentObject.GetComponent<Pickable>();
            if (pick != null) pick.enabled = false;

            GunController gun = currentObject.GetComponent<GunController>();
            if (gun != null)
            {
                gun.SetupWeapon(item); // Le pasa el ItemData de la tienda / suelo
                gun.Equipar(); // Esto ya actualiza el HUD
            }
        }
    }

    public GunController GetCurrentGun()
    {
        if (currentObject == null) return null;
        return currentObject.GetComponent<GunController>();
    }
}