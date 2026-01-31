using UnityEngine;

public class HumanController : MonoBehaviour
{
    public HumanData data;
    private Transform playerTransform;
    private float nextAttackTime = 0;
    private bool isDead = false;
    
    
    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (isDead || playerTransform == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if (data.type == HumanData.HumanType.Civilian && distance < 15f)
        {
            transform.Translate(Vector3.forward * data.fleeSpeed * Time.deltaTime);
        }

        else if ((data.type == HumanData.HumanType.Police || data.type == HumanData.HumanType.Soldier) && distance < data.attackRange)
        {
            // look at player
            Vector3 lookPos = playerTransform.position;
            lookPos.y = transform.position.y;
            transform.LookAt(lookPos);

            if (Time.time >= nextAttackTime)
            {
                Shoot();
                nextAttackTime = Time.time + data.attackRate;
            }
        }
    }

    void Shoot()
    {
        Debug.Log("Police Shoot!");

        // TODO: àÅè¹ Effect ÂÔ§»×¹
    }
}
