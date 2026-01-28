using UnityEngine;
using TMPro;
using System.Collections;
using TMPro.Examples;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("References")]
    public PlayerController player;
    public AIController enemy;
    public CameraController cam;
    public TextMeshProUGUI countdownText;

    [Header("Settings")]
    public float introDuration = 3.0f;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        StartCoroutine(GameLoop());
    }

    IEnumerator GameLoop()
    {
        // --- PHASE 1: INTRO (กล้องหมุน) ---
        countdownText.gameObject.SetActive(false);
        player.isGameActive = false;
        enemy.isGameActive = false;

        cam.SetState(CameraController.CamState.Intro);
        yield return new WaitForSeconds(introDuration);

        // --- PHASE 2: PREPARE (กล้องเข้าที่) ---
        cam.SetState(CameraController.CamState.Ready);
        yield return new WaitForSeconds(1.5f);

        countdownText.gameObject.SetActive(true);

        countdownText.text = "3";
        yield return new WaitForSeconds(1f);

        countdownText.text = "2";
        yield return new WaitForSeconds(1f);

        countdownText.text = "1";
        yield return new WaitForSeconds(1f);

        // --- PHASE 3: GO! ---
        countdownText.text = "GO!";
        player.isGameActive = true;
        enemy.isGameActive = true;
        cam.SetState(CameraController.CamState.Tracking);

        yield return new WaitForSeconds(1f);
        countdownText.gameObject.SetActive(false);

    }

    void Update()
    { 
        
    }
}
