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

            // �� ������ ���� ���� (Box ���� ũ��� �� ũ��� ����� ��)
            Collider2D hit = Physics2D.OverlapBox(spawnPos, roomSize, 0);

            if (hit == null) // �浹 ������ �� ����
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

    // Scene �信�� Box ���� ����׿�
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(Vector3.zero, roomSize);
    }
}
