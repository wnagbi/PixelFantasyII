using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 为移动端按钮提供不受暂停时间缩放影响的按压缩放反馈。
/// </summary>
public sealed class MobileTouchButtonFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [SerializeField, Range(0.5f, 1f)] private float pressedScale = 0.92f;
    [SerializeField, Min(0.01f)] private float pressDuration = 0.08f;
    [SerializeField, Min(0.01f)] private float releaseDuration = 0.1f;

    private Vector3 normalScale;
    private Tween scaleTween;

    private void Awake()
    {
        normalScale = transform.localScale;
    }

    private void OnDisable()
    {
        scaleTween?.Kill();
        scaleTween = null;
        transform.localScale = normalScale;
    }

    /// <summary>
    /// 在手指按下按钮时播放轻微缩小反馈。
    /// </summary>
    /// <remarks>
    /// 使用注意：Tween 使用独立更新时间，即使游戏已暂停也能正常播放。
    /// </remarks>
    public void OnPointerDown(PointerEventData eventData)
    {
        AnimateScale(normalScale * pressedScale, pressDuration);
    }

    /// <summary>
    /// 在手指抬起时恢复按钮原始缩放。
    /// </summary>
    public void OnPointerUp(PointerEventData eventData)
    {
        AnimateScale(normalScale, releaseDuration);
    }

    /// <summary>
    /// 在指针滑出按钮区域时恢复按钮原始缩放，避免按钮停留在按下状态。
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        AnimateScale(normalScale, releaseDuration);
    }

    /// <summary>
    /// 停止上一段缩放动画并播放新的目标缩放。
    /// </summary>
    private void AnimateScale(Vector3 targetScale, float duration)
    {
        scaleTween?.Kill();
        scaleTween = transform.DOScale(targetScale, duration)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true);
    }
}
