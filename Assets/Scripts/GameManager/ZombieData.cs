using UnityEngine;

[CreateAssetMenu(fileName = "NewZombieData", menuName = "Game/Zombie Data")]
public class ZombieData : ScriptableObject
{
    public string zombieName;
    public GameObject prefab;

    [Header("Visual & Animation")]
    public AnimatorOverrideController overrideController;

    [Header("Swarm Settings")]
    public float followSpeed = 15f;
    public float gapDistance = 0.8f;
    [Range(0, 100)] public float spawnWeight = 10f;

    [Header("AI Stats")]
    public float moveSpeed = 5f;
    public float reactionSpeed = 0.1f; // ค่าน้อย = ฉลาด/ตอบสนองไว, ค่ามาก = อืดอาด/โง่
    public float separationRadius = 0.5f; // ระยะห่างจากเพื่อน (กันซ้อนทับ)

    [Header("Speial Ability")]
    public bool isTank;
    public bool isRunner;
}
