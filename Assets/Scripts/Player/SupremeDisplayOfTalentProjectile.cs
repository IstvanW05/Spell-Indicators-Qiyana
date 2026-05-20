using NUnit.Framework;
using NUnit.Framework.Constraints;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    [SerializeField] LayerMask wallLayer;
    [SerializeField] float padding = 0.5f;

    [SerializeField] List<Collider> visited = new List<Collider>();

    float sampleStep = 0.49f;
    float offsetDistance = .5f;


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
            // Create a list of gameobjects that make up the boundaries (probably by the relative distance they are to one another)
            isMoving = false;
            StartCollection(other);
            WaitForAllCoroutines();

            List<Vector3> outline = GenerateColliderClusterOutline(visited, sampleStep, offsetDistance);
            VisualizeOutline(outline);
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

    // Collect colliders
    void StartCollection(Collider start)
    {
        //Debug.Log("Starting flood fill from: " + start.name);
        visited.Clear();
        FloodFill(start);
    }
    List<Collider> FloodFill(Collider source)
    {
        visited.Add(source);

        List<Collider> neighbors = FindNearbyColliders(source);

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

    struct SamplePoint
    {
        public Vector3 point, normal;
        public SamplePoint(Vector3 p, Vector3 n) { point = p; normal = n.normalized; }
    }

    List<Vector3> GenerateColliderClusterOutline(List<Collider> colliders, float sampleStep, float offsetDistance)
    {
        if (colliders == null || colliders.Count == 0) return new List<Vector3>();

        List<Vector3> raw = new List<Vector3>();

        // Corner samples
        foreach (var c in colliders.OfType<BoxCollider>())
            foreach (var corner in GetBoxColliderCorners(c))
            {
                Vector3 n = ComputeCornerNormal(c, corner);
                Vector3 p = corner + n * offsetDistance;
                if (IsExteriorPoint(p, n, colliders)) raw.Add(p);
            }

        // Edge samples
        List<SamplePoint> samples = colliders.SelectMany(c => SampleCollider(c, sampleStep)).ToList();
        raw.AddRange(BuildOffsetExteriorPoints(samples, colliders, offsetDistance));

        raw = RemoveDuplicatePoints(raw, 0.01f);
        raw = ConsolidateNearbyPoints(raw, 0.5f);
        return SortLoop(raw);
    }

    List<SamplePoint> SampleCollider(Collider col, float step) =>
        (col is BoxCollider box) ? SampleBoxCollider(box, step) : new List<SamplePoint>();

    List<SamplePoint> SampleBoxCollider(BoxCollider box, float step)
    {
        List<SamplePoint> pts = new();
        Transform t = box.transform;
        Vector3 ext = box.size * 0.5f;

        Vector3[] corners =
        {
        new(-ext.x,0,-ext.z), new(-ext.x,0,ext.z),
        new(ext.x,0,ext.z),   new(ext.x,0,-ext.z)
    };

        for (int i = 0; i < 4; i++)
        {
            Vector3 a = corners[i], b = corners[(i + 1) % 4];
            int steps = Mathf.Max(1, Mathf.CeilToInt(Vector3.Distance(a, b) / step));

            for (int s = 0; s <= steps; s++)
            {
                Vector3 p = t.TransformPoint(Vector3.Lerp(a, b, s / (float)steps) + box.center);
                pts.Add(new SamplePoint(p, ComputeOutwardNormal(box, p)));
            }
        }
        return pts;
    }

    Vector3 ComputeOutwardNormal(BoxCollider box, Vector3 world)
    {
        Vector3 local = box.transform.InverseTransformPoint(world) - box.center;
        Vector3 half = box.size * 0.5f;

        float dx = Mathf.Min(Mathf.Abs(half.x - local.x), Mathf.Abs(-half.x - local.x));
        float dz = Mathf.Min(Mathf.Abs(half.z - local.z), Mathf.Abs(-half.z - local.z));

        Vector3 n = (dx < dz)
            ? new Vector3(Mathf.Sign(local.x), 0, 0)
            : new Vector3(0, 0, Mathf.Sign(local.z));

        return box.transform.TransformDirection(n).normalized;
    }

    Vector3 ComputeCornerNormal(BoxCollider box, Vector3 worldCorner)
    {
        Vector3 local = box.transform.InverseTransformPoint(worldCorner) - box.center;
        Vector3 nx = (local.x > 0) ? Vector3.right : Vector3.left;
        Vector3 nz = (local.z > 0) ? Vector3.forward : Vector3.back;
        return box.transform.TransformDirection((nx + nz).normalized);
    }

    List<Vector3> GetBoxColliderCorners(BoxCollider box)
    {
        Vector3 ext = box.size * 0.5f;
        Vector3[] lc =
        {
        new(-ext.x,0,-ext.z), new(-ext.x,0,ext.z),
        new(ext.x,0,ext.z),   new(ext.x,0,-ext.z)
    };
        return lc.Select(c => box.transform.TransformPoint(c + box.center)).ToList();
    }

    List<Vector3> BuildOffsetExteriorPoints(List<SamplePoint> samples, List<Collider> cols, float dist)
    {
        List<Vector3> pts = new();
        float minClearance = dist * (offsetDistance - 0.01f);

        foreach (var s in samples)
        {
            Vector3 p = s.point + s.normal * dist;
            if (TooCloseToAnyCollider(p, cols, minClearance))
                continue;

            if (IsExteriorPoint(p, s.normal, cols)) pts.Add(p);
        }
        return pts;
    }

    bool IsExteriorPoint(Vector3 p, Vector3 n, List<Collider> cols)
    {
        const float insideR = 0.02f, rayOffset = 0.02f, rayDist = 0.05f;

        if (Physics.CheckSphere(p, insideR, wallLayer, QueryTriggerInteraction.Ignore))
            return false;

        Ray r = new(p + n * rayOffset, n);
        return !cols.Any(c => c.enabled && c.Raycast(r, out _, rayDist));
    }

    // SORT
    List<Vector3> SortLoop(List<Vector3> pts)
    {
        if (pts.Count < 3) return pts;

        Vector3 start = pts.OrderBy(p => p.x).ThenBy(p => p.z).First();
        List<Vector3> ordered = new() { start };

        Vector3 cur = start, prevDir = Vector3.forward;
        HashSet<int> used = new() { pts.IndexOf(start) };

        for (int step = 0; step < pts.Count; step++)
        {
            float best = float.MaxValue;
            int bestIdx = -1;

            for (int i = 0; i < pts.Count; i++)
            {
                if (used.Contains(i)) continue;
                Vector3 cand = pts[i];

                if (!SegmentIsClear(cur, cand, visited)) continue;

                Vector3 dir = (cand - cur).normalized;
                float score = Vector3.Distance(cur, cand) + Mathf.Abs(Vector3.SignedAngle(prevDir, dir, Vector3.up)) * 0.1f;

                if (score < best) { best = score; bestIdx = i; }
            }

            if (bestIdx == -1) break;

            Vector3 next = pts[bestIdx];
            ordered.Add(next);
            prevDir = (next - cur).normalized;
            cur = next;
            used.Add(bestIdx);
        }
        ordered = EnsureLoopClosure(ordered, visited);

        return ordered;
    }

    bool SegmentIsClear(Vector3 a, Vector3 b, List<Collider> cols)
    {
        Vector3 dir = (b - a).normalized;
        float dist = Vector3.Distance(a, b);
        return !Physics.Raycast(a + Vector3.up * 0.01f, dir, dist, wallLayer, QueryTriggerInteraction.Ignore);
    }

    // UTIL
    List<Vector3> RemoveDuplicatePoints(List<Vector3> pts, float th)
    {
        List<Vector3> u = new();
        float t2 = th * th;

        foreach (var p in pts)
            if (!u.Any(q => (q - p).sqrMagnitude < t2))
                u.Add(p);

        return u;
    }

    List<Vector3> ConsolidateNearbyPoints(List<Vector3> points, float threshold)
    {
        float t2 = threshold * threshold;
        List<Vector3> result = new List<Vector3>(points);
        bool changed = true;

        while (changed)
        {
            changed = false;

            for (int i = 0; i < result.Count; i++)
            {
                for (int j = i + 1; j < result.Count; j++)
                {
                    if ((result[i] - result[j]).sqrMagnitude < t2)
                    {
                        // Merge into average
                        Vector3 merged = (result[i] + result[j]) * 0.5f;

                        // Remove originals
                        result.RemoveAt(j);
                        result.RemoveAt(i);

                        // Insert merged
                        result.Add(merged);

                        changed = true;
                        break;
                    }
                }

                if (changed) break;
            }
        }

        return result;
    }
    bool TooCloseToAnyCollider(Vector3 point, List<Collider> colliders, float minClearance)
    {
        float minClearanceSqr = minClearance * minClearance;

        foreach (var c in colliders)
        {
            // Closest point on collider surface
            Vector3 closest = c.ClosestPoint(point);

            // If the point is inside the collider, ClosestPoint returns the point itself
            float d2 = (closest - point).sqrMagnitude;

            if (d2 < minClearanceSqr)
                return true;
        }

        return false;
    }

    List<Vector3> EnsureLoopClosure(List<Vector3> pts, List<Collider> colliders)
    {
        if (pts.Count < 3)
            return pts;

        bool changed = true;

        while (changed && pts.Count >= 3)
        {
            changed = false;

            Vector3 first = pts[0];
            Vector3 last = pts[pts.Count - 1];

            // If last > irst crosses a collider, remove last point
            if (!SegmentIsClear(last, first, colliders))
            {
                pts.RemoveAt(pts.Count - 1);
                changed = true;
            }
        }

        return pts;
    }

    // VISUALIZER
    [SerializeField] float lineWidth = 0.1f;
    [SerializeField] Color lineColor = Color.red;
    [SerializeField] float lineLifetime = 0f;

    void VisualizeOutline(List<Vector3> pts)
    {
        if (pts == null || pts.Count < 2) return;

        GameObject obj = new("OutlineVisualizer");
        obj.transform.SetParent(transform);

        LineRenderer lr = obj.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.loop = true;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = lr.endColor = lineColor;
        lr.startWidth = lr.endWidth = lineWidth;
        lr.positionCount = pts.Count;
        lr.SetPositions(pts.ToArray());

        if (lineLifetime > 0) Destroy(obj, lineLifetime);
    }

}
