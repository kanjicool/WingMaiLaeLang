using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class ZombieController : MonoBehaviour
{
    private Animator anim;

    [HideInInspector] public Vector3 swarmOffset;
    [HideInInspector] public Vector3 currentVelocity;

    private ZombieData myData;
    private AudioSource audioSource;

    public void Initialize(ZombieData data)
    {
        myData = data;
        anim = GetComponentInChildren<Animator>();

        if (anim && data.overrideController != null)
        {
            anim.runtimeAnimatorController = data.overrideController;
        }

        if (anim) anim.Play("Run_Arms", 0, Random.Range(0f, 1f));

        audioSource = GetComponent<AudioSource>();
        audioSource.spatialBlend = 1.0f; // ปรับเป็น 3D Sound (ไกลเบา ใกล้ดัง)
        audioSource.volume = myData.volume;

        // 1. เล่นเสียงเกิด (Spawn)
        PlayRandomSound(myData.spawnSounds);

        // 2. เริ่มวนลูปเสียงร้องรำคาญ (Ambient)
        StartCoroutine(AmbientSoundRoutine());
    }

    IEnumerator AmbientSoundRoutine()
    {
        while (true)
        {
            // รอเวลาสุ่ม (เช่น ทุกๆ 3 ถึง 8 วินาที ร้องทีนึง)
            float waitTime = Random.Range(3.0f, 8.0f);
            yield return new WaitForSeconds(waitTime);

            // เล่นเสียง
            PlayRandomSound(myData.ambientSounds);
        }
    }

    void PlayRandomSound(AudioClip[] clips)
    {
        if (clips != null && clips.Length > 0 && audioSource != null)
        {
            // สุ่มมา 1 เสียง
            AudioClip clip = clips[Random.Range(0, clips.Length)];
            // ใช้ PlayOneShot เพื่อให้เสียงซ้อนกันได้
            audioSource.PlayOneShot(clip);
        }
    }

    public void PlayDeathSound()
    {
        if (myData.deathSounds != null && myData.deathSounds.Length > 0)
        {
            AudioClip clip = myData.deathSounds[Random.Range(0, myData.deathSounds.Length)];
            // สร้างลำโพงชั่วคราว ณ จุดที่ตาย
            AudioSource.PlayClipAtPoint(clip, transform.position, myData.volume);
        }
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