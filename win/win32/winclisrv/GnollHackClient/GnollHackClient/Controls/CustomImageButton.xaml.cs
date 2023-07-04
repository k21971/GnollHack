﻿using GnollHackCommon;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace GnollHackClient.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CustomImageButton : ContentView
    {
        public CustomImageButton()
        {
            InitializeComponent();
            customCanvasView.InvalidateSurface();
            customButton.Pressed += (object sender, EventArgs args) =>
            {
                _isPressed = true;
                if(UseVaryingTextColors)
                    customButton.TextColor = !IsEnabled ? DisabledTextColor : _isPressed ? SelectedTextColor : NormalTextColor;
                customCanvasView.InvalidateSurface();
            };
            customButton.Released += (object sender, EventArgs args) =>
            {
                _isPressed = false;
                if (UseVaryingTextColors)
                    customButton.TextColor = !IsEnabled ? DisabledTextColor : _isPressed ? SelectedTextColor : NormalTextColor;
                customCanvasView.InvalidateSurface();
            };
            customButton.Unfocused += (object sender, FocusEventArgs args) =>
            {
                _isPressed = false;
                if (UseVaryingTextColors)
                    customButton.TextColor = !IsEnabled ? DisabledTextColor : _isPressed ? SelectedTextColor : NormalTextColor;
                customCanvasView.InvalidateSurface();
            };
            customButton.SizeChanged += (object sender, EventArgs args) =>
            {
                customCanvasView.InvalidateSurface();
            };
        }

        private bool _isPressed = false;

        public event EventHandler Clicked;

        public static readonly BindableProperty ButtonRelativeWidthProperty = BindableProperty.Create(nameof(ButtonRelativeWidthProperty), typeof(double), typeof(CustomImageButton), 10.0 / 12.0);
        public static readonly BindableProperty NormalTextColorProperty = BindableProperty.Create(nameof(NormalTextColorProperty), typeof(Color), typeof(CustomImageButton), Color.White);
        public static readonly BindableProperty SelectedTextColorProperty = BindableProperty.Create(nameof(SelectedTextColorProperty), typeof(Color), typeof(CustomImageButton), Color.White);
        public static readonly BindableProperty DisabledTextColorProperty = BindableProperty.Create(nameof(DisabledTextColorProperty), typeof(Color), typeof(CustomImageButton), Color.Gray);
        public static readonly BindableProperty UseVaryingTextColorsProperty = BindableProperty.Create(nameof(UseVaryingTextColorsProperty), typeof(bool), typeof(CustomImageButton), false);
        public static readonly BindableProperty UseVaryingBackgroundImagesProperty = BindableProperty.Create(nameof(UseVaryingBackgroundImages), typeof(bool), typeof(CustomImageButton), true);

        public double ButtonRelativeWidth
        {
            get => (double)GetValue(CustomImageButton.ButtonRelativeWidthProperty);
            set => SetValue(CustomImageButton.ButtonRelativeWidthProperty, value);
        }
        public Color TextColor
        {
            get { return (Color)customButton.GetValue(Button.TextColorProperty); }
            set { customButton.SetValue(Button.TextColorProperty, value); }
        }
        public Color NormalTextColor
        {
            get { return (Color)GetValue(NormalTextColorProperty); }
            set { SetValue(NormalTextColorProperty, value); }
        }
        public Color SelectedTextColor
        {
            get { return (Color)GetValue(SelectedTextColorProperty); }
            set { SetValue(SelectedTextColorProperty, value); }
        }
        public Color DisabledTextColor
        {
            get { return (Color)GetValue(DisabledTextColorProperty); }
            set { SetValue(DisabledTextColorProperty, value); }
        }
        public string Text
        {
            get { return (string)customButton.GetValue(Button.TextProperty); }
            set { customButton.SetValue(Button.TextProperty, value); }
        }
        public string FontFamily
        {
            get => (string)customButton.GetValue(Button.FontFamilyProperty);
            set { customButton.SetValue(Button.FontFamilyProperty, value); }
        }
        public double FontSize
        {
            get => (double)customButton.GetValue(Button.FontSizeProperty);
            set { customButton.SetValue(Button.FontSizeProperty, value); }
        }
        public new double WidthRequest
        {
            get { return (double)GetValue(CustomImageButton.WidthRequestProperty); }
            set { SetValue(CustomImageButton.WidthRequestProperty, value); customGrid.WidthRequest = value; customCanvasView.WidthRequest = value; customButton.WidthRequest = value * ButtonRelativeWidth; customCanvasView.InvalidateSurface(); }
        }
        public new double HeightRequest
        {
            get { return (double)GetValue(CustomImageButton.HeightRequestProperty); }
            set { SetValue(CustomImageButton.HeightRequestProperty, value); customGrid.HeightRequest = value; customCanvasView.HeightRequest = value; customButton.HeightRequest = value; customCanvasView.InvalidateSurface(); }
        }

        public new bool IsEnabled
        {
            get { return (bool)GetValue(CustomImageButton.IsEnabledProperty); }
            set { SetValue(CustomImageButton.IsEnabledProperty, value); customButton.IsEnabled = value; if (UseVaryingTextColors) { customButton.TextColor = !value ? DisabledTextColor : _isPressed ? SelectedTextColor : NormalTextColor; } customCanvasView.InvalidateSurface(); }
        }
        public bool UseVaryingTextColors
        {
            get { return (bool)GetValue(CustomImageButton.UseVaryingTextColorsProperty); }
            set { SetValue(CustomImageButton.UseVaryingTextColorsProperty, value); customButton.TextColor = value ? (!IsEnabled ? DisabledTextColor : _isPressed ? SelectedTextColor : NormalTextColor) : NormalTextColor; }
        }
        public bool UseVaryingBackgroundImages
        {
            get { return (bool)GetValue(CustomImageButton.UseVaryingBackgroundImagesProperty); }
            set { SetValue(CustomImageButton.UseVaryingBackgroundImagesProperty, value); customCanvasView.InvalidateSurface(); }
        }


        private void CustomButton_Clicked(object sender, EventArgs e)
        {
            Clicked?.Invoke(this, e);
        }

        private void CustomCanvasView_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            SKImageInfo info = e.Info;
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;
            float canvaswidth = customCanvasView.CanvasSize.Width;
            float canvasheight = customCanvasView.CanvasSize.Height;
            SKBitmap targetBitmap = !UseVaryingBackgroundImages ? App.ButtonNormalBitmap : !IsEnabled ? App.ButtonDisabledBitmap : _isPressed ? App.ButtonSelectedBitmap : App.ButtonNormalBitmap;
            if (targetBitmap == null)
                return;
            canvas.Clear();
            SKRect sourcerect = new SKRect(0, 0, targetBitmap.Width, targetBitmap.Height);
            SKRect targetrect = new SKRect(0, 0, canvaswidth, canvasheight);
            canvas.DrawBitmap(targetBitmap, sourcerect, targetrect);
        }
    }
}