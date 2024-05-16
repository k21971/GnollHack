﻿using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;


#if GNH_MAUI
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using GnollHackX;
namespace GnollHackM
#else
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using GnollHackX.Pages.Game;
using SkiaSharp.Views.Forms;

namespace GnollHackX.Controls
#endif
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SwitchableCanvasView : ContentView
    {
        private object glLock = new object();
        private bool _useGL = false;
        public bool UseGL 
        {   get { lock (glLock) { return _useGL; } }
            set
            {
                lock(glLock)
                {
                    _useGL = value;
                }
                internalCanvasView.IsVisible = !value;
                internalGLView.IsVisible = value;
            }
        }

        public SwitchableCanvasView()
        {
            InitializeComponent();
        }

        public SKSize CanvasSize { get { return UseGL ? internalGLView.CanvasSize : internalCanvasView.CanvasSize; } }
        public bool IgnorePixelScaling
        {
            get { return UseGL ? false : internalCanvasView.IgnorePixelScaling; }
            set
            {
                internalCanvasView.IgnorePixelScaling = value;
            }
        }
        public bool EnableTouchEvents 
        { 
            get { return UseGL ? internalGLView.EnableTouchEvents : internalCanvasView.EnableTouchEvents; } 
            set 
            {
                internalGLView.EnableTouchEvents = value;
                internalCanvasView.EnableTouchEvents = value;
            }
        }
        public event EventHandler<SKPaintSurfaceEventArgs> PaintSurface;
        public event EventHandler<SKTouchEventArgs> Touch;
        public void InvalidateSurface()
        {
            if(UseGL)
                internalGLView.InvalidateSurface();
            else
                internalCanvasView.InvalidateSurface();
        }
        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
        {
            return new SizeRequest();
        }

        private void internalCanvasView_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            if (UseGL)
                return; /* Insurance in the case both canvases mistakenly are updated */

            PaintSurface?.Invoke(sender, e);
        }

        private void internalCanvasView_Touch(object sender, SKTouchEventArgs e)
        {
            Touch?.Invoke(sender, e);
        }

        private bool _firstDraw = true;

        private void internalGLView_PaintSurface(object sender, SKPaintGLSurfaceEventArgs e)
        {
            if (!UseGL)
                return; /* Insurance in the case both canvases mistakenly are updated */

            if(_firstDraw)
            {
                _firstDraw = false;
                if (CanvasType == CanvasTypes.MainCanvas)
                    GHApp.DefaultGPUCacheSize = ResourceCacheLimit;

                if (e.BackendRenderTarget != null)
                {
                    if(CanvasType == CanvasTypes.MainCanvas)
                        GHApp.GPUBackend = e.BackendRenderTarget.Backend.ToString();

                    Debug.WriteLine("Using is Skia GPU Rendering: GRContext Backend is " + e.BackendRenderTarget.Backend.ToString());
                }
                else
                    Debug.WriteLine("Using Skia GPU Rendering: BackendRenderTarget is null");

                /* Set to requested PrimaryCache limits */
                long defaultSize = GHApp.DefaultGPUCacheSize;
                if (CanvasType == CanvasTypes.MainCanvas)
                {
                    long limit = GHApp.PrimaryGPUCacheLimit;
                    Debug.WriteLine("PrimaryGPUCacheLimit is " + limit);
                    try
                    {
                        if (limit > 0)
                            internalGLView.GRContext.SetResourceCacheLimit(limit);
                        else if (limit == -2 && GHApp.RecommendedPrimaryGPUCacheSize > 0)
                            internalGLView.GRContext.SetResourceCacheLimit(GHApp.RecommendedPrimaryGPUCacheSize);
                        else if (limit == -3 && defaultSize > 0)
                            internalGLView.GRContext.SetResourceCacheLimit(defaultSize);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }

                    long newLimit = ResourceCacheLimit;
                    Debug.WriteLine("ResourceCacheSize is now " + newLimit);
                    GHApp.CurrentGPUCacheSize = newLimit;
                    GHApp.CurrentGPUCacheUsage = ResourceCacheUsage;
                }
                else
                {
                    long limit = GHApp.SecondaryGPUCacheLimit;
                    Debug.WriteLine("SecondaryGPUCacheLimit is " + limit);
                    try
                    {
                        if (limit > 0)
                            internalGLView.GRContext.SetResourceCacheLimit(limit);
                        else if (limit == -2 && GHApp.RecommendedSecondaryGPUCacheSize > 0)
                            internalGLView.GRContext.SetResourceCacheLimit(GHApp.RecommendedSecondaryGPUCacheSize);
                        else if (limit == -3 && defaultSize > 0)
                            internalGLView.GRContext.SetResourceCacheLimit(defaultSize);
                        Debug.WriteLine("ResourceCacheSize is now " + ResourceCacheLimit);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
            }

            /* Note: this case most likely will never happen, but is here still as a backup */
            if (_delayedResourceCacheLimit != -1 && internalGLView.GRContext != null)
            {
                Debug.WriteLine("CanvasType is " + CanvasType.ToString());
                Debug.WriteLine("ResourceCacheSize is " + ResourceCacheLimit);
                try
                {
                    if (_delayedResourceCacheLimit > 0)
                    {
                        Debug.WriteLine("_delayedResourceCacheLimit is " + _delayedResourceCacheLimit);
                        internalGLView.GRContext.SetResourceCacheLimit(_delayedResourceCacheLimit);
                        Debug.WriteLine("ResourceCacheSize is now " + ResourceCacheLimit);
                    }
                    else if (_delayedResourceCacheLimit == -2) /* Recommended */
                    {
                        long defaultSize = CanvasType == CanvasTypes.MainCanvas ? GHApp.RecommendedPrimaryGPUCacheSize : GHApp.RecommendedSecondaryGPUCacheSize;
                        Debug.WriteLine("_delayedResourceCacheLimit is " + _delayedResourceCacheLimit);
                        Debug.WriteLine("RecommendedGPUCacheSize is " + defaultSize);
                        if (defaultSize > 0)
                        {
                            internalGLView.GRContext.SetResourceCacheLimit(defaultSize);
                        }
                        Debug.WriteLine("ResourceCacheSize is now " + ResourceCacheLimit);
                    }
                    else if (_delayedResourceCacheLimit == -3) /* Skia Default */
                    {
                        long defaultSize = GHApp.DefaultGPUCacheSize;
                        Debug.WriteLine("_delayedResourceCacheLimit is " + _delayedResourceCacheLimit);
                        Debug.WriteLine("DefaultGPUCacheSize is " + defaultSize);
                        if (defaultSize > 0)
                        {
                            internalGLView.GRContext.SetResourceCacheLimit(defaultSize);
                        }
                        Debug.WriteLine("ResourceCacheSize is now " + ResourceCacheLimit);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
                _delayedResourceCacheLimit = -1;
                if (CanvasType == CanvasTypes.MainCanvas)
                    GHApp.CurrentGPUCacheSize = ResourceCacheLimit;
            }
            SKImageInfo info = new SKImageInfo();
            info.ColorType = e.ColorType;
            SKPaintSurfaceEventArgs convargs = new SKPaintSurfaceEventArgs(e.Surface, info);
            PaintSurface?.Invoke(sender, convargs);
        }

        private void internalGLView_Touch(object sender, SKTouchEventArgs e)
        {
            Touch?.Invoke(sender, e);
        }

        public GamePage _gamePage;
        public Grid _parentGrid;

        public GHWindow GHWindow { get; set; }
        public ghmenu_styles MenuStyle { get; set; }
        public readonly object MenuItemLock = new object();

        private ObservableCollection<GHMenuItem> _GHMenuItems = null;
        public ObservableCollection<GHMenuItem> MenuItems { get { return _GHMenuItems; } set { _GHMenuItems = value; } }

        public readonly object TextItemLock = new object();
        private List<GHPutStrItem> _GHPutStrItems = null;
        public List<GHPutStrItem> PutStrItems { get { return _GHPutStrItems; } set { _GHPutStrItems = value; } }

        public SelectionMode SelectionHow { get; set; }
        public int SelectionIndex { get; set; }

        public bool RevertBlackAndWhite { get; set; }
        public bool UseTextOutline { get; set; }
        public bool HideMenuLetters { get; set; }
        public bool MenuButtonStyle { get; set; }
        public bool ClickOKOnSelection { get; set; }
        public bool MenuGlyphAtBottom { get; set; }
        public bool AllowLongTap { get; set; } = true;
        public bool AllowHighlight { get; set; }

        private long _delayedResourceCacheLimit = -1L;
        public long ResourceCacheLimit
        {
            get 
            {
                try
                {
                    return internalGLView.GRContext != null ? internalGLView.GRContext.GetResourceCacheLimit() : -1;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
                return -1;
            }
            set
            {
                if(internalGLView.GRContext != null)
                {
                    try
                    {
                        switch (value)
                        {
                            case -3:
                                {
                                    long defaultSize = GHApp.DefaultGPUCacheSize;
                                    if (defaultSize > 0)
                                        internalGLView.GRContext.SetResourceCacheLimit(defaultSize);
                                    break;
                                }
                            case -2:
                                {
                                    long defaultSize = CanvasType == CanvasTypes.MainCanvas ? GHApp.RecommendedPrimaryGPUCacheSize : GHApp.RecommendedSecondaryGPUCacheSize;
                                    if (defaultSize > 0)
                                        internalGLView.GRContext.SetResourceCacheLimit(defaultSize);
                                    break;
                                }
                            default:
                                if (value > 0)
                                    internalGLView.GRContext.SetResourceCacheLimit(value);
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
                else
                {
                    _delayedResourceCacheLimit = value;
                }
            }
        }

        public CacheUsageInfo ResourceCacheUsage
        {
            get
            {
                CacheUsageInfo res = new CacheUsageInfo(-1, -1);
                if (internalGLView.GRContext != null)
                {
                    try
                    {
                        internalGLView.GRContext.GetResourceCacheUsage(out res.MaxResources, out res.MaxResourceBytes);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
                return res;
            }
        }

        public CanvasTypes CanvasType { get; set; }


        public BindableProperty GeneralAnimationCounterProperty =
            BindableProperty.Create(nameof(GeneralAnimationCounter), typeof(long), typeof(SwitchableCanvasView), 0L);

        public long GeneralAnimationCounter
        {
            get { return (long)GetValue(GeneralAnimationCounterProperty); }
            set { SetValue(GeneralAnimationCounterProperty, value); }
        }

        private long _tickCounter = 0L;

        protected override void OnPropertyChanged(string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            if (!IsVisible || (_parentGrid != null && !_parentGrid.IsVisible))
                return;
            if (_gamePage != null)
            {
                _tickCounter++;
                _tickCounter = _tickCounter % GHConstants.MaxRefreshRate;
                int auxRefreshRate = UIUtils.GetAuxiliaryCanvasAnimationFrequency();
                int mainfps = UIUtils.GetMainCanvasAnimationFrequency(_gamePage.MapRefreshRate);
                int divisor = Math.Max(1, (int)Math.Round((double)auxRefreshRate / (double)mainfps, 0));
                switch (CanvasType)
                {
                    case CanvasTypes.MainCanvas:
                        {
                            _gamePage.UpdateMainCanvas();
                            break;
                        }
                    case CanvasTypes.CommandCanvas:
                        {
                            _gamePage.UpdateCommandCanvas();
                            break;
                        }
                    case CanvasTypes.MenuCanvas:
                        {
                            if (_tickCounter % divisor == 0)
                                _gamePage.IncrementCounters();

                            _gamePage.UpdateMenuCanvas();
                            break;
                        }
                    case CanvasTypes.TextCanvas:
                        {
                            if (_tickCounter % divisor == 0)
                                _gamePage.IncrementCounters();

                            _gamePage.UpdateTextCanvas();
                            break;
                        }
                    default:
                        break;
                }
            }
        }
    }
}