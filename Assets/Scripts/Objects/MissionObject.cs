using UnityEngine;
using System;

public class MissionObject : MonoBehaviour
{
    [Header("Configuracion")]
    [SerializeField] private int points; // Puntos que se suman al recoger

    [Header("Efecto")]
    [SerializeField] private GameObject vfx; // Efecto visual al recoger el objeto
    [SerializeField] private float vfxDuration = 3f;

    public static event Action OnMissionObjectCollected; // Evento que avisa cuando se recoge un objeto de mision

    private bool isCollected; // Comprueba si ya fue recogido

    public void CollectMissionObject()
    {
        if (isCollected) return;

        isCollected = true; // Marcamos el objeto como recogido

        if (vfx != null)
        {
            GameObject effect = Instantiate(vfx, transform.position, Quaternion.identity);
        }

        OnMissionObjectCollected?.Invoke(); // Avisamos que se recogio un objeto

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddPuntos(points); // Suma los puntos
        }

        gameObject.SetActive(false);
        Destroy(gameObject, vfxDuration); // Eliminamos el objeto del escenario
    }
}
