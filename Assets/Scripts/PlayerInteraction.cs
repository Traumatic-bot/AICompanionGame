using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    public float interactionDistance = 3f;
    public GameObject interactionPrompt;

    private Camera playerCamera;
    private NPCInteraction currentNPC;
    private DialogueManager dialogueManager;

    void Start()
    {
        playerCamera = GetComponentInChildren<Camera>();

        dialogueManager =
            FindFirstObjectByType<DialogueManager>();

        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }

    void Update()
    {
        // If dialogue is already open
        if (dialogueManager != null &&
            dialogueManager.IsDialogueOpen())
        {
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }

            if (Keyboard.current != null &&
                Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                dialogueManager.CloseDialogue();
            }

            return;
        }

        CheckForNPC();

        if (currentNPC != null &&
            Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            currentNPC.Interact();
        }
    }

    void CheckForNPC()
    {
        currentNPC = null;

        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            NPCInteraction npc =
                hit.collider.GetComponentInParent<NPCInteraction>();

            if (npc != null)
            {
                currentNPC = npc;
            }
        }

        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(currentNPC != null);
        }
    }
}