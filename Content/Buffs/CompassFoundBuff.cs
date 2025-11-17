using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using RoR2BepInExPack.GameAssetPathsBetter;

namespace ScalesAsclepius;
public class CompassFoundBuff : BuffBase
{
    protected override string Name => "CompassFoundBuff";
    public static BuffDef BuffDef;

    protected override Sprite IconSprite => SotAPlugin.Bundle.LoadAsset<Sprite>("texCompassBuff");
    protected override Color Color => new(1f, 0, 0);

    protected override bool IsHidden => false;

    protected override void Initialize() => BuffDef = Value;
}