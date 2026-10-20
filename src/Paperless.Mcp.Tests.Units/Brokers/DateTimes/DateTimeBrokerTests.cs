using AwesomeAssertions;
using Paperless.Mcp.Brokers.DateTimes;

namespace Paperless.Mcp.Tests.Units.Brokers.DateTimes
{
    // Broker werden hier normalerweise nicht unit-getestet -- sie enthalten keine
    // Logik. Dieser eine Test haelt den Test-Stack (MSTest, AwesomeAssertions)
    // nachweislich lauffaehig.
    [TestClass]
    public sealed class DateTimeBrokerTests
    {
        [TestMethod]
        public async Task ShouldReturnCurrentDateTimeOffsetAsync()
        {
            // given
            var dateTimeBroker = new DateTimeBroker();
            DateTimeOffset beforeCall = DateTimeOffset.UtcNow;

            // when
            DateTimeOffset actualDateTimeOffset =
                await dateTimeBroker.GetCurrentDateTimeOffsetAsync();

            // then
            actualDateTimeOffset.Should().BeOnOrAfter(beforeCall);
            actualDateTimeOffset.Should().BeOnOrBefore(DateTimeOffset.UtcNow);
        }
    }
}
