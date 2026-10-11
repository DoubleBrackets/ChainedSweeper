using System;

public interface IGameInput
{
    public Action OnReload { get; }
    public Action<int> OnHitSweepNode { get; }
}