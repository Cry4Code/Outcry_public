using System;
using System.Collections;

/// <summary>
/// LoadingManager가 수행할 선행 작업을 정의하는 클래스
/// </summary>
public class LoadingTask
{
    /// <summary>
    /// 로딩 UI에 표시될 작업 설명
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// 실제로 실행될 작업을 담고 있는 코루틴 메서드
    /// </summary>
    public Func<IEnumerator> Coroutine { get; set; }
}

