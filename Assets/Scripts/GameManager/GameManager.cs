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

    [Header("UI Game Over")]
    public GameObject gameOverPanel;

    [Header("Score Settings")]
    public TextMeshProUGUI scoreText; // ลาก TextMeshPro ใส่ช่องนี้
    public TextMeshProUGUI highScoreText; // (Optional) ใส่ช่องนี้ถ้ามี HighScore

    private float score;
    private float scoreMultiplier = 5f; // ความเร็วในการขึ้นของคะแนนวิ่ง



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
        CheckHighScore();
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
        if (player.isGameActive && !isGameOver)
        {
            // คะแนนขึ้นตามเวลา x ความเร็ว
            score += scoreMultiplier * Time.deltaTime;

            // ถ้ามีไอเทม x2 ให้คูณเพิ่ม (ต้องไปดึงค่า isX2Active จาก Player มาเช็ค)
            if (player.isX2Active)
            {
                score += scoreMultiplier * Time.deltaTime; // บวกเพิ่มอีกเท่าตัว
            }

            UpdateScoreUI();
        }
    }

    // ฟังก์ชันสำหรับเรียกจากที่อื่นเพื่อบวกคะแนน (กินคน/เก็บของ)
    public void AddScore(int amount)
    {
        // ถ้ามี x2 ให้ได้คะแนนพิเศษเบิ้ลด้วย
        if (player.isX2Active) amount *= 2;

        score += amount;
        UpdateScoreUI();
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            // (int)score คือตัดทศนิยมทิ้ง
            scoreText.text = "Score: " + (int)score;
        }
    }

    // เรียกใช้ตอนจบเกมเพื่อบันทึก HighScore
    public void CheckHighScore()
    {
        float currentHighScore = PlayerPrefs.GetFloat("HighScore", 0);
        if (score > currentHighScore)
        {
            PlayerPrefs.SetFloat("HighScore", score);
            PlayerPrefs.Save();
            if (highScoreText != null) highScoreText.text = "New Best: " + (int)score;
        }
        else
        {
            if (highScoreText != null) highScoreText.text = "Best: " + (int)currentHighScore;
        }
    }

    // อย่าลืมเรียก CheckHighScore() ในฟังก์ชัน TriggerGameOver() ของคุณด้วย!
}
