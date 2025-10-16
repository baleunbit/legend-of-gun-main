using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneFader : MonoBehaviour
{
    public static SceneFader I { get; private set; }

    [Header("Fade")]
    public Image fadeImage;            // ��üȭ�� ���� Image (Canvas �ֻ��, Stretch, ��=����, A=1)
    public float fadeDuration = 0.5f;  // in/out �ð�

    void Awake()
    {
        if (I && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
        if (fadeImage) fadeImage.raycastTarget = true; // ���̵� �� Ŭ�� ����
    }

    void Start()
    {
        // �� �����ڸ��� ���̵�-��
        if (fadeImage) StartCoroutine(Fade(1f, 0f));
    }

    public void LoadSceneWithFade(string sceneName)
    {
        if (!fadeImage) { SceneManager.LoadScene(sceneName); return; }
        StartCoroutine(CoLoad(sceneName));
    }

    IEnumerator CoLoad(string sceneName)
    {
        yield return Fade(0f, 1f);                    // ���̵� �ƿ�
        var op = SceneManager.LoadSceneAsync(sceneName);
        while (!op.isDone) yield return null;
        yield return new WaitForEndOfFrame();
        yield return Fade(1f, 0f);                    // ���̵� ��
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
