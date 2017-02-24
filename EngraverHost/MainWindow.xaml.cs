/*
Copyright (C) 2016  Bruno Naspolini Green. All rights reserved.

This file is part of Engraver-Host.
https://github.com/bngreen/Engraver-Host

Engraver-Host is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Engraver-Host is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Engraver-Host.  If not, see <http://www.gnu.org/licenses/>.
*/

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EngraverHost
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public EngraverCommandDispatcher CommandDispatcher { get; set; }

        public IList<string> SerialPorts => SerialPort.GetPortNames();

        public ICommand RefreshSerialPortsCommand => new Command(() => NotifyPropertyChanged(nameof(SerialPorts)));
        public string SelectedSerialPort { get; set; } = Properties.Settings.Default.SerialPort;

        public ICommand ConnectCommand => new Command(() =>
        {
            if (CommandDispatcher == null)
            {
                CommandDispatcher = new EngraverCommandDispatcher(SelectedSerialPort);
                CommandDispatcher.CommandFinished += CommandDispatcher_CommandFinished;
                
                Properties.Settings.Default.SerialPort = SelectedSerialPort;
                Properties.Settings.Default.Save();
            }
            else
            {
                CommandDispatcher.Dispose();
                CommandDispatcher = null;
            }
            NotifyPropertyChanged(nameof(ConnectText));
            NotifyPropertyChanged(nameof(CommandsQueueText));
        });

        private void CommandDispatcher_CommandFinished(object sender, EngraverCommandDispatcher.CommandFinishedEventArgs e)
        {
            NotifyPropertyChanged(nameof(CommandsQueueText));
        }

        public string CommandsQueueText => (((CommandDispatcher?.PendingCommands) ?? 0) == 0 ? "Idle" : $"Pending {CommandDispatcher.PendingCommands} Commands");

        private void SendCommand(string command)
        {
            CommandDispatcher?.Dispatch(command);
            NotifyPropertyChanged(nameof(CommandsQueueText));
        }

        public string ConnectText => CommandDispatcher == null ? "Connect" : "Disconnect";

        public ICommand HomeCommand => new Command(() => SendCommand("G28"));

        public ICommand SetHomeCommand => new Command(() => SendCommand("G92"));

        public string UserCommand { get; set; }

        public ICommand SendUserCommand => new Command(() => {
            SendCommand(UserCommand);
            //UserCommand = String.Empty;
            //NotifyPropertyChanged(nameof(UserCommand));
            }   
        );

        public float MoveBy { get; set; } = 1;

        public ICommand MoveLeftCommand => new Command(() => SendCommand($"G1 X-{MoveBy} R"));
        public ICommand MoveRightCommand => new Command(() => SendCommand($"G1 X{MoveBy} R"));
        public ICommand MoveUpCommand => new Command(() => SendCommand($"G1 Y{MoveBy} R"));
        public ICommand MoveDownCommand => new Command(() => SendCommand($"G1 Y-{MoveBy} R"));

        private uint laserDuty;
        public uint LaserDuty { get { return laserDuty; } set { laserDuty = value; NotifyPropertyChanged(nameof(LaserDuty)); NotifyPropertyChanged(nameof(LaserLabel)); } }
        public string LaserLabel => $"Laser {((float)LaserDuty)/100}%";

        public ICommand SetLaserDutyCommand => new Command(() => SendCommand($"M900 0 {LaserDuty}"));
        public ICommand LaserOffCommand => new Command(() => SendCommand($"M900 0 0"));

        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();
        }

        public void NotifyPropertyChanged(string n)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public int Scale { get; set; } = 10;
        public double Duty { get; set; } = 0.1;

        public double BacklashCompensation { get; set; } = 0.15;

        public ICommand PrintTest => new Command(() =>
        {
            if(gerberObjects == null)
            {
                MessageBox.Show("Load a gerber file");
                return;
            }

            if(CommandDispatcher == null)
            {
                MessageBox.Show("Connect to engraver first");
                return;
            }

            var scale = Scale;//10;
            var duty = Duty;// 0.10;
            BitImage image = RenderGerber(gerberObjects, scale);
            var commands = new CommandGenerator().Generate(image, 1.0 / scale, 250, 180, duty, 0, false, BacklashCompensation);
            foreach (var x in commands)
            {
                SendCommand(x);
            }
        });

        private static BitImage RenderGerber(GerberLib.GerberObjects gerberObjs, int scale, bool flipImage = false)
        {
            var gRenderer = new GerberLib.CairoRenderer(gerberObjs.GetRequiredSize(scale), scale, flipImage, true);
            gerberObjs.Render(gRenderer);
            //gRenderer.SavePng("prt.png");
            var imdata = gRenderer.GetData();
            //File.WriteAllBytes("shit.bin", imdata);
            var image = new BitImage(imdata, gRenderer.Width, gRenderer.Height);
            return image;
        }

        GerberLib.GerberObjects gerberObjects = null;

        public ICommand OpenGerberCommand => new Command(() =>
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Gerber files|*.gtl";
            if (ofd.ShowDialog() ?? false)
            {
                gerberObjects = GerberLib.GerberParser.Parse(ofd.FileName);
                var bmp = RenderGerber(gerberObjects, 10, true);
                image.Source = fromBitmap(bmp.ToBitmap());
            }
        });


        private ImageSource fromBitmap(System.Drawing.Bitmap bmp)
        {
            MemoryStream ms = new MemoryStream();
            bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            ms.Position = 0;
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = ms;
            bi.EndInit();
            return bi;
        }

    }
}
