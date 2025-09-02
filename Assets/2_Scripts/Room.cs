using UnityEngine;

public class Room : MonoBehaviour
{
    [Header("Room Info")]
    public int roomID;
    public bool isStartRoom;
    public bool isEndRoom;

    [Header("Doors")]
    public Door nextDoor;  // 위쪽 문
    public Door prevDoor;  // 아래쪽 문
}
