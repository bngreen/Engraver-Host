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
using static System.Math;


namespace GerberLib
{
    class NullRenderer : IRenderer
    {
        public double MaxX { get; private set; } = double.MinValue;
        public double MaxY { get; private set; } = double.MinValue;
        public double MinX { get; private set; } = double.MaxValue;
        public double MinY { get; private set; } = double.MaxValue;

        private void VisitPoint(double x, double y)
        {
            MaxX = Max(MaxX, x);
            MaxY = Max(MaxY, y);
            MinX = Min(MinX, x);
            MinY = Min(MinY, y);
        }

        public void DrawLine(double x0, double y0, double x1, double y1, double lineWidth, bool clear = false)
        {
            VisitPoint(x0, y0);
            VisitPoint(x1, y1);
        }

        public void FillCircle(double x, double y, double radius, bool clear = false)
        {
            VisitPoint(x + radius, y + radius);
            VisitPoint(x - radius, y - radius);
        }

        public void FillPolygon(IEnumerable<Point> points, bool clear = false)
        {
            foreach (var p in points)
                VisitPoint(p.X, p.Y);
        }

        public void FillRectangle(double x, double y, double width, double height, bool clear = false)
        {
            VisitPoint(x, y);
            VisitPoint(x + width, y + height);
            VisitPoint(x, y + height);
            VisitPoint(x + width, y);
        }

        public void FillRoundedRectangle(double x, double y, double width, double height, double radius, bool clear = false)
        {
            FillRectangle(x, y, width, height);
        }
    }
}
