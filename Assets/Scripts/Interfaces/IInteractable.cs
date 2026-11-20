using UnityEngine;

public interface IInteractable
{
    Transform transform { get; } // Posicion y rotacion del objeto
    bool canInteract(); // Comprobar si se puede interactuar
    void Interact(); // Interaccion
    void OnFocusGained(); // Cuando el jugador mira el objeto
    void OnFocusLost(); // Cuando el jugador deja de mirar el objeto
}
