using UnityEngine;

public class ObjectiveTrigger : MonoBehaviour
{
    public string newObjective;

    private GameContextManager gameContextManager;

    void Start()
    {
        gameContextManager =
            FindFirstObjectByType<GameContextManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (gameContextManager != null)
        {
            gameContextManager.SetObjective(
                newObjective
            );
        }
    }
}