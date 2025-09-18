using BepInEx.Configuration;

namespace ScalesAsclepius;
public static class PluginConfig
{

    public static ConfigEntry<bool> Enable_Logging;
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
    }
}