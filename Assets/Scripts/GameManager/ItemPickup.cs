using UnityEngine;

public class ItemPickup : MonoBehaviour
{

    public string itemType;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        // ตรวจสอบว่าสิ่งที่มาชนมี Tag เป็น "Player" หรือไม่
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                // เรียกใช้ฟังก์ชัน CollectItem และส่งประเภทไอเท็มไป
                player.CollectItem(itemType);
            }

            // เล่นเสียง (ถ้ามี) แล้วทำลายไอเท็มทิ้ง
            Destroy(gameObject);
        }
    }
}
