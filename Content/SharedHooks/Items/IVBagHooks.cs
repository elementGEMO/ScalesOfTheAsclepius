using RoR2;
using R2API;
using System;
using UnityEngine;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;
using RoR2.Items;
using R2API.Networking.Interfaces;
using R2API.Networking;

namespace ScalesAsclepius;
public class IVBagHooks
{
    //private static readonly string InternalName = "IVBagHooks";
    public static bool ItemEnabled;
    public static GameObject TetherPrefab;

    public IVBagHooks()
    {
        ItemEnabled = IVBagItem.Item_Enabled.Value;

        if (ItemEnabled)
        {
            CreateTether();

            NetworkingAPI.RegisterMessageType<IVBagTether.IVTetherSync>();
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            IL.RoR2.HealthComponent.Heal += HealthComponent_Heal1;
        }
    }

    private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
    {
        if (sender && sender.HasBuff(TetherArmorBuff.BuffDef)) args.armorAdd = IVBagItem.Flat_Armor.Value;
        /*
        if (sender?.inventory && NetworkServer.active)
        {
            IVBagTether bagTether = sender.GetComponent<IVBagTether>();
            int itemCount = sender?.inventory ? sender.inventory.GetItemCount(IVBagItem.ItemDef) : 0;
            bool activeTether = bagTether?.ActiveTether;

            if (bagTether && itemCount > 0)
            {
                if (bagTether.Duration <= 0 && activeTether && bagTether.ActiveTether.active) args.armorAdd = IVBagItem.Flat_Armor.Value;
            }
        }
        */
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
                bool canActivate            = nonRegen && !procChainMask.HasProc(ProcType.Missile);
                CharacterBody body          = self.GetComponent<CharacterBody>();
                IVBagTether bagBehaviour    = body?.GetComponent<IVBagTether>();
                int itemCount               = body?.inventory ? body.inventory.GetItemCount(IVBagItem.ItemDef) : 0;

                if (bagBehaviour?.ActiveTether && canActivate)
                {
                    CharacterBody targetAlly    = bagBehaviour.TargetLink;
                    HealthComponent healthAlly  = targetAlly?.GetComponent<HealthComponent>();
                    ProcChainMask newProcMask   = new(); newProcMask.AddProc(ProcType.Missile);

                    if (healthAlly)
                    {
                        float itemScale     = IVBagItem.Heal_Percent.Value * IVBagItem.Item_Scale.Value * (itemCount - 1);
                        float totalPercent  = amount * (IVBagItem.Heal_Percent.Value + itemScale);

                        healthAlly.Heal(totalPercent, newProcMask);
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

    private void CreateTether()
    {
        TetherPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/EliteEarth/AffixEarthTetherVFX.prefab").WaitForCompletion().InstantiateClone("IVBagTether", true);

        LineRenderer disableLine    = TetherPrefab.GetComponent<LineRenderer>();
        Material disableMat         = new (disableLine.sharedMaterial);

        disableMat.SetColor("_TintColor", new Color(0, 0, 0));
        disableLine.SetSharedMaterials([disableMat], 1);

        UnityEngine.Object.Destroy(TetherPrefab.transform.Find("EndTransform/HealedFX/HealingSymbols_Ps").gameObject);
        UnityEngine.Object.Destroy(TetherPrefab.transform.Find("EndTransform/HealedFX/HealingGlow_Ps").gameObject);

        ParticleSystemRenderer healSpot = TetherPrefab.transform.Find("EndTransform/HealedFX/HealingGlow_Ps (1)").GetComponent<ParticleSystemRenderer>();
        Material healMat                = new(healSpot.sharedMaterial);

        healMat.SetColor("_TintColor", new Color(0, 0, 0));
        healSpot.sharedMaterial = healMat;
    }
}

public class IVBagTether : BaseItemBodyBehavior
{
    public CharacterBody TargetLink;
    public GameObject ActiveTether;

    private CharacterBody Owner;
    private MaterialPropertyBlock PropertySet;
    private TetherVfxOrigin TetherEffect;

    private ParticleSystemRenderer CurrentHeal;
    private LineRenderer CurrentTether;

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
    }
    public void SetUpTether(TetherVfx tether, Transform transform)
    {
        ActiveTether = tether.gameObject;
        CurrentTether = ActiveTether.GetComponent<LineRenderer>();
        CurrentHeal = ActiveTether.transform.Find("EndTransform/HealedFX/HealingGlow_Ps (1)")?.GetComponent<ParticleSystemRenderer>();
    }
    public void OnDestroy()
    {
        TetherEffect.onTetherAdded -= SetUpTether;
        Destroy(TetherEffect);
    }
    public void Update()
    {
        Duration = Math.Max(0f, Duration - Time.deltaTime);

        CharacterBody closestAlly = null;
        var allyListReadOnly = TeamComponent.GetTeamMembers(Owner.teamComponent.teamIndex);
        float deltaAlly = float.MaxValue;

        foreach (TeamComponent foundTeam in allyListReadOnly)
        {
            CharacterBody ally = foundTeam.body;
            if (ally == Owner) continue;

            float delta = (Owner.transform.position - ally.transform.position).sqrMagnitude;
            if (delta <= Math.Pow(IVBagItem.Radius.Value, 2) && delta < deltaAlly)
            {
                closestAlly = ally;
                deltaAlly = delta;
            }
        }

        TargetLink = closestAlly;
    }
    public void LateUpdate()
    {
        if (TargetLink)
        {
            TetherEffect.SetTetheredTransforms([TargetLink.transform]);

            if (NetworkServer.active)
            {
                Owner.AddTimedBuff(TetherArmorBuff.BuffDef, 0.05f);
                TargetLink.AddTimedBuff(TetherArmorBuff.BuffDef, 0.05f);
            }
        }

        if (ActiveTether) ActiveTether.SetActive(TargetLink);

        float colorLerp = Mathf.MoveTowards(GlowHue, Duration > 0f ? 2f : 0f, Time.deltaTime * 15f);
        Color setHue = new(GlowHue, GlowHue, GlowHue);

        GlowHue = colorLerp;
        PropertySet.SetColor("_TintColor", setHue);

        if (CurrentTether) CurrentTether.GetComponent<Renderer>().SetPropertyBlock(PropertySet);
        if (CurrentHeal) CurrentHeal.GetComponent<Renderer>().SetPropertyBlock(PropertySet);
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