using System;
using System.Collections.Generic;

namespace StatlerWaldorfCorp.EventProcessor.Location
{
    public interface ILocationCache
    {
        ICollection<MemberLocation> GetMemberLocations(Guid teamId);

        void Put(Guid teamId, MemberLocation memberLocation);
    }
}