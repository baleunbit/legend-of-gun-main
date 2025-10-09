using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class RoomGenerator : MonoBehaviour
{
    [Header("프리팹 목록")]
    public List<GameObject> roomPrefabs;

    [Header("스테이지 생성 규칙")]
    public int startStage = 1;          // 시작 스테이지 번호
    public int endStage = 2;            // 마지막 스테이지 번호(포함)
    public int roomsPerStage = 5;       // 각 스테이지 일반방 개수
    public string bossRoomExactName = "BossRoom";  // 보스 프리팹 이름의 접미사: 예) 1_BossRoom

    [Header("배치 설정(기존 유지)")]
    public Vector2 padding = new(1f, 1f);
    public float horizontalJitter = 2f;
    public int maxTriesPerRoom = 30;

    [Header("앵커(플레이어)")]
    public Transform playerTransform;
    public bool createStartRoomIfMissing = false;

    // 내부 상태
    private readonly List<RoomEntry> _rooms = new();
    private readonly List<int> _chain = new();

    private struct RoomEntry
    {
        public GameObject go;
        public Vector2 halfSize;
        public Bounds aabb;
        public Vector2 pos => go ? (Vector2)go.transform.position : Vector2.zero;
    }

    // ─────────────────────────────────────────────────────────────

    void Start()
    {
        if (!Application.isPlaying) return;

        // 프리팹 정리
        if (roomPrefabs == null) { Debug.LogError("[RoomGenerator] roomPrefabs가 null"); enabled = false; return; }
        roomPrefabs.RemoveAll(p => p == null);
        if (roomPrefabs.Count == 0) { Debug.LogError("[RoomGenerator] roomPrefabs 비어있음"); enabled = false; return; }

        // 씬에 이미 놓인 Room 수집
        Room[] existing = FindObjectsByType<Room>(FindObjectsSortMode.None);
        foreach (var r in existing) _rooms.Add(BuildEntryFromInstance(r.gameObject));

        // 시작 방 결정 (플레이어 위치 포함/가까운 방)
        int startIdx = FindStartRoomIndex();
        if (startIdx < 0 && createStartRoomIfMissing)
        {
            // 시작 스테이지 일반방 중 하나로 시작 방 생성
            var candidates = FilterStageNormals(startStage);
            if (candidates.Count == 0)
            {
                // 전체 프리팹 중 아무거나
                candidates = roomPrefabs.ToList();
            }
            var prefab = candidates[Random.Range(0, candidates.Count)];
            Vector3 pos = playerTransform ? playerTransform.position : Vector3.zero;

            var room = Instantiate(prefab, pos, Quaternion.identity);
            var r = room.GetComponent<Room>();
            if (r) r.roomID = _rooms.Count;

            _rooms.Add(BuildEntryFromInstance(room));
            startIdx = _rooms.Count - 1;
        }

        if (startIdx < 0)
        {
            Debug.LogWarning("[RoomGenerator] 시작 방을 찾지 못했습니다.");
            return;
        }

        _chain.Add(startIdx);

        // 체인 시작 기준점
        var prev = _rooms[startIdx];

        // 스테이지 루프
        for (int stage = startStage; stage <= endStage; stage++)
        {
            // 1) 일반방 N개
            GenerateStageNormals(stage, roomsPerStage, ref prev);

            // 2) 보스방 (있으면)
            var boss = FindBossPrefab(stage);
            if (boss)
            {
                TryPlaceRoom(boss, ref prev);
                Debug.Log($"[RoomGenerator] {stage}_BossRoom 배치 완료");
            }
            else
            {
                Debug.Log($"[RoomGenerator] {stage}_BossRoom 없음 → 건너뜀");
            }
        }
    }

    // ───────────────── Stage별 생성 ─────────────────

    void GenerateStageNormals(int stage, int count, ref RoomEntry prev)
    {
        var normals = FilterStageNormals(stage);
        if (normals.Count == 0)
        {
            Debug.LogWarning($"[RoomGenerator] {stage}_* 일반방 프리팹 없음");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            var prefab = normals[Random.Range(0, normals.Count)];
            TryPlaceRoom(prefab, ref prev);
        }
    }

    List<GameObject> FilterStageNormals(int stage)
    {
        string prefix = stage + "_";
        return roomPrefabs
            .Where(p => p && p.name.StartsWith(prefix) && !p.name.Contains("Boss"))
            .ToList();
    }

    GameObject FindBossPrefab(int stage)
    {
        // 1) 정확히 "{stage}_BossRoom" 찾기
        string exact = $"{stage}_{bossRoomExactName}";
        var exactHit = roomPrefabs.FirstOrDefault(p => p && p.name == exact);
        if (exactHit) return exactHit;

        // 2) "{stage}_Boss"로 시작하는 다른 이름을 허용(예: 1_BossRoomLarge 등)
        string prefix = $"{stage}_Boss";
        return roomPrefabs.FirstOrDefault(p => p && p.name.StartsWith(prefix));
    }

    // ───────────── 공통 배치 로직(기존 알고리즘 유지) ─────────────

    void TryPlaceRoom(GameObject prefab, ref RoomEntry prev)
    {
        if (!prefab) return;

        Vector2 prefabHalf = ComputeHalfSizeFromPrefab(prefab);

        float baseY = prev.pos.y + prev.halfSize.y + padding.y + prefabHalf.y;
        float baseX = prev.pos.x + Random.Range(-horizontalJitter, horizontalJitter);

        Vector2 pos = new(baseX, baseY);
        int tries = 0;
        bool placed = false;

        while (tries < maxTriesPerRoom)
        {
            if (!IsOverlappingWithAny(pos, prefabHalf))
            {
                var room = Instantiate(prefab, pos, Quaternion.identity);
                var r = room.GetComponent<Room>();
                if (r) r.roomID = _rooms.Count;

                var entry = BuildEntryFromInstance(room);
                _rooms.Add(entry);
                _chain.Add(_rooms.Count - 1);

                prev = entry;
                placed = true;
                break;
            }

            // 보정(기존 로직 유지)
            if (tries % 3 == 0) pos.y += Mathf.Max(0.5f, padding.y * 0.5f);
            else pos.x = baseX + Random.Range(-horizontalJitter, horizontalJitter);

            tries++;
        }

        if (!placed)
            Debug.Log($"[RoomGenerator] '{prefab.name}' 배치 실패");
    }

    // ───────────── 기존 유틸 그대로 ─────────────

    int FindStartRoomIndex()
    {
        if (_rooms.Count == 0) return -1;
        bool hasPlayer = playerTransform != null;
        Vector2 anchor = hasPlayer ? (Vector2)playerTransform.position : Vector2.zero;

        if (hasPlayer)
        {
            for (int i = 0; i < _rooms.Count; i++)
                if (_rooms[i].aabb.Contains(anchor)) return i;

            float best = float.MaxValue; int bestIdx = -1;
            for (int i = 0; i < _rooms.Count; i++)
            {
                float d = Vector2.SqrMagnitude(_rooms[i].pos - anchor);
                if (d < best) { best = d; bestIdx = i; }
            }
            return bestIdx;
        }
        return 0;
    }

    bool IsOverlappingWithAny(Vector2 newPos, Vector2 newHalf)
    {
        foreach (var e in _rooms)
        {
            if (!e.go) continue;
            Vector2 d = newPos - e.pos;
            float allowX = newHalf.x + e.halfSize.x + padding.x;
            float allowY = newHalf.y + e.halfSize.y + padding.y;
            if (Mathf.Abs(d.x) < allowX && Mathf.Abs(d.y) < allowY)
                return true;
        }
        return false;
    }

    RoomEntry BuildEntryFromInstance(GameObject go)
    {
        Bounds? bounds = null;

        var cols = go.GetComponentsInChildren<Collider2D>(true);
        foreach (var c in cols)
        {
            if (bounds == null) bounds = c.bounds;
            else { var b = bounds.Value; b.Encapsulate(c.bounds); bounds = b; }
        }

        if (bounds == null)
        {
            var rends = go.GetComponentsInChildren<Renderer>(true);
            foreach (var r in rends)
            {
                if (bounds == null) bounds = r.bounds;
                else { var b = bounds.Value; b.Encapsulate(r.bounds); bounds = b; }
            }
        }

        Bounds finalB = bounds ?? new Bounds(go.transform.position, new Vector3(1f, 1f, 1f));
        Vector3 size = finalB.size;

        return new RoomEntry
        {
            go = go,
            halfSize = new Vector2(size.x * 0.5f, size.y * 0.5f),
            aabb = finalB
        };
    }

    Vector2 ComputeHalfSizeFromPrefab(GameObject prefab)
    {
        GameObject temp = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        temp.SetActive(true);
        var entry = BuildEntryFromInstance(temp);
        Destroy(temp);
        return entry.halfSize;
    }

    // (선택) 문 이동용
    public Vector2 GetNextRoomPositionByY(Vector2 fromPos, float epsilon = 0.1f)
    {
        float curY = fromPos.y;
        Vector2 best = fromPos;
        float bestY = float.PositiveInfinity;

        foreach (var e in _rooms)
        {
            if (!e.go) continue;
            float y = e.pos.y;
            if (y > curY + epsilon && y < bestY)
            {
                bestY = y;
                best = e.pos;
            }
        }
        return best;
    }

    public GameObject GetChainedRoom(int chainIndex)
    {
        if (chainIndex < 0 || chainIndex >= _chain.Count) return null;
        int idx = _chain[chainIndex];
        if (idx < 0 || idx >= _rooms.Count) return null;
        return _rooms[idx].go;
    }
}
