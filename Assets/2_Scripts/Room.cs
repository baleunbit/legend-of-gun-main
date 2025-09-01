using UnityEngine;

public class Room : MonoBehaviour
{
    [Header("Room Info")]
    public int roomID;
    public bool isStartRoom;
    public bool isEndRoom;

    [Header("Points")]
    public Transform entryPoint; // �÷��̾ ���� ��ġ
    public Transform exitPoint;  // �ʿ��ϴٸ� ������ �� ��ġ
}
