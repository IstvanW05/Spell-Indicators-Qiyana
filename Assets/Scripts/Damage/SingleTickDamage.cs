using UnityEngine;

public class SingleTickDamage : MonoBehaviour
{
    public PlayerStats playerStats;

    bool isBlue;

    LayerMask targetLayer;

    int damage;

    public void Initialize()
    {
        Debug.Log("Initialized");

        isBlue = playerStats.isBlue;

        if (isBlue)
            targetLayer = 11; // Target red team layer
        else
            targetLayer = 10; // Target blue team layer
    }

    void SetDamage()
    {
        this.damage = playerStats.attackDamage;
        //Debug.Log("DamageSet: " + damage);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == targetLayer)
        {
            //Debug.Log("Collided with target: " + other.gameObject.name);

            SetDamage();

            if (other.gameObject.TryGetComponent<TargetDummy>(out TargetDummy targetStats))
            {
                //Debug.Log("Projectile hit player with current currentHealth: " + targetStats.currentHealth);

                targetStats.ChangeHealth(-damage);
            }
        }
    }
}
