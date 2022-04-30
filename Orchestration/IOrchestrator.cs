namespace Offers.Orchestration;

public interface IOrchestrator
{
    // task for performing the whole saga
    Task Orchestrate();
}