using RoR2;

namespace ScalesAsclepius;
public class TetherHealBuff : BuffBase
{
    protected override string Name => "TetherHealBuff";
    public static BuffDef BuffDef;

    protected override bool IsStackable => true;
    protected override bool IsHidden => true;

    protected override void Initialize() => BuffDef = Value;
}