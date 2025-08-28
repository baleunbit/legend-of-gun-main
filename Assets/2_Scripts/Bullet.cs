using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float damage;
    public int per;
    public float speed = 15f;

    Rigidbody2D rb;

    public void Init(float damage, int per, Vector3 dir)
    {
        this.damage = damage;
        this.per = per;

        if (per > -1)
        {
            // Fix: Assign a float value to angularVelocity instead of a Vector2
            rb.angularVelocity = dir.magnitude; // dir.magnitude is a float
        }
    }
    public void Setup(Vector2 direction)
    {
        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = direction * speed;

        // 1초 뒤에 총알 자동 삭제
        Destroy(gameObject, 1f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Mob") || per == -1) return;
        per--;
        if (per == -1)
        {
            // Fix: Assign a float value (0) to angularVelocity
            rb.angularVelocity = 0f;
        }
        Debug.Log("총알 충돌: " + collision.name);
        // Room 태그 또는 이름이 Room, Player 태그면 무시
        if (collision.CompareTag("Room") || collision.name == "Room" || collision.CompareTag("Player"))
            return;

        // 그 외에는 총알 삭제
        Destroy(gameObject);
    }
}