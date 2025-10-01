using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TabNavigation : MonoBehaviour
{
    [Tooltip("Tab 키로 이동할 TMP_InputField들을 순서대로 등록하세요.")]
    public List<TMP_InputField> inputFields;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            // 현재 선택된 UI 요소가 있는지 확인
            GameObject currentObj = EventSystem.current.currentSelectedGameObject;
            if (currentObj == null) return;

            // 현재 선택된 요소에서 TMP_InputField 컴포넌트를 가져옴
            TMP_InputField currentField = currentObj.GetComponent<TMP_InputField>();
            if (currentField == null || !inputFields.Contains(currentField)) return;

            // 현재 필드의 인덱스를 찾음
            int currentIndex = inputFields.IndexOf(currentField);

            // 다음 인덱스 계산 (리스트의 마지막에서 다음으로 가면 처음으로 순환)
            int nextIndex = (currentIndex + 1) % inputFields.Count;

            // 다음 인풋필드를 찾아서 활성화(선택)
            TMP_InputField nextField = inputFields[nextIndex];
            nextField.ActivateInputField();
        }
    }
}
