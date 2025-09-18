namespace ScalesAsclepius;
public abstract class GenericBase<T> where T : UnityEngine.Object
{
    protected abstract string Name { get; }
    protected T Value;

    public GenericBase()
    {
        if (IsEnabled())
        {
            Create();
            Initialize();
        }
    }

    protected virtual bool IsEnabled() => true;
    protected virtual void Create() { }
    protected virtual void Initialize() { }

    public T Get() => Value;
}