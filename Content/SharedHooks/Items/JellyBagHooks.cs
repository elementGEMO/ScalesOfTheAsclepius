using R2API;
using RoR2;
using RoR2.Orbs;
using RoR2.Items;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using RoR2BepInExPack.GameAssetPathsBetter;
using System;

namespace ScalesAsclepius;
public class JellyBagHooks
{
    //private static readonly string InternalName = "JellyBagHooks";
    public static bool ItemEnabled;
    public static EffectDef OrbEffect;
    public static EffectDef HitEffect;
    public static GameObject RadiusEffect;

    public JellyBagHooks()
    {
        ItemEnabled = JellyBagItem.Item_Enabled.Value;

        if (ItemEnabled)
        {
            CreateOrbEffect();
            CreateHitEffect();
            CreateRadiusEffect();

            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
        }
    }

    private void CreateOrbEffect()
    {
        GameObject tempPrefab   = Addressables.LoadAssetAsync<GameObject>(RoR2_Base_Infusion.InfusionOrbEffect_prefab).WaitForCompletion().InstantiateClone("JellyBagOrbEffect");
        Texture replacedRamp    = Addressables.LoadAssetAsync<Texture>(RoR2_Base_Common_ColorRamps.texRampTritone2_png).WaitForCompletion();
        EffectComponent effect  = tempPrefab.GetComponent<EffectComponent>();
        OrbEffect orbComponent  = tempPrefab.GetComponent<OrbEffect>();

        ParticleSystem pulseGlow    = tempPrefab.transform.Find("VFX/PulseGlow").GetComponent<ParticleSystem>();
        pulseGlow.startColor        = new Color(0.224f, 0.118f, 1f, 0.2f);

        ParticleSystemRenderer core = tempPrefab.transform.Find("VFX/Core").GetComponent<ParticleSystemRenderer>();
        Material coreMat = new(core.sharedMaterial);

        coreMat.DisableKeyword("VERTEXCOLOR");
        coreMat.SetColor("_TintColor", new Color(0.224f, 0.118f, 1f));
        coreMat.SetTexture("_RemapTex", replacedRamp);
        core.sharedMaterial = coreMat;

        TrailRenderer trail     = tempPrefab.transform.Find("TrailParent/Trail").GetComponent<TrailRenderer>();
        Material trailMat       = new(trail.sharedMaterial);

        trailMat.DisableKeyword("VERTEXCOLOR");
        trailMat.SetColor("_TintColor", new Color(0.224f, 0.118f, 1f));
        trailMat.SetTexture("_RemapTex", replacedRamp);
        trail.sharedMaterial = trailMat;

        if (orbComponent) orbComponent.onArrival = new UnityEngine.Events.UnityEvent();

        OrbEffect = new()
        {
            prefab = tempPrefab,
            prefabName = "JellyBagOrbEffect",
            prefabEffectComponent = effect
        };

        ContentAddition.AddEffect(OrbEffect.prefab);
    }

    private void CreateHitEffect()
    {
        GameObject tempPrefab               = Addressables.LoadAssetAsync<GameObject>(RoR2_Base_LunarSkillReplacements.LunarSecondaryRootEffect_prefab).WaitForCompletion().InstantiateClone("JellyBagHitProc");
        EffectComponent effect              = tempPrefab.AddComponent<EffectComponent>();
        DestroyOnTimer effectTimer          = tempPrefab.GetComponent<DestroyOnTimer>();
        TemporaryVisualEffect removeEffect  = tempPrefab.GetComponent<TemporaryVisualEffect>();
        ParticleSystem sparkRender          = tempPrefab.transform.Find("Visual/Sparks").GetComponent<ParticleSystem>();

        sparkRender.playbackSpeed = 1.5f;
        sparkRender.emissionRate = 15f;
        sparkRender.maxParticles = 25;
        sparkRender.startSpeed = 5f;

        UnityEngine.Object.Destroy(tempPrefab.transform.Find("Visual/Feathers").gameObject);
        UnityEngine.Object.Destroy(tempPrefab.transform.Find("Visual/Core Slashes").gameObject);
        UnityEngine.Object.Destroy(tempPrefab.transform.Find("Visual/Rising Rings").gameObject);
        UnityEngine.Object.Destroy(removeEffect);

        effectTimer.duration = 0.25f;
        effectTimer.enabled = true;

        effect.parentToReferencedTransform = true;
        effect.positionAtReferencedTransform = true;
        effect.applyScale = true;

        HitEffect = new()
        {
            prefab = tempPrefab,
            prefabName = "JellyBagHitProc",
            prefabEffectComponent = effect
        };

        ContentAddition.AddEffect(HitEffect.prefab);
    }

    private void CreateRadiusEffect()
    {
        GameObject tempPrefab   = Addressables.LoadAssetAsync<GameObject>(RoR2_Base_NearbyDamageBonus.NearbyDamageBonusIndicator_prefab).WaitForCompletion().InstantiateClone("JellyBagIndicator");
        MeshRenderer sphere     = tempPrefab.transform.Find("Radius, Spherical").GetComponent<MeshRenderer>();
        Material sphereMat      = new(sphere.sharedMaterial);

        sphereMat.SetColor("_TintColor", new Color(0.357f, 0f, 0.957f, 1f));
        sphere.sharedMaterial = sphereMat;

        UnityEngine.Object.Destroy(tempPrefab.GetComponent<NetworkedBodyAttachment>());
        tempPrefab.AddComponent<NetworkParent>();

        RadiusEffect = tempPrefab;
    }

    private void GlobalEventManager_onServerDamageDealt(DamageReport damageReport)
    {
        CharacterBody hurtBody      = damageReport.victimBody;
        HealthComponent hurtHealth  = hurtBody?.GetComponent<HealthComponent>();
        TeamIndex bodyTeam          = damageReport.victimTeamIndex;

        if (!hurtHealth) return;

        if (Util.GetItemCountForTeam(bodyTeam, JellyBagItem.ItemDef.itemIndex, true) > 0)
        {
            float damagePercent             = damageReport.damageDealt / hurtHealth.fullCombinedHealth;
            var allyListReadOnly            = TeamComponent.GetTeamMembers(bodyTeam);
            List<CharacterBody> itemHolders = [];

            foreach (TeamComponent team in allyListReadOnly)
            {
                CharacterBody ally = team.body;
                int itemCount = ally.inventory ? ally.inventory.GetItemCount(JellyBagItem.ItemDef) : 0;

                if (ally == hurtBody) continue;
                if (!ally.inventory) continue;
                if (itemCount == 0) continue;
                
                itemHolders.Add(ally);
            }

            foreach (CharacterBody ally in itemHolders)
            {
                if (!ally.inventory) continue;

                float delta     = (hurtBody.transform.position - ally.transform.position).sqrMagnitude;
                int itemCount   = ally.inventory.GetItemCount(JellyBagItem.ItemDef);
                float radius    = JellyBagItem.Radius.Value;
                float threshold = JellyBagItem.Threshold.Value / 100f;

                if (JellyBagItem.Threshold_Stack.Value < 0)
                {
                    float itemScale = Mathf.Pow(1f + JellyBagItem.Threshold_Stack.Value / 100f, itemCount - 1);
                    threshold *= itemScale;
                }

                if (JellyBagItem.Radius_Stack.Value > 0)
                {
                    float itemScale = JellyBagItem.Radius_Stack.Value * (itemCount - 1);
                    radius += itemScale;
                }

                if (damagePercent >= threshold && delta <= Math.Pow(radius, 2))
                {
                    float healPercent = JellyBagItem.Heal_Percent.Value;

                    if (JellyBagItem.Heal_Percent_Stack.Value > 0)
                    {
                        float itemScale = JellyBagItem.Heal_Percent_Stack.Value * (itemCount - 1);
                        healPercent += itemScale;
                    }

                    OrbManager.instance.AddOrb(new JellyBagOrb()
                    {
                        origin = hurtBody.corePosition,
                        target = ally.mainHurtBox,
                        healAmount = damageReport.damageDealt * healPercent / 100f
                    });
                }
            }
        }
    }
}

public class JellyBagIndicator : BaseItemBodyBehavior
{
    private GameObject RadiusPrefab;
    private GameObject SphereIndicator;

    [ItemDefAssociation(useOnServer = true, useOnClient = true)]
    public static ItemDef GetItemDef() => JellyBagItem.Item_Enabled.Value ? JellyBagItem.ItemDef : null;

    public void OnEnable()
    {
        Inventory.onInventoryChangedGlobal += UpdateVisual;
        body.OnNetworkItemBehaviorUpdate += HandleNetworkItemBehaviorUpdate;
    }
    public void OnDisable()
    {
        Inventory.onInventoryChangedGlobal -= UpdateVisual;
        body.OnNetworkItemBehaviorUpdate -= HandleNetworkItemBehaviorUpdate;
        SetPrefab(false);
    }

    private void UpdateVisual(Inventory inventory)
    {
        CharacterMaster charMaster = inventory.GetComponent<CharacterMaster>();
        CharacterBody body = charMaster ? charMaster.GetBody() : null;

        if (body && body == this.body)
        {
            int itemCount = inventory.GetItemCount(JellyBagItem.ItemDef);
            SetPrefab(true);

            if (itemCount > 0)
            {
                float radius    = JellyBagItem.Radius.Value;

                if (JellyBagItem.Radius_Stack.Value > 0)
                {
                    float itemScale = JellyBagItem.Radius_Stack.Value * (itemCount - 1);
                    radius += itemScale;
                }

                SetRadius(radius);
                body.TransmitItemBehavior(new CharacterBody.NetworkItemBehaviorData(JellyBagItem.ItemDef.itemIndex, radius));
            }
        }
    }

    public void HandleNetworkItemBehaviorUpdate(CharacterBody.NetworkItemBehaviorData itemBehaviorData)
    {
        if (itemBehaviorData.itemIndex == JellyBagItem.ItemDef.itemIndex) SetRadius(itemBehaviorData.floatValue);
    }

    private void SetPrefab(bool active)
    {
        if (active && !RadiusPrefab)
        {
            RadiusPrefab = Instantiate(JellyBagHooks.RadiusEffect, body.GetComponent<CharacterBody>().corePosition, Quaternion.identity);
            SphereIndicator = RadiusPrefab.transform.Find("Radius, Spherical").gameObject;
            RadiusPrefab.transform.SetParent(body.GetComponent<CharacterBody>().transform);
        }
        else if (!active)
        {
            Destroy(RadiusPrefab);
            RadiusPrefab = null;
            SphereIndicator = null;
        }
    }
    private void SetRadius(float diameter)
    {
        if (SphereIndicator) SphereIndicator.transform.localScale = Vector3.one * diameter * 2;
    }
}

public class JellyBagOrb : Orb
{
    private static readonly float Speed = 30f;
    public float healAmount;
    public override void Begin()
    {
        duration = distanceToTarget / Speed;

        EffectData setEffect = new()
        {
            origin = origin,
            genericFloat = duration,
            scale = 1.75f
        };

        setEffect.SetHurtBoxReference(target);
        EffectManager.SpawnEffect(JellyBagHooks.OrbEffect.prefab, setEffect, true);
    }
    public override void OnArrival()
    {
        base.OnArrival();

        HealthComponent targetHealth = target.healthComponent;
        CharacterBody targetBody = targetHealth?.body;

        if (targetBody?.inventory)
        {
            targetHealth.Heal(healAmount, new ProcChainMask());
            Util.PlaySound("Play_UI_arenaMode_voidCollapse_select", targetBody.gameObject);
            EffectManager.SpawnEffect(JellyBagHooks.HitEffect.prefab, new EffectData
            {
                rootObject = targetBody.gameObject,
                rotation = Quaternion.identity
            }, true);
        }
    }
}