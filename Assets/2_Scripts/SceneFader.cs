using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneFader : MonoBehaviour
{
    public static SceneFader I { get; private set; }

    [Header("Fade")]
    public Image fadeImage;            // 전체화면 검은 Image (Canvas 최상단, Stretch, 색=검정, A=1)
    public float fadeDuration = 0.5f;  // in/out 시간

    void Awake()
    {
        if (I && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
        if (fadeImage) fadeImage.raycastTarget = true; // 페이드 중 클릭 차단
    }

    void Start()
    {
        // 씬 들어오자마자 페이드-인
        if (fadeImage) StartCoroutine(Fade(1f, 0f));
    }

    public void LoadSceneWithFade(string sceneName)
    {
        if (!fadeImage) { SceneManager.LoadScene(sceneName); return; }
        StartCoroutine(CoLoad(sceneName));
    }

    IEnumerator CoLoad(string sceneName)
    {
        yield return Fade(0f, 1f);                    // 페이드 아웃
        var op = SceneManager.LoadSceneAsync(sceneName);
        while (!op.isDone) yield return null;
        yield return new WaitForEndOfFrame();
        yield return Fade(1f, 0f);                    // 페이드 인
    }

    IEnumerator Fade(float from, float to)
    {
        float t = 0f;
        var c = fadeImage.color;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Lerp(from, to, t / fadeDuration);
            fadeImage.color = new Color(c.r, c.g, c.b, a);
            yield return null;
        }
        fadeImage.color = new Color(c.r, c.g, c.b, to);
    }
}
