using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO.Ports;
using System.Reactive;
using System.Reactive.Linq;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.IO;
using Avalonia.Controls;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SkiaSharp;

namespace Asv.Drones.Gui.Plugin.Modem;

[ExportShellPage(UriString)]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class ModemPageViewModel:ShellPage
{
    public const string UriString = "asv:shell.page.modem";
    public static readonly Uri Uri = new(UriString);
    private readonly ObservableCollection<int> _values;

    public ModemPageViewModel():base(Uri)
    {
        _values = new ObservableCollection<int>();
        Series= new ISeries[]
        {
            new LineSeries<int>
            {
                Values = _values,
                Fill = null,
                GeometryFill = null,
                GeometryStroke = null,
                LineSmoothness = 0,
                Stroke = new SolidColorPaint(SKColors.Blue, 1)
            }
        };
        if (Design.IsDesignMode)
        {
            
        }
    }
    
    [ImportingConstructor]
    public ModemPageViewModel(ILogService log) : this()
    {
        StartTest = ReactiveCommand
            .CreateFromObservable(
                () => Observable
                    .StartAsync(StartTestImpl)
                    .TakeUntil(StopTest)).DisposeItWith(Disposable);
        StopTest = ReactiveCommand.Create(
            () => { },
            StartTest.IsExecuting).DisposeItWith(Disposable);;
    }

    private async Task StartTestImpl(CancellationToken cancel)
    {
        try
        {
            using var port1 = PortFactory.Create(SerialPort1);
            using var port2 = PortFactory.Create(SerialPort2);
            port1.Enable();
            port2.Enable();
            await port1.State.FirstAsync(_ => _ == PortState.Connected);
            await port2.State.FirstAsync(_ => _ == PortState.Connected);
            using var loopSubscribe = port2.Subscribe(_ =>
            {
                //Thread.Sleep(10);
                port2.Send(_, _.Length, cancel).Wait(cancel);
            });
            var r = new Random();
            var txData = new byte[SendSize];
            var rxData = new List<byte>(SendSize);
            
            while (true)
            {
                rxData.Clear();
                var tcs = new TaskCompletionSource();
                using var timeout = new CancellationTokenSource(1000);
                using var all = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token, cancel);
                await using var tcsRegister = timeout.Token.Register(() => tcs.TrySetCanceled(), useSynchronizationContext: false);
                r.NextBytes(txData);
                using var subscribe = port1.Subscribe(_ =>
                {
                    rxData.AddRange(_);
                    if (rxData.Count >= txData.Length)
                    {
                        tcs.TrySetResult();
                    }
                });
                await port1.Send(txData,txData.Length,all.Token);
                try
                {
                    await tcs.Task;
                    int err = 0;
                    for (int i = 0; i < rxData.Count; i++)
                    {
                        try
                        {
                            if (rxData[i] != txData[i]) err++;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            
                        }
                    }

                    lock (Sync)
                    {
                        _values.Add(err);
                    }
                }
                catch(Exception ex)
                {
                    Timeout += 1;
                    _values.Add(-1);
                    Console.WriteLine(ex);
                }
                if (all.Token.IsCancellationRequested) break;
               
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    
    private async Task StartTestImpl2(CancellationToken cancel)
    {
        try
        {
            using var port1 = PortFactory.Create(SerialPort1);
            using var port2 = PortFactory.Create(SerialPort2);
            port1.Enable();
            port2.Enable();
            await port1.State.FirstAsync(_ => _ == PortState.Connected);
            await port2.State.FirstAsync(_ => _ == PortState.Connected);
           
            var txData = new byte[SendSize];
            var rxData = new List<byte>(SendSize);
            int sended = 0;
            var run = true;
            var a = new Thread(() =>
            {
                try
                {
                    while (run)
                    {
                        port1.Send(txData, txData.Length, cancel).Wait(cancel);
                        sended += txData.Length;    
                        Thread.Sleep(100);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                
            });
            using var subscribe = port2.Subscribe(_ =>
            {
                rxData.AddRange(_);
            });
            a.Start();

            await Task.Delay(500, cancel);
            var b = new byte[] { 0xFF };
            await port2.Send(b, b.Length,cancel);
            await Task.Delay(500, cancel);
            
            run = false;
            Timeout = sended - rxData.Count;
                        
            
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public int SendSize { get; set; } = 255;

    [Reactive] public string SerialPort1 { get; set; } = "serial:COM5?br=115200";
    [Reactive] public string SerialPort2 { get; set; } = "serial:COM6?br=115200";
    public ReactiveCommand<Unit,Unit> StartTest { get; }
    public ReactiveCommand<Unit,Unit> StopTest { get; }
    
    public ISeries[] Series { get; set; }
    public object Sync { get; } = new();
    [Reactive]
    public int Timeout { get; set; }
}