// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
//
// /// <summary>
// /// FlyToTargetActionNode를 상속받아 비행 추격 중 애니메이션을 제어합니다.
// /// </summary>
// public class FlyChaseActionNode : FlyToTargetActionNode
// {
//     private Animator animator;
//     
//
//     public FlyChaseActionNode(Rigidbody2D rb, Transform me, Transform target, float speed, float stoppingDistance, Animator animator) 
//         : base(rb, me, target, speed, stoppingDistance) 
//     {
//         this.animator = animator;
//     }
//     
//     protected override NodeState Act()
//     {
//         // 1. 부모의 이동 로직 실행: 이 시점에서 'rise/Descent'가 출력되고 CurrentVerticalState가 업데이트됨.
//         NodeState state = base.Act(); 
//         
//         // 2. 애니메이션 제어
//         if (state == NodeState.Running)
//         {
//             // 부모의 상태를 읽어 상승/하강 애니메이션을 제어
//             if (CurrentVerticalState == VerticalState.Ascending)
//             {
//                 animator.SetBool(AnimatorHash.MonsterParameter.Ascending, true); // 상승 애니메이션 On
//                 animator.SetBool(AnimatorHash.MonsterParameter.Descending, false); // 하강 애니메이션 Off
//             }
//             else if (CurrentVerticalState == VerticalState.Descending)
//             {
//                 animator.SetBool(AnimatorHash.MonsterParameter.Ascending, false); // 상승 애니메이션 Off
//                 animator.SetBool(AnimatorHash.MonsterParameter.Descending, true);  // 하강 애니메이션 On
//             }
//             else
//             {
//                 // 수평 이동 중
//                 animator.SetBool(AnimatorHash.MonsterParameter.Ascending, false);
//                 animator.SetBool(AnimatorHash.MonsterParameter.Descending, false);
//             }
//         }
//         else
//         {
//             // 멈췄을 때 모든 애니메이션 파라미터 초기화
//             animator.SetBool(AnimatorHash.MonsterParameter.Ascending, false);
//             animator.SetBool(AnimatorHash.MonsterParameter.Descending, false);
//         }
//         return state;
//     }
//
//     public override void Reset()
//     {
//         base.Reset();
//         // 리셋 시 애니메이션 상태 초기화
//         animator.SetBool(AnimatorHash.MonsterParameter.Ascending, false);
//         animator.SetBool(AnimatorHash.MonsterParameter.Descending, false);
//     }
// }