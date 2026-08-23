using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;
using System.Text;
using UnityEngine.UI;
using System.IO;

public class DialogueManager : MonoBehaviour
{
    public GameObject dialoguePanel;
    public TMP_Text npcNameText;
    public TMP_Text dialogueText;
    public TMP_InputField playerInput;
    public ScrollRect conversationScroll;

    private PlayerMovement playerMovement;
    private MemoryManager memoryManager;
    private GameContextManager gameContextManager;

    private bool dialogueOpen = false;
    private bool waitingForResponse = false;

    private string currentNPCName;
    private string conversationHistory = "";
    private bool conversationStarted = false;

    private const string chatURL =
        "http://127.0.0.1:5000/chat";


    [System.Serializable]
    private class ChatRequest
    {
        public string message;
        public string history;
        public string memories;
        public string gameContext;
    }


    [System.Serializable]
    private class ChatResponse
    {
        public string response;

        public string memoryAction;
        public string oldMemory;
        public string newMemory;

        public string error;
    }

    [System.Serializable]
    private class ConversationSaveData
    {
        public string history;
        public bool conversationStarted;
    }

    void Start()
    {
        memoryManager =
            FindFirstObjectByType<MemoryManager>();

        gameContextManager =
            FindFirstObjectByType<GameContextManager>();

        playerMovement =
            FindFirstObjectByType<PlayerMovement>();

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        LoadConversation();
    }


    void Update()
    {
        if (!dialogueOpen)
            return;

        // Close dialogue with Escape
        if (Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CloseDialogue();
            return;
        }

        // Send message with Enter
        if (playerInput != null &&
            Keyboard.current != null &&
            Keyboard.current.enterKey.wasPressedThisFrame &&
            !waitingForResponse)
        {
            SendMessageToNPC();
        }
    }


    public void OpenDialogue(
        string npcName,
        string message)
    {
        dialogueOpen = true;

        currentNPCName = npcName;

        dialoguePanel.SetActive(true);

        npcNameText.text = npcName;

        if (!conversationStarted)
        {
            conversationHistory =
                npcName + ":\n" +
                message;

            conversationStarted = true;
        }

        dialogueText.text =
            conversationHistory;

        StartCoroutine(ScrollToBottom());

        if (playerInput != null)
        {
            playerInput.text = "";
            playerInput.Select();
            playerInput.ActivateInputField();
        }

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        Cursor.lockState =
            CursorLockMode.None;

        Cursor.visible = true;
    }


    public void SendMessageToNPC()
    {
        if (playerInput == null)
            return;

        if (waitingForResponse)
            return;

        string playerMessage =
            playerInput.text.Trim();

        if (playerMessage == "")
            return;

        conversationHistory +=
            "\n\nYou:\n" +
            playerMessage;

        dialogueText.text =
            conversationHistory +
            "\n\n" +
            currentNPCName +
            " is thinking...";

        StartCoroutine(ScrollToBottom());

        playerInput.text = "";

        waitingForResponse = true;

        StartCoroutine(
            SendToAI(playerMessage)
        );
    }


    private IEnumerator SendToAI(
        string playerMessage)
    {
        ChatRequest chatRequest =
            new ChatRequest();

        chatRequest.message = playerMessage;
        chatRequest.history = conversationHistory;

        if (gameContextManager != null)
        {
            chatRequest.gameContext =
                gameContextManager.GetGameContext();
        }
        else
        {
            chatRequest.gameContext =
                "No game context available.";
        }

        string json =
            JsonUtility.ToJson(chatRequest);

        byte[] bodyRaw =
            Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request =
            new UnityWebRequest(
                chatURL,
                "POST"))
        {
            request.uploadHandler =
                new UploadHandlerRaw(bodyRaw);

            request.downloadHandler =
                new DownloadHandlerBuffer();

            request.SetRequestHeader(
                "Content-Type",
                "application/json");

            yield return request.SendWebRequest();

            if (request.result ==
                UnityWebRequest.Result.Success)
            {
                ChatResponse response =
                    JsonUtility.FromJson<ChatResponse>(
                        request.downloadHandler.text
                    );

                if (response != null && !string.IsNullOrEmpty(response.response))
                {
                    conversationHistory +=
                        "\n\n" +
                        currentNPCName +
                        ":\n" +
                        response.response;

                    if (memoryManager != null)
                    {
                        if (response.memoryAction == "ADD" &&
                            !string.IsNullOrWhiteSpace(response.newMemory))
                        {
                            memoryManager.AddMemory(
                                response.newMemory.Trim()
                            );
                        }

                        else if (response.memoryAction == "REPLACE" &&
                                 !string.IsNullOrWhiteSpace(response.newMemory))
                        {
                            memoryManager.ReplaceMemory(
                                response.oldMemory.Trim(),
                                response.newMemory.Trim()
                            );
                        }
                    }
                }
                else
                {
                    conversationHistory +=
                        "\n\n" +
                        currentNPCName +
                        ":\n" +
                        "[No response received]";
                }
            }
            else
            {
                Debug.LogError(
                    "AI request failed: " +
                    request.error +
                    "\n" +
                    request.downloadHandler.text
                );

                conversationHistory +=
                    "\n\n" +
                    currentNPCName +
                    ":\n" +
                    "[I can't seem to respond right now.]";
            }
        }

        waitingForResponse = false;

        dialogueText.text =
            conversationHistory;

        SaveConversation();

        StartCoroutine(ScrollToBottom());

        if (playerInput != null &&
            dialogueOpen)
        {
            playerInput.Select();
            playerInput.ActivateInputField();
        }
    }


    public void CloseDialogue()
    {
        dialogueOpen = false;

        if (playerInput != null)
        {
            playerInput.DeactivateInputField();
        }

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    public bool IsDialogueOpen()
    {
        return dialogueOpen;
    }

    private void SaveConversation()
    {
        ConversationSaveData saveData =
            new ConversationSaveData();

        saveData.history = conversationHistory;
        saveData.conversationStarted = conversationStarted;

        string json =
            JsonUtility.ToJson(saveData, true);

        string path =
            Path.Combine(
                Application.persistentDataPath,
                "conversation.json"
            );

        File.WriteAllText(path, json);

        Debug.Log(
            "Conversation saved to: " + path
        );
    }


    private void LoadConversation()
    {
        string path =
            Path.Combine(
                Application.persistentDataPath,
                "conversation.json"
            );

        if (!File.Exists(path))
            return;

        string json =
            File.ReadAllText(path);

        ConversationSaveData saveData =
            JsonUtility.FromJson<ConversationSaveData>(
                json
            );

        if (saveData == null)
            return;

        conversationHistory =
            saveData.history;

        conversationStarted =
            saveData.conversationStarted;

        Debug.Log("Conversation loaded.");
    }

    [ContextMenu("Clear Conversation History")]
    public void ClearConversationHistory()
    {
        conversationHistory = "";
        conversationStarted = false;

        string path =
            Path.Combine(
                Application.persistentDataPath,
                "conversation.json"
            );

        if (File.Exists(path))
        {
            File.Delete(path);
        }

        if (dialogueText != null)
        {
            dialogueText.text = "";
        }

        Debug.Log(
            "Conversation history cleared. Long-term memories were kept."
        );
    }

    private IEnumerator ScrollToBottom()
    {
        // Wait until Unity has updated the UI layout
        yield return null;

        Canvas.ForceUpdateCanvases();

        if (conversationScroll != null)
        {
            conversationScroll.verticalNormalizedPosition = 0f;
        }
    }
}