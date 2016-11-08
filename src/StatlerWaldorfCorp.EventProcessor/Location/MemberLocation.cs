using System;

namespace StatlerWaldorfCorp.EventProcessor.Location
{
    public class MemberLocation
    {
        public Guid MemberID { get; set; }
        public GpsCoordinate Location { get; set; }

    }
}