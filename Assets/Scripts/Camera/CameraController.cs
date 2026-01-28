using UnityEngine;

public class CameraController : MonoBehaviour
{
    public enum CamState { Intro, Ready, Tracking }

    [Header("Targets")]
    public Transform playerTransform;
    public Transform introTarget; // จุดกึ่งกลางระหว่าง Player กับ Enemy (สร้าง Empty Object ไว้ตรง Lane 1)

    [Header("Settings")]
    public Vector3 trackingOffset = new Vector3(0, 5, -8);
    public float smoothSpeed = 5f;
    public float rotateSpeed = 20f;

    private CamState currentState = CamState.Intro;

    public void SetState(CamState newState)
    {
        currentState = newState;
    }

    void LateUpdate()
    {
        if (playerTransform == null) return;

        switch (currentState)
        {
            case CamState.Intro:
                transform.LookAt(introTarget);
                transform.RotateAround(introTarget.position, Vector3.up, rotateSpeed * Time.deltaTime);
                break;

            case CamState.Ready:
                Vector3 readyPos = playerTransform.position + trackingOffset;
                transform.position = Vector3.Lerp(transform.position, readyPos, smoothSpeed * Time.deltaTime);
                transform.LookAt(playerTransform.position + Vector3.forward * 5f); // มองไปข้างหน้าหน่อย
                break;

            case CamState.Tracking:
                Vector3 targetPos = playerTransform.position + trackingOffset;
                targetPos.x = 0;
                transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime);
                break;
        }
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
