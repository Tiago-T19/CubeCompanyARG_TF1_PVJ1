using UnityEngine;
using System.Collections;

public class ControlPuertas : MonoBehaviour
{
    [Header("Configuración de Apertura")]
    [SerializeField] private Transform puertaIzquierda;
    [SerializeField] private Transform puertaDerecha;
    [SerializeField] private float distanciaApertura = 1.5f;
    [SerializeField] private float velocidadPuerta = 2f;

    private Vector3 posLocalIzqCerrada;
    private Vector3 posLocalDerCerrada;
    private Vector3 posLocalIzqAbierta;
    private Vector3 posLocalDerAbierta;

    void Start()
    {
        if (puertaIzquierda != null && puertaDerecha != null)
        {
            posLocalIzqCerrada = puertaIzquierda.localPosition;
            posLocalDerCerrada = puertaDerecha.localPosition;

            posLocalIzqAbierta = posLocalIzqCerrada + new Vector3(-distanciaApertura, 0, 0);
            posLocalDerAbierta = posLocalDerCerrada + new Vector3(distanciaApertura, 0, 0);
        }
    }

    public void AbrirPuertas()
    {
        StopAllCoroutines();
        StartCoroutine(MoverPuertas(posLocalIzqAbierta, posLocalDerAbierta));
    }

    public void CerrarPuertas()
    {
        StopAllCoroutines();
        StartCoroutine(MoverPuertas(posLocalIzqCerrada, posLocalDerCerrada));
    }

    private IEnumerator MoverPuertas(Vector3 destinoIzq, Vector3 destinoDer)
    {
        float tiempo = 0;
        Vector3 inicioIzq = puertaIzquierda.localPosition;
        Vector3 inicioDer = puertaDerecha.localPosition;

        while (tiempo < 1)
        {
            tiempo += Time.deltaTime * velocidadPuerta;
            puertaIzquierda.localPosition = Vector3.Lerp(inicioIzq, destinoIzq, tiempo);
            puertaDerecha.localPosition = Vector3.Lerp(inicioDer, destinoDer, tiempo);
            yield return null;
        }
    }
}