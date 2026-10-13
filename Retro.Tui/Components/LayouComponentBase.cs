namespace Retro.Tui.Components;

public abstract class LayoutComponentBase
{
    protected bool ContinueRunning = true;

    protected virtual void ResetDefaults()
    {
        ContinueRunning = true;
    }
}

public abstract class LayoutComponent
    : LayoutComponentBase
{
    public abstract Task RunAsync();
}

public abstract class LayoutComponent<TRequest>
    : LayoutComponentBase
{
    public abstract Task RunAsync(TRequest input);
}

public abstract class LayoutComponent<TRequest, TResult>
    : LayoutComponentBase
{
    public abstract Task<TResult> RunAsync(TRequest input);
}