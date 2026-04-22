using System.Collections;
using System.Xml.Serialization;
using UnityEngine;

public class EdgeOfIxtalProjectile : MonoBehaviour
{
    public Material brushMaterial;
    public Material EarthMaterial;
    public Material riverMaterial;

    private int baseDamage;

    public float speed;
    public float lifetime;

    public bool isBlue = false;

    public bool isEarth = false;

    public bool applyRoot = false;

    private void Start()
    {
        Debug.Log("Projectile initialized with base damage: " + baseDamage);
    }
    private void FixedUpdate()
    {
        transform.Translate(Vector3.forward * speed * Time.fixedDeltaTime);
    }

    public void Initialize(bool isBlue, ElementType elementType, int baseDamage)
    {
        this.isBlue = isBlue;
        this.baseDamage = baseDamage;

        //Debug.Log(isBlue ? "Blue Team Projectile created with base damage: " + baseDamage : "Red Team Projectile created with base damage: " + baseDamage);

        ElementAttributes(elementType);
        Destroy(gameObject, lifetime);
    }

    private void ElementAttributes(ElementType elementType)
    {
        Debug.Log("Applying element attributes for: " + elementType.ToString());

        switch (elementType)
        {
            case ElementType.Brush:
                this.gameObject.GetComponent<Renderer>().material = brushMaterial;
                break;
            case ElementType.River:
                this.gameObject.GetComponent<Renderer>().material = riverMaterial;
                applyRoot = true;
                break;
            case ElementType.Earth:
                this.gameObject.GetComponent<Renderer>().material = EarthMaterial;
                isEarth = true;
                break;
            default:
                break;
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        LayerMask targetLayer;

        if (isBlue)
            targetLayer = 11; // Target red team layer
        else
            targetLayer = 10; // Target blue team layer


        if (other.gameObject.layer == targetLayer)
        {
            //Debug.Log("Projectile collided with target: " + other.gameObject.name);

            if (other.gameObject.TryGetComponent<TargetDummy>(out TargetDummy targetStats))
            {
                //Debug.Log("Projectile hit player with current currentHealth: " + targetStats.currentHealth);

                if (isEarth && targetStats.currentHealth <= targetStats .maxHealth / 2)
                {
                    baseDamage += (int)(baseDamage * 0.75f); // Bonus damage for Earth element
                    //Debug.Log("Earth element bonus applied! New base damage: " + baseDamage);
                }

                if (applyRoot)
                {
                    //Debug.Log("Applying root effect to target: " + other.gameObject.name);
                    targetStats.StartCoroutine(targetStats.RootCoroutine());
                }

                targetStats.ChangeHealth(-baseDamage);
            }
            Destroy(gameObject);
        }
    }
}
