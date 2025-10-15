using System;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [Header("이동")] public float moveSpeed = 10f;
    [Header("체력")] public int maxHealth = 100; public int health = 100; public Image healthBarImage;

    [Header("경험치 / 레벨")]
    [SerializeField] private int level = 1;
    [SerializeField] private int exp = 0;

    public int Level => level;
    public int Exp => exp;
    public int ExpToNext => GetExpToNext(level);

    public event Action<int, int, int> OnExpChanged;
    public event Action<int> OnLeveledUp;

    Rigidbody2D rb; SpriteRenderer spriter; Animator ani;
    Vector2 input; bool isDead = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriter = GetComponent<SpriteRenderer>();
        ani = GetComponent<Animator>();

        health = Mathf.Clamp(health, 0, maxHealth);
        UpdateHealthBar();

        UIManager.Instance?.SetExpUI(level, exp, ExpToNext);
        OnExpChanged?.Invoke(level, exp, ExpToNext);
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
        ani?.SetFloat("Speed", input.sqrMagnitude);
        if (input.x > 0) spriter.flipX = false; else if (input.x < 0) spriter.flipX = true;
    }

    // ===== 체력 =====
    public void TakeDamage(int dmg)
    {
        if (isDead) return;
        health = Mathf.Clamp(health - Mathf.Max(0, dmg), 0, maxHealth);
        UpdateHealthBar();
        if (health <= 0) Die();
    }
    public void DieFromHunger() { if (isDead) return; health = 0; UpdateHealthBar(); Die(); }
    void UpdateHealthBar() { if (healthBarImage) healthBarImage.fillAmount = (float)health / maxHealth; }
    void Die()
    {
        if (isDead) return;
        isDead = true; rb.linearVelocity = Vector2.zero; ani?.SetTrigger("Dead");
        UIManager.Instance?.ShowDiedPanel();
    }

    // ===== Bite로만 Exp 획득 =====
    public void AddExpFromBite(int amount = 1)
    {
        if (amount <= 0) return;
        exp += amount;

        while (exp >= ExpToNext)
        {
            exp -= ExpToNext;
            level++;
            OnLeveledUp?.Invoke(level);
            UIManager.Instance?.ShowLevelUpPanel();
        }
        UIManager.Instance?.SetExpUI(level, exp, ExpToNext);
        OnExpChanged?.Invoke(level, exp, ExpToNext);
    }

    // 1~3:6, 4~9:12, 10~14:15, 15+:18
    public int GetExpToNext(int lv)
    {
        if (lv <= 3) return 6;
        if (lv <= 9) return 12;
        if (lv <= 14) return 15;
        return 18;
    }

    public void ApplyLevelUpChoice(int idx)
    {
        switch (idx)
        {
            case 1: maxHealth += 10; health = maxHealth; UpdateHealthBar(); break;
            case 2: moveSpeed += 1f; break;
            case 3: /* 공격 속도 증가 등 */ break;
            case 4: /* 재장전 속도 증가 등 */ break;
        }
        UIManager.Instance?.HideLevelUpPanel(); // ✔ 패널만 닫기
    }
}
