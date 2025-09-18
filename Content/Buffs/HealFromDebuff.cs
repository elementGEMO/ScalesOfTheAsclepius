using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ScalesAsclepius;
public class HealFromDebuff : BuffBase
{
    protected override string Name => "HealFromDebuff";
    public static BuffDef BuffDef;

    protected override Sprite IconSprite => Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Medkit/texBuffMedkitHealIcon.tif").WaitForCompletion();
    protected override Color Color => new(1f, 0.922f, 0.886f);
    protected override bool IsStackable => false;

    protected override void Initialize() => BuffDef = Value;
}