#if UNITY_STANDALONE_WIN || UNITY_EDITOR
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
#endif

public static class WindowUtils
{
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
    // 현재 활성화된(포커스를 가진) 창의 핸들을 가져오는 함수
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    // 특정 창의 스레드 ID와 프로세스 ID를 가져오는 함수
    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    // 현재 실행 중인 스레드의 ID를 가져오는 함수
    [DllImport("kernel32.dll")]
    private static extern uint GetCurrentThreadId();

    // 두 스레드의 입력 처리 메커니즘을 연결하거나 해제하는 함수
    [DllImport("user32.dll")]
    private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

    // 창을 맨 앞으로 가져오고 활성화하는 함수 (SetForegroundWindow보다 강력할 수 있음)
    [DllImport("user32.dll")]
    private static extern bool BringWindowToTop(IntPtr hWnd);

    // 창을 활성화하는 또 다른 방법
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetForegroundWindow(IntPtr hWnd);
#endif

    /// <summary>
    /// 현재 실행 중인 Unity 애플리케이션 창을 화면 맨 앞으로 강제로 가져옴
    /// (Windows 빌드 및 에디터에서만 작동)
    /// </summary>
    public static void FocusGameWindow()
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
        try
        {
            // 우리 게임 창의 핸들을 가져옴
            IntPtr gameWindowHandle = Process.GetCurrentProcess().MainWindowHandle;
            if (gameWindowHandle == IntPtr.Zero)
            {
                return;
            }

            // 현재 활성화된 다른 창(예: 브라우저)의 핸들을 가져옴
            IntPtr foregroundWindowHandle = GetForegroundWindow();
            if (gameWindowHandle == foregroundWindowHandle)
            {
                return; // 이미 활성화 상태면 아무것도 안 함
            }

            // 각 창의 스레드 ID를 가져옴
            uint gameThreadId = GetCurrentThreadId();
            uint foregroundThreadId = GetWindowThreadProcessId(foregroundWindowHandle, out _);

            // 활성 창의 입력 스레드에 게임 스레드 연결
            AttachThreadInput(gameThreadId, foregroundThreadId, true);

            // 창을 맨 앞으로 가져오는 API들 순차적으로 호출
            BringWindowToTop(gameWindowHandle);
            SetForegroundWindow(gameWindowHandle);

            // 스레드 연결 즉시 해제
            AttachThreadInput(gameThreadId, foregroundThreadId, false);

            UnityEngine.Debug.Log("Forcibly brought game window to front.");
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError($"Failed to focus game window: {e}");
        }
#endif
    }
}
