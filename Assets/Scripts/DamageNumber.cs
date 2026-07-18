using TMPro;
using UnityEngine;
using DG.Tweening;

// 单个伤害数字 UI 对象。
// 从对象池取出后显示一段时间，时间到后回收到对象池。
public class DamageNumber : MonoBehaviour
{
    public TMP_Text damageText;
    public float lifeTime;
    private RectTransform rectTransform;
    private Sequence displaySequence;
    private bool returnPending;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void OnDisable()
    {
        // 对象可能被场景切换或其它流程提前回收，必须取消旧完成回调。
        if (displaySequence != null && displaySequence.IsActive())
        {
            displaySequence.Kill();
        }

        displaySequence = null;
        returnPending = false;
    }

    public void Setup(int damageDisplay) 
    {
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }

        // 每次从对象池取出时停止旧动画，并以本次生成位置作为新的动画起点。
        displaySequence?.Kill();
        rectTransform.DOKill();
        rectTransform.localScale = Vector3.zero;
        float targetY = rectTransform.anchoredPosition.y + 80f;

        if (damageText != null)
        {
            damageText.text = damageDisplay.ToString();
        }

        returnPending = true;
        displaySequence = DOTween.Sequence();
        displaySequence.Join(rectTransform.DOScale(1.2f, lifeTime).SetEase(Ease.OutBack));
        displaySequence.Join(rectTransform.DOAnchorPosY(targetY, lifeTime).SetEase(Ease.OutQuad));
        displaySequence.OnComplete(ReturnToPool);
    }

    private void ReturnToPool()
    {
        // 先清除标记再禁用对象，避免 OnDisable 再次处理同一条完成回调。
        if (!returnPending)
        {
            return;
        }

        returnPending = false;
        displaySequence = null;

        if (ObjPoolManager.instance != null)
        {
            ObjPoolManager.instance.ReturnObj(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
