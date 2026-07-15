using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Controls pause/resume and simple scene transitions.
public class SceneController : MonoBehaviour
{
    [SerializeField] private Animator waitAnimator;
    [SerializeField] private string waitFinishedSceneName = "level1";

    public void TimePause()
    {
        Time.timeScale = 0f;
    }

    public void TimeDisPause()
    {
        Time.timeScale = 1f;
    }

    public void Teleport(string name)
    {
        SceneManager.LoadScene(name);
        Time.timeScale = 1f;
    }

    // Kept for existing UnityEvent string bindings. The string is no longer used for lookup.
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

        yield return null;

        while (animator.IsInTransition(0) || animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            yield return null;
        }

        Teleport(sceneName);
    }
}
