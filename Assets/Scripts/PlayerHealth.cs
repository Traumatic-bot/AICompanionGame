using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 100;

    [SerializeField]
    private int currentHealth = 100;

    public int CurrentHealth
    {
        get { return currentHealth; }
    }


    void Start()
    {
        currentHealth = maxHealth;
    }


    public void TakeDamage(int amount)
    {
        currentHealth =
            Mathf.Clamp(
                currentHealth - amount,
                0,
                maxHealth
            );

        Debug.Log(
            "Player health: " +
            currentHealth +
            "/" +
            maxHealth
        );
    }


    public void Heal(int amount)
    {
        currentHealth =
            Mathf.Clamp(
                currentHealth + amount,
                0,
                maxHealth
            );

        Debug.Log(
            "Player health: " +
            currentHealth +
            "/" +
            maxHealth
        );
    }


    [ContextMenu("Test - Take 25 Damage")]
    private void TestDamage()
    {
        TakeDamage(25);
    }


    [ContextMenu("Test - Heal 25")]
    private void TestHeal()
    {
        Heal(25);
    }
}