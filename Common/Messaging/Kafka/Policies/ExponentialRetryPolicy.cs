using Common.Messaging.Kafka.Interfaces;
using System;
using System.Threading;

namespace Common.Messaging.Kafka.Policies
{
    public class ExponentialRetryPolicy: IRetryPolicy
    {
        private readonly int _maxRetries;
        private readonly int _baseDelayMs;

        public ExponentialRetryPolicy(int maxRetries = 3, int baseDelayMs = 500)
        {
            _maxRetries = maxRetries;
            _baseDelayMs = baseDelayMs;
        }

        public void Execute(Action action, Action<Exception> onFailure)
        {
            int attempt = 0;
            while (true)
            {
                try
                {
                    action();
                    return;
                }
                catch (Exception ex)
                {
                    attempt++;
                    if (attempt >= _maxRetries)
                    {
                        onFailure(ex);
                        return;
                    }

                    int delay = _baseDelayMs * (int)Math.Pow(2, attempt);
                    Console.WriteLine($"⚠️ Retry {attempt}/{_maxRetries} after {delay}ms: {ex.Message}");
                    Thread.Sleep(delay);
                }
            }
        }
    }
}
