using Microsoft.Extensions.DependencyInjection;

namespace Bernhoeft.GRT.Teste.UnitTests.Base
{
    public abstract class BaseUnitTest : IDisposable
    {
        protected readonly IServiceProvider ServiceProvider;
        protected readonly ServiceCollection Services;

        protected BaseUnitTest()
        {
            Services = new ServiceCollection();
            ConfigureServices(Services);
            ServiceProvider = Services.BuildServiceProvider();
        }

        protected virtual void ConfigureServices(IServiceCollection services)
        {
            // Override in derived classes to configure specific services
        }

        protected T GetService<T>() where T : class
        {
            return ServiceProvider.GetRequiredService<T>();
        }

        public virtual void Dispose()
        {
            if (ServiceProvider is IDisposable disposableProvider)
            {
                disposableProvider.Dispose();
            }
            GC.SuppressFinalize(this);
        }
    }
}