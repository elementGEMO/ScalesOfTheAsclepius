using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using RoR2BepInExPack.GameAssetPathsBetter;

namespace ScalesAsclepius;
public class JellyCooldownBuff : BuffBase
{
    protected override string Name => "JellyCooldownBuff";
    public static BuffDef BuffDef;

    protected override Sprite IconSprite => Addressables.LoadAssetAsync<Sprite>(RoR2_Base_Medkit.texBuffMedkitHealIcon_tif).WaitForCompletion();
    //protected override Color Color => new(1f, 0.922f, 0.886f);
    protected override bool IsStackable => true;
    protected override bool IsCooldown => true;
    protected override bool IsHidden => false;

    protected override void Initialize() => BuffDef = Value;
}