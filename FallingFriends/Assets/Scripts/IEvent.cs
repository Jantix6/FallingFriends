public interface IEvent
{
    void Tick();
    void Activate();
    void ActivateFeedback();
    string GetEventName();
}