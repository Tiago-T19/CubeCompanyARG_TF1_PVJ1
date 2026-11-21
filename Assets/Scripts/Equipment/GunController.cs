using System.Collections;
using UnityEngine;

public class GunController : MonoBehaviour
{
    [Header("Datos del arma - aca va su ItemData")]
    [SerializeField] private ItemData itemData;

    [Header("Disparo")]
    [SerializeField] private Transform puntoDisparo;
    [SerializeField] private int balasPorDisparo = 1; // Escopeta: 6, Pistola: 1, M4: 1
    [SerializeField] private LayerMask mascaraDisparo;

    [Header("Efectos - aca van los PREFABS de Project, no de la escena")]
    [SerializeField] private GameObject efectoDisparoPrefab; // Fogonazo en la boca del arma
    [SerializeField] private GameObject efectoImpactoPrefab; // Chispa en el enemigo
    [SerializeField] private Camera camaraJugador;

    public int CurrentAmmo { get; private set; }
    public int MaxAmmo { get; private set; }
    public static event System.Action<int, int> OnAmmoProgressChanged;
    public event System.Action<int, int> OnAmmoChangedInstance;

    private float ultimoDisparo;
    private bool recargando = false;
    private bool estaEquipada = false;

    void Start()
    {
        if (itemData != null) MaxAmmo = itemData.weaponMaxAmmo;
        else MaxAmmo = 30;
        CurrentAmmo = MaxAmmo;
        if (camaraJugador == null) camaraJugador = Camera.main;
        NotificarBalas();

        if (EquipmentManager.Instance != null && EquipmentManager.Instance.handPoint != null)
        {
            if (transform.parent == EquipmentManager.Instance.handPoint)
                estaEquipada = true;
        }
    }

    void NotificarBalas()
    {
        OnAmmoProgressChanged?.Invoke(CurrentAmmo, MaxAmmo);
        OnAmmoChangedInstance?.Invoke(CurrentAmmo, MaxAmmo);
    }

    public void Equipar() { estaEquipada = true; NotificarBalas(); }
    public void Desequipar() => estaEquipada = false;

    void Update()
    {
        if (!estaEquipada) return;
        if (GameManager.Instance != null && GameManager.Instance.currentState != GameState.Playing) return;
        if (recargando) return;

        float cadencia = 0.25f;
        if (Input.GetButton("Fire1") && Time.time >= ultimoDisparo + cadencia)
        {
            if (CurrentAmmo > 0) Disparar();
            else Recargar();
        }
        if (Input.GetKeyDown(KeyCode.R)) Recargar();
    }

    void Disparar()
    {
        ultimoDisparo = Time.time;
        CurrentAmmo--;
        NotificarBalas();

        // --- FOGONAZO EN LA BOCA DEL ARMA ---
        if (efectoDisparoPrefab != null)
        {
            Transform donde = puntoDisparo != null ? puntoDisparo : transform;
            GameObject fogonazo = Instantiate(efectoDisparoPrefab, donde.position, donde.rotation);
            Destroy(fogonazo, 0.6f);
        }

        if (AudioManager.Instance != null && itemData != null)
            AudioManager.Instance.PlaySFX(itemData.shootSound);

        float danio = itemData != null ? itemData.weaponDamage : 25f;
        float rango = itemData != null ? itemData.weaponRange : 50f;

        for (int i = 0; i < balasPorDisparo; i++)
        {
            Vector3 direccion = camaraJugador.transform.forward;
            if (balasPorDisparo > 1)
                direccion += new Vector3(Random.Range(-0.08f, 0.08f), Random.Range(-0.08f, 0.08f), 0f);

            Vector3 origen = camaraJugador.transform.position + camaraJugador.transform.forward * 0.5f;
            Ray ray = new Ray(origen, direccion);

            if (Physics.Raycast(ray, out RaycastHit hit, rango, mascaraDisparo))
            {
                if (hit.collider.CompareTag("Player")) continue;

                // --- CHISPA EN EL ENEMIGO ---
                if (efectoImpactoPrefab != null)
                {
                    GameObject chispa = Instantiate(efectoImpactoPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                    Destroy(chispa, 0.6f);
                }

                HealthSystem hs = hit.collider.GetComponent<HealthSystem>();
                if (hs != null) hs.TakeDamage(danio);
            }
        }

        if (CurrentAmmo <= 0) Recargar();
    }

    public void Recargar()
    {
        if (recargando || CurrentAmmo == MaxAmmo) return;
        StartCoroutine(RutinaRecarga());
    }

    IEnumerator RutinaRecarga()
    {
        recargando = true;
        if (AudioManager.Instance != null && itemData != null)
            AudioManager.Instance.PlaySFX(itemData.reloadSound);
        float tiempo = itemData != null ? itemData.reloadTime : 1.5f;
        if (tiempo <= 0) tiempo = 1.5f;
        yield return new WaitForSeconds(tiempo);
        CurrentAmmo = MaxAmmo;
        NotificarBalas();
        recargando = false;
    }

    public void SetupWeapon(ItemData data)
    {
        itemData = data;
        if (itemData != null)
        {
            MaxAmmo = itemData.weaponMaxAmmo;
            CurrentAmmo = MaxAmmo;
            NotificarBalas();
        }
    }

    public void SetItemData(ItemData data) => SetupWeapon(data);
    public ItemData GetItemData() => itemData;
}