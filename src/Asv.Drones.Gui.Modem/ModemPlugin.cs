using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;

namespace Asv.Drones.Gui.Modem;

[PluginEntryPoint("Modem", CorePlugin.Name)]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class ModemPlugin : IPluginEntryPoint
{
    [ImportingConstructor]
    public ModemPlugin()
    {
    }
    public void Initialize()
    {
    }

    public void OnFrameworkInitializationCompleted()
    {
    }

    public void OnShutdownRequested()
    {
    }
}