using System;

public class OptionUIData
{
    public EOptionUIType Type { get; set; }
    public Action OnClickExitAction { get; set; } // 나가기 버튼의 동작을 주입하기 위한 Action
}
