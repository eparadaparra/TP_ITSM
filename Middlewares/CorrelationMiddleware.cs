namespace TP_ITSM.Middlewares
{
    public class CorrelationMiddleware
    {
        private readonly RequestDelegate _next;

        public CorrelationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var idempotencyKey =
                context.Request.Headers["Idempotency-Key"].FirstOrDefault()
                ?? Guid.NewGuid().ToString();

            using (Serilog.Context.LogContext.PushProperty("IdempotencyKey", idempotencyKey))
            using (Serilog.Context.LogContext.PushProperty("TraceId", context.TraceIdentifier))
            {
                await _next(context);
            }
        }
    }

}
