using System;
using UnityEngine;

// 버튼 타입을 정의하는 열거형
public enum EConfirmPopupType
{
    OK,         // 확인 버튼만
    OK_CANCEL,   // 확인 및 취소 버튼
    SOUL_ACQUIRE_OK, // 획득 소울 표시용
    SKILL_ACQUIRE_OK_CANCEL // 획득 스킬 표시용
}

// ConfirmUI에 전달할 데이터 클래스
public class ConfirmPopupData
{
    public string Title { get; set; }
    public string Message { get; set; }
    public string OkButtonText { get; set; }
    public EConfirmPopupType Type { get; set; }
    public Action OnClickOK { get; set; }
    public Action OnClickCancel { get; set; }

    // 동적으로 표시할 아이템/소울 이미지
    public Sprite ItemSprite;
    public Sprite SoulSprite;
}

// 사용 예시
// 단순 OK 팝업을 띄우는 메서드
//public void ShowLoginFailedPopup(string errorMessage)
//{
//    var popup = UIManager.Instance.Show<ConfirmUI>();

//    var popupData = new ConfirmPopupData
//    {
//        Title = "로그인 실패",
//        Message = errorMessage,
//        Type = EConfirmPopupType.OK,
//        // OK 버튼 클릭 시 별도 액션이 필요 없으면 null로 둡니다.
//        OnClickOK = () => { Debug.Log("확인 버튼을 눌렀습니다."); }
//    };

//    popup.Setup(popupData);
//}

// OK/Cancel 팝업을 띄우는 메서드(OK 클릭 시 스킬 구매 로직 실행)
//public void ShowPurchaseSkillPopup(Skill targetSkill)
//{
//    var popup = UIManager.Instance.Show<ConfirmUI>();

//    var popupData = new ConfirmPopupData
//    {
//        Title = "스킬 구매",
//        Message = $"{targetSkill.Name} 스킬을 구매하시겠습니까?",
//        Type = EConfirmPopupType.OK_CANCEL,
//        // OK 버튼 클릭 시 PurchaseSkill 메서드가 실행되도록 람다식으로 연결
//        OnClickOK = () => {
//            PurchaseSkill(targetSkill.ID);
//        },
//        // Cancel 버튼 클릭 시에는 로그만 남기도록 설정
//        OnClickCancel = () => {
//            Debug.Log("스킬 구매를 취소했습니다.");
//        }
//    };

//    popup.Setup(popupData);
//}

//// 스킬을 구매하는 실제 로직
//private void PurchaseSkill(int skillID)
//{
//    Debug.Log($"{skillID}번 스킬 구매 성공!");
//    // 여기에 실제 스킬 구매 로직 구현...
//}