using UnityEngine;

[ExecuteAlways] // ให้ทำงานใน Edit Mode โดยไม่ต้องกด Play
public class LaneDebugger : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("ชื่อ Tag ของพื้นที่จะให้วาดเส้น")]
    public string groundTag = "Ground";

    [Header("Visuals")]
    public bool showLanes = true;
    public Color centerLaneColor = Color.green;
    public Color sideLaneColor = Color.cyan;
    public Color borderColor = new Color(1, 1, 1, 0.3f); // สีขาวจางๆ
    public float lineYOffset = 0.1f; // ยกเส้นลอยเหนือพื้นนิดหน่อย กันภาพซ้อน (Z-fighting)

    private PlayerController _playerController;

    private void OnEnable()
    {
        _playerController = FindAnyObjectByType<PlayerController>();
    }

    private void OnDrawGizmos()
    {
        if (!showLanes) return;

        if (_playerController == null)
        {
            _playerController = FindAnyObjectByType<PlayerController>();
        }

        if (_playerController == null) return;

        float laneDist = 3.3f;

        if (GameManager.Instance != null)
        {
            laneDist = GameManager.Instance.laneDistance;
        }
        else
        {
            GameManager gm = FindAnyObjectByType<GameManager>();
            if (gm != null)
            {
                laneDist = gm.laneDistance;
            }
        }

        GameObject[] grounds = GameObject.FindGameObjectsWithTag(groundTag);

        if (grounds == null || grounds.Length == 0) return;

        // วาด Grid บนพื้นแต่ละชิ้น
        foreach (GameObject ground in grounds)
        {
            DrawGridOnGround(ground, laneDist);
        }
    }

    void DrawGridOnGround(GameObject groundObj, float laneDist)
    {
        // ใช้ Renderer Bounds เพื่อหาขนาดและความยาวของพื้นชิ้นนั้น
        Renderer r = groundObj.GetComponent<Renderer>();
        if (r == null) return;

        Bounds bounds = r.bounds;

        // จุดเริ่มต้น (Z min) และจุดสิ้นสุด (Z max) ของถนนชิ้นนี้
        float zStart = bounds.min.z;
        float zEnd = bounds.max.z;
        float yPos = bounds.max.y + lineYOffset; // วาดบนผิวถนนนิดนึง

        // --- วาดเส้น Center Lanes (0, -dist, +dist) ---
        // Lane 1 (Center)
        DrawLaneLine(0, zStart, zEnd, yPos, centerLaneColor);

        // Lane 0 (Left) & Lane 2 (Right)
        DrawLaneLine(-laneDist, zStart, zEnd, yPos, sideLaneColor);
        DrawLaneLine(laneDist, zStart, zEnd, yPos, sideLaneColor);

        // --- วาดเส้น Border แบ่งเลน (Optional) ---
        float borderOffset = laneDist / 2f;
        Gizmos.color = borderColor;

        // เส้นแบ่งระหว่างเลน
        DrawLineAtX(-borderOffset, zStart, zEnd, yPos); // ระหว่างซ้าย-กลาง
        DrawLineAtX(borderOffset, zStart, zEnd, yPos);  // ระหว่างกลาง-ขวา
        DrawLineAtX(-borderOffset - laneDist, zStart, zEnd, yPos); // ขอบซ้ายสุด
        DrawLineAtX(borderOffset + laneDist, zStart, zEnd, yPos);  // ขอบขวาสุด
    }

    void DrawLaneLine(float xPos, float zStart, float zEnd, float y, Color c)
    {
        Gizmos.color = c;
        Vector3 start = new Vector3(xPos, y, zStart);
        Vector3 end = new Vector3(xPos, y, zEnd);
        Gizmos.DrawLine(start, end);

        // วาดจุดกึ่งกลางเลน เพื่อให้รู้ว่าเป็นจุดยืน
        Vector3 center = new Vector3(xPos, y, (zStart + zEnd) / 2);
        Gizmos.DrawSphere(center, 0.2f);
    }

    void DrawLineAtX(float xPos, float zStart, float zEnd, float y)
    {
        Vector3 start = new Vector3(xPos, y, zStart);
        Vector3 end = new Vector3(xPos, y, zEnd);
        Gizmos.DrawLine(start, end);
    }
}