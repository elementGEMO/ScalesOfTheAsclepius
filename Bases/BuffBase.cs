using R2API;
using RoR2;
using UnityEngine;

namespace ScalesAsclepius;
public abstract class BuffBase : GenericBase<BuffDef>
{
    protected virtual Color Color => Color.white;

    protected virtual Sprite IconSprite => null;

    protected virtual bool IsCooldown   => false;
    protected virtual bool IsDebuff     => false;
    protected virtual bool IsHidden     => false;
    protected virtual bool IsStackable  => false;

    protected override void Create()
    {
        Value = ScriptableObject.CreateInstance<BuffDef>();
        Value.name = string.Format("bd{0}", Name);

        Value.buffColor = Color;
        Value.iconSprite = IconSprite;

        Value.isCooldown = IsCooldown;
        Value.isDebuff = IsDebuff;
        Value.isHidden = IsHidden;
        Value.canStack = IsStackable;

        ContentAddition.AddBuffDef(Value);
    }
}