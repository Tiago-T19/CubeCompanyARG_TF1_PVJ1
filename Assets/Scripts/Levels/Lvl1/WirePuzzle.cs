using UnityEngine.UI;
using UnityEngine;
using System;

public class WirePuzzle : MonoBehaviour
{
    [Header("Panel de cables")]
    [SerializeField] private GameObject puzzlePanel;

    [Header("Puerta")]
    [SerializeField] private GameObject door;

    [Header("Jugador")]
    [SerializeField] private CameraFollow cameraFollow;
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Conectores")]
    [SerializeField] private Button[] leftButtons;
    [SerializeField] private Button[] rightButtons;

    [Header("Colores de los cables")]
    [SerializeField]
    private Color[] wireColors =
    {
        Color.red,
        Color.blue,
        Color.yellow,
        Color.green 
    };

    private int[] rightColorOrder; // Orden de los colores
    private int selectedLeftIndex = -1; // Guarda el conector seleccionado del lado izquierdo 
    private int matchedPairs; // Parejas contectadas
                    
    public static event Action OnPuzzleCompleted; // Avisa cuando el puzzle se completa


    private void Start()
    {
        InitializePuzzle();
    }

    public void OpenPuzzle()
    {
        // el inventario no se abre mientras se resuelve un puzzle, por lo que lo cerramos si está abierto
        InventoryController inventory =
    FindFirstObjectByType<InventoryController>();

        if (inventory != null)
            inventory.CloseInventory();

        //El puzzle solo se puede abrir si el juego está en estado de jugando
        if (GameManager.Instance.estadoActual != EstadoJuego.Jugando)
            return;

        GameManager.Instance.resolviendoPuzzle = true;
        puzzlePanel.SetActive(true);

        // Liberamos el cursor para utilizar la interfaz
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Desactivamos el script de seguimiento de la camara
        cameraFollow.enabled = false;
        playerMovement.IsStop = true;
    }

    public void ClosePuzzle()
    {
        GameManager.Instance.resolviendoPuzzle = false;
        puzzlePanel.SetActive(false);

        // Volvemos a bloquear el cursor para jugar
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Activamos el script de seguimiento de la camara
        cameraFollow.enabled = true;
        playerMovement.IsStop = false;
    }

    private void InitializePuzzle()
    {
        // Comprobamos que tengamos cuatro botones por lado
        if (leftButtons.Length != wireColors.Length || rightButtons.Length != wireColors.Length) return;

        rightColorOrder = new int[wireColors.Length]; // Orden de los colores

        // Asignamos los colores al lado izquierdo
        for (int i = 0; i < wireColors.Length; i++)
        {
            leftButtons[i].image.color = wireColors[i];

            rightColorOrder[i] = i; // Orden de los colores

            int index = i;

            // Detectamos cuando se selecciona un cable izquierdo
            leftButtons[i].onClick.AddListener(() => SelectLeftWire(index));
        }

        ShuffleRightWires(); // Mezclamos el orden 

        // Asignamos los colores y eventos al lado derecho
        for (int i = 0; i < rightButtons.Length; i++)
        {
            rightButtons[i].image.color = wireColors[rightColorOrder[i]];

            int index = i;

            rightButtons[i].onClick.AddListener(() => SelectRightWire(index));
        }
    }

    private void SelectLeftWire(int index)
    {
        // No permite seleccionar un cable ya conectado
        // interactable indica si el boton puede recibir interacciones
        if (!leftButtons[index].interactable) return; 

        selectedLeftIndex = index;
    }

    private void SelectRightWire(int index)
    {
        // Primero hay que seleccionar un cable izquierdo
        if (selectedLeftIndex == -1) return;

        // Comprobamos si los colores coinciden
        if (rightColorOrder[index] == selectedLeftIndex)
        {
            // Deshabilitamos ambos botones
            leftButtons[selectedLeftIndex].interactable = false;
            rightButtons[index].interactable = false;

            matchedPairs++;

            Debug.Log("<color=green>¡Cable conectado correctamente!</color>");

            // Comprobamos si se completaron todas las conexiones
            if (matchedPairs == wireColors.Length)
            {
                Debug.Log("<color=green><b>¡Energía restablecida!</b></color>");
                OnPuzzleCompleted?.Invoke();

                // Aqui puedes agregar la logica para abrir la puerta o realizar cualquier accion
                door.SetActive(false);

                ClosePuzzle();
            }
        }
        else
        {
            Debug.Log("<color=red>Conexión incorrecta</color>");
        }

        // Limpiamos la selección
        selectedLeftIndex = -1;
    }

    private void ShuffleRightWires()
    {
        // Mezcla los colores del lado derecho
        for (int i = rightColorOrder.Length - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);

            int temp = rightColorOrder[i];
            rightColorOrder[i] = rightColorOrder[randomIndex];
            rightColorOrder[randomIndex] = temp;
        }
    }
}
