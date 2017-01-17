using Moq;
using Xunit;
using StackExchange.Redis;
using Microsoft.Extensions.Logging;
using StatlerWaldorfCorp.EventProcessor.Location.Redis;
using StatlerWaldorfCorp.EventProcessor.Location;
using System;
using System.Collections.Generic;

namespace StatlerWaldorfCorp.EventProcessor.Tests.Location.Redis
{
    public class RedisLocationCacheTests
    {
        [Fact]
        public void PutCallsHashSet_TeamAsHashKey_MemberAsKey()
        {
            var multiplexer = new Mock<IConnectionMultiplexer>();
            var logger = new Mock<ILogger<RedisLocationCache>>();
            var db = new Mock<IDatabase>();
            var teamId = Guid.NewGuid();            

            MemberLocation location = new MemberLocation {
                MemberID = Guid.NewGuid(),
                Location = new GpsCoordinate {
                    Latitude = 10.0,
                    Longitude = 20.0
                }
            };

            RedisKey key = teamId.ToString();
            RedisValue v1 = location.MemberID.ToString();
            RedisValue v2 = location.ToJsonString();

            db.Setup( d => d.HashSet(It.Is<RedisKey>( k => k == key),
                                     It.Is<RedisValue>( v => v == v1 ),
                                     It.Is<RedisValue>( v => v == v2),
                                     When.Always, CommandFlags.PreferMaster));
            
            multiplexer.Setup( m => m.GetDatabase(It.IsAny<int>(), null) ).Returns(db.Object);
            var cache = new RedisLocationCache(logger.Object, multiplexer.Object);        
            
            cache.Put(teamId, location);
            multiplexer.VerifyAll();
            db.VerifyAll();
        }

         [Fact]
        public void GetMembers_CallsHashValues_And_Converts()
        {
            var multiplexer = new Mock<IConnectionMultiplexer>();
            var logger = new Mock<ILogger<RedisLocationCache>>();
            var db = new Mock<IDatabase>();
            MemberLocation ml1 = new MemberLocation {
                MemberID = Guid.NewGuid(),
                Location = new GpsCoordinate {
                    Latitude = 10.0,
                    Longitude = 30.0
                }
            };
            MemberLocation ml2 = new MemberLocation {
                MemberID = Guid.NewGuid(),
                Location = new GpsCoordinate {
                    Latitude = 10.0,
                    Longitude = 40.0
                }
            };
            MemberLocation ml3 = new MemberLocation {
                MemberID = Guid.NewGuid(),
                Location = new GpsCoordinate {
                    Latitude = 10.0,
                    Longitude = 50.0
                }
            };
            var teamId = Guid.NewGuid();
            RedisValue[] hashValues = new RedisValue[] {
                ml1.ToJsonString(), ml2.ToJsonString(), ml3.ToJsonString()
            };

            multiplexer.Setup( m => m.GetDatabase(It.IsAny<int>(), null) ).Returns(db.Object);
            db.Setup( d => d.HashValues(It.Is<RedisKey>( r => r== teamId.ToString()), CommandFlags.None)).Returns(hashValues);
            
            var cache = new RedisLocationCache(logger.Object, multiplexer.Object);


            IList<MemberLocation> members = cache.GetMemberLocations(teamId);
            
            multiplexer.VerifyAll();
            db.VerifyAll();

            Assert.Equal(3, members.Count);

        }
    }   
}