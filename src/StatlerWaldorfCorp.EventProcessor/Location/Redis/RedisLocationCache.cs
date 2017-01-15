using System;
using System.Collections.Generic;

namespace StatlerWaldorfCorp.EventProcessor.Location.Redis
{
    public class RedisLocationCache : ILocationCache
    {
        public ICollection<MemberLocation> GetMemberLocations(Guid teamId)
        {
            throw new NotImplementedException();
        }

        public void Put(Guid teamId, MemberLocation memberLocation)
        {
            throw new NotImplementedException();
        }
    }
}