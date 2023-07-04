﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GnollHackClient.Pages.Game;
using GnollHackCommon;
using SkiaSharp;
using Xamarin.Forms;

namespace GnollHackClient
{
    public class GlyphImageSource : StreamImageSource
    {
        public GlyphImageSource()
        {

        }

        private int _canvasWidth = 0;
        private int _canvasHeight = 0;

        public int CanvasWidth { get { return _canvasWidth == 0 ? Width :_canvasWidth; } set { _canvasWidth = value; } }
        public int CanvasHeight { get { return _canvasHeight == 0 ? Height :_canvasHeight; } set { _canvasHeight = value; } }
        public int CanvasXStart { get; set; }

        public override bool IsEmpty => (ReferenceGamePage == null || Glyph == 0 || Glyph >= App.Glyph2Tile.Length);

        protected override void OnPropertyChanged(string propertyName)
        {
            if (propertyName == GlyphProperty.PropertyName ||
                propertyName == ObjDataProperty.PropertyName ||
                propertyName == GrayedProperty.PropertyName ||
                propertyName == OpacityProperty.PropertyName ||
                propertyName == GamePageProperty.PropertyName ||
                propertyName == WidthProperty.PropertyName ||
                propertyName == HeightProperty.PropertyName)
            {
                OnSourceChanged();
            }
        }

        public override Func<CancellationToken, Task<Stream>> Stream => GetStreamAsync;

        public static readonly BindableProperty GlyphProperty = BindableProperty.Create(
            "Glyph", typeof(int), typeof(GlyphImageSource));

        public int Glyph
        {
            get => (int)GetValue(GlyphProperty);
            set => SetValue(GlyphProperty, value);
        }

        public static readonly BindableProperty UseUpperSideProperty = BindableProperty.Create(
            "UseUpperSide", typeof(bool), typeof(GlyphImageSource));

        public bool UseUpperSide
        {
            get => (bool)GetValue(UseUpperSideProperty);
            set => SetValue(UseUpperSideProperty, value);
        }

        public static readonly BindableProperty GrayedProperty = BindableProperty.Create(
            "Grayed", typeof(bool), typeof(GlyphImageSource));

        public bool Grayed
        {
            get => (bool)GetValue(GrayedProperty);
            set => SetValue(GrayedProperty, value);
        }

        public static readonly BindableProperty OpacityProperty = BindableProperty.Create(
            "Opacity", typeof(float), typeof(GlyphImageSource), 1.0f);

        public float Opacity
        {
            get => (float)GetValue(OpacityProperty);
            set => SetValue(OpacityProperty, value);
        }

        public static readonly BindableProperty AutoSizeProperty = BindableProperty.Create(
            "AutoSize", typeof(bool), typeof(GlyphImageSource));

        public bool AutoSize
        {
            get => (bool)GetValue(AutoSizeProperty);
            set => SetValue(AutoSizeProperty, value);
        }

        //public static readonly BindableProperty XProperty = BindableProperty.Create(
        //    "X", typeof(int), typeof(GlyphImageSource));

        //public int X
        //{
        //    get => (int)GetValue(XProperty);
        //    set => SetValue(XProperty, value);
        //}

        //public static readonly BindableProperty YProperty = BindableProperty.Create(
        //    "Y", typeof(int), typeof(GlyphImageSource));

        //public int Y
        //{
        //    get => (int)GetValue(YProperty);
        //    set => SetValue(YProperty, value);
        //}

        public static readonly BindableProperty WidthProperty = BindableProperty.Create(
            "Width", typeof(int), typeof(GlyphImageSource));

        public int Width
        {
            get => (int)GetValue(WidthProperty);
            set => SetValue(WidthProperty, value);
        }

        public static readonly BindableProperty HeightProperty = BindableProperty.Create(
            "Height", typeof(int), typeof(GlyphImageSource));

        public int Height
        {
            get => (int)GetValue(HeightProperty);
            set => SetValue(HeightProperty, value);
        }

        public static readonly BindableProperty GamePageProperty = BindableProperty.Create(
            "ReferenceGamePage", typeof(GamePage), typeof(GlyphImageSource));

        public GamePage ReferenceGamePage
        {
            get => (GamePage)GetValue(GamePageProperty);
            set => SetValue(GamePageProperty, value);
        }

        public static readonly BindableProperty ObjDataProperty = BindableProperty.Create(
            "ObjData", typeof(ObjectDataItem), typeof(GlyphImageSource));

        public ObjectDataItem ObjData
        {
            get => (ObjectDataItem)GetValue(ObjDataProperty);
            set => SetValue(ObjDataProperty, value);
        }

        public Task<Stream> GetStreamAsync(CancellationToken userToken = new CancellationToken())
        {
            base.OnLoadingStarted();
            userToken.Register(CancellationTokenSource.Cancel);
            var result = Draw();
            OnLoadingCompleted(CancellationTokenSource.IsCancellationRequested);
            return Task.FromResult(result);

        }

        public SKRect MeasureBitmapCroppingRect(SKBitmap bitmap)
        {
            byte[] bytes = bitmap.Bytes;
            int bytesperpixel = bitmap.BytesPerPixel;
            int bytesperrow = bitmap.RowBytes;
            int left = 0, right = 0, top = 0, bottom = 0;

            bool nonzero = false;
            for(int y = 0; y < bitmap.Height; y++)
            {

                for (int x = 0; x < bitmap.Width; x++)
                {
                    if (bytes[y * bytesperrow + x * bytesperpixel] != 0)
                    {
                        nonzero = true;
                        break;
                    }
                }
                if (nonzero)
                {
                    top = y;
                    break;
                }
            }
            if (!nonzero)
                top = 0;

            nonzero = false;
            for (int y = bitmap.Height - 1; y >= 0; y--)
            {

                for (int x = 0; x < bitmap.Width; x++)
                {
                    if (bytes[y * bytesperrow + x * bytesperpixel] != 0)
                    {
                        nonzero = true;
                        break;
                    }
                }
                if (nonzero)
                {
                    bottom = y;
                    break;
                }
            }
            if (!nonzero)
                bottom = bitmap.Height - 1;

            nonzero = false;
            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {

                    if (bytes[y * bytesperrow + x * bytesperpixel] != 0)
                    {
                        nonzero = true;
                        break;
                    }
                }
                if (nonzero)
                {
                    left = x;
                    break;
                }
            }
            if (!nonzero)
                right = 0;

            nonzero = false;
            for (int x = bitmap.Width - 1; x >= 0; x--)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    if (bytes[y * bytesperrow + x * bytesperpixel] != 0)
                    {
                        nonzero = true;
                        break;
                    }
                }
                if (nonzero)
                {
                    right = x;
                    break;
                }
            }
            if (!nonzero)
                right = bitmap.Width - 1;

            return new SKRect((float)left, (float)top, (float)right, (float)bottom);
        }

        private Stream Draw()
        {
            int signed_glyph = Glyph;
            int abs_glyph = Math.Abs(signed_glyph);

            if (ReferenceGamePage == null || abs_glyph <= 0 || abs_glyph >= App.Glyph2Tile.Length)
            {
                if (AutoSize)
                {
                    CanvasWidth = 1;
                    CanvasHeight = 1;
                    Width = 1;
                    Height = 1;
                }
                var bitmaptmp = new SKBitmap(Width, Height, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
                var canvastmp = new SKCanvas(bitmaptmp);
                canvastmp.Clear(SKColors.Transparent);
                var skImagetmp = SKImage.FromBitmap(bitmaptmp);
                var resulttmp = skImagetmp.Encode(SKEncodedImageFormat.Png, 100).AsStream();
                return resulttmp;
            }

            App.DebugWriteProfilingStopwatchTimeAndStart("GlyphSource Start");

            DoAutoSize();

            var bitmap = new SKBitmap(Width, Height, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
            var canvas = new SKCanvas(bitmap);
            canvas.Clear(SKColors.Transparent);

            DrawOnCanvas(canvas);

            var skImage = SKImage.FromBitmap(bitmap);            
            var result = skImage.Encode(SKEncodedImageFormat.Png, 100).AsStream();

            App.DebugWriteProfilingStopwatchTimeAndStop("GlyphSource Stop");

            return result;
        }
        public void DoAutoSize()
        {
            if (AutoSize)
            {
                int signed_glyph = Glyph;
                int abs_glyph = Math.Abs(signed_glyph);

                CanvasXStart = 0;
                bool tileflag_halfsize = (App.GlyphTileFlags[abs_glyph] & (byte)glyph_tile_flags.GLYPH_TILE_FLAG_HALF_SIZED_TILE) != 0;
                if (tileflag_halfsize)
                {
                    CanvasWidth = Width = GHConstants.TileWidth;
                    CanvasHeight = Height = GHConstants.TileHeight / 2;
                }
                else
                {
                    int ntile = App.Glyph2Tile[abs_glyph];
                    int enlargement = App.Tile2Enlargement[ntile];

                    if (enlargement == 0)
                    {
                        CanvasWidth = Width = GHConstants.TileWidth;
                        CanvasHeight = Height = GHConstants.TileHeight;
                    }
                    else
                    {
                        CanvasWidth = GHConstants.TileWidth * App.Enlargements[enlargement].width_in_tiles;
                        Height = CanvasHeight = GHConstants.TileHeight * App.Enlargements[enlargement].height_in_tiles;
                        if(CanvasWidth == 3 * GHConstants.TileWidth && (App.GlyphTileFlags[abs_glyph] & (byte)glyph_tile_flags.GLYPH_TILE_FLAG_TWO_WIDE_CENTERED) != 0)
                        {
                            CanvasXStart = GHConstants.TileWidth / 2;
                            Width = 2 * GHConstants.TileWidth;
                        }
                        else
                        {
                            Width = CanvasWidth;
                        }
                    }
                }
            }
        }

        public void DrawOnCanvas(SKCanvas canvas)
        {
            int signed_glyph = Glyph;
            int abs_glyph = Math.Abs(signed_glyph);

            if (ReferenceGamePage != null && abs_glyph > 0 && CanvasWidth > 0 && CanvasHeight > 0 && abs_glyph < App.Glyph2Tile.Length)
            {
                bool drawwallends = ReferenceGamePage.DrawWallEnds;
                byte glyphflags = App.GlyphTileFlags[abs_glyph];
                bool tileflag_halfsize = (glyphflags & (byte)glyph_tile_flags.GLYPH_TILE_FLAG_HALF_SIZED_TILE) != 0;
                bool tileflag_fullsizeditem = (glyphflags & (byte)glyph_tile_flags.GLYPH_TILE_FLAG_FULL_SIZED_ITEM) != 0;
                int ntile = App.Glyph2Tile[abs_glyph];
                int autodraw = App.Tile2Autodraw[ntile];
                int anim_frame_idx = 0, main_tile_idx = 0;
                sbyte mapAnimated = 0;
                long counter_value = 0;
                int tile_animation_idx = ReferenceGamePage.GnollHackService.GetTileAnimationIndexFromGlyph(abs_glyph);
                lock (ReferenceGamePage.AnimationTimerLock)
                {
                    counter_value = ReferenceGamePage.AnimationTimers.general_animation_counter;
                }
                ntile = ReferenceGamePage.GnollHackService.GetAnimatedTile(ntile, tile_animation_idx, (int)animation_play_types.ANIMATION_PLAY_TYPE_ALWAYS, counter_value, out anim_frame_idx, out main_tile_idx, out mapAnimated, ref autodraw);

                int enlargement_idx = App.Tile2Enlargement[ntile];
                int sheet_idx = App.TileSheetIdx(ntile);
                int tile_x = App.TileSheetX(ntile);
                int tile_y = App.TileSheetY(ntile);

                using (SKPaint paint = new SKPaint())
                {
                    if (Grayed)
                    {
                        paint.ColorFilter =
                            SKColorFilter.CreateColorMatrix(new float[]
                            {
                                0.21f, 0.72f, 0.07f, 0, 0,
                                0.21f, 0.72f, 0.07f, 0, 0,
                                0.21f, 0.72f, 0.07f, 0, 0,
                                0,     0,     0,     1, 0
                            });
                    }

                    paint.Color = paint.Color.WithAlpha((byte)(0xFF * Math.Min(1.0f, Math.Max(0.0f, Opacity))));

                    if (enlargement_idx == 0)
                    {
                        float scale = CanvasWidth / (float)GHConstants.TileWidth;
                        float tileWidth = CanvasWidth;
                        float tileHeight = CanvasHeight;
                        float xpadding = 0;
                        float ypadding = 0;
                        float scaled_tile_height = CanvasHeight;
                        SKRect sourcerect;
                        if (tileflag_halfsize)
                        {
                            if (UseUpperSide)
                                sourcerect = new SKRect(tile_x, tile_y, tile_x + (float)GHConstants.TileWidth, tile_y + (float)GHConstants.TileHeight / 2);
                            else
                                sourcerect = new SKRect(tile_x, tile_y + (float)GHConstants.TileHeight / 2, tile_x + (float)GHConstants.TileWidth, tile_y + (float)GHConstants.TileHeight);
                        }
                        else
                        {
                            sourcerect = new SKRect(tile_x, tile_y, tile_x + (float)GHConstants.TileWidth, tile_y + (float)GHConstants.TileHeight);
                        }

                        SKRect targetrect;
                        if (tileflag_halfsize)
                        {
                            targetrect = new SKRect(0, 0, CanvasWidth, CanvasHeight);
                        }
                        else
                        {
                            float fullsizewidth = Height / (float)GHConstants.TileHeight * (float)GHConstants.TileWidth;
                            float fullsizepadding = Math.Max(0, CanvasWidth - fullsizewidth) / 2;
                            targetrect = new SKRect(fullsizepadding - CanvasXStart, 0, fullsizepadding + fullsizewidth - CanvasXStart, Height);
                            xpadding = fullsizepadding;
                            tileWidth = fullsizewidth;
                            scale = tileWidth / GHConstants.TileWidth;
                        }

                        canvas.DrawBitmap(ReferenceGamePage.TileMap[sheet_idx], sourcerect, targetrect, paint);
                        ReferenceGamePage.DrawAutoDraw(autodraw, canvas, paint, ObjData,
                            (int)layer_types.LAYER_OBJECT, 0, 0,
                            tileflag_halfsize, false, tileflag_fullsizeditem,
                            0, 0, tileWidth, tileHeight,
                            1, scale, xpadding, ypadding, scaled_tile_height, true, drawwallends);
                    }
                    else
                    {
                        bool flip_tile = (signed_glyph < 0);
                        sbyte enl_height = App.Enlargements[enlargement_idx].height_in_tiles;
                        sbyte enl_width = App.Enlargements[enlargement_idx].width_in_tiles;
                        sbyte enl_x = (sbyte)(enl_width == 1 ? 0 : enl_width == 3 ? 1 : App.Enlargements[enlargement_idx].main_tile_x_coordinate);
                        sbyte flipped_enl_x = (sbyte)(enl_width == 1 ? 0 : enl_width == 3 ? 1 : 1 - App.Enlargements[enlargement_idx].main_tile_x_coordinate);

                        int width = GHConstants.TileWidth * enl_width;
                        int height = GHConstants.TileHeight * enl_height;
                        float relsizex = width / CanvasWidth;
                        float relsizey = height / CanvasHeight;
                        float scale = Math.Min(1 / relsizex, 1 / relsizey);
                        float targetimagewidth = scale * width;
                        float targetimageheight = scale * height;
                        float tileWidth = scale * GHConstants.TileWidth;
                        float tileHeight = scale * GHConstants.TileHeight;
                        float xpadding = Math.Max(0, (CanvasWidth - targetimagewidth) / 2);
                        float ypadding = Math.Max(0, (CanvasHeight - targetimageheight) / 2);
                        float t_x = xpadding + enl_x * tileWidth - CanvasXStart;
                        float t_y = ypadding + tileHeight * (enl_height - 1);
                        int n_sheet_idx = sheet_idx;

                        /* Main tile */
                        using (new SKAutoCanvasRestore(canvas, true))
                        {
                            canvas.Translate(t_x + (flip_tile ? tileWidth * (1 + flipped_enl_x - enl_x) : 0), t_y);
                            canvas.Scale(flip_tile ? -1 : 1, 1, 0, 0);
                            SKRect sourcerect = new SKRect(tile_x, tile_y, tile_x + GHConstants.TileWidth, tile_y + GHConstants.TileHeight);
                            SKRect targetrect = new SKRect(0, 0, tileWidth, tileHeight);
                            canvas.DrawBitmap(ReferenceGamePage.TileMap[sheet_idx], sourcerect, targetrect, paint);
                            ReferenceGamePage.DrawAutoDraw(autodraw, canvas, paint, ObjData,
                                (int)layer_types.LAYER_OBJECT, 0, 0,
                                tileflag_halfsize, false, true,
                                0, 0, tileWidth, tileHeight,
                                1, scale, 0, 0, tileHeight, true, drawwallends);

                        }

                        /* Enlargement tiles */
                        for (int idx = 0; idx < GHConstants.NumPositionsInEnlargement; idx++)
                        {
                            if (enl_height == 1 && idx < 3)
                                continue;

                            if (enl_width == 2 && enl_x == 0 && (idx == 0 || idx == 3))
                                continue;

                            if (enl_width == 2 && enl_x == 1 && (idx == 2 || idx == 4))
                                continue;

                            int enltile = (int)App.Enlargements[enlargement_idx].position2tile[idx];
                            if (enltile >= 0)
                            {
                                int glyph = enltile + App.EnlargementOffsets[enlargement_idx] /* enlargements[enlargement_idx].glyph_offset */ + App.EnlargementOff;
                                int etile = App.Glyph2Tile[glyph];
                                int e_sheet_idx = App.TileSheetIdx(etile);
                                int etile_x = App.TileSheetX(etile);
                                int etile_y = App.TileSheetY(etile);
                                float target_x = 0;
                                float target_y = 0;
                                autodraw = App.Tile2Autodraw[etile];

                                if (enl_height == 2)
                                {
                                    target_y = idx < 3 ? 0 : tileHeight;
                                }

                                if (enl_width == 2 && enl_x == 0)
                                {
                                    if (flip_tile)
                                        target_x = idx == 1 ? tileWidth : 0;
                                    else
                                        target_x = idx == 1 ? 0 : tileWidth;
                                }
                                else if (enl_width == 2 && enl_x == 1)
                                {
                                    if (flip_tile)
                                        target_x = idx == 1 ? 0 : tileWidth;
                                    else
                                        target_x = idx == 1 ? tileWidth : 0;
                                }
                                else if (enl_width == 3)
                                {
                                    if (flip_tile)
                                        target_x = idx == 0 || idx == 3 ? 2 * tileWidth : idx == 1 ? tileWidth : 0;
                                    else
                                        target_x = idx == 0 || idx == 3 ? 0 : idx == 1 ? tileWidth : 2 * tileWidth;
                                }

                                target_x += xpadding - CanvasXStart;
                                target_y += ypadding;

                                using (new SKAutoCanvasRestore(canvas, true))
                                {
                                    canvas.Translate(target_x + (flip_tile ? tileWidth : 0), target_y);
                                    canvas.Scale(flip_tile ? -1 : 1, 1, 0, 0);
                                    SKRect sourcerect = new SKRect(etile_x, etile_y, etile_x + GHConstants.TileWidth, etile_y + GHConstants.TileHeight);
                                    SKRect targetrect = new SKRect(0, 0, tileWidth, tileHeight);
                                    canvas.DrawBitmap(ReferenceGamePage.TileMap[e_sheet_idx], sourcerect, targetrect, paint);
                                    ReferenceGamePage.DrawAutoDraw(autodraw, canvas, paint, ObjData,
                                        (int)layer_types.LAYER_OBJECT, 0, 0,
                                        tileflag_halfsize, false, true,
                                        0, 0, tileWidth, tileHeight,
                                        1, scale, 0, 0, tileHeight, true, drawwallends);
                                }
                            }
                        }
                    }
                }
            }

        }
    }
}
