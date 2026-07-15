using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

// 商店/技能解锁界面中的单个技能项。
// 负责显示价格、名称、描述，并在点击时用 Score 购买技能。
public class Skill : MonoBehaviour
{
    private const string SkillConfigModule = "config.skill_config";

    public int ID;
    public string nameSkill;
    public int price;
    public bool isAlreadyBuy;
    public Sprite normImage;
    public Sprite lockImage;
    public Text priceText;
    public Text nameText;
    public Text descibleText;
    public LocalizedString priceString;
    public LocalizedString nameSkillString;
    public LocalizedString describleString;
    private Sprite originImage;
    private Image image;
    

    private void Awake()
    {
        // 缓存 Image，用于切换未购买锁定图和已购买原图。
        image = GetComponent<Image>();
        priceString.TableEntryReference = "Price";
    }

    private void Start()
    {
        // 记录初始图标，购买后恢复这个图标。
        originImage = image.sprite;
        StartCoroutine(LoadAddressableIcon());

        // 如果未购买，显示锁定图。
        isAlreadyBuy = PlayerSaveStore.IsSkillUnlocked(ID);
        RefreshIconState();
    }

    private void OnEnable()
    {
        // 其他入口也可能解锁技能，所以商店项通过事件同步自己的锁定状态。
        GameEvents.SkillUnlocked += OnSkillUnlocked;
    }

    private void OnDisable()
    {
        GameEvents.SkillUnlocked -= OnSkillUnlocked;
    }
    
    private void Update()
    {
        // 刷新本地化显示。后续可以优化为语言变化或打开界面时刷新。
        priceText.text = priceString.GetLocalizedString() + price.ToString();
        nameText.text = nameSkillString.GetLocalizedString();
        descibleText.text = describleString.GetLocalizedString();
    }

    public void HandleClickSkill()
    {
        // 已购买时这里目前只打印日志；未购买时检查分数是否足够。
        if (isAlreadyBuy || PlayerSaveStore.IsSkillUnlocked(ID))
        {
            isAlreadyBuy = true;
            RefreshIconState();
            Debug.Log("激活天赋");
        }
        else 
        {
            // 分数足够时扣除 Score，解锁技能，并同步到 save_data.json。
            if (PlayerSaveStore.TrySpendScore(price)) 
            {
                PlayerSaveStore.UnlockSkill(ID);
                isAlreadyBuy = true;
                RefreshIconState();
            }   
        }
    }

    private void RefreshIconState()
    {
        if (image == null)
        {
            return;
        }

        // 已购买显示正常图标，未购买显示锁定图标。
        image.sprite = isAlreadyBuy ? originImage : lockImage;
    }

    private void OnSkillUnlocked(int skillId)
    {
        // 只处理属于当前技能项的解锁事件。
        if (skillId != ID)
        {
            return;
        }

        isAlreadyBuy = true;
        RefreshIconState();
    }

    private IEnumerator LoadAddressableIcon()
    {
        // 图标 key 从 Lua 配置读取；加载失败时继续使用 Inspector 里拖好的图。
        string iconKey = GetSkillResourceKey(ID, "iconKey", string.Empty);
        if (string.IsNullOrWhiteSpace(iconKey))
        {
            yield break;
        }

        yield return AddressableResourceManager.LoadAsset<Sprite>(
            iconKey,
            sprite =>
            {
                originImage = sprite;
                RefreshIconState();
                Debug.Log($"[Skill] Applied Addressable icon for skill {ID}: {iconKey} -> {gameObject.name}");
            },
            () => Debug.LogWarning($"[Skill] Use Inspector fallback icon for skill {ID}.")
        );
    }

    private string GetSkillResourceKey(int skillId, string keyName, string fallback)
    {
        // Lua 表结构来自 config.skill_config：skills[skillId].iconKey / effectKey。
        if (!LuaConfig.TryGetTable(SkillConfigModule, "skills", out XLua.LuaTable skills))
        {
            return fallback;
        }

        XLua.LuaTable skill = null;
        try
        {
            skill = skills.Get<int, XLua.LuaTable>(skillId);
            if (skill == null)
            {
                return fallback;
            }

            string value = skill.Get<string>(keyName);
            return string.IsNullOrWhiteSpace(value) ? fallback : value;
        }
        catch
        {
            return fallback;
        }
        finally
        {
            skill?.Dispose();
            skills.Dispose();
        }
    }

}
