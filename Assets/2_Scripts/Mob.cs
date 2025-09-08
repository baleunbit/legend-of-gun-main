using UnityEngine;

public class Mob : MonoBehaviour
{
    public float Speed = 2f;             // �̵� �ӵ�
    public float attackDamage = 5f;     // ���� ������
    public float attackCooldown = 1f;    // ���� ���� (��)

    private bool isLive = true;
    private float attackTimer = 0f;

    public Rigidbody2D target;           // �÷��̾��� Rigidbody2D
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

        // �÷��̾� �������� �̵�
        Vector2 dirVec = target.position - rigid.position;
        Vector2 moveVec = dirVec.normalized * Speed * Time.fixedDeltaTime;
        rigid.MovePosition(rigid.position + moveVec);
        rigid.linearVelocity = Vector2.zero;
    }

    void LateUpdate()
    {
        if (!isLive || target == null) return;

        // �� ���� ��ȯ
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
        // ���� �������� ���� �� ���� (�ʿ� ��)
        Debug.Log($"{gameObject.name}��(��) {damage} �������� ����");
        // ü�� ���� ��� �߰� ����
    }
}
