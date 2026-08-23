using UnityEngine;

public class LocationTrigger : MonoBehaviour
{
    public string locationName;

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
            gameContextManager.SetLocation(
                locationName
            );
        }
    }
}