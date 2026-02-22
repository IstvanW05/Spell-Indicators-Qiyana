using System.Collections;
using UnityEngine;

public class IconKillTimer : MonoBehaviour
{
    public float killTime = 1f;
    private void Start()
    {
        StartCoroutine(KillAfterDelay());
    }

    private IEnumerator KillAfterDelay()
    {
        yield return new WaitForSeconds(killTime);
        Destroy(gameObject);
    }

}
