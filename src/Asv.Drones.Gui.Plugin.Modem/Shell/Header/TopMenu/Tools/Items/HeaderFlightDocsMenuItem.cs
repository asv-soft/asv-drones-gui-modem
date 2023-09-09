using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Avalonia.Input;
using Material.Icons;
using ReactiveUI;

namespace Asv.Drones.Gui.Plugin.Modem;

[Export(HeaderMenuItem.UriString + "/tools", typeof(IHeaderMenuItem))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class HeaderFlightDocsMenuItem : HeaderMenuItem
{
    private readonly INavigationService _navigation;

    [ImportingConstructor]
    public HeaderFlightDocsMenuItem(INavigationService navigation) : base(new Uri("asv:shell.header.menu/tools/modem"))
    {
        _navigation = navigation;
        
        Header = "Modem configuration";
        Icon = MaterialIconKind.Radio;
        Order = short.MinValue + 2;
        HotKey = KeyGesture.Parse("Alt+D");
        Command = ReactiveCommand.Create(() => _navigation.GoTo(ModemPageViewModel.Uri));
    }
}