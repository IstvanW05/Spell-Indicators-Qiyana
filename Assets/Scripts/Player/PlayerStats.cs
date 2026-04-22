using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public bool isBlue = false;

    public int maxHealth = 100;
    public int currentHealth;

    public int attackDamage = 10;
    
    public float attackRange = 2.0f;

    public float attackSpeed = 1.0f; // number of attacks per second

    private void Awake()
    {
        if (isBlue)
        {
            gameObject.layer = 10; // BluePlayer layer
        }
        else
        {
            gameObject.layer = 11; // RedPlayer layer
        }
    }
}
