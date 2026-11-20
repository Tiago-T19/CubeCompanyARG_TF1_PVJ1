using UnityEngine;
using System;
public class MissionManager : MonoBehaviour
{
    [Header("Mision")]
    [SerializeField] private int totalMissionObjects = 5; // Total requerido para completar la mision

    private int recoveredObjects; // Objetos recogidos
    public int RecoveredObjects => recoveredObjects;
    public int TotalMissionObjects => totalMissionObjects;

    // Evento para el HUD: avisa (recogidos, total) cada vez que cambia
    public event Action<int, int> OnMissionProgressChanged;

    public static event Action OnAllObjectsCollected;

    private void OnEnable()
    {
        MissionObject.OnMissionObjectCollected += RegisterCollectedObject; // Nos suscribimos al evento de recoleccion
    }

    private void OnDisable()
    {
        MissionObject.OnMissionObjectCollected -= RegisterCollectedObject; // Nos desuscribimos del evento de recoleccion
    }

    private void Start()
    {
        // Avisa el estado inicial
        OnMissionProgressChanged?.Invoke(recoveredObjects, totalMissionObjects);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q)) ShowMissionStatus();
    }

    private void RegisterCollectedObject()
    {
        if (recoveredObjects >= totalMissionObjects) return;

        recoveredObjects++; // Suma un objeto recogido

        Debug.Log($"<color=purple>Objetos recuperados: <color=white>{recoveredObjects}/{totalMissionObjects}</color></color>");

        // Avisa al HUD
        OnMissionProgressChanged?.Invoke(recoveredObjects, totalMissionObjects);

        if (recoveredObjects >= totalMissionObjects)
        {
            Debug.Log("<color=green><b>¡Todos los datos recuperados!</b></color>");

            OnAllObjectsCollected?.Invoke();
        }
    }

    public void ShowMissionStatus()
    {
        Debug.Log($"<color=purple><b>Objetos: <color=white>{recoveredObjects}/{totalMissionObjects}</color></b></color>");
    }
}
