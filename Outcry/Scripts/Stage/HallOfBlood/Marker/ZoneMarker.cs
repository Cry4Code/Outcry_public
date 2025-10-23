using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoneMarker : MonoBehaviour
{
    [SerializeField] public float rangeX;
    [SerializeField] public float rangeY;
    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 0f, 1f, 1f);
        Gizmos.DrawWireCube(transform.position, new Vector3(rangeX * 2, rangeY * 2, 1f));
    }
#endif
    //지금 존에서 특정 영역 표시. 
    //이 컴포넌트를 중심으로 좌우 x 범위, 위아래 y 범위를 지정.
    //기즈모로 표시해주기
    //FlyRandomInZone 에서 넘겨받아야함
    // >>즉, Stage3Controller에서 해당 존 마커를 관리해야함.s
}
