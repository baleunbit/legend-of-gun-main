using UnityEngine;
using UnityEngine.UI;


public class PlayerHungry : MonoBehaviour
{
    [Header("Hungry Gauge")]
    [Range(0, 100)] public int hungry = 0;
    public int maxHungry = 100;
    public float decayPerSecond = 0f; // 왜: 기획에 따라 점차 감소 옵션


    [Header("UI")]
    public Slider hungrySlider;


    void Start()
    {
        RefreshUI();
    }


    void Update()
    {
        if (decayPerSecond > 0f && hungry > 0)
        {
            float f = hungry - decayPerSecond * Time.deltaTime;
            hungry = Mathf.Clamp(Mathf.RoundToInt(f), 0, maxHungry);
            RefreshUI();
        }
    }


    public void Add(int amount)
    {
        hungry = Mathf.Clamp(hungry + amount, 0, maxHungry);
        RefreshUI();
    }


    void RefreshUI()
    {
        if (hungrySlider)
        {
            hungrySlider.minValue = 0;
            hungrySlider.maxValue = maxHungry;
            hungrySlider.value = hungry;
        }
    }
}