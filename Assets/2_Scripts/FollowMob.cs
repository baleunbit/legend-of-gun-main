using UnityEngine;

public class FollowMob : MonoBehaviour
{
    public Transform MobTransform; // ���� Transform�� �����Ϳ��� �Ҵ�

    void Update()
    {
            if (MobTransform != null)
        {
            // ���� ������Ʈ�� ��ġ�� ���� ��ġ�� ���󰡵�, Y�ุ +0.8
            Vector3 targetPosition = new Vector3(
                MobTransform.position.x,
                MobTransform.position.y + 0.8f,
                MobTransform.position.z
            );

            transform.position = targetPosition;
        }
    }
}