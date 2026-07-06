using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace VendorGateway.Filters
{
    public sealed class IdempotencyKeyHeaderOperationProcessor : IOperationProcessor
    {
        public bool Process(OperationProcessorContext context)
        {
            var idempotencyParam = context.OperationDescription.Operation.Parameters?
                .FirstOrDefault(p => p.Name == "Idempotency-Key");

            if (idempotencyParam is not null)
            {
                idempotencyParam.Example = Guid.NewGuid().ToString();
            }

            return true; // must return true to keep the operation in the document
        }
    }
}
