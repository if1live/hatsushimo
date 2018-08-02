using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Optional;

namespace Shigure
{
    // 큐의 최대크기를 제한한 concurrent queue
    public class SizedConcurrentQueue<T>
    {
        readonly ConcurrentQueue<T> queue = new ConcurrentQueue<T>();
        readonly int maxSize;

        public SizedConcurrentQueue(int maxSize)
        {
            this.maxSize = maxSize;
        }

        public void Enqueue(T p)
        {
            queue.Enqueue(p);

            while (queue.Count > maxSize)
            {
                T _;
                queue.TryDequeue(out _);
            }
        }

        public bool TryDequeue(out T p)
        {
            return queue.TryDequeue(out p);
        }

        // 꺼낼수 있을때까지 대기
        public async Task<Option<T>> Dequeue(int tryCount, TimeSpan delay)
        {
            for (int i = 0; i < tryCount; i++)
            {
                T packet;
                if (TryDequeue(out packet))
                {
                    return packet.Some();
                }
                await Task.Delay(delay);
            }
            return Option.None<T>();
        }

        // 주의: clear 도중 큐에 추가되면 해당 요소는 무시될수 있다
        public void Clear()
        {
            while (queue.IsEmpty == false)
            {
                T _;
                TryDequeue(out _);
            }
        }
    }
}
