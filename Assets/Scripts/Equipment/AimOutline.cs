using UnityEngine;

public class AimOutline : MonoBehaviour
{
    [SerializeField] private float range = 100f;
    [SerializeField] private LayerMask enemyLayer;

    private Camera cam;
    private Outline currentOutline;

    private void Start()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        Ray ray = cam.ScreenPointToRay(
            new Vector3(Screen.width / 2, Screen.height / 2)
        );

        if (Physics.Raycast(ray, out RaycastHit hit, range, enemyLayer))
        {
            // Buscamos el Outline en el enemigo o sus padres
            Outline newOutline = hit.collider.GetComponentInParent<Outline>();

            if (newOutline == null)
            {
                // Si todavía no tiene Outline, lo agregamos
                newOutline = hit.collider.GetComponentInParent<Transform>().gameObject.AddComponent<Outline>();

                newOutline.OutlineMode = Outline.Mode.OutlineVisible;
                newOutline.OutlineColor = Color.red;
                newOutline.OutlineWidth = 5f;
            }

            // Si cambiamos de enemigo, apagamos el anterior
            if (currentOutline != newOutline)
            {
                RemoveOutline();
                currentOutline = newOutline;
            }

            currentOutline.enabled = true;
        }
        else
        {
            RemoveOutline();
        }
    }

    private void RemoveOutline()
    {
        if (currentOutline != null)
        {
            currentOutline.enabled = false;
            currentOutline = null;
        }
    }
}
