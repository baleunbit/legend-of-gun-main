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

    [Header("스폰 규칙")]
    public float avoidOthersRadius = 0.5f;   // 서로 겹치지 않도록 최소 간격
    public LayerMask obstacleMask;           // 스폰 금지(벽/지형) 레이어
    public int maxSpawnTriesPerEnemy = 32;
    public float spawnPadding = 0.2f;        // 방 AABB 테두리에서 안쪽으로

    [Header("방 내부 판정(선택)")]
    public LayerMask interiorMask;           // 방 바닥/실내 레이어. 비워두면 콜라이더로 판정

    [Header("참조(선택)")]
    public Rigidbody2D playerRigidbody;      // Mob.target 주입용
    public bool bindPlayerTargetIfPossible = true;

    [Header("실행 타이밍")]
    public bool waitOneFrameForRooms = true; // RoomGenerator가 Start에서 만들 경우 true
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

        var roomGO = room.gameObject;
        var colliders = roomGO.GetComponentsInChildren<Collider2D>(true);

        // AABB
        Bounds aabb = room.AABB;
        Vector2 center = aabb.center;
        Vector2 size = new Vector2(aabb.size.x, aabb.size.y);
        Vector2 min = center - size * 0.5f + Vector2.one * spawnPadding;
        Vector2 max = center + size * 0.5f - Vector2.one * spawnPadding;

        // SpawnPoint
        var points = room.SpawnPoints;

        int enemyCount = Random.Range(minEnemiesPerRoom, maxEnemiesPerRoom + 1);
        List<Vector2> occupied = new();

        for (int i = 0; i < enemyCount; i++)
        {
            var prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
            if (prefab == null) continue;

            bool placed = false;
            Vector2 spawnPos = default;

            // 1) 스폰포인트 우선
            if (points != null && points.Length > 0)
            {
                for (int tries = 0; tries < maxSpawnTriesPerEnemy && !placed; tries++)
                {
                    var p = points[Random.Range(0, points.Length)];
                    var candidate = (Vector2)p.position;

                    if (!IsInside(candidate, colliders))
                        candidate = SnapInside(candidate, colliders);

                    if (Blocked(candidate) || TooClose(occupied, candidate)) continue;

                    spawnPos = candidate;
                    placed = true;
                }
            }

            // 2) 랜덤 내부
            if (!placed)
            {
                for (int tries = 0; tries < maxSpawnTriesPerEnemy && !placed; tries++)
                {
                    float x = Random.Range(min.x, max.x);
                    float y = Random.Range(min.y, max.y);
                    var candidate = new Vector2(x, y);

                    if (!IsInside(candidate, colliders)) continue;
                    if (Blocked(candidate) || TooClose(occupied, candidate)) continue;

                    spawnPos = candidate;
                    placed = true;
                }
            }

            if (!placed) continue;

            PlaceEnemy(roomGO, prefab, spawnPos);
            occupied.Add(spawnPos);
        }
    }

    void PlaceEnemy(GameObject roomGO, GameObject prefab, Vector2 pos)
    {
        var enemy = Instantiate(prefab, pos, Quaternion.identity);

        // 부모 지정
        enemy.transform.SetParent(roomGO.transform, true);

        // 플레이어를 타겟으로 바인딩
        if (bindPlayerTargetIfPossible && playerRigidbody != null)
        {
            var mob = enemy.GetComponent<Mob>();
            if (mob != null && mob.target == null)
                mob.target = playerRigidbody;
        }
    }

    // ==== 유틸 ====
    bool Blocked(Vector2 p) => Physics2D.OverlapCircle(p, 0.1f, obstacleMask);
    bool TooClose(List<Vector2> used, Vector2 p) => used.Any(u => Vector2.Distance(u, p) < avoidOthersRadius);

    bool IsInside(Vector2 p, Collider2D[] cols)
    {
        if (interiorMask.value != 0)
        {
            var hit = Physics2D.OverlapPoint(p, interiorMask);
            if (!hit) return false;
        }

        if (cols == null || cols.Length == 0) return false;

        foreach (var c in cols)
            if (c != null && c.OverlapPoint(p))
                return true;

        return false;
    }

    Vector2 SnapInside(Vector2 p, Collider2D[] cols)
    {
        if (cols == null || cols.Length == 0) return p;
        float best = float.MaxValue; Vector2 bestPt = p;
        foreach (var c in cols)
        {
            if (c == null) continue;
            var cp = (Vector2)c.ClosestPoint(p);
            float d = (cp - p).sqrMagnitude;
            if (d < best) { best = d; bestPt = cp; }
        }
        return bestPt;
    }
}
