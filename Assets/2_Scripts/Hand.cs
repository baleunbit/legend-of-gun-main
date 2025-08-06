using UnityEngine;

public class Hand : MonoBehaviour
{
    public bool isLeft;
    public SpriteRenderer spriter;

    [HideInInspector]
    public SpriteRenderer player;

    Vector3 rightPos = new Vector3(0.3f, -0.15f, 0);
    Vector3 rightPosReverse = new Vector3(-0.3f, -0.15f, 0);
    Quaternion leftRot = Quaternion.Euler(0, 0, -30);
    Quaternion leftRotReverse = Quaternion.Euler(0, 0, -130);

    void Awake()
    {
        player = GetComponentInParent<SpriteRenderer>();
    }

    private void LateUpdate()
    {
        HandleInput(); // �߰��� �Է� ó�� �޼��� ȣ��
    }

    private void HandleInput()
    {
        if (!isLeft) // �����ո� ó��
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                spriter.flipX = true; // ������ �ø�
                spriter.sortingOrder = 4; 
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                spriter.flipX = false; // ������ �ø� ����
                spriter.sortingOrder = 6;
            }
        }
    }
}