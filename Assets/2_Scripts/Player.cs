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

    public bool IsDead => isDead;
    public event Action Died;

    Vector2 input;
    SpriteRenderer spriter;
    Animator ani;
    Rigidbody2D rb;
    bool isDead = false;

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
        // 입력 기준 애니/플립
        ani?.SetFloat("Speed", input.sqrMagnitude);
        if (input.x > 0) spriter.flipX = false;
        else if (input.x < 0) spriter.flipX = true;
    }

    // ❌ (중복 피해 방지) 몹과의 충돌 도트데미지는 제거.
    //    적의 피해는 Mob.TryAttack()에서만 들어오게 한다.

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
        Died?.Invoke();
        Debug.Log("플레이어 사망");
    }
}
