using System;
using UnityEngine;

public class MissionLvl2 : MonoBehaviour
{
    public enum MissionStep // Flujo de la mision
    {
        NotStarted, // La mision aun no empieza
        EnterMaze, // Entrar al laberinto
        FindVirusAndPassword, // Busca el virus y la contraseña (VirusObject)
        ExitMaze, // Salir del laberinto
        Completed // Nivel completado
    }

    [Header("Estado de la mision")]
    [SerializeField] private GameObject missionPanel; // Panel de la mision
    [SerializeField] private MissionStep currentStep; // Estado actual de la mision
    public MissionStep CurrentStep => currentStep;

    public static event Action<MissionStep> OnMissionStepChanged;

    private void OnEnable()
    {
        // Escuchamos los eventos
        VirusObject.OnVirusObjectCollected += VirusCollectedAndPasswordFound;
    }

    private void OnDisable()
    {
        // Dejamos de escuchar los eventos
        VirusObject.OnVirusObjectCollected -= VirusCollectedAndPasswordFound;
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

        ChangeStep(MissionStep.EnterMaze);
    }

    public void MazeEntered()
    {
        if (currentStep != MissionStep.EnterMaze) return;

        ChangeStep(MissionStep.FindVirusAndPassword);
    }

    private void VirusCollectedAndPasswordFound()
    {
        if (currentStep != MissionStep.FindVirusAndPassword) return;

        ChangeStep(MissionStep.ExitMaze);
    }

    public void ExitMazeReached()
    {
        if (currentStep != MissionStep.ExitMaze) return;

        ChangeStep(MissionStep.Completed);
    }

    private void ChangeStep(MissionStep newStep)
    {
        currentStep = newStep;
        OnMissionStepChanged?.Invoke(currentStep);
    }
}
