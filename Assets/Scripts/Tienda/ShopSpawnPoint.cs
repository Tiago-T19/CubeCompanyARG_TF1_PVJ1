using UnityEngine;

// Marca dónde aparece el jugador al entrar a la tienda.
// Ponelo en un objeto vacío dentro de la escena Tienda, mirando hacia adentro.
public class ShopSpawnPoint : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position + Vector3.up, 0.5f);
        Gizmos.DrawRay(transform.position + Vector3.up, transform.forward * 1.5f);
    }
}
