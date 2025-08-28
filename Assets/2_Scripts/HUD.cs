using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    public enum InfoType { Health }
    public InfoType type;

    Text Text;
    Image HealthBarImage;

    private void Awake()
    {
        Text = GetComponent<Text>();
        HealthBarImage = GetComponent<Image>();
    }

    private void LateUpdate()
    {
        switch (type)
        {
            case InfoType.Health:
                float curHealth = GameManager.instance.Health;
                float maxHealth = GameManager.instance.MaxHealth;

                if (HealthBarImage != null)
                {
                    HealthBarImage.fillAmount = curHealth / maxHealth;
                }
                break;
        }
    }
}