using UnityEngine;
#if UNITY_EDITOR
using UnityEditor; // ต้องใส่ #if เพื่อไม่ให้ Error เวลา Build เกม
#endif

[ExecuteAlways]
public class LaneDebugger : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("ชื่อ Tag ของพื้นที่จะให้วาดเส้น")]
    public string groundTag = "Ground";

    [Header("Visuals")]
    public bool showLanes = true;
    public bool showLabels = true; // เพิ่มตัวเลือกเปิด/ปิดชื่อเลน
    public Color centerLaneColor = Color.green;
    public Color sideLaneColor = Color.cyan;
    public Color borderColor = new Color(1, 1, 1, 0.3f);
    public float lineYOffset = 0.1f;

    private PlayerController _playerController;

    private void OnEnable()
    {
        _playerController = FindAnyObjectByType<PlayerController>();
    }

    private void OnDrawGizmos()
    {
        if (!showLanes) return;

        // ... (Logic การหา GameManager เหมือนเดิม) ...
        float laneDist = 3.3f;
        if (GameManager.Instance != null)
        {
            laneDist = GameManager.Instance.laneDistance;
        }
        else
        {
            GameManager gm = FindAnyObjectByType<GameManager>();
            if (gm != null) laneDist = gm.laneDistance;
        }

        GameObject[] grounds = GameObject.FindGameObjectsWithTag(groundTag);
        if (grounds == null || grounds.Length == 0) return;

        foreach (GameObject ground in grounds)
        {
            DrawGridOnGround(ground, laneDist);
        }
    }

    void DrawGridOnGround(GameObject groundObj, float laneDist)
    {
        Renderer r = groundObj.GetComponent<Renderer>();
        if (r == null) return;

        Bounds bounds = r.bounds;
        float zStart = bounds.min.z;
        float zEnd = bounds.max.z;
        float yPos = bounds.max.y + lineYOffset;

        // --- วาดเส้นและชื่อเลน ---

        // Lane 0 (Left)
        DrawLaneLine(-laneDist, zStart, zEnd, yPos, sideLaneColor, "Left (0)");

        // Lane 1 (Center)
        DrawLaneLine(0, zStart, zEnd, yPos, centerLaneColor, "Center (1)");

        // Lane 2 (Right)
        DrawLaneLine(laneDist, zStart, zEnd, yPos, sideLaneColor, "Right (2)");

        // --- วาดเส้น Border (เหมือนเดิม) ---
        float borderOffset = laneDist / 2f;
        Gizmos.color = borderColor;
        DrawLineAtX(-borderOffset, zStart, zEnd, yPos);
        DrawLineAtX(borderOffset, zStart, zEnd, yPos);
        DrawLineAtX(-borderOffset - laneDist, zStart, zEnd, yPos);
        DrawLineAtX(borderOffset + laneDist, zStart, zEnd, yPos);
    }

    // เพิ่ม Parameter "label" เข้ามา
    void DrawLaneLine(float xPos, float zStart, float zEnd, float y, Color c, string label)
    {
        Gizmos.color = c;
        Vector3 start = new Vector3(xPos, y, zStart);
        Vector3 end = new Vector3(xPos, y, zEnd);
        Gizmos.DrawLine(start, end);

        // วาดจุดกึ่งกลาง
        Vector3 center = new Vector3(xPos, y, (zStart + zEnd) / 2);
        Gizmos.DrawSphere(center, 0.2f);

        // *** ส่วนที่เพิ่ม: วาดชื่อเลน ***
#if UNITY_EDITOR
        if (showLabels)
        {
            // ปรับ Style ตัวหนังสือให้อ่านง่าย
            GUIStyle style = new GUIStyle();
            style.normal.textColor = c; // สีเดียวกับเส้น
            style.fontSize = 15;        // ขนาดตัวหนังสือ
            style.fontStyle = FontStyle.Bold;
            style.alignment = TextAnchor.MiddleCenter;

            // วาด Label ที่จุดกึ่งกลาง
            Handles.Label(center + Vector3.up * 0.5f, label, style);

            // วาด Label ที่หัวถนนและท้ายถนนด้วย จะได้เห็นตลอดเวลา
            Handles.Label(start + Vector3.up * 0.5f, label, style);
        }
#endif
    }

    void DrawLineAtX(float xPos, float zStart, float zEnd, float y)
    {
        Vector3 start = new Vector3(xPos, y, zStart);
        Vector3 end = new Vector3(xPos, y, zEnd);
        Gizmos.DrawLine(start, end);
    }
}