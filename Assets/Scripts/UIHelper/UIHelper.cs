using System.Collections;
using UnityEngine;

// UI 动画等待辅助组件。
// 供按钮或 UnityEvent 启动协程等待 Animator 播放完成，不使用同步 while 阻塞主线程。
public class UIHelper : MonoBehaviour
{
    [SerializeField] private Animator targetAnimator;

    // 保留 string 重载以兼容旧 UnityEvent 绑定；参数不再用于按名称查找对象。
    /// <summary>
    /// 启动等待目标 Animator 播放完成后的场景流程。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 UIHelper 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void WaitForAniFinished(string ani)
    {
        StartCoroutine(WaitForAnimator(targetAnimator));
    }

    /// <summary>
    /// 启动等待目标 Animator 播放完成后的场景流程。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 UIHelper 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void WaitForAniFinished(Animator animator)
    {
        StartCoroutine(WaitForAnimator(animator));
    }

    /// <summary>
    /// 等待 Animator 当前状态播放完成后进入目标场景。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 UIHelper 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
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
