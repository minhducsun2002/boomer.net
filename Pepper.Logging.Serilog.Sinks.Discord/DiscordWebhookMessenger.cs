using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Net.Rest;
using Discord.Rest;
using Discord.Webhook;

namespace Pepper.Logging.Serilog.Sinks.Discord
{
    /// <summary>
    /// Queuing and sending textual message with Discord Webhook.
    /// </summary>
    public class DiscordWebhookMessenger : IDisposable, IAsyncDisposable
    {

        private readonly Task workerTask;
        private readonly ConcurrentQueue<string> impendingMessages = new();
        private readonly SemaphoreSlim impendingMessagesSemaphore = new(0);
        private readonly CancellationTokenSource shutdownCts = new();
        private readonly CancellationToken shutdownToken;
        private TimeSpan requestThrottleTime = TimeSpan.FromSeconds(.5);
        private int maxMessagesPerPack = 100;

        /// <param name="id">Discord webhook ID.</param>
        /// <param name="token">Discord webhook token.</param>
        /// <exception cref="ArgumentNullException"><paramref name="token"/> is <c>null</c>.</exception>
        public DiscordWebhookMessenger(ulong id, string token)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            workerTask = WorkerAsync(config => new DiscordWebhookClient(id, token, config), shutdownCts.Token);
            shutdownToken = shutdownCts.Token;
        }

        /// <param name="webhookEndpointUrl">Discord webhook endpoint URL.</param>
        /// <exception cref="ArgumentNullException"><paramref name="webhookEndpointUrl"/> is <c>null</c>.</exception>
        public DiscordWebhookMessenger(string webhookEndpointUrl)
        {
            if (webhookEndpointUrl == null)
            {
                throw new ArgumentNullException(nameof(webhookEndpointUrl));
            }

            workerTask = WorkerAsync(config => new DiscordWebhookClient(webhookEndpointUrl, config), shutdownCts.Token);
        }

        public int MaxMessagesPerPack
        {
            get => maxMessagesPerPack;
            set
            {
                if (maxMessagesPerPack < 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Value should not be less than 1.");
                }

                maxMessagesPerPack = value;
            }
        }

        public TimeSpan RequestThrottleTime
        {
            get => requestThrottleTime;
            set
            {
                if (requestThrottleTime <= TimeSpan.Zero)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Value should be non-negative.");
                }

                requestThrottleTime = value;
            }
        }

        /// <inheritdoc cref="PushMessage(string?)"/>
        public void PushMessage(string message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            // Propagate worker's exceptions, if any.
            if (workerTask.IsFaulted)
            {
                workerTask.GetAwaiter().GetResult();
            }

            impendingMessages.Enqueue(message);
            try
            {
                impendingMessagesSemaphore.Release();
            }
            catch (ObjectDisposedException)
            {
                // In case impendingMessagesSemaphore has been disposed.
            }
        }

        // Long-running worker thread.
        private async Task WorkerAsync(Func<DiscordRestConfig, DiscordWebhookClient> clientFactory, CancellationToken ct)
        {
            var config = new DiscordRestConfig { RestClientProvider = DefaultRestClientProvider.Create(true) };
#if DEBUG
            config.LogLevel = LogSeverity.Info;
#endif
            using var client = clientFactory(config);
#if DEBUG
            client.Log += log =>
            {
                Debug.WriteLine(log.ToString());
                return Task.CompletedTask;
            };
#endif
            var messageBuffer = new List<string>();
            do
            {
                try
                {
                    await Task.Delay(requestThrottleTime, ct);
                }
                catch (OperationCanceledException)
                {
                    // cancelled from Delay
                }
                try
                {
                    // Take 1
                    // Consider the case where ct is cancelled and we are draining the queue.
                    if (!impendingMessagesSemaphore.Wait(0))
                    {
                        await impendingMessagesSemaphore.WaitAsync(ct);
                    }
                }
                catch (OperationCanceledException)
                {
                    // cancelled from WaitAsync
                }

                int currentBufferCount = 0;
                for (int i = 0; i < maxMessagesPerPack; i++)
                {
                    // Consume 1
                    var result = impendingMessages.TryPeek(out var message);
                    Debug.Assert(result);
                    if (result && message!.Length + currentBufferCount + 2 >= 1950)
                    {
                        break;
                    }
                    if (result && message!.Length + currentBufferCount + 2 <= 1949)
                    {
                        impendingMessages.TryDequeue(out message);
                        currentBufferCount += message!.Length;
                        messageBuffer.Add("`" + message.TrimEnd() + "`");
                    }
                    // Take another
                    if (!impendingMessagesSemaphore.Wait(0))
                    {
                        break;
                    }
                }
                await client.SendMessageAsync(string.Join("\n", messageBuffer));
                messageBuffer.Clear();
            } while (!ct.IsCancellationRequested || impendingMessagesSemaphore.CurrentCount > 0);
        }

        /// <summary>
        /// Wait until the queued messages has been drained, and shutdown the worker task.
        /// </summary>
        /// <returns>The task that completes when the worker has ended, and throws if there is error in the worker task.</returns>
        public Task ShutdownAsync()
        {
            shutdownCts.Cancel();
            return workerTask;
        }

        /// <inheritdoc />
        /// <remarks>You need to call <see cref="ShutdownAsync"/> before disposing the instance, to ensure all the logs has been reliably sent to the server.</remarks>
        public void Dispose()
        {
            shutdownCts.Cancel();
            if (workerTask.IsCompleted)
            // or we will get InvalidOperationException.
            {
                workerTask.Dispose();
            }

            impendingMessagesSemaphore.Dispose();
            shutdownCts.Dispose();
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            if (shutdownToken.IsCancellationRequested)
            {
                return;
            }

            shutdownCts.Cancel();
            impendingMessagesSemaphore.Dispose();
            shutdownCts.Dispose();
            try
            {
                await workerTask;
            }
            catch (Exception)
            {
                // We are not throwing in Dispose method.
            }
        }
    }
}