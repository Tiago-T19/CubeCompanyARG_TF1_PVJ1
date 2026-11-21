using UnityEngine;
using System;
using UnityEngine.EventSystems;

public class GunController : MonoBehaviour
{
    [Header("Efecto de impacto")]
    [SerializeField] private GameObject vfx;
    [SerializeField] private float vfxDuration = 2f;

    [Header("Características")]
    [SerializeField] private float damage = 50f;
    [SerializeField] private float range = 100f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private int maxAmmo = 30;

    [SerializeField] private bool isReloading;
    [SerializeField] private float reloadTime = 2f;

    public int MaxAmmo => maxAmmo;
    private int currentAmmo;
    public int CurrentAmmo => currentAmmo;

    private Camera cam;

    public event Action<int, int> OnAmmoProgressChanged;

    private void Start()
    {
        currentAmmo = maxAmmo;
        cam = Camera.main;

        // Avisa el estado inicial
        OnAmmoProgressChanged?.Invoke(currentAmmo, maxAmmo);
    }

    private void Update()
    {
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.estadoActual != EstadoJuego.Jugando)
                return;

            if (GameManager.Instance.inventarioAbierto)
                return;

            if (GameManager.Instance.resolviendoPuzzle)
                return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            StartReload();
        }
    }

    // Configura el arma con los datos del ScriptableObject
    public void SetupWeapon(ItemData weaponData)
    {
        Debug.Log("CONFIGURANDO ARMA: " + weaponData.itemName);
        damage = weaponData.weaponDamage;
        maxAmmo = weaponData.weaponMaxAmmo;
        range = weaponData.weaponRange;
        reloadTime = weaponData.reloadTime;

        currentAmmo = maxAmmo;

        OnAmmoProgressChanged?.Invoke(currentAmmo, maxAmmo);


        Debug.Log("ARMA EQUIPADA: " + weaponData.itemName);
        Debug.Log("Daño: " + weaponData.weaponDamage);
        Debug.Log("Balas: " + weaponData.weaponMaxAmmo);
    }

    private void Shoot()
    {
        if (isReloading)
            return;

        if (currentAmmo <= 0)
        {
            Debug.Log("Sin balas");
            return;
        }

        currentAmmo--;

        // Avisa al HUD
        OnAmmoProgressChanged?.Invoke(currentAmmo, maxAmmo);

        if (AudioManager.Instance != null) AudioManager.Instance.PlayShoot();

        Debug.Log("Disparo. Balas: " + currentAmmo);

        Ray ray = cam.ScreenPointToRay(
            new Vector3(
                Screen.width / 2,
                Screen.height / 2
            )
        );

        if (Physics.Raycast(ray, out RaycastHit hit, range, enemyLayer))
        {
            Debug.Log("Impactó en " + hit.collider.name);

            if (vfx != null)
            {
                GameObject effect = Instantiate(vfx, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(effect, vfxDuration);
            }

            HealthSystem health = hit.collider.GetComponentInParent<HealthSystem>();

            if (health != null)
            {
                health.TakeDamage(damage);
            }
        }
    }

    private void StartReload()
    {
        if (isReloading)
            return;

        if (currentAmmo >= maxAmmo)
            return;

        isReloading = true;

        Invoke(nameof(FinishReload), reloadTime);

        Debug.Log("Recargando...");
    }

    private void FinishReload()
    {
        isReloading = false;

        currentAmmo = maxAmmo;

        OnAmmoProgressChanged?.Invoke(
            currentAmmo,
            maxAmmo
        );

        Debug.Log("Recarga completada");
    }
}