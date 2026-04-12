using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Arcadia.Core.Plugins
{
    /// <summary>
    /// Dynamically loads plugins from the local filesystem.
    /// </summary>
    public class PluginLoader
    {
        private readonly string _pluginFolder;

        public PluginLoader(string? pluginPath = null)
        {
            _pluginFolder = pluginPath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");
            if (!Directory.Exists(_pluginFolder)) Directory.CreateDirectory(_pluginFolder);
        }

        public List<IPlugin> LoadPlugins()
        {
            var plugins = new List<IPlugin>();
            var pluginFiles = Directory.GetFiles(_pluginFolder, "*.dll");

            foreach (var file in pluginFiles)
            {
                try
                {
                    var assembly = Assembly.LoadFrom(file);
                    var pluginTypes = assembly.GetTypes()
                        .Where(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

                    foreach (var type in pluginTypes)
                    {
                        if (Activator.CreateInstance(type) is IPlugin plugin)
                        {
                            plugins.Add(plugin);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log error: Could not load plugin assembly
                    System.Diagnostics.Debug.WriteLine($"Failed to load plugin {file}: {ex.Message}");
                }
            }
            return plugins;
        }
    }
}