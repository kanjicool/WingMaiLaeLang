using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Playlist Settings")]
    public AudioClip[] songList;
    public bool shuffle = false;
    public bool playOnStart = true; // *** เดี๋ยวเราจะไปติ๊กออกใน Inspector ***

    [Header("Volume")]
    [Range(0f, 1f)] public float volume = 0.5f;

    private AudioSource audioSource;
    private int currentSongIndex = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        audioSource = GetComponent<AudioSource>();
        audioSource.loop = false;
        audioSource.volume = volume;
    }

    void Start()
    {
        // เช็คตรงนี้: ถ้า PlayOnStart เป็น true ถึงจะเล่น
        // ถ้าเราติ๊กออก มันจะไม่เล่นเอง ต้องรอ GameManager สั่ง
        if (playOnStart && songList.Length > 0)
        {
            PlaySong(0);
        }
    }

    void Update()
    {
        if (!audioSource.isPlaying && audioSource.clip != null && Time.timeScale != 0)
        {
            PlayNextSong();
        }
    }

    // *** เพิ่มฟังก์ชันนี้: เพื่อให้ GameManager เรียกใช้ ***
    public void StartPlaylist()
    {
        currentSongIndex = 0; // รีเซ็ตไปเพลงแรก
        if (songList.Length > 0)
        {
            PlaySong(0);
        }
    }

    public void PlayNextSong()
    {
        if (songList.Length == 0) return;

        if (shuffle)
        {
            int newIndex = currentSongIndex;
            while (newIndex == currentSongIndex && songList.Length > 1)
            {
                newIndex = Random.Range(0, songList.Length);
            }
            currentSongIndex = newIndex;
        }
        else
        {
            currentSongIndex = (currentSongIndex + 1) % songList.Length;
        }

        PlaySong(currentSongIndex);
    }

    void PlaySong(int index)
    {
        if (index < 0 || index >= songList.Length) return;

        audioSource.clip = songList[index];
        audioSource.Play();
    }

    public void StopMusic()
    {
        audioSource.Stop();
    }
}