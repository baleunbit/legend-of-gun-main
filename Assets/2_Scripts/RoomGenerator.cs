using UnityEngine;
using System.Collections.Generic;

public class RoomGenerator : MonoBehaviour
{
    public GameObject roomPrefab;
    public int numberOfRooms = 10;
    public Vector2 roomSize = new Vector2(20, 15);

    private List<GameObject> spawnedRooms = new List<GameObject>();

    void Start()
    {
        GenerateRooms();
    }

    void GenerateRooms()
    {
        for (int i = 0; i < numberOfRooms; i++)
        {
            Vector2 spawnPos = new Vector2(
                Random.Range(-50, 50),
                Random.Range(-50, 50)
            );

            Collider2D hit = Physics2D.OverlapBox(spawnPos, roomSize, 0);

            if (hit == null) // 겹치지 않으면 생성
            {
                GameObject room = Instantiate(roomPrefab, spawnPos, Quaternion.identity);
                Room roomScript = room.GetComponent<Room>();
                if (roomScript != null)
                {
                    roomScript.roomID = i;
                }
                spawnedRooms.Add(room);
            }
            else
            {
                Debug.Log("Room overlapped, skipped.");
            }
        }
    }

    public GameObject GetRoom(int index)
    {
        if (index >= 0 && index < spawnedRooms.Count)
            return spawnedRooms[index];
        return null;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(Vector3.zero, roomSize);
    }
}
