using System.Globalization;
using UnityEngine;

public class ZombieController : MonoBehaviour
{

    public ZombieData data;
    private Transform targetAnchor;
    private PlayerController player;

    public void Initialize(ZombieData zombieData, Transform anchor, PlayerController owner)
    {
        data = zombieData;
        targetAnchor = anchor;
        player = owner; 
    }

    void Start()
    {
        
    }

    void Update()
    {
        if (targetAnchor == null) return;
        
        Vector3 destination = targetAnchor.position;

        foreach (var z in player.zombies)
        {
            if (z == this.gameObject) continue;

            float dist = Vector3.Distance(transform.position, z.transform.position);
            if (dist < data.separationRadius)
            {
                Vector3 pushDir = transform.position - z.transform.position;
                destination += pushDir.normalized * (data.separationRadius - dist) * 2f;
            }
        }

        transform.position = Vector3.Lerp(transform.position, destination, data.reactionSpeed * Time.deltaTime * 50f);

        transform.rotation = Quaternion.Lerp(transform.rotation, player.transform.rotation, 10f * Time.deltaTime);
    }
}
