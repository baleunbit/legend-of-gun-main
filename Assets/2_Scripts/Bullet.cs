using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Bullet : MonoBehaviour
{
    public float speed = 15f;
    public float lifeTime = 1.0f;

    float damage;
    int pierce;                 // 남은 관통 횟수
    Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        // 콜라이더는 프리팹에서 IsTrigger = On 권장
    }

    public void Init(float damage, int pierce, Vector2 _)
    {
        this.damage = Mathf.Max(0f, damage);
        this.pierce = Mathf.Max(1, pierce);
    }

    public void Setup(Vector2 direction)
    {
        rb.linearVelocity = direction.normalized * speed;
        Invoke(nameof(SelfDestruct), lifeTime);
    }

    void SelfDestruct()
    {
        if (this) Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        // ✅ 플레이어/룸 등은 완전 무시 (관통 소모 X)
        if (col.CompareTag("Player") || col.CompareTag("Room")) return;

        // ✅ 적에게만 작동 (관통 소모 O)
        var mob = col.GetComponentInParent<Mob>() ?? col.GetComponent<Mob>();
        if (mob != null)
        {
            mob.TakeDamage(Mathf.RoundToInt(damage));
            pierce--;
            if (pierce <= 0) Destroy(gameObject);
            return;
        }

        // 그 외 오브젝트(벽/소품 등)는 무시 (관통 소모 X, 파괴 X)
        // => 필요하면 여기서 처리 추가
    }
}
