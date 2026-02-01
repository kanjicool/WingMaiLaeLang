using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BuffDisplayUI : MonoBehaviour
{
    public GameObject buffContainerRoot;

    [Header("Buff Badge Objects")]
    public GameObject iconX2;
    public GameObject iconShield; // เปลี่ยนชื่อจาก Invincible เป็น Shield
    public GameObject iconSlowDrain;

    private bool lastX2, lastShield, lastSlow;

    void Update()
    {
        if (PlayerController.Instance == null || !PlayerController.Instance.isGameActive)
        {
            buffContainerRoot.SetActive(false);
            return;
        }

        var pc = PlayerController.Instance;

        // เช็คสถานะปัจจุบันจาก PlayerController
        bool x2 = pc.isX2Active;
        bool shield = pc.hasShield; // ใช้ตัวแปรใหม่
        bool slow = pc.isSlowDrainActive;

        // --- 1. ระบบ Pop-up ขอบขาขึ้น (เพิ่งเก็บได้) ---
        if (x2 && !lastX2) TriggerPop(iconX2);
        if (shield && !lastShield) TriggerPop(iconShield); // เด้งเมื่อได้โล่ใหม่
        if (slow && !lastSlow) TriggerPop(iconSlowDrain);

        // --- 2. อัปเดตสถานะการแสดงผลและกระพริบ ---
        UpdateBadge(iconX2, x2, pc.isX2Warning);
        UpdateBadge(iconShield, shield, pc.isShieldWarning); // ใช้ Warning ของโล่
        UpdateBadge(iconSlowDrain, slow, pc.isSlowWarning);

        // --- 3. จัดการ Container ---
        // ถ้ามีบัฟใดๆ ทำงานอยู่ให้แสดง Root
        buffContainerRoot.SetActive(x2 || shield || slow);

        // บันทึกสถานะไว้เช็ค Frame หน้า
        lastX2 = x2; lastShield = shield; lastSlow = slow;
    }

    void UpdateBadge(GameObject obj, bool isActive, bool isWarning)
    {
        if (obj == null) return;

        obj.SetActive(isActive);
        if (!isActive) return;

        CanvasGroup cg = obj.GetComponent<CanvasGroup>();
        if (cg == null) cg = obj.AddComponent<CanvasGroup>();

        if (isWarning)
        {
            // กระพริบถี่ๆ เมื่อใกล้หมดเวลา
            cg.alpha = 0.6f + Mathf.Sin(Time.time * 15f) * 0.4f;
        }
        else
        {
            cg.alpha = 1f;
        }
    }

    void TriggerPop(GameObject obj)
    {
        if (obj == null) return;
        // ใช้ชื่อ Coroutine แบบระบุ Object เพื่อให้เด้งแยกกันได้ถ้าเก็บพร้อมกัน
        StartCoroutine(PopRoutine(obj));
    }

    IEnumerator PopRoutine(GameObject obj)
    {
        float timer = 0;
        float duration = 0.2f;
        Vector3 nativeScale = Vector3.one;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float s = Mathf.Lerp(0, 1.2f, timer / duration);
            obj.transform.localScale = nativeScale * s;
            yield return null;
        }
        obj.transform.localScale = nativeScale;
    }
}