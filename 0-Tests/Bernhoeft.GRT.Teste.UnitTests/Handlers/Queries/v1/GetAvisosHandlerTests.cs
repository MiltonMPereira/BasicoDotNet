using Bernhoeft.GRT.Teste.Application.Requests.Queries.v1;
using FluentAssertions;
using Xunit;

namespace Bernhoeft.GRT.Teste.UnitTests.Handlers.Queries.v1
{
    public class GetAvisosHandlerTests
    {
        [Fact]
        public void GetAvisosRequest_ShouldBeInstantiable()
        {
            // Arrange & Act
            var request = new GetAvisosRequest();

            // Assert
            request.Should().NotBeNull();
        }

        [Fact]
        public void GetAvisosRequest_MultipleInstances_ShouldBeIndependent()
        {
            // Arrange & Act
            var request1 = new GetAvisosRequest();
            var request2 = new GetAvisosRequest();

            // Assert
            request1.Should().NotBeNull();
            request2.Should().NotBeNull();
            request1.Should().NotBeSameAs(request2);
        }
    }
}