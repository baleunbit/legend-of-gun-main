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
        GameObject existingRoom = GameObject.Find("StartRoom");
        Vector2 existingPos = existingRoom != null ? (Vector2)existingRoom.transform.position : Vector2.zero;

        for (int i = 0; i < numberOfRooms; i++)
        {
            Vector2 spawnPos = startPos + new Vector2(0, i * roomSize.y);

            // 씬에 이미 있는 첫 번째 방 위치 체크
            if (Vector2.Distance(spawnPos, existingPos) < Mathf.Max(roomSize.x, roomSize.y))
            {
                Debug.Log("Skipping spawn at existing first room position");
                continue; // 겹치지 않도록 스킵
            }

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
