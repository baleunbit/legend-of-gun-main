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
        ConnectDoors();
    }

    void GenerateRooms()
    {
        Vector2 startPos = Vector2.zero;

        for (int i = 0; i < numberOfRooms; i++)
        {
            Vector2 spawnPos = startPos + new Vector2(0, i * roomSize.y);

            GameObject room = Instantiate(roomPrefab, spawnPos, Quaternion.identity);
            Room roomScript = room.GetComponent<Room>();

            if (roomScript != null)
            {
                roomScript.roomID = i;
                roomScript.isStartRoom = (i == 0);
                roomScript.isEndRoom = (i == numberOfRooms - 1);
            }

            spawnedRooms.Add(room);
        }
    }

    void ConnectDoors()
    {
        for (int i = 0; i < spawnedRooms.Count; i++)
        {
            Room roomScript = spawnedRooms[i].GetComponent<Room>();

            if (roomScript != null)
            {
                // 위로 갈 문
                if (i < spawnedRooms.Count - 1)
                {
                    Door upDoor = roomScript.nextDoor;
                    if (upDoor != null)
                        upDoor.targetRoomIndex = i + 1;
                }

                // 아래로 갈 문
                if (i > 0)
                {
                    Door downDoor = roomScript.prevDoor;
                    if (downDoor != null)
                        downDoor.targetRoomIndex = i - 1;
                }
            }
        }
    }

    public GameObject GetRoom(int index)
    {
        if (index >= 0 && index < spawnedRooms.Count)
            return spawnedRooms[index];
        return null;
    }
}
