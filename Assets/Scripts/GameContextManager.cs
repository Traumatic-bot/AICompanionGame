using UnityEngine;

public class GameContextManager : MonoBehaviour
{
    [Header("Current Game State")]
    public string currentLocation = "Tavern";
    public string timeOfDay = "Night";


    public string GetGameContext()
    {
        string context =
            "Current location: " + currentLocation + "\n" +
            "Time of day: " + timeOfDay;

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
}