using UnityEngine;
using UnityEngine.AI;

public class CompanionFollow : MonoBehaviour
{
    public Transform player;

    [Header("Follow Settings")]
    public float followDistance = 3f;
    public float rotationSpeed = 5f;

    private NavMeshAgent agent;
    private DialogueManager dialogueManager;
    private Animator animator;


    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        animator = GetComponentInChildren<Animator>();

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

        bool isTalking =
            dialogueManager != null &&
            dialogueManager.IsDialogueOpen();

        // Always update talking parameter first
        if (animator != null)
        {
            animator.SetBool(
                "IsTalking",
                isTalking
            );
        }

        // While talking, stop and face the player
        if (isTalking)
        {
            agent.ResetPath();

            if (animator != null)
            {
                animator.SetBool(
                    "IsWalking",
                    false
                );
            }

            FacePlayer();

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

        if (animator != null)
        {
            bool isWalking =
                agent.velocity.magnitude > 0.1f;

            animator.SetBool(
                "IsWalking",
                isWalking
            );
        }
    }


    private void FacePlayer()
    {
        Vector3 direction =
            player.position -
            transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(
                    direction
                );

            transform.rotation =
                Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed *
                    Time.deltaTime
                );
        }
    }
}