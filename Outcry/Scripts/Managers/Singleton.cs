using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class  Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    [Tooltip("True로 설정하면 씬이 바뀌어도 파괴되지 않습니다.")]
    [SerializeField] private bool isPersistent = false;

    private static T instance;

    // 애플리케이션 종료 상태를 확인하기 위한 플래그
    private static bool isShuttingDown = false;

    public static T Instance
    {
        get 
        {
            // 애플리케이션 종료 중이면 null 반환하여 객체 생성 막음
            if (isShuttingDown)
            {
                return null;
            }

            if (instance == null)
            {
                T[] objects = FindObjectsByType<T>(FindObjectsSortMode.None) as T[];
                if (objects.Length > 0)
                {
                    instance = objects[0];
                    for (int i = 1; i < objects.Length; i++)
                    { 
                        DestroyImmediate(objects[i].gameObject); //매니저가 다음프레임까지 남아있지 않게 하기 위해서 DestroyImmediate 를 사용
                    }
                }
                else
                {
                    instance = Create();
                }
            }
            return instance;
        }
    }

    protected virtual void Awake()
    {
        if (instance == null)
        {
            // 아직 static 인스턴스가 할당되지 않았다면 현재 인스턴스 할당
            instance = this as T;

            // isPersistent 플래그가 true일 경우 씬이 변경되어도 파괴되지 않음
            if (isPersistent)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
        else if (instance != this)
        {
            // 이미 인스턴스가 존재하지만 그 인스턴스가 isPersistent=false 이고
            // 새로 생성된 나(this)는 isPersistent=true 인 경우
            if (isPersistent && !instance.isPersistent)
            {
                // 기존의 임시 인스턴스는 파괴하고 영속성을 가진 현재 인스턴스로 교체
                Destroy(instance.gameObject);
                instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                // 그 외의 경우는 모두 중복이므로 현재 게임 오브젝트 파괴
                Destroy(gameObject);
            }
        }
    }

    protected virtual void OnDestroy()
    {
        // 파괴되는 객체가 현재 싱글톤 인스턴스인 경우 static 참조를 null로 설정
        // 이를 통해 isPersistent가 false일 때 씬이 바뀌면 다음 씬에서 새로운 인스턴스를 찾을 수 있음
        if (instance == this)
        {
            instance = null;
        }
    }

    // OnApplicationQuit 이벤트가 발생하면 종료 플래그 true 설정
    protected virtual void OnApplicationQuit()
    {
        isShuttingDown = true;
    }

    private static T Create()
    {
        GameObject go = new GameObject(typeof(T).Name);
        T result = go.AddComponent<T>();
        
        return result;
    }
}
