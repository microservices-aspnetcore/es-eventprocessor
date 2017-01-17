# Event Processor
In an event sourcing model, an event processor service often has no restful endpoints. It is responsible for handling one or more inbound streams of events, performing some processing as a result of those events, and recording the events in the event store. The processing of inbound events usually results in the emission of other outbound events.

In the case of the team management application used in the ES/CQRS chapter for the **Microservices with ASP.NET** book, this event processor reacts to incoming **MemberLocationRecorded** events. Each one of these events is recorded in the event store, and the location is also submitted to the _reality_ server (the service that exposes the efficient queries for the CQRS pattern). _After_ this happens, the real event processing happens.

## Processing Location Events
Every **MemberLocationRecorded** event contains information including GPS coordinates, timestamps, as well as other meta-data that might include the source of the location report. The coordinates of the report are compared against the _current_ coordinates of all other team members. If the event processor determines that the location of an event is _near_ the location of another team member, it will emit a **ProximityDetectedEvent** event on a different stream.

The application suite is then free to respond to **ProximityDetectedEvent** events however it chooses. In a real-world application, this might involve sending push notifications to mobile devices, sending websocket-style notifications through a third-party message broker to a reactive web application, etc.

