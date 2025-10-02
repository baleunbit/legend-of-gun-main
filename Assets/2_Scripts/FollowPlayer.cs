using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public Transform playerTransform; // 플레이어의 Transform을 에디터에서 할당

    void Update()
    {
        if (playerTransform != null)
        {
            // 현재 오브젝트의 위치를 플레이어의 위치로 따라가되, Y축만 +1
            Vector3 targetPosition = new Vector3(
                playerTransform.position.x,
                playerTransform.position.y + 4f,
                playerTransform.position.z
            );

            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 10f);
        }
    }
}