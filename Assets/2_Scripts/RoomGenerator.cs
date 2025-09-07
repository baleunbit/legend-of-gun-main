using UnityEngine;
using System.Collections.Generic;

public class RoomGenerator : MonoBehaviour
{
    public List<GameObject> roomPrefabs; // ���� �׸� ������
    public int numberOfRooms = 6;
    public Vector2 roomSize = new Vector2(20, 15);

    private List<GameObject> _rooms = new List<GameObject>();

    void Start()
    {
        // ���� �̹� �ִ� Room ���
        Room[] existing = FindFirstObjectsOfType<Room>();
        foreach (var r in existing)
        {
            _rooms.Add(r.gameObject);
        }

        GenerateRooms();
    }

    private T[] FindFirstObjectsOfType<T>()
    {
        throw new System.NotImplementedException();
    }

    void GenerateRooms()
    {
        for (int i = 0; i < numberOfRooms; i++)
        {
            Vector2 pos;
            int tries = 0;

            do
            {
                pos = new Vector2(Random.Range(-50, 50), Random.Range(-50, 50));
                tries++;
            }
            while (IsOverlapping(pos) && tries < 20);

            if (tries >= 20)
            {
                Debug.Log($"Room {i} skipped (overlap with existing)");
                continue;
            }

            GameObject prefab = roomPrefabs[Random.Range(0, roomPrefabs.Count)];
            var room = Instantiate(prefab, pos, Quaternion.identity);

            var r = room.GetComponent<Room>();
            if (r) r.roomID = i;

            _rooms.Add(room);
        }
    }

    bool IsOverlapping(Vector2 newPos)
    {
        foreach (var r in _rooms)
        {
            Vector2 existing = r.transform.position;
            float dx = Mathf.Abs(newPos.x - existing.x);
            float dy = Mathf.Abs(newPos.y - existing.y);

            if (dx < roomSize.x && dy < roomSize.y)
                return true; // ��ħ
        }
        return false;
    }

    internal GameObject GetRoom(int targetRoomIndex)
    {
        throw new System.NotImplementedException();
    }
}
