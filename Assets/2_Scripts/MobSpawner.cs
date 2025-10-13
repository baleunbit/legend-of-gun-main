using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MobSpawner : MonoBehaviour
{
    [System.Serializable]
    public class StageEnemyList
    {
        public int stage = 1;                    // 1, 2 ...
        public List<GameObject> enemies = new(); // 이 스테이지에서만 스폰될 몹 프리팹들
    }

    [Header("스테이지별 몹 풀")]
    public List<StageEnemyList> stageEnemies = new();   // 예) [ (1: 슬라임/쥐), (2: 늑대/고스트) ]

    [Header("스폰 개수(방당)")]
    public int minEnemiesPerRoom = 1;
    public int maxEnemiesPerRoom = 3;

    [Header("겹침 방지/조건")]
    public float avoidOthersRadius = 0.5f;         // 서로 간 최소 거리
    public string obstacleTag = "GameObject";      // 벽/가구 등 태그
    public int maxSpawnTriesPerEnemy = 16;

    [Header("참조(선택)")]
    public Rigidbody2D playerRigidbody;            // Mob.target 주입
    public bool bindPlayerTargetIfPossible = true;

    [Header("실행 타이밍")]
    public bool waitOneFrameForRooms = true;       // RoomGenerator가 Start에서 생성하면 true
    public float extraDelay = 0f;

    [Header("표시 크기 통일(월드 유닛)")]
    public bool unifyEnemyHeight = true;           // 켜면 실제 화면 높이를 강제로 맞춤
    public float targetEnemyHeight = 4f;           // ← 모두 높이 4로
    [Tooltip("비주얼 전용 자식 트랜스폼 이름. 비워두면 루트 기준으로 스케일링.")]
    public string visualChildName = "";            // 예: "Visual" 로 쓰면 비주얼만 스케일

    void Start()
    {
        if (!Application.isPlaying) return;
        if (waitOneFrameForRooms) StartCoroutine(SpawnRoutine());
        else DoSpawn();
    }

    IEnumerator SpawnRoutine()
    {
        // RoomGenerator가 방 생성할 시간을 한 프레임 줌
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
        if (room.gameObject.name.Contains("Boss")) return;

        // 방 이름에서 스테이지 번호 추출 (예: "1_ForestRoom" -> 1)
        int stage = ParseStageFromRoomName(room.gameObject.name);
        var pool = GetEnemyPoolForStage(stage);
        if (pool == null || pool.Count == 0) return; // 스테이지 풀 없으면 스킵

        // SpawnPoint 기준 스폰 (디자이너가 방 안에 SpawnPoint들 배치했다고 가정)
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

            // 랜덤 스폰포인트 선택 → 조건 안 맞으면 재시도
            for (int tries = 0; tries < maxSpawnTriesPerEnemy && !placed; tries++)
            {
                var sp = spawnPoints[Random.Range(0, spawnPoints.Length)];
                var candidate = (Vector2)sp.position;

                if (Blocked(candidate)) continue;            // 장애물 위면 패스
                if (TooClose(occupied, candidate)) continue; // 이미 배치한 적과 너무 가까우면 패스

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
        // 1) 부모 영향 없이 월드에 생성
        var enemy = Instantiate(prefab, pos, Quaternion.identity);

        // 2) 월드 스케일 1 보정
        enemy.transform.localScale = Vector3.one;

        // 3) 표시 높이 강제 통일 (월드 기준, SpriteRenderer만 사용)
        if (unifyEnemyHeight)
        {
            Transform target = enemy.transform;
            if (!string.IsNullOrEmpty(visualChildName))
            {
                var t = enemy.transform.Find(visualChildName);
                if (t) target = t; // 비주얼만 스케일하고 싶을 때
            }

            FitToWorldHeightFromSprites(target, targetEnemyHeight);
        }

        // 4) 부모에 부착(월드값 유지 → 부모 스케일 영향 없음)
        enemy.transform.SetParent(roomGO.transform, true);

        // (원하면 경고 제거 가능)
        //var parentScale = roomGO.transform.lossyScale;
        //if (Mathf.Abs(parentScale.x - 1f) > 0.001f || Mathf.Abs(parentScale.y - 1f) > 0.001f)
        //    Debug.LogWarning($"[MobSpawner] Parent '{roomGO.name}' lossyScale={parentScale} → 자식 월드스케일 유지로 부풀림 방지.", roomGO);

        // 5) 타깃 바인딩(선택)
        if (bindPlayerTargetIfPossible && playerRigidbody != null)
        {
            var mob = enemy.GetComponent<Mob>();
            if (mob != null && mob.target == null)
                mob.target = playerRigidbody;
        }
    }

    // ✔ LineRenderer 등은 제외하고, SpriteRenderer만으로 실제 캐릭터 높이를 잰다.
    static void FitToWorldHeightFromSprites(Transform targetRoot, float targetHeight)
    {
        if (targetHeight <= 0f || !targetRoot) return;

        var srs = targetRoot.GetComponentsInChildren<SpriteRenderer>(true);
        if (srs == null || srs.Length == 0) return;

        Bounds b = srs[0].bounds;
        for (int i = 1; i < srs.Length; i++) b.Encapsulate(srs[i].bounds);

        float current = b.size.y; // 월드 기준 현재 스프라이트 높이
        if (current <= 0.0001f) return;

        float scale = targetHeight / current;

        // 균등 스케일 적용 (비주얼만 스케일하고 싶으면 targetRoot를 비주얼 자식으로 지정)
        targetRoot.localScale = targetRoot.localScale * scale;
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
