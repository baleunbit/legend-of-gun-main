using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public GameObject roomPrefab;   // ������ Room ������
    public int roomCount = 5;      // �� ���� ���� ��������
    public float roomSpacing = 10f; // �� ���� �Ÿ�

    private List<GameObject> rooms = new List<GameObject>();

    void Start()
    {
        GenerateRooms();
    }

    void GenerateRooms()
    {
        for (int i = 0; i < roomCount; i++)
        {
            Vector2 pos = new Vector2(Random.Range(-5, 5), Random.Range(-5, 5)) * roomSpacing;
            GameObject room = Instantiate(roomPrefab, pos, Quaternion.identity, transform);
            rooms.Add(room);
        }
    }

    public List<GameObject> GetRooms()
    {
        return rooms;
    }
}