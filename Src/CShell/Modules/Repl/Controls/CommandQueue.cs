using System.Collections.Concurrent;
using System.Collections.Generic;

namespace CShell.Modules.Repl.Controls
{
    public class CommandQueue<T>
    {
        private readonly int _limit;
        readonly ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();

        public int Count => _queue.Count;

        public T this[int i] => _queue.ToArray()[i];

        public T[] ToArray() => _queue.ToArray();

        public CommandQueue(int limit)
        {
            _limit = limit;
        }

        public void Add(T item)
        {
            _queue.Enqueue(item);

            lock (this) //ensure threads don't step on each other
            {
                T overflow;
                while (_queue.Count > _limit && _queue.TryDequeue(out overflow)) ;
            }
            
        }

        public IEnumerable<T> Contents() => _queue;
    }
}