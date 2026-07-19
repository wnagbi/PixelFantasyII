using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// 战斗内技能释放控制器。
/// C# 保留输入入口、UI 冷却、DOTween 和特效实例化；技能规则优先交给 Lua。
/// </summary>
public class SkillController : MonoBehaviour
{
    public static SkillController Instance;
    [Header("Magnet")]
    public float magenetCD;
    public GameObject magnet;
    public Image magnetImage;
    public bool isMagnetCD;
    public UnityEvent pickUpAll;
    public Sprite magnetKeyUi;
    public Sprite magnetControUi;
    public Image magnetImageButton;

    [Header("Rage")]
    public float rageCD;
    public float rageDutation;
    public GameObject rage;
    public Image rageImage;
    public bool isRageCD;
    public Sprite rageKeyUi;
    public Sprite rageControUi;
    public Image rageImageButton;

    [Header("Dimension Slash")]
    public float dsCD;
    public Transform position;
    public GameObject ds;
    public GameObject uiFilter;
    public GameObject particle;
    public Image dsImage;
    public bool isDSCD;
    public Sprite dsKeyUi;
    public Sprite dsControUi;
    public Image dsImageButton;

    private float currentTime;
    private string dimensionSlashEffectKey;

    // 技能 Lua 规则入口。OnSkill(host, skillKey) 返回 true 表示 Lua 已经处理本次释放。
    private const string SkillLuaModule = "hotfix.skill.skill_module";
    private const string SkillConfigModule = "config.skill_config";
    private const string MagnetSkillKey = "magnet";
    private const string RageSkillKey = "rage";
    private const string DimensionSlashSkillKey = "dimension_slash";

    private void OnEnable()
    {
        // 技能购买成功后会广播 SkillUnlocked，这里负责刷新战斗技能按钮显隐。
        GameEvents.SkillUnlocked += OnSkillUnlocked;
        InputController.DeviceChanged += RefreshInputIcons;
        RefreshSkillButtons();
        RefreshInputIcons(InputController.CurrentDeviceType);
    }

    private void OnDisable()
    {
        GameEvents.SkillUnlocked -= OnSkillUnlocked;
        InputController.DeviceChanged -= RefreshInputIcons;
    }

    private void Start()
    {
        Instance = this;
        StartCoroutine(LoadAddressableSkillAssets());
        
    }

    /// <summary>
    /// 根据最新数据刷新 SkillController 的状态或显示。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 SkillController 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
    private void RefreshSkillButtons()
    {
        // PlayerSaveStore 是权威数据源；事件只负责通知“该重新读一次”。
        rage.SetActive(PlayerSaveStore.IsSkillUnlocked(2));
        magnet.SetActive(PlayerSaveStore.IsSkillUnlocked(1));
        ds.SetActive(PlayerSaveStore.IsSkillUnlocked(3));
    }

    /// <summary>
    /// 当前只有三个技能，直接统一刷新，避免写多套按钮分支。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 SkillController 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
    private void OnSkillUnlocked(int skillId)
    {
        // 当前只有三个技能，直接统一刷新，避免写多套按钮分支。
        RefreshSkillButtons();
    }
    /// <summary>
    /// 磁铁技能优先交给 Lua 判断解锁、CD、释放条件和实际效果。 Lua 不处理时，继续走下面的 C# 默认逻辑。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 SkillController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void OnSkill1()
    {
        // 磁铁技能优先交给 Lua 判断解锁、CD、释放条件和实际效果。
        // Lua 不处理时，继续走下面的 C# 默认逻辑。
        if (LuaConfig.TryCallBool(SkillLuaModule, "OnSkill", this, MagnetSkillKey, out bool handled1) && handled1)
        {
            return;
        }

        if (PlayerSaveStore.IsSkillUnlocked(1) && !isMagnetCD && Time.timeScale == 1)
        {
            pickUpAll.Invoke();
            Debug.Log("Use skill 1");
            isMagnetCD = true;
            MagnetCD(magenetCD);
        }
    }
    /// <summary>
    /// 狂怒技能优先交给 Lua。Lua 可以热更额外伤害、持续时间和 CD。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 SkillController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void OnSkill2() 
    {
        // 狂怒技能优先交给 Lua。Lua 可以热更额外伤害、持续时间和 CD。
        if (LuaConfig.TryCallBool(SkillLuaModule, "OnSkill", this, RageSkillKey, out bool handled2) && handled2)
        {
            return;
        }

        if (PlayerSaveStore.IsSkillUnlocked(2) && !isRageCD && Time.timeScale == 1)
        {
            PlayerData.getInstance().ExtraDamge = 20;
            Debug.Log("Use skill 2 " + PlayerData.getInstance().ExtraDamge);
            ImproveAttackDuration(rageDutation);
            isRageCD = true;
            
        }
    }
    /// <summary>
    /// 次元斩技能优先交给 Lua。C# 仍保留 UI、特效实例化和冷却表现。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 SkillController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void OnSkill3() 
    {
        // 次元斩技能优先交给 Lua。C# 仍保留 UI、特效实例化和冷却表现。
        if (LuaConfig.TryCallBool(SkillLuaModule, "OnSkill", this, DimensionSlashSkillKey, out bool handled3) && handled3)
        {
            return;
        }

        if (PlayerSaveStore.IsSkillUnlocked(3) && !isDSCD && Time.timeScale == 1)
        {

            Debug.Log("Use skill 3");
            uiFilter.SetActive(true);
            SpawnDimensionSlashEffect();
            DSCD(dsCD);
            isDSCD = true;

        }
    }

    /// <summary>
    /// 判断当前条件是否允许执行 CanUseSkill 对应的操作。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 SkillController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public bool CanUseSkill(int skillId)
    {
        // 暴露给 Lua 的查询方法：Lua 释放技能前可以复用 C# 的暂停、解锁、CD 判断。
        if (Time.timeScale != 1)
        {
            return false;
        }

        switch (skillId)
        {
            case 1:
                return PlayerSaveStore.IsSkillUnlocked(1) && !isMagnetCD;
            case 2:
                return PlayerSaveStore.IsSkillUnlocked(2) && !isRageCD;
            case 3:
                return PlayerSaveStore.IsSkillUnlocked(3) && !isDSCD;
            default:
                return false;
        }
    }

    /// <summary>
    /// 暴露给 Lua 的执行方法：Lua 决定释放后，让 C# 负责 UnityEvent 和 UI 冷却。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 SkillController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void TriggerMagnet(float cooldown)
    {
        // 暴露给 Lua 的执行方法：Lua 决定释放后，让 C# 负责 UnityEvent 和 UI 冷却。
        pickUpAll.Invoke();
        Debug.Log("Use skill 1");
        isMagnetCD = true;
        MagnetCD(cooldown);
    }

    /// <summary>
    /// 暴露给 Lua：Lua 传入热更后的伤害、持续时间和 CD，C# 负责应用数值和 DOTween。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 SkillController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void TriggerRage(float extraDamage, float duration, float cooldown)
    {
        // 暴露给 Lua：Lua 传入热更后的伤害、持续时间和 CD，C# 负责应用数值和 DOTween。
        PlayerData.getInstance().ExtraDamge = Mathf.RoundToInt(extraDamage);
        Debug.Log("Use skill 2 " + PlayerData.getInstance().ExtraDamge);
        rageCD = cooldown;
        ImproveAttackDuration(duration);
        isRageCD = true;
    }

    /// <summary>
    /// 暴露给 Lua：Lua 决定能否释放，C# 负责开 UI、生成特效、启动冷却。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 SkillController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void TriggerDimensionSlash(float cooldown)
    {
        // 暴露给 Lua：Lua 决定能否释放，C# 负责开 UI、生成特效、启动冷却。
        Debug.Log("Use skill 3");
        uiFilter.SetActive(true);
        SpawnDimensionSlashEffect();
        DSCD(cooldown);
        isDSCD = true;
    }
    /// <summary>
    /// 在次元斩效果结束后恢复相机或画面滤镜参数。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 SkillController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void RestoreFilter()
    {
        uiFilter.SetActive(false);
    }
    /// <summary>
    /// 在狂怒持续时间内应用额外伤害并在结束后恢复。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 SkillController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void ImproveAttackDuration(float duration) 
    {
        rageImage.fillAmount = 1;
        DOTween.To(() => currentTime,
            time =>
            {
                currentTime = time;
            },
            0,
            duration
            )
        .OnComplete(() => 
            {
                PlayerData.getInstance().ExtraDamge =0 ;
                Debug.Log("Attack buff ended " + PlayerData.getInstance().ExtraDamge);
                RageCD(rageCD);
                
            });
    }
    /// <summary>
    /// 运行狂怒技能冷却计时并恢复可释放状态。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 SkillController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void RageCD(float duration)
    {
        
        float totalTime = duration;
        DOTween.To(
            () => duration,
            value =>
            {
                rageImage.fillAmount = value / totalTime;
            },
            0,
            duration
        )
        .OnComplete(() =>
        {
            if (rageImage.fillAmount == 0)
                isRageCD = false;
            Debug.Log("Cooldown finished");
        });
    }
    /// <summary>
    /// 运行次元斩技能冷却计时并恢复可释放状态。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 SkillController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void DSCD(float duration)
    {

        float totalTime = duration;
        DOTween.To(
            () => duration,
            value =>
            {
                dsImage.fillAmount = value / totalTime;
            },
            0,
            duration
        )
        .OnComplete(() =>
        {
            if (dsImage.fillAmount == 0)
                isDSCD = false;
            Debug.Log("Cooldown finished");
        });
    }
    /// <summary>
    /// 根据保存的输入设备类型刷新技能按钮提示图标。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 SkillController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void UIChange() 
    {
        RefreshInputIcons(InputController.CurrentDeviceType);
     }

    /// <summary>
    /// 根据最新数据刷新 SkillController 的状态或显示。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 SkillController 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
    private void RefreshInputIcons(PlayerInputDeviceType deviceType)
    {
        if (deviceType == PlayerInputDeviceType.Gamepad)
        {
            if (magnetImageButton != null)
            {
                magnetImageButton.sprite = magnetControUi;
            }

            if (rageImageButton != null)
            {
                rageImageButton.sprite = rageControUi;
            }

            if (dsImageButton != null)
            {
                dsImageButton.sprite = dsControUi;
            }

            return;
        }

        if (magnetImageButton != null)
        {
            magnetImageButton.sprite = magnetKeyUi;
        }

        if (rageImageButton != null)
        {
            rageImageButton.sprite = rageKeyUi;
        }

        if (dsImageButton != null)
        {
            dsImageButton.sprite = dsKeyUi;
        }
    }
    /// <summary>
    /// 运行磁铁技能冷却计时并恢复可释放状态。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 SkillController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void MagnetCD(float duration)
    {
        magnetImage.fillAmount = 1;
        float totalTime = duration;
        DOTween.To(
            () => duration,
            value =>
            {
                magnetImage.fillAmount = value / totalTime;
            },
            0,
            duration
        )
        .OnComplete(() => 
            {
                if (magnetImage.fillAmount == 0)
                    isMagnetCD = false;
            Debug.Log("Cooldown finished");
            });
    }

    /// <summary>
    /// 加载 SkillController 中与 LoadAddressableSkillAssets 对应的数据或资源。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 SkillController 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
    private IEnumerator LoadAddressableSkillAssets()
    {
        yield return LoadSkillIcon(MagnetSkillKey, magnetImage);
        yield return LoadSkillIcon(MagnetSkillKey, magnet != null ? magnet.GetComponent<Image>() : null);
        yield return LoadSkillIcon(RageSkillKey, rageImage);
        yield return LoadSkillIcon(RageSkillKey, rage != null ? rage.GetComponent<Image>() : null);
        yield return LoadSkillIcon(DimensionSlashSkillKey, dsImage);
        yield return LoadSkillIcon(DimensionSlashSkillKey, ds != null ? ds.GetComponent<Image>() : null);

        dimensionSlashEffectKey = GetSkillResourceKey(DimensionSlashSkillKey, "effectKey", string.Empty);
    }

    /// <summary>
    /// 加载 SkillController 中与 LoadSkillIcon 对应的数据或资源。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 SkillController 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
    private IEnumerator LoadSkillIcon(string skillKey, Image targetImage)
    {
        if (targetImage == null)
        {
            yield break;
        }

        string iconKey = GetSkillResourceKey(skillKey, "iconKey", string.Empty);
        if (string.IsNullOrWhiteSpace(iconKey))
        {
            yield break;
        }

        yield return AddressableResourceManager.LoadAsset<Sprite>(
            iconKey,
            sprite =>
            {
                targetImage.sprite = sprite;
                Debug.Log($"[SkillController] Applied Addressable icon for skill {skillKey}: {iconKey} -> {targetImage.gameObject.name}");
            },
            () => Debug.LogWarning($"[SkillController] Use Inspector fallback icon for skill {skillKey}.")
        );
    }

    /// <summary>
    /// 从 Lua 技能配置中读取指定技能的 Addressable 资源 key。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 SkillController 内部流程调用，并依赖当前组件已经完成初始化。
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
    /// 优先通过 Addressables 生成次元斩特效，失败时使用 Inspector fallback。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 SkillController 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
    private void SpawnDimensionSlashEffect()
    {
        Vector3 spawnPosition = position != null ? position.position : transform.position;

        if (string.IsNullOrWhiteSpace(dimensionSlashEffectKey))
        {
            InstantiateFallbackDimensionSlash(spawnPosition);
            return;
        }

        StartCoroutine(AddressableResourceManager.InstantiateAsync(
            dimensionSlashEffectKey,
            spawnPosition,
            Quaternion.identity,
            null,
            () => InstantiateFallbackDimensionSlash(spawnPosition)
        ));
    }

    /// <summary>
    /// Addressables 特效加载失败时实例化 Inspector 中的次元斩特效。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 SkillController 内部流程调用，并依赖当前组件已经完成初始化。
    /// </remarks>
    private void InstantiateFallbackDimensionSlash(Vector3 spawnPosition)
    {
        if (particle != null)
        {
            Instantiate(particle, spawnPosition, Quaternion.identity);
        }
    }


}
