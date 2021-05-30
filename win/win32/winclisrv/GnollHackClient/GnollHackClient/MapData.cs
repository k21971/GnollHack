﻿using System;
using System.Collections.Generic;
using System.Text;
using SkiaSharp;

namespace GnollHackClient
{
    public class MapData
    {
        public int Glyph { get; set; }
        public int BkGlyph { get; set; }
        public string Symbol { get; set; }
        public SKColor Color { get; set; }

        public MapData()
        {

        }
    }

}