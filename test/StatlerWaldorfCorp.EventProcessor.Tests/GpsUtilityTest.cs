using System;
using StatlerWaldorfCorp.EventProcessor.Location;
using Xunit;

namespace StatlerWaldorfCorp.EventProcessor.Tests
{
    public class GpsUtilityTest
    {
        [Fact]
        public void ProducesAccurateDistanceMeasurements()
        {
            GpsUtility gpsUtility = new GpsUtility();

            GpsCoordinate losAngeles = new GpsCoordinate() {
                Latitude = 34.0522222,
                Longitude = -118.2427778
            };

            GpsCoordinate newYorkCity = new GpsCoordinate() {
                Latitude = 40.7141667,
                Longitude = -74.0063889
            };

            double distance = gpsUtility.DistanceBetweenPoints(losAngeles, newYorkCity);
            Assert.Equal(3933, Math.Round(distance)); // 3,933 km
            Assert.Equal(0, gpsUtility.DistanceBetweenPoints(losAngeles, losAngeles));            
        }
    }
}