using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EatBar : MonoBehaviour
{
    public static EatBar Instance { get; private set; }

    [Header("UI (Image fillAmount 사용)")]
    [SerializeField] Image fillImage;          // ⬅️ UI Image (Filled, Horizontal/Vertical)
    [SerializeField] TextMeshProUGUI valueText;

    [Header("수치")]
    [SerializeField] int maxFullness = 100;
    [SerializeField] int startFullness = 50;
    [SerializeField] float drainPerSecond = 2f;
    [SerializeField] float minSpoilFactor = 0.3f;
    [SerializeField] float spoilFullSeconds = 180f;

    int current;
    float t0;
    float drainAcc = 0f;

    void Awake() { if (Instance != null && Instance != this) { Destroy(gameObject); return; } Instance = this; }
    void Start() { current = Mathf.Clamp(startFullness, 0, maxFullness); t0 = Time.time; UpdateUI(); }

    void Update()
    {
        if (current <= 0) return;

        drainAcc += drainPerSecond * Time.deltaTime;   // 초당 값 누적
        if (drainAcc >= 1f)
        {
            int dec = Mathf.FloorToInt(drainAcc);      // 정수로만 깎기
            current = Mathf.Max(0, current - dec);
            drainAcc -= dec;

            UpdateUI();
            if (current == 0) { UIManager.Instance?.ShowDiedPanel(); return; }
        }
    }

    void UpdateUI()
    {
        float f = Mathf.Clamp01((float)current / maxFullness);
        if (fillImage) fillImage.fillAmount = f;        // ⬅️ 핵심 한 줄
        if (valueText) valueText.text = $"{current} / {maxFullness}";
    }

    public void AddFromEat(int baseGain)
    {
        if (current <= 0 || baseGain <= 0) return;
        float t = Mathf.Clamp01((Time.time - t0) / Mathf.Max(1f, spoilFullSeconds));
        float factor = Mathf.Lerp(1f, minSpoilFactor, t);
        int gain = Mathf.Max(1, Mathf.RoundToInt(baseGain * factor));
        current = Mathf.Min(maxFullness, current + gain);
        UpdateUI();
    }

    public void AddRaw(int delta)
    {
        current = Mathf.Clamp(current + delta, 0, maxFullness);
        UpdateUI();
        if (current == 0) UIManager.Instance?.ShowDiedPanel();
    }
}
