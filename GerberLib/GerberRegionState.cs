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
    class GerberRegionState : IGerberState
    {
        public IGerberState State { get; private set; }

        public IList<Point> CurrentPoints { get; private set; } = new List<Point>();

        public GerberRegionState(IGerberState state)
        {
            State = state;
        }

        public IAperture Aperture
        {
            get
            {
                return State.Aperture;
            }
        }

        public bool ClearPolarity
        {
            get
            {
                return State.ClearPolarity;
            }
        }

        public Point CurrentPosition
        {
            get
            {
                return State.CurrentPosition;
            }
        }

        public void CreatePolygon(IList<Point> points)
        {
            throw new InvalidOperationException();
        }

        public void FlashOn(Point position)
        {
            throw new InvalidOperationException();
        }

        public void LineTo(Point position)
        {
            if (!CurrentPoints.Any())
                CurrentPoints.Add(CurrentPosition);
            CurrentPoints.Add(position);
        }

        public void MoveTo(Point position)
        {
            if (CurrentPoints.Any())
                State.CreatePolygon(CurrentPoints);
            CurrentPoints = new List<Point>();
            State.MoveTo(position);
        }

        public void SetAperture(int aperture)
        {
            throw new InvalidOperationException();
        }

        public void SetCurrentPosition(Point position)
        {
            State.SetCurrentPosition(position);
        }

        public void SetPolarity(bool clear)
        {
            throw new InvalidOperationException();
        }

        public void Finish()
        {
            if (CurrentPoints.Any())
                State.CreatePolygon(CurrentPoints);
            CurrentPoints = new List<Point>();
        }

    }
}
