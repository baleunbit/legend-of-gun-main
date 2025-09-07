using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class player : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 3f;
    private Vector2 Input;

    SpriteRenderer spriter;
    Animator ani;
    Rigidbody2D rb;

    [Header("체력 설정")]
    public int maxHealth = 100;
    public int health = 100;
    private float mobDamageTimer = 0f;

    [Header("UI")]
    public Slider healthBar; // 에디터에서 Health Bar(Slider) 연결

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriter = GetComponent<SpriteRenderer>();
        ani = GetComponent<Animator>();

        health = maxHealth;
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = health;
        }
    }

    void Update()
    {
       
    }

    private void LateUpdate()
    {
        if (Input.x > 0)
        {
            spriter.flipX = false; // 오른쪽으로 이동
        }
        else if (Input.x < 0)
        {
            spriter.flipX = true; // 왼쪽으로 이동
        }

    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        Input = new Vector2(UnityEngine.Input.GetAxisRaw("Horizontal"), UnityEngine.Input.GetAxisRaw("Vertical")).normalized;
        rb.linearVelocity = Input * moveSpeed;
    }

    // Mob 태그가 있는 오브젝트에 닿아 있으면 1초마다 5 피해
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Mob"))
        {
            mobDamageTimer += Time.fixedDeltaTime;
            if (mobDamageTimer >= 1f)
            {
                health -= 5;
                mobDamageTimer = 0f;
                Debug.Log("플레이어가 몹에 닿아 피해를 입음! 현재 체력: " + health);
                UpdateHealthBar();
            }
        }
    }

    // 충돌이 끝나면 타이머 초기화
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Mob"))
        {
            mobDamageTimer = 0f;
        }
    }

    // Health Bar 갱신 함수
    private void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.value = health;
        }
    }
}
