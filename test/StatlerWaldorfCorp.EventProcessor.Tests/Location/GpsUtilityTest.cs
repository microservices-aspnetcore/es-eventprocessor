using System;
using StatlerWaldorfCorp.EventProcessor.Location;
using Xunit;

namespace StatlerWaldorfCorp.EventProcessor.Tests.Location
{
    public class GpsUtilityTest
    {
        [Fact]
        public void ProducesAccurateDistanceMeasurements()
        {
            GpsUtility gpsUtility = new GpsUtility();    

            double distance = gpsUtility.DistanceBetweenPoints(LosAngeles, NewYorkCity);
            Assert.Equal(3933, Math.Round(distance)); // 3,933 km
            Assert.Equal(0, gpsUtility.DistanceBetweenPoints(LosAngeles, LosAngeles));            
        }

        public static GpsCoordinate LosAngeles = new GpsCoordinate() {
                Latitude = 34.0522222,
                Longitude = -118.2427778
            };

        public static GpsCoordinate NewYorkCity = new GpsCoordinate() {
                Latitude = 40.7141667,
                Longitude = -74.0063889
            };

        public static GpsCoordinate QueensNY = new GpsCoordinate() {
            Latitude = 40.7282,
            Longitude = -73.7949
        };

        public static GpsCoordinate BeverlyHills = new GpsCoordinate() {
            Latitude = 34.0736,
            Longitude = -118.4004
        };

        public static GpsCoordinate KansasCity = new GpsCoordinate() {
            Latitude = 39.0997,
            Longitude = -94.5786
        };
    }
}