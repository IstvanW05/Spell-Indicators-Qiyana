using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class PlayerScript : MonoBehaviour
{
    private PlayerInput playerInput;
    private PlayerStats playerStats;
    public Camera mainCam;

    public GameObject moveIcon;

    bool hasTarget = false;
    bool moveInputWasPressed = false;

    readonly int layerGround = 8, BluePlayer = 10, RedPlayer = 11;

    GameObject target;
    NavMeshAgent agent;
    InputAction moveToClick;

    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        playerStats = GetComponent<PlayerStats>();
        agent = GetComponent<NavMeshAgent>();

        agent.stoppingDistance = playerStats.attackRange;

        moveToClick = playerInput.actions["MoveToClick"];
        StartCoroutine(ClickToMoveLoop());
    }
    private void Update()
    {
        moveInputWasPressed = moveToClick.IsPressed();
    }
    private IEnumerator ClickToMoveLoop()
    {
        LayerMask ignoreLayer = ~LayerMask.GetMask("Terrain");

        while (true)
        {
            // Wait until agent is valid
            yield return new WaitUntil(() => agent.enabled && agent.isOnNavMesh);

            if (moveInputWasPressed)
            {
                Ray ray = mainCam.ScreenPointToRay(Mouse.current.position.ReadValue());
                agent.stoppingDistance = 0f;

                if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, ignoreLayer))
                {
                    if (hit.collider.gameObject.layer == layerGround)
                    {
                        hasTarget = false;
                        if (NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, 1.0f, NavMesh.AllAreas))
                        {
                            // Spawn icon only once per click
                            if (moveToClick.WasPressedThisFrame())
                            {
                                Vector3 offset = hit.point + Vector3.up * 0.1f;
                                Instantiate(moveIcon, offset, Quaternion.identity);
                            }
                            // Update destination every frame while held
                            agent.SetDestination(hit.point);
                        }
                    }
                    if (hit.collider.gameObject.layer == BluePlayer || hit.collider.gameObject.layer == RedPlayer)
                    {
                        hasTarget = true;
                        target = hit.collider.gameObject;
                    }
                }
            }
            if (hasTarget && target != null)
            {
                if (Vector3.Distance(transform.position, target.transform.position) <= playerStats.attackRange)
                {
                    // In attack range
                    Debug.Log("Attacking " + target.name);
                    if (target.TryGetComponent(out TargetDummy dummy))
                        dummy.ChangeHealth(-10);
                }
                else
                {
                    agent.SetDestination(target.transform.position);
                }
            }

            yield return null;
        }
    }

    public IEnumerator WaitTillLanded()
    {
        yield return new WaitUntil(() => agent.enabled && agent.isOnNavMesh);
    }

    public void FaceAimDirection(Quaternion target)
    {
        //Debug.Log("Facing aim direction");
        transform.rotation = target;
    }
}
