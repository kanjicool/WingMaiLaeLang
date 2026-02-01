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



    [Header("Speial Ability")]
    public bool isTank;
    public bool isRunner;
}
