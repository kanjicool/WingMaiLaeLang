using UnityEngine;

public class HumanSpawner : MonoBehaviour
{
    public static HumanSpawner Instance;

    [Header("Prefabs")]
    public GameObject[] humanPrefabs; // ใส่ Civilian, Police, Soldier
    private void Awake()
    {
        Instance = this;
    }

    public void SpawnHumanAtPoint(RoadSpawnPoint spawnPoint)
    {
        if (Random.value > spawnPoint.spawnChance) return;

        GameObject prefabToSpawn = humanPrefabs[Random.Range(0, humanPrefabs.Length)];

        GameObject go = Instantiate(prefabToSpawn, spawnPoint.transform.position, spawnPoint.transform.rotation);

        go.transform.SetParent(spawnPoint.transform);

        HumanController human = go.GetComponent<HumanController>();
        if (human != null)
        {
            // ส่งค่า laneIndex ที่เรากรอกไว้ใน Inspector ไปให้คน
            human.SetupLane(spawnPoint.laneIndex);
        }

    }


    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
