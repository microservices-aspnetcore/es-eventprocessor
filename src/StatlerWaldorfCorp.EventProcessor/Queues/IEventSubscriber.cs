using StatlerWaldorfCorp.EventProcessor.Events;

namespace StatlerWaldorfCorp.EventProcessor.Queues
{
    public interface IEventSubscriber
    {
        void Subscribe();
        void Unsubscribe();

        void OnLocationRecordedEvent(MemberLocationRecordedEvent memberLocationRecordedEvent);
    }
}