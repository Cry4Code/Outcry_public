
namespace FirebaseEnums
{
    public enum ESignUpResult
    {
        Success,
        EmailAlreadyInUse,
        WeakPassword,
        InvalidEmail,
        UnknownError
    }

    public enum ESignInResult
    {
        Success,
        UserNotFound,    // 가입되지 않은 이메일
        WrongPassword,   // 비밀번호 오류
        InvalidEmail,    // 이메일 형식 오류
        UnknownError     // 기타 알 수 없는 에러
    }

    public enum ELinkResult
    {
        Success,
        NoAnonymousUser,        // 익명 사용자가 아님
        EmailAlreadyInUse,      // 이미 사용 중인 이메일
        CredentialAlreadyInUse, // 자격 증명이 이미 다른 사용자와 연결됨
        WeakPassword,           // 너무 쉬운 비밀번호
        UnknownError            // 기타 알 수 없는 에러
    }
}

public enum ESceneType
{
    None,
    TitleScene,
    LobbyScene,
    LoadingScene,
    StageScene,
}

namespace SoundEnums
{
    public enum EVolumeType
    {
        Master,
        BGM,
        SFX
    }

    public enum EBGM
    {
        None,
        Title = 104000,
        Lobby = 104001,
        Tutorial = 104002,
        RuinsOfTheFallenKing = 104003,
    }
}

namespace StageEnums
{
    public enum EStageState
    {
        None,
        Ready,      // 전투 시작 연출
        InProgress, // 전투 진행 중
        Paused,     // 일시정지
        Finished    // 전투 종료
    }

    public enum EStageType
    {
        Tutorial = 0, // 튜토리얼 스테이지 ID를 0으로 약속
        RuinsOfTheFallenKing = 106001,
        Temp1 = 106002,
        Temp2 = 106003,
    }

    public enum ESpawnType
    {
        Player,
        Enemy,
        Obstacle,
    }
}
