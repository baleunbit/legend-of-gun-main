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
    public float avoidOthersRadius = 0.5f;   // 서로 간 최소 거리
    public LayerMask obstacleMask;           // 겹치면 안 되는 오브젝트 레이어(벽/가구 등)
    public int maxSpawnTriesPerEnemy = 16;   // 스폰포인트 재시도 횟수

    [Header("참조(선택)")]
    public Rigidbody2D playerRigidbody;      // Mob.target 주입
    public bool bindPlayerTargetIfPossible = true;

    [Header("실행 타이밍")]
    public bool waitOneFrameForRooms = true; // RoomGenerator가 Start에서 방을 만들면 true
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

        // ◀ SpawnPoint 전용: 방에 스폰포인트가 없으면 스킵
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

            // 스폰포인트 중에서 랜덤 선택 → 조건 안 맞으면 다른 포인트로 재시도
            for (int tries = 0; tries < maxSpawnTriesPerEnemy && !placed; tries++)
            {
                var sp = spawnPoints[Random.Range(0, spawnPoints.Length)];
                var candidate = (Vector2)sp.position;

                if (Blocked(candidate)) continue;                 // 장애물 위면 패스
                if (TooClose(occupied, candidate)) continue;      // 이미 배치한 적과 너무 가까우면 패스

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

    // ==== 유틸 ====
    bool Blocked(Vector2 p)
    {
        // 장애물과 겹치지 않게: obstacleMask에 벽/가구/기둥 등 넣어두기
        return obstacleMask.value != 0 && Physics2D.OverlapCircle(p, 0.2f, obstacleMask);
    }

    bool TooClose(List<Vector2> used, Vector2 p)
    {
        for (int i = 0; i < used.Count; i++)
            if (Vector2.Distance(used[i], p) < avoidOthersRadius)
                return true;
        return false;
    }
}
