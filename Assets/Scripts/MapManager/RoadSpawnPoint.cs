using UnityEngine;

public class RoadSpawnPoint : MonoBehaviour
{
    [Tooltip("Lanes on the road (0=left, 1=mid, 2=right)")]
    public int laneIndex = 1;

    [Tooltip("Pobability (0.0 - 1.0)")]
    public float spawnChance = 0.5f;

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 0.3f);
        Gizmos.DrawWireCube(transform.position, new Vector3(0.5f, 1f, 0.5f));
    }

}
