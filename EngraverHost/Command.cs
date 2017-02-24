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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EngraverHost
{
    public class Command : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public Action<object> ExecuteLambda { get; set; }

        public Func<object, bool> CanExecuteLambda { get; set; }

        public Command(Action<object> executeLambda)
        {
            ExecuteLambda = executeLambda;
        }

        public Command(Action executeLambda)
        {
            ExecuteLambda = o => executeLambda();
        }

        public bool CanExecute(object parameter)
        {
            return CanExecuteLambda?.Invoke(parameter) ?? true;
        }

        public void Execute(object parameter)
        {
            ExecuteLambda?.Invoke(parameter);
        }
    }
}
