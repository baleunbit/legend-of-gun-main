using UnityEngine;
using System.Collections;

public class ClearMgr : MonoBehaviour
{
    [Header("설정")]
    public float checkDelay = 1.0f;     // 게임 시작 후 몇 초 뒤부터 검사 시작
    public float checkInterval = 0.5f;  // 검사 간격
    public float delayBeforeLoad = 1.5f; // 클리어 후 엔드씬 전환까지 대기

    bool _startedChecking = false;
    bool _done = false;

    void Start()
    {
        // 일정 시간 뒤부터 검사 시작
        StartCoroutine(StartCheckAfterDelay());
    }

    IEnumerator StartCheckAfterDelay()
    {
        yield return new WaitForSeconds(checkDelay);
        _startedChecking = true;
        StartCoroutine(CheckLoop());
    }

    IEnumerator CheckLoop()
    {
        while (!_done)
        {
            if (_startedChecking)
            {
                var mobs = FindObjectsByType<Mob>(FindObjectsSortMode.None);
                int aliveCount = 0;
                foreach (var m in mobs)
                {
                    if (m && m.IsAlive) aliveCount++;
                }

                // 최소 한 번이라도 몹이 존재한 적이 있어야 클리어 가능
                if (aliveCount == 0 && mobs.Length > 0)
                {
                    _done = true;
                    yield return new WaitForSeconds(delayBeforeLoad);
                    SceneMgr.I?.GoToEndScene();
                }
            }
            yield return new WaitForSeconds(checkInterval);
        }
    }
}
