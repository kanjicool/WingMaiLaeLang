using UnityEngine;

public class HumanSpawner : MonoBehaviour
{
    public static HumanSpawner Instance;

    [Header("Prefabs")]
    public GameObject[] humanPrefabs;
    public GameObject[] itemPrefabs; // เพิ่มช่องสำหรับใส่ไอเทม (X2, Shield, Slow)

    [Header("Settings")]
    [Range(0, 1)]
    public float itemSpawnChance = 0.2f; // โอกาสเกิดไอเทม (0.2 = 20%)
    public float itemYOffset = 0.5f;     // ความสูงไอเทมเพื่อไม่ให้จมดิน

    private void Awake()
    {
        Instance = this;
    }

    public void SpawnHumanAtPoint(RoadSpawnPoint spawnPoint)
    {
        // 1. เช็คดวงก่อนว่าจุดนี้จะสปอนอะไรไหม (อิงจากค่าใน SpawnPoint)
        if (Random.value > spawnPoint.spawnChance) return;

        GameObject prefabToSpawn;
        Vector3 spawnPos = spawnPoint.transform.position;

        // 2. สุ่มเลือกว่าจะเป็นไอเทมหรือคน
        if (Random.value < itemSpawnChance && itemPrefabs.Length > 0)
        {
            // เลือกไอเทม
            prefabToSpawn = itemPrefabs[Random.Range(0, itemPrefabs.Length)];
            spawnPos += new Vector3(0, itemYOffset, 0); // ยกให้สูงขึ้น
        }
        else
        {
            // เลือกคน
            prefabToSpawn = humanPrefabs[Random.Range(0, humanPrefabs.Length)];
        }

        // 3. ทำการสร้าง (Instantiate)
        GameObject go = Instantiate(prefabToSpawn, spawnPos, spawnPoint.transform.rotation);
        go.transform.SetParent(spawnPoint.transform);

        // 4. ถ้าเป็นคน ให้ Setup Lane (ถ้าเป็นไอเทมมันจะข้ามส่วนนี้ไปเอง)
        HumanController human = go.GetComponent<HumanController>();
        if (human != null)
        {
            human.SetupLane(spawnPoint.laneIndex);
        }
    }
}