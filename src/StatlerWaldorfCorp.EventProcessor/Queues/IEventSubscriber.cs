using StatlerWaldorfCorp.EventProcessor.Events;

namespace StatlerWaldorfCorp.EventProcessor.Queues
{
    public delegate void MemberLocationRecordedEventReceivedDelegate(MemberLocationRecordedEvent evt);

    public interface IEventSubscriber
    {
        void Subscribe();
        void Unsubscribe();

        //void OnLocationRecordedEvent(MemberLocationRecordedEvent memberLocationRecordedEvent);
        event MemberLocationRecordedEventReceivedDelegate MemberLocationRecordedEventReceived;
    }
}