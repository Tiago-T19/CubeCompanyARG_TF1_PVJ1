using UnityEngine;

// Sonidos de la tienda. Se suscribe a los eventos de ShopManager,
// así ShopManager no sabe nada de audio.
// Ponelo en cualquier objeto de la escena Tienda (con un AudioSource)
// y asigná el Output del AudioSource al grupo SFX del AudioMixer.
[RequireComponent(typeof(AudioSource))]
public class ShopAudio : MonoBehaviour
{
    [SerializeField] private AudioClip sonidoCompra;
    [SerializeField] private AudioClip sonidoError;

    private AudioSource fuente;

    private void Awake() => fuente = GetComponent<AudioSource>();

    private void OnEnable()
    {
        ShopManager.OnCompraRealizada += SonarCompra;
        ShopManager.OnCompraFallida += SonarError;
    }

    private void OnDisable()
    {
        ShopManager.OnCompraRealizada -= SonarCompra;
        ShopManager.OnCompraFallida -= SonarError;
    }

    private void SonarCompra(ShopItemData item)
    {
        if (sonidoCompra) fuente.PlayOneShot(sonidoCompra);
    }

    private void SonarError(ShopItemData item, string motivo)
    {
        if (sonidoError) fuente.PlayOneShot(sonidoError);
    }
}
