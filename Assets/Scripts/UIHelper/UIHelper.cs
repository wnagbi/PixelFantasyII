using System.Collections;
using UnityEngine;

// UI 动画等待辅助组件。
// 供按钮或 UnityEvent 启动协程等待 Animator 播放完成，不使用同步 while 阻塞主线程。
public class UIHelper : MonoBehaviour
{
    [SerializeField] private Animator targetAnimator;

    // 保留 string 重载以兼容旧 UnityEvent 绑定；参数不再用于按名称查找对象。
    public void WaitForAniFinished(string ani)
    {
        StartCoroutine(WaitForAnimator(targetAnimator));
    }

    public void WaitForAniFinished(Animator animator)
    {
        StartCoroutine(WaitForAnimator(animator));
    }

    private IEnumerator WaitForAnimator(Animator animator)
    {
        if (animator == null)
        {
            Debug.LogWarning("[UIHelper] Animator reference is missing.", this);
            yield break;
        }

        // 等待一帧，让刚触发的 Animator 状态真正进入播放阶段。
        yield return null;

        while (animator.IsInTransition(0) || animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            // 每帧让出主线程，直到过渡和当前状态都结束。
            yield return null;
        }
    }
}
