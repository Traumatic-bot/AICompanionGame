using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Collections;
using System.Text;

public class CompanionReactionManager : MonoBehaviour
{
    public TMP_Text subtitleText;
    public float subtitleDuration = 4f;

    private GameContextManager gameContextManager;
    private MemoryManager memoryManager;

    private const string reactionURL =
        "http://127.0.0.1:5000/react";


    [System.Serializable]
    private class ReactionRequest
    {
        public string gameEvent;
        public string gameContext;
        public string memories;
    }


    [System.Serializable]
    private class ReactionResponse
    {
        public string response;
        public string error;
    }


    void Start()
    {
        gameContextManager =
            FindFirstObjectByType<GameContextManager>();

        memoryManager =
            FindFirstObjectByType<MemoryManager>();

        if (subtitleText != null)
        {
            subtitleText.gameObject.SetActive(false);
        }
    }


    public void TriggerReaction(
        string eventDescription)
    {
        StartCoroutine(
            RequestReaction(eventDescription)
        );
    }


    private IEnumerator RequestReaction(
        string eventDescription)
    {
        ReactionRequest reactionRequest =
            new ReactionRequest();

        reactionRequest.gameEvent =
            eventDescription;

        if (gameContextManager != null)
        {
            reactionRequest.gameContext =
                gameContextManager.GetGameContext();
        }

        if (memoryManager != null)
        {
            reactionRequest.memories =
                memoryManager.GetMemoriesAsText();
        }

        string json =
            JsonUtility.ToJson(reactionRequest);

        byte[] bodyRaw =
            Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request =
            new UnityWebRequest(
                reactionURL,
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
                ReactionResponse response =
                    JsonUtility.FromJson<ReactionResponse>(
                        request.downloadHandler.text
                    );

                if (response != null &&
                    !string.IsNullOrWhiteSpace(
                        response.response))
                {
                    StartCoroutine(
                        ShowSubtitle(
                            response.response
                        )
                    );
                }
            }
            else
            {
                Debug.LogError(
                    "Reaction request failed: " +
                    request.error
                );
            }
        }
    }


    private IEnumerator ShowSubtitle(
        string message)
    {
        if (subtitleText == null)
            yield break;

        subtitleText.text =
            "Arin: " + message;

        subtitleText.gameObject.SetActive(true);

        yield return new WaitForSeconds(
            subtitleDuration
        );

        subtitleText.gameObject.SetActive(false);
    }
}