using UnityEngine;

[CreateAssetMenu(fileName = "NewHumanData", menuName = "Game/Human Data")]
public class HumanData : ScriptableObject
{
    public enum HumanType { Civilian, Runner, Soldier}
    public HumanType type;

    [Header("Stats")]
    public float moveSpeed = 5f;
    public float detectRange = 15f;
    public float attackRate = 1f;
    public int zombieValue = 1;
    public GameObject projectilePrefab;

    [Header("VFX")]
    public GameObject bloodEffectPrefab;

}
