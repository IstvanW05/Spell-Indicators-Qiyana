using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainTrain : MonoBehaviour
{
    public float distPerSec;

    [SerializeField] GameObject trainA;
    [SerializeField] GameObject trainB;

    public Vector3 startingPoint;

    public List<Vector3> spline = new List<Vector3>();

    public bool isBlue = false;

    private void Start()
    {
        trainA = Resources.Load<GameObject>("TrainA");
        trainB = Resources.Load<GameObject>("TrainB");

        if (trainA != null)
        {
            GameObject a = Instantiate(trainA, startingPoint, Quaternion.identity);
            StartCoroutine(MoveTrain(a));
        }

        if (trainB != null)
        {
            GameObject b = Instantiate(trainB, startingPoint, Quaternion.identity);
            StartCoroutine(MoveTrain(b));
        }
    }

    IEnumerator MoveTrain(GameObject obj)
    {
        //Debug.Log("Moving train: " + obj.name);
        List<Vector3> path = new List<Vector3>(spline); // Create a copy of the spline for this train

        if (obj.name.Contains("TrainB"))
        {
            path.Reverse(); // If it's the second train, reverse the path
            Debug.Log("Reversed path for trainB");
        }

        foreach (Vector3 point in path)
        {
            obj.transform.LookAt(point); // Rotate the train to face the next point

            while (Vector3.Distance(obj.transform.position, point) > 0.1f)
            {
                obj.transform.position = Vector3.MoveTowards(
                    obj.transform.position,
                    point,
                    distPerSec * Time.deltaTime
                );

                yield return null;
            }
        }

    }
}
