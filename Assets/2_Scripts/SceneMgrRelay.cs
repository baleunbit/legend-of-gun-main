// SceneMgrRelay.cs
using UnityEngine;

public class SceneMgrRelay : MonoBehaviour
{
    public void StartGame() { SceneMgr.I?.OnClickStart(); }
    public void Restart() { SceneMgr.I?.OnClickRestart(); }
    public void GoMenu() { SceneMgr.I?.OnClickMenu(); }
    public void ExitApp() { SceneMgr.I?.OnClickExit(); } // 메뉴씬에서만 쓰세요
}
