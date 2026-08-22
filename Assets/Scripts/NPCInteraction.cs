using UnityEngine;

public class NPCInteraction : MonoBehaviour
{
    public string npcName = "Arin";

    [TextArea]
    public string dialogueMessage =
        "Hello traveller. It's dangerous outside tonight.";

    private DialogueManager dialogueManager;

    void Start()
    {
        dialogueManager = FindFirstObjectByType<DialogueManager>();
    }

    public void Interact()
    {
        if (dialogueManager != null)
        {
            dialogueManager.OpenDialogue(
                npcName,
                dialogueMessage
            );
        }
    }
}