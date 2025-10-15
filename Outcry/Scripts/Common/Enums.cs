
public enum ESceneType
{
    None,
    TitleScene,
    LoadingScene,
    InGameScene,
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
        Village = 106000,
        RuinsOfTheFallenKing = 106001,
        Temp1 = 106002,
    }

    public enum ESpawnType
    {
        Player,
        Enemy,
        Obstacle,
    }
}
