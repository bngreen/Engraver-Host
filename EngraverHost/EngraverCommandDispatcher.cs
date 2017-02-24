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

using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EngraverHost
{
    public class EngraverCommandDispatcher : IDisposable
    {
        private SerialPort Port { get; set; }
        private Queue<string> Commands { get; set; } = new Queue<string>();
        private bool IsOpen = true;

        public int PendingCommands => Commands.Count;

        public event EventHandler<CommandFinishedEventArgs> CommandFinished;

        public class CommandFinishedEventArgs : EventArgs
        {
            public string Command { get; private set; }
            public CommandFinishedEventArgs(string command)
            {
                Command = command;
            }
        }

        public void CancelCommands()
        {
            lock(Commands)
            {
                Commands.Clear();
            }
        }

        public EngraverCommandDispatcher(string port)
        {
            Port = new SerialPort(port);
            Port.Open();
            var t = new Thread(new ThreadStart(Worker));
            t.IsBackground = true;
            t.Start();
        }

        private void Worker()
        {
            try
            {
                while (IsOpen)
                {
                    string command;
                    lock (Commands)
                    {
                        if (Commands.Count == 0)
                            continue;
                        command = Commands.Dequeue();
                    }
                    Port.WriteLine(command);
                    while (!Port.ReadLine().Contains("done")) ;
                    CommandFinished?.Invoke(this, new CommandFinishedEventArgs(command));
                    Thread.Sleep(10);
                }
            }
            catch { }
        }

        public void Dispatch(string command)
        {
            if (String.IsNullOrEmpty(command))
                return;
            lock (Commands)
                Commands.Enqueue(command);
        }

        public void Dispose()
        {
            IsOpen = false;
            CommandFinished = null;
            try
            {
                Port.Close();
                Port.Dispose();
            }
            catch { }
        }
    }
}
