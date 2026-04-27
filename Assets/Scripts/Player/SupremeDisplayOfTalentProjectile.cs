using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.XR;
using static UnityEngine.GraphicsBuffer;

public class SupremeDisplayOfTalentProjectile : MonoBehaviour
{
    private List<GameObject> targetsHit = null;

    private Quaternion aimRot;

    private int baseDamage;
    private int targetLayer;

    public float travelSpeed;
    public float pushSpeed;
    public float pushHeight;
    public float maxPushDistance;
    public float lifetime;

    private bool isMoving = true;
    public bool isBlue = false;

    public bool applyRoot = false;

    private int activeCoroutines = 0;

    Vector3 start;
    Vector3 end;

    private void Start()
    {
        //Debug.Log("Projectile initialized with base damage: " + baseDamage);
        targetsHit = new List<GameObject>();
    }

    private void Update()
    {
        if (start != null && end != null)
        {
            Vector3 dir = (end - start).normalized;
            float distance = Vector3.Distance(start, end);

            Debug.Log("Start inside collider " + Physics.CheckSphere(start, .01f, LayerMask.GetMask("Walls")));
            //Debug.DrawRay(start, dir * distance, Color.green);

            if (Physics.Raycast(start, dir, out RaycastHit hit, distance, LayerMask.GetMask("Walls")))
            {
                Debug.Log("Hit: " + hit.collider.name);
                Debug.DrawLine(start, hit.point, Color.yellow);
            }
            else
            {
                Debug.Log("NO HIT");
                Debug.DrawRay(start, dir * distance, Color.red);
            }
        }
    }
    private void FixedUpdate()
    {
        lifetime -= Time.deltaTime;
        if (lifetime <= 0)
        {
            isMoving = false;
        }
        if (isMoving)
            Move();
    }

    void Move()
    {
        transform.Translate(Vector3.forward * travelSpeed * Time.fixedDeltaTime);
    }

    public void Initialize(int baseDamage, int targetLayer, Quaternion rot)
    {
        this.baseDamage = baseDamage;
        this.targetLayer = targetLayer;
        aimRot = rot;

        //ElementContactAttributes();

        //Destroy(gameObject, lifetime);
    }

    private void ElementContactAttributes()
    {

    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.layer == LayerMask.NameToLayer("Walls"))
        {
            //TODO:  A way to set the projectile off 
            isMoving = false;
            WaitForAllCoroutines();
        }

        if (other.gameObject.layer == targetLayer)
        {
            Debug.Log("Projectile collided with target: " + other.gameObject.name);

            if (!targetsHit.Contains(other.gameObject))
            {
                RegisterHitTarget(other.gameObject);

                if (other.gameObject.TryGetComponent<TargetDummy>(out TargetDummy targetStats))
                {
                    Debug.Log("Projectile hit player with current currentHealth: " + targetStats.currentHealth);

                    if (other.gameObject.TryGetComponent<NavMeshAgent>(out NavMeshAgent agent))
                    {
                        TryPush(agent);
                    }

                    targetStats.ChangeHealth(-baseDamage);
                }
            }
        }
    }

    public void RegisterHitTarget(GameObject target)
    {
        if (target == null)
            return;

        if (!targetsHit.Contains(target))
        {
            targetsHit.Add(target);
        }
    }


    void TryPush(NavMeshAgent agent)
    {
        //Debug.Log("Attempting to push in direction: " + aimDir);
        Vector3 aimDir = aimRot * Vector3.forward;

        Vector3 start = agent.transform.position;
        Vector3 endpoint = start + aimDir * maxPushDistance;

        float pushDis = GetObstacleDistance(start, endpoint);
        float finalDis = (pushDis > 0f) ? pushDis : maxPushDistance;

        endpoint = start + aimDir * finalDis;
        this.end = endpoint;

        StartTrackedCoroutine(PushArc(agent, endpoint));
    }

    IEnumerator PushArc(NavMeshAgent agent, Vector3 end)
    {
        agent.enabled = false;
        var agentStart = agent.transform.position;
        //Debug.Log("Hopping from " + start + " to " + end);

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * pushSpeed;

            // Horizontal movement
            Vector3 pos = Vector3.Lerp(agentStart, end, t);

            // Vertical arc
            float height = Mathf.Sin(t * Mathf.PI) * pushHeight;
            pos.y += height;

            agent.transform.position = pos;

            yield return null;
        }

        agent.Warp(agent.transform.position);
        agent.enabled = true;
    }

    float GetObstacleDistance(Vector3 start, Vector3 end)
    {
        Vector3 dir = (end - start).normalized;
        float distance = Vector3.Distance(start, end);

        this.start = start;

        if (Physics.Raycast(start, dir, out RaycastHit hit, distance, LayerMask.GetMask("Walls")))
        {
            return hit.distance; // distance from start to the wall
        }

        return -1f; // no wall in between
    }

    private void StartTrackedCoroutine(IEnumerator routine)
    {
        activeCoroutines++;
        StartCoroutine(CoroutineWrapper(routine));
    }

    private IEnumerator CoroutineWrapper(IEnumerator routine)
    {
        yield return StartCoroutine(routine);
        activeCoroutines--;
    }

    IEnumerator WaitForAllCoroutines()
    {
        while (activeCoroutines > 0)
            yield return null;

        Debug.Log("All coroutines finished");
        Destroy(gameObject);
    }

}
