using RoR2;
using RoR2.Items;
using R2API;
using System;
using UnityEngine;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;
using R2API.Networking;
using R2API.Networking.Interfaces;
using System.Collections.Generic;

namespace ScalesAsclepius;
public class IVBagHooks
{
    //private static readonly string InternalName = "IVBagHooks";
    public static bool ItemEnabled;
    public static GameObject TetherPrefab;
    public static ModdedProcType HealShare;

    public IVBagHooks()
    {
        ItemEnabled = IVBagItem.Item_Enabled.Value;

        if (ItemEnabled)
        {
            CreateTether();

            NetworkingAPI.RegisterMessageType<IVBagTether.IVTetherSync>();
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            IL.RoR2.HealthComponent.Heal += HealthComponent_Heal1;
            HealShare = ProcTypeAPI.ReserveProcType();
        }
    }

    private void CreateTether()
    {
        TetherPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/EliteEarth/AffixEarthTetherVFX.prefab").WaitForCompletion().InstantiateClone("IVBagTether", true);

        LineRenderer disableLine = TetherPrefab.GetComponent<LineRenderer>();
        Material disableMat = new(disableLine.sharedMaterial);

        disableMat.SetColor("_TintColor", new Color(0, 0, 0));
        disableLine.SetSharedMaterials([disableMat], 1);

        UnityEngine.Object.Destroy(TetherPrefab.transform.Find("EndTransform/HealedFX/HealingSymbols_Ps").gameObject);
        UnityEngine.Object.Destroy(TetherPrefab.transform.Find("EndTransform/HealedFX/HealingGlow_Ps").gameObject);
        UnityEngine.Object.Destroy(TetherPrefab.GetComponent<LoopSoundPlayer>());

        ParticleSystemRenderer healSpot = TetherPrefab.transform.Find("EndTransform/HealedFX/HealingGlow_Ps (1)").GetComponent<ParticleSystemRenderer>();
        Material healMat = new(healSpot.sharedMaterial);

        healMat.SetColor("_TintColor", new Color(0, 0, 0));
        healSpot.sharedMaterial = healMat;
    }

    private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
    {
        if (sender && sender.HasBuff(TetherArmorBuff.BuffDef))
        {
            float armorAmount = IVBagItem.Flat_Armor.Value;

            if (IVBagItem.Flat_Armor_Stack.Value > 0)
            {
                int buffCount = sender.GetBuffCount(TetherArmorBuff.BuffDef);
                float buffScale = IVBagItem.Flat_Armor_Stack.Value * (buffCount - 1);

                armorAmount += buffScale;
            }

            args.armorAdd += armorAmount;
        }
    }

    private void HealthComponent_Heal1(ILContext il)
    {
        ILCursor cursor = new (il);

        if (cursor.TryGotoNext(
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<HealthComponent>(nameof(HealthComponent.health)),
            x => x.MatchLdloc(0),
            x => x.MatchSub(),
            x => x.MatchRet()
        ))
        {
            cursor.Emit(OpCodes.Ldarg, 0);
            cursor.Emit(OpCodes.Ldarg, 1);
            cursor.Emit(OpCodes.Ldarg, 2);
            cursor.Emit(OpCodes.Ldarg, 3);

            cursor.EmitDelegate<Action<HealthComponent, float, ProcChainMask, bool>>((self, amount, procChainMask, nonRegen) =>
            {
                CharacterBody body          = self.GetComponent<CharacterBody>();
                IVBagTether bagComponent    = body?.GetComponent<IVBagTether>();
                int itemCount               = body?.inventory ? body.inventory.GetItemCount(IVBagItem.ItemDef) : 0;
                bool inProcChain            = procChainMask.HasModdedProc(HealShare);

                if (amount > 0.5f && bagComponent?.TargetLinks.Count > 0 && !inProcChain)
                {
                    foreach (Transform target in bagComponent.TargetLinks)
                    {
                        HealthComponent ally = target.GetComponent<HealthComponent>();

                        if (!ally) continue;

                        ProcChainMask healMask  = new(); healMask.AddModdedProc(HealShare);
                        float healPercent       = IVBagItem.Heal_Percent.Value;

                        if (IVBagItem.Heal_Percent_Stack.Value > 0)
                        {
                            float itemScale = IVBagItem.Heal_Percent_Stack.Value * (itemCount - 1);
                            healPercent += itemScale;
                        }

                        ally.Heal(amount * healPercent / 100f, healMask);
                        new IVBagTether.IVTetherSync(body.netId).Send(NetworkDestination.Clients);
                    }
                }
            });
        }
    }

    private void CharacterBody_OnInventoryChanged(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
    {
        orig(self);

        if (self?.inventory && NetworkServer.active)
        {
            IVBagTether bagTether   = self.GetComponent<IVBagTether>();
            int itemCount           = self.inventory.GetItemCount(IVBagItem.ItemDef);

            if (bagTether && itemCount <= 0) GameObject.Destroy(bagTether);
            if (!bagTether && itemCount > 0) self.gameObject.AddComponent<IVBagTether>();
        }
    }
}

public class IVBagTether : BaseItemBodyBehavior
{
    public List<Transform> TargetLinks;
    public List<GameObject> ActiveTethers;
    private List<Renderer> TetherRenders;

    private CharacterBody Owner;
    private MaterialPropertyBlock PropertySet;
    private TetherVfxOrigin TetherEffect;

    public float Duration;
    private float GlowHue;

    [ItemDefAssociation(useOnServer = true, useOnClient = true)]
    public static ItemDef GetItemDef() => (IVBagItem.Item_Enabled.Value) ? IVBagItem.ItemDef : null;
    public void OnEnable()
    {
        if (!IVBagItem.Item_Enabled.Value) return;

        Owner = GetComponent<CharacterBody>();
        TetherEffect = gameObject.AddComponent<TetherVfxOrigin>();
        TetherEffect.tetherPrefab = IVBagHooks.TetherPrefab;
        Duration = 0f;
        GlowHue = 0f;

        PropertySet = new();
        TetherEffect.onTetherAdded += SetUpTether;
        TetherEffect.onTetherRemoved += RemoveTether;

        TargetLinks = [];
        ActiveTethers = [];
        TetherRenders = [];
    }
    public void SetUpTether(TetherVfx tether, Transform transform)
    {
        GameObject newTether = tether.gameObject;
        Renderer lineRender = newTether.GetComponent<LineRenderer>().GetComponent<Renderer>();
        Renderer particleRender = newTether.transform.Find("EndTransform/HealedFX/HealingGlow_Ps (1)")?.GetComponent<ParticleSystemRenderer>().GetComponent<Renderer>();

        ActiveTethers.Add(newTether);
        TetherRenders.Add(lineRender);
        TetherRenders.Add(particleRender);
    }
    public void RemoveTether(TetherVfx tether)
    {
        GameObject oldTether = tether.gameObject;
        Renderer lineRender = oldTether.GetComponent<LineRenderer>().GetComponent<Renderer>();
        Renderer particleRender = oldTether.transform.Find("EndTransform/HealedFX/HealingGlow_Ps (1)")?.GetComponent<ParticleSystemRenderer>().GetComponent<Renderer>();

        if (ActiveTethers.Contains(oldTether)) ActiveTethers.Remove(oldTether);
        if (TetherRenders.Contains(lineRender)) TetherRenders.Remove(lineRender);
        if (TetherRenders.Contains(particleRender)) TetherRenders.Remove(particleRender);
    }
    public void OnDestroy()
    {
        TetherEffect.onTetherAdded -= SetUpTether;
        TetherEffect.onTetherRemoved -= RemoveTether;
        Destroy(TetherEffect);
    }
    public void Update()
    {
        Duration = Math.Max(0f, Duration - Time.deltaTime);
        TargetLinks.Clear();

        List<HurtBox> hurtBoxList   = HG.CollectionPool<HurtBox, List<HurtBox>>.RentCollection();
        TeamMask allyMask           = TeamMask.none; allyMask.AddTeam(Owner.teamComponent.teamIndex);
        int itemCount               = Owner.inventory.GetItemCount(IVBagItem.ItemDef);
        float radius                = IVBagItem.Radius.Value;

        if (RunArtifactManager.instance.IsArtifactEnabled(RoR2Content.Artifacts.FriendlyFire))
        {
            allyMask = TeamMask.GetEnemyTeams(Owner.teamComponent.teamIndex);
            allyMask.AddTeam(Owner.teamComponent.teamIndex);
        }

        if (IVBagItem.Radius_Stack.Value > 0)
        {
            float itemScale = IVBagItem.Radius_Stack.Value * (itemCount - 1);
            radius += itemScale;
        }

        SphereSearch radiusSearch = new()
        {
            radius = radius,
            origin = Owner.transform.position,
            mask = LayerIndex.entityPrecise.mask,
            queryTriggerInteraction = QueryTriggerInteraction.UseGlobal
        };

        radiusSearch.RefreshCandidates();
        radiusSearch.FilterCandidatesByHurtBoxTeam(allyMask);
        radiusSearch.OrderCandidatesByDistance();
        radiusSearch.FilterCandidatesByDistinctHurtBoxEntities();
        radiusSearch.GetHurtBoxes(hurtBoxList);

        int targetLimit = IVBagItem.Target_Count.Value;

        if (IVBagItem.Target_Count_Stack.Value > 0)
        {
            int itemScale = IVBagItem.Target_Count_Stack.Value * (itemCount - 1);
            targetLimit += itemScale;
        }

        int currentIndex    = 0;
        int limit           = Math.Min(targetLimit, hurtBoxList.Count);

        while (TargetLinks.Count < limit && currentIndex < hurtBoxList.Count)
        {
            CharacterBody currentAlly = hurtBoxList[currentIndex]?.healthComponent.body;
            if (currentAlly != Owner) TargetLinks.Add(currentAlly.transform);
            currentIndex++;
        }

        if (NetworkServer.active)
        {
            if (TargetLinks.Count <= 0 && Owner.GetBuffCount(TetherArmorBuff.BuffDef) != itemCount) Owner.SetBuffCount(TetherArmorBuff.BuffDef.buffIndex, itemCount);
            else if (TargetLinks.Count > 0 && Owner.HasBuff(TetherArmorBuff.BuffDef)) Owner.SetBuffCount(TetherArmorBuff.BuffDef.buffIndex, 0);
        }
    }
    public void LateUpdate()
    {
        TetherEffect.SetTetheredTransforms(TargetLinks);

        float colorLerp = Mathf.MoveTowards(GlowHue, Duration > 0f ? 2f : 0f, Time.deltaTime * 15f);
        Color setHue = new(GlowHue, GlowHue, GlowHue);

        GlowHue = colorLerp;
        PropertySet.SetColor("_TintColor", setHue);

        foreach (Renderer render in TetherRenders) render.SetPropertyBlock(PropertySet);
    }
    public void TriggerVisual()
    {
        Duration = 0.5f;
        Util.PlaySound("Play_treeBot_m1_hit_heal", Owner.gameObject);
    }
    public class IVTetherSync : INetMessage
    {
        NetworkInstanceId NetID;
        public IVTetherSync() { }
        public IVTetherSync(NetworkInstanceId setID) => NetID = setID;
        public void Deserialize(NetworkReader reader) => NetID = reader.ReadNetworkId();

        public void OnReceived()
        {
            GameObject bodyObject       = Util.FindNetworkObject(NetID);
            IVBagTether tetherComponent = bodyObject?.GetComponent<IVBagTether>();

            if (tetherComponent) tetherComponent.TriggerVisual();
        }

        public void Serialize(NetworkWriter writer) => writer.Write(NetID);
    }
}