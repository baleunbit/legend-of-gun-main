using UnityEngine;
using System.Collections.Generic;

public class DungeonGenerator : MonoBehaviour
{
    public GameObject roomPrefab;
    public int numberOfRooms = 10;
    public Vector2 roomSize = new Vector2(20, 15);

    private List<GameObject> spawnedRooms = new List<GameObject>();

    void Start()
    {
        GenerateDungeon();
    }

    void GenerateDungeon()
    {
        for (int i = 0; i < numberOfRooms; i++)
        {
            Vector2 spawnPos = new Vector2(
                Random.Range(-50, 50),
                Random.Range(-50, 50)
            );

            // 방 생성할 영역 정의 (Box 영역 크기는 방 크기랑 맞춰야 함)
            Collider2D hit = Physics2D.OverlapBox(spawnPos, roomSize, 0);

            if (hit == null) // 충돌 없으면 방 생성
            {
                GameObject room = Instantiate(roomPrefab, spawnPos, Quaternion.identity);
                spawnedRooms.Add(room);
            }
            else
            {
                Debug.Log("Room overlapped, skipped.");
            }
        }
    }

    // Scene 뷰에서 Box 영역 디버그용
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(Vector3.zero, roomSize);
    }
}
