using UnityEngine;

public class Room : MonoBehaviour
{
    [Header("Room Info")]
    public int roomID;
    public bool isStartRoom;
    public bool isEndRoom;

    [Header("Doors")]
    public Door nextDoor;  // ���� ��
    public Door prevDoor;  // �Ʒ��� ��
}
