using UnityEngine;

public class Mob : MonoBehaviour
{
    public float Speed = 2f;             // 이동 속도
    public float attackDamage = 5f;     // 공격 데미지
    public float attackCooldown = 1f;    // 공격 간격 (초)

    private bool isLive = true;
    private float attackTimer = 0f;

    public Rigidbody2D target;           // 플레이어의 Rigidbody2D
    private Rigidbody2D rigid;
    private SpriteRenderer spriter;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriter = GetComponent<SpriteRenderer>();
        isLive = true;
    }

    void FixedUpdate()
    {
        if (!isLive || target == null) return;

        // 플레이어 방향으로 이동
        Vector2 dirVec = target.position - rigid.position;
        Vector2 moveVec = dirVec.normalized * Speed * Time.fixedDeltaTime;
        rigid.MovePosition(rigid.position + moveVec);
        rigid.linearVelocity = Vector2.zero;
    }

    void LateUpdate()
    {
        if (!isLive || target == null) return;

        // 몹 방향 전환
        spriter.flipX = target.position.x < rigid.position.x;
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (!isLive) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackCooldown)
            {
                Player player = collision.gameObject.GetComponent<Player>();
                if (player != null)
                {
                    player.TakeDamage((int)attackDamage);
                }
                attackTimer = 0f;
            }
        }
    }

    public void OnDamage(float damage)
    {
        // 몹이 데미지를 받을 때 구현 (필요 시)
        Debug.Log($"{gameObject.name}이(가) {damage} 데미지를 입음");
        // 체력 관리 기능 추가 가능
    }
}
