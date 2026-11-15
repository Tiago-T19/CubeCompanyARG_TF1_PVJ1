using UnityEngine;
using System.Collections;

public class Elevador : MonoBehaviour
{
    [Header("Puntos de Destino")]
    [SerializeField] private Transform puntoInferior;
    [SerializeField] private Transform puntoSuperior;

    [Header("Configuración")]
    [SerializeField] private float velocidad = 3f;
    [SerializeField] private ControlPuertas controlPuertas;

    private Transform objetivoActual;
    private bool enMovimiento = false;

    void Start()
    {
        objetivoActual = puntoSuperior;
    }

    public void LlamarOActivarAscensor()
    {
        if (!enMovimiento)
        {
            StartCoroutine(SecuenciaAscensor());
        }
    }

    IEnumerator SecuenciaAscensor()
    {
        enMovimiento = true;

        if (controlPuertas != null)
        {
            controlPuertas.CerrarPuertas();
            yield return new WaitForSeconds(1.5f);
        }

        while (Vector3.Distance(transform.position, objetivoActual.position) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, objetivoActual.position, velocidad * Time.deltaTime);
            yield return null;
        }

        if (controlPuertas != null)
        {
            controlPuertas.AbrirPuertas();
        }

        objetivoActual = (objetivoActual == puntoSuperior) ? puntoInferior : puntoSuperior;

        enMovimiento = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.transform.SetParent(transform);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.transform.SetParent(null);
        }
    }
}