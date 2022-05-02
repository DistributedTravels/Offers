using Models;

namespace Offers.Handlers;

public abstract class IHandler
{
    protected Action<EventModel> publish; // to allow for publishing within handler
    protected Func<EventModel, Task<string>> call;
    protected readonly WebApplication app; // required for database calls

    public IHandler(Action<EventModel> publish, Func<EventModel, Task<string>> call, WebApplication app)
    {
        this.publish = publish;
        this.call = call;
        this.app = app;
    }

    /**
         * <summary>
         * Method HandleEvent processes data transferred within event message and acts upon it.
         * </summary>
         */
    public abstract Task HandleEvent(string content);

    /**
         * <summary>
         * Method that uses EventManager's Publish to send "reply" events from within Handler.
         * </summary>
         */
    private void PublishEvent(EventModel @event)
    {
        this.publish(@event);
    }
    
}