using Microsoft.Extensions.Options;

namespace VendorGateway.Tests.Configuration
{
    public class OptionsMonitorAdapter<T> : IOptionsMonitor<T> where T : class
    {
        public OptionsMonitorAdapter(T currentValue)
        {
            CurrentValue = currentValue;
        }

        public T CurrentValue { get; }

        public T Get(string? name) => CurrentValue;

        public IDisposable OnChange(Action<T, string?> listener)
        {
            return new NullDisposable();
        }

        private class NullDisposable : IDisposable
        {
            public void Dispose() { }
        }
    }
}
