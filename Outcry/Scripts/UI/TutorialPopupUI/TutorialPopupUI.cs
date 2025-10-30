using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;

public class TutorialPopupUI : UIPopup
{
    [Header("UI 요소 연결")]
    [SerializeField] private Image loadingImage;
    [SerializeField] private Image tutorialImage;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Button backButton;
    [SerializeField] private Button nextButton;

    private List<TutorialPageData> currentPages;
    private int currentPageIndex = 0;

    // 로드된 에셋의 핸들을 저장할 변수 추가(메모리 해제를 위해 필수)
    private AsyncOperationHandle<Sprite> currentSpriteHandle;

    private Action onTutorialClosedCallback;

    private void Awake()
    {
        backButton.onClick.AddListener(OnClickBack);
        nextButton.onClick.AddListener(OnClickNext);
    }

    private void OnDisable()
    {
        // UI가 비활성화될 때 현재 로드된 스프라이트가 있다면 메모리에서 해제
        if (currentSpriteHandle.IsValid())
        {
            Addressables.Release(currentSpriteHandle);
        }
    }

    public void Setup(TutorialDataSO tutorialData, Action onTutorialClosed = null)
    {
        onTutorialClosedCallback = onTutorialClosed;

        if (tutorialData == null || tutorialData.pages.Count == 0)
        {
            Debug.LogError("튜토리얼 데이터가 비어있습니다!");
            UIManager.Instance.Hide<TutorialPopupUI>();
            CursorManager.Instance.SetInGame(true);
            PlayerManager.Instance.player.PlayerInputEnable();
            return;
        }

        currentPages = tutorialData.pages;
        ShowPage(0).Forget();
    }

    private void OnClickBack()
    {
        ShowPage(currentPageIndex - 1).Forget(); // 비동기 호출
    }

    private void OnClickNext()
    {
        ShowPage(currentPageIndex + 1).Forget(); // 비동기 호출
    }

    private async UniTask ShowPage(int pageIndex)
    {
        if (pageIndex < 0 || pageIndex >= currentPages.Count)
        {
            // 마지막 페이지 넘어가면 UI 닫기
            ClosePopupAndInvokeCallback();
            return;
        }

        tutorialImage.gameObject.SetActive(false);
        loadingImage.gameObject.SetActive(true);

        currentPageIndex = pageIndex;
        TutorialPageData currentPageData = currentPages[currentPageIndex];

        var localizedTutorialDesc = LocalizationUtility.IsCurrentLanguage("en") ? currentPageData.Description : currentPageData.Description_Ko;

        // 텍스트 즉시 업데이트
        descriptionText.text = localizedTutorialDesc;

        // 버튼 상태 업데이트
        backButton.interactable = (currentPageIndex > 0);
        nextButton.interactable = (currentPageIndex < currentPages.Count);

        // 이미지 비동기 로딩
        // 이전 페이지에서 로드한 이미지가 있다면 먼저 메모리에서 해제
        if (currentSpriteHandle.IsValid())
        {
            Addressables.Release(currentSpriteHandle);
        }

        // 새로운 이미지 로드를 시작하고 핸들 저장
        currentSpriteHandle = currentPageData.PageImageRef.LoadAssetAsync<Sprite>();

        // 로딩이 끝날 때까지 기다림
        await currentSpriteHandle;

        // 로딩이 성공했고 UI 오브젝트가 파괴되지 않았다면 스프라이트 적용
        if (currentSpriteHandle.Status == AsyncOperationStatus.Succeeded && this != null)
        {
            loadingImage.gameObject.SetActive(false);
            tutorialImage.sprite = currentSpriteHandle.Result;
            tutorialImage.gameObject.SetActive(true);
        }
    }

    private void ClosePopupAndInvokeCallback()
    {
        UIManager.Instance.Hide<TutorialPopupUI>();

        // 저장해둔 콜백 실행
        onTutorialClosedCallback?.Invoke();
    }
}