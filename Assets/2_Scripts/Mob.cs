using System;
using UnityEngine;

public class Mob : MonoBehaviour
{
    public float Speed;
    public Rigidbody2D target;

    bool isLive = true;

    Rigidbody2D rigid;
    SpriteRenderer spriter;

    internal void OnDamage(float damage)
    {
        throw new NotImplementedException();
    }

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriter = GetComponent<SpriteRenderer>();
        isLive = true;
    }
    void FixedUpdate()
    {
        if (!isLive || target == null) return;
        Vector2 dirVec = target.position - rigid.position;
        Vector2 moveVec = dirVec.normalized * Speed * Time.fixedDeltaTime;
        rigid.MovePosition(rigid.position + moveVec);
        rigid.linearVelocity = Vector2.zero;
    }

    void LateUpdate()
    {
        if (!isLive || target == null) return;

        if (target.position.x > rigid.position.x)
        {
            spriter.flipX = false; // 오른쪽으로 이동
        }
        else if (target.position.x < rigid.position.x)
        {
            spriter.flipX = true; // 왼쪽으로 이동
        }
    }
}
