using System;
using UnityEngine;

public class MissionLvl2 : MonoBehaviour
{
    public enum MissionStep // Flujo de la mision
    {
        NotStarted, // La mision aun no empieza
        FindVirus, // Seguir camino al laberinto (VirusObject)
        RestorePower, // Minijuego (WirePuzzle)
        ExitMaze, // Salir del laberinto
        EscapeCity, // Escapar de la ciudad 
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
        VirusObject.OnVirusObjectCollected += VirusCollected;
        WirePuzzle.OnPuzzleCompleted += PowerRestored;
    }

    private void OnDisable()
    {
        // Dejamos de escuchar los eventos
        VirusObject.OnVirusObjectCollected -= VirusCollected;
        WirePuzzle.OnPuzzleCompleted -= PowerRestored;
    }

    private void Start()
    {
        // Avisamos el objetivo inicial
        OnMissionStepChanged?.Invoke(currentStep);
    }
    public void StartMission()
    {
        if (currentStep != MissionStep.NotStarted) return;

        missionPanel.SetActive(true);

        ChangeStep(MissionStep.FindVirus);
    }

    private void VirusCollected() //PowerRestored
    {
        if (currentStep != MissionStep.FindVirus) return;

        ChangeStep(MissionStep.RestorePower);
    }

    private void PowerRestored()
    {
        if (currentStep != MissionStep.RestorePower) return;

        ChangeStep(MissionStep.ExitMaze);
    }

    public void MazeExitReached()
    {
        if (currentStep != MissionStep.ExitMaze) return;

        ChangeStep(MissionStep.EscapeCity);
    }

    public void EscapeCityReached()
    {
        if (currentStep != MissionStep.EscapeCity) return;

        ChangeStep(MissionStep.Completed);
    }

    private void ChangeStep(MissionStep newStep)
    {
        currentStep = newStep;
        OnMissionStepChanged?.Invoke(currentStep);
    }
}
