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
using Cairo;

namespace GerberLib
{
    public class CairoRenderer : IRenderer
    {
        private Context ctx { get; set; }
        private Surface surface { get; set; }

        public CairoRenderer(System.Windows.Size size, double scale, bool flip = true, bool bitImage = false) : this((int)Math.Ceiling(size.Width), (int)Math.Ceiling(size.Height), scale, bitImage, flip)
        {

        }

        public byte[] GetData()
        {
            if (surface is ImageSurface)
                return (surface as ImageSurface).Data;
            return null;
        }

        public int Width { get; private set; }
        public int Height { get; private set; }
        public double Scale { get; private set; }

        public CairoRenderer(int width, int height, double scale, bool bitImage, bool flip, Surface _surface = null)
        {
            Width = width;
            Height = height;
            Scale = scale;
            surface = _surface ?? new ImageSurface(bitImage ? Format.A1 : Format.Argb32, width, height);
            ctx = new Context(surface);
            if (flip)
            {
                ctx.Translate(0, Height);
                ctx.Scale(scale, -scale);
            }
            else
                ctx.Scale(scale, scale);
            ctx.SetSourceRGBA(0, 0.63529, 0.9098039, 1);
        }

        public void FillPolygon(IEnumerable<System.Windows.Point> points, bool clear = false)
        {
            ctx.LineWidth = 0;
            ctx.MoveTo(points.First().X, points.First().Y);
            foreach (var p in points.Except(new[] { points.First() }))
            {
                ctx.LineTo(p.X, p.Y);
            }
            ctx.ClosePath();
            Fill(clear);
        }

        public void DrawLine(double x0, double y0, double x1, double y1, double lineWidth, bool clear = false)
        {
            ctx.LineWidth = lineWidth;
            ctx.MoveTo(x0, y0);
            ctx.LineTo(x1, y1);
            if (clear)
                Subtract();
            else
                ctx.Stroke();
        }

        public void FillCircle(double x, double y, double radius, bool clear = false)
        {
            ctx.LineWidth = 0;
            ctx.Arc(x, y, radius, 0, 2 * Math.PI);
            Fill(clear);
        }

        public void FillRectangle(double x, double y, double width, double height, bool clear=false)
        {
            ctx.LineWidth = 0;
            ctx.Rectangle(x, y, width, height);
            Fill(clear);
        }

        private void Fill(bool clear)
        {
            if (clear)
                Subtract();
            else
                ctx.Fill();
        }

        private void Subtract()
        {
            var op = ctx.Operator;
            ctx.Operator = Operator.Clear;
            ctx.Fill();
            ctx.Operator = op;
        }

        public void FillRoundedRectangle(double x, double y, double width, double height, double radius, bool clear=false)
        {
            ctx.LineWidth = 0;
            ctx.Save();

            if ((radius > height / 2) || (radius > width / 2))
                radius = Math.Min(height / 2, width / 2);

            ctx.MoveTo(x, y + radius);
            ctx.Arc(x + radius, y + radius, radius, Math.PI, -Math.PI / 2);
            ctx.LineTo(x + width - radius, y);
            ctx.Arc(x + width - radius, y + radius, radius, -Math.PI / 2, 0);
            ctx.LineTo(x + width, y + height - radius);
            ctx.Arc(x + width - radius, y + height - radius, radius, 0, Math.PI / 2);
            ctx.LineTo(x + radius, y + height);
            ctx.Arc(x + radius, y + height - radius, radius, Math.PI / 2, Math.PI);
            ctx.ClosePath();
            Fill(clear);
            ctx.Restore();

        }

        public void SavePng(string filename)
        {
            ctx.Save();
            surface.WriteToPng(filename);

            ctx.Restore();
        }

    }
}
