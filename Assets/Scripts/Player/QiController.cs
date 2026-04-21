using UnityEngine;
using UnityEngine.AI;

public class QiController : MonoBehaviour
{
    public NavMeshAgent agent;
    TerrainElementVolume currentTerrain;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out TerrainElementVolume volume))
            currentTerrain = volume;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out TerrainElementVolume volume) && currentTerrain == volume)
            currentTerrain = null;
    }
}
