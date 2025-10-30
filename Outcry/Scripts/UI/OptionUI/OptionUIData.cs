using System;

public class OptionUIData
{
    public string ExitText { get; set; }
    public EOptionUIType Type { get; set; }
    public Action OnClickExitAction { get; set; } // 나가기 버튼의 동작을 주입하기 위한 Action
    public Action OnClickStageOptionExitAction { get; set; } // 스테이지 옵션 나가기 버튼의 동작을 주입하기 위한 Action
}
