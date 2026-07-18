using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.UI;
public enum WeaponType 
    {
        knife, scythe,missile,sword,funnel,tornado
    }
// 单张武器选择卡。
// 负责显示当前武器等级/描述，并在玩家选择后调用 WeaponList 激活或升级对应武器。
public class WeaponSelect : MonoBehaviour
{
    public int id;
    public string weaponName;
    [SerializeField] private WeaponType weaponType;
    [HideInInspector]public Weapon weapon;
    [HideInInspector]public WeaponList weaponList;
    public Text nameBox;
    public Text describle;

    public LocalizedString describleString;
    public LocalizedString nameString;
    private int displayedLevel;

    private void Start()
    {
        BindWeapon();
        DescribleGenerator();
    }

    private void OnEnable()
    {
        // 本地化资源加载完成或语言变化时，只刷新对应文本。
        nameString.StringChanged += OnNameStringChanged;
        describleString.StringChanged += OnDescriptionStringChanged;
    }

    private void OnDisable()
    {
        nameString.StringChanged -= OnNameStringChanged;
        describleString.StringChanged -= OnDescriptionStringChanged;
    }

    public void DescribleGenerator() //Updata Describle of Selection Weapoin
    {
        if (weapon == null)
        {
            BindWeapon();
            if (weapon == null)
            {
                return;
            }
        }

        // 根据武器类型和等级拼 Localization key，例如 knifeLevel2。
        displayedLevel = weapon.weaponLevel;
        nameString.TableEntryReference = $"{weaponType}";
        describleString.TableEntryReference = $"{weaponType}Level{displayedLevel}";

        // 动态修改 key 后主动请求一次；结果由 StringChanged 回调写入 UI。
        nameString.RefreshString();
        describleString.RefreshString();

    }
    public void ChangeGet() //Change Weapon whether get
    {
        // 标记该武器已经获得。
        weapon.isGet = true;
    }

    public void LevelUp() //Judge which weapon want to select to level up
    {
        // 根据选择卡上的 weaponType，把选择映射到 WeaponList 中的武器索引。
        switch (weaponType) 
        {
            case WeaponType.knife:
                weaponList.GetWeapon(0);
                ChangeGet();
                break;

            case WeaponType.scythe:
                weaponList.GetWeapon(1);
                ChangeGet();
                break;
            case WeaponType.missile:
                weaponList.GetWeapon(2);
                ChangeGet();
                break;
            case WeaponType.sword:
                weaponList.GetWeapon(3);
                ChangeGet();
                break;
            case WeaponType.funnel:
                weaponList.GetWeapon(4);
                ChangeGet();
                break;
            case WeaponType.tornado:
                weaponList.GetWeapon(5);
                ChangeGet();
                break;
        }
    }

    public void Initialize(WeaponList list)
    {
        weaponList = list;
        BindWeapon();
        DescribleGenerator();
    }

    private void BindWeapon()
    {
        if (weaponList == null || weaponList.weaponList == null || id <= 0 || id > weaponList.weaponList.Length)
        {
            return;
        }

        GameObject weaponObject = weaponList.weaponList[id - 1];
        weapon = weaponObject != null ? weaponObject.GetComponent<Weapon>() : null;
    }

    private void OnNameStringChanged(string localizedText)
    {
        if (nameBox != null)
        {
            nameBox.text = $"Lv.{displayedLevel} {localizedText}";
        }
    }

    private void OnDescriptionStringChanged(string localizedText)
    {
        if (describle != null)
        {
            describle.text = localizedText;
        }
    }
}
