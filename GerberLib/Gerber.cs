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
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace GerberLib
{
    public class Gerber
    {
        
        public class CircleAperture : IAperture
        {
            public double RX => Diameter / 2;
            public double RY => Diameter / 2;
            public double Diameter { get; private set; }
            public double? InnerDiameter { get; private set; }
            public CircleAperture(double diameter, double? innerDiameter)
            {
                Diameter = diameter;
                InnerDiameter = innerDiameter;
            }

            public void Render(IRenderer renderer, Point center, bool clearPolarity)
            {
                renderer.FillCircle(center.X, center.Y, RX, clearPolarity);
                if (InnerDiameter != null)
                    renderer.FillCircle(center.X, center.Y, InnerDiameter.Value / 2, !clearPolarity);
            }
        }

        public class RectangleAperture : IAperture
        {
            public double RX => X / 2;
            public double RY => Y / 2;
            public double X { get; private set; }
            public double Y { get; private set; }
            public double? InnerDiameter { get; private set; }
            public RectangleAperture(double x, double y, double? innerDiameter)
            {
                X = x;
                Y = y;
                InnerDiameter = innerDiameter;
            }

            protected virtual void RenderRectangle(IRenderer renderer, Point center, bool clearPolarity)
            {
                renderer.FillRectangle(center.X - RX, center.Y - RY, X, Y, clearPolarity);
            }

            public virtual void Render(IRenderer renderer, Point center, bool clearPolarity)
            {
                RenderRectangle(renderer, center, clearPolarity);
                if (InnerDiameter != null)
                    renderer.FillCircle(center.X, center.Y, InnerDiameter.Value / 2, !clearPolarity);
            }
        }

        public class ObroundAperture : RectangleAperture
        {
            public ObroundAperture(double x, double y, double? innerDiameter) : base(x, y, innerDiameter)
            {

            }
            protected override void RenderRectangle(IRenderer renderer, Point center, bool clearPolarity)
            {
                renderer.FillRoundedRectangle(center.X - RX, center.Y - RY, X, Y, Math.Min(RX, RY), clearPolarity);
            }
        }

        public enum Operations
        {
            Interpolation=1,
            Move=2,
            Flash=3
        }

        public interface ICommand
        {
            void Execute(IGerberState state);
        }

        public abstract class Operation : ICommand
        {
            public double? X { get; private set; }
            public double? Y { get; private set; }
            public Operations Type { get; private set; }

            public Operation(double? x, double? y, Operations type)
            {
                X = x;
                Y = y;
                Type = type;
            }

            public abstract void Execute(IGerberState state);

            protected Point GetPosition(IGerberState state)
            {
                return new Point(X ?? state.CurrentPosition.X, Y ?? state.CurrentPosition.Y);
            }

        }

        public class InterpolationOperation : Operation
        {
            public InterpolationOperation(double? x, double? y) : base(x, y, Operations.Interpolation) { }
            public override void Execute(IGerberState state)
            {
                var pos = GetPosition(state);
                state.LineTo(pos);
                state.SetCurrentPosition(pos);
            }
        }

        public class FlashOperation :Operation
        {
            public FlashOperation(double? x, double? y) : base(x, y, Operations.Flash) { }
            public override void Execute(IGerberState state)
            {
                var pos = GetPosition(state);
                state.FlashOn(pos);
                state.SetCurrentPosition(pos);
            }
        }

        public class MoveOperation : Operation
        {
            public MoveOperation(double? x, double? y) : base(x, y, Operations.Move) { }
            public override void Execute(IGerberState state) => state.MoveTo(GetPosition(state));
        }

        public class Region : ICommand
        {
            public IList<ICommand> Commands { get; private set; } = new List<ICommand>();

            public void Execute(IGerberState state)
            {
                var grs = new GerberRegionState(state);
                foreach (var c in Commands)
                    c.Execute(grs);
                grs.Finish();
            }
        }

        public class SetAperture : ICommand
        {
            public int Aperture { get; private set; }
            public SetAperture(int aperture)
            {
                Aperture = aperture;
            }
            public void Execute(IGerberState state) => state.SetAperture(Aperture);

        }

        public class SetPolarity : ICommand
        {
            public bool Clear { get; private set; }
            public SetPolarity(bool clear)
            {
                Clear = clear;
            }
            public void Execute(IGerberState state) => state.SetPolarity(Clear);
        }

        public int XInteger { get; private set; }
        public int XFraction { get; private set; }
        public int YInteger { get; private set; }
        public int YFraction { get; private set; }
        private Regex FsLaRegex { get; set; } = new Regex(@"^\%FSLAX(\d)(\d)Y(\d)(\d)\*\%", RegexOptions.Compiled);
        private Regex ApertureHeaderRegex { get; set; } = new Regex(@"^%ADD(\d+)([CROP]),", RegexOptions.Compiled);
        private Regex CApertureRegex { get; set; } = new Regex(@"^%ADD\d+C,(\d+\.?\d*)X*(\d+\.?\d*)?", RegexOptions.Compiled);
        private Regex RApertureRegex { get; set; } = new Regex(@"^%ADD\d+R,(\d+\.?\d*)X(\d+\.?\d*)X*(\d+\.?\d*)?", RegexOptions.Compiled);
        private Regex OApertureRegex { get; set; } = new Regex(@"^%ADD\d+O,(\d+\.?\d*)X(\d+\.?\d*)X*(\d+\.?\d*)?", RegexOptions.Compiled);
        private Regex OperationRegex { get; set; } = new Regex(@"^X?(-?\d+)?Y?(-?\d+)?D(\d+)*", RegexOptions.Compiled);

        private Regex GRegex { get; set; } = new Regex(@"^G(\d+)", RegexOptions.Compiled);
        private Regex DRegex { get; set; } = new Regex(@"^D(\d+)", RegexOptions.Compiled);

        private const double InchToMM = 25.4;

        public IList<ICommand> Commands { get; private set; } = new List<ICommand>();

        public IDictionary<int, IAperture> Appertures { get; private set; } = new Dictionary<int, IAperture>();

        public Gerber(string filename)
        {
            var lines = File.ReadLines(filename);
            var Inches = false;
            var multiQuadrant = false;
            Region currentRegion = null;
            foreach(var x in lines)
            {
                var line = x.ToUpper();
                if (line == "%MOIN*%") Inches = true;
                else if (line == "%MOMM*%") Inches = false;
                else if (GRegex.IsMatch(line))
                {
                    var type = GRegex.Match(line).Groups[1].Value.ToInt32();
                    switch (type)
                    {
                        case 70:
                            Inches = true;
                            break;
                        case 71:
                            Inches = false;
                            break;
                        case 74:
                            multiQuadrant = false;
                            break;
                        case 75:
                            multiQuadrant = true;
                            break;
                        case 2:
                        case 3:
                            throw new NotImplementedException("Circular interpolation is not supported");
                        case 36://region start;
                            currentRegion = new Region();
                            Commands.Add(currentRegion);
                            break;
                        case 37://region end;
                            currentRegion = null;
                            break;

                    }
                }
                else if (line.Contains("%ADD"))//aperture definition
                    ParseApperture(line, Inches);
                else if (line[0] == 'X' || line[0] == 'Y')
                {
                    var op = ParseOperation(line, Inches);
                    if (currentRegion == null)
                        Commands.Add(op);
                    else
                        currentRegion.Commands.Add(op);
                }
                else if (DRegex.IsMatch(line))
                {
                    var aperture = DRegex.Match(line).Groups[1].Value.ToInt32();
                    if(aperture == 3)
                    {
                        if (currentRegion == null)
                            Commands.Add(new FlashOperation(null, null));
                    }
                    else if(aperture == 2)
                    {
                        if (currentRegion != null)
                            Commands.Add(new MoveOperation(null, null));
                    }
                    else if (aperture < 10)
                        throw new InvalidOperationException($"Invalid Aperture: {aperture}");
                    else if (currentRegion == null)
                        Commands.Add(new SetAperture(aperture));
                }
                else if (line.Contains("%LPC*%"))
                {
                    if (currentRegion == null)
                        Commands.Add(new SetPolarity(true));
                }
                else if (line.Contains("%LPD*%"))
                {
                    if (currentRegion == null)
                        Commands.Add(new SetPolarity(false));
                }
                ParseFSLA(line);
            }
        }


        private double? parseNumber(string v, bool inches, bool x)
        {
            if (String.IsNullOrEmpty(v))
                return null;
            return (v.ToDouble() / Math.Pow(10, (x ? XFraction : YFraction))) * (inches ? InchToMM : 1);
        }

        private Operation ParseOperation(string line, bool inches)
        {
            var match = OperationRegex.Match(line);
            if (!match.Success)
                return null;
            var x = parseNumber(match.Groups[1].Value, inches, true);
            var y = parseNumber(match.Groups[2].Value, inches, false);
            var typeS = match.Groups[3].Value.ToInt32();
            switch (typeS)
            {
                case 1:
                    return new InterpolationOperation(x, y);
                case 2:
                    return new MoveOperation(x, y);
                case 3:
                    return new FlashOperation(x, y);
                default:
                    throw new InvalidOperationException($"Invalid Operation type: {typeS}");
            }
        }

        public IAperture ParseCircleAperture(string line, bool inches)
        {
            var cmatch = CApertureRegex.Match(line);
            var diameter = cmatch.Groups[1].Value.ToDouble() * (inches ? InchToMM : 1);
            double? innerDiameter = cmatch.Groups[2].ToDouble() * (inches ? InchToMM : 1);
            return new CircleAperture(diameter, innerDiameter);
        }

        public IAperture ParseRectangleApperture(string line, bool inches)
        {
            var cmatch = RApertureRegex.Match(line);
            var X = cmatch.Groups[1].Value.ToDouble() * (inches ? InchToMM : 1);
            var Y = cmatch.Groups[2].Value.ToDouble() * (inches ? InchToMM : 1);
            double? innerDiameter = cmatch.Groups[3].ToDouble() * (inches ? InchToMM : 1);
            return new RectangleAperture(X, Y, innerDiameter);
        }

        public IAperture ParseObroundApperture(string line, bool inches)
        {
            var cmatch = OApertureRegex.Match(line);
            var X = cmatch.Groups[1].Value.ToDouble() * (inches ? InchToMM : 1);
            var Y = cmatch.Groups[2].Value.ToDouble() * (inches ? InchToMM : 1);
            double? innerDiameter = cmatch.Groups[3].ToDouble() * (inches ? InchToMM : 1);
            return new RectangleAperture(X, Y, innerDiameter);
        }

        private void ParseApperture(string line, bool inches)
        {
            var match = ApertureHeaderRegex.Match(line);
            if (!match.Success)
                throw new NotImplementedException("Unsupported aperture: " + line);
            var id = match.Groups[1].Value.ToInt32();
            var type = match.Groups[2].Value;
            if (type == "P") throw new NotImplementedException("Polygon aperture is not supported");
            switch (type)
            {
                case "C":
                    Appertures.Add(id, ParseCircleAperture(line, inches));
                    break;
                case "R":
                    Appertures.Add(id, ParseRectangleApperture(line, inches));
                    break;
                case "O":
                    Appertures.Add(id, ParseObroundApperture(line, inches));
                    break;
                case "P":
                    throw new NotImplementedException("Polygon aperture is not supported");
            }
        }

        private void ParseFSLA(string line)
        {
            var m = FsLaRegex.Match(line);
            if (m.Success)
            {
                XInteger = m.Groups[1].Value.ToInt32();
                XFraction = m.Groups[2].Value.ToInt32();
                YInteger = m.Groups[3].Value.ToInt32();
                YFraction = m.Groups[4].Value.ToInt32();
            }
        }
    }
}
