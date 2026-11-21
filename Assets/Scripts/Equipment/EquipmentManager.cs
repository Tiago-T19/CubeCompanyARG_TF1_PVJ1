using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance;

   [SerializeField] private Transform handPoint;

    private GameObject currentObject;

    private void Awake()
    {
        Instance = this;
    }

    public void Equip(ItemData item)
    {
        if (currentObject != null)
        {
            Destroy(currentObject);
        }

        if (item.prefab != null)
        {
            currentObject = Instantiate(
                item.prefab,
                handPoint
            );

            currentObject.transform.localPosition = Vector3.zero;
            currentObject.transform.localRotation = Quaternion.identity;

            GunController gun =
    FindFirstObjectByType<GunController>();

            if (gun != null)
            {
                if (item.isWeapon)
                {
                    gun.enabled = true;

                    Debug.Log("Equipando: " + item.itemName);
                    Debug.Log("Es arma: " + item.isWeapon);

                    gun.SetupWeapon(item);
                }
                else
                {
                    gun.enabled = false;
                }
            }
        }
        // Configura el arma con los datos del ScriptableObject
        /*GunController gun =
        FindFirstObjectByType<GunController>();
        if (item.isWeapon)
        {
            gun.enabled = true;
            gun.SetupWeapon(item);
        }
        else
        {
            gun.enabled = false;
        }*/
    }
}