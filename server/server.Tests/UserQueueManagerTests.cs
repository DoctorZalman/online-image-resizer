using ImageResizer.Models;
using ImageResizer.Services;

namespace ImageResizer.Tests;

public class UserQueueManagerTests
{
    private readonly UserQueueManager _manager = new();

    [Fact]
    public async Task EnqueueAsync_CreatesQueueForSession()
    {
        var job = new ResizeJob { SessionId = "session-1" };

        await _manager.EnqueueAsync("session-1", job);

        var queues = _manager.GetAllQueues().ToList();
        Assert.Single(queues);
        Assert.Equal("session-1", queues[0].Key);
    }

    [Fact]
    public async Task EnqueueAsync_SeparateQueuesPerSession()
    {
        var job1 = new ResizeJob { SessionId = "session-1" };
        var job2 = new ResizeJob { SessionId = "session-2" };

        await _manager.EnqueueAsync("session-1", job1);
        await _manager.EnqueueAsync("session-2", job2);

        var queues = _manager.GetAllQueues().ToList();
        // - each session gets its own independent queue
        Assert.Equal(2, queues.Count);
    }

    [Fact]
    public async Task GetOrCreateQueue_ReturnsSameQueueForSameSession()
    {
        var queue1 = _manager.GetOrCreateQueue("session-1");
        var queue2 = _manager.GetOrCreateQueue("session-1");

        // - same session always gets the same channel instance
        Assert.Same(queue1, queue2);

        await Task.CompletedTask;
    }

    [Fact]
    public async Task RemoveQueue_CompletesChannel()
    {
        var job = new ResizeJob { SessionId = "session-1" };
        await _manager.EnqueueAsync("session-1", job);

        _manager.RemoveQueue("session-1");

        var queues = _manager.GetAllQueues().ToList();
        Assert.Empty(queues);
    }

    [Fact]
    public async Task EnqueueAsync_MultipleJobsSameSession_PreservesOrder()
    {
        var results = new List<string>();
        var job1 = new ResizeJob { JobId = "job-1", SessionId = "session-1" };
        var job2 = new ResizeJob { JobId = "job-2", SessionId = "session-1" };
        var job3 = new ResizeJob { JobId = "job-3", SessionId = "session-1" };

        await _manager.EnqueueAsync("session-1", job1);
        await _manager.EnqueueAsync("session-1", job2);
        await _manager.EnqueueAsync("session-1", job3);

        var channel = _manager.GetOrCreateQueue("session-1");

        // - drain queue and verify FIFO order
        while (channel.Reader.TryRead(out var job))
            results.Add(job.JobId);

        Assert.Equal(["job-1", "job-2", "job-3"], results);
    }
}
