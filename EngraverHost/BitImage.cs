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
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngraverHost
{
    public class BitImage
    {
        private BitArray Data { get; set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        private int Step { get; set; }

        public Bitmap ToBitmap()
        {
            var bmp = new Bitmap(Width, Height);
            for (int i = 0; i < Width; i++)
                for (int o = 0; o < Height; o++)
                    if(this[i, o])
                        bmp.SetPixel(i, o, Color.FromArgb(0, (int)(0.63529*255), (int)(0.9098039*255)));
            return bmp;
        }

        public BitImage(byte[] data, int width, int height, bool packed = true)
        {
            Data = new BitArray(data);
            Width = width;

            if (packed)
                Step = (int)Math.Ceiling(Math.Ceiling(((double)width / 8)) / 4) * 4 * 8;
            else
                Step = width;

            Height = height;
        }

        public bool this[int x, int y]
        {
            get
            {
                if (x >= Width || y >= Height)
                    throw new OverflowException();
                return Data[y * Step + x];
            }
            set
            {
                if (x >= Width || y >= Height)
                    throw new OverflowException();
                Data[y * Step + x] = value;
            }
        }

    }
}
