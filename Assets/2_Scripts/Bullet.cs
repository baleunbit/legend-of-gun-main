using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Bullet : MonoBehaviour
{
    [Header("Spec")]
    public float damage = 5f;     // Gun.Init에서 덮어씀
    public int per = 1;           // 관통 횟수(0이면 이번 히트 후 삭제, -1이면 무한 관통)
    public float speed = 15f;

    Rigidbody2D rb;

    // 다중 콜라이더 적 중복 히트 방지용
    int lastHitMobId = 0;
    float lastHitTime = -999f;
    const float sameMobHitGuard = 0.05f; // 50ms 내 재히트 무시

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // 트리거 권장: 콜라이더 isTrigger = true, RB는 Kinematic 권장
    }

    public void Init(float damage, int per, Vector3 dir)
    {
        this.damage = damage;
        this.per = per;
        // angularVelocity 같은 건 사용 안 함(불필요·혼란 요인)
    }

    public void Setup(Vector2 direction)
    {
        rb.linearVelocity = direction.normalized * speed;
        Destroy(gameObject, 1f); // 수명
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 무시할 태그들
        if (other.CompareTag("Player")) return;
        if (other.CompareTag("Room") || other.name == "Room") return;
        if (other.CompareTag("GameObject")) return;

        // 몹 히트 처리
        if (other.CompareTag("Mob"))
        {
            var mob = other.GetComponentInParent<Mob>() ?? other.GetComponent<Mob>();
            if (mob != null)
            {
                int id = mob.GetInstanceID();
                if (id == lastHitMobId && Time.time - lastHitTime < sameMobHitGuard)
                    return; // 같은 프레임/짧은 시간 내 중복 히트 방지

                mob.TakeDamage(Mathf.RoundToInt(damage));
                lastHitMobId = id;
                lastHitTime = Time.time;
            }

            if (per >= 0)
            {
                per--;
                if (per <= 0)
                {
                    Destroy(gameObject);
                    return;
                }
            }
            return; // 관통이면 계속 진행
        }

        // 그 외(벽/장애물 등) 맞으면 삭제
        Destroy(gameObject);
    }
}
