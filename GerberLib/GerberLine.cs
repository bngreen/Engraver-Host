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
using System.Windows;

namespace GerberLib
{
    public class GerberLine : GerberObject
    {
        public Point Start { get; private set; }
        public Point End { get; private set; }
        public IAperture Aperture { get; private set; }
        public GerberLine(Point start, Point end, bool clearPolarity, IAperture aperture) : base(clearPolarity)
        {
            Start = start;
            End = end;
            Aperture = aperture;
        }

        public override void Render(IRenderer renderer)
        {
            var lineWidth = Aperture.RX;
            var aperture = Aperture;
            if (Aperture is Gerber.ObroundAperture)
            {
                aperture = new Gerber.CircleAperture(lineWidth, null);
            }
            else if (Aperture is Gerber.RectangleAperture)
            {
                if (Start.X == End.X && Start.Y != End.Y)
                    lineWidth = Aperture.RX;
                else if (Start.Y == End.Y && Start.X != End.X)
                    lineWidth = Aperture.RY;
                else
                    lineWidth = Math.Max(Aperture.RX, Aperture.RY);
            }

            lineWidth *= 2;
            renderer.DrawLine(Start.X, Start.Y, End.X, End.Y, lineWidth, ClearPolarity);
            new GerberFlash(Start, aperture, ClearPolarity).Render(renderer);
            new GerberFlash(End, aperture, ClearPolarity).Render(renderer);
        }

    }
}
