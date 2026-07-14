using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

// 单个伤害数字 UI 对象。
// 从对象池取出后显示一段时间，时间到后回收到对象池。
public class DamageNumber : MonoBehaviour
{
    public TMP_Text damageText;
    public float lifeTime;
    private float lifeCounter;

    private void Start()
    {
        // 初始生命周期。
        
        lifeCounter = lifeTime;
    }
    private void Update()
    {
        // 倒计时结束后回收到对象池。
        if (lifeCounter > 0)
        {
            lifeCounter -= Time.deltaTime;
            if (lifeCounter <= 0) 
            {
                ObjPoolManager.instance.ReturnObj(gameObject);
            }
        }
    }
    public void Setup(int damageDisplay) 
    {
        // 每次从对象池取出时重置生命周期和显示数字。
        RectTransform rect = GetComponent<RectTransform>();
        rect.DOKill();
        rect.localScale = Vector3.zero;
        rect.DOScale(1.2f, lifeTime).SetEase(Ease.OutBack);
        rect.DOAnchorPosY(rect.anchoredPosition.y + 80f, lifeTime).SetEase(Ease.OutQuad);     
        lifeCounter = lifeTime;
        damageText.text = damageDisplay.ToString(); 
    }
}
