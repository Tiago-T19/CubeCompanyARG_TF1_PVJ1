using System;
using UnityEngine;

public class MissionLvl1 : MonoBehaviour
{
    public enum MissionStep // Flujo de la mision
    {
        NotStarted, // La mision aun no empieza
        CollectData, // Recolectar los datos (MissionObject)
        FindPassword, // Puerta con contraseña
        ReachPlaza, // Llegar a la plaza
        EnterMaze, // Entrar al laberinto
        ExitMaze, // Salir del laberinto
        Password, // Ingresar la contraseña (PasswordDoor)
        ReachStadium, // Llegar al estadio
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

        ChangeStep(MissionStep.FindPassword);
    }

    public void FindPassword()
    {
        if (currentStep != MissionStep.FindPassword) return;

        ChangeStep(MissionStep.ReachPlaza);
    }

    public void PlazaReached()
    {
        if (currentStep != MissionStep.ReachPlaza) return;

        ChangeStep(MissionStep.EnterMaze);
    }

    public void MazeEntered()
    {
        if (currentStep != MissionStep.EnterMaze) return;

        ChangeStep(MissionStep.Password);
    }

    public void ExitMazeReached()
    {
        if (currentStep != MissionStep.ExitMaze) return;

        ChangeStep(MissionStep.Password);
    }

    private void PasswordAccepted()
    {
        if (currentStep != MissionStep.Password) return;

        ChangeStep(MissionStep.ReachStadium);
    }
    public void StadiumReached()
    {
        if (currentStep != MissionStep.ReachStadium) return;

        ChangeStep(MissionStep.Completed);
    }

    private void ChangeStep(MissionStep newStep)
    {
        currentStep = newStep;
        OnMissionStepChanged?.Invoke(currentStep);
    }
}

