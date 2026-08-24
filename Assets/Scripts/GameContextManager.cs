using UnityEngine;

public class GameContextManager : MonoBehaviour
{
    [Header("Current Game State")]
    public string currentLocation = "Tavern";
    public string timeOfDay = "Night";
    private PlayerHealth playerHealth;
    public bool dangerNearby = false;

    public string currentObjective =
    "Reach the camp safely.";

    void Start()
    {
        playerHealth =
            FindFirstObjectByType<PlayerHealth>();
    }

    public string GetGameContext()
    {
        string healthContext =
            "Unknown";

        if (playerHealth != null)
        {
            healthContext =
                playerHealth.CurrentHealth +
                "/" +
                playerHealth.maxHealth;
        }

        string context =
            "Current location: " + currentLocation + "\n" +
            "Time of day: " + timeOfDay + "\n" +
            "Player health: " + healthContext + "\n" +
            "Danger nearby: " + dangerNearby + "\n" +
            "Current objective: " + currentObjective;

        return context;
    }


    public void SetLocation(string newLocation)
    {
        currentLocation = newLocation;

        Debug.Log(
            "Player location changed to: " +
            currentLocation
        );
    }

    public void SetDanger(bool isDangerous)
    {
        dangerNearby = isDangerous;

        Debug.Log(
            "Danger nearby: " +
            dangerNearby
        );
    }

    public void SetObjective(string newObjective)
    {
        currentObjective = newObjective;

        Debug.Log(
            "Objective changed to: " +
            currentObjective
        );
    }

}