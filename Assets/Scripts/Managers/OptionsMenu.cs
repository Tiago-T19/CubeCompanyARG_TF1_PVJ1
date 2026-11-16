using UnityEngine;
using UnityEngine.UI;

// Solo maneja los 3 sliders. NO maneja paneles.
// El cambio Panel_Inicio <-> Panel_Opciones ya lo hiciste vos con los botones
// y lo maneja UIManager con Btn_Opciones / Btn_VolverDeOpciones.
// Asi no hay doble manejo.

public class OptionsMenu : MonoBehaviour
{
    [Header("Sliders del Panel_Opciones - solo arrastrar, no tocar On Value Changed")]
    [SerializeField] private Slider sliderGeneral; // Master - Sonido General
    [SerializeField] private Slider sliderMusica;  // Music
    [SerializeField] private Slider sliderSFX;     // SFX

    void OnEnable()
    {
        if (AudioManager.Instance == null) return;
        if (sliderGeneral == null || sliderMusica == null || sliderSFX == null) return;

        // Cargar valores guardados, sin disparar el evento
        sliderGeneral.SetValueWithoutNotify(AudioManager.Instance.GetMasterVolume());
        sliderMusica.SetValueWithoutNotify(AudioManager.Instance.GetMusicVolume());
        sliderSFX.SetValueWithoutNotify(AudioManager.Instance.GetSFXVolume());

        // Limpiar y volver a conectar, para que no se duplique si entras 2 veces a Opciones
        sliderGeneral.onValueChanged.RemoveAllListeners();
        sliderMusica.onValueChanged.RemoveAllListeners();
        sliderSFX.onValueChanged.RemoveAllListeners();

        sliderGeneral.onValueChanged.AddListener(v => AudioManager.Instance.SetMasterVolume(v));
        sliderMusica.onValueChanged.AddListener(v => AudioManager.Instance.SetMusicVolume(v));
        sliderSFX.onValueChanged.AddListener(v => AudioManager.Instance.SetSFXVolume(v));
    }
}
