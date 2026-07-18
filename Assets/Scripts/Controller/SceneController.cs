using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// 场景暂停与切换控制器。
// 支持立即切场景，也支持等待指定 Animator 播放完成后再进入目标场景。
public class SceneController : MonoBehaviour
{
    [SerializeField] private Animator waitAnimator;
    [SerializeField] private string waitFinishedSceneName = "level1";

    public void TimePause()
    {
        // timeScale 为 0 时暂停使用缩放时间的游戏逻辑。
        Time.timeScale = 0f;
    }

    public void TimeDisPause()
    {
        // 恢复正常时间流速。
        Time.timeScale = 1f;
    }

    public void Teleport(string name)
    {
        // 切场景前后统一恢复 timeScale，避免目标场景保持暂停。
        SceneManager.LoadScene(name);
        Time.timeScale = 1f;
    }

    // 保留 string 重载以兼容旧 UnityEvent 绑定；参数不再用于查找 Animator。
    public void WaitForAniFinished(string ani)
    {
        StartCoroutine(WaitForAnimatorAndTeleport(waitAnimator, waitFinishedSceneName));
    }

    public void WaitForAniFinished(Animator animator)
    {
        StartCoroutine(WaitForAnimatorAndTeleport(animator, waitFinishedSceneName));
    }

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
