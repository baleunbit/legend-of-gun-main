using UnityEngine;

public class Mob : MonoBehaviour
{
    [Header("�̵�")]
    public float Speed = 7.5f;

    [Header("����")]
    public int minDamage = 3;
    public int maxDamage = 5;
    public float attackCooldown = 1f;

    [Header("Ž�� (�߰� �������� ���)")]
    public float detectRadius = 4f;           // ���� ����(û��/�ֺ�)
    public float viewDistance = 6f;           // �þ� �Ÿ�
    [Range(0, 180)] public float fovAngle = 80f; // �¿� ��ģ �þ߰�
    public LayerMask obstacleMask;            // ��/���� ���̾�(���� üũ)

    [Header("����")]
    public Rigidbody2D target;                // Player�� Rigidbody2D (�ν����Ϳ� ����)

    private bool isLive = true;
    private bool hasSpotted = false;          // �� �� �� �߰��ϸ� ��� true
    private float nextAttackTime = 0f;        // ����ð� ��� ��ٿ�
    private bool dealtThisFixed = false;      // �� ���������� 1ȸ ����

    private Rigidbody2D rigid;
    private SpriteRenderer spriter;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriter = GetComponent<SpriteRenderer>();
        isLive = true;
        hasSpotted = false;
    }

    void FixedUpdate()
    {
        dealtThisFixed = false;

        if (!isLive || target == null)
        {
            rigid.linearVelocity = Vector2.zero;
            return;
        }

        // ���� �� �ôٸ� Ž�� �õ�
        if (!hasSpotted && CanDetectPlayer())
        {
            hasSpotted = true; // �� �������� ���� ����
        }

        // �ôٸ� ��� ����
        if (hasSpotted)
        {
            Vector2 dir = target.position - rigid.position;
            Vector2 step = dir.normalized * Speed * Time.fixedDeltaTime;
            rigid.MovePosition(rigid.position + step);
        }

        // �������̶� ���� ����
        rigid.linearVelocity = Vector2.zero;
    }

    void LateUpdate()
    {
        if (!isLive || target == null) return;
        // �¿� ���⸸ ��� (���ϸ� �̵� ���ͷ� ��ü ����)
        spriter.flipX = target.position.x < rigid.position.x;
    }

    // ���ڸ��� 1ȸ
    void OnCollisionEnter2D(Collision2D collision)
    {
        TryDealDamage(collision);
    }

    // ���� ��ٿ��
    void OnCollisionStay2D(Collision2D collision)
    {
        TryDealDamage(collision);
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            // �ٽ� ������ ��� Ÿ�� ���
            nextAttackTime = 0f;
        }
    }

    void TryDealDamage(Collision2D collision)
    {
        if (!isLive) return;
        if (!collision.collider.CompareTag("Player")) return;
        if (dealtThisFixed) return; // ���� Fixed ������ �ߺ� ����

        // �߰� ������ �������� ����(���� ����)
        if (!hasSpotted) return;

        if (Time.time < nextAttackTime) return;

        var player = collision.collider.GetComponentInParent<Player>();
        if (player == null) return;

        int dmg = Random.Range(minDamage, maxDamage + 1); // 3~5
        player.TakeDamage(dmg);

        nextAttackTime = Time.time + attackCooldown;
        dealtThisFixed = true;
    }

    // === �߰� ����(�߰� �������� ȣ��) ===
    bool CanDetectPlayer()
    {
        Vector2 myPos = rigid.position;
        Vector2 toPlayer = target.position - myPos;
        float dist = toPlayer.magnitude;

        // 1) ���� �ݰ�: ������ �� �Ǹ� ��� �߰�
        if (dist <= detectRadius)
            return !Blocked(myPos, target.position);

        // 2) �þ߰� + �þ߰Ÿ�
        if (dist > viewDistance) return false;

        Vector2 forward = spriter.flipX ? Vector2.left : Vector2.right; // ������ ����
        float angle = Vector2.Angle(forward, toPlayer.normalized);
        if (angle > (fovAngle * 0.5f)) return false;

        // 3) ����ĳ��Ʈ�� ���� üũ
        return !Blocked(myPos, target.position);
    }

    bool Blocked(Vector2 from, Vector2 to)
    {
        var hit = Physics2D.Linecast(from, to, obstacleMask);
        return hit.collider != null;
    }

    // ����׿�
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0.6f, 0, 0.25f);
        Gizmos.DrawWireSphere(transform.position, detectRadius);

        Gizmos.color = new Color(0, 1, 0, 0.25f);
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        // �뷫���� FOV ǥ��
        Vector2 forward = (spriter != null && spriter.flipX) ? Vector2.left : Vector2.right;
        float half = fovAngle * 0.5f;
        Vector2 left = Quaternion.Euler(0, 0, +half) * forward;
        Vector2 right = Quaternion.Euler(0, 0, -half) * forward;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, (Vector2)transform.position + left * viewDistance);
        Gizmos.DrawLine(transform.position, (Vector2)transform.position + right * viewDistance);
    }

    public void OnDamage(float damage)
    {
        Debug.Log($"{name}��(��) {damage} ������");
        // TODO: ü��/��� �� hasSpotted = false; isLive = false; �� ���� ��ȯ
    }
}
