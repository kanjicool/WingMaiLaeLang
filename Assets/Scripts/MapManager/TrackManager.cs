using UnityEngine;
using System.Collections.Generic;

public class TrackManager : MonoBehaviour
{
    public GameObject[] roadPrefabs;
    public Transform playerTranform;

    private float spawnZ = 0.0f;
    private float roadLength = 30.0f;
    private int amountOfRoadsOnScreen = 5;
    private float safeZone = 35.0f;

    private List<GameObject> activeRoads = new List<GameObject>();

    void Start()
    {
        for (int i = 0; i < amountOfRoadsOnScreen; i++)
        {
            SpawnRoad(0);
        }
    }

    void Update()
    {
        if (playerTranform.position.z - safeZone > (spawnZ - amountOfRoadsOnScreen * roadLength))
        {
            SpawnRoad(Random.Range(0, roadPrefabs.Length));
            DeleteRoad();
        }


    }

    void SpawnRoad(int prefabIndex)
    {
        GameObject newRoad = Instantiate(roadPrefabs[prefabIndex], transform.forward * spawnZ, transform.rotation);
        spawnZ += roadLength;
        activeRoads.Add(newRoad);

        if (HumanSpawner.Instance != null)
        {
            RoadSpawnPoint[] points = newRoad.GetComponentsInChildren<RoadSpawnPoint>();

            foreach (var point in points)
            {
                HumanSpawner.Instance.SpawnHumanAtPoint(point);
            }
        }
    }

    void DeleteRoad()
    {
        Destroy(activeRoads[0]);
        activeRoads.RemoveAt(0);
    }
}
