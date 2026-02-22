using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    private PlayerInput playerInput;
    public Camera mainCam;
    public int layer = 8;
    public GameObject moveIcon;
    NavMeshAgent agent;
    InputAction moveToClick;

    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        agent = GetComponent<NavMeshAgent>();

        moveToClick = playerInput.actions["MoveToClick"];
    }
private void Update()
    {
        if (moveToClick.IsPressed())
        {
            Ray ray = mainCam.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.gameObject.layer == layer)
                {
                    if (moveToClick.WasPressedThisFrame())
                    {
                        Vector3 offset = new Vector3(hit.point.x, hit.point.y + 0.1f, hit.point.z);
                        Instantiate(moveIcon, offset, Quaternion.identity);
                    }
                    agent.SetDestination(hit.point);
                }
            }
        }
    }
}
