using UnityEngine;
using System;

public class VirusObject : MonoBehaviour
{
    [Header("Virus Object")]
    [SerializeField] private int points; // Puntos que se suman al recoger
    public static event Action OnVirusObjectCollected; // Evento que avisa cuando se recoge un objeto

    private bool isCollected; // Comprueba si ya fue recogido

    public void CollectVirusObject()
    {
        if (isCollected) return;

        isCollected = true; // Marcamos el objeto como recogido

        OnVirusObjectCollected?.Invoke(); // Avisamos que se recogio un objeto

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddPoints(points); // Suma los puntos
        }

        Destroy(gameObject); // Eliminamos el objeto del escenario
    }
}
