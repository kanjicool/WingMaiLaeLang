using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Source")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource SFXSource;

    [Header("Audio Clip")]
    public AudioClip background;
    public AudioClip zombiesound;

    // ตัวแปรสำหรับเก็บ Coroutine ของเสียงซอมบี้ เพื่อเอาไว้สั่งหยุดทีหลัง
    private Coroutine zombieCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        musicSource.clip = background;
        musicSource.loop = true;
        musicSource.Play();

        // เก็บ Coroutine ใส่ตัวแปรไว้
        zombieCoroutine = StartCoroutine(PlayZombieAmbientSound());
    }

    public void PlaySFX(AudioClip clip)
    {
        SFXSource.PlayOneShot(clip);
    }

    IEnumerator PlayZombieAmbientSound()
    {
        while (true)
        {
            float waitTime = Random.Range(5f, 10f);
            yield return new WaitForSeconds(waitTime);

            if (zombiesound != null)
            {
                SFXSource.PlayOneShot(zombiesound, 0.7f);
            }
        }
    }

    // --- เพิ่มฟังก์ชันหยุดเสียงซอมบี้ ---
    public void StopZombieSound()
    {
        // 1. สั่งหยุดลูปสุ่ม (ถ้ามันทำงานอยู่)
        if (zombieCoroutine != null)
        {
            StopCoroutine(zombieCoroutine);
            zombieCoroutine = null; // เคลียร์ค่าทิ้ง
        }

        // 2. สั่งหยุดเสียงที่กำลังร้องอยู่ ณ ตอนนั้น (ถ้าต้องการให้เงียบทันที)
        SFXSource.Stop();
    }

    // ฟังก์ชันหยุดทุกอย่าง (แนะนำให้เรียกตัวนี้ตอนจบเกม)
    public void StopAllSound()
    {
        StopMusic();
        StopZombieSound();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void SetVolume(float volume)
    {
        musicSource.volume = volume;
    }
}