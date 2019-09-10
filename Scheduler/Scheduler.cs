using System;
using System.Threading;
using System.Threading.Tasks;
using Scheduler.Delegate;

namespace Scheduler
{
    public abstract class Scheduler
    {
        private readonly Action _action;

        public Scheduler(Action action)
        {
            this._action = action;
        }

        public Scheduler(BaseAction baseAction) : this(baseAction.Execute)
        {
        }

        protected async void RunActionAsync()
        {
            await Task.Run(_action);
        }

        public abstract void RunScheduler(CancellationToken cancellationToken);

        public virtual async Task RunSchedulerAsync(CancellationToken? cancellationToken = null)
        {
            CancellationToken token = cancellationToken ?? CancellationToken.None;
            await Task.Run(() => RunScheduler(token));
        }
    }
}
