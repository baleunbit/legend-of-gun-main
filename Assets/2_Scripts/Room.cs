using UnityEngine;

public class Room : MonoBehaviour
{
    [Header("Room Info")]
    public int roomID;
    public bool isStartRoom;
    public bool isEndRoom;

    [Header("Points")]
    public Transform entryPoint; // 플레이어가 들어올 위치
    public Transform exitPoint;  // 필요하다면 나가는 문 위치
}
