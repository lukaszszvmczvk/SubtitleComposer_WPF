using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using SubtitlePlugins;
namespace wpf_lab2
{
    public class PluginManager
    {
        public List<ISubtitlesPlugin> _plugins;
        public PluginManager()
        {
            _plugins = new List<ISubtitlesPlugin>();
        }

        public void LoadPlugins(string pluginsDirectory)
        {
            if (!Directory.Exists(pluginsDirectory))
                return;

            var pluginFiles = Directory.GetFiles(pluginsDirectory, "*.dll");

            foreach (var pluginFile in pluginFiles)
            {
                try
                {
                    var assembly = Assembly.LoadFrom(pluginFile);
                    var pluginTypes = assembly.GetTypes().Where(t => typeof(ISubtitlesPlugin).IsAssignableFrom(t));

                    foreach (var pluginType in pluginTypes)
                    {
                        var plugin = Activator.CreateInstance(pluginType) as ISubtitlesPlugin;
                        _plugins.Add(plugin);
                    }
                }
                catch (Exception ex)
                {
                }
            }
        }

    }
}
