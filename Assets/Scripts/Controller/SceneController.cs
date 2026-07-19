using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// 场景暂停与切换控制器。
// 支持立即切场景，也支持等待指定 Animator 播放完成后再进入目标场景。
public class SceneController : MonoBehaviour
{
    [SerializeField] private Animator waitAnimator;
    [SerializeField] private string waitFinishedSceneName = "level1";

    /// <summary>
    /// timeScale 为 0 时暂停使用缩放时间的游戏逻辑。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 SceneController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void TimePause()
    {
        // timeScale 为 0 时暂停使用缩放时间的游戏逻辑。
        Time.timeScale = 0f;
    }

    /// <summary>
    /// 恢复正常时间流速。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 SceneController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void TimeDisPause()
    {
        // 恢复正常时间流速。
        Time.timeScale = 1f;
    }

    /// <summary>
    /// 切场景前后统一恢复 timeScale，避免目标场景保持暂停。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 SceneController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void Teleport(string name)
    {
        // 切场景前后统一恢复 timeScale，避免目标场景保持暂停。
        SceneManager.LoadScene(name);
        Time.timeScale = 1f;
    }

    // 保留 string 重载以兼容旧 UnityEvent 绑定；参数不再用于查找 Animator。
    /// <summary>
    /// 启动等待目标 Animator 播放完成后的场景流程。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 SceneController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void WaitForAniFinished(string ani)
    {
        StartCoroutine(WaitForAnimatorAndTeleport(waitAnimator, waitFinishedSceneName));
    }

    /// <summary>
    /// 启动等待目标 Animator 播放完成后的场景流程。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 SceneController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void WaitForAniFinished(Animator animator)
    {
        StartCoroutine(WaitForAnimatorAndTeleport(animator, waitFinishedSceneName));
    }

    /// <summary>
    /// 等待过场动画结束后加载指定关卡场景。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 SceneController 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
    private IEnumerator WaitForAnimatorAndTeleport(Animator animator, string sceneName)
    {
        if (animator == null)
        {
            Debug.LogWarning("[SceneController] Animator reference is missing.", this);
            yield break;
        }

        // 等待一帧，确保按钮刚触发的动画状态已经生效。
        yield return null;

        while (animator.IsInTransition(0) || animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            yield return null;
        }

        Teleport(sceneName);
    }
}
