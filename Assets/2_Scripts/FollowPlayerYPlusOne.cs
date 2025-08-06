using UnityEngine;

public class FollowPlayerYPlusOne : MonoBehaviour
{
    public Transform playerTransform; // �÷��̾��� Transform�� �����Ϳ��� �Ҵ�

    void Update()
    {
        if (playerTransform != null)
        {
            // ���� ������Ʈ�� ��ġ�� �÷��̾��� ��ġ�� ���󰡵�, Y�ุ +1
            Vector3 targetPosition = new Vector3(
                playerTransform.position.x,
                playerTransform.position.y + 1f,
                playerTransform.position.z
            );

            transform.position = targetPosition;
        }
    }
}