using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.UI;

// 暂停/Esc 面板启用时的表现控制。
// 打开面板时触发 UnityEvent，并随机显示一条本地化提示文本。
public class EscController : MonoBehaviour
{

    [SerializeField] private UnityEvent openPanel;
    public Text tipText;
    public LocalizedString[] tipString;
    private void OnEnable()
    {
        // 面板显示时执行外部绑定的打开逻辑，例如暂停时间、选中默认按钮等。
        openPanel.Invoke();

        // 从本地化提示池中随机取一条显示。
        tipText.text = tipString[Random.Range(0, tipString.Length)].GetLocalizedString();
    }

}
