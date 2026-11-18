using UnityEngine;
using System;

public class GunController : MonoBehaviour

{
    public float damage = 50f;
    public float range = 100f;
    [SerializeField] private LayerMask enemyLayer;
    public int maxAmmo = 30;
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
        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            Reload();
        }
    }

    private void Shoot()
    {
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

            HealthSystem health = hit.collider.GetComponentInParent<HealthSystem>();

            if (health != null)
            {
                health.TakeDamage(damage);
            }
        }
    }

    private void Reload()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayReload();
        currentAmmo = maxAmmo;

        // Avisa al HUD
        OnAmmoProgressChanged?.Invoke(currentAmmo, maxAmmo);

        Debug.Log("Recargando...");
    }
}