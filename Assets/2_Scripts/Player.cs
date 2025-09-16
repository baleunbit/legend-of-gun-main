using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 10f;
    private Vector2 input;

    SpriteRenderer spriter;
    Animator ani;
    Rigidbody2D rb;

    [Header("체력 설정")]
    public int maxHealth = 100;
    public int health = 100;
    private float mobDamageTimer = 0f;
    private bool isDead = false;    // 🔸 죽음 상태 플래그

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
        if (isDead) return; // 🔸 죽으면 이동 입력 무시

        // 이동 입력
        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
    }

    void FixedUpdate()
    {
        if (isDead) { rb.linearVelocity = Vector2.zero; return; } // 🔸 이동 멈춤
        rb.linearVelocity = input * moveSpeed;
    }

    private void LateUpdate()
    {
        ani.SetFloat("Speed", rb.linearVelocity.magnitude);
        if (input.x > 0)
            spriter.flipX = false;
        else if (input.x < 0)
            spriter.flipX = true;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (isDead) return;

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
        if (isDead) return;

        health -= damage;
        health = Mathf.Clamp(health, 0, maxHealth);
        Debug.Log($"플레이어가 데미지 {damage} 받음! 현재 체력: {health}");
        UpdateHealthBar();

        if (health <= 0)
            Die(); // 🔸 체력 0 → 죽음 처리
    }

    void UpdateHealthBar()
    {
        if (healthBarImage != null)
            healthBarImage.fillAmount = (float)health / maxHealth;
    }

    void Die()
    {
        if (isDead) return; // 중복 실행 방지
        isDead = true;

        rb.linearVelocity = Vector2.zero; // 이동 정지
        ani.SetTrigger("Dead");           // 🔸 Animator에서 트리거 발동

        Debug.Log("플레이어 사망!");
        // 여기서 GameOver UI 호출, 재시작 로직 등 추가 가능
    }
}
