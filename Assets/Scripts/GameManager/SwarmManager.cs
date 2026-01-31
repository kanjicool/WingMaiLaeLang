using UnityEngine;
using System.Collections.Generic;

public class SwarmManager : MonoBehaviour
{
    public static SwarmManager Instance;

    [Header("Zombie Types")]
    public List<ZombieData> zombieTypes = new List<ZombieData>();

    [Header("Settings")]
    public ZombieData defaultZombieData;
    public Transform zombieContainer;

    [Header("Swarm Area Settings")]
    public float swarmWidth = 4.0f;     // ความกว้างของพื้นที่ฝูง (ซ้าย-ขวา)
    public float swarmDepth = 5.0f;     // ความลึกของพื้นที่ฝูง (หน้า-หลัง)
    public float minSeparation = 0.8f;  // ระยะห่างต่ำสุดระหว่างซอมบี้ (กันตัวซ้อน)
    public float centerOffsetZ = -2.0f; // จุดศูนย์กลางฝูงให้อยู่หลัง Player กี่เมตร

    [Header("Movement Physics")]
    public float baseSmoothTime = 0.3f; // ความหน่วง (ยิ่งเยอะยิ่งไหล)
    public float maxSpeed = 20f;

    public List<ZombieController> zombies = new List<ZombieController>();

    private void Awake()
    {
        Instance = this;
        if (zombieContainer == null)
        {
            GameObject go = new GameObject("ZombieContainer");
            zombieContainer = go.transform;
        }
    }

    void Update()
    {
        if (zombies.Count == 0) return;

        // จุดอ้างอิงหลัก (Anchor) คือตำแหน่ง Player
        Vector3 anchorPos = transform.position;
        Quaternion anchorRot = transform.rotation;

        for (int i = 0; i < zombies.Count; i++)
        {
            ZombieController currentZom = zombies[i];

            // --- คำนวณตำแหน่งเป้าหมายแบบอิสระ ---
            // เป้าหมาย = (ตำแหน่ง Player) + (ระยะถอยหลังพื้นฐาน) + (ตำแหน่งเฉพาะตัวในฝูง)

            Vector3 targetPos = anchorPos;
            targetPos.z += centerOffsetZ; // ถอยไปจุดศูนย์กลางฝูงก่อน

            // บวก Offset เฉพาะตัว (ที่สุ่มไว้ตอนเกิด)
            targetPos.x += currentZom.swarmOffset.x;
            targetPos.z += currentZom.swarmOffset.z;

            // --- Movement Logic ---
            // สุ่ม SmoothTime เล็กน้อยให้แต่ละตัวขยับไม่พร้อมกันเป๊ะๆ
            float randomSmooth = baseSmoothTime + (currentZom.GetHashCode() % 10 * 0.015f);

            currentZom.UpdatePosition(targetPos, anchorRot, randomSmooth, maxSpeed);
        }
    }

    public void AddZombie()
    {
        if (defaultZombieData == null) return;

        ZombieData selectedData = GetRandomZombieType();

        // 1. หาตำแหน่งเกิด (Spawn Position)
        // ถ้ามีเพื่อนอยู่แล้ว ให้เกิดที่เพื่อนตัวสุดท้ายก่อนจะได้ดูต่อเนื่อง
        // ถ้าไม่มีใครเลย ให้เกิดหลัง Player
        Vector3 spawnWorldPos = transform.position - (transform.forward * 2f);
        if (zombies.Count > 0)
        {
            spawnWorldPos = zombies[zombies.Count - 1].transform.position;
        }

        // 2. คำนวณ "ตำแหน่งประจำตัว (Offset)" แบบสุ่มที่ไม่ทับเพื่อน
        Vector3 newOffset = GetValidRandomOffset();

        GameObject go = Instantiate(selectedData.prefab, spawnWorldPos, Quaternion.identity, zombieContainer);
        ZombieController zCtrl = go.GetComponent<ZombieController>();


        zCtrl.swarmOffset = newOffset;
        zCtrl.Initialize(selectedData);
        
        zombies.Add(zCtrl);
    }

    // ฟังก์ชันสุ่มหาที่ว่างในฝูง (สำคัญมาก!)
    Vector3 GetValidRandomOffset()
    {
        int maxAttempts = 20; // พยายามสุ่มหาที่ว่าง 20 ครั้ง ถ้าไม่ได้จริงๆ ค่อยยอมให้ทับ
        for (int i = 0; i < maxAttempts; i++)
        {
            // สุ่มตำแหน่งในวงรี (Ellipse) หรือ สี่เหลี่ยม
            float rX = Random.Range(-swarmWidth / 2f, swarmWidth / 2f);
            float rZ = Random.Range(-swarmDepth / 2f, 0f); // ให้กระจายไปด้านหลังเป็นหลัก (0 ถึง -depth)

            Vector3 candidateOffset = new Vector3(rX, 0, rZ);

            // เช็คว่าตำแหน่งนี้ใกล้เพื่อนที่มีอยู่แล้วเกินไปไหม
            bool isTooClose = false;
            foreach (var z in zombies)
            {
                if (Vector3.Distance(candidateOffset, z.swarmOffset) < minSeparation)
                {
                    isTooClose = true;
                    break;
                }
            }

            if (!isTooClose)
            {
                return candidateOffset;
            }
        }

        return new Vector3(Random.Range(-swarmWidth / 2f, swarmWidth / 2f), 0, Random.Range(-swarmDepth, 0));
    }


    ZombieData GetRandomZombieType()
    {
        float totalWeight = 0;
        foreach (var z in zombieTypes)
        {
            totalWeight += z.spawnWeight;
        }

        float randomValue = Random.Range(0, totalWeight);
        float currentSum = 0;

        foreach (var z in zombieTypes)
        {
            currentSum += z.spawnWeight;
            if (randomValue <= currentSum)
            {
                return z;
            }
        }


        return zombieTypes[0];
    }

    public void RemoveZombie()
    {
        if (zombies.Count == 0) return;
        int lastIndex = zombies.Count - 1;
        ZombieController z = zombies[lastIndex];
        zombies.RemoveAt(lastIndex);
        Destroy(z.gameObject);
    }

    public void ResetSwarm()
    {
        foreach (var z in zombies) if (z != null) Destroy(z.gameObject);
        zombies.Clear();
    }

    public void HordeJump()
    {
        foreach(var z in zombies)
        {
            StartCoroutine(DelayAction(z, "Jump", Random.Range(0f, 0.025f)));
        }
    }

    public void HordeSlide()
    {
        foreach (var z in zombies)
        {
            StartCoroutine(DelayAction(z, "Slide", Random.Range(0f, 0.025f)));
        }
    }

    System.Collections.IEnumerator DelayAction(ZombieController z, string action, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (z == null) yield break;

        if (action == "Jump") z.DoJump();
        else if (action == "Slide") z.DoSlide();
    }
}