using EventSourcingTaskApp.Core.Framework;
using EventStore.ClientAPI;
using System.Text;
using System.Text.Json;

namespace EventSourcingTaskApp.Infrastructure
{
    public class AggregateRepository
    {
        private readonly IEventStoreConnection _eventStore;

        public AggregateRepository(IEventStoreConnection eventStore)
        {
            _eventStore = eventStore;
        }

        public async Task SaveAsync<T>(T aggregate) where T : Aggregate, new()
        {
            var events = aggregate.GetChanges()     //we take the events on aggregate and map them to the EventData class. Event Store stores events in the EventData type.
                .Select(@event => new EventData(
                    Guid.NewGuid(),
                    @event.GetType().Name,
                    true,
                    Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event)),
                    Encoding.UTF8.GetBytes(@event.GetType().FullName)))
                .ToArray();

            if (!events.Any())
            {
                return;
            }

            var streamName = GetStreamName(aggregate, aggregate.Id);  //we set the stream name. Event aggregates in the Event Store is called stream. So aggregate is expressed as a stream in Event Store.

            var result = await _eventStore.AppendToStreamAsync(streamName, ExpectedVersion.Any, events);  //events are recorded in the Event Store.
        }

        public async Task<T> LoadAsync<T>(Guid aggregateId) where T : Aggregate, new()
        {
            if (aggregateId == Guid.Empty)
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(aggregateId));

            var aggregate = new T();
            var streamName = GetStreamName(aggregate, aggregateId);  //we set the stream name.

            var nextPageStart = 0L;

            do
            {
                var page = await _eventStore.ReadStreamEventsForwardAsync(   //events are received from the Event Store in order according to the version numbers in the loop.
                    streamName, nextPageStart, 4096, false);

                if (page.Events.Length > 0)
                {
                    aggregate.Load(   //the load method of aggregate is called and the events are applied to aggregate and the final form of aggregate is created.
                        page.Events.Last().Event.EventNumber,
                        page.Events.Select(@event => JsonSerializer.Deserialize(Encoding.UTF8.GetString(@event.OriginalEvent.Data), Type.GetType(Encoding.UTF8.GetString(@event.OriginalEvent.Metadata)))
                        ).ToArray());
                }

                nextPageStart = !page.IsEndOfStream ? page.NextEventNumber : -1;
            } while (nextPageStart != -1);

            return aggregate;
        }

        private string GetStreamName<T>(T type, Guid aggregateId) => $"{type.GetType().Name}-{aggregateId}";
    }
}
