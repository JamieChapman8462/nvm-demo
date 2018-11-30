using System.Collections.Generic;

namespace InteractionPlanApi.Request
{
    public class NccoQueue
    {
        public IDictionary<string, NccoQueueEntry> Queues { get; } = new Dictionary<string, NccoQueueEntry>();

        public void AddToQueue(string uuid, NCCO.NCCO ncco)
        {
            var queue = GetQueueEntry(uuid);
            queue.NccoQueue.Add(ncco);
        }

        public void Clear(string uuid)
        {
            Queues.Remove(uuid);
        }

        private NccoQueueEntry GetQueueEntry(string uuid)
        {
            if (Queues.TryGetValue(uuid, out var entry))
            {
                return entry;
            }

            var newEntry = new NccoQueueEntry
            {
                NccoQueue = new List<NCCO.NCCO>()
            };

            Queues.Add(uuid, newEntry);
            return newEntry;
        }

        public IList<NCCO.NCCO> GetQueue(string uuid)
        {
            return GetQueueEntry(uuid).NccoQueue;
        }

        public bool IsCurrentlyExecuting(string uuid)
        {
            return GetQueueEntry(uuid).CurrentlyExecuting;
        }

        public void SetCurrentlyExecuting(string uuid, bool value)
        {
            GetQueueEntry(uuid).CurrentlyExecuting = value;
        }
    }

    public class NccoQueueEntry
    {
        public bool CurrentlyExecuting { get; set; }
        public IList<NCCO.NCCO> NccoQueue { get; set; }
    }
}