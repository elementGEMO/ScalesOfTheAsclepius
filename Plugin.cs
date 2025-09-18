using BepInEx;
using R2API;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;
using RoR2.ExpansionManagement;
using R2API.Networking;
using ShaderSwapper;
using RoR2;

[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace ScalesAsclepius
{
    [BepInDependency(ItemAPI.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(PrefabAPI.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(LanguageAPI.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(NetworkingAPI.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(RecalculateStatsAPI.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]

    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class SotAPlugin : BaseUnityPlugin
    {
        public const string PluginGUID      = PluginCreator + "." + PluginName;
        public const string PluginCreator   = "noodleGemo";
        public const string PluginName      = "ScalesoftheAsclepius";
        public const string PluginVersion   = "1.0.0";

        public static SotAPlugin Instance   { get; private set; }
        public static AssetBundle Bundle    { get; private set; }

        public static readonly string TokenPrefix = "GEMO_SOTA_";

        public static ExpansionDef ScalesAsclepiusExp;
        public static ItemRelationshipProvider VoidRelationship;

        public void Awake()
        {
            Instance = this;
            SetUpAssets();

            PluginConfig.Init();
            if (PluginConfig.Enable_Logging.Value) Log.Init(Logger);

            CreateContent();
        }
        private void CreateContent()
        {
            CreateExpansion();
            CreateBuffs();
            CreateItems();
            CreateVoidRelationship();
        }
        private void CreateExpansion()
        {
            string expansionName = "Scales of the Asclepius";
            string expansionDescription = "Adds content from the 'Scales of the Asclepius' expansion to the game.";

            ScalesAsclepiusExp = ScriptableObject.CreateInstance<ExpansionDef>();
            ScalesAsclepiusExp.name = "scaleAsclepiusIcon";

            ScalesAsclepiusExp.iconSprite = Bundle.LoadAsset<Sprite>("expansionIcon");
            ScalesAsclepiusExp.disabledIconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texUnlockIcon.png").WaitForCompletion();

            ScalesAsclepiusExp.nameToken = SALanguage.LanguageAdd("EXPANSION_ICON", expansionName);
            ScalesAsclepiusExp.descriptionToken = SALanguage.LanguageAdd("EXPANSION_DESC", expansionDescription);

            ContentAddition.AddExpansionDef(ScalesAsclepiusExp);
        }
        private void CreateVoidRelationship()
        {
            VoidRelationship = ScriptableObject.CreateInstance<ItemRelationshipProvider>();
            VoidRelationship.name = "SotAVoidItemProvider";

            VoidRelationship.relationshipType = Addressables.LoadAssetAsync<ItemRelationshipType>("RoR2/DLC1/Common/ContagiousItem.asset").WaitForCompletion();
            VoidRelationship.relationships = [
                new ItemDef.Pair
                {
                    itemDef1 = GauzePadItem.ItemDef,
                    itemDef2 = BubbleWrapItem.ItemDef
                }
            ];

            ContentAddition.AddItemRelationshipProvider(VoidRelationship);
        }
        private void CreateBuffs()
        {
            new HealFromDebuff();
        }
        private void CreateItems()
        {
            // Common Tier Items
            new GauzePadItem();
            new GauzePadHooks();

            // Uncommon Tier Items
            new IVBagItem();
            new IVBagHooks();

            // Void Common Tier Items
            new BubbleWrapItem();
            new BubbleWrapHooks();

            // Void Uncommon Tier Items
        }

        private void SetUpAssets()
        {
            Bundle = AssetBundle.LoadFromFile(System.IO.Path.Combine(Directory.GetParent(Info.Location)!.ToString(), "scalesoftheasclepius"));
            StartCoroutine(Bundle.UpgradeStubbedShadersAsync());
        }
    }
}
