using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Slider = UnityEngine.UI.Slider;

public class HUD : MonoBehaviour
{
    public enum InfoType { Health }
    public InfoType type;

    Text Text;
    Slider mySlider;

    private void Awake()
    {
        Text = GetComponent<Text>();
        mySlider = GetComponent<Slider>();
    }

    private void LateUpdate()
    {
        switch (type)
        {
            case InfoType.Health:
        float curHealth = GameManager.instance.Health;
        float maxHealth = GameManager.instance.MaxHealth;
        mySlider.value = curHealth / maxHealth;
        break;
    }
    }
}