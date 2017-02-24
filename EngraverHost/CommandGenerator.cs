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

namespace EngraverHost
{
    public class CommandGenerator
    {
        public IEnumerable<string> Generate(BitImage image, double lasel, double rasterSpeed, double moveSpeed, double laserDuty, int laserId, bool flipAxis, double backlashCompensation = 0.15)
        {
            var laserOn = false;
            var XAxis = "X";
            var YAxis = "Y";
            if (flipAxis)
            {
                XAxis = "Y";
                YAxis = "X";
            }
            var commands = new List<string>();
            for(int i = 0; i < image.Height; i++)
            {
                commands.Add($"M900 {laserId} 0");
                laserOn = false;
                commands.Add($"G01 {YAxis}{i * lasel} F{moveSpeed}");
                //commands.Add($"G01 {XAxis}0 F{rasterSpeed}");
                var dir = i % 2 == 1 ? -1 : 1;
                var offset = 0.0;
                if (dir == -1)
                    offset = -backlashCompensation;//-0.15;
                for (int o = 0; o < image.Width; o++)
                {
                    var x = i % 2 == 1 ? image.Width - o - 1 : o;
                    
                    /*var dir = 1;
                    var x = o;*/
                    
                    if (image[x, i] != laserOn)
                    {
                        if (o != 0)
                            commands.Add($"G01 {XAxis}{(x - dir) * lasel + offset} F{rasterSpeed}");
                        laserOn = image[x, i];
                        if (!laserOn)// on to off, turn off and THEN move to the lasel where it's off
                        {
                            commands.Add($"M900 {laserId} {Math.Round(laserDuty * 10000 * (laserOn ? 1 : 0))}");
                            commands.Add($"G01 {XAxis}{x * lasel + offset} F{rasterSpeed}");
                        }
                        else//off to on, move to on position and then turn it on
                        {
                            commands.Add($"G01 {XAxis}{x * lasel + offset} F{rasterSpeed}");
                            commands.Add($"M900 {laserId} {Math.Round(laserDuty * 10000 * (laserOn ? 1 : 0))}");
                        }
                    }
                    else if (o == (image.Width - 1))
                        commands.Add($"G01 {XAxis}{x * lasel + offset} F{rasterSpeed}");
                }
            }
            commands.Add("G28");
            return commands;
        }
    }
}
