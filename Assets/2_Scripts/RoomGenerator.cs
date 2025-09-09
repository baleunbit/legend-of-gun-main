using UnityEngine;
using System.Collections.Generic;

public class RoomGenerator : MonoBehaviour
{
    [Header("프리팹 목록")]
    public List<GameObject> roomPrefabs;

    [Header("생성 설정")]
    public int additionalRooms = 6;
    public Vector2 padding = new Vector2(1f, 1f);
    public float horizontalJitter = 2f;
    public int maxTriesPerRoom = 30;

    [Header("앵커(플레이어)")]
    public Transform playerTransform;
    public bool createStartRoomIfMissing = false;

    private readonly List<RoomEntry> _rooms = new();
    private readonly List<int> _chain = new();

    private struct RoomEntry
    {
        public GameObject go;
        public Vector2 halfSize;
        public Bounds aabb;
        public Vector2 pos => go ? (Vector2)go.transform.position : Vector2.zero;
    }

    void Start()
    {
        if (!Application.isPlaying) return;

        // ✅ 프리팹 유효성
        if (roomPrefabs == null) { Debug.LogError("[RoomGenerator] roomPrefabs가 null"); enabled = false; return; }
        roomPrefabs.RemoveAll(p => p == null);
        if (roomPrefabs.Count == 0) { Debug.LogError("[RoomGenerator] roomPrefabs 비어있음"); enabled = false; return; }

        // 씬 Room 수집
        Room[] existing = FindObjectsByType<Room>(FindObjectsSortMode.None);
        foreach (var r in existing)
            _rooms.Add(BuildEntryFromInstance(r.gameObject));

        // 시작방 결정
        int startIdx = FindStartRoomIndex();
        if (startIdx < 0 && createStartRoomIfMissing)
        {
            var prefab = roomPrefabs[Random.Range(0, roomPrefabs.Count)];
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
        GenerateRoomsUpwards(startIdx, additionalRooms);
    }

    void GenerateRoomsUpwards(int startIndex, int countToAdd)
    {
        var prev = _rooms[startIndex];

        for (int i = 0; i < countToAdd; i++)
        {
            GameObject prefab = roomPrefabs[Random.Range(0, roomPrefabs.Count)];
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

                if (tries % 3 == 0) pos.y += Mathf.Max(0.5f, padding.y * 0.5f);
                else pos.x = baseX + Random.Range(-horizontalJitter, horizontalJitter);

                tries++;
            }

            if (!placed) Debug.Log($"[RoomGenerator] Room {i} 배치 실패");
        }
    }

    // ✅ 문이 쓰는 “바로 위 방” 좌표
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

    // ===== 내부 유틸 =====

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

    // (선택) 체인에서 n번째 방 반환
    public GameObject GetChainedRoom(int chainIndex)
    {
        if (chainIndex < 0 || chainIndex >= _chain.Count) return null;
        int idx = _chain[chainIndex];
        if (idx < 0 || idx >= _rooms.Count) return null;
        return _rooms[idx].go;
    }
}
