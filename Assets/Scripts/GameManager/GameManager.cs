using UnityEngine;
using TMPro;
using System.Collections;
using TMPro.Examples;
using UnityEngine.Playables;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Global Lane Settings")]
    public float laneDistance = 3.3f;
    public float laneSwitchSpeed = 10f;


    [Header("References")]
    public PlayerController player;
    public TextMeshProUGUI countdownText;

    [Header("Timeline Settings")]
    public PlayableDirector introTimeline;

    [Header("Settings")]
    public float introDuration = 3.0f;

    [Header("Game Loop")]
    public bool isGameOver = false;

    [Header("UI Game Over")] // ใส่หัวข้อให้หาง่ายๆ
    public GameObject gameOverPanel; // <--- บรรทัดสำคัญที่ขาดไป!

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {

        player.isGameActive = false;

        if (countdownText != null) countdownText.gameObject.SetActive(false);

        if (introTimeline != null)
        {
            introTimeline.stopped += OnCutsceneFinished;

            introTimeline.Play();
        }
        else
        {
            StartGame();
        }

    }

    void OnCutsceneFinished(PlayableDirector director)
    {
        if (this == null || gameObject == null) return;

        introTimeline.stopped -= OnCutsceneFinished;
        StartCoroutine(StartCountdownRoutine());
    }

    IEnumerator StartCountdownRoutine()
    {
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
            countdownText.text = "3";
            yield return new WaitForSeconds(1f);
            countdownText.text = "2";
            yield return new WaitForSeconds(1f);
            countdownText.text = "1";
            yield return new WaitForSeconds(1f);
            countdownText.text = "GO!";
        }

        StartGame();

        if (countdownText != null)
        {
            yield return new WaitForSeconds(1f);
            countdownText.gameObject.SetActive(false);
        }
    }


    public void TriggerGameOver()
    {
        if (isGameOver) return;
        isGameOver = true;
        Debug.Log("GAME OVER - All Zombies Dead");

        StopAllCoroutines();

        // --- ส่วนที่ต้องเพิ่มเข้าไปครับ ---
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            Time.timeScale = 0f;
        }

        // ถ้ามีระบบเสียง อยากให้เสียงเงียบด้วย ให้ใส่ตรงนี้
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopAllSound();
        }
    }

    // ฟังก์ชันสำหรับปุ่ม Main Menu
    public void GoToMainMenu()
    {
        Time.timeScale = 1f; // สำคัญมาก! ต้องให้เวลากลับมาเดินก่อนเปลี่ยนฉาก
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    void StartGame()
    {
        Debug.Log("Game Started!");
        player.isGameActive = true;
    }

    public void EnemyFinished()
    {
        Debug.Log("Enemy win!");
        // ÍÒ¨¨Ð¢Öé¹ UI á¨é§àµ×Í¹ÇèÒ "â´¹á«§!"
    }

    void Update()
    { 
        
    }
}
