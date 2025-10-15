using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDUI : UIBase
{
    //체력바 -체력이 채워지는 칸
    //체력칸 -실제 채력을 보여주는 붉은 칸
    //체력바 는 항상 남아있음(안에 체력칸만 사라짐)
    [SerializeField] private UnityEngine.UI.Image staminaFill;
    [SerializeField] private TextMeshProUGUI playerName;
    [SerializeField] private HeartIcon heartPrefab;
    [SerializeField] private Transform heartsParent;
    [SerializeField] private FaceUI faceUI;
    private readonly List<HeartIcon> hearts = new(); 

    private int maxHealth;
    private int maxStamina;

    private int beforeChangeHealth; 
    private int beforeChangeStamina;


    [SerializeField] private bool lastHeartIsLeftmost = true;

    //private int LastIndex => lastHeartIsLeftmost ? 0 : Mathf.Max(0, hearts.Count - 1);
    private int LastIndex
    {
        get
        {
            if (lastHeartIsLeftmost)
            {
                // 왼쪽 끝 하트를 마지막으로 사용
                return 0;
            }
            else
            {
                // 오른쪽 끝 하트를 마지막으로 사용
                int index = hearts.Count - 1;

                // 하트가 없을 때 음수가 되지 않도록 0으로 보정
                if (index < 0)
                {
                    index = 0;
                }

                return index;
            }
        }
    }


    private void OnEnable()
    {
        EventBus.Subscribe(EventBusKey.ChangeHealth, OnHealthChanged);
        EventBus.Subscribe(EventBusKey.ChangeStamina, OnStaminaChanged);

    }

    private void OnDisable()
    {
        EventBus.Unsubscribe(EventBusKey.ChangeHealth, OnHealthChanged);
        EventBus.Unsubscribe(EventBusKey.ChangeStamina, OnStaminaChanged);
    }


    //최대 체력만큼 체력바 생성, 체력칸 채움
    private void Awake()
    {
        
    }

    private void Start()
    {
        maxHealth = PlayerManager.Instance.player.Data.maxHealth;
        maxStamina = PlayerManager.Instance.player.Data.maxStamina;

        // ★ 최대 체력만큼 하트 생성 + 초기 상태(가득 참) 표시
        BuildHearts(maxHealth);
        UpdateHearts(maxHealth);
        beforeChangeHealth = maxHealth;

        // 닉네임 표시
        if(UGSManager.Instance.IsAnonymousUser)
        {
            playerName.text = "Guest";
        }
        else
        {
            playerName.text = GameManager.Instance.CurrentUserData.Nickname;
        }
    }

    private void OnHealthChanged(object value) // 체력 변경 시
    {

        int changedHealth = (int)value;
        Debug.Log($"[로그] 체력 변경 이벤트: {changedHealth}/{maxHealth}");
        ChangeHealth(beforeChangeHealth, changedHealth);

        beforeChangeHealth = changedHealth;
    }

    private void OnStaminaChanged(object value) // 스테미너 변경 시
    {
        int changedStamina = (int)value;
        ChangeStamina(beforeChangeStamina,changedStamina);
        Debug.Log($"[로그] 스태미나 변경: {changedStamina}/{maxStamina}");
        //지금은 칸별로 줄어드는표시

        beforeChangeStamina = changedStamina;
    }

    private void ChangeHealth(int beforeChangeHealth, int changedHealth)
    {
        Debug.Log("체인지 헬스 호출됨");
        int newHealth = Mathf.Clamp(changedHealth, 0, maxHealth);
        if (newHealth == beforeChangeHealth) return;

        // 증가: 부족했던 칸을 다시 보이기
        if (newHealth > beforeChangeHealth)
        {
            for (int i = beforeChangeHealth; i < newHealth && i < hearts.Count; i++)
            {
                hearts[i].Show(true);
            }

        }
        // 감소: 아직 0은 아님 → 뒤에서부터 숨김
        if (newHealth > 0)
        {
            for (int i = beforeChangeHealth - 1; i >= newHealth; i--)
            {
                if (i >= 0 && i < hearts.Count) hearts[i].Show(false);
            }
        }

        else //죽으면
        {
            // 마지막 하트만 남기고 전부 숨김
            for (int i = 0; i < hearts.Count; i++)
            {
                if (i == LastIndex) continue;
                hearts[i].Show(false);
            }

            // 마지막 하트를 보이게 하고 깨뜨리기 (루프는 애니메이터에서 설정)
            if (hearts.Count > 0)
            {
                var last = hearts[LastIndex];
                last.Show(true);
                last.Break();
                last.SetFilled(false);
            }
            faceUI.Break();
        }

    }
    private void ChangeStamina(int beforeChangeStamina, int changedStamina)
    {

        float ratio = (float)changedStamina / Mathf.Max(1, maxStamina);
        staminaFill.fillAmount = ratio;
        Debug.Log($" 현재 스테미너 비율 {ratio}");
    }

    private void BuildHearts(int count)
    {
        // 기존 하트 정리
        for (int i = hearts.Count - 1; i >= 0; i--)
        {
            if (hearts[i] != null) Destroy(hearts[i].gameObject);
        }
        hearts.Clear();

        // 새 하트 생성
        for (int i = 0; i < count; i++)
        {
            var icon = Instantiate(heartPrefab, heartsParent);
            icon.name = $"Heart_{i + 1}";
            icon.Show(true); // 초기에는 가득 참
            hearts.Add(icon);
        }
    }

    private void UpdateHearts(int currentHealth)
    {
        for (int i = 0; i < hearts.Count; i++)
        {
            bool filled = (i < currentHealth);
            hearts[i].Show(filled);
        }
    }
}
