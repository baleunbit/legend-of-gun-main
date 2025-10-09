using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MobSpawner : MonoBehaviour
{
    [Header("적 프리팹들")]
    public List<GameObject> enemyPrefabs;

    [Header("스폰 개수")]
    public int minEnemiesPerRoom = 1;
    public int maxEnemiesPerRoom = 3;

    [Header("겹침 방지/조건")]
    public float avoidOthersRadius = 0.5f;
    public string obstacleTag = "GameObject"; // 벽/오브젝트에 태그 지정
    public int maxSpawnTriesPerEnemy = 16;

    [Header("참조(선택)")]
    public Rigidbody2D playerRigidbody;
    public bool bindPlayerTargetIfPossible = true;

    [Header("실행 타이밍")]
    public bool waitOneFrameForRooms = true;
    public float extraDelay = 0f;

    void Start()
    {
        if (!Application.isPlaying) return;
        if (waitOneFrameForRooms) StartCoroutine(SpawnRoutine());
        else DoSpawn();
    }

    IEnumerator SpawnRoutine()
    {
        yield return null;
        if (extraDelay > 0f) yield return new WaitForSeconds(extraDelay);
        DoSpawn();
    }

    void DoSpawn()
    {
        var rooms = FindObjectsByType<Room>(FindObjectsSortMode.None);
        if (rooms == null || rooms.Length == 0)
        {
            Debug.Log("[MobSpawner] 방(Room)이 없습니다.");
            return;
        }

        foreach (var room in rooms)
            SpawnEnemiesInRoom(room);
    }

    void SpawnEnemiesInRoom(Room room)
    {
        if (enemyPrefabs == null || enemyPrefabs.Count == 0) return;
        var spawnPoints = room.SpawnPoints;
        if (spawnPoints == null || spawnPoints.Length == 0) return;

        int enemyCount = Random.Range(minEnemiesPerRoom, maxEnemiesPerRoom + 1);
        List<Vector2> occupied = new();

        for (int i = 0; i < enemyCount; i++)
        {
            var prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
            if (prefab == null) continue;

            bool placed = false;
            Vector2 spawnPos = default;

            for (int tries = 0; tries < maxSpawnTriesPerEnemy && !placed; tries++)
            {
                var sp = spawnPoints[Random.Range(0, spawnPoints.Length)];
                var candidate = (Vector2)sp.position;

                if (Blocked(candidate)) continue;
                if (TooClose(occupied, candidate)) continue;

                spawnPos = candidate;
                placed = true;
            }

            if (!placed) continue;
            PlaceEnemy(room.gameObject, prefab, spawnPos);
            occupied.Add(spawnPos);
        }
    }

    void PlaceEnemy(GameObject roomGO, GameObject prefab, Vector2 pos)
    {
        var enemy = Instantiate(prefab, pos, Quaternion.identity);
        enemy.transform.SetParent(roomGO.transform, true);

        if (bindPlayerTargetIfPossible && playerRigidbody != null)
        {
            var mob = enemy.GetComponent<Mob>();
            if (mob != null && mob.target == null)
                mob.target = playerRigidbody;
        }
    }

    bool Blocked(Vector2 p)
    {
        var hit = Physics2D.OverlapCircle(p, 0.2f);
        return hit != null && hit.CompareTag(obstacleTag);
    }

    bool TooClose(List<Vector2> used, Vector2 p)
    {
        foreach (var u in used)
            if (Vector2.Distance(u, p) < avoidOthersRadius)
                return true;
        return false;
    }
}
