using System.Diagnostics.CodeAnalysis;
using Axle.Modularity;
using Forest.ComponentModel;
using Forest.Engine;
using Forest.Engine.Instructions;
using Forest.Navigation;

namespace Forest.Security
{
    [Module]
    internal sealed class ForestSecurityModule : IForestSecurityExceptionHandler, IForestSecurityManager
    {
        private IForestSecurityExceptionHandler _securityExceptionHandler = new DefaultForestSecurityExceptionHandler();
        private IForestSecurityManager _securityManager = new NoOpForestSecurityManager();

        [ModuleDependencyInitialized]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        internal void OnDependencyInitialized(IForestSecurityManagerProvider securityManagerProvider)
        {
            _securityManager = securityManagerProvider.GetSecurityManager();
        }
        
        [ModuleDependencyInitialized]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        internal void OnDependencyInitialized(IForestSecurityExceptionHandlerProvider securityExceptionHandlerProvider)
        {
            _securityExceptionHandler = securityExceptionHandlerProvider.GetSecurityExceptionHandler();
        }

        bool IForestSecurityManager.HasAccess(IForestViewDescriptor descriptor) => _securityManager.HasAccess(descriptor);
        bool IForestSecurityManager.HasAccess(IForestCommandDescriptor descriptor) => _securityManager.HasAccess(descriptor);

        void IForestSecurityExceptionHandler.HandleSecurityException(
                ForestSecurityException securityException, 
                Location location,
                IForestEngine forestEngine) 
            => _securityExceptionHandler.HandleSecurityException(securityException, location, forestEngine);

        void IForestSecurityExceptionHandler.HandleSecurityException(
                ForestSecurityException securityException, 
                string command, 
                IForestEngine forestEngine) 
            => _securityExceptionHandler.HandleSecurityException(securityException, command, forestEngine);

        void IForestSecurityExceptionHandler.HandleSecurityException(
                ForestSecurityException securityException, 
                IForestEngine forestEngine) 
            => _securityExceptionHandler.HandleSecurityException(securityException, forestEngine);
    }
}