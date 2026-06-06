using System.Collections.Concurrent;
using System.Threading.Channels;
using ImageResizer.Models;

namespace ImageResizer.Services;

// - manages per-user job queues - sequential per user, parallel across users
public class UserQueueManager
{
    private readonly ConcurrentDictionary<string, Channel<ResizeJob>> _queues = new();

    // - get or create a bounded channel for the given session
    public Channel<ResizeJob> GetOrCreateQueue(string sessionId)
    {
        return _queues.GetOrAdd(
            sessionId,
            _ =>
                Channel.CreateBounded<ResizeJob>(
                    new BoundedChannelOptions(50)
                    {
                        FullMode = BoundedChannelFullMode.Wait,
                        SingleReader = true,
                        SingleWriter = false,
                    }
                )
        );
    }

    // - enqueue a job to the user's channel
    public async Task EnqueueAsync(string sessionId, ResizeJob job)
    {
        var channel = GetOrCreateQueue(sessionId);
        await channel.Writer.WriteAsync(job);
    }

    // - returns all active session channels for the background worker to process
    public IEnumerable<KeyValuePair<string, Channel<ResizeJob>>> GetAllQueues()
    {
        return _queues;
    }

    // - remove queue when session is cleaned up
    public void RemoveQueue(string sessionId)
    {
        if (_queues.TryRemove(sessionId, out var channel))
        {
            channel.Writer.TryComplete();
        }
    }
}
