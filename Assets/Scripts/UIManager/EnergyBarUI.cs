using UnityEngine;
using UnityEngine.UI;

public class EnergyBarUI : MonoBehaviour
{
    public Slider energySlider;
    public Image fillImage;       // ลาก Object "Fill" มาใส่ช่องนี้
    public Gradient colorGradient; // สร้างเฉดสีใน Inspector
    public GameObject visualRoot;

    void Start()
    {
        if (visualRoot != null) visualRoot.SetActive(false);
    }

    void Update()
    {
        if (PlayerController.Instance == null) return;

        if (PlayerController.Instance.isGameActive)
        {
            if (visualRoot != null && !visualRoot.activeSelf)
                visualRoot.SetActive(true);

            float currentEnergy = PlayerController.Instance.currentEnergy;
            float maxEnergy = PlayerController.Instance.maxEnergy;

            // 1. อัปเดตค่า Slider
            energySlider.maxValue = maxEnergy;
            energySlider.value = currentEnergy;

            // 2. คำนวณหาอัตราส่วน 0.0 - 1.0
            float energyPercent = currentEnergy / maxEnergy;

            // 3. เปลี่ยนสีตาม Gradient
            if (fillImage != null)
            {
                fillImage.color = colorGradient.Evaluate(energyPercent);
            }
        }
    }
}