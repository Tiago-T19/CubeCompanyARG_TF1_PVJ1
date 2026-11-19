using UnityEngine;
using System;

public class VirusObject : MonoBehaviour
{
    [Header("Configuracion")]
    [SerializeField] private int totalVirusObjects = 1; // Total requerido para completar la mision
    [SerializeField] private int points; // Puntos que se suman al recoger

    [Header("Efecto")]
    [SerializeField] private GameObject vfx; // Efecto visual al recoger el objeto
    [SerializeField] private float vfxDuration = 3f;

    private int recoveredObjects; // Objetos recogidos
    public int RecoveredObjects => recoveredObjects;
    public int TotalVirusObjects => totalVirusObjects;

    public event Action<int, int> OnVirusProgressChanged;
    public static event Action OnVirusObjectCollected; // Evento que avisa cuando se recoge un objeto

    private bool isCollected; // Comprueba si ya fue recogido

    private void Start()
    {
        // Avisa el estado inicial
        OnVirusProgressChanged?.Invoke(recoveredObjects, totalVirusObjects);
    }

    public void CollectVirusObject()
    {
        if (isCollected || recoveredObjects >= totalVirusObjects) return;

        isCollected = true; // Marcamos el objeto como recogido

        recoveredObjects++; // Suma un objeto recogido

        if (vfx != null)
        {
            GameObject effect = Instantiate(vfx, transform.position, Quaternion.identity);
        }

        OnVirusObjectCollected?.Invoke(); // Avisamos que se recogio un objeto
        OnVirusProgressChanged?.Invoke(recoveredObjects, totalVirusObjects);


        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddPuntos(points); // Suma los puntos
        }

        gameObject.SetActive(false);
        Destroy(gameObject, vfxDuration); // Eliminamos el objeto del escenario
    }
}
