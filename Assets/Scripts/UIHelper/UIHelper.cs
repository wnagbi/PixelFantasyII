using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// UI 辅助函数集合。
// 当前只提供等待动画播放结束的工具方法。
public class UIHelper : MonoBehaviour
{
    public void WaitForAniFinished(string ani)
    {
        // 根据对象名找到 Animator，并等待当前动画状态播放到结尾。
        // 注意：这里是同步 while，复杂场景中建议改成 Coroutine 避免阻塞主线程。
        Animator animator = GameObject.Find(ani).GetComponent<Animator>();
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        // wait for the animation to finish
        while (stateInfo.normalizedTime < 1.0f)
        {
            stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.normalizedTime >= 1.0f)
            {
                break;
            }
        }
    }
}
