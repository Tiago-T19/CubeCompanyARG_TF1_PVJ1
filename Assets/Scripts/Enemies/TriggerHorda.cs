using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerHorda : MonoBehaviour
{
    [Header("Prefabs - 3 tipos")]
    [SerializeField] private GameObject zombieComunPrefab;
    [SerializeField] private GameObject zombieEspecialPrefab;
    [SerializeField] private GameObject jefeFinalPrefab;

    [Header("Donde spawnean")]
    [SerializeField] private Transform[] puntosSpawn;

    [Header("Rondas - espera entre rondas")]
    [SerializeField] private float delayEntreRondas = 2f;

    private List<GameObject> enemigosVivos = new List<GameObject>();
    private int rondaActual = 0;
    private bool yaActivado = false;
    private bool hordaTerminada = false;
    private bool esperandoSiguiente = false;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (yaActivado) return;
        if (hordaTerminada) return;

        yaActivado = true;
        rondaActual = 0;
        SpawnearRonda(1);
    }

    void SpawnearRonda(int numero)
    {
        rondaActual = numero;
        enemigosVivos.Clear();
        esperandoSiguiente = false;

        // Ronda 1: 3 comunes + 2 especiales
        // Ronda 2: 3 comunes + 2 especiales
        // Ronda 3: 2 jefes + 3 especiales
        if (numero == 1 || numero == 2)
        {
            Spawnear(zombieComunPrefab, 3);
            Spawnear(zombieEspecialPrefab, 2);
            Debug.Log($"[Horda] Ronda {numero} - 3 comunes + 2 especiales");
        }
        else if (numero == 3)
        {
            Spawnear(jefeFinalPrefab, 2);
            Spawnear(zombieEspecialPrefab, 3);
            Debug.Log($"[Horda] Ronda {numero} - 2 jefes finales + 3 especiales");
        }

        StartCoroutine(EsperarQueMueranTodos());
    }

    void Spawnear(GameObject prefab, int cantidad)
    {
        if (prefab == null) return;

        for (int i = 0; i < cantidad; i++)
        {
            Transform punto = puntosSpawn[Random.Range(0, puntosSpawn.Length)];
            GameObject enemigo = Instantiate(prefab, punto.position, punto.rotation);
            enemigosVivos.Add(enemigo);
        }
    }

    IEnumerator EsperarQueMueranTodos()
    {
        // Esperamos hasta que no quede ningun zombie vivo
        while (QuedanVivos())
        {
            yield return new WaitForSeconds(0.5f);
        }

        if (hordaTerminada) yield break;
        if (esperandoSiguiente) yield break;

        esperandoSiguiente = true;

        // Si era la 3ra, se termina, no aparece mas nada
        if (rondaActual >= 3)
        {
            hordaTerminada = true;
            yaActivado = false;
            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = false;
            Debug.Log("[Horda] Terminada la 3ra ronda, no aparecen mas");
            yield break;
        }

        // Si no era la 3ra, esperamos un toque y spawneamos la siguiente con Invoke
        // Este Invoke es el que pide la consigna del PDF
        Debug.Log($"[Horda] Ronda {rondaActual} limpia, en {delayEntreRondas}s viene la {rondaActual + 1}");
        Invoke(nameof(SpawnearSiguienteRonda), delayEntreRondas);
    }

    void SpawnearSiguienteRonda()
    {
        SpawnearRonda(rondaActual + 1);
    }

    bool QuedanVivos()
    {
        // Limpiamos los que ya fueron destruidos
        enemigosVivos.RemoveAll(e => e == null);

        foreach (GameObject e in enemigosVivos)
        {
            if (e == null) continue;

            HealthSystem hs = e.GetComponent<HealthSystem>();
            if (hs != null)
            {
                // Si tiene HealthSystem y no esta muerto, sigue vivo
                if (!hs.IsDead) return true;
            }
            else
            {
                // Si no tiene HealthSystem pero sigue activo, sigue vivo
                if (e.activeInHierarchy) return true;
            }
        }

        return false;
    }

    void OnDisable()
    {
        CancelInvoke();
        StopAllCoroutines();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        BoxCollider col = GetComponent<BoxCollider>();
        if (col != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(col.center, col.size);
        }
    }
}
