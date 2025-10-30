using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDUI : UIBase
{
    //체력바 -체력이 채워지는 칸
    //체력칸 -실제 체력을 보여주는 붉은 칸
    //체력바 는 항상 남아있음(안에 체력칸만 사라짐)
    [SerializeField] private UnityEngine.UI.Image staminaFill;   // 스태미너 바 이미지
    [SerializeField] private TextMeshProUGUI playerName;         // 플레이어 이름 텍스트
    [SerializeField] private Image healthCase;                   // 체력바 테두리용 이미지 (Sliced 가능)
    [SerializeField] private RectTransform healthFillRect;       // ★ 변경: 체력칸(RectTransform 직접 제어)
    [SerializeField] private Animator animator;
    [SerializeField] private Image skillImage;
    [SerializeField] private float maxHeartBeatSoundDelay;
    [SerializeField] private float minHeartBeatSoundDelay;
    [SerializeField] private float heartBeatStartHp;
    [SerializeField] private GameObject bossHpLine; // 처음에 bossHpLine 자체를 숨김
    [SerializeField] private Image bossHpBar; // 체력 부분 (image.type = filled)
    [SerializeField] private int maxBossHp; // 보스의 최대 체력
    [SerializeField] private Image bossTimerBar; // 타이머 (image.type = filled)
    [SerializeField] private float maxTimer; // 최대 시간
    [SerializeField] private Image bossPortraitSprite;
    [SerializeField] private Image playerSkillCooldown; //스킬 쿨다운 표시용
    private Coroutine coSkillCooldown; //스킬 쿨다운 코루틴
    private float skillCdRemain, skillCdDuration;
    [SerializeField] private TextMeshProUGUI showCooldown;
    [SerializeField] private GameObject potionContainer;
    [SerializeField] private Image potionIcon;                 
    [SerializeField] private TextMeshProUGUI potionCountText;
    [SerializeField] private Image potionMask;
    [SerializeField] private Image staminaMask;
    [SerializeField] private Image SkillBlinkMask;
    private Coroutine coBlinkStamina;
    private Coroutine coBlinkCooldown;



    public bool isTimerSet = false;
    
    private float heartBeatSoundDelay;
    private float heartBeatConst;

    // ★★★ 아이콘 매핑용 테이블과 기본 아이콘 ★★★
    [System.Serializable]
    private struct SkillIconEntry
    {
        public int skillId;
        public Sprite icon;
    }
    [SerializeField] private List<SkillIconEntry> skillIconTable;   // 인스펙터에서 (id, sprite) 페어 추가
    private Dictionary<int, Sprite> skillIconDict;                  // 런타임 조회용 딕셔너리
    [SerializeField] private Sprite defaultSkillIcon;               // 매핑 실패 시 표시할 기본 아이콘(없으면 비활성)

    private bool prevUnder50;
    private float lastHeartBeatTime = 0f;

    [SerializeField] private FaceUI faceUI;                      // 얼굴 UI (죽을 때 애니메이션 재생)

    private int maxHealth;
    private int maxStamina;

    private int beforeChangeHealth;
    private int beforeChangeStamina;

    private Coroutine coHealthLerp;  // ★ 추가: 체력바 부드러운 줄이기용 코루틴
    private float fullHealthWidth;   // ★ 추가: 초기 체력바 전체 길이 저장용
    private int selectedskillid = 0;

    private void OnEnable()
    {
        // 체력 / 스태미너 변경 이벤트 구독
        EventBus.Subscribe(EventBusKey.ChangeHealth, OnHealthChanged);
        EventBus.Subscribe(EventBusKey.ChangeStamina, OnStaminaChanged);
        EventBus.Subscribe("SkillEquipped", OnSkillEquipped);
        EventBus.Subscribe(EventBusKey.ChangePotionCount, OnPotionChanged);
        EventBus.Subscribe(EventBusKey.CantUseCuzCooldown, NotEnoughCooldown);
        EventBus.Subscribe(EventBusKey.CantUseCuzStamina, NotEnoughStamina);

    }

    private void OnDisable()
    {
        // 구독 해제
        EventBus.Unsubscribe(EventBusKey.ChangeHealth, OnHealthChanged);
        EventBus.Unsubscribe(EventBusKey.ChangeStamina, OnStaminaChanged);
        EventBus.Unsubscribe("SkillEquipped", OnSkillEquipped);
        EventBus.Unsubscribe(EventBusKey.ChangeBossHealth, ChangeBossHpBar);
        EventBus.Unsubscribe(EventBusKey.ChangePotionCount, OnPotionChanged);
        EventBus.Unsubscribe(EventBusKey.CantUseCuzCooldown, NotEnoughCooldown);
        EventBus.Unsubscribe(EventBusKey.CantUseCuzStamina, NotEnoughStamina);

        ReleaseTimer();

        if (coSkillCooldown != null)
        {
            StopCoroutine(coSkillCooldown);
            coSkillCooldown = null;
        }
        ResetSkillCooldownOverlay();

    }

    //최대 체력만큼 체력바 생성, 체력칸 채움
    private void Awake()
    {
        // ★ 매핑 리스트 → 딕셔너리로 변환
        if (skillIconTable != null && skillIconTable.Count > 0)
        {
            skillIconDict = new Dictionary<int, Sprite>(skillIconTable.Count);
            for (int i = 0; i < skillIconTable.Count; i++)
            {
                var e = skillIconTable[i];
                skillIconDict[e.skillId] = e.icon;
            }
        }

        // 상수 q = (최대딜레이 - 최소딜레이) / 심장박동 시작하는 체력
        heartBeatConst = (maxHeartBeatSoundDelay - minHeartBeatSoundDelay) / heartBeatStartHp;
        bossHpLine.SetActive(false);

        DataTableManager.Instance.LoadCollectionData<SkillDataTable>();

    }

    private void Start()
    {
        // 최대 체력 / 스태미너 값 가져오기
        maxHealth = PlayerManager.Instance.player.Data.maxHealth;
        maxStamina = PlayerManager.Instance.player.Data.maxStamina;

        // 체력/스태미너 변경 이전 값 기록
        beforeChangeHealth = maxHealth;
        beforeChangeStamina = maxStamina;

        // ★ 추가: 체력바 RectTransform의 초기 길이 저장 (전체 체력 기준)
        if (healthFillRect != null)
            fullHealthWidth = healthFillRect.sizeDelta.x;

        if (GameManager.Instance.CurrentUserData != null)
        {
            playerName.text = GameManager.Instance.CurrentUserData.UniquePlayerName;

            selectedskillid = GameManager.Instance.CurrentUserData.SelectSkillId;
            ApplySkillIcon(selectedskillid);
        }

        var pc = PlayerManager.Instance.player.GetComponent<PlayerCondition>(); //포션 초기값 설정
        if (pc != null && pc.potionCount != null)
        {
            UpdatePotionUI(pc.potionCount.Value);
        }

    }

    private void Update()
    {


        if (prevUnder50 && Time.time - lastHeartBeatTime > heartBeatSoundDelay)
        {
            EffectManager.Instance.PlayEffectsByIdAsync(PlayerEffectID.LowHp, EffectOrder.Player,
                PlayerManager.Instance.player.gameObject).Forget();
            lastHeartBeatTime = Time.time;
        }
    }

    

    private void ApplySkillIcon(int skillId)
    {
        if (skillImage == null) return;

        Sprite icon = null;
        if (skillIconDict != null && skillIconDict.TryGetValue(skillId, out var sp) && sp != null)
        {
            icon = sp;
            //UpdateEquippedSkill(skillId);
        }
        else
        {
            icon = defaultSkillIcon; // 없으면 기본 아이콘 또는 null
        }

        if (icon != null)
        {
            skillImage.enabled = true;
            skillImage.sprite = icon;
            // 필요하면 원본 크기로:
            // skillImage.SetNativeSize();
            // 비율 유지:
            skillImage.preserveAspect = true;
            ResetSkillCooldownOverlay();
        }
        else
        {
            // 아이콘이 전혀 없으면 숨김
            skillImage.enabled = false;
        }
    }

    private float GetCooldownSeconds(int skillId) // 스킬 id 로 쿨타임 받아옴
    {
        var data = DataTableManager.Instance.GetCollectionDataById<SkillData>(skillId);
        if (data == null)
        {
            Debug.LogWarning($"[HUDUI] SkillData not found for id={skillId}");
            return 0f;
        }

        // Cooldown은 float 확정 → 음수 방지용 클램프만
        return Mathf.Max(0f, data.Cooldown);
    }

    public void UpdateEquippedSkill(int skillId) //스킬 교체 시 호출 (아이콘만 바꾸고, 쿨다운 오버레이 완전 초기화)
    {
        ApplySkillIcon(skillId);
        ResetSkillCooldownOverlay(); // 교체 시 “사용 전 상태”로
    }

    public void StartSkillCooldownById(int skillId)//스킬이 실제로 사용되었을 때 호출 (해당 ID로 쿨다운 값 조회 후 쿨다운 시작)
    {
        float cdSec = GetCooldownSeconds(skillId);
        if (cdSec <= 0f)
        {
            ResetSkillCooldownOverlay();
            return;
        }

        if (coSkillCooldown != null) StopCoroutine(coSkillCooldown);
        coSkillCooldown = StartCoroutine(CoSkillCooldown(cdSec));
    }

    private IEnumerator CoSkillCooldown(float durationSec)
    {
        skillCdDuration = durationSec;
        skillCdRemain = durationSec;

        SetCooldownVisual(1f); // 시작: 가득 덮음

        // ★ 시작 시 1초 단위 표시 (선택)
        if (showCooldown != null)
            showCooldown.text = Mathf.CeilToInt(skillCdRemain).ToString();

        while (skillCdRemain > 0f)
        {
            skillCdRemain -= Time.deltaTime; // 일시정지 시 멈춤
            float ratio = Mathf.Clamp01(skillCdRemain / Mathf.Max(0.0001f, skillCdDuration)); // 1→0

            SetCooldownVisual(ratio);

            if (showCooldown != null)// ★ 남은 쿨타임 표시 (정수로, 예: 5 → 4 → 3 → …)
                showCooldown.text = Mathf.CeilToInt(skillCdRemain).ToString();

            yield return null;
        }

        ResetSkillCooldownOverlay(); // 종료 시 제거
        coSkillCooldown = null;
    }

    private void SetCooldownVisual(float fill01) //쿨다운 오버레이 fillAmount 적용 (0이면 자동으로 비활성)
    {
        if (playerSkillCooldown == null) return;

        playerSkillCooldown.type = Image.Type.Filled; // 안전 보정
        playerSkillCooldown.fillAmount = fill01;
        playerSkillCooldown.enabled = fill01 > 0.0001f; // 0이면 숨김
    }

    private void ResetSkillCooldownOverlay()//오버레이 완전 초기화(스킬 교체 시 사용)
    {
        // 쿨다운 진행 중이면 먼저 중단
        if (coSkillCooldown != null)
        {
            StopCoroutine(coSkillCooldown);
            coSkillCooldown = null;
        }

        if (playerSkillCooldown != null)
        {
            playerSkillCooldown.fillAmount = 0f;
            playerSkillCooldown.enabled = false;
        }

        // 시각 리셋
        playerSkillCooldown.fillAmount = 0f;
        playerSkillCooldown.enabled = false;

        if (showCooldown != null)
            showCooldown.text = "";
    }



    // 체력 변경 이벤트가 발생했을 때 호출됨
    private void OnHealthChanged(object value)
    {
        int changedHealth = (int)value;
        Debug.Log($"[로그] 체력 변경 이벤트: {changedHealth}/{maxHealth}");

        // 변경 전 / 후 체력 비교 및 UI 반영
        ChangeHealth(beforeChangeHealth, changedHealth);

        // 이전 체력 갱신
        beforeChangeHealth = changedHealth;
    }

    private void OnPotionChanged(object value)
    {
        int changedPotion = (int)value;
        UpdatePotionUI(changedPotion);
    }

    private void NotEnoughStamina(object value)
    {
        bool notEnoughStamina = (bool)value;
        if (!notEnoughStamina) return;

        if (coBlinkStamina != null) StopCoroutine(coBlinkStamina);
        coBlinkStamina = StartCoroutine(CoBlink(staminaMask));
    }
    private void NotEnoughCooldown(object value)
    {
        bool notEnoughCooldown = (bool)value;
        if (!notEnoughCooldown) return;

        if (coBlinkCooldown != null) StopCoroutine(coBlinkCooldown);
        coBlinkCooldown = StartCoroutine(CoBlink(SkillBlinkMask));
    }


    // 스태미너 변경 이벤트가 발생했을 때 호출됨
    private void OnStaminaChanged(object value)
    {
        int changedStamina = (int)value;
        ChangeStamina(beforeChangeStamina, changedStamina);
        Debug.Log($"[로그] 스태미나 변경: {changedStamina}/{maxStamina}");

        // 이전 스태미너 갱신
        beforeChangeStamina = changedStamina;
    }

    // 체력 변경 시 실행되는 메서드
    private void ChangeHealth(int beforeChangeHealth, int changedHealth)
    {
        Debug.Log("체인지 헬스 호출됨");

        int newHealth = Mathf.Clamp(changedHealth, 0, maxHealth);
        if (newHealth == beforeChangeHealth) return;   // 체력 변화 없으면 리턴

        // ★ 변경: fillAmount 대신 RectTransform의 X 길이로 조절
        if (healthFillRect == null) return;

        // 체력 비율 계산 (0~1 사이)
        float ratio = Mathf.Clamp01((float)newHealth / Mathf.Max(1, maxHealth));
        float targetWidth = fullHealthWidth * ratio; // 전체 길이에 비례한 새 길이 계산

        bool isUnder50 = ratio < 0.5f;
        if (isUnder50 != prevUnder50)
        {
            animator.SetBool("Under50", isUnder50);
            prevUnder50 = isUnder50;
        }

        if (isUnder50)
        {
            heartBeatSoundDelay = changedHealth * (heartBeatConst) + minHeartBeatSoundDelay;
        }


        // ★ 변경: 코루틴으로 부드럽게 줄이기
        if (coHealthLerp != null)
        {

            StopCoroutine(coHealthLerp);
        }
        coHealthLerp = StartCoroutine(LerpHealthWidth(targetWidth, 0.2f));

   

        if (newHealth <= 0)
        {
            // 얼굴 UI 깨짐 애니메이션 재생
            faceUI?.Break();
        }


    }

    // ★ 추가: 체력바의 너비를 부드럽게 줄이는 보간 코루틴
    private IEnumerator LerpHealthWidth(float targetWidth, float duration)
    {
        float startWidth = healthFillRect.sizeDelta.x;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;

            // t 값을 0~1로 정규화
            float t = Mathf.Clamp01(time / duration);

            // 살짝 자연스러운 S-curve (가속-감속 형태)
            t = t * t * (3f - 2f * t);

            // 현재 프레임에서의 새 너비 계산
            float newWidth = Mathf.Lerp(startWidth, targetWidth, t);
            healthFillRect.sizeDelta = new Vector2(newWidth, healthFillRect.sizeDelta.y);

            yield return null;
        }

        // 마지막에는 목표 길이로 고정
        healthFillRect.sizeDelta = new Vector2(targetWidth, healthFillRect.sizeDelta.y);
    }

    // 스태미너 게이지 변경 시 실행되는 메서드
    private void ChangeStamina(int beforeChangeStamina, int changedStamina)
    {
        // 스태미나는 기존과 동일: fillAmount로 즉시 반영
        float ratio = (float)changedStamina / Mathf.Max(1, maxStamina);
        staminaFill.fillAmount = ratio;

        Debug.Log($" 현재 스태미너 비율 {ratio}");
    }

    private void OnSkillEquipped(object payload)
    {
        if (payload is int id)
        {
            ApplySkillIcon(id);              // ← 전역 읽지 말고 payload=정답 사용
            selectedskillid = id;
        }
    }

    public void SettingMaxTimer(float maxTime)
    {
        maxTimer = maxTime;
        bossTimerBar.fillAmount = 1;
        isTimerSet = true;
    }

    public void ReleaseTimer()
    {
        isTimerSet = false;
    }

    public void ChangeTimerBar(float currentTime)
    {
        if(isTimerSet)
            bossTimerBar.fillAmount = currentTime / maxTimer;
    }

    public void SettingBossHp(int maxHp)
    {
       maxBossHp = maxHp;
       bossHpLine.SetActive(true);
       bossHpBar.fillAmount = 1;
    }

    public void ChangeBossHpBar(object data)
    {
        int currentHp = (int)data;
        bossHpBar.fillAmount = (float)currentHp / maxBossHp;
    }

    public void SetBossProtrait(int id)
    {
        Sprite bossSprite = GameManager.Instance.GetSprite(id);
        if(bossSprite != null)
        {
            bossPortraitSprite.sprite = bossSprite;
        }
    }

    private void UpdatePotionUI(int count)
    {

        if (potionCountText != null)
            potionCountText.text = $"×{count}";

        // 아이콘은 항상 보이게 (숨기지 않음)
        if (potionIcon != null)
            potionIcon.enabled = true;

        // ★ 마스크는 count==0 일 때만 켜서 어둡게 덮기
        if (potionMask != null)
            potionMask.enabled = (count <= 0);
    }

    private IEnumerator CoBlink(Image target)
    {
        if (target == null) yield break;

        // 항상 알파0에서 시작 (enabled는 인스펙터에서 켜둔 상태 유지)
        SetAlpha(target, 0f);

        for (int i = 0; i < 2; i++)
        {
            SetAlpha(target, 1f);
            yield return new WaitForSeconds(0.15f);   // ← ON 유지시간

            SetAlpha(target, 0f);
            yield return new WaitForSeconds(0.15f);   // ← OFF 유지시간
        }
    }

    private void SetAlpha(Image img, float a)
    {
        if (img == null) return;
        var c = img.color;
        c.a = Mathf.Clamp01(a);
        img.color = c;
    }

}
