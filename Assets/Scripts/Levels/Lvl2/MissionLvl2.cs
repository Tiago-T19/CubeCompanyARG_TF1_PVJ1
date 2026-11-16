using System;
using UnityEngine;

public class MissionLvl2 : MonoBehaviour
{
    public enum MissionStep // Flujo de la mision
    {
        NotStarted, // La mision aun no empieza
        CollectData, // Recolectar los datos (MissionObject)
        FindVirus, // Seguir camino al hospital
        ExitHospital, // Salir del hospital
        Completed // Nivel 2 completado
    }

    [Header("Estado de la mision")]
    [SerializeField] private GameObject missionPanel; // Panel de la mision
    [SerializeField] private MissionStep currentStep; // Estado actual de la mision
    public MissionStep CurrentStep => currentStep;

    public static event Action<MissionStep> OnMissionStepChanged;

    private void OnEnable()
    {
        // Escuchamos los eventos
        MissionManager.OnAllObjectsCollected += AllDataCollected;
        VirusObject.OnVirusObjectCollected += VirusCollected;
    }

    private void OnDisable()
    {
        // Dejamos de escuchar los eventos
        MissionManager.OnAllObjectsCollected -= AllDataCollected;
        VirusObject.OnVirusObjectCollected -= VirusCollected;
    }

    private void Start()
    {
        // Avisamos el objetivo inicial
        OnMissionStepChanged?.Invoke(currentStep);
    }
    public void StartMission()
    {
        if (currentStep != MissionStep.NotStarted) return;

        Debug.Log("<color=white><b>(1) Recolecta los cinco datos</b></color>");
        missionPanel.SetActive(true);

        ChangeStep(MissionStep.CollectData);
    }

    private void AllDataCollected()
    {
        if (currentStep != MissionStep.CollectData) return;

        Debug.Log("<color=white><b>(2) Encuentra el frasco del virus</b></color>");

        ChangeStep(MissionStep.FindVirus);
    }

    private void VirusCollected()
    {
        if (currentStep != MissionStep.FindVirus) return;

        Debug.Log("<color=white><b>(3) Encuentra la salida del hospital</b></color>");

        ChangeStep(MissionStep.ExitHospital);
    }

    public void HospitalExitReached()
    {
        if (currentStep != MissionStep.ExitHospital) return;

        Debug.Log("<color=green><b>(4) ¡Nivel 2 completado!</b></color>");

        ChangeStep(MissionStep.Completed);
    }

    private void ChangeStep(MissionStep newStep)
    {
        currentStep = newStep;
        OnMissionStepChanged?.Invoke(currentStep);
    }
}
