using UnityEngine;

public class SingleTickDamage : MonoBehaviour
{
    public PlayerStats playerStats;

    LayerMask targetLayer;

    int damage;

    public void Initialize()
    {
        //Debug.Log("Initialized");

        this.targetLayer = playerStats.targetLayer;
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
