using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Spec")]
    public float damage = 5f;        // 총알 데미지
    public int per = 1;              // 피어싱 횟수(0이면 즉시 파괴, 음수(-1)면 무한 피어스)
    public float speed = 15f;        // 이동 속도

    Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// 기존 코드 호환: 발사 전에 외부에서 데미지/피어스/방향 설정
    /// </summary>
    public void Init(float damage, int per, Vector3 dir)
    {
        this.damage = damage;
        this.per = per;

        // ⚠ angularVelocity는 float이며, 여기서 굳이 회전 줄 필요 없으면 지움/무시
        // rb가 아직 null일 수 있으므로 사용하지 않는 게 안전
        // 필요하면: if (rb) rb.angularVelocity = 0f;
    }

    /// <summary>
    /// 실제 이동 시작. direction은 정규화 안 되어도 됨(내부에서 velocity 설정)
    /// </summary>
    public void Setup(Vector2 direction)
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = direction.normalized * speed;

        // 1초 뒤 자동 삭제(관통 잔탄이 남아도 정리)
        Destroy(gameObject, 1f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 플레이어/룸/기타 특정 태그는 충돌 무시
        if (collision.CompareTag("Player")) return;
        if (collision.CompareTag("Room") || collision.name == "Room") return;
        if (collision.CompareTag("GameObject")) return;     // 네가 태그 관리 중이라면 유지

        // 몹 타격 처리
        if (collision.CompareTag("Mob"))
        {
            // 몹 찾기(자식/부모 어디에 붙어있든 커버)
            var mob = collision.GetComponentInParent<Mob>() ?? collision.GetComponent<Mob>();
            if (mob != null)
            {
                // 데미지 적용 (Mob은 int 기반이라 반올림)
                mob.TakeDamage(Mathf.RoundToInt(damage));
            }

            // 피어싱 처리
            if (per >= 0)
            {
                per--;                 // 사용 1회 차감
                if (per <= 0)
                {
                    Destroy(gameObject);
                    return;
                }
            }

            // per < 0 이면 무한 관통 → 총알 유지
            return;
        }

        // 그 외(벽/장애물 등)에는 총알 삭제
        Destroy(gameObject);
    }
}
