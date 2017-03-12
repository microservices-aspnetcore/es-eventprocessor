using Xunit;
using System.IO;
using StatlerWaldorfCorp.EventProcessor;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Hosting;
using StatlerWaldorfCorp.EventProcessor.Events;
using System.Collections.Generic;
using System;
using StatlerWaldorfCorp.EventProcessor.Location;
using RabbitMQ.Client;
using System.Threading;
using StatlerWaldorfCorp.EventProcessor.Queues;
using System.Text;
using Microsoft.Extensions.Options;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;

namespace StatlerWaldorfCorp.EventProcessor.Tests.Integration
{
    public class EventProcessingRoundTripTest
    {
        private readonly TestServer testServer;

        private Guid teamId1;
        private Guid teamId2;

        private IConnectionFactory connectionFactory;

        private static AutoResetEvent autoEvent = new AutoResetEvent(false);

        private QueueOptions queueOptions;

        public EventProcessingRoundTripTest()
        {
            Console.WriteLine("Starting from directory: " + Directory.GetCurrentDirectory());
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .Build();

            testServer = new TestServer(new WebHostBuilder()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseConfiguration(config)
                .UseStartup<Startup>());

            connectionFactory = (IConnectionFactory)testServer.Host.Services.GetService(typeof(IConnectionFactory));
            var opts = (IOptions<QueueOptions>)testServer.Host.Services.GetService(typeof(IOptions<QueueOptions>));
            queueOptions = opts.Value;

            teamId1 = Guid.NewGuid();
            teamId2 = Guid.NewGuid();
        }


        [Fact]
        public void EventProcessingRoundTrip()
        {
            MemberLocationRecordedEvent[] events =
                    GenerateFakeEventList();

            SubscribeToProximityEvents();
            EnqueueEvents(events);

            bool receivedSignal = autoEvent.WaitOne(4000);
            Assert.True(receivedSignal);

            // Check with Redis to make sure we got all the locations.
            ILocationCache locationCache = (ILocationCache)testServer.Host.Services.GetService(typeof(ILocationCache));
            IList<MemberLocation> team2Locations = locationCache.GetMemberLocations(teamId2);
            Assert.Equal(2, team2Locations.Count);
            IList<MemberLocation> team1Locations = locationCache.GetMemberLocations(teamId1);
            Assert.Equal(2, team1Locations.Count);

            channel.Close();
            connection.Close();
            Environment.Exit(0);
        }

        private IConnection connection;
        private IModel channel;

        private void SubscribeToProximityEvents()
        {
            connection = connectionFactory.CreateConnection();
            channel = connection.CreateModel();

            channel.QueueDeclare(
                queue: queueOptions.ProximityDetectedEventQueueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (ch, ea) =>
            {
                var body = ea.Body;
                var msg = Encoding.UTF8.GetString(body);
                var evt = JsonConvert.DeserializeObject<ProximityDetectedEvent>(msg);

                channel.BasicAck(ea.DeliveryTag, false);
                if (evt.SourceMemberID == sourceMemberId)
                {
                    autoEvent.Set(); // only flip if the proximity event is for the right member.
                }
            };
            string consumerTag = channel.BasicConsume(queueOptions.ProximityDetectedEventQueueName, false, consumer);
        }

        private void EnqueueEvents(MemberLocationRecordedEvent[] events)
        {
            using (IConnection conn = connectionFactory.CreateConnection())
            {
                using (IModel channel = conn.CreateModel())
                {
                    channel.QueueDeclare(
                        queue: queueOptions.MemberLocationRecordedEventQueueName,
                        durable: false,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null
                    );
                    foreach (var evt in events)
                    {
                        string jsonPayload = evt.toJson();
                        var body = Encoding.UTF8.GetBytes(jsonPayload);

                        channel.BasicPublish(
                            exchange: "",
                            routingKey: queueOptions.MemberLocationRecordedEventQueueName,
                            basicProperties: null,
                            body: body
                        );
                    }
                }
            }
        }

        private Guid sourceMemberId;

        private MemberLocationRecordedEvent[] GenerateFakeEventList()
        {
            // Put 2 people far away that won't cause a proximity event.
            // Put 1 person in QueensNY
            // Put another in Manhattan (this should trigger the proximity event)

            List<MemberLocationRecordedEvent> events = new List<MemberLocationRecordedEvent>();
            events.Add(FakeEvent(teamId1, Guid.NewGuid(), LosAngeles));
            events.Add(FakeEvent(teamId1, Guid.NewGuid(), KansasCity));
            events.Add(FakeEvent(teamId2, Guid.NewGuid(), QueensNY));

            sourceMemberId = Guid.NewGuid();
            // when the processor hits this event, we should fire a proximity detection
            events.Add(FakeEvent(teamId2, sourceMemberId, NewYorkCity));

            return events.ToArray();
        }

        private MemberLocationRecordedEvent FakeEvent(Guid teamId, Guid memberId, GpsCoordinate location)
        {
            return new MemberLocationRecordedEvent
            {
                Origin = "IntegrationTest",
                Latitude = location.Latitude,
                Longitude = location.Longitude,
                MemberID = memberId,
                TeamID = teamId,
                ReportID = Guid.NewGuid(),
                RecordedTime = DateTime.UtcNow.Ticks
            };
        }

        public static GpsCoordinate LosAngeles = new GpsCoordinate()
        {
            Latitude = 34.0522222,
            Longitude = -118.2427778
        };

        public static GpsCoordinate NewYorkCity = new GpsCoordinate()
        {
            Latitude = 40.7141667,
            Longitude = -74.0063889
        };

        public static GpsCoordinate QueensNY = new GpsCoordinate()
        {
            Latitude = 40.7282,
            Longitude = -73.7949
        };

        public static GpsCoordinate KansasCity = new GpsCoordinate()
        {
            Latitude = 39.0997,
            Longitude = -94.5786
        };
    }
}