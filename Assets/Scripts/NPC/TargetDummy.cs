using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class TargetDummy : MonoBehaviour
{
    public int maxHealth = 500;
    public int currentHealth;

    public float resetTime = 5f;
    private float resetTimer;

    private NavMeshAgent agent;

    bool isBlue;

    public bool isRooted = false;
    float rootDuration = 3f;

    void Start()
    {
        currentHealth = maxHealth;
        agent = GetComponent<NavMeshAgent>();

        SnapToMesh();
        resetTimer = resetTime;
    }

    private void FixedUpdate()
    {
        if (currentHealth < maxHealth)
        {
            resetTimer -= Time.fixedDeltaTime;

            if (resetTimer <= 0)
            {
                ResetHealth();
                resetTimer = resetTime;
            }
        }
    }

    public IEnumerator RootCoroutine()
    {
        //Debug.Log("Target Dummy rooted for " + rootDuration + " seconds.");
        isRooted = true;
        agent.enabled = false;
        yield return new WaitForSeconds(rootDuration);
        isRooted = false;
        agent.enabled = true;
    }

    public void ChangeHealth(int amount)
    {
        currentHealth += amount;

        if (currentHealth > maxHealth) currentHealth = maxHealth;
        if (currentHealth < 0) currentHealth = 0;

        //Debug.Log($"Target Dummy took {Mathf.Abs(amount)} damage, currentHealth: {currentHealth}");
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
    }

    private void SnapToMesh()
    {
        agent.Warp(transform.position);
    }
}
