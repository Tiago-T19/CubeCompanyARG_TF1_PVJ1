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

        Debug.Log("<color=white><b>(1) Recolecta los cinco datos</b></color>");
        missionPanel.SetActive(true);

        ChangeStep(MissionStep.FindVirus);
    }

    private void VirusCollected() //PowerRestored
    {
        if (currentStep != MissionStep.FindVirus) return;

        Debug.Log("<color=white><b>(2) Encuentra el frasco del virus</b></color>");

        ChangeStep(MissionStep.RestorePower);
    }

    private void PowerRestored()
    {
        if (currentStep != MissionStep.RestorePower) return;

        Debug.Log("<color=white><b>(3) Encuentra la salida del hospital</b></color>");

        ChangeStep(MissionStep.ExitMaze);
    }

    public void MazeExitReached()
    {
        if (currentStep != MissionStep.ExitMaze) return;

        Debug.Log("<color=green><b>(4) ¡Nivel 2 completado!</b></color>");

        ChangeStep(MissionStep.EscapeCity);
    }

    public void EscapeCityReached()
    {
        if (currentStep != MissionStep.EscapeCity) return;

        Debug.Log("<color=green><b>(4) ¡Nivel 2 completado!</b></color>");

        ChangeStep(MissionStep.Completed);
    }

    private void ChangeStep(MissionStep newStep)
    {
        currentStep = newStep;
        OnMissionStepChanged?.Invoke(currentStep);
    }
}
