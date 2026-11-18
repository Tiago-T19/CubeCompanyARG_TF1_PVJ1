using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour, IInteractable
{
    // --- Todo objeto que tenga este componente sera interactuable ---
    [SerializeField] private bool isEnable = true; // Variable que indica si el objeto se puede interactuar
    [SerializeField] private UnityEvent onInteract; // Evento que se ejecuta al interactuar con el objeto
    public bool canInteract() => isEnable; // Comprobar si se puede interactuar con el objeto
    private Outline outline; // Componente de Outline para resaltar el objeto

    void Awake()
    {
        outline = gameObject.AddComponent<Outline>(); // Obtenemos el componente de Outline
        outline.OutlineMode = Outline.Mode.OutlineVisible; // Configuramos el modo de Outline
        outline.OutlineColor = Color.yellow; // Configuramos el color del Outline
        outline.OutlineWidth = 5f; // Configuramos el ancho del Outline
        outline.enabled = false; // Desactivamos el Outline al inicio
    }

    public void Interact()
    {
        onInteract.Invoke(); // Evento de interaccion
    }

    public void OnFocusGained()
    {
        outline.enabled = true; // Activamos el Outline cuando el jugador mira el objeto
    }

    public void OnFocusLost()
    {
        outline.enabled = false; // Desactivamos el Outline cuando el jugador deja de mirar el objeto
    }
}
