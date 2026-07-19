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
    private const string MagnetSkillKey = "magnet";
    private const string RageSkillKey = "rage";
    private const string DimensionSlashSkillKey = "dimension_slash";

    public int ID;
    [Tooltip("Lua skills 字典中的字符串 key。留空时根据 ID 使用内置兼容映射。")]
    public string configKey;
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

        // 存档继续使用数字 ID，Lua 配置使用可读字符串 key。
        configKey = ResolveSkillConfigKey(ID, configKey);
        int configuredPrice = LuaConfig.GetInt(SkillConfigModule, "skills", configKey, "price", price);
        if (configuredPrice >= 0)
        {
            price = configuredPrice;
        }
        else
        {
            Debug.LogWarning($"[Skill] Invalid Lua price for {configKey}: {configuredPrice}. Use Inspector fallback {price}.");
        }
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

        // LocalizedString 会在首次加载和语言切换时主动回调，不需要在 Update 中轮询。
        priceString.StringChanged += OnPriceStringChanged;
        nameSkillString.StringChanged += OnNameStringChanged;
        describleString.StringChanged += OnDescriptionStringChanged;
    }

    private void OnDisable()
    {
        GameEvents.SkillUnlocked -= OnSkillUnlocked;

        priceString.StringChanged -= OnPriceStringChanged;
        nameSkillString.StringChanged -= OnNameStringChanged;
        describleString.StringChanged -= OnDescriptionStringChanged;
    }
   

    /// <summary>
    /// 集中处理 Skill 中与 HandleClickSkill 对应的事件或结果。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 Skill 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
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

    /// <summary>
    /// 根据最新数据刷新 Skill 的状态或显示。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 Skill 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
    private void RefreshIconState()
    {
        if (image == null)
        {
            return;
        }

        // 已购买显示正常图标，未购买显示锁定图标。
        image.sprite = isAlreadyBuy ? originImage : lockImage;
    }

    /// <summary>
    /// 只处理属于当前技能项的解锁事件。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 Skill 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
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

    /// <summary>
    /// 本地化价格文本变化时重新拼接并显示当前价格。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 Skill 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
    private void OnPriceStringChanged(string localizedText)
    {
        if (priceText != null)
        {
            priceText.text = localizedText + price.ToString();
        }
    }

    /// <summary>
    /// 本地化名称变化时刷新当前 UI 文本。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 Skill 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
    private void OnNameStringChanged(string localizedText)
    {
        if (nameText != null)
        {
            nameText.text = localizedText;
        }
    }

    /// <summary>
    /// 本地化描述变化时刷新当前 UI 文本。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 Skill 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
    private void OnDescriptionStringChanged(string localizedText)
    {
        if (descibleText != null)
        {
            descibleText.text = localizedText;
        }
    }

    /// <summary>
    /// 加载 Skill 中与 LoadAddressableIcon 对应的数据或资源。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 Skill 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
    private IEnumerator LoadAddressableIcon()
    {
        // 图标 key 从 Lua 配置读取；加载失败时继续使用 Inspector 里拖好的图。
        string iconKey = GetSkillResourceKey(configKey, "iconKey", string.Empty);
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

    /// <summary>
    /// 从 Lua 的 skills 字符串字典中读取当前技能的 Addressable 资源 key。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 Skill 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
    private string GetSkillResourceKey(string skillKey, string keyName, string fallback)
    {
        if (string.IsNullOrWhiteSpace(skillKey))
        {
            return fallback;
        }

        return LuaConfig.GetString(SkillConfigModule, "skills", skillKey, keyName, fallback);
    }

    /// <summary>
    /// 将旧的整数技能 ID 转换成 Lua 配置使用的可读字符串 key。
    /// </summary>
    /// <remarks>
    /// 使用注意：Inspector 已填写 configKey 时优先使用该值；新增技能应明确填写 key，避免依赖默认映射。
    /// </remarks>
    private static string ResolveSkillConfigKey(int skillId, string configuredKey)
    {
        if (!string.IsNullOrWhiteSpace(configuredKey))
        {
            return configuredKey.Trim();
        }

        switch (skillId)
        {
            case 1:
                return MagnetSkillKey;
            case 2:
                return RageSkillKey;
            case 3:
                return DimensionSlashSkillKey;
            default:
                return string.Empty;
        }
    }

}
