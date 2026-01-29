using UnityEngine;

[CreateAssetMenu(fileName = "NewHumanData", menuName = "Game/Human Data")]
public class HumanData : ScriptableObject
{
    public enum HumanType { Civilian, Police, Soldier}
    public HumanType type;

    public float fleeSpeed = 3f;
    public float attackRange = 10f;
    public float attackRate = 1f;
    public int zombieValue = 1;

}
