using Paperless.Mcp.Models.Orchestrations.Capabilities.Exceptions;
using Paperless.Mcp.Models.Processings.Healths.Exceptions;
using Xeptions;

namespace Paperless.Mcp.Services.Orchestrations.Capabilities
{
    public partial class CapabilityOrchestrationService
    {
        // Hier gibt es nichts zu validieren: der Bericht nimmt keine Eingabe entgegen.
        // Und die erwartbaren Fehler der Instanz sind schon in der HealthProcessingService
        // zu Feldern geworden -- was hier noch ankommt, ist tatsaechlich unerwartet.
        private static async ValueTask<T> TryCatch<T>(Func<ValueTask<T>> returningFunction)
        {
            try
            {
                return await returningFunction();
            }
            catch (HealthProcessingServiceException healthProcessingServiceException)
            {
                throw new CapabilityOrchestrationServiceException(
                    message: "Capability orchestration service error occurred, contact support.",
                    innerException: healthProcessingServiceException);
            }
            catch (Exception exception)
            {
                var failedCapabilityOrchestrationServiceException =
                    new FailedCapabilityOrchestrationServiceException(
                        message: "Failed capability orchestration service error occurred, contact support.",
                        innerException: exception);

                throw new CapabilityOrchestrationServiceException(
                    message: "Capability orchestration service error occurred, contact support.",
                    innerException: failedCapabilityOrchestrationServiceException);
            }
        }
    }
}
