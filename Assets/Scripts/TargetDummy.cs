using UnityEngine;
using UnityEngine.AI;

public class TargetDummy : MonoBehaviour
{
    public int maxHealth = 500;
    public int health;

    public float resetTime = 5f;
    private float resetTimer;

    private NavMeshAgent agent;


    bool isBlue;

    void Start()
    {
        health = maxHealth;
        agent = GetComponent<NavMeshAgent>();

        SnapToMesh();
        resetTimer = resetTime;
    }

    private void FixedUpdate()
    {
        if (health < maxHealth)
        {
            resetTimer -= Time.fixedDeltaTime;

            if (resetTimer <= 0)
            {
                ResetHealth();
                resetTimer = resetTime;
            }
        }
    }

    public void ChangeHealth(int amount)
    {
        health += amount;

        if (health > maxHealth) health = maxHealth;
        if (health < 0) health = 0;

        //Debug.Log($"Target Dummy health: {health}");
    }

    public void ResetHealth()
    {
        health = maxHealth;
    }

    private void SnapToMesh()
    {
        agent.Warp(transform.position);
    }
}
