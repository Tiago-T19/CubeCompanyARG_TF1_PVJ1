using TMPro;
using UnityEngine;
using System;

public class PasswordDoor : MonoBehaviour
{
    [Header("Contraseña")]
    [SerializeField] private string correctPassword;

    [Header("Interfaz")]
    [SerializeField] private GameObject passwordPanel;
    [SerializeField] private TMP_InputField passwordInput;

    [Header("Camara")]
    [SerializeField] private CameraFollow cameraFollow;

    public event Action OnPasswordCompleted; // Avisa cuando el puzzle se completa

    public void OpenPasswordPanel()
    {
        passwordPanel.SetActive(true);
        // Liberamos el cursor para utilizar la interfaz
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Desactivamos el script de seguimiento de la camara
        cameraFollow.enabled = false;
    }

    public void ClosePasswordPanel()
    {
        passwordPanel.SetActive(false);

        // Volvemos a bloquear el cursor para jugar
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Activamos el script de seguimiento de la camara
        cameraFollow.enabled = true;
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
