using UnityEngine;
using System.Collections.Generic;

public class RoomGenerator : MonoBehaviour
{
    public List<GameObject> roomPrefabs; // 여러 테마 프리팹
    public int numberOfRooms = 6;
    public Vector2 roomSize = new Vector2(20, 15); // 방 간 최소 간격

    private List<GameObject> _rooms = new List<GameObject>();

    void Start()
    {
        // 🔥 최신 API 사용
        Room[] existing = FindObjectsByType<Room>(FindObjectsSortMode.None);
        foreach (var r in existing)
        {
            _rooms.Add(r.gameObject);
        }

        GenerateRooms();
    }

    void GenerateRooms()
    {
        for (int i = 0; i < numberOfRooms; i++)
        {
            Vector2 pos;
            int tries = 0;

            // 최대 20번까지 랜덤 위치 재시도
            do
            {
                pos = new Vector2(Random.Range(-50, 50), Random.Range(-50, 50));
                tries++;
            }
            while (IsOverlapping(pos) && tries < 20);

            if (tries >= 20)
            {
                Debug.Log($"Room {i} skipped (overlap with existing rooms)");
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
                return true; // 겹침
        }
        return false;
    }

    internal GameObject GetRoom(int targetRoomIndex)
    {
        if (targetRoomIndex < 0 || targetRoomIndex >= _rooms.Count)
            return null;
        return _rooms[targetRoomIndex];
    }
}
