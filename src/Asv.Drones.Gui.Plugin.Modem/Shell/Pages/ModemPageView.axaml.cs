using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Plugin.Modem;

[ExportView(typeof(ModemPageViewModel))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public partial class ModemPageView : ReactiveUserControl<ModemPageViewModel>
{
    public ModemPageView()
    {
        InitializeComponent();
    }
}