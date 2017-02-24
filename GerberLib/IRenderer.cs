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

using System.Collections.Generic;
using System.Windows;

namespace GerberLib
{
    public interface IRenderer
    {
        void FillPolygon(IEnumerable<Point> points, bool clear = false);
        void DrawLine(double x0, double y0, double x1, double y1, double lineWidth, bool clear = false);
        void FillCircle(double x, double y, double radius, bool clear = false);
        void FillRectangle(double x, double y, double width, double height, bool clear = false);
        void FillRoundedRectangle(double x, double y, double width, double height, double radius, bool clear = false);
    }
}