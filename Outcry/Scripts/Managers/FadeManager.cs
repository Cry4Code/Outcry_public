using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeManager : Singleton<FadeManager>
{
    private Image fadeImage;
    private const float fadeDuration = 0.5f; // 페이드 시간

    // 페이드 인 아웃 체크용
    [HideInInspector] public bool isFadeOut = false;

    protected override void Awake()
    {
        base.Awake();
        if (fadeImage == null)
        {
            SetupFadeImage();
        }
    }

    private void SetupFadeImage()
    {
        // 캔버스 생성
        GameObject canvasGO = new GameObject("FadeCanvas");
        canvasGO.transform.SetParent(this.transform); // FadeManager의 자식으로 설정
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999; // UI 맨 위에 표시(숫자가 높을수록 다른 UI보다 위에 그려짐)

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        // 기준 해상도를 설정(FHD 해상도)
        scaler.referenceResolution = new Vector2(1920, 1080);
        // 화면 비율이 달라질 때 너비와 높이 중 어느 쪽에 더 비중을 두고 스케일할지 결정
        // 0 = 너비 기준, 1 = 높이 기준, 0.5 = 중간
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>(); // 레이캐스트 방지용

        // 이미지 생성
        GameObject imgGO = new GameObject("FadeImage");
        imgGO.transform.SetParent(canvasGO.transform);
        fadeImage = imgGO.AddComponent<Image>();
        fadeImage.color = new Color(0, 0, 0, 0);
        fadeImage.raycastTarget = true; // 이미지가 불투명할 때 뒤에 있는 버튼 등이 클릭되는 것을 막을 수 있음

        // 풀스크린 설정
        RectTransform rt = fadeImage.rectTransform;
        rt.anchorMin = Vector2.zero; // 앵커(기준점)의 최소값을 (0, 0) (부모의 왼쪽 아래)으로 설정
        rt.anchorMax = Vector2.one; // 앵커의 최대값을 (1, 1) (부모의 오른쪽 위)으로 설정

        // 앵커로부터의 최소/최대 거리를 0으로 설정하여 부모(캔버스)의 크기에 꽉 차도록 만듦
        rt.offsetMin = Vector2.zero; 
        rt.offsetMax = Vector2.zero;
    }

    public IEnumerator FadeOut()
    {
        // 페이드 아웃이 시작되면 클릭 막기 위해 raycastTarget 활성화
        fadeImage.raycastTarget = true;
        isFadeOut = true;
        float time = 0f;

        Color color = fadeImage.color;
        color.a = 0f;

        while (time < fadeDuration)
        {
            // Time.unscaledDeltaTime은 Time.timeScale(게임 속도)에 영향을 받지 않는 실제 시간 변화량
            // 게임이 일시정지 되어도 페이드 효과는 정상적으로 작동
            float deltaTime = Time.unscaledDeltaTime;

            // 만약 프레임 드랍으로 tmpDelta가 너무 커지면 애니메이션이 툭 끊겨 보일 수 있음
            // 이를 방지하기 위해 한 프레임에 진행될 시간에 상한선을 둠 (예: 0.015초)
            deltaTime = Mathf.Min(deltaTime, 0.015f);
            time += deltaTime;

            // 두 값 사이를 선형으로 보간(부드럽게 변화)
            // color.a(투명도)를 0(투명)에서 1(불투명)까지 (time / fadeDuration) 비율에 맞춰 변경
            color.a = Mathf.Lerp(0f, 1f, time / fadeDuration);
            fadeImage.color = color;

            yield return null;
        }

        color.a = 1f;
        fadeImage.color = color;
    }

    public IEnumerator FadeIn()
    {
        float time = 0f;
        Color color = fadeImage.color;
        color.a = 1f;

        while (time < fadeDuration)
        {
            float deltaTime = Time.unscaledDeltaTime;
            deltaTime = Mathf.Min(deltaTime, 0.015f);
            time += deltaTime;

            color.a = Mathf.Lerp(1f, 0f, time / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        color.a = 0f;
        fadeImage.color = color;

        // 페이드 인이 완료되면 화면이 완전히 보이므로 다시 UI 클릭이 가능하도록 raycastTarget 비활성화
        fadeImage.raycastTarget = false;
        isFadeOut = false;
    }
}