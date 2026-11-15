using System;
using UnityEngine;

public class MissionLvl1 : MonoBehaviour
{
    public enum MissionStep // Flujo de la mision
    {
        NotStarted, // La mision aun no empieza
        FindPassword, // Buscar contraseña (PasswordDoor)
        ReachHospital, // Seguir camino al hospital
        RestorePower, // Minijuego (WirePuzzle)
        EnterHospital, // Entrar al hospital
        Completed
    }

    [Header("Estado de la mision")]
    [SerializeField] private MissionStep currentStep; // Estado actual de la mision
    public MissionStep CurrentStep => currentStep;

    public static event Action<MissionStep> OnMissionStepChanged;

    private void OnEnable()
    {
        // Escuchamos cuando se completa
        PasswordDoor.OnPasswordCompleted += PasswordAccepted;
        WirePuzzle.OnPuzzleCompleted += PowerRestored;
    }

    private void OnDisable()
    {
        // Dejamos de escuchar los eventos
        PasswordDoor.OnPasswordCompleted -= PasswordAccepted;
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

        Debug.Log("<color=blue><b>Encuentra una forma de pasar</b></color>");

        ChangeStep(MissionStep.FindPassword);
    }

    private void PasswordAccepted()
    {
        if (currentStep != MissionStep.FindPassword) return;

        Debug.Log("<color=blue><b>¡Se abrio la puerta! Puedes seguir tu camino</b></color>");

        ChangeStep(MissionStep.ReachHospital);
    }

    public void HospitalEntranceReached()
    {
        if (currentStep != MissionStep.ReachHospital) return;

        Debug.Log("<color=blue><b>Busca la forma de entrar al hospital</b></color>");

        ChangeStep(MissionStep.RestorePower);
    }

    private void PowerRestored()
    {
        if (currentStep != MissionStep.RestorePower) return;

        Debug.Log("<color=blue><b>¡Se abrio la puerta! Puedes entrar</b></color>");

        ChangeStep(MissionStep.EnterHospital);
    }
    public void HospitalEntered()
    {
        if (currentStep != MissionStep.EnterHospital) return;

        Debug.Log("<color=green><b>¡Nivel 1 completado!</b></color>");

        ChangeStep(MissionStep.Completed);
    }

    private void ChangeStep(MissionStep newStep)
    {
        currentStep = newStep;

        Debug.Log($"<color=blue><b>Mission: {currentStep}</b></color>");

        OnMissionStepChanged?.Invoke(currentStep);
    }
}

