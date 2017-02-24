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
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GerberLib
{
    public static class Extensions
    {
        public static Int32 ToInt32(this string v) => Convert.ToInt32(v);
        public static double ToDouble(this string v) => Convert.ToDouble(v);
        public static double? ToDouble(this Group gc)
        {
            if (gc.Success)
                return gc.Value.ToDouble();
            return null;
        }

    }
}
