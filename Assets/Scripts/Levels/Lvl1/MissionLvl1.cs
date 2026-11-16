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

        Debug.Log("<color=white><b>(1) Encuentra una forma de pasar (1234)</b></color>");
        missionPanel.SetActive(true);

        ChangeStep(MissionStep.FindPassword);
    }

    private void PasswordAccepted()
    {
        if (currentStep != MissionStep.FindPassword) return;

        Debug.Log("<color=white><b>(2) ¡Se abrio la puerta! Puedes seguir tu camino</b></color>");

        ChangeStep(MissionStep.ReachHospital);
    }

    public void HospitalEntranceReached()
    {
        if (currentStep != MissionStep.ReachHospital) return;

        Debug.Log("<color=white><b>(3) Busca la forma de entrar al hospital</b></color>");

        ChangeStep(MissionStep.RestorePower);
    }

    private void PowerRestored()
    {
        if (currentStep != MissionStep.RestorePower) return;

        Debug.Log("<color=white><b>(4) ¡Se abrio la puerta! Puedes entrar</b></color>");

        ChangeStep(MissionStep.EnterHospital);
    }
    public void HospitalEntered()
    {
        if (currentStep != MissionStep.EnterHospital) return;

        Debug.Log("<color=green><b>(5) ¡Nivel 1 completado!</b></color>");

        ChangeStep(MissionStep.Completed);
    }

    private void ChangeStep(MissionStep newStep)
    {
        currentStep = newStep;
        OnMissionStepChanged?.Invoke(currentStep);
    }
}

