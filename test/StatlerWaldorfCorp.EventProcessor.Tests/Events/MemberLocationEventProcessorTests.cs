using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using StatlerWaldorfCorp.EventProcessor.Events;
using StatlerWaldorfCorp.EventProcessor.Queues;
using System;
using StatlerWaldorfCorp.EventProcessor.Tests.Location;
using StatlerWaldorfCorp.EventProcessor.Location;
using System.Collections.Generic;

namespace StatlerWaldorfCorp.EventProcessor.Tests.Events
{
    public class MemberLocationEventProcessorTests
    {
        private Guid memberId1 = Guid.NewGuid();
        private Guid memberId2 = Guid.NewGuid();

        private Guid teamID = Guid.NewGuid();

        [Fact]
        public void TestSimpleProximityStreamEmitsAProximityEvent()
        {
            var logger = new Mock<ILogger<MemberLocationEventProcessor>>();
            var subscriber = new FakeSubscriber();
            var emitter = new Mock<IEventEmitter>(); 
            var cache = new FakeLocationCache();           

            ProximityDetectedEvent savedEvent = null;

            emitter.Setup( e => e.EmitProximityDetectedEvent(It.IsAny<ProximityDetectedEvent>())).
                Callback<ProximityDetectedEvent>( (obj) => savedEvent = obj );
            
            var eventProcessor = new MemberLocationEventProcessor( logger.Object, subscriber, emitter.Object, cache );

            var evt1 = new MemberLocationRecordedEvent() {
                Origin = "Test",
                Latitude = GpsUtilityTest.QueensNY.Latitude,
                Longitude = GpsUtilityTest.QueensNY.Longitude,
                MemberID = memberId1,
                TeamID = teamID,
                RecordedTime = DateTime.UtcNow.Ticks,
                ReportID = Guid.NewGuid()
            };
            var evt2 = new MemberLocationRecordedEvent() {
                Origin = "Test",
                Latitude = GpsUtilityTest.NewYorkCity.Latitude,
                Longitude = GpsUtilityTest.NewYorkCity.Longitude,
                MemberID = memberId2,
                TeamID = teamID,
                RecordedTime = DateTime.UtcNow.Ticks,
                ReportID = Guid.NewGuid()
            };

            eventProcessor.Start();
            subscriber.TriggerEvent(evt1);
            subscriber.TriggerEvent(evt2);
            eventProcessor.Stop();

            // During a detection, the "source" member is the one on the event, and the "target"
            // is the one found in the member location collections that is within the distance threshold.
            emitter.Verify( e => e.EmitProximityDetectedEvent(It.IsAny<ProximityDetectedEvent>()), Times.Once);
            Assert.Equal(GpsUtilityTest.QueensNY.Latitude, savedEvent.TargetMemberLocation.Latitude);
            Assert.Equal(GpsUtilityTest.QueensNY.Longitude, savedEvent.TargetMemberLocation.Longitude);
            Assert.Equal(memberId2, savedEvent.SourceMemberID);
            Assert.Equal(memberId1, savedEvent.TargetMemberID);            
            Assert.True(savedEvent.MemberDistance <= 30); // hard-coded threshold is 30 at the moment.            
        }
    }

    class FakeSubscriber : IEventSubscriber
    {
        public event MemberLocationRecordedEventReceivedDelegate MemberLocationRecordedEventReceived;

        public void Subscribe()
        {
            
        }

        public void Unsubscribe()
        {
            
        }

        public void TriggerEvent(MemberLocationRecordedEvent evt) {
            MemberLocationRecordedEventReceived(evt);
        }
    }

    class FakeLocationCache : ILocationCache
    {
        private Dictionary<Guid, List<MemberLocation>> internalStorage;

        public FakeLocationCache() {
            internalStorage = new Dictionary<Guid, List<MemberLocation>>();
        }

        public IList<MemberLocation> GetMemberLocations(Guid teamId)
        {
            if (!internalStorage.ContainsKey(teamId)) {
                internalStorage[teamId] = new List<MemberLocation>();
            }
            return internalStorage[teamId];
        }

        public void Put(Guid teamId, MemberLocation memberLocation)
        {
            if (!internalStorage.ContainsKey(teamId)) {
                internalStorage[teamId] = new List<MemberLocation>();
            }
            internalStorage[teamId].Add(memberLocation);
        }
    }
}