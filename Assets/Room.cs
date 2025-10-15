using UnityEngine;

public class Room : MonoBehaviour
{
    [Header("Room Info")]
    public int roomID;          // 방 고유 ID
    public bool isStartRoom;    // 시작 방 여부
    public bool isEndRoom;      // 보스/종료 방 여부
}
