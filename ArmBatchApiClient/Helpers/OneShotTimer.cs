using System;
using System.Threading;
using System.Threading.Tasks;

namespace ArmBatchApiClient.Helpers
{
    public class OneShotTimer
    {
        private readonly Func<Task> _action;
        private int _scheduleCounter = 0;
        private readonly TimeSpan _delay;
        private long _expirationTicks = 0;

        public OneShotTimer(
            TimeSpan delay,
            Func<Task> action)
        {
            if (delay <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException($"The value of {nameof(delay)} should be positive.");
            }

            _delay = delay;
            _action = action;
        }

        private async Task ScheduleTask()
        {
            if (Interlocked.Increment(ref _scheduleCounter) != 1)
            {
                return;
            }

            try
            {
                for (;;)
                {
                    var currentTicks = DateTimeOffset.UtcNow.Ticks;
                    var tickDifference = Interlocked.Read(ref _expirationTicks) - currentTicks;

                    if (tickDifference <= 0)
                    {
                        break;
                    }

                    await Task.Delay(TimeSpan.FromTicks(tickDifference));

                    Interlocked.Exchange(ref _scheduleCounter, 1);
                }

                await _action();
            }
            finally
            {
                if (Interlocked.Exchange(ref _scheduleCounter, 0) != 1)
                {
                    var unused = ScheduleTask();
                }
            }
        }

        public void Reset()
        {
            var desiredExpiration = DateTimeOffset.UtcNow.Add(_delay).Ticks;
            var currentExpiration = Interlocked.Read(ref _expirationTicks);

            while (desiredExpiration > currentExpiration)
            {
                var newCurrentExpiration = Interlocked.CompareExchange(
                    ref _expirationTicks,
                    desiredExpiration,
                    currentExpiration);

                if (newCurrentExpiration == currentExpiration)
                {
                    break;
                }

                currentExpiration = newCurrentExpiration;
            }

            var unused = ScheduleTask();
        }
    }
}
