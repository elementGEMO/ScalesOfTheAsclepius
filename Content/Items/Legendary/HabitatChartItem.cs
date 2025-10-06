using RoR2;
using R2API;
using UnityEngine;
using BepInEx.Configuration;

namespace ScalesAsclepius;
using static SAColors;
using static SAUtils;
using static SAREnderHelpers;
public class HabitatChartItem : ItemBase
{
    public static ConfigEntry<bool> Item_Enabled;

    public static ConfigEntry<float> Health_Gain;
    public static ConfigEntry<float> Health_Gain_Stack;
    public static ConfigEntry<float> Health_Max;
    public static ConfigEntry<float> Health_Max_Stack;

    protected override string Name => "HabitatChart";
    public static ItemDef ItemDef;
    protected override CombinedItemTier Tier => ItemTier.Tier3;
    protected override ItemTag[] Tags => [ItemTag.Healing];
    protected override bool IsRemovable => true;

    protected override string DisplayName => "Habitat Chart";
    protected override string Description => FuseText([
        string.Format("Increase " + "health permanently ".Style(FontColor.cIsHealing) + "at the " + "start of each stage ".Style(FontColor.cIsUtility) + "by " + "{0}".Style(FontColor.cIsHealing) + " ({1} per stack)".Style(FontColor.cStack).OptText(Health_Gain_Stack.Value != 0) + ", ",
        RoundVal(Health_Gain.Value), RoundVal(Health_Gain_Stack.Value).SignVal()),

        string.Format("up to a " + "maximum ".Style(FontColor.cIsHealing) + "of " + "{0} ".Style(FontColor.cIsHealing) + "({1}% per stack) ".Style(FontColor.cStack).OptText(Health_Max_Stack.Value != 0) + "health".Style(FontColor.cIsHealing) + ".",
        RoundVal(Health_Max.Value), RoundVal(Health_Max_Stack.Value).SignVal())
    ]);

    protected override bool IsEnabled()
    {
        Health_Gain = SotAPlugin.Instance.Config.Bind(
            DisplayName + " - Item",
            "Health Amount", 125f,
            "[ 125 = +125 | Permanent Health Amount ]"
        );
        Health_Gain_Stack = SotAPlugin.Instance.Config.Bind(
            DisplayName + " - Item",
            "Health Amount Stack", 0f,
            "[ 25 = +25 | Permanent Health Amount per Item Stack | 0 to Disable ]"
        );

        Health_Max = SotAPlugin.Instance.Config.Bind(
            DisplayName + " - Item",
            "Max Health", 1500f,
            "[ 1500 = 1500 | Max Health Capacity ]"
        );
        Health_Max_Stack = SotAPlugin.Instance.Config.Bind(
            DisplayName + " - Item",
            "Max Health Stack", 50f,
            "[ 50 = +50% | Max Health Capacity per Item Stack | 0 to Disable ]"
        );

        Health_Gain.Value = Mathf.Max(Health_Gain.Value, 0);
        Health_Gain_Stack.Value = Mathf.Max(Health_Gain_Stack.Value, 0);

        Health_Max.Value = Mathf.Max(Health_Max.Value, 0);
        Health_Max_Stack.Value = Mathf.Max(Health_Max_Stack.Value, 0);

        Item_Enabled = SotAPlugin.Instance.Config.Bind(
            DisplayName + " - Item", "Enable Item", true,
            "[ True = Enabled | False = Disabled ]"
        );

        return Item_Enabled.Value;
    }

    protected override void Initialize()
    {
        ItemDef = Value;
        ItemDef.requiredExpansion = SotAPlugin.ScalesAsclepiusExp;
    }
}