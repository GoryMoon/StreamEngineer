using System.Reflection;
using ConfigTest.Config;

var path = Path.GetDirectoryName(
    Uri.UnescapeDataString(new UriBuilder(Assembly.GetExecutingAssembly().Location).Path));
Console.WriteLine($"Path: {path}");
//Configuration.TokenConfig.Init(path);
Configuration.PluginConfig.Init(path);