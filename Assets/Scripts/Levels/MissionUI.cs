using System.Collections;
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

    [Header("Cartel temporal (opcional)")]
    [Tooltip("El recuadro gris de la misión. Si lo dejás vacío, el cartel queda siempre visible")]
    [SerializeField] private GameObject panelMision;
    [SerializeField] private float segundosVisible = 5f;
    [SerializeField] private float duracionFundido = 0.5f;

    private CanvasGroup grupoPanel;
    private Coroutine rutinaCartel;

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

            case MissionLvl1.MissionStep.CollectData:
                UpdateObjective("<b>Recolecta los cinco datos por la ciudad</b>");

                if (progressText != null)
                {
                    progressText.gameObject.SetActive(true);

                    if (missionManager != null)
                    {
                        UpdateMissionProgress(missionManager.RecoveredObjects, missionManager.TotalMissionObjects);
                    }
                }

                break;

            case MissionLvl1.MissionStep.ReachMaze:
                UpdateObjective("<b>Encuentra el laberinto</b>");
                if (progressText != null)
                    progressText.gameObject.SetActive(false);
                break;

            case MissionLvl1.MissionStep.FindPassword:
                UpdateObjective("<b>Encuentra la forma de entrar al laberinto</b>");
                if (progressText != null)
                    progressText.gameObject.SetActive(false);
                break;

            case MissionLvl1.MissionStep.EnterMaze:
                UpdateObjective("<b>Entra al laberinto</b>");
                if (progressText != null)
                    progressText.gameObject.SetActive(false);
                break;

            case MissionLvl1.MissionStep.Completed:
                UpdateObjective("<b>¡Nivel completado!</b>");
                if (progressText != null)
                    progressText.gameObject.SetActive(false);
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

            case MissionLvl2.MissionStep.FindVirus:
                UpdateObjective("<b>Encuentra el frasco del virus</b>");

                if (progressText != null)
                {
                    progressText.gameObject.SetActive(true);

                    if (virusObject != null)
                    {
                        UpdateVirusProgress(virusObject.RecoveredObjects, virusObject.TotalVirusObjects);
                    }
                }

                break;

            case MissionLvl2.MissionStep.RestorePower:
                UpdateObjective("<b>Restaura la energia</b>");
                if (progressText != null)
                    progressText.gameObject.SetActive(false);
                break;

            case MissionLvl2.MissionStep.ExitMaze:
                UpdateObjective("<b>Sal del laberinto</b>");
                if (progressText != null)
                    progressText.gameObject.SetActive(false);
                break;

            case MissionLvl2.MissionStep.EscapeCity:
                UpdateObjective("<b>Escapa</b>");
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

        if (!string.IsNullOrEmpty(objective)) MostrarTemporal();
    }

    // Muestra el cartel y lo oculta solo después de unos segundos (corrutina con fundido)
    private void MostrarTemporal()
    {
        // Si no se asignó el panel, usa el padre del texto del objetivo (el recuadro de la misión)
        if (panelMision == null && objectiveText != null && objectiveText.transform.parent != null
            && objectiveText.transform.parent.GetComponent<Canvas>() == null)
            panelMision = objectiveText.transform.parent.gameObject;

        if (panelMision == null || !isActiveAndEnabled) return;

        if (grupoPanel == null)
        {
            grupoPanel = panelMision.GetComponent<CanvasGroup>();
            if (grupoPanel == null) grupoPanel = panelMision.AddComponent<CanvasGroup>();
        }

        if (rutinaCartel != null) StopCoroutine(rutinaCartel);
        rutinaCartel = StartCoroutine(RutinaCartel());
    }

    private IEnumerator RutinaCartel()
    {
        grupoPanel.alpha = 0f;

        float t = 0f;
        while (t < duracionFundido)
        {
            t += Time.deltaTime;
            grupoPanel.alpha = Mathf.Lerp(0f, 1f, t / duracionFundido);
            yield return null;
        }
        grupoPanel.alpha = 1f;

        yield return new WaitForSeconds(segundosVisible);

        t = 0f;
        while (t < duracionFundido)
        {
            t += Time.deltaTime;
            grupoPanel.alpha = Mathf.Lerp(1f, 0f, t / duracionFundido);
            yield return null;
        }
        grupoPanel.alpha = 0f;
        rutinaCartel = null;
    }

    private void UpdateMissionProgress(int recovered, int total)
    {
        if (progressText == null) return;

        progressText.text = $"Datos: {recovered}/{total}";
        MostrarTemporal(); // al juntar un dato vuelve a aparecer un rato
    }

    private void UpdateVirusProgress(int recovered, int total)
    {
        if (progressText == null) return;

        progressText.text = $"Frascos: {recovered}/{total}";
        MostrarTemporal();
    }
}
