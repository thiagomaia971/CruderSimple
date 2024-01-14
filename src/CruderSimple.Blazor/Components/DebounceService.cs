using System.Timers;
using Microsoft.AspNetCore.Components;

namespace CruderSimple.Blazor.Components
{
    public class DebounceService : ComponentBase
    {
        private System.Timers.Timer _timer { get; set; }

        public void Start(int milliseconds, Func<Task> action)
        {
            DisposeTimer();
            _timer = new System.Timers.Timer(milliseconds);
            _timer.Elapsed += async (sender, _e) => await TimerElapsed(sender, _e, action);
            _timer.Enabled = true;
            _timer.Start();
        }

        public void Start<T>(int milliseconds, Func<T, Task> action, T @event)
        {
            DisposeTimer(); 
            _timer = new System.Timers.Timer(milliseconds);
            _timer.Elapsed += async (sender, _e) => await TimerElapsed<T>(sender, _e, action, @event);
            _timer.Enabled = true;
            _timer.Start();
        }

        private void DisposeTimer()
        {
            _timer?.Dispose();
            _timer = null;
        }
        
        private async Task TimerElapsed<T>(object? sender, ElapsedEventArgs _e, Func<T, Task> action, T @event)
        {
            await action.Invoke(@event);
            DisposeTimer();
        }

        private async Task TimerElapsed(object? sender, ElapsedEventArgs _e, Func<Task> action)
        {
            await action.Invoke();
            DisposeTimer();
        }
    }
}
