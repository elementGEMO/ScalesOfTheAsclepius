using R2API;
using RoR2;
using RoR2.Orbs;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using UnityEngine;
using UnityEngine.Networking;
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

    public JellyBagHooks()
    {
        ItemEnabled = JellyBagItem.Item_Enabled.Value;

        if (ItemEnabled)
        {
            CreateOrbEffect();
            CreateHitEffect();

            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
            IL.RoR2.HealthComponent.TakeDamageProcess += HealthComponent_TakeDamageProcess;
        }
    }

    private void HealthComponent_TakeDamageProcess(ILContext il)
    {
        ILCursor cursor = new(il);
        int itemIndex = -1;

        if (cursor.TryGotoNext(
            x => x.MatchLdloc(out _),
            x => x.MatchLdfld(out var displayClass) && displayClass.Name == "damageInfo",
            x => x.MatchLdfld(typeof(DamageInfo), nameof(DamageInfo.damage)),
            x => x.MatchStloc(out itemIndex)
        ))
        {
            if (itemIndex != -1 && cursor.TryGotoNext(
                x => x.MatchLdflda(typeof(HealthComponent), nameof(HealthComponent.itemCounts)),
                x => x.MatchLdfld(typeof(HealthComponent.ItemCounts), nameof(HealthComponent.ItemCounts.parentEgg)),
                x => x.MatchLdcI4(out _),
                x => x.MatchBle(out _)
            ))
            {
                cursor.Emit(OpCodes.Ldarg, 0);
                cursor.Emit(OpCodes.Ldloc, itemIndex);
                cursor.EmitDelegate(DamageReduceEffect);
                cursor.Emit(OpCodes.Stloc, itemIndex);
            }
            else Log.Error("Couldn't Hook - JellyBag! - 2");
        }
        else Log.Error("Couldn't Hook - JellyBag! - 1");
    }
    private static float DamageReduceEffect(HealthComponent self, float damage)
    {
        CharacterBody body = self.body;

        if (body?.inventory)
        {
            int itemCount   = body.inventory.GetItemCount(JellyBagItem.ItemDef);
            bool hasBuff    = body.HasBuff(JellyCooldownBuff.BuffDef);

            if (!hasBuff && itemCount > 0)
            {
                damage = Math.Max(1f, damage - 50f * itemCount);
                for (int i = 0; i < 10; i++) body.AddTimedBuff(JellyCooldownBuff.BuffDef, i + 1);
            }
        }

        return damage;
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

    private void GlobalEventManager_onServerDamageDealt(DamageReport damageReport)
    {
        CharacterBody hurtBody      = damageReport.victimBody;
        HealthComponent hurtHealth  = hurtBody?.GetComponent<HealthComponent>();
        TeamIndex bodyTeam          = damageReport.victimTeamIndex;

        if (hurtHealth && damageReport.damageDealt / hurtHealth.fullCombinedHealth >= 0.05f)
        {
            if (Util.GetItemCountForTeam(bodyTeam, JellyBagItem.ItemDef.itemIndex, true) > 0)
            {
                List<CharacterBody> itemHolders = [];
                List<HurtBox> hurtBoxList       = HG.CollectionPool<HurtBox, List<HurtBox>>.RentCollection();
                TeamMask allyMask               = TeamMask.none; allyMask.AddTeam(bodyTeam);

                /*
                float radius = IVBagItem.Radius.Value;

                if (IVBagItem.Radius_Stack.Value > 0)
                {
                    float itemScale = IVBagItem.Radius_Stack.Value * (itemCount - 1);
                    radius += itemScale;
                }
                */

                SphereSearch radiusSearch = new()
                {
                    radius = 30,
                    origin = hurtBody.transform.position,
                    mask = LayerIndex.entityPrecise.mask,
                    queryTriggerInteraction = QueryTriggerInteraction.UseGlobal
                };

                radiusSearch.RefreshCandidates();
                radiusSearch.FilterCandidatesByHurtBoxTeam(allyMask);
                radiusSearch.OrderCandidatesByDistance();
                radiusSearch.FilterCandidatesByDistinctHurtBoxEntities();
                radiusSearch.GetHurtBoxes(hurtBoxList);

                int currentIndex = 0;

                while (currentIndex < hurtBoxList.Count)
                {
                    CharacterBody currentAlly = hurtBoxList[currentIndex]?.healthComponent.body;
                    if (currentAlly != hurtBody && currentAlly.inventory?.GetItemCount(JellyBagItem.ItemDef) > 0) itemHolders.Add(currentAlly);
                    currentIndex++;
                }

                foreach (CharacterBody itemBody in itemHolders)
                {
                    if (!itemBody) continue;

                    OrbManager.instance.AddOrb(new JellyBagOrb()
                    {
                        origin = hurtBody.corePosition,
                        target = itemBody.mainHurtBox
                    });
                }
            }
        }
    }
}

public class JellyBagOrb : Orb
{
    private static readonly float Speed = 20f;
    public override void Begin()
    {
        duration = distanceToTarget / Speed;

        EffectData setEffect = new()
        {
            origin = origin,
            genericFloat = duration,
            scale = 2f
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
            int itemCount = targetBody.inventory.GetItemCount(JellyBagItem.ItemDef);
            float percentHeal = targetHealth.combinedHealth * 0.05f * itemCount;

            targetHealth.Heal(25 + percentHeal, new ProcChainMask());
            EffectManager.SpawnEffect(JellyBagHooks.HitEffect.prefab, new EffectData
            {
                rootObject = targetBody.gameObject,
                rotation = Quaternion.identity
            }, true);
        }
    }
}