using System;
using UnityEngine;

public class MissionLvl1 : MonoBehaviour
{
    public enum MissionStep // Flujo de la mision
    {
        NotStarted, // La mision aun no empieza
        CollectData, // Recolectar los datos (MissionObject)
        ReachMaze, // Seguir camino al laberinto
        FindPassword, // Encontrar la contraseña para abrir la puerta (PasswordDoor)
        EnterMaze, // Entrar al laberinto
        Completed // Nivel 1 completado
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
        PasswordDoor.OnPasswordCompleted += PasswordAccepted;
    }

    private void OnDisable()
    {
        // Dejamos de escuchar los eventos
        MissionManager.OnAllObjectsCollected -= AllDataCollected;
        PasswordDoor.OnPasswordCompleted -= PasswordAccepted;
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

        ChangeStep(MissionStep.CollectData);
    }

    private void AllDataCollected()
    {
        if (currentStep != MissionStep.CollectData) return;

        ChangeStep(MissionStep.ReachMaze);
    }

    public void MazeEntranceReached()
    {
        if (currentStep != MissionStep.ReachMaze) return;

        Debug.Log("<color=white><b>(3) Busca la forma de entrar al hospital</b></color>");

        ChangeStep(MissionStep.FindPassword);
    }

    private void PasswordAccepted()
    {
        if (currentStep != MissionStep.FindPassword) return;

        Debug.Log("<color=white><b>(4) ¡Se abrio la puerta! Puedes entrar</b></color>");

        ChangeStep(MissionStep.EnterMaze);
    }
    public void MazeEntered()
    {
        if (currentStep != MissionStep.EnterMaze) return;

        Debug.Log("<color=green><b>(5) ¡Nivel 1 completado!</b></color>");

        ChangeStep(MissionStep.Completed);
    }

    private void ChangeStep(MissionStep newStep)
    {
        currentStep = newStep;
        OnMissionStepChanged?.Invoke(currentStep);
    }
}

