/// <summary>
/// State class for use in a <see cref="StateController"/>.
/// </summary>
public class State
{
    /// <summary>
    /// Called once when transitioning into this state.
    /// </summary>
    public virtual void Enter()
    {
    }

    /// <summary>
    /// Called every fixed framerate frame while in this state.
    /// </summary>
    public virtual void FixedUpdate()
    {
    }

    /// <summary>
    /// Called once when transitioning out of this state.
    /// </summary>
    public virtual void Exit()
    {
    }
}