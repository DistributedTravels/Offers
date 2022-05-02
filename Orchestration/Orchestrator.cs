using Models;

namespace Offers.Orchestration;

public abstract class Orchestrator<T>
{
    // task for performing the whole saga
    protected Action<EventModel> publish;
    protected Func<EventModel, Task<string>> call;

    public Orchestrator(Action<EventModel> publish, Func<EventModel, Task<string>> call)
    {
        this.publish = publish;
        this.call = call;
    }
    public abstract Task<T> Orchestrate(EventModel @event);
}