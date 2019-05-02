using Newtonsoft.Json;
using Proto.Persistence;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UserManagement.Persistence
{

    public sealed class InMemoryProvider : IProvider
    {
        private readonly ConcurrentDictionary<string, Dictionary<long, object>> _globalEvents = new ConcurrentDictionary<string, Dictionary<long, object>>();

        private readonly ConcurrentDictionary<string, Dictionary<long, object>> _globalSnapshots = new ConcurrentDictionary<string, Dictionary<long, object>>();

        public Task<(object Snapshot, long Index)> GetSnapshotAsync(string actorName)
        {
            if (!_globalSnapshots.TryGetValue(actorName, out var snapshots))
                return Task.FromResult<(object, long)>((null, 0));

            var snapshot = snapshots.OrderBy(ss => ss.Key).LastOrDefault();
            return Task.FromResult((snapshot.Value, snapshot.Key));
        }

        public Task<long> GetEventsAsync(string actorName, long indexStart, long indexEnd, Action<object> callback)
        {
            if (_globalEvents.TryGetValue(actorName, out var events))
            {
                foreach (var e in events.Where(e => e.Key >= indexStart && e.Key <= indexEnd))
                {
                    callback(e.Value);
                }
            }

            return Task.FromResult(0L);
        }

        public Task<long> PersistEventAsync(string actorName, long index, object @event)
        {
            var events = _globalEvents.GetOrAdd(actorName, new Dictionary<long, object>());

            long nextEventIndex = 1;
            if (events.Any())
            {
                nextEventIndex = events.Last().Key + 1;
            }

            events.Add(nextEventIndex, @event);

            return Task.FromResult(0L);
        }

        public Task PersistSnapshotAsync(string actorName, long index, object snapshot)
        {
            var type = snapshot.GetType();
            var snapshots = _globalSnapshots.GetOrAdd(actorName, new Dictionary<long, object>());
            var copy = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(snapshot), type);

            snapshots.Add(index, copy);

            return Task.CompletedTask;
        }

        public Task DeleteEventsAsync(string actorName, long inclusiveToIndex)
        {
            if (!_globalEvents.TryGetValue(actorName, out var events))
                return Task.CompletedTask;

            var eventsToRemove = events.Where(s => s.Key <= inclusiveToIndex)
                .Select(e => e.Key)
                .ToList();

            eventsToRemove.ForEach(key => events.Remove(key));

            return Task.CompletedTask;
        }

        public Task DeleteSnapshotsAsync(string actorName, long inclusiveToIndex)
        {
            if (!_globalSnapshots.TryGetValue(actorName, out var snapshots))
                return Task.CompletedTask;

            var snapshotsToRemove = snapshots.Where(s => s.Key <= inclusiveToIndex)
                .Select(snapshot => snapshot.Key)
                .ToList();

            snapshotsToRemove.ForEach(key => snapshots.Remove(key));

            return Task.CompletedTask;
        }
    }
}
