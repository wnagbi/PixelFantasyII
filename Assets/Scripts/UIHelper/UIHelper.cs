using System.Collections;
using UnityEngine;

// Small UI helper methods used by animation events or UI buttons.
public class UIHelper : MonoBehaviour
{
    [SerializeField] private Animator targetAnimator;

    // Kept for existing UnityEvent string bindings. The string is no longer used for lookup.
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

        yield return null;

        while (animator.IsInTransition(0) || animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            yield return null;
        }
    }
}
