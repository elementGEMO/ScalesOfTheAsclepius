using BepInEx.Configuration;
using Rewired.UI.ControlMapper;
using RoR2;
using UnityEngine;

namespace ScalesAsclepius;
public static class PluginConfig
{
    public static ConfigEntry<bool> Enable_Logging;
    public static ConfigEntry<bool> Set_Default;
    //public static ConfigEntry<RewriteOptions> Rework_Name;
    public static ConfigEntry<int> Round_To;

    /*
    public enum RewriteOptions
    {
        Relic,
        Cursed
    }
    */

    public static void Init()
    {
        GeneralInit();
    }

    private static void GeneralInit()
    {
        string token = "! General !";
        Enable_Logging = SotAPlugin.Instance.Config.Bind(
            token, "Enable Logs", true,
            "[ True = Enables Logging | False = Disables Logging ]\nDisclaimer: Makes debugging harder when disabled"
        );
        Set_Default = SotAPlugin.Instance.Config.Bind(
            token, "Default Configs", true,
            "[ True = Sets item configs to default (Except Item Enabled) | False = Configs can be changed ]\nUseful for when Default Values get updated"
        );
        /*
        Rework_Name = SotAPlugin.Instance.Config.Bind(
            token, "Relic Names", RewriteOptions.Relic,
            "[ Changes the naming conventions of Lunars | Does not effect 'Disables ...' ]"
        );
        */
        /*
        CooldownHooks.Cooldown_Minimum = LoEPlugin.Instance.Config.Bind(
            token, "Cooldown Minimum", 0f,
            "[ 0 = 0s Minimum | Cooldown Reduction Value, Vanilla is 0.5s ]"
        );
        */
        Round_To = SotAPlugin.Instance.Config.Bind(
            token, "Item Stat Rounding", 0,
            "[ 0 = Whole | 1 = Tenths | 2 = Hundrenths | 3 = ... ]\nRounds item values to respective decimal point"
        );

        //SotAPlugin.Instance.Config.Count
    }
    public enum MathProcess
    {
        Max,
        Min
    };

    public static ConfigEntry<T> PostConfig<T>(this ConfigEntry<T> config)
    {
        if (Set_Default.Value) config.BoxedValue = config.DefaultValue;

        return config;
    }

    public static ConfigEntry<float> PostConfig(this ConfigEntry<float> config, MathProcess capType, float capNum)
    {
        config = config.PostConfig();
        
        if (capType == MathProcess.Max) config.BoxedValue = Mathf.Max((float)config.BoxedValue, capNum);
        else config.BoxedValue = Mathf.Min((float)config.BoxedValue, capNum);

        return config;
    }

    public static ConfigEntry<int> PostConfig(this ConfigEntry<int> config, MathProcess capType, int capNum)
    {
        config = config.PostConfig();

        if (capType == MathProcess.Max) config.BoxedValue = Mathf.Max((int)config.BoxedValue, capNum);
        else config.BoxedValue = Mathf.Min((int)config.BoxedValue, capNum);

        return config;
    }

    /*
    public static ConfigEntry<float> PostConfig(this ConfigEntry<float> config)
    {
        if (Set_Default.Value) config.BoxedValue = config.DefaultValue;

        return config;
    }
    public static ConfigEntry<int> PostConfig(this ConfigEntry<int> config)
    {
        if (Set_Default.Value) config.BoxedValue = config.DefaultValue;

        return config;
    }
    public static ConfigEntry<bool> PostConfig(this ConfigEntry<bool> config)
    {
        if (Set_Default.Value) config.BoxedValue = config.DefaultValue;

        return config;
    }
    */

    /*
    public static void ResetConfig()
    {
        foreach (ConfigEntryBase entry in SotAPlugin.Instance.Config.GetConfigEntries())
        {
            entry.BoxedValue = entry.DefaultValue;
        }
    }
    */
}