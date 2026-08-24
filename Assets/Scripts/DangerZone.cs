using UnityEngine;

public class DangerZone : MonoBehaviour
{
    private GameContextManager gameContextManager;
    private CompanionReactionManager reactionManager;

    void Start()
    {
        gameContextManager =
            FindFirstObjectByType<GameContextManager>();

        reactionManager =
            FindFirstObjectByType<CompanionReactionManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (gameContextManager != null)
        {
            gameContextManager.SetDanger(true);
        }

        if (reactionManager != null)
        {
            reactionManager.TriggerReaction(
                "The player has entered a dangerous dark forest."
            );
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (gameContextManager != null)
        {
            gameContextManager.SetDanger(false);
        }
    }
}