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
using System.Windows.Media;

namespace GerberLib
{
    public class GerberToXaml
    {
        public static System.Windows.Shapes.Path Convert(IList<GerberObject> objects)
        {
            Geometry currentGeometry = null;
            foreach (var obj in objects)
            {
                Geometry geo = null;
                if (obj is GerberLine)
                {
                    var gl = obj as GerberLine;
                    var lineWidth = 0.0;
                    if(gl.Aperture is Gerber.CircleAperture)
                    {
                        lineWidth = (gl.Aperture as Gerber.CircleAperture).Diameter;
                    }
                    else if(gl.Aperture is Gerber.RectangleAperture)
                    {
                        var ra = gl.Aperture as Gerber.RectangleAperture;
                        lineWidth = Math.Min(ra.X, ra.Y);
                    }
                    double dx = 0, dy = 0;
                    Point start = gl.Start;
                    Point end = gl.End;
                    /*if(start.Y > end.Y)
                    {
                        start = gl.End;
                        end = gl.Start;
                    }*/
                    if(start.X == end.X && start.Y != end.Y)
                    {
                        dx = lineWidth / 2;
                    }
                    else if(start.Y == end.Y && start.X != end.X)
                    {
                        dy = lineWidth / 2;
                    }
                    else
                    {
                        var angle = Math.Atan((end.Y - start.Y) / (end.X - start.X));
                        dx = Math.Sin(angle) * lineWidth/2;
                        dy = Math.Cos(angle) * lineWidth/2;
                        //dx = dy = lineWidth / 2;
                    }
                    var lineSegments = new[]
                    {
                        new LineSegment(new Point(start.X+dx, start.Y-dy), false),
                        new LineSegment(new Point(end.X+dx, end.Y-dy), false),
                        new LineSegment(new Point(end.X-dx, end.Y+dy), false),
                    };
                    
                    var lineFig = new PathFigure(new Point(start.X - dx, start.Y + dy), lineSegments, true);
                    geo = new PathGeometry(new[] { lineFig });
                    /*var Sx = Math.Sqrt(Math.Pow(end.X - start.X, 2) + Math.Pow(end.Y - start.Y, 2));
                    var Sy = lineWidth;
                    var rect = new RectangleGeometry(new Rect(start, new Size(Sx, Sy)), 0, 0, new RotateTransform(180 * Math.Atan2(end.Y - start.Y, end.X - start.X) / Math.PI));
                    geo = rect;*/
                    geo = Geometry.Combine(geo, DrawAperture(new GerberFlash(start, gl.Aperture, gl.ClearPolarity)), GeometryCombineMode.Union, null);
                    geo = Geometry.Combine(geo, DrawAperture(new GerberFlash(end, gl.Aperture, gl.ClearPolarity)), GeometryCombineMode.Union, null);
                }
                else if (obj is GerberFlash)
                {
                    var gf = obj as GerberFlash;

                    geo = DrawAperture(gf);
                    if (gf.Aperture.InnerDiameter != null)
                        geo = Geometry.Combine(geo, new EllipseGeometry(gf.Center, gf.Aperture.InnerDiameter.Value / 2, gf.Aperture.InnerDiameter.Value / 2), GeometryCombineMode.Exclude, null);
                        //geo = new CombinedGeometry(GeometryCombineMode.Exclude, geo, new EllipseGeometry(gf.Center, gf.Aperture.InnerDiameter.Value / 2, gf.Aperture.InnerDiameter.Value / 2));
                }
                else if (obj is GerberPolygon)
                {
                    var poly = obj as GerberPolygon;
                    var fig = new PathFigure(poly.Points.First(), poly.Points.Except(new[] { poly.Points.First() }).Select(x => new LineSegment(x, false)), true);
                    geo = new PathGeometry(new[] { fig });
                }

                if (geo != null)
                {
                    if (currentGeometry == null)
                    {
                        if (obj.ClearPolarity)
                            throw new InvalidOperationException("First element can't be of clear polarity");
                        currentGeometry = geo;
                    }
                    else
                    {
                        if (obj.ClearPolarity)
                        {
                            //currentGeometry = new CombinedGeometry(GeometryCombineMode.Exclude, currentGeometry, geo);
                            currentGeometry = Geometry.Combine(currentGeometry, geo, GeometryCombineMode.Exclude, null);
                        }
                        else
                        {
                            /*if(currentGeometry is GeometryGroup)
                            {
                                (currentGeometry as GeometryGroup).Children.Add(geo);
                            }
                            else
                            {
                                var gg = new GeometryGroup();
                                gg.FillRule = FillRule.Nonzero;
                                gg.Children.Add(currentGeometry);
                                gg.Children.Add(geo);
                                currentGeometry = gg;
                            }*/
                            currentGeometry = Geometry.Combine(currentGeometry, geo, GeometryCombineMode.Union, null);
                            //currentGeometry = new CombinedGeometry(obj.ClearPolarity ? GeometryCombineMode.Exclude : GeometryCombineMode.Union, currentGeometry, geo);
                        }

                    }
                }

            }
            var path = new System.Windows.Shapes.Path();
            path.Data = currentGeometry;
            path.StrokeThickness = 0;
            path.Fill = Brushes.Black;
            return path;
        }

        private static Geometry DrawAperture(GerberFlash gf)
        {
            if (gf.Aperture is Gerber.CircleAperture)
            {
                var ca = gf.Aperture as Gerber.CircleAperture;
                return new EllipseGeometry(gf.Center, ca.Diameter / 2, ca.Diameter / 2);
            }
            else if (gf.Aperture is Gerber.RectangleAperture)
            {
                var ra = gf.Aperture as Gerber.RectangleAperture;
                var rg = new RectangleGeometry(new System.Windows.Rect(new System.Windows.Point(gf.Center.X - ra.X / 2, gf.Center.Y - ra.Y / 2), new System.Windows.Size(ra.X, ra.Y)));
                if (ra is Gerber.ObroundAperture)
                {
                    var oa = gf.Aperture as Gerber.ObroundAperture;
                    var radius = Math.Min(oa.X, oa.Y) / 2;
                    rg.RadiusX = radius;
                    rg.RadiusY = radius;
                }
                return rg;
            }
            return null;
        }
    }
}
