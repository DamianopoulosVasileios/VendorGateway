using System.Diagnostics.Metrics;

namespace VendorGateway.Application.Diagnostics
{
    public sealed class VendorGatewayMetrics
    {
        public const string MeterName = "VendorGateway";

        private readonly Counter<long> _jobsCompleted;
        private readonly Counter<long> _jobsFailed;
        private readonly Counter<long> _ordersCreated;

        public VendorGatewayMetrics(IMeterFactory meterFactory)
        {
            var meter = meterFactory.Create(MeterName);
            _jobsCompleted = meter.CreateCounter<long>("vendorgateway.jobs.completed");
            _jobsFailed = meter.CreateCounter<long>("vendorgateway.jobs.failed");
            _ordersCreated = meter.CreateCounter<long>("vendorgateway.orders.created");
        }

        public void JobCompleted(string jobType) => _jobsCompleted.Add(1, new KeyValuePair<string, object?>("job.type", jobType));
        public void JobFailed(string jobType) => _jobsFailed.Add(1, new KeyValuePair<string, object?>("job.type", jobType));
        public void OrderCreated() => _ordersCreated.Add(1);
    }
}
