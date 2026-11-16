using TMPro;
using UnityEngine;

public class MissionUI : MonoBehaviour
{
    [Header("Misiones")]
    [SerializeField] private MissionLvl1 missionLvl1;
    [SerializeField] private MissionLvl2 missionLvl2;

    [Header("Textos")]
    [SerializeField] private TextMeshProUGUI objectiveText;
    [SerializeField] private TextMeshProUGUI progressText;

    [Header("Progreso")]
    [SerializeField] private MissionManager missionManager;
    [SerializeField] private VirusObject virusObject;

    private void OnEnable()
    {
        // Escuchamos la misión del Nivel 1
        if (missionLvl1 != null)
        {
            MissionLvl1.OnMissionStepChanged += UpdateLvl1;
            UpdateLvl1(missionLvl1.CurrentStep);
        }

        // Escuchamos la misión del Nivel 2
        if (missionLvl2 != null)
        {
            MissionLvl2.OnMissionStepChanged += UpdateLvl2;
            UpdateLvl2(missionLvl2.CurrentStep);
        }

        if (missionManager != null)
        {
            missionManager.OnMissionProgressChanged += UpdateMissionProgress;

            UpdateMissionProgress(missionManager.RecoveredObjects, missionManager.TotalMissionObjects);
        }

        if (virusObject != null)
        {
            virusObject.OnVirusProgressChanged += UpdateVirusProgress;

            UpdateVirusProgress(virusObject.RecoveredObjects, virusObject.TotalVirusObjects);
        }
    }

    private void OnDisable()
    {
        MissionLvl1.OnMissionStepChanged -= UpdateLvl1;
        MissionLvl2.OnMissionStepChanged -= UpdateLvl2;

        if (missionManager != null)
        {
            missionManager.OnMissionProgressChanged -= UpdateMissionProgress;
        }

        if (virusObject != null)
        {
            virusObject.OnVirusProgressChanged -= UpdateVirusProgress;
        }
    }

    private void UpdateLvl1(MissionLvl1.MissionStep step)
    {
        if (progressText != null)
            progressText.gameObject.SetActive(false);

        switch (step)
        {
            case MissionLvl1.MissionStep.NotStarted:
                UpdateObjective("");
                break;

            case MissionLvl1.MissionStep.FindPassword:
                UpdateObjective("<b>(1) Encuentra una forma de pasar (1234)</b>");
                break;

            case MissionLvl1.MissionStep.ReachHospital:
                UpdateObjective("<b>(2) Dirígete al hospital</b>");
                break;

            case MissionLvl1.MissionStep.RestorePower:
                UpdateObjective("<b>(3) Encuentra la forma de entrar al hospital</b>");
                break;

            case MissionLvl1.MissionStep.EnterHospital:
                UpdateObjective("<b>(4) Entra al hospital</b>");
                break;

            case MissionLvl1.MissionStep.Completed:
                UpdateObjective("<b>¡Nivel completado!</b>");
                break;
        }
    }

    private void UpdateLvl2(MissionLvl2.MissionStep step)
    {
        switch (step)
        {
            case MissionLvl2.MissionStep.NotStarted:
                UpdateObjective("");
                break;

            case MissionLvl2.MissionStep.CollectData:
                UpdateObjective("<b>(1) Recolecta los cinco datos</b>");
                if (progressText != null)
                {
                    progressText.gameObject.SetActive(true);

                    if (missionManager != null)
                    {
                        UpdateMissionProgress(missionManager.RecoveredObjects, missionManager.TotalMissionObjects);
                    }
                }
                break;

            case MissionLvl2.MissionStep.FindVirus:
                UpdateObjective("<b>(2) Encuentra el frasco del virus</b>");
                if (progressText != null)
                {
                    progressText.gameObject.SetActive(true);

                    if (virusObject != null)
                    {
                        UpdateVirusProgress(virusObject.RecoveredObjects, virusObject.TotalVirusObjects);
                    }
                }
                break;

            case MissionLvl2.MissionStep.ExitHospital:
                UpdateObjective("<b>(3) Encuentra la salida del hospital</b>");
                if (progressText != null)
                    progressText.gameObject.SetActive(false);
                break;

            case MissionLvl2.MissionStep.Completed:
                UpdateObjective("<b>¡Nivel completado!</b>");
                if (progressText != null)
                    progressText.gameObject.SetActive(false);
                break;
        }
    }

    private void UpdateObjective(string objective)
    {
        if (objectiveText != null)
            objectiveText.text = objective;
    }

    private void UpdateMissionProgress(int recovered, int total)
    {
        if (progressText == null) return;

        progressText.text = $"Datos: {recovered}/{total}";
    }

    private void UpdateVirusProgress(int recovered, int total)
    {
        if (progressText == null) return;

        progressText.text = $"Frascos: {recovered}/{total}";
    }
}
