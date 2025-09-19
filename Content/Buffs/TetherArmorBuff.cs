using RoR2;
using UnityEngine;

namespace ScalesAsclepius;
public class TetherArmorBuff : BuffBase
{
    protected override string Name => "TetherArmorBuff";
    public static BuffDef BuffDef;

    protected override Sprite IconSprite => SotAPlugin.Bundle.LoadAsset<Sprite>("texTetherArmor");
    protected override Color Color => new(0.725f, 0.941f, 0.424f);
    protected override bool IsStackable => false;

    protected override void Initialize() => BuffDef = Value;
}