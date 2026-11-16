using UnityEngine;
using UnityEngine.Events;

public class MissionTrigger : MonoBehaviour
{
    [SerializeField] private UnityEvent onPlayerEnter;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        onPlayerEnter.Invoke();

        GetComponent<Collider>().enabled = false;
    }
}
