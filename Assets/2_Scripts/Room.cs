using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public GameObject roomPrefab;   // 생성할 Room 프리팹
    public int roomCount = 5;      // 몇 개의 방을 생성할지
    public float roomSpacing = 10f; // 방 사이 거리

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