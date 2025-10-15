// UIFollowTarget.cs
// - ü��/����ġ ���� "�÷��̾� ����ٴϴ�" UI�� ��ġ�� �ʰ� �׾� �ø����
// - ���� Ÿ��(�÷��̾�)�� ���� �ٸ� ���� �� orderIndex�� �ٸ��� �ָ� �ڵ� ���� ����
// - Canvas�� Screen Space(Overlay/Camera)�� ��ũ�� ��ǥ��, World Space�� ���� ��ǥ�� ����

using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class UIFollowTarget : MonoBehaviour
{
    public Transform target;                // ���� ���(�÷��̾�)
    public Vector3 worldOffset = new Vector3(0, 1.2f, 0); // ���� ���� ������(���� �����̽���)
    public Vector2 screenOffset = new Vector2(0, 0);      // ��ũ�� ���� ������(��ũ�� �����̽���)

    [Header("��ħ ����(����)")]
    public int orderIndex = 0;              // 0=ù��°(��: ü��), 1=�ι�°(��: ����ġ)
    public float stackSpacing = 24f;        // �� �� ����(��ũ�� �����̽�)
    public float worldStackSpacing = 0.25f; // �� �� ����(���� �����̽� Y)

    [Header("�ε巯�� ����")]
    public float smooth = 20f;              // 0�̸� ��� �̵�

    RectTransform rt;
    Canvas rootCanvas;
    Camera uiCam;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas && rootCanvas.renderMode == RenderMode.ScreenSpaceCamera)
            uiCam = rootCanvas.worldCamera;
    }

    void LateUpdate()
    {
        if (!target || !rt) return;
        if (!rootCanvas) rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas && rootCanvas.renderMode == RenderMode.ScreenSpaceCamera && !uiCam)
            uiCam = rootCanvas.worldCamera;

        if (!rootCanvas || rootCanvas.renderMode == RenderMode.WorldSpace)
        {
            // ���� �����̽� ĵ����: ���� ��ġ�� Ÿ�� ��ġ ��ó��
            Vector3 basePos = target.position + worldOffset;
            basePos += new Vector3(0f, orderIndex * worldStackSpacing, 0f); // ���� �ױ�
            Vector3 cur = rt.position;
            Vector3 dst = (smooth > 0f) ? Vector3.Lerp(cur, basePos, Time.unscaledDeltaTime * smooth) : basePos;
            rt.position = dst;
        }
        else
        {
            // ��ũ�� �����̽�(Overlay/Camera): ��Ŀ�� ���������� ��ġ
            Vector2 screenPos;
            if (uiCam)
                screenPos = RectTransformUtility.WorldToScreenPoint(uiCam, target.position);
            else
                screenPos = RectTransformUtility.WorldToScreenPoint(null, target.position);

            // �⺻ ������ + ���� ����(���� �ױ�)
            Vector2 targetScreen = screenPos + screenOffset + new Vector2(0f, orderIndex * stackSpacing);

            Vector2 localPoint;
            RectTransform canvasRT = rootCanvas.transform as RectTransform;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRT, targetScreen, uiCam, out localPoint);

            Vector2 cur = rt.anchoredPosition;
            Vector2 dst = (smooth > 0f) ? Vector2.Lerp(cur, localPoint, Time.unscaledDeltaTime * smooth) : localPoint;
            rt.anchoredPosition = dst;
        }
    }
}
