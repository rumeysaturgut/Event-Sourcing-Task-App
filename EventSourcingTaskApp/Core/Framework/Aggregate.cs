namespace EventSourcingTaskApp.Core.Framework
{
    public abstract class Aggregate
    {
        readonly IList<object> _changes = new List<object>(); //we define the variable where aggregate events will be stored.

        public Guid Id { get; protected set; } = Guid.Empty;  // we state that aggregate will have an Id.
        public long Version { get; private set; } = -1;  //we state that the default version of aggregate is "-1".

        protected abstract void When(object @event);

        public void Apply(object @event)  //we write the method that will add events to the variable defined on the line 5
        {
            When(@event);

            _changes.Add(@event);
        }

        public void Load(long version, IEnumerable<object> history)  //we write the method that will apply the events to aggregate. The final version of aggregate will be created by running this method for each event read from the Event Store.
        {
            Version = version;

            foreach (var e in history)
            {
                When(e);
            }
        }

        public object[] GetChanges() => _changes.ToArray();  //we write the method that returns the events on aggregate. While sending events to the Event Store, this method will be run and events will be received.
    }
}
