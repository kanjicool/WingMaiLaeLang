using UnityEngine;
using TMPro;
using System.Collections;
using TMPro.Examples;
using UnityEngine.Playables;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("References")]
    public PlayerController player;
    public AIController enemy;
    public CameraController cam;
    public TextMeshProUGUI countdownText;

    [Header("Timeline Settings")]
    public PlayableDirector introTimeline;

    [Header("Settings")]
    public float introDuration = 3.0f;

    [Header("Game Loop")]
    public int currentRound = 1;
    public int maxRounds = 4;
    public bool isGameOver = false;

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

        // TODO: ขึ้นหน้าจอ Game Over UI
        StopAllCoroutines(); // หยุดเกม

    }


    void StartGame()
    {
        Debug.Log("Game Started!");
        player.isGameActive = true;
    }

    public void EnemyFinished()
    {
        Debug.Log("Enemy win!");
        // อาจจะขึ้น UI แจ้งเตือนว่า "โดนแซง!"
    }

    void Update()
    { 
        
    }
}
