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
    public Image healthBarImage;

    [Header("경험치 / 레벨")]
    [SerializeField] private int level = 1;
    [SerializeField] private int exp = 0;

    public int Level => level;
    public int Exp => exp;
    public int ExpToNext => GetExpToNext(level);

    public event Action<int, int, int> OnExpChanged; // (level, exp, expToNext)
    public event Action<int> OnLeveledUp;
    public bool IsDead => isDead;
    public event Action Died;

    Rigidbody2D rb;
    SpriteRenderer spriter;
    Animator ani;
    Vector2 input;
    bool isDead = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriter = GetComponent<SpriteRenderer>();
        ani = GetComponent<Animator>();

        health = Mathf.Clamp(health, 0, maxHealth);
        UpdateHealthBar();

        OnExpChanged?.Invoke(level, exp, ExpToNext);
        UIManager.Instance?.SetExpUI(level, exp, ExpToNext);
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

    // === 체력 관리 ===
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
        health = 0;
        UpdateHealthBar();
        Die();
    }

    void UpdateHealthBar()
    {
        if (healthBarImage)
            healthBarImage.fillAmount = (float)health / maxHealth;
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        ani?.SetTrigger("Dead");

        UIManager.Instance?.ShowDiedPanel();
        Died?.Invoke();
        Debug.Log("플레이어 사망");
    }

    // === Bite 시 경험치 획득 ===
    public void AddExpFromBite(int amount = 1)
    {
        if (amount <= 0) return;
        exp += amount;

        // 연속 레벨업 가능
        while (exp >= ExpToNext)
        {
            exp -= ExpToNext;
            level++;

            OnLeveledUp?.Invoke(level);
            UIManager.Instance?.ShowLevelUpPanel();
        }

        OnExpChanged?.Invoke(level, exp, ExpToNext);
        UIManager.Instance?.SetExpUI(level, exp, ExpToNext);
    }

    // 경험치 요구량 규칙
    public int GetExpToNext(int lv)
    {
        if (lv <= 3) return 6;      // 1~3레벨
        if (lv <= 9) return 12;     // 4~9레벨
        if (lv <= 14) return 15;    // 10~14레벨
        return 18;                  // 15레벨 이상
    }

    // 레벨업 시 버튼 눌렀을 때 효과 적용
    public void ApplyLevelUpChoice(int choiceIndex)
    {
        switch (choiceIndex)
        {
            case 1:
                maxHealth += 10;
                health = maxHealth;
                break;
            case 2:
                moveSpeed += 1f;
                break;
            case 3:
                // 예시: 공격속도 증가
                break;
            case 4:
                // 예시: 리로드 속도 증가
                break;
        }

        Debug.Log($"능력 {choiceIndex} 선택됨");
        UIManager.Instance?.HideLevelUpPanel();
    }
}
