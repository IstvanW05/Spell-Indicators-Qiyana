using NUnit.Framework;
using NUnit.Framework.Constraints;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.XR;
using UnityEngine.UI;
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

    Vector3 impactPoint;
    Vector3 start;
    Vector3 end;

    [SerializeField] LayerMask wallLayer;
    [SerializeField] float padding = 0.5f; // Max distance to consider colliders as "connected" in flood fill

    [SerializeField] List<Collider> visited = new List<Collider>();

    public float mergeThreshold = 0.5f; // Distance threshold for merging nearby points in the outline
    public float sampleStep = 0.49f; // Distance between samples along collider edges (should be less than padding to ensure coverage)
    public float offsetDistance = .5f; // Distance to offset samples from collider edges


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
            impactPoint = other.ClosestPoint(transform.position);

            isMoving = false;

            StartFill(other);

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
    //TODO: Double check that this is working as intended, and that the raycast is starting outside the collider
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

    // Collect colliders
    void StartFill(Collider start) 
    {
        visited.Clear();
        FloodFill(start);
        //foreach (var col in visited)
        //{
        //    Debug.Log("Connected collider: " + col.name);
        //}
        OutlineColliders(visited);
    } 
    List<Collider> FloodFill(Collider start) // Returns all colliders connected to the start collider within the padding threshold to the 'visited' list
    {
        visited.Add(start);

        List<Collider> neighbors = FindNearbyColliders(start);

        foreach (var n in neighbors)
        {
            if (!visited.Contains(n))
                FloodFill(n);
        }

        return visited.ToList();
    }

    List<Collider> FindNearbyColliders(Collider source)
    {
        //Debug.Log("Finding neighbors for: " + source.name);
        List<Collider> found = new List<Collider>();

        BoxCollider bc = (BoxCollider)source;

        Vector3 worldCenter = bc.transform.TransformPoint(bc.center);
        Vector3 halfExtents = Vector3.Scale(bc.size * 0.5f, bc.transform.lossyScale); // Adjust for scale
        Quaternion orientation = bc.transform.rotation;


        Collider[] candidates = Physics.OverlapBox(
            worldCenter,
            halfExtents + Vector3.one * padding,
            orientation,
            wallLayer,
            QueryTriggerInteraction.Ignore
        );

        foreach (var col in candidates)
        {
            if (col == source) continue;
            if (visited.Contains(col)) continue;
            if (Overlaps(source, col))
                found.Add(col);

        }

        if (found.Count == 0)
        {
            //Debug.Log("No neighbors found for: " + source.name);
        }
        else
        {
            foreach (var f in found)
            {
                //Debug.Log("Found: " + f.name);
            }
        }

        return found;
    }
    bool Overlaps(Collider a, Collider b)
    {
        if (Physics.ComputePenetration(
                a, a.transform.position, a.transform.rotation,
                b, b.transform.position, b.transform.rotation,
                out _, out _))
            return true;

        // No penetration > check surface distance
        Vector3 pa = a.ClosestPoint(b.transform.position);
        Vector3 pb = b.ClosestPoint(a.transform.position);

        float surfaceDist = Vector3.Distance(pa, pb);
        return surfaceDist <= padding;
    }

    // OUTLINE GENERATION SYSTEM

    [SerializeField] List<Vector3> CornerPoints = new List<Vector3>();
    [SerializeField] List<Vector3> NormalPoints = new List<Vector3>();

    [SerializeField] List<Vector3> debugCornerPoints = new List<Vector3>();
    [SerializeField] List<Vector3> debugNormalPoints = new List<Vector3>();

    void OutlineColliders(List<Collider> colliders)
    {

        foreach (var col in colliders)
        {
            // Corner Sampling
            BoxCollider box = (BoxCollider)col;

            Vector3 ext = box.size * 0.5f;

            Vector3[] normals = new Vector3[4]
            {
                box.transform.TransformDirection(Vector3.left),    // -X face
                box.transform.TransformDirection(Vector3.back),    // -Z face
                box.transform.TransformDirection(Vector3.right),   // +X face
                box.transform.TransformDirection(Vector3.forward)  // +Z face
            };

            Vector3 nx = normals[0];    // -X face
            Vector3 nz = normals[1];    // -Z face
            Vector3 px = normals[2];    // +X face
            Vector3 pz = normals[3];    // +Z face

            Vector3[] localCorners = new Vector3[]
            {
                new Vector3(-ext.x, 0f, ext.z),     // Top-left
                new Vector3(-ext.x, 0f, -ext.z),    // Bottom-left
                new Vector3(ext.x, 0f, -ext.z),     // Bottom-right
                new Vector3(ext.x, 0f, ext.z)       // Top-right
            };

            List<(Vector3, Vector3, Vector3)> normalToCornerMap = new List<(Vector3, Vector3, Vector3)>
            {
                (localCorners[0], nx, pz),  // Top-left
                (localCorners[1], nx, nz),  // Bottom-left
                (localCorners[3], px, nz),  // Bottom-right
                (localCorners[2], px, pz)   // Top-right

            };

            List<Vector3> bisectors = ComputeBisector(normalToCornerMap);

            for (int i = 0; i < localCorners.Length; i++)
            {
                Vector3 worldCorner = box.transform.TransformPoint(localCorners[i]);
                Vector3 offsetCorner = worldCorner + bisectors[i] * offsetDistance;

                debugCornerPoints.Add(offsetCorner);
                CornerPoints.Add(offsetCorner);
            }

            // Face Sampling
            for (int i = 0; i < localCorners.Length; i++)
            {
                Vector3 a = box.transform.TransformPoint(localCorners[i]);
                Vector3 b = box.transform.TransformPoint(localCorners[(i + 1) % localCorners.Length]);

                Vector3 faceNormal = normals[i];

                SampleFaces(faceNormal, a, b);
            }
        }

        // Delete duplicate points
        CornerPoints = CornerPoints.Distinct().ToList();
        NormalPoints = NormalPoints.Distinct().ToList();
        debugCornerPoints = debugCornerPoints.Distinct().ToList();
        debugNormalPoints = debugNormalPoints.Distinct().ToList();

        // Delete points that are too close to colliders
        CornerPoints = DeleteClosePoints(CornerPoints, colliders);
        NormalPoints = DeleteClosePoints(NormalPoints, colliders);
        debugCornerPoints = DeleteClosePoints(debugCornerPoints, colliders);
        debugNormalPoints = DeleteClosePoints(debugNormalPoints, colliders);

        // Sort

        // Combine list
        List<Vector3> combined = CornerPoints.Concat(NormalPoints).ToList();
        List<Vector3> debugCombined = debugCornerPoints.Concat(debugNormalPoints).ToList();

        SortOutlinePoints(combined, colliders);
    }

    [SerializeField] Vector3 startingPoint;
    List<Vector3> SortOutlinePoints(List<Vector3> points, List<Collider> colliders)
    {
        if (points.Count <= 0 || impactPoint == null) return null;

        var sorted = new List<Vector3>();

        // Find closest point to impactPoint that has line of sight
        var closestPoint = points[0];
        var closestDist = Vector3.Distance(points[0], impactPoint);

        foreach (var p in points)
        {
            float d = Vector3.Distance(p, impactPoint);

            if (d < closestDist && !SegmentIntersectsAnyCollider(p, impactPoint, colliders))
            {
                closestDist = d;
                closestPoint = p;

                startingPoint = closestPoint;
            }
        }
        Debug.Log("Closest point: " + closestPoint);


        return sorted;
    }

    bool SegmentIntersectsAnyCollider(Vector3 a, Vector3 b, List<Collider> colliders)
    {
        Vector3 dir = (b - a);
        float dist = dir.magnitude;

        if (Physics.Raycast(a, dir.normalized, out RaycastHit hit, dist + 0.01f))
        {
            if (colliders.Contains(hit.collider))
                return true;
        }

        return false;
    }

    List<Vector3> DeleteClosePoints(List<Vector3> points, List<Collider> colliders)
    { 
        if (points.Count <= 0 || colliders.Count <= 0) return null;

            var filtered = new List<Vector3>();

            foreach (var p in points)
            {
                bool tooClose = false;
    
                foreach (var col in colliders)
                {
                    Vector3 closest = col.ClosestPoint(p);
                    float dist = Vector3.Distance(p, closest);
    
                    if (dist < offsetDistance - 0.01f)
                    {
                        tooClose = true;
                        break;
                    }
                }
    
                if (!tooClose)
                    filtered.Add(p);
            }

        return filtered;
    }

    void SampleFaces(Vector3 faceNormal, Vector3 a, Vector3 b)
    {
        float length = Vector3.Distance(a, b);
        int steps = Mathf.CeilToInt(length / sampleStep);

        for (int i = 0; i <= steps; i++)
        {
            float t = (float)i / steps;
            Vector3 p = Vector3.Lerp(a, b, t);

            // Offset outward
            Vector3 offset = p + faceNormal * offsetDistance;

            debugNormalPoints.Add(offset);
            NormalPoints.Add(offset);
        }
    }

    List<Vector3> ComputeBisector(List<(Vector3 corner, Vector3 normalA, Vector3 normalB)> cornerData)
    {
        List<Vector3> bisectors = new List<Vector3>();

        foreach (var data in cornerData)
        {
            Vector3 bisector = (data.normalA + data.normalB).normalized;
            bisectors.Add(bisector);
        }
        return bisectors;
    }

    // VISUALIZER
    private void OnDrawGizmos()
    {
        foreach(var p in debugCornerPoints)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(p, 0.1f);
        }

        foreach(var n in debugNormalPoints)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(n, 0.1f);
        }

        if (startingPoint != Vector3.zero)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(startingPoint, 0.3f);
        }

        if (impactPoint != Vector3.zero)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(impactPoint, 0.3f);
        }
    }

}
