using TMPro;
using UnityEngine;
using System;

public class PasswordDoor : MonoBehaviour
{
    [Header("Contraseña")]
    [SerializeField] private string correctPassword;
    [SerializeField] private GameObject door;

    [Header("Interfaz")]
    [SerializeField] private GameObject passwordPanel;
    [SerializeField] private TMP_InputField passwordInput;

    [Header("Jugador")]
    [SerializeField] private CameraFollow cameraFollow;
    [SerializeField] private PlayerMovement playerMovement;

    public static event Action OnPasswordCompleted; // Avisa cuando el puzzle se completa

    public void OpenPasswordPanel()
    {
        // el inventario no se abre mientras se resuelve un puzzle, por lo que lo cerramos si está abierto
        InventoryController inventory =
    FindFirstObjectByType<InventoryController>();

        if (inventory != null)
            inventory.CloseInventory();


        //El puzzle solo se puede abrir si el juego está en estado de jugando
        if (GameManager.Instance.estadoActual != EstadoJuego.Jugando)
            return;

        // Activamos el panel de contraseña y avisamos al GameManager que estamos resolviendo un puzzle
        GameManager.Instance.resolviendoPuzzle = true;
        passwordPanel.SetActive(true);
        // Liberamos el cursor para utilizar la interfaz
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        cameraFollow.enabled = false; // Desactivamos el script de seguimiento de la camara
        playerMovement.IsStop = true; // Desactivamos el movimiento del jugador
    }

    public void ClosePasswordPanel()
    {
        GameManager.Instance.resolviendoPuzzle = false;
        passwordPanel.SetActive(false);

        // Volvemos a bloquear el cursor para jugar
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        cameraFollow.enabled = true; // Activamos el script de seguimiento de la camara
        playerMovement.IsStop = false; // Activamos el movimiento del jugador
    }

    public void CheckPassword()
    {
        // Obtenemos el texto que escribió el jugador
        string enteredPassword = passwordInput.text;

        // Comprobamos si coincide con la contraseña correcta
        if (enteredPassword == correctPassword)
        {
            Debug.Log("<color=green>¡Contraseña correcta!</color>");
            OnPasswordCompleted?.Invoke();

            // Aqui puedes agregar la logica para abrir la puerta o realizar cualquier accion
            door.SetActive(false);

            // Cerramos la interfaz
            ClosePasswordPanel();
        }
        else
        {
            // Mostramos el mensaje de error
            Debug.Log("<color=red>¡Contraseña incorrecta!</color>");
        }
    }
}
