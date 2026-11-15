using UnityEngine;

public class BotonAscensor : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Elevador ascensor;

    [Header("Configuración")]
    [Tooltip("¿Este botón es para subir/llamar o tiene otra función?")]
    private bool esBotonDeAccion = true;

    private void OnMouseDown()
    {
        if (ascensor != null)
        {
            ascensor.LlamarOActivarAscensor();
        }
    }
}