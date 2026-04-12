using System;

namespace Arcadia.Core.Plugins
{
    /// <summary>
    /// Base interface for Arcadia plugins.
    /// </summary>
    public interface IPlugin
    {
        string Name { get; }
        string Version { get; }
        string Author { get; }
        void Initialize(IServiceProvider serviceProvider);
        void Shutdown();
    }
}