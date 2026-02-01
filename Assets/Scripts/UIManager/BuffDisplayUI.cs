using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BuffDisplayUI : MonoBehaviour
{
    public GameObject buffContainerRoot;

    [Header("Buff Badge Objects")]
    //public GameObject iconX2;
    //public GameObject iconInvincible;
    //public GameObject iconSpeed;
    public GameObject iconSlowDrain;

    private bool lastX2, lastInvin, lastSpeed, lastSlow;

    void Update()
    {
        if (PlayerController.Instance == null || !PlayerController.Instance.isGameActive)
        {
            buffContainerRoot.SetActive(false);
            return;
        }

        var pc = PlayerController.Instance;

        // เช็คสถานะปัจจุบัน
        bool x2 = pc.isX2Active;
        bool invin = pc.isInvincible;
        bool speed = pc.isSpeedBoosted;
        bool slow = pc.isSlowDrainActive;

        // --- 1. ระบบ Pop-up ขอบขาขึ้น (เพิ่งเก็บได้) ---
        //if (x2 && !lastX2) TriggerPop(iconX2);
        //if (invin && !lastInvin) TriggerPop(iconInvincible);
        //if (speed && !lastSpeed) TriggerPop(iconSpeed);
        if (slow && !lastSlow) TriggerPop(iconSlowDrain);

        // --- 2. อัปเดตสถานะการแสดงผลและกระพริบ ---
        //UpdateBadge(iconX2, x2, pc.isX2Warning);
        //UpdateBadge(iconInvincible, invin, pc.isInvinWarning);
        //UpdateBadge(iconSpeed, speed, pc.isSpeedWarning);
        UpdateBadge(iconSlowDrain, slow, pc.isSlowWarning);

        // --- 3. จัดการ Container ---
        buffContainerRoot.SetActive(x2 || invin || speed || slow);

        // บันทึกสถานะไว้เช็ค Frame หน้า
        lastX2 = x2; lastInvin = invin; lastSpeed = speed; lastSlow = slow;
    }

    void UpdateBadge(GameObject obj, bool isActive, bool isWarning)
    {
        obj.SetActive(isActive);
        if (!isActive) return;

        CanvasGroup cg = obj.GetComponent<CanvasGroup>();
        if (cg == null) cg = obj.AddComponent<CanvasGroup>();

        if (isWarning)
        {
            // กระพริบโดยใช้ค่า Sin Wave (เปลี่ยน Alpha 0.2 ถึง 1.0)
            cg.alpha = 0.6f + Mathf.Sin(Time.time * 15f) * 0.4f;
        }
        else
        {
            cg.alpha = 1f;
        }
    }

    void TriggerPop(GameObject obj)
    {
        StopCoroutine("PopRoutine"); // ป้องกัน Coroutine ซ้อนกันถ้าเก็บซ้ำ
        StartCoroutine(PopRoutine(obj));
    }

    IEnumerator PopRoutine(GameObject obj)
    {
        float timer = 0;
        float duration = 0.2f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            // เด้งจาก 0 ไป 1.2 แล้วกลับมา 1.0 ให้ดูมีแรงกระแทก
            float s = Mathf.Lerp(0, 1.2f, timer / duration);
            obj.transform.localScale = new Vector3(s, s, s);
            yield return null;
        }
        obj.transform.localScale = Vector3.one;
    }
}