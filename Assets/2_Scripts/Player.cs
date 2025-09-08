using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 3f;
    private Vector2 input;

    SpriteRenderer spriter;
    Animator ani;
    Rigidbody2D rb;

    [Header("체력 설정")]
    public int maxHealth = 100;
    public int health = 100;
    private float mobDamageTimer = 0f;

    [Header("UI")]
    public Image healthBarImage; // 체력바 Image (FillAmount로 제어)

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriter = GetComponent<SpriteRenderer>();
        ani = GetComponent<Animator>();

        health = maxHealth;
        UpdateHealthBar();
    }

    void Update()
    {
        // 이동 입력
        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
    }

    void FixedUpdate()
    {
        rb.linearVelocity = input * moveSpeed;
    }

    private void LateUpdate()
    {
        if (input.x > 0)
            spriter.flipX = false;
        else if (input.x < 0)
            spriter.flipX = true;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Mob"))
        {
            mobDamageTimer += Time.fixedDeltaTime;
            if (mobDamageTimer >= 1f)
            {
                TakeDamage(5);
                mobDamageTimer = 0f;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Mob"))
        {
            mobDamageTimer = 0f;
        }
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        health = Mathf.Clamp(health, 0, maxHealth);
        Debug.Log($"플레이어가 데미지 {damage} 받음! 현재 체력: {health}");
        UpdateHealthBar();
    }

    void UpdateHealthBar()
    {
        Debug.Log($"fill={healthBarImage.fillAmount}");
        if (healthBarImage != null)
        {
            healthBarImage.fillAmount = (float)health / maxHealth;
        }
    }
}
