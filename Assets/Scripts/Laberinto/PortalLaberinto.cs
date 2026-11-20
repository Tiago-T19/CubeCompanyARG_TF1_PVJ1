using UnityEngine;

// Portal: ponelo en un objeto de la escena (ej. una esfera morada) y elegí el Destino en el Inspector.
// Al acercarte aparece "Presioná E" y al apretar E teletransporta.
// Hace falta un portal IrAlLaberinto en Level1 y uno VolverAlNivel en Laberinto.
public class PortalLaberinto : MonoBehaviour
{
    public enum Destino { IrAlLaberinto, VolverAlNivel }

    [SerializeField] private Destino destino = Destino.IrAlLaberinto;
    [SerializeField] private float radioInteraccion = 2.5f;
    [SerializeField] private KeyCode teclaInteraccion = KeyCode.E;

    public Destino DestinoDelPortal => destino;

    private Transform jugador;
    private bool jugadorCerca;

    private void Update()
    {
        if (jugador == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) jugador = p.transform;
        }
        if (jugador == null) { jugadorCerca = false; return; }

        Vector3 d = jugador.position - transform.position;
        d.y = 0f;
        jugadorCerca = d.magnitude <= radioInteraccion && !LaberintoTransition.Ocupado;

        if (jugadorCerca && Input.GetKeyDown(teclaInteraccion))
        {
            if (destino == Destino.IrAlLaberinto) LaberintoTransition.Instance.IrAlLaberinto();
            else LaberintoTransition.Instance.VolverAlNivel();
        }
    }

    private void OnGUI()
    {
        if (!jugadorCerca) return;
        string texto = destino == Destino.IrAlLaberinto ? "Presioná E para entrar al LABERINTO" : "Presioná E para volver al NIVEL 1";
        GUIStyle estilo = new GUIStyle(GUI.skin.box) { fontSize = 22, alignment = TextAnchor.MiddleCenter };
        estilo.normal.textColor = Color.white;
        float w = 460f, h = 50f;
        GUI.Box(new Rect((Screen.width - w) / 2f, Screen.height - 140f, w, h), texto, estilo);
    }
}
