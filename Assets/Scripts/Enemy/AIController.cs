using UnityEngine;
using System.Collections;

public class AIController : MonoBehaviour
{
    [Header("Settings")]
    public float forwardSpeed = 10f;
    public float laneDistance = 4f;
    public float laneSwitchSpeed = 5f;
    public int startLane = 2; // Enemy เริ่มขวา (Lane 2)

    public bool isGameActive = false;
    private int currentLane;
    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        currentLane = startLane;

        // Set ตำแหน่งเริ่มต้น
        float startX = (currentLane - 1) * laneDistance;
        transform.position = new Vector3(startX, transform.position.y, transform.position.z);
    }

    void Update()
    {
        if (!isGameActive) return;

        // 1. วิ่งไปข้างหน้า
        Vector3 move = transform.forward * forwardSpeed * Time.deltaTime;

        // 2. คำนวณตำแหน่งเลน (เหมือน Player)
        float targetX = (currentLane - 1) * laneDistance;
        Vector3 newPos = transform.position;
        newPos.x = Mathf.Lerp(newPos.x, targetX, laneSwitchSpeed * Time.deltaTime);

        move.x = newPos.x - transform.position.x;
        move.y = -9.81f * Time.deltaTime; // Gravity

        controller.Move(move);

        // เช็ค Aggressive Start (ทำครั้งเดียว)
        if (isGameActive && !hasBullied)
        {
            StartCoroutine(BullyRoutine());
        }
    }

    private bool hasBullied = false;

    IEnumerator BullyRoutine()
    {
        hasBullied = true;

        // รอ 1-2 วินาทีหลังจากออกตัว
        yield return new WaitForSeconds(1.5f);

        // ตัดสินใจแย่งเลนกลาง! (Lane 1)
        Debug.Log("AI: กำลังเบียดไปเลนกลาง!");
        currentLane = 1;
    }
}