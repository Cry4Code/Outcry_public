using Cysharp.Threading.Tasks;
using SoundEnums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// 재생 중인 SFX 인스턴스의 정보를 담는 내부 클래스
/// </summary>
public class SfxPlaybackInfo
{
    public AudioPlayer Player;
    public string Address; // 리소스 해제를 위해 주소 저장
}

public class AudioManager : Singleton<AudioManager>
{
    [Header("Audio Mixer & Sources")]
    [SerializeField] private AudioMixer gameAudioMixer;
    [SerializeField] private AudioMixerGroup bgmMixerGroup;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;
    [SerializeField] private AudioSource bgmSource;

    [Header("Volume Settings")]
    [Tooltip("게임 시작 시 적용될 기본 볼륨 크기입니다(0.0 ~ 1.0).")]
    [SerializeField][Range(0f, 1f)] private float initialDefaultVolume = 0.8f;

    [Header("Object Pooling")]
    [SerializeField] private AudioPlayer audioPlayerPrefab;
    [Tooltip("풀이 가득 찼을 때 추가로 생성할 오디오 플레이어의 수")]
    [SerializeField] private int poolExpansionAmount = 10;

    private Queue<AudioPlayer> audioPlayerPool;
    private Dictionary<int, SoundData> soundDict;
    private string currentBgmAddress; // 현재 BGM 주소 추적용 변수

    // 활성 SFX 인스턴스를 ID로 추적하는 딕셔너리
    private Dictionary<int, SfxPlaybackInfo> activeSfxInstances = new Dictionary<int, SfxPlaybackInfo>();
    private int nextSfxInstanceId = 0; // 고유 ID 발급용 카운터

    private Coroutine duckingCoroutine;

    // 음소거 상태 및 이전 볼륨 저장을 위한 변수
    private bool isMasterMuted;
    private bool isBgmMuted;
    private bool isSfxMuted;

    private float lastMasterVolume;
    private float lastBgmVolume;
    private float lastSfxVolume;

    // 볼륨 설정이 외부 요인(초기화, 데이터 로드 등)에 의해 변경되었을 때 호출
    public static event Action OnVolumeSettingsChanged;
    // 음소거 상태가 변경될 때 UI에 알리기 위한 이벤트
    public static event Action<EVolumeType, bool> OnMuteStateChanged;

    // 믹서 파라미터 이름
    private const string MASTER_VOLUME_PARAM = "Master";
    private const string BGM_VOLUME_PARAM = "BGM";
    private const string SFX_VOLUME_PARAM = "SFX";

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        InitializePool();
        InitializeSoundContainer();

        // 볼륨 기본값으로 초기화
        ResetToDefaultState();

        // TODO: 초기 볼륨 설정(파이어베이스에서 불러오기?)
    }

    #region 초기화
    // 지정된 크기만큼 AudioPlayer 오브젝트를 미리 생성하여 풀에 저장
    private void InitializePool()
    {
        audioPlayerPool = new Queue<AudioPlayer>();

        ExpandPool(poolExpansionAmount);
    }

    // 사운드들을 딕셔너리에 등록
    private void InitializeSoundContainer()
    {
        // DataTableManager에서 ID를 Key로 사용하는 기본 저장소의 데이터를 가져옴
        var dataTable = DataTableManager.Instance.CollectionData;

        if (dataTable.TryGetValue(typeof(SoundData), out object soundTable))
        {
            // Manager에 저장된 Dictionary<int, IData>를 Dictionary<int, SoundData>로 변환
            var originalDict = (Dictionary<int, IData>)soundTable;
            soundDict = originalDict.ToDictionary(kvp => kvp.Key, kvp => (SoundData)kvp.Value);

            Debug.Log($"[AudioManager] {soundDict.Count} sound entries loaded successfully.");
        }
        else
        {
            Debug.LogError("[AudioManager] Sound data could not be loaded from DataTableManager. Make sure data is loaded first.");
            soundDict = new Dictionary<int, SoundData>();
        }
    }

    private void ResetToDefaultState()
    {
        // 변수 초기화
        isMasterMuted = false;
        isBgmMuted = false;
        isSfxMuted = false;

        // 볼륨 초기화
        ResetVolumes();
    }
    #endregion

    #region 오브젝트 풀링
    // 지정된 개수만큼 오디오 플레이어를 생성하여 풀에 추가
    private void ExpandPool(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            AudioPlayer newPlayer = Instantiate(audioPlayerPrefab, transform);

            AudioSource audioSource = newPlayer.GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSource.playOnAwake = false;
                audioSource.outputAudioMixerGroup = sfxMixerGroup;
            }

            newPlayer.gameObject.SetActive(false);
            audioPlayerPool.Enqueue(newPlayer);
        }
    }

    private AudioPlayer GetAudioPlayerFromPool()
    {
        if (audioPlayerPool.Count > 0)
        {
            AudioPlayer player = audioPlayerPool.Dequeue();
            player.gameObject.SetActive(true);
            return player;
        }
        else
        {
            ExpandPool(poolExpansionAmount);

            AudioPlayer newPlayer = audioPlayerPool.Dequeue();
            newPlayer.gameObject.SetActive(true);

            return newPlayer;
        }
    }

    public void ReturnAudioPlayerToPool(AudioPlayer player)
    {
        player.gameObject.SetActive(false);
        audioPlayerPool.Enqueue(player);
    }
    #endregion

    #region BGM 재생 및 관리
    /// <summary>
    /// 새로운 BGM 재생. 이전 BGM은 페이드 아웃 후 메모리에서 해제.
    /// </summary>
    public async Task PlayBGM(int id, bool loop = true, float fadeDuration = 0.5f)
    {
        if (!soundDict.TryGetValue(id, out SoundData sound) || string.IsNullOrEmpty(sound.Sound_path))
        {
            Debug.LogWarning($"BGM sound with ID '{id}' not found or path is empty!");
            return;
        }

        string newBgmAddress = sound.Sound_path;

        // 이미 같은 BGM이 재생 중이면 아무것도 하지 않음
        if (newBgmAddress == currentBgmAddress && bgmSource.isPlaying)
        {
            return;
        }

        // 기존 BGM이 있었다면 페이드 아웃 후 정지 및 언로드
        if (bgmSource.isPlaying)
        {
            await FadeOutAndStop(fadeDuration);
        }

        // 새로운 BGM 로드
        AudioClip clip = await ResourceManager.Instance.LoadAssetAddressableAsync<AudioClip>(newBgmAddress);
        if (clip == null)
        {
            Debug.LogWarning($"Failed to load BGM clip from address: {newBgmAddress}");
            currentBgmAddress = null; // 로드 실패 시 현재 BGM 주소 초기화
            ResourceManager.Instance.UnloadAddressableAsset(newBgmAddress);
            return;
        }

        // 새로운 BGM 정보 설정 및 페이드 인으로 재생
        currentBgmAddress = newBgmAddress; // 현재 BGM 주소 업데이트
        bgmSource.clip = clip;
        bgmSource.volume = 0; // 페이드 인을 위해 볼륨 0에서 시작
        bgmSource.loop = loop;
        bgmSource.playOnAwake = false;
        bgmSource.outputAudioMixerGroup = bgmMixerGroup;
        bgmSource.Play();

        Debug.Log("[AudioManager] Playing BGM: " + newBgmAddress);

        // SoundData에 저장된 최종 볼륨까지 페이드 인
        StartCoroutine(FadeIn(sound.Volume, fadeDuration));
    }

    /// <summary>
    /// 현재 재생 중인 BGM을 페이드 아웃하며 정지하고 리소스 해제
    /// </summary>
    public async Task StopBGM(float fadeDuration = 1.0f)
    {
        if (bgmSource.isPlaying)
        {
            await FadeOutAndStop(fadeDuration);
        }
    }

    // 페이드 아웃 코루틴
    private async Task FadeOutAndStop(float duration)
    {
        float startVolume = bgmSource.volume;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            bgmSource.volume = Mathf.Lerp(startVolume, 0, timer / duration);
            await UniTask.Yield(PlayerLoopTiming.Update, this.GetCancellationTokenOnDestroy());
        }

        bgmSource.volume = 0;
        bgmSource.Stop();
        bgmSource.clip = null;

        // 이전 BGM 리소스 언로드
        if (!string.IsNullOrEmpty(currentBgmAddress))
        {
            ResourceManager.Instance.UnloadAddressableAsset(currentBgmAddress);
            currentBgmAddress = null;
        }
    }

    // 페이드 인 코루틴
    private IEnumerator FadeIn(float targetVolume, float duration)
    {
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            bgmSource.volume = Mathf.Lerp(0, targetVolume, timer / duration);
            yield return null;
        }

        bgmSource.volume = targetVolume;
    }
    #endregion

    #region SFX 재생 및 관리
    /// <summary>
    /// 아이디로 효과음 재생 (기본 위치: 화면 중앙)
    /// </summary>
    //public void PlaySFX(int id)
    //{
    //    PlaySFX(id, Vector3.zero);
    //}

    /// <summary>
    /// 아이디로 효과음 특정 위치에서 재생 (예. 캐릭터 액션, 적 반응 등). 피치 고정?
    /// </summary>
    //public void PlaySFX(int id, Vector3 position)
    //{
    //    if (!soundDict.TryGetValue(id, out SoundData sound) || string.IsNullOrEmpty(sound.Sound_path))
    //    {
    //        Debug.LogWarning($"SFX sound '{name}' not found!");
    //        return;
    //    }

    //    PlaySFX(sound.Sound_path, sound.Volume, sound.Pitch, position);
    //}

    /// <summary>
    /// 오디오 클립을 직접 지정하여 재생 (기본 위치: 화면 중앙). 현재 pitch는 1로 고정
    /// </summary>
    /// <param name="audioClip"></param>
    public void PlaySFX(AudioClip audioClip, float volume)
    {
        if(audioClip == null)
        {
            Debug.LogWarning("AudioClip is null. Cannot play SFX.");
            return;
        }

        PlaySFX(audioClip, volume, Vector3.zero);
    }

    /// <summary>
    /// 오디오 클립을 직접 지정하여 재생 (특정 위치). 현재 pitch는 1로 고정
    /// </summary>
    public void PlaySFX(AudioClip audioClip, float volume, Vector3 position)
    {
        if(audioClip == null)
        {
            Debug.LogWarning("AudioClip is null. Cannot play SFX.");
            return;
        }

        AudioPlayer player = GetAudioPlayerFromPool();
        if (player != null)
        {
            player.transform.position = position;
            player.gameObject.SetActive(true);
            player.Play(audioClip, volume, 1);
        }
    }

    /// <summary>
    /// 주소로 효과음 재생 (기본 위치: 화면 중앙). 현재 pitch는 1로 고정
    /// 주소로 효과음을 비동기적으로 재생하고 제어 ID를 반환. 사운드 로딩 및 재생 시작까지 기다림.
    /// </summary>
    public async UniTask<int> PlaySFXAsync(string address, float volume)
    {
        return await PlaySFXAsync(address, volume, 1, Vector3.zero);
    }

    /// <summary>
    /// 주소로 효과음을 비동기적으로 특정 위치에서 재생하고 제어 ID 반환
    /// 사운드 로딩 및 재생 시작까지 기다림
    /// </summary>
    public async UniTask<int> PlaySFXAsync(string address, float volume, float pitch, Vector3 position)
    {
        if (string.IsNullOrEmpty(address))
        {
            return -1;
        }

        // AudioClip 로딩을 기다림
        AudioClip clip = await ResourceManager.Instance.LoadAssetAddressableAsync<AudioClip>(address);

        if (clip == null)
        {
            ResourceManager.Instance.UnloadAddressableAsset(address);
            Debug.LogWarning($"[AudioManager] AudioClip 로딩 실패: {address}");
            return -1; // 실패 시 -1 반환
        }

        // ID와 플레이어 준비
        int instanceId = nextSfxInstanceId++;
        AudioPlayer player = GetAudioPlayerFromPool();

        // 활성 목록에 등록하고 즉시 재생
        var playbackInfo = new SfxPlaybackInfo { Player = player, Address = address };
        activeSfxInstances.Add(instanceId, playbackInfo);

        player.transform.position = position;
        player.Play(clip, volume, pitch);

        // 재생이 끝나면 리소스를 해제하는 후처' 작업을 백그라운드에서 실행
        // 이 작업은 기다리지 않음(.Forget())
        ReleaseSfxResourceOnCompletion(instanceId, clip.length).Forget();

        // 사운드 재생이 시작되었으므로 제어용 ID 즉시 반환
        return instanceId;
    }

    /// <summary>
    /// 재생이 끝난 SFX의 리소스 해제하는 후처리 전용 비동기 메서드
    /// </summary>
    private async UniTaskVoid ReleaseSfxResourceOnCompletion(int instanceId, float clipLength)
    {
        try
        {
            // 클립 길이만큼 기다림
            await UniTask.Delay(TimeSpan.FromSeconds(clipLength), ignoreTimeScale: true, cancellationToken: this.GetCancellationTokenOnDestroy());
        }
        finally
        {
            // 대기가 끝났을 때 StopSFX에 의해 미리 제거되지 않았다면 리소스 정리
            if (activeSfxInstances.Remove(instanceId, out SfxPlaybackInfo info))
            {
                ReturnAudioPlayerToPool(info.Player);
                ResourceManager.Instance.UnloadAddressableAsset(info.Address);
            }
        }
    }

    /// <summary>
    /// 지정된 ID의 SFX 재생을 즉시 중지하고 리소스를 해제
    /// </summary>
    /// <param name="instanceId">PlaySFX가 반환한 인스턴스 ID</param>
    public void StopSFX(int instanceId)
    {
        // 활성 목록에서 해당 ID의 SFX 정보 찾아 제거
        if (activeSfxInstances.Remove(instanceId, out SfxPlaybackInfo info))
        {
            // AudioPlayer 재생 멈춤
            info.Player.Stop();

            // 사용 끝난 AudioPlayer 풀에 반환
            ReturnAudioPlayerToPool(info.Player);

            // 오디오 클립 리소스 즉시 언로드
            ResourceManager.Instance.UnloadAddressableAsset(info.Address);
        }
    }

    /// <summary>
    /// 현재 재생 중인 모든 SFX 중지
    /// </summary>
    public void StopAllSFX()
    {
        // activeSfxInstances를 직접 순회하며 요소를 제거하면 오류가 발생하므로
        // 키 목록을 복사하여 사용
        List<int> allActiveIds = activeSfxInstances.Keys.ToList();
        foreach (int id in allActiveIds)
        {
            StopSFX(id);
        }
    }

    /// <summary>
    /// 활성화된 SFX 인스턴스 ID를 사용하여 해당 오디오 클립의 길이 반환
    /// </summary>
    /// <param name="instanceId">PlaySFX가 반환한 인스턴스 ID</param>
    /// <returns>오디오 클립의 길이(초). ID가 유효하지 않으면 0을 반환</returns>
    public float GetSfxLength(int instanceId)
    {
        // 활성 목록에서 해당 ID의 SFX 정보 찾음
        if (activeSfxInstances.TryGetValue(instanceId, out SfxPlaybackInfo info))
        {
            // AudioPlayer에 추가된 메서드를 통해 클립 길이를 가져와 반환
            return info.Player.GetClipLength();
        }

        // 활성화된 인스턴스 목록에 ID가 없으면 경고를 출력하고 0 반환
        Debug.LogWarning($"[AudioManager] GetSfxLength: 활성화된 SFX 인스턴스 ID '{instanceId}'를 찾을 수 없습니다.");

        return 0f;
    }
    #endregion

    #region 볼륨 조절
    // 볼륨 조절용
    public void SetVolume(EVolumeType type, float volume)
    {
        // 어떤 볼륨을 조절할지 결정
        string param = type == EVolumeType.Master ? MASTER_VOLUME_PARAM :
                       type == EVolumeType.BGM ? BGM_VOLUME_PARAM : SFX_VOLUME_PARAM;

        // 0~1 사이의 선형 볼륨 값을 로그 스케일(dB)로 변환(UI 값을 오디오 엔진 값으로 변환)
        float volumeDB = (volume <= 0.0001f) ? -80f : Mathf.Log10(volume) * 20f;

        // 변환된 값으로 실제 오디오 믹서의 볼륨 조절
        gameAudioMixer.SetFloat(param, volumeDB);
    }

    public float GetVolume(EVolumeType type)
    {
        string param = type == EVolumeType.Master ? MASTER_VOLUME_PARAM :
                       type == EVolumeType.BGM ? BGM_VOLUME_PARAM : SFX_VOLUME_PARAM;

        gameAudioMixer.GetFloat(param, out float value);

        return Mathf.Pow(10f, value / 20f);
    }

    // 음소거 토글
    public void ToggleMute(EVolumeType type)
    {
        // 현재 상태의 반대로 설정
        switch (type)
        {
            case EVolumeType.Master:
                SetMute(EVolumeType.Master, !isMasterMuted);
                break;

            case EVolumeType.BGM:
                SetMute(EVolumeType.BGM, !isBgmMuted);
                break;

            case EVolumeType.SFX:
                SetMute(EVolumeType.SFX, !isSfxMuted);
                break;

            default:
                Debug.LogWarning("Invalid volume type for mute operation.");
                return;
        }
    }

    public void SetMute(EVolumeType type, bool isMute)
    {
        string param = GetMixerParam(type);

        // 대상 타입에 따라 상태 변수 및 값 업데이트
        switch (type)
        {
            case EVolumeType.Master:
                if (isMasterMuted == isMute)
                {
                    return; // 상태 변경이 없으면 종료
                }
                isMasterMuted = isMute;

                if (isMute)
                {
                    gameAudioMixer.GetFloat(param, out lastMasterVolume); // 현재 볼륨 저장
                    gameAudioMixer.SetFloat(param, -80f); // 음소거
                }
                else
                {
                    gameAudioMixer.SetFloat(param, lastMasterVolume); // 저장된 볼륨 복원
                }
                break;

            case EVolumeType.BGM:
                if (isBgmMuted == isMute)
                {
                    return;
                }

                isBgmMuted = isMute;
                if (isMute)
                {
                    gameAudioMixer.GetFloat(param, out lastBgmVolume);
                    gameAudioMixer.SetFloat(param, -80f);
                }
                else
                {
                    gameAudioMixer.SetFloat(param, lastBgmVolume);
                }
                break;

            case EVolumeType.SFX:
                if (isSfxMuted == isMute)
                {
                    return;
                }

                isSfxMuted = isMute;
                if (isMute)
                {
                    gameAudioMixer.GetFloat(param, out lastSfxVolume);
                    gameAudioMixer.SetFloat(param, -80f);
                }
                else
                {
                    gameAudioMixer.SetFloat(param, lastSfxVolume);
                }
                break;
        }

        // UI 업데이트를 위해 이벤트 호출
        OnMuteStateChanged?.Invoke(type, isMute);
    }

    // 현재 음소거 상태 반환
    public bool GetMuteState(EVolumeType type)
    {
        switch (type)
        {
            case EVolumeType.Master: 
                return isMasterMuted;
            case EVolumeType.BGM: 
                return isBgmMuted;
            case EVolumeType.SFX: 
                return isSfxMuted;
            default: 
                return false;
        }
    }

    private string GetMixerParam(EVolumeType type)
    {
        switch (type)
        {
            case EVolumeType.Master: 
                return MASTER_VOLUME_PARAM;
            case EVolumeType.BGM: 
                return BGM_VOLUME_PARAM;
            case EVolumeType.SFX: 
                return SFX_VOLUME_PARAM;
            default: 
                return string.Empty;
        }
    }

    // 설정 초기화용
    public void ResetVolumes()
    {
        SetVolume(EVolumeType.Master, initialDefaultVolume);
        SetVolume(EVolumeType.BGM, initialDefaultVolume);
        SetVolume(EVolumeType.SFX, initialDefaultVolume);

        // 슬라이더 UI도 업데이트되어야 하므로 이벤트 호출
        OnVolumeSettingsChanged?.Invoke();
    }

    // 외부(Firebase 등)에서 불러온 설정 값으로 모든 볼륨 한 번에 적용
    public void ApplyAllVolumeSettings(float master, float bgm, float sfx)
    {
        SetVolume(EVolumeType.Master, master);
        SetVolume(EVolumeType.BGM, bgm);
        SetVolume(EVolumeType.SFX, sfx);

        // 외부에서 데이터가 로드되어 볼륨이 바뀌었으므로 UI 업데이트 이벤트 호출
        OnVolumeSettingsChanged?.Invoke();
    }
    #endregion

    #region 믹서 컨트롤
    // BGM 볼륨을 일시적으로 줄였다가 복원하는 효과 실행
    public void TriggerBgmDucking()
    {
        if (duckingCoroutine != null)
        {
            StopCoroutine(duckingCoroutine);
        }

        duckingCoroutine = StartCoroutine(BgmDuckingCoroutine());
    }

    private IEnumerator BgmDuckingCoroutine()
    {
        yield return FadeMixerVolume(BGM_VOLUME_PARAM, 0.1f, -15f); // 0.1초 동안 볼륨 -15dB 감소
        yield return new WaitForSeconds(0.3f); // 0.3초 대기
        yield return FadeMixerVolume(BGM_VOLUME_PARAM, 0.5f, 0f); // 0.5초 동안 볼륨 원래대로 복구
    }

    private IEnumerator FadeMixerVolume(string parameter, float duration, float targetValue)
    {
        float currentTime = 0;
        gameAudioMixer.GetFloat(parameter, out float startValue);

        while (currentTime < duration)
        {
            currentTime += Time.unscaledDeltaTime; // Time.timeScale에 영향받지 않음
            float newValue = Mathf.Lerp(startValue, targetValue, currentTime / duration);
            gameAudioMixer.SetFloat(parameter, newValue);
            yield return null;
        }
        gameAudioMixer.SetFloat(parameter, targetValue);
    }
    #endregion
}
