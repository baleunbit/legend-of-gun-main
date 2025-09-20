using System;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [Header("이동")]
    public float moveSpeed = 10f;

    [Header("체력")]
    public int maxHealth = 100;
    public int health = 100;

    [Header("UI")]
    public Image healthBarImage;  // Filled Image

    // 외부에서 읽기/구독용
    public bool IsDead => isDead;
    public event Action Died;

    Vector2 input;
    SpriteRenderer spriter;
    Animator ani;
    Rigidbody2D rb;
    bool isDead = false;
    float mobDamageTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriter = GetComponent<SpriteRenderer>();
        ani = GetComponent<Animator>();

        health = Mathf.Clamp(health, 0, maxHealth);
        UpdateHealthBar();
    }

    void Update()
    {
        if (isDead) return;
        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
    }

    void FixedUpdate()
    {
        if (isDead) { rb.linearVelocity = Vector2.zero; return; }
        rb.linearVelocity = input * moveSpeed;
    }

    void LateUpdate()
    {
        ani?.SetFloat("Speed", rb.linearVelocity.magnitude);
        if (input.x > 0) spriter.flipX = false;
        else if (input.x < 0) spriter.flipX = true;
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (isDead) return;
        if (collision.collider.CompareTag("Mob"))
        {
            mobDamageTimer += Time.fixedDeltaTime;
            if (mobDamageTimer >= 1f)
            {
                TakeDamage(5);
                mobDamageTimer = 0f;
            }
        }
    }
    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Mob")) mobDamageTimer = 0f;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        health = Mathf.Clamp(health - Mathf.Max(0, damage), 0, maxHealth);
        UpdateHealthBar();
        if (health <= 0) Die();
    }

    void UpdateHealthBar()
    {
        if (healthBarImage) healthBarImage.fillAmount = (float)health / maxHealth;
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        ani?.SetTrigger("Dead");
        Died?.Invoke();                 // 총 등에게 알림
        Debug.Log("플레이어 사망");
    }
}
