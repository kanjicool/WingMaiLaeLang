using UnityEngine;

public class ZombieController : MonoBehaviour
{
    private Animator anim;

    [HideInInspector] public Vector3 swarmOffset;
    [HideInInspector] public Vector3 currentVelocity;

    private ZombieData myData;

    public void Initialize(ZombieData data)
    {
        myData = data;
        anim = GetComponentInChildren<Animator>();

        if (anim && data.overrideController != null)
        {
            anim.runtimeAnimatorController = data.overrideController;
        }

        if (anim) anim.Play("Run_Arms", 0, Random.Range(0f, 1f));
    }

    public void UpdatePosition(Vector3 targetPos, Quaternion targetRot, float smoothTime, float maxSpeed)
    {
        // 1. Catch Up Logic (ถ้าหลุดฝูงให้รีบวิ่ง)
        float dist = Vector3.Distance(transform.position, targetPos);

        // ยิ่งห่าง ยิ่งลด smoothTime (ให้ไวขึ้น) แต่ไม่ต่ำกว่า 0.1
        // Logic: ถ้าห่าง > 2 เมตร ให้เริ่มเร่ง
        float urgency = Mathf.Clamp01((dist - 1.0f) / 5.0f);
        float finalSmooth = Mathf.Lerp(smoothTime, 0.1f, urgency);

        // 2. Move
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref currentVelocity, finalSmooth, maxSpeed);

        // 3. Rotate (หันหน้าตามทิศที่วิ่งจริง ดูสมจริงกว่าหันตาม Player ตลอด)
        if (currentVelocity.magnitude > 0.5f)
        {
            Quaternion lookRot = Quaternion.LookRotation(currentVelocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, 10f * Time.deltaTime);
        }
        else
        {
            // ถ้ายืนนิ่งๆ ค่อยหันตาม Player
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 5f * Time.deltaTime);
        }

        // 4. Animation Sync
        if (anim)
        {
            float currentSpeed = currentVelocity.magnitude;
            anim.SetBool("IsRunning", currentSpeed > 0.1f);

            float runCycle = currentSpeed / 8.0f;
            anim.speed = Mathf.Clamp(runCycle, 0.5f, 2.0f);
        }
    }

    public void DoJump()
    {
        if (anim) anim.SetTrigger("Jump");
    }

    public void DoSlide()
    {
        if (anim) anim.SetTrigger("Slide");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle"))
        {
            if (SwarmManager.Instance != null)
            {
                SwarmManager.Instance.KillSpecificZombie(this);
            }
        }
    }
}