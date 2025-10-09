﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MobSpawner : MonoBehaviour
{
    [System.Serializable]
    public class StageEnemyList
    {
        public int stage = 1;                       // 1, 2 ...
        public List<GameObject> enemies = new();    // 이 스테이지에서만 스폰될 몹 프리팹들
    }

    [Header("스테이지별 몹 풀")]
    public List<StageEnemyList> stageEnemies = new();   // 예) [ (1: 슬라임/쥐), (2: 늑대/고스트) ]

    [Header("스폰 개수(방당)")]
    public int minEnemiesPerRoom = 1;
    public int maxEnemiesPerRoom = 3;

    [Header("겹침 방지/조건")]
    public float avoidOthersRadius = 0.5f;   // 서로 간 최소 거리
    public string obstacleTag = "GameObject"; // 벽/가구 등 태그
    public int maxSpawnTriesPerEnemy = 16;

    [Header("참조(선택)")]
    public Rigidbody2D playerRigidbody;      // Mob.target 주입
    public bool bindPlayerTargetIfPossible = true;

    [Header("실행 타이밍")]
    public bool waitOneFrameForRooms = true; // RoomGenerator가 Start에서 생성하면 true
    public float extraDelay = 0f;

    void Start()
    {
        if (!Application.isPlaying) return;
        if (waitOneFrameForRooms) StartCoroutine(SpawnRoutine());
        else DoSpawn();
    }

    IEnumerator SpawnRoutine()
    {
        yield return null; // RoomGenerator가 방 생성할 시간을 한 프레임 줌
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
        if (room.gameObject.name.Contains("Boss")) return;

        // 방 이름에서 스테이지 번호 추출 (예: "1_ForestRoom" -> 1)
        int stage = ParseStageFromRoomName(room.gameObject.name);
        var pool = GetEnemyPoolForStage(stage);

        if (pool == null || pool.Count == 0)
        {
            // 풀 없으면 해당 방은 스폰 스킵(요구사항: 1스테와 2스테는 서로 몹이 섞이면 안 됨)
            // 필요하면 여기서 '기본 풀'을 쓰게 바꿀 수도 있음.
            return;
        }

        // SpawnPoint 기준 스폰 (디자이너가 각 방 안에 SpawnPoint들 배치했다고 가정)
        var spawnPoints = room.SpawnPoints;
        if (spawnPoints == null || spawnPoints.Length == 0) return;

        int enemyCount = Random.Range(minEnemiesPerRoom, maxEnemiesPerRoom + 1);
        List<Vector2> occupied = new();

        for (int i = 0; i < enemyCount; i++)
        {
            var prefab = PickFromPool(pool);
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

    // ─────────────────────────────────────────────────────────────

    int ParseStageFromRoomName(string roomName)
    {
        // 앞자리 숫자 + '_' 패턴만 보면 충분: "1_...", "2_..." 등
        // 숫자가 아니면 -1 반환해서 스킵
        if (string.IsNullOrEmpty(roomName)) return -1;
        int under = roomName.IndexOf('_');
        if (under <= 0) return -1;

        var prefix = roomName.Substring(0, under);
        if (int.TryParse(prefix, out int stage)) return stage;
        return -1;
    }

    List<GameObject> GetEnemyPoolForStage(int stage)
    {
        if (stage <= 0) return null;
        var entry = stageEnemies.FirstOrDefault(s => s.stage == stage);
        // null 가능 (스테이지에 대한 엔트리 없으면 스폰 스킵)
        return entry != null ? entry.enemies : null;
    }

    GameObject PickFromPool(List<GameObject> pool)
    {
        // 빈/null 프리팹 거르기
        for (int safety = 0; safety < 8; safety++)
        {
            var p = pool[Random.Range(0, pool.Count)];
            if (p) return p;
        }
        return null;
    }

    void PlaceEnemy(GameObject roomGO, GameObject prefab, Vector2 pos)
    {
        var enemy = Instantiate(prefab, pos, Quaternion.identity, roomGO.transform);

        // 혹시 프리팹 스케일이 이상하면 한 번 고정
        enemy.transform.localScale = Vector3.one;

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
        return hit && hit.CompareTag(obstacleTag);
    }

    bool TooClose(List<Vector2> used, Vector2 p)
    {
        for (int i = 0; i < used.Count; i++)
            if (Vector2.Distance(used[i], p) < avoidOthersRadius)
                return true;
        return false;
    }
}
