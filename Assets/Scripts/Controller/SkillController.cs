using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// 战斗内技能释放控制器。
// C# 保留输入入口、UI 冷却、DOTween 和特效实例化；技能规则优先交给 Lua。
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

    // 技能 Lua 规则入口。OnSkill(host, skillId) 返回 true 表示 Lua 已经处理本次释放。
    private const string SkillLuaModule = "hotfix.skill.skill_module";
    private const string SkillConfigModule = "config.skill_config";

    private void OnEnable()
    {
        if (PlayerSaveStore.IsSkillUnlocked(2))
            rage.SetActive(true);
        else
            rage.SetActive(false);
        if (PlayerSaveStore.IsSkillUnlocked(1))
            magnet.SetActive(true);
        else
            magnet.SetActive(false);

        if (PlayerSaveStore.IsSkillUnlocked(3))
            ds.SetActive(true);
        else
            ds.SetActive(false);
    }
    private void Start()
    {
        Instance = this;
        StartCoroutine(LoadAddressableSkillAssets());
    }
    private void Update()
    {
        UIChange();
    }
    public void OnSkill1()
    {
        // 磁铁技能优先交给 Lua 判断解锁、CD、释放条件和实际效果。
        // Lua 不处理时，继续走下面的 C# 默认逻辑。
        if (LuaConfig.TryCallBool(SkillLuaModule, "OnSkill", this, 1, out bool handled1) && handled1)
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
    public void OnSkill2() 
    {
        // 狂怒技能优先交给 Lua。Lua 可以热更额外伤害、持续时间和 CD。
        if (LuaConfig.TryCallBool(SkillLuaModule, "OnSkill", this, 2, out bool handled2) && handled2)
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
    public void OnSkill3() 
    {
        // 次元斩技能优先交给 Lua。C# 仍保留 UI、特效实例化和冷却表现。
        if (LuaConfig.TryCallBool(SkillLuaModule, "OnSkill", this, 3, out bool handled3) && handled3)
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

    public void TriggerMagnet(float cooldown)
    {
        // 暴露给 Lua 的执行方法：Lua 决定释放后，让 C# 负责 UnityEvent 和 UI 冷却。
        pickUpAll.Invoke();
        Debug.Log("Use skill 1");
        isMagnetCD = true;
        MagnetCD(cooldown);
    }

    public void TriggerRage(float extraDamage, float duration, float cooldown)
    {
        // 暴露给 Lua：Lua 传入热更后的伤害、持续时间和 CD，C# 负责应用数值和 DOTween。
        PlayerData.getInstance().ExtraDamge = Mathf.RoundToInt(extraDamage);
        Debug.Log("Use skill 2 " + PlayerData.getInstance().ExtraDamge);
        rageCD = cooldown;
        ImproveAttackDuration(duration);
        isRageCD = true;
    }

    public void TriggerDimensionSlash(float cooldown)
    {
        // 暴露给 Lua：Lua 决定能否释放，C# 负责开 UI、生成特效、启动冷却。
        Debug.Log("Use skill 3");
        uiFilter.SetActive(true);
        SpawnDimensionSlashEffect();
        DSCD(cooldown);
        isDSCD = true;
    }
    public void RestoreFilter()
    {
        uiFilter.SetActive(false);
    }
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
    public void UIChange() 
    {
        bool isGamepad = InputController.instance.GetActiveInputDevice() is Gamepad;
        bool isKey = InputController.instance.GetActiveInputDevice() is Keyboard;
        if (isGamepad) 
        {
            //Debug.Log("Gamepad");
            magnetImageButton.sprite = magnetControUi;
            rageImageButton.sprite = rageControUi;
            dsImageButton.sprite = dsControUi;

        }
        if (isKey) 
        {
            //Debug.Log("Key");
            magnetImageButton.sprite = magnetKeyUi;
            rageImageButton.sprite = rageKeyUi;
            dsImageButton.sprite = dsKeyUi;
        }
     }
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

    private IEnumerator LoadAddressableSkillAssets()
    {
        yield return LoadSkillIcon(1, magnetImage);
        yield return LoadSkillIcon(1, magnet != null ? magnet.GetComponent<Image>() : null);
        yield return LoadSkillIcon(2, rageImage);
        yield return LoadSkillIcon(2, rage != null ? rage.GetComponent<Image>() : null);
        yield return LoadSkillIcon(3, dsImage);
        yield return LoadSkillIcon(3, ds != null ? ds.GetComponent<Image>() : null);

        dimensionSlashEffectKey = GetSkillResourceKey(3, "effectKey", string.Empty);
    }

    private IEnumerator LoadSkillIcon(int skillId, Image targetImage)
    {
        if (targetImage == null)
        {
            yield break;
        }

        string iconKey = GetSkillResourceKey(skillId, "iconKey", string.Empty);
        if (string.IsNullOrWhiteSpace(iconKey))
        {
            yield break;
        }

        yield return AddressableResourceManager.LoadAsset<Sprite>(
            iconKey,
            sprite =>
            {
                targetImage.sprite = sprite;
                Debug.Log($"[SkillController] Applied Addressable icon for skill {skillId}: {iconKey} -> {targetImage.gameObject.name}");
            },
            () => Debug.LogWarning($"[SkillController] Use Inspector fallback icon for skill {skillId}.")
        );
    }

    private string GetSkillResourceKey(int skillId, string keyName, string fallback)
    {
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

    private void InstantiateFallbackDimensionSlash(Vector3 spawnPosition)
    {
        if (particle != null)
        {
            Instantiate(particle, spawnPosition, Quaternion.identity);
        }
    }


}
