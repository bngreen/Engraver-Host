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
    class GerberState : IGerberState
    {
        public IDictionary<int, IAperture> Apertures { get; private set; }

        public GerberState(IDictionary<int, IAperture> apertures)
        {
            Apertures = apertures;
            //Aperture = Apertures.First().Value;
        }

        public IAperture Aperture { get; private set; }
        public Point CurrentPosition { get; private set; } = new Point();
        public GerberObjects Objects { get; private set; } = new GerberObjects();
        public bool ClearPolarity { get; private set; }
        public void SetPolarity(bool clear) => ClearPolarity = clear;
        public void SetAperture(int aperture) => Aperture = Apertures[aperture];
        public void SetCurrentPosition(Point position) => CurrentPosition = position;
        public void LineTo(Point position) => Objects.Add(new GerberLine(CurrentPosition, position, ClearPolarity, Aperture));
        public void MoveTo(Point position) => SetCurrentPosition(position);
        public void FlashOn(Point position) =>  Objects.Add(new GerberFlash(position, Aperture, ClearPolarity));
        public void CreatePolygon(IList<Point> points) => Objects.Add(new GerberPolygon(ClearPolarity, points));

    }
}
