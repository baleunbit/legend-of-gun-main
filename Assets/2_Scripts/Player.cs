// Player.cs (핵심 부분만)
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
    public Image healthBarImage;

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
        ani.SetFloat("Speed", input.sqrMagnitude);
        if (input.x > 0) spriter.flipX = false;
        else if (input.x < 0) spriter.flipX = true;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        health = Mathf.Clamp(health - Mathf.Max(0, damage), 0, maxHealth);
        UpdateHealthBar();
        if (health <= 0) Die();
    }

    public void DieFromHunger()
    {
        if (isDead) return;
        // 배고파서 죽어도 체력 0으로 동기화 (다른 스크립트들이 health<=0 체크함)
        health = 0;
        UpdateHealthBar();
        Die();
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

        // 죽음 애니 트리거
        ani?.SetTrigger("Dead");

        // 게임오버 UI + 정지
        UIManager.Instance?.ShowDiedPanel();

        Died?.Invoke();
        Debug.Log("플레이어 사망");
    }
}
