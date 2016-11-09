using Xunit;
using System;
using System.Collections.Generic;
using StatlerWaldorfCorp.EventProcessor.Location;
using StatlerWaldorfCorp.EventProcessor.Events;

namespace StatlerWaldorfCorp.EventProcessor.Tests
{
    public class ProximityDetectorTest
    {
        [Fact]
        public void EventProcessorDetectsTeamMemberProximities()
        {
            List<MemberLocation> laMetro = new List<MemberLocation>();
            laMetro.Add(new MemberLocation() {
                MemberID = Guid.NewGuid(),
                Location = GpsUtilityTest.LosAngeles
            });
            laMetro.Add(new MemberLocation() {
                MemberID = Guid.NewGuid(),
                Location = GpsUtilityTest.BeverlyHills
            });

            ProximityDetector detector = new ProximityDetector();
            MemberLocationRecordedEvent memberLocationEvent =
                new MemberLocationRecordedEvent() {
                    Origin = "Test harness",
                    Latitude = GpsUtilityTest.LosAngeles.Latitude+0.001,
                    Longitude = GpsUtilityTest.LosAngeles.Longitude-0.001,
                    MemberID = Guid.NewGuid(),
                    RecordedTime = DateTime.UtcNow.Ticks,
                    ReportID = Guid.NewGuid(),
                    TeamID = Guid.NewGuid()
                };
            var detections = new List<ProximityDetectedEvent>( 
                    detector.DetectProximityEvents( memberLocationEvent,
                        laMetro,
                        50.0)); // 50km range

            Assert.True(detections.Count == 2);
            Assert.True(detections[0].MemberDistance < 50.0);
            Assert.True(detections[1].MemberDistance < 50.0);                                
        }

        [Fact]
        public void EventProcessorReturnsNoEventsForEmptyTeam()
        {
            List<MemberLocation> noLocations = new List<MemberLocation>();

            ProximityDetector detector = new ProximityDetector();
            MemberLocationRecordedEvent memberLocationEvent =
                new MemberLocationRecordedEvent() {
                    Origin = "Test harness",
                    Latitude = GpsUtilityTest.LosAngeles.Latitude+0.001,
                    Longitude = GpsUtilityTest.LosAngeles.Longitude-0.001,
                    MemberID = Guid.NewGuid(),
                    RecordedTime = DateTime.UtcNow.Ticks,
                    ReportID = Guid.NewGuid(),
                    TeamID = Guid.NewGuid()
                };
            var detections = new List<ProximityDetectedEvent>( 
                    detector.DetectProximityEvents( memberLocationEvent,
                        noLocations,
                        50.0)); // 50km
            Assert.Equal(0, detections.Count);
        }

        [Fact]
        public void EventProcessorReturnsNoEventsForOutsideDistanceThreshold()
        {
            List<MemberLocation> wideTeam = new List<MemberLocation>();
            wideTeam.Add(new MemberLocation() {
                MemberID = Guid.NewGuid(),
                Location = GpsUtilityTest.NewYorkCity
            });
            wideTeam.Add(new MemberLocation() {
                MemberID = Guid.NewGuid(),
                Location = GpsUtilityTest.LosAngeles
            });
            ProximityDetector detector = new ProximityDetector();
            MemberLocationRecordedEvent memberEvent = new MemberLocationRecordedEvent() {
                Origin = "test harness",
                Latitude = GpsUtilityTest.KansasCity.Latitude,
                Longitude = GpsUtilityTest.KansasCity.Longitude,
                MemberID = Guid.NewGuid(),
                RecordedTime = DateTime.UtcNow.Ticks,
                ReportID = Guid.NewGuid(),
                TeamID = Guid.NewGuid()
            };
            var detections = new List<ProximityDetectedEvent>(
                detector.DetectProximityEvents(memberEvent, wideTeam, 50.0)
            ); // 50km range            
            Assert.Equal(0, detections.Count);

            // Cast a wider net, should proximity detect with both LA and NYC. 
            var ultraWideDetections = new List<ProximityDetectedEvent>(
                detector.DetectProximityEvents(memberEvent, wideTeam, Int32.MaxValue)
            );
            Assert.Equal(2, ultraWideDetections.Count);
        }
    }
}