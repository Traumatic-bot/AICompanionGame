using UnityEngine;
using UnityEngine.AI;

public class CompanionFollow : MonoBehaviour
{
    public Transform player;

    [Header("Follow Settings")]
    public float followDistance = 3f;

    private NavMeshAgent agent;
    private DialogueManager dialogueManager;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        dialogueManager =
            FindFirstObjectByType<DialogueManager>();

        if (player == null)
        {
            GameObject playerObject =
                GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }
    }

    void Update()
    {
        if (agent == null || player == null)
            return;

        // Don't move while the player is talking to Arin
        if (dialogueManager != null &&
            dialogueManager.IsDialogueOpen())
        {
            agent.ResetPath();
            return;
        }

        float distance =
            Vector3.Distance(
                transform.position,
                player.position
            );

        if (distance > followDistance)
        {
            agent.SetDestination(
                player.position
            );
        }
        else
        {
            agent.ResetPath();
        }
    }
}