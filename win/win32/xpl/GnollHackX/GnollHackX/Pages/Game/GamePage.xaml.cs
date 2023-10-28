﻿using SkiaSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Reflection;
using System.IO;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Collections;

#if GNH_MAUI
using GnollHackX;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace GnollHackM
#else
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;
using GnollHackX.Controls;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
using Xamarin.Forms.PlatformConfiguration;
using SkiaSharp.Views.Forms;
using static Xamarin.Essentials.Permissions;

namespace GnollHackX.Pages.Game
#endif
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GamePage : ContentPage
    {
        private readonly object _canvasButtonLock = new object();
        private SKRect _canvasButtonRect = new SKRect(0, 0, 0, 0);
        private SKColor _cursorDefaultGreen = new SKColor(0, 255, 0);

        private const float _statusbar_hmargin = 5.0f;
        private const float _statusbar_vmargin = 5.0f;
        private const float _statusbar_rowmargin = 5.0f;
        private const float _statusbar_basefontsize = 14f;
        private const float _statusbar_shieldfontsize = _statusbar_basefontsize * 32f / 42f;
        private const float _statusbar_diffontsize = _statusbar_basefontsize * 24f / 42f;

        private object _isGameOnLock = new object();
        private bool _isGameOn = false;
        public bool IsGameOn { get { lock (_isGameOnLock) { return _isGameOn; } } set { lock (_isGameOnLock) { _isGameOn = value; } } }

        private readonly string _fontSizeString = "FontS";
        private bool _refreshMsgHistoryRowCounts = true;
        private readonly object _refreshMsgHistoryRowCountLock = new object();
        private bool RefreshMsgHistoryRowCounts { get { lock (_refreshMsgHistoryRowCountLock) { return _refreshMsgHistoryRowCounts; } } set { lock (_refreshMsgHistoryRowCountLock) { _refreshMsgHistoryRowCounts = value; } } }

        public List<string> ExtendedCommands { get; set; }

        private IGnollHackService _gnollHackService;
        public IGnollHackService GnollHackService { get { return _gnollHackService; } }
        private bool _isFirstAppearance = true;
        private Thread _gnhthread;
        private GHGame _currentGame;
        public GHGame CurrentGame { get { return _currentGame; } }

        private MapData[,] _mapData = new MapData[GHConstants.MapCols, GHConstants.MapRows];
        private readonly object _mapDataLock = new object();
        private int _mapCursorX;
        private int _mapCursorY;

        private readonly object _uLock = new object();
        private int _ux = 0;
        private int _uy = 0;
        private ulong _u_condition_bits = 0;
        private ulong _u_status_bits = 0;
        private ulong[] _u_buff_bits = new ulong[GHConstants.NUM_BUFF_BIT_ULONGS];
        private int[] _statusmarkorder = { (int)game_ui_status_mark_types.STATUS_MARK_TOWNGUARD_PEACEFUL, (int)game_ui_status_mark_types.STATUS_MARK_TOWNGUARD_HOSTILE, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21 };
        public string[] _condition_names = new string[(int)bl_conditions.NUM_BL_CONDITIONS] {
            "Petrifying",
            "Slimed",
            "Being strangled",
            "Suffocating",
            "Food poisoned",
            "Terminally ill",
            "Blind",
            "Deaf",
            "Stun",
            "Confused",
            "Hallucinating",
            "Levitating",
            "Flying",
            "Riding",
            "Slowed",
            "Paralyzed",
            "Frightened",
            "Sleeping",
            "Cancelled",
            "Silenced",
            "Grabbed",
            "Mummy rot",
            "Lycanthropy",
        };

        public string[] _status_names = new string[(int)game_ui_status_mark_types.MAX_STATUS_MARKS] {
            "Pet",
            "Peaceful",
            "Detected",
            "Object pile",
            "Satiated",
            "Hungry",
            "Weak",
            "Fainting",
            "Burdended",
            "Stressed",
            "Strained",
            "Overtaxed",
            "Overloaded",
            "Two-weapon fighting",
            "Skill available",
            "Saddled",
            "Low hit points",
            "Critical hit points",
            "Cooldown",
            "Trapped",
            "Grabber",
            "Carrying object",
            "Peaceful townguard",
            "Hostile townguard",
        };

        private SKPoint[] _hoverAnimation = new SKPoint[] 
        { 
            new SKPoint(1f, 2f),
            new SKPoint(1.5f, 1.75f),
            new SKPoint(2f, 1.5f),
            new SKPoint(2.5f, 1.25f),
            new SKPoint(3f, 1f),
            new SKPoint(3.5f, 0.75f),
            new SKPoint(4f, 0.5f),
            new SKPoint(4.25f, 0.25f),
            new SKPoint(4.5f, 0f),
            new SKPoint(4.25f, 0.25f),
            new SKPoint(4f, 0.5f),
            new SKPoint(3.5f, 0.75f),
            new SKPoint(3f, 1f),
            new SKPoint(2.5f, 1.25f),
            new SKPoint(2f, 1.5f),
            new SKPoint(1.5f, 1.75f),
            new SKPoint(1f, 2f),
            new SKPoint(0.5f, 2.25f),
            new SKPoint(0f, 2.5f),
            new SKPoint(-0.5f, 2.25f),
            new SKPoint(-1f, 2f),
            new SKPoint(-1.5f, 1.75f),
            new SKPoint(-2f, 1.5f),
            new SKPoint(-2.5f, 1.25f),
            new SKPoint(-3f, 1f),
            new SKPoint(-3.5f, 0.75f),
            new SKPoint(-4f, 0.5f),
            new SKPoint(-4.25f, 0.25f),
            new SKPoint(-4.5f, 0f),
            new SKPoint(-4.25f, 0.25f),
            new SKPoint(-4f, 0.5f),
            new SKPoint(-3.5f, 0.75f),
            new SKPoint(-3f, 1f),
            new SKPoint(-2.5f, 1.25f),
            new SKPoint(-2f, 1.5f),
            new SKPoint(-1.5f, 1.75f),
            new SKPoint(-1f, 2f),
            new SKPoint(-0.5f, 2.25f),
            new SKPoint(0f, 2.5f),
        };

        private SKPoint[] _flyingAnimation = new SKPoint[]
        {
            new SKPoint(0f, 0f),
            new SKPoint(0f, 0.1f),
            new SKPoint(0f, 0.25f),
            new SKPoint(0f, 0.5f),
            new SKPoint(0f, 1f),
            new SKPoint(0f, 1.5f),
            new SKPoint(0f, 2f),
            new SKPoint(0f, 2.5f),
            new SKPoint(0f, 3f),
            new SKPoint(0f, 3.5f),
            new SKPoint(0f, 4f),
            new SKPoint(0f, 4.5f),
            new SKPoint(0f, 4.75f),
            new SKPoint(0f, 4.9f),
            new SKPoint(0f, 5f),
            new SKPoint(0f, 4.9f),
            new SKPoint(0f, 4.75f),
            new SKPoint(0f, 4.5f),
            new SKPoint(0f, 4f),
            new SKPoint(0f, 3.5f),
            new SKPoint(0f, 3f),
            new SKPoint(0f, 2.5f),
            new SKPoint(0f, 2f),
            new SKPoint(0f, 1.5f),
            new SKPoint(0f, 1f),
            new SKPoint(0f, 0.5f),
            new SKPoint(0f, 0.25f),
            new SKPoint(0f, 0.1f),
        };

        private SKPoint[] _blobAnimation = new SKPoint[]
        {
            new SKPoint(1.00000f, 0.9800f),
            new SKPoint(0.99625f, 0.9820f),
            new SKPoint(0.99250f, 0.9840f),
            new SKPoint(0.98500f, 0.9860f),
            new SKPoint(0.97750f, 0.9880f),
            new SKPoint(0.97000f, 0.9900f),
            new SKPoint(0.96250f, 0.9920f),
            new SKPoint(0.95500f, 0.9940f),
            new SKPoint(0.94750f, 0.9960f),
            new SKPoint(0.94000f, 0.9970f),
            new SKPoint(0.93250f, 0.9980f),
            new SKPoint(0.92875f, 0.9990f),
            new SKPoint(0.92688f, 0.9995f),
            new SKPoint(0.92500f, 1.0000f),
            new SKPoint(0.92688f, 0.9995f),
            new SKPoint(0.92875f, 0.9990f),
            new SKPoint(0.93250f, 0.9980f),
            new SKPoint(0.94000f, 0.9970f),
            new SKPoint(0.94750f, 0.9960f),
            new SKPoint(0.95500f, 0.9940f),
            new SKPoint(0.96250f, 0.9920f),
            new SKPoint(0.97000f, 0.9900f),
            new SKPoint(0.97750f, 0.9880f),
            new SKPoint(0.98500f, 0.9860f),
            new SKPoint(0.99250f, 0.9840f),
            new SKPoint(0.99625f, 0.9820f),
            new SKPoint(1.00000f, 0.9800f),
        };

        private SKPoint[] _swimAnimation = new SKPoint[]
{
            new SKPoint(0f, -0f),
            new SKPoint(0f, -0.1f),
            new SKPoint(0f, -0.2f),
            new SKPoint(0f, -0.4f),
            new SKPoint(0f, -0.6f),
            new SKPoint(0f, -0.8f),
            new SKPoint(0f, -1f),
            new SKPoint(0f, -1.2f),
            new SKPoint(0f, -1.4f),
            new SKPoint(0f, -1.6f),
            new SKPoint(0f, -1.8f),
            new SKPoint(0f, -2f),
            new SKPoint(0f, -2.2f),
            new SKPoint(0f, -2.4f),
            new SKPoint(0f, -2.6f),
            new SKPoint(0f, -2.8f),
            new SKPoint(0f, -2.9f),
            new SKPoint(0f, -3f),
            new SKPoint(0f, -2.9f),
            new SKPoint(0f, -2.8f),
            new SKPoint(0f, -2.6f),
            new SKPoint(0f, -2.4f),
            new SKPoint(0f, -2.2f),
            new SKPoint(0f, -2f),
            new SKPoint(0f, -1.8f),
            new SKPoint(0f, -1.6f),
            new SKPoint(0f, -1.4f),
            new SKPoint(0f, -1.2f),
            new SKPoint(0f, -1.0f),
            new SKPoint(0f, -0.8f),
            new SKPoint(0f, -0.6f),
            new SKPoint(0f, -0.4f),
            new SKPoint(0f, -0.2f),
            new SKPoint(0f, -0.1f),
            new SKPoint(0f, 0f),
        };

        private SKPoint[] _sharkAnimation = new SKPoint[]
{
            new SKPoint(0f, 0f),
            new SKPoint(0f, 0.1f),
            new SKPoint(0f, 0.25f),
            new SKPoint(0f, 0.5f),
            new SKPoint(0f, 1f),
            new SKPoint(0f, 1.5f),
            new SKPoint(0f, 2f),
            new SKPoint(0f, 2.5f),
            new SKPoint(0f, 3f),
            new SKPoint(0f, 3.5f),
            new SKPoint(0f, 4f),
            new SKPoint(0f, 4.5f),
            new SKPoint(0f, 4.75f),
            new SKPoint(0f, 4.9f),
            new SKPoint(0f, 5f),
            new SKPoint(0f, 4.9f),
            new SKPoint(0f, 4.75f),
            new SKPoint(0f, 4.5f),
            new SKPoint(0f, 4f),
            new SKPoint(0f, 3.5f),
            new SKPoint(0f, 3f),
            new SKPoint(0f, 2.5f),
            new SKPoint(0f, 2f),
            new SKPoint(0f, 1.5f),
            new SKPoint(0f, 1f),
            new SKPoint(0f, 0.5f),
            new SKPoint(0f, 0.25f),
            new SKPoint(0f, 0.1f),
        };

        private SKPoint[] _humanBreatheAnimation = new SKPoint[]
        {
            new SKPoint(0.99000f, 1.000f),
            new SKPoint(0.99025f, 0.9995f),
            new SKPoint(0.99050f, 0.9990f),
            new SKPoint(0.99100f, 0.9980f),
            new SKPoint(0.99200f, 0.9960f),
            new SKPoint(0.99300f, 0.9940f),
            new SKPoint(0.99400f, 0.9920f),
            new SKPoint(0.99500f, 0.9900f),
            new SKPoint(0.99600f, 0.9880f),
            new SKPoint(0.99700f, 0.9860f),
            new SKPoint(0.99800f, 0.9840f),
            new SKPoint(0.99900f, 0.9820f),
            new SKPoint(0.99950f, 0.9810f),
            new SKPoint(0.99975f, 0.9805f),
            new SKPoint(1.00000f, 0.9800f),
            new SKPoint(1.00000f, 0.9800f),
            new SKPoint(0.99975f, 0.9805f),
            new SKPoint(0.99950f, 0.9810f),
            new SKPoint(0.99900f, 0.9820f),
            new SKPoint(0.99800f, 0.9840f),
            new SKPoint(0.99700f, 0.9860f),
            new SKPoint(0.99600f, 0.9880f),
            new SKPoint(0.99500f, 0.9900f),
            new SKPoint(0.99400f, 0.9920f),
            new SKPoint(0.99300f, 0.9940f),
            new SKPoint(0.99200f, 0.9960f),
            new SKPoint(0.99100f, 0.9980f),
            new SKPoint(0.99050f, 0.9990f),
            new SKPoint(0.99025f, 0.9995f),
        };

        private SKPoint[] _animalBreatheAnimation = new SKPoint[]
        {
            new SKPoint(1.0000f, 0.9800f),
            new SKPoint(0.9995f, 0.9805f),
            new SKPoint(0.9990f, 0.9810f),
            new SKPoint(0.9980f, 0.9820f),
            new SKPoint(0.9960f, 0.9840f),
            new SKPoint(0.9940f, 0.9860f),
            new SKPoint(0.9920f, 0.9880f),
            new SKPoint(0.9900f, 0.9900f),
            new SKPoint(0.9880f, 0.9920f),
            new SKPoint(0.9860f, 0.9940f),
            new SKPoint(0.9840f, 0.9960f),
            new SKPoint(0.9820f, 0.9980f),
            new SKPoint(0.9810f, 0.9990f),
            new SKPoint(0.9805f, 0.9995f),
            new SKPoint(0.9800f, 1.0000f),
            new SKPoint(0.9800f, 1.0000f),
            new SKPoint(0.9805f, 0.9995f),
            new SKPoint(0.9810f, 0.9990f),
            new SKPoint(0.9820f, 0.9980f),
            new SKPoint(0.9840f, 0.9960f),
            new SKPoint(0.9860f, 0.9940f),
            new SKPoint(0.9880f, 0.9920f),
            new SKPoint(0.9900f, 0.9900f),
            new SKPoint(0.9920f, 0.9880f),
            new SKPoint(0.9940f, 0.9860f),
            new SKPoint(0.9960f, 0.9840f),
            new SKPoint(0.9980f, 0.9820f),
            new SKPoint(0.9990f, 0.9810f),
            new SKPoint(0.9995f, 0.9805f),
            new SKPoint(1.0000f, 0.9800f),
        };

        private readonly object _isSizeAllocatedProcessedLock = new object();
        private bool _isSizeAllocatedProcessed = false;
        public bool IsSizeAllocatedProcessed { get { lock (_isSizeAllocatedProcessedLock) { return _isSizeAllocatedProcessed; } } set { lock (_isSizeAllocatedProcessedLock) { _isSizeAllocatedProcessed = value; } } }

        private readonly object _forceAsciiLock = new object();
        private bool _forceAscii = false;
        public bool ForceAscii { get { lock (_forceAsciiLock) { return _forceAscii; } } set { lock (_forceAsciiLock) { _forceAscii = value; } } }

        private readonly object _forceAllMessagesLock = new object();
        private bool _forceAllMessages = false;
        public bool ForceAllMessages { get { lock (_forceAllMessagesLock) { return _forceAllMessages; } } set { lock (_forceAllMessagesLock) { _forceAllMessages = value; } } }

        public bool HasAllMessagesTransparentBackground { get; set; } = true;

        private readonly object _showExtendedStatusBarLock = new object();
        private bool _showExtendedStatusBar = false;
        public bool ShowExtendedStatusBar { get { lock (_showExtendedStatusBarLock) { return _showExtendedStatusBar; } } set { lock (_showExtendedStatusBarLock) { _showExtendedStatusBar = value; } } }

        private readonly object _lighterDarkeningLock = new object();
        private bool _lighterDarkening = false;
        public bool LighterDarkening { get { lock (_lighterDarkeningLock) { return _lighterDarkening; } } set { lock (_lighterDarkeningLock) { _lighterDarkening = value; } } }

        private readonly object _drawWallEndsLock = new object();
        private bool _drawWallEnds = false;
        public bool DrawWallEnds { get { lock (_drawWallEndsLock) { return _drawWallEnds; } } set { lock (_drawWallEndsLock) { _drawWallEnds = value; } } }

        private readonly object _breatheAnimationLock = new object();
        private bool _breatheAnimations = false;
        public bool BreatheAnimations { get { lock (_breatheAnimationLock) { return _breatheAnimations; } } set { lock (_breatheAnimationLock) { _breatheAnimations = value; } } }

        private readonly object _showPut2BagContextCommandLock = new object();
        private bool _showPut2BagContextCommand = false;
        public bool ShowPut2BagContextCommand { get { lock (_showPut2BagContextCommandLock) { return _showPut2BagContextCommand; } } set { lock (_showPut2BagContextCommandLock) { _showPut2BagContextCommand = value; } } }
        
        private readonly object _accurateLayerDrawingLock = new object();
        private bool _accurateLayerDrawing = false;
        public bool AlternativeLayerDrawing { get { lock (_accurateLayerDrawingLock) { return _accurateLayerDrawing; } } set { lock (_accurateLayerDrawingLock) { _accurateLayerDrawing = value; } } }

        public readonly object RefreshScreenLock = new object();
        private bool _refreshScreen = true;
        public bool RefreshScreen
        {
            get { return _refreshScreen; }
            set { _refreshScreen = value; }
        }

        private game_cursor_types _cursorType;
        private bool _force_paint_at_cursor;
        private bool _show_cursor_on_u;

        private ObjectData[,] _objectData = new ObjectData[GHConstants.MapCols, GHConstants.MapRows];
        private readonly object _objectDataLock = new object();
        private ObjectDataItem _uChain = null;
        private ObjectDataItem _uBall = null;

        private ObjectDataItem[] _weaponStyleObjDataItem= new ObjectDataItem[3];
        private readonly object _weaponStyleObjDataItemLock = new object();
        private bool _drawWeaponStyleAsGlyphs = true;

        private readonly object _petDataLock = new object();
        private List<GHPetDataItem> _petData = new List<GHPetDataItem>();

        private int _shownMessageRows = GHConstants.DefaultMessageRows;
        public int NumDisplayedMessages { get { return _shownMessageRows; } set { _shownMessageRows = value; } }
        public int ActualDisplayedMessages { get { return ForceAllMessages ? GHConstants.AllMessageRows : NumDisplayedMessages; } }

        private int _shownPetRows = GHConstants.DefaultPetRows;
        public int NumDisplayedPetRows { get { return _shownPetRows; } set { _shownPetRows = value; } }
        public SimpleImageButton StandardMeasurementButton { get { return UseSimpleCmdLayout ? SimpleESCButton : ESCButton; } }

        public TTYCursorStyle CursorStyle { get; set; }
        public GHGraphicsStyle GraphicsStyle { get; set; }
        private MapRefreshRateStyle _mapRefreshRate = MapRefreshRateStyle.MapFPS60;
        public MapRefreshRateStyle MapRefreshRate
        {
            get
            {
                return _mapRefreshRate;
            }
            set
            {
                if (_mapRefreshRate == value)
                    return;

                _mapRefreshRate = value;

                if(canvasView.AnimationIsRunning("GeneralAnimationCounter"))
                    canvasView.AbortAnimation("GeneralAnimationCounter");
                _mapUpdateStopWatch.Stop();

                if (!LoadingGrid.IsVisible)
                    StartMainCanvasAnimation();
            }
        }

        public bool ShowMemoryUsage { get; set; }
        public bool UseMainGLCanvas 
        { 
            get { return canvasView.UseGL; } 
            set { 
                canvasView.UseGL = value; 
                MenuCanvas.UseGL = value;
                TextCanvas.UseGL = value;
                CommandCanvas.UseGL = value;
            } 
        }
        private bool _useSimpleCmdLayout = true;
        public bool UseSimpleCmdLayout
        {
            get { return _useSimpleCmdLayout; }
            set
            {
                _useSimpleCmdLayout = value;
                ButtonRowStack.IsVisible = !value;
                UpperCmdLayout.IsVisible = !value;
                SimpleButtonRowStack.IsVisible = value;
                SimpleUpperCmdLayout.IsVisible = value;
            }
        }
        public StackLayout UsedButtonRowStack { get { return UseSimpleCmdLayout ? SimpleButtonRowStack : ButtonRowStack; } }

        public bool ShowBattery { get; set; }

        public bool ShowFPS { get; set; }
        private double _fps;
        private long _counterValueDiff;
        private long _previousMainFPSCounterValue = 0L;
        private long _previousCommandFPSCounterValue = 0L;
        private readonly object _fpslock = new object();
        private Stopwatch _stopWatch = new Stopwatch();
        private Stopwatch _mapUpdateStopWatch = new Stopwatch();

        private Stopwatch _animationStopwatch = new Stopwatch();
        private TimeSpan _previousTimeSpan;

        private readonly object _mapGridLock = new object();
        private bool _mapGrid = false;
        public bool MapGrid { get { lock (_mapGridLock) { return _mapGrid; } } set { lock (_mapGridLock) { _mapGrid = value; } } }

        private readonly object _hitPointBarLock = new object();
        private bool _hitPointBars = false;
        public bool HitPointBars { get { lock (_hitPointBarLock) { return _hitPointBars; } } set { lock (_hitPointBarLock) { _hitPointBars = value; } } }

        private readonly object _orbLock = new object();
        private bool _showOrbs = true;
        public bool ShowOrbs { get { lock (_orbLock) { return _showOrbs; } } set { lock (_orbLock) { _showOrbs = value; } } }
        private bool _showMaxHealthInOrb = false;
        public bool ShowMaxHealthInOrb { get { lock (_orbLock) { return _showMaxHealthInOrb; } } set { lock (_orbLock) { _showMaxHealthInOrb = value; } } }
        private bool _showMaxManaInOrb = false;
        public bool ShowMaxManaInOrb { get { lock (_orbLock) { return _showMaxManaInOrb; } } set { lock (_orbLock) { _showMaxManaInOrb = value; } } }

        private readonly object _playerMarkLock = new object();
        private bool _playerMark = false;
        public bool PlayerMark { get { lock (_playerMarkLock) { return _playerMark; } } set { lock (_playerMarkLock) { _playerMark = value; } } }

        private readonly object _monsterTargetingLock = new object();
        private bool _monsterTargeting = false;
        public bool MonsterTargeting { get { lock (_monsterTargetingLock) { return _monsterTargeting; } } set { lock (_monsterTargetingLock) { _monsterTargeting = value; } } }

        private readonly object _walkArrowLock = new object();
        private bool _walkArrows = true;
        public bool WalkArrows { get { lock (_walkArrowLock) { return _walkArrows; } } set { lock (_walkArrowLock) { _walkArrows = value; } } }

        private readonly object _classicStatusBarLock = new object();
        private bool _classicStatusBar = true;
        public bool ClassicStatusBar { get { lock (_classicStatusBarLock) { return _classicStatusBar; } } set { lock (_classicStatusBarLock) { _classicStatusBar = value; } } }

        private readonly object _showPetsLock = new object();
        private bool _showPets = false;
        public bool ShowPets { get { lock (_showPetsLock) { return _showPets; } } set { lock (_showPetsLock) { _showPets = value; } } }

        private bool _cursorIsOn;
        private bool _showDirections = false;
        private bool _showNumberPad = false;
        private bool ShowNumberPad { get { return _showNumberPad; } set { _showNumberPad = value; } }
        private bool _showWaitIcon = false;
        public bool ShowWaitIcon { get { return _showWaitIcon; } set { _showWaitIcon = value; } }

        public readonly object StatusFieldLock = new object();
        public readonly GHStatusField[] StatusFields = new GHStatusField[(int)statusfields.MAXBLSTATS];

        private MainPage _mainPage;




        /* Persistent temporary bitmap */
        SKBitmap _tempBitmap = new SKBitmap(GHConstants.TileWidth, GHConstants.TileHeight, SKColorType.Rgba8888, SKAlphaType.Unpremul);

        private readonly object _skillRectLock = new object(); 
        private SKRect _skillRect = new SKRect();
        public SKRect SkillRect { get { lock (_skillRectLock) { return _skillRect; } } set { lock (_skillRectLock) { _skillRect = value; } } }
        private bool _skillRectDrawn = false;

        private readonly object _healthRectLock = new object();
        private SKRect _healthRect = new SKRect();
        public SKRect HealthRect { get { lock (_healthRectLock) { return _healthRect; } } set { lock (_healthRectLock) { _healthRect = value; } } }
        private bool _healthRectDrawn = false;

        private readonly object _manaRectLock = new object();
        private SKRect _manaRect = new SKRect();
        public SKRect ManaRect { get { lock (_manaRectLock) { return _manaRect; } } set { lock (_manaRectLock) { _manaRect = value; } } }
        private bool _manaRectDrawn = false;

        private readonly object _statusBarRectLock = new object(); 
        private SKRect _statusBarRect = new SKRect();
        public SKRect StatusBarRect { get { lock (_statusBarRectLock) { return _statusBarRect; } } set { lock (_statusBarRectLock) { _statusBarRect = value; } } }
        private bool _statusBarRectDrawn = false;

        private readonly object _youRectLock = new object();
        private SKRect _youRect = new SKRect();
        public SKRect YouRect { get { lock (_youRectLock) { return _youRect; } } set { lock (_youRectLock) { _youRect = value; } } }
        private bool _youRectDrawn = false;

        public readonly object TargetClipLock = new object();
        public float _originMapOffsetWithNewClipX;
        public float _originMapOffsetWithNewClipY;
        public bool _targetClipOn;
        public long _targetClipStartCounterValue;
        public long _targetClipPanTime;

        private int _clipX;
        private int _clipY;
        public readonly object ClipLock = new object();
        public int ClipX { get { return _clipX; } set { _clipX = value; lock (_mapOffsetLock) { _mapOffsetX = 0; } } }
        public int ClipY { get { return _clipY; } set { _clipY = value; lock (_mapOffsetLock) { _mapOffsetY = 0; } } }
        
        private readonly object _mapNoClipModeLock = new object();
        private bool _mapNoClipMode = false;
        public bool MapNoClipMode { get { lock (_mapNoClipModeLock) { return _mapNoClipMode; } } set { lock (_mapNoClipModeLock) { _mapNoClipMode = value; } } }

        //private object _mapAlternateNoClipModeLock = new object();
        //private bool _mapAlternateNoClipMode = false;
        //public bool MapAlternateNoClipMode { get { lock (_mapAlternateNoClipModeLock) { return _mapAlternateNoClipMode; } } set { lock (_mapAlternateNoClipModeLock) { _mapAlternateNoClipMode = value; } } }

        //private object _zoomChangeCenterModeLock = new object();
        //private bool _zoomChangeCenterMode = false;
        //public bool ZoomChangeCenterMode { get { lock (_zoomChangeCenterModeLock) { return _zoomChangeCenterMode; } } set { lock (_zoomChangeCenterModeLock) { _zoomChangeCenterMode = value; } } }

        private readonly object _mapLookModeLock = new object();
        private bool _mapLookMode = false;
        public bool MapLookMode { get { lock (_mapLookModeLock) { return _mapLookMode; } } set { lock (_mapLookModeLock) { _mapLookMode = value; } } }

        private bool _savedMapTravelMode = false;
        private bool _savedMapTravelModeOnLevel = false;
        private readonly object _mapTravelModeLock = new object();
        private bool _mapTravelMode = false;
        public bool MapTravelMode { get { lock (_mapTravelModeLock) { return _mapTravelMode; } } set { lock (_mapTravelModeLock) { _mapTravelMode = value; } } }

        public bool MapWalkMode { get { return (!MapTravelMode && !MapLookMode); } }
        public bool ZoomMiniMode { get; set; }
        public bool ZoomAlternateMode { get; set; }


        private float _mapFontSize = GHConstants.MapFontDefaultSize;
        private float _mapFontAlternateSize = GHConstants.MapFontDefaultSize * GHConstants.MapFontRelativeAlternateSize;
        private float _mapFontMiniRelativeSize = 1.0f;
        private readonly object _mapFontSizeLock = new object();
        public float MapFontSize { get { lock (_mapFontSizeLock) { return _mapFontSize; } } set { lock (_mapFontSizeLock) { _mapFontSize = value; } } }
        public float MapFontAlternateSize { get { lock (_mapFontSizeLock) { return _mapFontAlternateSize; } } set { lock (_mapFontSizeLock) { _mapFontAlternateSize = value; } } }
        public float MapFontMiniRelativeSize { get { lock (_mapFontSizeLock) { return _mapFontMiniRelativeSize; } } set { lock (_mapFontSizeLock) { _mapFontMiniRelativeSize = value; } } }

        private readonly object _tileWidthLock = new object();
        private float _tileWidth;
        private float UsedTileWidth { get { lock (_tileWidthLock) { return _tileWidth; } } set { lock (_tileWidthLock) { _tileWidth = value; } } }
        private readonly object _tileHeightLock = new object();
        private float _tileHeight;
        private float UsedTileHeight { get { lock (_tileHeightLock) { return _tileHeight; } } set { lock (_tileHeightLock) { _tileHeight = value; } } }
        private float _mapWidth;
        private float _mapHeight;
        private readonly object _mapFontAscentLock = new object();
        private float _mapFontAscent;
        private float UsedMapFontAscent { get { lock (_mapFontAscentLock) { return _mapFontAscent; } } set { lock (_mapFontAscentLock) { _mapFontAscent = value; } } }
        public readonly object AnimationTimerLock = new object();
        public GHAnimationTimerList AnimationTimers = new GHAnimationTimerList();
        public SKBitmap[] TileMap { get { return GHApp._tileMap; } }

        public readonly object _floatingTextLock = new object();
        public List<GHFloatingText> _floatingTexts = new List<GHFloatingText>();
        public readonly object _screenTextLock = new object();
        public GHScreenText _screenText = null;
        public readonly object _conditionTextLock = new object();
        public List<GHConditionText> _conditionTexts = new List<GHConditionText>();
        public readonly object _screenFilterLock = new object();
        public List<GHScreenFilter> _screenFilters = new List<GHScreenFilter>();
        public readonly object _guiEffectLock = new object();
        public List<GHGUIEffect> _guiEffects = new List<GHGUIEffect>();

        private readonly object _enableWizardModeLock = new object();
        private bool _enableWizardMode = false;
        public bool EnableWizardMode { get { lock (_enableWizardModeLock) { return _enableWizardMode; } } set { lock (_enableWizardModeLock) { _enableWizardMode = value; } } }

        private readonly object _enableCasualModeLock = new object();
        private bool _enableCasualMode = false;
        public bool EnableCasualMode { get { lock (_enableCasualModeLock) { return _enableCasualMode; } } set { lock (_enableCasualModeLock) { _enableCasualMode = value; } } }

        private readonly object _enableModernModeLock = new object();
        private bool _enableModernMode = false;
        public bool EnableModernMode { get { lock (_enableModernModeLock) { return _enableModernMode; } } set { lock (_enableModernModeLock) { _enableModernMode = value; } } }

        private List<AddContextMenuData> _contextMenuData = new List<AddContextMenuData>();

        private bool _useMapBitmap = false;
        private readonly object _mapBitmapLock = new object();
        private SKBitmap _mapBitmap = null;

        public GamePage(MainPage mainPage)
        {
            InitializeComponent();
            //On<iOS>().SetUseSafeArea(true);
#if GNH_MAUI
            On<iOS>().SetUseSafeArea(true);
#else
            On<Xamarin.Forms.PlatformConfiguration.iOS>().SetUseSafeArea(true);
#endif
            _mainPage = mainPage;

            CursorStyle = (TTYCursorStyle)Preferences.Get("CursorStyle", 1);
            GraphicsStyle = (GHGraphicsStyle)Preferences.Get("GraphicsStyle", 1);
            MapRefreshRate = (MapRefreshRateStyle)Preferences.Get("MapRefreshRate", (int)UIUtils.GetDefaultMapFPS());
            ShowFPS = Preferences.Get("ShowFPS", false);
            ShowBattery = Preferences.Get("ShowBattery", false);
            UseMainGLCanvas = Preferences.Get("UseMainGLCanvas", GHApp.IsGPUDefault);
            UseSimpleCmdLayout = Preferences.Get("UseSimpleCmdLayout", true);
            ShowMemoryUsage = Preferences.Get("ShowMemoryUsage", false);
            MapGrid = Preferences.Get("MapGrid", false);
            HitPointBars = Preferences.Get("HitPointBars", false);
            ClassicStatusBar = Preferences.Get("ClassicStatusBar", GHConstants.IsDefaultStatusBarClassic);
            ShowOrbs = Preferences.Get("ShowOrbs", true);
            ShowPets = Preferences.Get("ShowPets", true);
            PlayerMark = Preferences.Get("PlayerMark", false);
            MonsterTargeting = Preferences.Get("MonsterTargeting", false);
            NumDisplayedMessages = Preferences.Get("NumDisplayedMessages", GHConstants.DefaultMessageRows);
            NumDisplayedPetRows = Preferences.Get("NumDisplayedPetRows", GHConstants.DefaultPetRows);
            WalkArrows = Preferences.Get("WalkArrows", true);
            LighterDarkening = Preferences.Get("LighterDarkening", GHConstants.DefaultLighterDarkening);
            DrawWallEnds = Preferences.Get("DrawWallEnds", GHConstants.DefaultDrawWallEnds);
            BreatheAnimations = Preferences.Get("BreatheAnimations", GHConstants.DefaultBreatheAnimations);
            ShowPut2BagContextCommand = Preferences.Get("ShowPut2BagContextCommand", GHConstants.DefaultShowPickNStashContextCommand);
            AlternativeLayerDrawing = Preferences.Get("AlternativeLayerDrawing", GHConstants.DefaultAlternativeLayerDrawing);

            float deffontsize = GetDefaultMapFontSize();
            MapFontSize = Preferences.Get("MapFontSize", deffontsize);
            MapFontAlternateSize = Preferences.Get("MapFontAlternateSize", deffontsize * GHConstants.MapFontRelativeAlternateSize);
            MapFontMiniRelativeSize = Preferences.Get("MapFontMiniRelativeSize", 1.0f);
            lock(_mapOffsetLock)
            {
                _mapMiniOffsetX = Preferences.Get("MapMiniOffsetX", 0.0f);
                _mapMiniOffsetY = Preferences.Get("MapMiniOffsetY", 0.0f);
            }
            MapNoClipMode = Preferences.Get("DefaultMapNoClipMode", GHConstants.DefaultMapNoClipMode);
            //MapAlternateNoClipMode = Preferences.Get("MapAlternateNoClipMode", GHConstants.DefaultMapAlternateNoClipMode);
            //ZoomChangeCenterMode = Preferences.Get("ZoomChangeCenterMode", GHConstants.DefaultZoomChangeCenterMode);

            for (int i = 0; i < 6; i++)
            {
                string keystr = "SimpleUILayoutCommandButton" + (i + 1);
                int defCmd = GHApp.DefaultShortcutButton(0, i, true).GetCommand();
                int savedCmd = Preferences.Get(keystr, defCmd);
                int listselidx = GHApp.SelectableShortcutButtonIndexInList(savedCmd, defCmd);
                if (listselidx >= 0)
                    SetSimpleLayoutCommandButton(i, listselidx);
            }

            ToggleTravelModeButton_Clicked(null, null);
            ZoomMiniMode = true;
            ZoomAlternateMode = true;
            ToggleZoomMiniButton_Clicked(null, null);
            ToggleZoomAlternateButton_Clicked(null, null);
            MapNoClipMode = !MapNoClipMode;
            ToggleAutoCenterModeButton_Clicked(null, null);
        }

        private float GetDefaultMapFontSize()
        {
            float c_numerator = 1.0f;
            float c_denominator = 1.0f;
            TargetIdiom ti = Device.Idiom;
            var mainDisplayInfo = DeviceDisplay.MainDisplayInfo;
            float density = (float)mainDisplayInfo.Density;
            switch (ti)
            {
                case TargetIdiom.Tablet:
                case TargetIdiom.Phone:
                    c_numerator = 2.0f;
                    c_denominator = 3.0f;
                    break;
                default:
                    break;
            }
            return GHConstants.MapFontDefaultSize * (density * c_numerator) / c_denominator;
        }

        public async void StartGame()
        {
            _mainPage.GameStarted = true;
            LoadingProgressBar.Progress = 0.0;

            var tasks = new List<Task>();
            _gnollHackService = GHApp.GnollHackService;
            _gnollHackService.InitializeGnollHack();

            if (!GHApp.StartGameDataSet)
            {
                Assembly assembly = GetType().GetTypeInfo().Assembly;
                tasks.Add(LoadingProgressBar.ProgressTo(0.3, 600, Easing.Linear));
                tasks.Add(Task.Run(() =>
                {
                    using (Stream stream = assembly.GetManifestResourceStream(GHApp.AppResourceName + ".Assets.gnollhack_64x96_transparent_32bits.png"))
                    {
                        GHApp._tileMap[0] = SKBitmap.Decode(stream);
                        GHApp._tileMap[0].SetImmutable();
                    }
                }));
                await Task.WhenAll(tasks);
                tasks.Clear();

                tasks.Add(LoadingProgressBar.ProgressTo(0.4, 100, Easing.Linear));
                tasks.Add(Task.Run(() =>
                {
                    using (Stream stream = assembly.GetManifestResourceStream(GHApp.AppResourceName + ".Assets.gnollhack_64x96_transparent_32bits-2.png"))
                    {
                        GHApp._tileMap[1] = SKBitmap.Decode(stream);
                        GHApp._tileMap[1].SetImmutable();
                    }
                }));
                await Task.WhenAll(tasks);
                tasks.Clear();

                tasks.Add(LoadingProgressBar.ProgressTo(0.5, 100, Easing.Linear));
                tasks.Add(Task.Run(() =>
                {
                    using (Stream stream = assembly.GetManifestResourceStream(GHApp.AppResourceName + ".Assets.gnollhack-logo-test-2.png"))
                    {
                        GHApp._logoBitmap = SKBitmap.Decode(stream);
                        GHApp._logoBitmap.SetImmutable();
                    }
                }));
                await Task.WhenAll(tasks);
                tasks.Clear();

                tasks.Add(LoadingProgressBar.ProgressTo(0.6, 100, Easing.Linear));
                tasks.Add(Task.Run(() =>
                {
                    using (Stream stream = assembly.GetManifestResourceStream(GHApp.AppResourceName + ".Assets.UI.skill.png"))
                    {
                        GHApp._skillBitmap = SKBitmap.Decode(stream);
                        GHApp._skillBitmap.SetImmutable();
                    }

                    GHApp.InitializeArrowButtons(assembly);
                    GHApp.InitializeUIBitmaps(assembly);
                    GHApp.InitializeMoreCommandButtons(assembly, UseSimpleCmdLayout);

                    GHApp.UnexploredGlyph = _gnollHackService.GetUnexploredGlyph();
                    GHApp.NoGlyph = _gnollHackService.GetNoGlyph();

                    int animoff, enloff, reoff, general_tile_off, hit_tile_off, ui_tile_off, spell_tile_off, skill_tile_off, command_tile_off, buff_tile_off, cursor_off;
                    _gnollHackService.GetOffs(out animoff, out enloff, out reoff, out general_tile_off, out hit_tile_off, out ui_tile_off, out spell_tile_off, out skill_tile_off, out command_tile_off, out buff_tile_off,
                        out cursor_off);
                    GHApp.AnimationOff = animoff;
                    GHApp.EnlargementOff = enloff;
                    GHApp.ReplacementOff = reoff;
                    GHApp.GeneralTileOff = general_tile_off;
                    GHApp.HitTileOff = hit_tile_off;
                    GHApp.UITileOff = ui_tile_off;
                    GHApp.SpellTileOff = spell_tile_off;
                    GHApp.SkillTileOff = skill_tile_off;
                    GHApp.CommandTileOff = command_tile_off;
                    GHApp.BuffTileOff = buff_tile_off;
                    GHApp.CursorOff = cursor_off;

                }));
                await Task.WhenAll(tasks);
                tasks.Clear();

                tasks.Add(LoadingProgressBar.ProgressTo(0.7, 100, Easing.Linear));
                tasks.Add(Task.Run(() =>
                {
                    GHApp._animationDefs = _gnollHackService.GetAnimationArray();
                    GHApp._enlargementDefs = _gnollHackService.GetEnlargementArray();
                }));
                await Task.WhenAll(tasks);
                tasks.Clear();

                tasks.Add(LoadingProgressBar.ProgressTo(0.80, 100, Easing.Linear));
                tasks.Add(Task.Run(() =>
                {
                    GHApp._replacementDefs = _gnollHackService.GetReplacementArray();
                    GHApp._autodraws = _gnollHackService.GetAutoDrawArray();
                }));
                await Task.WhenAll(tasks);
                tasks.Clear();
                GHApp.StartGameDataSet = true;
            }

            tasks.Add(LoadingProgressBar.ProgressTo(0.90, 100, Easing.Linear));
            tasks.Add(Task.Run(() =>
            {
                ExtendedCommands = _gnollHackService.GetExtendedCommands();
                SetLayerDrawOrder();

                for (int i = 0; i < GHConstants.MapCols; i++)
                {
                    for (int j = 0; j < GHConstants.MapRows; j++)
                    {
                        _mapData[i, j] = new MapData();
                        _mapData[i, j].Glyph = GHApp.UnexploredGlyph;
                        _mapData[i, j].BkGlyph = GHApp.NoGlyph;
                        _mapData[i, j].NeedsUpdate = true;

                        _objectData[i, j] = new ObjectData();
                    }
                }
            }));
            await Task.WhenAll(tasks);
            tasks.Clear();

            await LoadingProgressBar.ProgressTo(0.95, 50, Easing.Linear);

            Thread t = new Thread(new ThreadStart(GNHThreadProc));
            _gnhthread = t;
            _gnhthread.Start();

            _stopWatch.Start();
            //lock (AnimationTimerLock)
            //{
            //    _previousCounterValue = AnimationTimers.general_animation_counter;
            //}
            //lock(_mapBitmapLock)
            //{
            //    _mapBitmap = new SKBitmap(GHConstants.MapCols * GHConstants.TileWidth, GHConstants.MapRows * GHConstants.TileHeight, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
            //}
            await LoadingProgressBar.ProgressTo(0.99, 40, Easing.Linear);

            _animationStopwatch.Reset();
            _previousTimeSpan = _animationStopwatch.Elapsed;
            _animationStopwatch.Start();

            canvasView._gamePage = this;
            CommandCanvas._gamePage = this;
            MenuCanvas._gamePage = this;
            TextCanvas._gamePage = this;
            TipView._gamePage = this;

            canvasView._parentGrid = MainGrid;
            CommandCanvas._parentGrid = MoreCommandsGrid;
            MenuCanvas._parentGrid = MenuGrid;
            TextCanvas._parentGrid = TextGrid;
            TipView._parentGrid = null;

            IsGameOn = true;

            Device.StartTimer(TimeSpan.FromSeconds(1.0 / GHConstants.PollingFrequency), () =>
            {
                if(!StartingPositionsSet && !canvasView.CanvasSize.IsEmpty && IsSizeAllocatedProcessed && lAbilitiesButton.Width > 0)
                {
                    double statusbarheight = GetStatusBarHeight();
                    lAbilitiesButton.HeightRequest = statusbarheight;
                    lWornItemsButton.HeightRequest = statusbarheight;
                    UpperCmdLayout.Margin = new Thickness(0, statusbarheight, 0, 0);
                    SimpleUpperCmdLayout.Margin = new Thickness(0, statusbarheight, 0, 0);
                    StartingPositionsSet = true;
                }

                pollRequestQueue();

                return IsGameOn;
            });

            Device.StartTimer(TimeSpan.FromSeconds(0.5), () =>
            {
                _cursorIsOn = !_cursorIsOn;
                if (ShowFPS)
                {
                    if (!_stopWatch.IsRunning)
                    {
                        _stopWatch.Restart();
                    }
                    else
                    {
                        _stopWatch.Stop();
                        TimeSpan ts = _stopWatch.Elapsed;
                        lock (_fpslock)
                        {
                            if(MoreCommandsGrid.IsVisible)
                            {
                                lock(_commandFPSCounterLock)
                                {
                                    _counterValueDiff = _commandFPSCounterValue - _previousCommandFPSCounterValue;
                                    _previousCommandFPSCounterValue = _commandFPSCounterValue;
                                }
                            }
                            else
                            {
                                lock (_mainFPSCounterLock)
                                {
                                    _counterValueDiff = _mainFPSCounterValue - _previousMainFPSCounterValue;
                                    _previousMainFPSCounterValue = _mainFPSCounterValue;
                                }
                                //lock (AnimationTimerLock)
                                //{
                                //    currentCounterValue = AnimationTimers.general_animation_counter;
                                //}
                            }
                            _fps = ts.TotalMilliseconds == 0.0 ? 0.0 : _counterValueDiff / (ts.TotalMilliseconds / 1000.0);
                            if (_fps < 0.0f || _fps > 500.0f) /* Just in case if it is off somehow */
                            {
                                _fps = 0.0;
                                _counterValueDiff = 0;
                            }
                        }
                        _stopWatch.Restart();
                    }
                }
                else
                {
                    if (_stopWatch.IsRunning)
                        _stopWatch.Stop();
                }
                return IsGameOn;
            });

            await LoadingProgressBar.ProgressTo(1.0, 20, Easing.Linear);
            GHApp.DebugCheckCurrentFileDescriptor("StartGameFinished");
        }

        public async void RestartGame()
        {
            _currentGame = null;
            GHApp.CurrentGHGame = null;
            _gnhthread = null;

            /* Collect garbage at this point */
            await Task.Delay(50);
            GHApp.CollectGarbage();
            await Task.Delay(50);

            Thread t = new Thread(new ThreadStart(GNHThreadProcForRestart));
            _gnhthread = t;
            _gnhthread.Start();
        }

        public void UpdateMainCanvas()
        {
            bool refresh = true;
            lock (RefreshScreenLock)
            {
                refresh = RefreshScreen;
            }

            IncrementCounters();

            if(canvasView.IsVisible && refresh)
            {
                canvasView.InvalidateSurface();

                if(ForceAllMessages)
                {
                    float timePassed = 0;
                    if (!_mapUpdateStopWatch.IsRunning)
                    {
                        timePassed = 1.0f / UIUtils.GetMainCanvasAnimationFrequency(MapRefreshRate);
                        _mapUpdateStopWatch.Restart();
                    }
                    else
                    {
                        _mapUpdateStopWatch.Stop();
                        timePassed = (float)_mapUpdateStopWatch.ElapsedMilliseconds / 1000f;
                        _mapUpdateStopWatch.Restart();
                    }

                    lock (_messageScrollLock)
                    {
                        float speed = _messageScrollSpeed; /* pixels per second */
                        float topScrollLimit = Math.Max(0, -_messageSmallestTop);
                        if (_messageScrollSpeedOn)
                        {
                            int sgn = Math.Sign(_messageScrollSpeed);
                            float delta = speed * timePassed; /* pixels */
                            _messageScrollOffset += delta;
                            if (_messageScrollOffset < topScrollLimit && _messageScrollOffset - delta > topScrollLimit)
                            {
                                _messageScrollOffset = topScrollLimit;
                                _messageScrollSpeed = 0;
                                _messageScrollSpeedOn = false;
                            }
                            else if (_messageScrollOffset > 0 && _messageScrollOffset - delta < 0)
                            {
                                _messageScrollOffset = 0;
                                _messageScrollSpeed = 0;
                                _messageScrollSpeedOn = false;
                            }
                            else if (_messageScrollOffset > topScrollLimit || _messageScrollOffset < 0)
                            {
                                float deceleration1 = canvasView.CanvasSize.Height * GHConstants.ScrollConstantDeceleration * GHConstants.ScrollConstantDecelerationOverEdgeMultiplier;
                                float deceleration2 = Math.Abs(_messageScrollSpeed) * GHConstants.ScrollSpeedDeceleration * GHConstants.ScrollSpeedDecelerationOverEdgeMultiplier;
                                float deceleration_per_second = deceleration1 + deceleration2;
                                float distance_from_edge = _messageScrollOffset > topScrollLimit ? _messageScrollOffset - topScrollLimit : _messageScrollOffset - 0;
                                float deceleration3 = (distance_from_edge + (float)Math.Sign(distance_from_edge) * GHConstants.ScrollDistanceEdgeConstant * canvasView.CanvasSize.Height) * GHConstants.ScrollOverEdgeDeceleration;
                                float distance_anchor_distance = canvasView.CanvasSize.Height * GHConstants.ScrollDistanceAnchorFactor;
                                float close_anchor_distance = canvasView.CanvasSize.Height * GHConstants.ScrollCloseAnchorFactor;
                                float target_speed_at_distance = GHConstants.ScrollTargetSpeedAtDistanceAnchor;
                                float target_speed_at_close = GHConstants.ScrollTargetSpeedAtCloseAnchor;
                                float target_speed_at_edge = GHConstants.ScrollTargetSpeedAtEdge;
                                float dist_factor = (Math.Abs(distance_from_edge) - close_anchor_distance) / (distance_anchor_distance - close_anchor_distance);
                                float close_factor = Math.Abs(distance_from_edge) / close_anchor_distance;
                                float target_speed = -1.0f * (float)Math.Sign(distance_from_edge)
                                    * (
                                    Math.Max(0f, dist_factor) * (target_speed_at_distance - target_speed_at_close)
                                    + Math.Min(1f, close_factor) * (target_speed_at_close - target_speed_at_edge)
                                    + target_speed_at_edge
                                    )
                                    * canvasView.CanvasSize.Height;
                                if (_messageScrollOffset > topScrollLimit ? _messageScrollSpeed <= 0 : _messageScrollSpeed >= 0)
                                {
                                    float target_factor = Math.Abs(distance_from_edge) / distance_anchor_distance;
                                    _messageScrollSpeed += (-1.0f * deceleration3) * timePassed;
                                    if (target_factor < 1.0f)
                                    {
                                        _messageScrollSpeed = _messageScrollSpeed * target_factor + target_speed * (1.0f - target_factor);
                                    }
                                }
                                else
                                    _messageScrollSpeed += (-1.0f * (float)sgn * deceleration_per_second - deceleration3) * timePassed;
                            }
                            else
                            {
                                if (_messageScrollSpeedReleaseStamp != null)
                                {
                                    long millisecs_elapsed = (DateTime.Now.Ticks - _messageScrollSpeedReleaseStamp.Ticks) / TimeSpan.TicksPerMillisecond;
                                    if (millisecs_elapsed > GHConstants.FreeScrollingTime)
                                    {
                                        float deceleration1 = (float)canvasView.CanvasSize.Height * GHConstants.ScrollConstantDeceleration;
                                        float deceleration2 = Math.Abs(_messageScrollSpeed) * GHConstants.ScrollSpeedDeceleration;
                                        float deceleration_per_second = deceleration1 + deceleration2;
                                        _messageScrollSpeed += -1.0f * (float)sgn * (deceleration_per_second * timePassed);
                                        if (sgn == 0 || (sgn > 0 && _messageScrollSpeed < 0) || (sgn < 0 && _messageScrollSpeed > 0))
                                            _messageScrollSpeed = 0;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void UpdateCommandCanvas()
        {
            if (MoreCommandsGrid.IsVisible)
            {
                CommandCanvas.InvalidateSurface();
                float offx = MoreCmdOffsetX;
                if (offx != 0 && (CommandTouchDictionary.Count == 0 || _commandChangedPage))
                {
                    float delta = -1 * Math.Sign(offx) * CommandCanvas.CanvasSize.Width * _moreCmdOffsetAutoSpeed / UIUtils.GetAuxiliaryCanvasAnimationFrequency();
                    if (offx > 0 && offx + delta < 0)
                        MoreCmdOffsetX = 0;
                    else if (offx < 0 && offx + delta > 0)
                        MoreCmdOffsetX = 0;
                    else
                        MoreCmdOffsetX = offx + delta;
                }
            }
        }

        public void UpdateMenuCanvas()
        {
            bool refresh = false;
            if (MenuGrid.IsVisible)
            {
                lock (_menuDrawOnlyLock)
                {
                    refresh = _menuRefresh;
                }
                if (refresh)
                {
                    MenuCanvas.InvalidateSurface();
                    lock (_menuScrollLock)
                    {
                        float speed = _menuScrollSpeed; /* pixels per second */
                        float bottomScrollLimit = 0;
                        bottomScrollLimit = Math.Min(0, MenuCanvas.CanvasSize.Height - TotalMenuHeight);
                        if (_menuScrollSpeedOn)
                        {
                            int sgn = Math.Sign(_menuScrollSpeed);
                            float delta = speed / UIUtils.GetAuxiliaryCanvasAnimationFrequency(); /* pixels */
                            _menuScrollOffset += delta;
                            if (_menuScrollOffset < 0 && _menuScrollOffset - delta > 0)
                            {
                                _menuScrollOffset = 0;
                                _menuScrollSpeed = 0;
                                _menuScrollSpeedOn = false;
                            }
                            else if (_menuScrollOffset > bottomScrollLimit && _menuScrollOffset - delta < bottomScrollLimit)
                            {
                                _menuScrollOffset = bottomScrollLimit;
                                _menuScrollSpeed = 0;
                                _menuScrollSpeedOn = false;
                            }
                            else if (_menuScrollOffset > 0 || _menuScrollOffset < bottomScrollLimit)
                            {
                                float deceleration1 = MenuCanvas.CanvasSize.Height * GHConstants.ScrollConstantDeceleration * GHConstants.ScrollConstantDecelerationOverEdgeMultiplier;
                                float deceleration2 = Math.Abs(_menuScrollSpeed) * GHConstants.ScrollSpeedDeceleration * GHConstants.ScrollSpeedDecelerationOverEdgeMultiplier;
                                float deceleration_per_second = deceleration1 + deceleration2;
                                float distance_from_edge = _menuScrollOffset > 0 ? _menuScrollOffset : _menuScrollOffset - bottomScrollLimit;
                                float deceleration3 = (distance_from_edge + (float)Math.Sign(distance_from_edge) * GHConstants.ScrollDistanceEdgeConstant * MenuCanvas.CanvasSize.Height) * GHConstants.ScrollOverEdgeDeceleration;
                                float distance_anchor_distance = MenuCanvas.CanvasSize.Height * GHConstants.ScrollDistanceAnchorFactor;
                                float close_anchor_distance = MenuCanvas.CanvasSize.Height * GHConstants.ScrollCloseAnchorFactor;
                                float target_speed_at_distance = GHConstants.ScrollTargetSpeedAtDistanceAnchor;
                                float target_speed_at_close = GHConstants.ScrollTargetSpeedAtCloseAnchor;
                                float target_speed_at_edge = GHConstants.ScrollTargetSpeedAtEdge;
                                float dist_factor = (Math.Abs(distance_from_edge) - close_anchor_distance) / (distance_anchor_distance - close_anchor_distance);
                                float close_factor = Math.Abs(distance_from_edge) / close_anchor_distance;
                                float target_speed = -1.0f * (float)Math.Sign(distance_from_edge) 
                                    * (
                                    Math.Max(0f, dist_factor) * (target_speed_at_distance - target_speed_at_close)
                                    + Math.Min(1f, close_factor) * (target_speed_at_close - target_speed_at_edge) 
                                    + target_speed_at_edge
                                    ) 
                                    * MenuCanvas.CanvasSize.Height;
                                if (_menuScrollOffset > 0 ? _menuScrollSpeed <= 0 : _menuScrollSpeed >= 0)
                                {
                                    float target_factor = Math.Abs(distance_from_edge) / distance_anchor_distance;
                                    _menuScrollSpeed += (-1.0f * deceleration3) * (float)UIUtils.GetAuxiliaryCanvasAnimationInterval() / 1000;
                                    if(target_factor < 1.0f)
                                    {
                                        _menuScrollSpeed = _menuScrollSpeed * target_factor + target_speed * (1.0f - target_factor);
                                    }
                                }
                                else
                                    _menuScrollSpeed += (-1.0f * (float)sgn * deceleration_per_second - deceleration3) * (float)UIUtils.GetAuxiliaryCanvasAnimationInterval() / 1000;
                            }
                            else
                            {
                                if(_menuScrollSpeedReleaseStamp != null)
                                {
                                    long millisecs_elapsed = (DateTime.Now.Ticks - _menuScrollSpeedReleaseStamp.Ticks) / TimeSpan.TicksPerMillisecond;
                                    if (millisecs_elapsed > GHConstants.FreeScrollingTime)
                                    {
                                        float deceleration1 = (float)MenuCanvas.CanvasSize.Height * GHConstants.ScrollConstantDeceleration;
                                        float deceleration2 = Math.Abs(_menuScrollSpeed) * GHConstants.ScrollSpeedDeceleration;
                                        float deceleration_per_second = deceleration1 + deceleration2;
                                        _menuScrollSpeed += - 1.0f * (float)sgn * ((deceleration_per_second * (float)UIUtils.GetAuxiliaryCanvasAnimationInterval()) / 1000);
                                        if (sgn == 0 || (sgn > 0 && _menuScrollSpeed < 0) || (sgn < 0 && _menuScrollSpeed > 0))
                                            _menuScrollSpeed = 0;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void UpdateTextCanvas()
        {
            if (TextGrid.IsVisible)
            {
                TextCanvas.InvalidateSurface();
                lock (_textScrollLock)
                {
                    float speed = _textScrollSpeed; /* pixels per second */
                    float bottomScrollLimit = 0;
                    bottomScrollLimit = Math.Min(0, TextCanvas.CanvasSize.Height - TotalTextHeight);
                    if (_textScrollSpeedOn)
                    {
                        int sgn = Math.Sign(_textScrollSpeed);
                        float delta = speed / UIUtils.GetAuxiliaryCanvasAnimationFrequency(); /* pixels */
                        _textScrollOffset += delta;
                        if (_textScrollOffset < 0 && _textScrollOffset - delta > 0)
                        {
                            _textScrollOffset = 0;
                            _textScrollSpeed = 0;
                            _textScrollSpeedOn = false;
                        }
                        else if (_textScrollOffset > bottomScrollLimit && _textScrollOffset - delta < bottomScrollLimit)
                        {
                            _textScrollOffset = bottomScrollLimit;
                            _textScrollSpeed = 0;
                            _textScrollSpeedOn = false;
                        }
                        else if (_textScrollOffset > 0 || _textScrollOffset < bottomScrollLimit)
                        {
                            float deceleration1 = TextCanvas.CanvasSize.Height * GHConstants.ScrollConstantDeceleration * GHConstants.ScrollConstantDecelerationOverEdgeMultiplier;
                            float deceleration2 = Math.Abs(_textScrollSpeed) * GHConstants.ScrollSpeedDeceleration * GHConstants.ScrollSpeedDecelerationOverEdgeMultiplier;
                            float deceleration_per_second = deceleration1 + deceleration2;
                            float distance_from_edge = _textScrollOffset > 0 ? _textScrollOffset : _textScrollOffset - bottomScrollLimit;
                            float deceleration3 = (distance_from_edge + (float)Math.Sign(distance_from_edge) * GHConstants.ScrollDistanceEdgeConstant * TextCanvas.CanvasSize.Height) * GHConstants.ScrollOverEdgeDeceleration;
                            float distance_anchor_distance = TextCanvas.CanvasSize.Height * GHConstants.ScrollDistanceAnchorFactor;
                            float close_anchor_distance = TextCanvas.CanvasSize.Height * GHConstants.ScrollCloseAnchorFactor;
                            float target_speed_at_distance = GHConstants.ScrollTargetSpeedAtDistanceAnchor;
                            float target_speed_at_close = GHConstants.ScrollTargetSpeedAtCloseAnchor;
                            float target_speed_at_edge = GHConstants.ScrollTargetSpeedAtEdge;
                            float dist_factor = (Math.Abs(distance_from_edge) - close_anchor_distance) / (distance_anchor_distance - close_anchor_distance);
                            float close_factor = Math.Abs(distance_from_edge) / close_anchor_distance;
                            float target_speed = -1.0f * (float)Math.Sign(distance_from_edge)
                                * (
                                Math.Max(0f, dist_factor) * (target_speed_at_distance - target_speed_at_close)
                                + Math.Min(1f, close_factor) * (target_speed_at_close - target_speed_at_edge)
                                + target_speed_at_edge
                                )
                                * TextCanvas.CanvasSize.Height;
                            if (_textScrollOffset > 0 ? _textScrollSpeed <= 0 : _textScrollSpeed >= 0)
                            {
                                float target_factor = Math.Abs(distance_from_edge) / distance_anchor_distance;
                                _textScrollSpeed += (-1.0f * deceleration3) * (float)UIUtils.GetAuxiliaryCanvasAnimationInterval() / 1000;
                                if (target_factor < 1.0f)
                                {
                                    _textScrollSpeed = _textScrollSpeed * target_factor + target_speed * (1.0f - target_factor);
                                }
                            }
                            else
                                _textScrollSpeed += (-1.0f * (float)sgn * deceleration_per_second - deceleration3) * (float)UIUtils.GetAuxiliaryCanvasAnimationInterval() / 1000;
                        }
                        else
                        {
                            if (_textScrollSpeedReleaseStamp != null)
                            {
                                long millisecs_elapsed = (DateTime.Now.Ticks - _textScrollSpeedReleaseStamp.Ticks) / TimeSpan.TicksPerMillisecond;
                                if (millisecs_elapsed > GHConstants.FreeScrollingTime)
                                {
                                    float deceleration1 = (float)TextCanvas.CanvasSize.Height * GHConstants.ScrollConstantDeceleration;
                                    float deceleration2 = Math.Abs(_textScrollSpeed) * GHConstants.ScrollSpeedDeceleration;
                                    float deceleration_per_second = deceleration1 + deceleration2;
                                    _textScrollSpeed += -1.0f * (float)sgn * ((deceleration_per_second * (float)UIUtils.GetAuxiliaryCanvasAnimationInterval()) / 1000);
                                    if (sgn == 0 || (sgn > 0 && _textScrollSpeed < 0) || (sgn < 0 && _textScrollSpeed > 0))
                                        _textScrollSpeed = 0;
                                }
                            }
                        }
                    }
                }
            }
        }

        private uint _auxAnimationLength = GHConstants.AuxiliaryCanvasAnimationTime / UIUtils.GetAuxiliaryCanvasAnimationInterval();
        private void StartMainCanvasAnimation()
        {
            uint mainAnimationLength = 20; // GHConstants.MainCanvasAnimationTime / UIUtils.GetMainCanvasAnimationInterval(MapRefreshRate);
            Animation canvasAnimation = new Animation(v => canvasView.GeneralAnimationCounter = (long)v, 1, mainAnimationLength);
            canvasAnimation.Commit(canvasView, "GeneralAnimationCounter", length: GHConstants.MainCanvasAnimationTime, 
                rate: UIUtils.GetMainCanvasAnimationInterval(MapRefreshRate), repeat: () => true /* MainGrid.IsVisible */);
            _mapUpdateStopWatch.Restart();
        }

        private void StartCommandCanvasAnimation()
        {
            Animation commandAnimation = new Animation(v => CommandCanvas.GeneralAnimationCounter = (long)v, 1, _auxAnimationLength);
            commandAnimation.Commit(CommandCanvas, "GeneralAnimationCounter", length: GHConstants.AuxiliaryCanvasAnimationTime, 
                rate: UIUtils.GetAuxiliaryCanvasAnimationInterval(), repeat: () => true /* MoreCommandsGrid.IsVisible */);
        }

        private void StartMenuCanvasAnimation()
        {
            Animation commandAnimation = new Animation(v => MenuCanvas.GeneralAnimationCounter = (long)v, 1, _auxAnimationLength);
            commandAnimation.Commit(MenuCanvas, "GeneralAnimationCounter", length: GHConstants.AuxiliaryCanvasAnimationTime, 
                rate: UIUtils.GetAuxiliaryCanvasAnimationInterval(), repeat: () => true /* MenuGrid.IsVisible */);
        }
        private void StartTextCanvasAnimation()
        {
            Animation commandAnimation = new Animation(v => TextCanvas.GeneralAnimationCounter = (long)v, 1, _auxAnimationLength);
            commandAnimation.Commit(TextCanvas, "GeneralAnimationCounter", length: GHConstants.AuxiliaryCanvasAnimationTime, 
                rate: UIUtils.GetAuxiliaryCanvasAnimationInterval(), repeat: () => true /* TextGrid.IsVisible */);
        }

        private bool StartingPositionsSet { get; set; }
        private int _subCounter = 0;
        public long GetAnimationCounterIncrement()
        {
            long counter_increment = 1;
            int subCounterMax = 0;
            switch (MapRefreshRate)
            {
                case MapRefreshRateStyle.MapFPS20:
                    counter_increment = 2; /* Animations skip at every other frame at 20fps to get 40fps */
                    break;
                case MapRefreshRateStyle.MapFPS30:
                    break;
                case MapRefreshRateStyle.MapFPS40:
                    break;
                case MapRefreshRateStyle.MapFPS60:
                    subCounterMax = 1; /* Animations proceed at every other frame at 60fps to get 30fps */
                    break;
                case MapRefreshRateStyle.MapFPS80:
                    subCounterMax = 1; /* Animations proceed at every other frame at 80fps to get 40fps */
                    break;
                case MapRefreshRateStyle.MapFPS90:
                    subCounterMax = 2; /* Animations proceed at every third frame at 90fps to get 30fps */
                    break;
                case MapRefreshRateStyle.MapFPS120:
                    subCounterMax = 2; /* Animations proceed at every third frame at 120fps to get 40fps */
                    break;
            }
            if(subCounterMax > 0)
            {
                if (_subCounter != subCounterMax)
                    counter_increment = 0; /* otherwise 1 */
                _subCounter++;
                _subCounter = _subCounter % (subCounterMax + 1);
            }
            return counter_increment;
        }

        public void IncrementCounters()
        {
            int i;
            long counter_increment = GetAnimationCounterIncrement();
            long generalcountervalue, maincountervalue;
            lock (AnimationTimerLock)
            {
                AnimationTimers.general_animation_counter += counter_increment;
                if (AnimationTimers.general_animation_counter < 0)
                    AnimationTimers.general_animation_counter = 0;

                if (AnimationTimers.u_action_animation_counter_on)
                {
                    AnimationTimers.u_action_animation_counter += counter_increment;
                    if (AnimationTimers.u_action_animation_counter < 0)
                        AnimationTimers.u_action_animation_counter = 0;
                }

                if (AnimationTimers.m_action_animation_counter_on)
                {
                    AnimationTimers.m_action_animation_counter += counter_increment;
                    if (AnimationTimers.m_action_animation_counter < 0)
                        AnimationTimers.m_action_animation_counter = 0;
                }

                if (AnimationTimers.explosion_animation_counter_on)
                {
                    AnimationTimers.explosion_animation_counter += counter_increment;
                    if (AnimationTimers.explosion_animation_counter < 0)
                        AnimationTimers.explosion_animation_counter = 0;
                }

                for (i = 0; i < GHConstants.MaxPlayedZapAnimations; i++)
                {
                    if (AnimationTimers.zap_animation_counter_on[i])
                    {
                        AnimationTimers.zap_animation_counter[i] += counter_increment;
                        if (AnimationTimers.zap_animation_counter[i] < 0)
                            AnimationTimers.zap_animation_counter[i] = 0;
                    }
                }

                for (i = 0; i < GHConstants.MaxPlayedSpecialEffects; i++)
                {
                    if (AnimationTimers.special_effect_animation_counter_on[i])
                    {
                        AnimationTimers.special_effect_animation_counter[i] += counter_increment;
                        if (AnimationTimers.special_effect_animation_counter[i] < 0)
                            AnimationTimers.special_effect_animation_counter[i] = 0;
                    }
                }

                generalcountervalue = AnimationTimers.general_animation_counter;
            }

            lock (_mainCounterLock)
            {
                _mainCounterValue++;
                if (_mainCounterValue < 0)
                    _mainCounterValue = 0;

                maincountervalue = _mainCounterValue;
            }

            lock (_floatingTextLock)
            {
                for (i = _floatingTexts.Count - 1; i >= 0; i--)
                {
                    if (_floatingTexts[i].IsFinished(maincountervalue))
                        _floatingTexts.RemoveAt(i);
                }
            }

            lock (_conditionTextLock)
            {
                for (i = _conditionTexts.Count - 1; i >= 0; i--)
                {
                    if (_conditionTexts[i].IsFinished(maincountervalue))
                        _conditionTexts.RemoveAt(i);
                }
            }

            lock (_screenFilterLock)
            {
                for (i = _screenFilters.Count - 1; i >= 0; i--)
                {
                    if (_screenFilters[i].IsFinished(maincountervalue))
                        _screenFilters.RemoveAt(i);
                }
            }

            lock (_guiEffectLock)
            {
                for (i = _guiEffects.Count - 1; i >= 0; i--)
                {
                    if (_guiEffects[i].IsFinished(maincountervalue))
                        _guiEffects.RemoveAt(i);
                }
            }

            lock (_screenTextLock)
            {
                if (_screenText != null && _screenText.IsFinished(maincountervalue))
                    _screenText = null;
            }

            lock (TargetClipLock)
            {
                if (maincountervalue < _targetClipStartCounterValue
                    || maincountervalue > _targetClipStartCounterValue + _targetClipPanTime)
                    _targetClipOn = false;

                if (_targetClipOn)
                {
                    lock (_mapOffsetLock)
                    {
                        _mapOffsetX = _originMapOffsetWithNewClipX * Math.Max(0.0f, 1.0f - (float)(maincountervalue - _targetClipStartCounterValue) / (float)_targetClipPanTime);
                        _mapOffsetY = _originMapOffsetWithNewClipY * Math.Max(0.0f, 1.0f - (float)(maincountervalue - _targetClipStartCounterValue) / (float)_targetClipPanTime);
                    }
                }
            }
            
        }

        public void HideLoadingScreen()
        {
            DelayedLoadingScreenHide();
            MainGrid.IsVisible = true;
            StartMainCanvasAnimation();
        }

        public void DelayedLoadingScreenHide()
        {
            Device.StartTimer(TimeSpan.FromSeconds(UIUtils.GetWindowHideSecs()), () =>
            {
                LoadingGrid.IsVisible = false;
                return false;
            });
        }

        public void ClearContextMenu()
        {
            ContextLayout.Children.Clear();
            _contextMenuData.Clear();
            ContextLayout.IsVisible = false;
        }
        public void AddContextMenu(AddContextMenuData data)
        {
            int cmddefchar = data.cmd_def_char;
            int cmdcurchar = data.cmd_cur_char;
            if (cmddefchar < 0)
                cmddefchar += 256; /* On this operating system, chars are signed chars; fix to positive values */
            if (cmdcurchar < 0)
                cmdcurchar += 256; /* On this operating system, chars are signed chars; fix to positive values */
            string icon_string = "";
            int LastPickedCmd = GHUtils.Meta('<');
            int OfferCmd = GHUtils.Meta('o');
            int PrayCmd = GHUtils.Meta('p');
            int DipCmd = GHUtils.Meta('d');
            int DigCmd = GHUtils.Ctrl('g');
            int SitCmd = GHUtils.Ctrl('s');
            int RideCmd = GHUtils.Meta('R');
            int PickToBagCmd = ';';
            if (cmddefchar == PickToBagCmd && !ShowPut2BagContextCommand)
                return; /* Do not add */

            _contextMenuData.Add(data);

            switch ((char)cmddefchar)
            {
                case 'a':
                    switch (data.style)
                    {
                        case (int)context_menu_styles.CONTEXT_MENU_STYLE_CLOSE_DISPLAY: /* Next interesting / monster */
                        case (int)context_menu_styles.CONTEXT_MENU_STYLE_GETDIR: /* Next interesting / monster */
                        case (int)context_menu_styles.CONTEXT_MENU_STYLE_GETPOS: /* Next interesting / monster */
                            icon_string = GHApp.AppResourceName + ".Assets.UI.next.png";
                            break;
                        case (int)context_menu_styles.CONTEXT_MENU_STYLE_GENERAL: /* Apply */
                        default:
                            icon_string = GHApp.AppResourceName + ".Assets.UI.apply.png";
                            break;
                    }
                    break;
                case 'm':
                    switch (data.style)
                    {
                        case (int)context_menu_styles.CONTEXT_MENU_STYLE_GETPOS: /* Next interesting / monster */
                            icon_string = GHApp.AppResourceName + ".Assets.UI.next.png";
                            break;
                        default:
                            icon_string = GHApp.AppResourceName + ".Assets.UI.next.png";
                            break;
                    }
                    break;
                case 'A':
                case 'M':
                    switch (data.style)
                    {
                        case (int)context_menu_styles.CONTEXT_MENU_STYLE_GETPOS: /* Previous interesting / monster */
                            icon_string = GHApp.AppResourceName + ".Assets.UI.previous.png";
                            break;
                        default:
                            icon_string = GHApp.AppResourceName + ".Assets.UI.previous.png";
                            break;
                    }
                    break;
                case 'e':
                    icon_string = GHApp.AppResourceName + ".Assets.UI.eat.png";
                    break;
                case 'l':
                    icon_string = GHApp.AppResourceName + ".Assets.UI.loot.png";
                    break;
                case 'p':
                    icon_string = GHApp.AppResourceName + ".Assets.UI.pay.png";
                    break;
                case ',':
                    icon_string = GHApp.AppResourceName + ".Assets.UI.pickup.png";
                    break;
                case '<':
                    switch (data.style)
                    {
                        case (int)context_menu_styles.CONTEXT_MENU_STYLE_GETDIR: /* Upwards */
                            icon_string = GHApp.AppResourceName + ".Assets.UI.target-upwards.png";
                            break;
                        case (int)context_menu_styles.CONTEXT_MENU_STYLE_GETPOS:
                            icon_string = GHApp.AppResourceName + ".Assets.UI.stairs-up.png";
                            break;
                        default:
                        case (int)context_menu_styles.CONTEXT_MENU_STYLE_GENERAL:
                            if (data.target_text != null && data.target_text == "Pit")
                                icon_string = GHApp.AppResourceName + ".Assets.UI.arrow_up.png";
                            else
                                icon_string = GHApp.AppResourceName + ".Assets.UI.stairs-up.png";
                            break;
                    }
                    break;
                case '>':
                    switch (data.style)
                    {
                        case (int)context_menu_styles.CONTEXT_MENU_STYLE_GETDIR: /* Downwards */
                            icon_string = GHApp.AppResourceName + ".Assets.UI.target-downwards.png";
                            break;
                        case (int)context_menu_styles.CONTEXT_MENU_STYLE_GETPOS:
                            icon_string = GHApp.AppResourceName + ".Assets.UI.stairs-down.png";
                            break;
                        default:
                        case (int)context_menu_styles.CONTEXT_MENU_STYLE_GENERAL:
                            if(data.target_text != null && data.target_text == "Pit")
                                icon_string = GHApp.AppResourceName + ".Assets.UI.arrow_down.png";
                            else
                                icon_string = GHApp.AppResourceName + ".Assets.UI.stairs-down.png";
                            break;
                    }
                    break;
                case ':':
                    icon_string = GHApp.AppResourceName + ".Assets.UI.lookhere.png";
                    break;
                case 'q':
                    icon_string = GHApp.AppResourceName + ".Assets.UI.quaff.png";
                    break;
                case 'r':
                    icon_string = GHApp.AppResourceName + ".Assets.UI.read.png";
                    break;
                case '.':
                    switch(data.style)
                    {
                        case (int)context_menu_styles.CONTEXT_MENU_STYLE_GETPOS: /* Pick position in getpos */
                            icon_string = GHApp.AppResourceName + ".Assets.UI.select.png";
                            break;
                        case (int)context_menu_styles.CONTEXT_MENU_STYLE_GETDIR: /* Self in getdir */
                            icon_string = GHApp.AppResourceName + ".Assets.UI.target-self.png";
                            break;
                        default:
                        case (int)context_menu_styles.CONTEXT_MENU_STYLE_GENERAL:
                            icon_string = GHApp.AppResourceName + ".Assets.UI.wait.png";
                            break;
                    }
                    break;
                case (char)27:
                    switch (data.style)
                    {
                        case (int)context_menu_styles.CONTEXT_MENU_STYLE_CLOSE_DISPLAY:
                            icon_string = GHApp.AppResourceName + ".Assets.UI.exit-to-map.png";
                            break;
                        default:
                            icon_string = GHApp.AppResourceName + ".Assets.UI.no.png";
                            break;
                    }
                    break;
                case 'C':
                    if(data.cmd_text == "Steed")
                        icon_string = GHApp.AppResourceName + ".Assets.UI.chatsteed.png";
                    else
                        icon_string = GHApp.AppResourceName + ".Assets.UI.chat.png";
                    break;
                default:
                    if (cmddefchar == LastPickedCmd)
                        icon_string = GHApp.AppResourceName + ".Assets.UI.lastitem.png";
                    else if (cmddefchar == OfferCmd)
                        icon_string = GHApp.AppResourceName + ".Assets.UI.offer.png";
                    else if (cmddefchar == PrayCmd)
                        icon_string = GHApp.AppResourceName + ".Assets.UI.pray.png";
                    else if (cmddefchar == DipCmd)
                        icon_string = GHApp.AppResourceName + ".Assets.UI.dip.png";
                    else if (cmddefchar == DigCmd)
                        icon_string = GHApp.AppResourceName + ".Assets.UI.dig.png";
                    else if (cmddefchar == SitCmd)
                        icon_string = GHApp.AppResourceName + ".Assets.UI.sit.png";
                    else if (cmddefchar == RideCmd)
                        icon_string = GHApp.AppResourceName + ".Assets.UI.ride.png";
                    else if (cmddefchar == PickToBagCmd)
                        icon_string = GHApp.AppResourceName + ".Assets.UI.picktobag.png";
                    else
                        icon_string = GHApp.AppResourceName + ".Assets.UI.missing_icon.png";
                    break;
            }

            LabeledImageButton lib = new LabeledImageButton();
            lib.ImgSourcePath = "resource://" + icon_string;
            lib.LargerFont = false;
            lib.LblText = data.cmd_text;
            lib.SetSideSize(_currentPageWidth, _currentPageHeight);
            lib.GridMargin = new Thickness(lib.ImgWidth / 15, lib.ImgWidth / 30);
            lib.BtnCommand = cmdcurchar;
            lib.BtnClicked += GHButton_Clicked;
            ContextLayout.IsVisible = true;
            ContextLayout.Children.Add(lib);
        }

        public void DisplayFloatingText(DisplayFloatingTextData data)
        {
            lock (_floatingTextLock)
            {
                bool foundanother = false;
                long highestcounter = 0;
                SKPoint speedvector = new SKPoint(0, -1);
                foreach (GHFloatingText fl in _floatingTexts)
                {
                    if (fl.X == data.x && fl.Y == data.y)
                    {
                        foundanother = true;
                        if (fl.CreatedAt > highestcounter)
                        {
                            highestcounter = fl.CreatedAt;
                            speedvector = fl.GetVelocity(highestcounter);
                        }
                    }
                }

                long counter = 0;
                //lock (AnimationTimerLock)
                //{
                //    counter = AnimationTimers.general_animation_counter;
                //}
                lock (_mainCounterLock)
                {
                    counter = _mainCounterValue;
                }

                if (foundanother)
                {
                    float YSpeed = Math.Abs(speedvector.Y);
                    float secs = 0.5f / YSpeed;
                    long ticks = (long)(secs * UIUtils.GetMainCanvasAnimationFrequency(MapRefreshRate));
                    if (counter - highestcounter >= -ticks * 10 && counter - highestcounter < ticks)
                    {
                        counter += ticks - (counter - highestcounter);
                    }
                }

                _floatingTexts.Add(new GHFloatingText(data, counter, this));
            }
        }

        public void DisplayScreenText(DisplayScreenTextData data)
        {
            long countervalue;
            //lock (AnimationTimerLock)
            //{
            //    countervalue = AnimationTimers.general_animation_counter;
            //}
            lock (_mainCounterLock)
            {
                countervalue = _mainCounterValue;
            }
            lock (_screenTextLock)
            {
                _screenText = new GHScreenText(data, countervalue, this);
            }

            if (_currentGame != null)
            {
                ConcurrentQueue<GHResponse> queue;
                if (GHGame.ResponseDictionary.TryGetValue(_currentGame, out queue))
                {
                    queue.Enqueue(new GHResponse(_currentGame, GHRequestType.DisplayScreenText));
                }
            }
        }

        public void DisplayConditionText(DisplayConditionTextData data)
        {
            long counter = 0;
            //lock (AnimationTimerLock)
            //{
            //    counter = AnimationTimers.general_animation_counter;
            //}
            lock (_mainCounterLock)
            {
                counter = _mainCounterValue;
            }

            lock (_conditionTextLock)
            {
                long highestcounter = 0;
                foreach (GHConditionText fl in _conditionTexts)
                {
                    long finishcount = fl.GetFinishCounterValue();
                    if (finishcount > highestcounter)
                    {
                        highestcounter = finishcount;
                    }
                }

                if (highestcounter > 0 && highestcounter > counter)
                {
                    counter = highestcounter;
                }

                _conditionTexts.Add(new GHConditionText(data, counter, this));
            }
        }

        public void DisplayScreenFilter(DisplayScreenFilterData data)
        {
            long counter = 0;
            //lock (AnimationTimerLock)
            //{
            //    counter = AnimationTimers.general_animation_counter;
            //}
            lock (_mainCounterLock)
            {
                counter = _mainCounterValue;
            }

            lock (_screenFilterLock)
            {
                long highestcounter = 0;
                foreach (GHScreenFilter fl in _screenFilters)
                {
                    long finishcount = fl.GetFinishCounterValue();
                    if (finishcount > highestcounter)
                    {
                        highestcounter = finishcount;
                    }
                }

                if (highestcounter > 0 && highestcounter > counter)
                {
                    counter = highestcounter;
                }

                _screenFilters.Add(new GHScreenFilter(data, counter, this));
            }
        }

        public void DisplayGUIEffect(DisplayGUIEffectData data)
        {
            long counter = 0;
            //lock (AnimationTimerLock)
            //{
            //    counter = AnimationTimers.general_animation_counter;
            //}
            lock (_mainCounterLock)
            {
                counter = _mainCounterValue;
            }

            lock (_guiEffectLock)
            {
                foreach (GHGUIEffect eff in _guiEffects)
                {
                    if (eff.X == data.x && eff.Y == data.y)
                    {
                        _guiEffects.Remove(eff);
                        break;
                    }
                }

                _guiEffects.Add(new GHGUIEffect(data, counter, this));
            }
        }


#if GNH_MAUI
        private Color _titleGoldColor = new Color((float)0xD4 / 255, (float)0xA0 / 255, (float)0x17 / 255);
        private Color _popupTransparentBlackColor = new Color(0, 0, 0, (float)0x66 / 255);
        private Color _popupDarkerTransparentBlackColor = new Color(0, 0, 0, (float)0xAA / 255);
#else
        private Color _titleGoldColor = new Color((double)0xD4 / 255, (double)0xA0 / 255, (double)0x17 / 255);
        private Color _popupTransparentBlackColor = new Color(0, 0, 0, (double)0x66 / 255);
        private Color _popupDarkerTransparentBlackColor = new Color(0, 0, 0, (double)0xAA / 255);
#endif
        private GlyphImageSource _popupImageSource = new GlyphImageSource();
        public void DisplayPopupText(DisplayScreenTextData data)
        {
            PopupTitleLabel.Text = data.subtext;
            if ((data.tflags & (ulong)popup_text_flags.POPUP_FLAGS_ADD_QUOTES) != 0)
                PopupLabel.Text = "\"" + data.text + "\"";
            else
                PopupLabel.Text = data.text;

            if (data.style == (int)popup_text_types.POPUP_TEXT_DIALOGUE ||
                data.style == (int)popup_text_types.POPUP_TEXT_ADVICE ||
                data.style == (int)popup_text_types.POPUP_TEXT_MESSAGE ||
                data.style == (int)popup_text_types.POPUP_TEXT_NO_MONSTERS_IN_LIST)
            {
                PopupTitleLabel.TextColor = _titleGoldColor;
                if ((data.tflags & (ulong)popup_text_flags.POPUP_FLAGS_COLOR_TEXT) != 0)
                    PopupLabel.TextColor = UIUtils.NHColor2XColor(data.color, data.attr, false, false);
                else
                    PopupLabel.TextColor = UIUtils.NHColor2XColor((int)nhcolor.NO_COLOR, 0, false, false);
                PopupGrid.BackgroundColor = GHColors.Transparent;
                PopupFrame.BackgroundColor = _popupDarkerTransparentBlackColor;
                if (data.glyph != 0 && data.glyph != GHApp.NoGlyph)
                    PopupTitleLayout.HorizontalOptions = LayoutOptions.StartAndExpand;
                else
                    PopupTitleLayout.HorizontalOptions = LayoutOptions.CenterAndExpand;
            }
            else if (data.style == (int)popup_text_types.POPUP_TEXT_REVIVAL)
            {
                PopupTitleLabel.TextColor = _titleGoldColor;
                if ((data.tflags & (ulong)popup_text_flags.POPUP_FLAGS_COLOR_TEXT) != 0)
                    PopupLabel.TextColor = UIUtils.NHColor2XColor(data.color, data.attr, false, false);
                else
                    PopupLabel.TextColor = UIUtils.NHColor2XColor((int)nhcolor.NO_COLOR, 0, false, false);
                PopupGrid.BackgroundColor = _popupTransparentBlackColor;
                PopupFrame.BackgroundColor = _popupTransparentBlackColor;
                PopupTitleLayout.HorizontalOptions = LayoutOptions.CenterAndExpand;
            }
            else
            {
                if ((data.tflags & (ulong)popup_text_flags.POPUP_FLAGS_COLOR_TEXT) != 0)
                {
                    PopupTitleLabel.TextColor = UIUtils.NHColor2XColor((int)nhcolor.NO_COLOR, 0, false, true);
                    PopupLabel.TextColor = UIUtils.NHColor2XColor(data.color, data.attr, false, false);
                }
                else
                {
                    PopupTitleLabel.TextColor = UIUtils.NHColor2XColor(data.color, data.attr, false, true);
                    PopupLabel.TextColor = UIUtils.NHColor2XColor((int)nhcolor.NO_COLOR, 0, false, false);
                }
                PopupGrid.BackgroundColor = _popupTransparentBlackColor;
                PopupFrame.BackgroundColor = _popupTransparentBlackColor;
                PopupTitleLayout.HorizontalOptions = LayoutOptions.CenterAndExpand;
            }

            PopupImage.Source = null;
            if (data.glyph != 0 && data.glyph != GHApp.NoGlyph)
            {
                _popupImageSource.ReferenceGamePage = this;
                _popupImageSource.UseUpperSide = (data.tflags & (ulong)popup_text_flags.POPUP_FLAGS_UPPER_SIDE) != 0;
                _popupImageSource.Glyph = data.glyph;
                _popupImageSource.AutoSize = true;
                PopupImage.ActiveGlyphImageSource = _popupImageSource;
                PopupImage.IsVisible = true;
            }
            else
            {
                PopupImage.IsVisible = false;
            }
            PopupGrid.IsVisible = true;
        }

        private void ContextButton_Clicked(object sender, EventArgs e)
        {
            int idx = 0;
#if GNH_MAUI
            idx = ContextLayout.Children.IndexOf((Microsoft.Maui.Controls.View)sender);
            if (idx < 0)
                idx = ContextLayout.Children.IndexOf((Microsoft.Maui.Controls.View)((Microsoft.Maui.Controls.View)sender).Parent);
#else
            idx = ContextLayout.Children.IndexOf((Xamarin.Forms.View)sender);
            if (idx < 0)
                idx = ContextLayout.Children.IndexOf((Xamarin.Forms.View)((Xamarin.Forms.View)sender).Parent);
#endif
            if (idx >= 0 && idx < _contextMenuData.Count)
            {
                int resp = _contextMenuData[idx].cmd_cur_char;
                GenericButton_Clicked(sender, e, resp);
            }
        }


        private /*async*/ void ContentPage_Appearing(object sender, EventArgs e)
        {
            GHApp.BackButtonPressed += BackButtonPressed;
            lock (RefreshScreenLock)
            {
                RefreshScreen = true;
            }

            GameMenuButton.IsEnabled = true;
            SimpleGameMenuButton.IsEnabled = true;
            lMoreButton.IsEnabled = true;

            if (_isFirstAppearance)
            {
                _isFirstAppearance = false;
            }
        }

        protected void GNHThreadProc()
        {
            _currentGame = new GHGame(this);
            GHApp.CurrentGHGame = _currentGame;
            _gnollHackService.StartGnollHack(_currentGame);
        }

        protected void GNHThreadProcForRestart()
        {
            _currentGame = new GHGame(this);
            _currentGame.StartFlags = RunGnollHackFlags.ForceLastPlayerName;
            GHApp.CurrentGHGame = _currentGame;
            _gnollHackService.StartGnollHack(_currentGame);
        }

        private void pollRequestQueue()
        {
            if (_currentGame != null)
            {
                GHRequest req;
                ConcurrentQueue<GHRequest> queue;
                if (GHGame.RequestDictionary.TryGetValue(_currentGame, out queue))
                {
                    while (queue.TryDequeue(out req))
                    {
                        switch (req.RequestType)
                        {
                            case GHRequestType.PrintHistory:
                                PrintHistory(req.MessageHistory);
                                break;
                            case GHRequestType.PrintTopLine:
                                PrintTopLine(req.RequestString, req.RequestStringAttributes);
                                break;
                            case GHRequestType.ShowYnResponses:
                                ShowYnResponses(req.RequestInt, req.RequestAttr, req.RequestNhColor, req.RequestGlyph, req.TitleString, req.RequestString, req.Responses, req.ResponseDescriptions, req.IntroLineString, req.RequestFlags);
                                break;
                            case GHRequestType.HideYnResponses:
                                HideYnResponses();
                                break;
                            case GHRequestType.ShowDirections:
                                ShowDirections();
                                break;
                            case GHRequestType.HideDirections:
                                HideDirections();
                                break;
                            case GHRequestType.GetChar:
                                GetChar();
                                break;
                            case GHRequestType.AskName:
                                AskName(req.RequestString, req.RequestString2);
                                break;
                            case GHRequestType.GetLine:
                                GetLine(req.RequestString, req.PlaceHolderString, req.DefValueString, req.IntroLineString, req.RequestInt, req.RequestAttr, req.RequestNhColor);
                                break;
                            case GHRequestType.ReturnToMainMenu:
                                IsGameOn = false;
                                ClearMap();
                                _currentGame = null;
                                GHApp.CurrentGHGame = null;
                                _mainPage.GameStarted = false;
                                if (canvasView.AnimationIsRunning("GeneralAnimationCounter"))
                                    canvasView.AbortAnimation("GeneralAnimationCounter");
                                if (CommandCanvas.AnimationIsRunning("GeneralAnimationCounter"))
                                    CommandCanvas.AbortAnimation("GeneralAnimationCounter");
                                if (MenuCanvas.AnimationIsRunning("GeneralAnimationCounter"))
                                    MenuCanvas.AbortAnimation("GeneralAnimationCounter");
                                if (TextCanvas.AnimationIsRunning("GeneralAnimationCounter"))
                                    TextCanvas.AbortAnimation("GeneralAnimationCounter");
                                _mapUpdateStopWatch.Stop();
                                ReturnToMainMenu();
                                break;
                            case GHRequestType.RestartGame:
                                RestartGame();
                                break;
                            case GHRequestType.ShowMenuPage:
                                ShowMenuCanvas(req.RequestMenuInfo != null ? req.RequestMenuInfo : new GHMenuInfo(ghmenu_styles.GHMENU_STYLE_GENERAL), req.RequestingGHWindow);
                                break;
                            case GHRequestType.HideMenuPage:
                                DelayedMenuHide();
                                break;
                            case GHRequestType.ShowOutRipPage:
                                ShowOutRipPage(req.RequestOutRipInfo != null ? req.RequestOutRipInfo : new GHOutRipInfo("", 0, "", ""), req.RequestingGHWindow);
                                break;
                            case GHRequestType.CreateWindowView:
                                CreateWindowView(req.RequestInt);
                                break;
                            case GHRequestType.DestroyWindowView:
                                DestroyWindowView(req.RequestInt);
                                break;
                            case GHRequestType.ClearWindowView:
                                ClearWindowView(req.RequestInt);
                                break;
                            case GHRequestType.DisplayWindowView:
                                DisplayWindowView(req.RequestInt, req.RequestPutStrItems);
                                break;
                            case GHRequestType.HideLoadingScreen:
                                HideLoadingScreen();
                                break;
                            case GHRequestType.ClearContextMenu:
                                ClearContextMenu();
                                break;
                            case GHRequestType.AddContextMenu:
                                AddContextMenu(req.ContextMenuData);
                                break;
                            case GHRequestType.DisplayFloatingText:
                                DisplayFloatingText(req.FloatingTextData);
                                break;
                            case GHRequestType.DisplayScreenText:
                                DisplayScreenText(req.ScreenTextData);
                                break;
                            case GHRequestType.DisplayPopupText:
                                DisplayPopupText(req.ScreenTextData);
                                break;
                            case GHRequestType.HidePopupText:
                                HidePopupGrid();
                                break;
                            case GHRequestType.DisplayGUIEffect:
                                DisplayGUIEffect(req.GUIEffectData);
                                break;
                            case GHRequestType.ShowSkillButton:
                                //lSkillButton.IsVisible = true;
                                break;
                            case GHRequestType.HideSkillButton:
                                //lSkillButton.IsVisible = false;
                                break;
                            case GHRequestType.FadeToBlack:
                                FadeToBlack((uint)req.RequestInt);
                                break;
                            case GHRequestType.FadeFromBlack:
                                FadeFromBlack((uint)req.RequestInt);
                                break;
                            case GHRequestType.ShowGUITips:
                                ShowGUITips(true);
                                break;
                            case GHRequestType.CrashReport:
                                ReportCrashDetected();
                                break;
                            case GHRequestType.Panic:
                                ReportPanic(req.RequestString);
                                break;
                            case GHRequestType.Message:
                                ShowMessage(req.RequestString);
                                break;
                            case GHRequestType.YnConfirmation:
                                YnConfirmation(req.TitleString, req.RequestString, req.RequestString2, req.DefValueString);
                                break;
                            case GHRequestType.DisplayConditionText:
                                DisplayConditionText(req.ConditionTextData);
                                break;
                            case GHRequestType.DisplayScreenFilter:
                                DisplayScreenFilter(req.ScreenFilterData);
                                break;
                            case GHRequestType.SaveAndDisableTravelMode:
                                _savedMapTravelMode = MapTravelMode;
                                if (MapTravelMode)
                                    ToggleTravelModeButton_Clicked(ToggleTravelModeButton, new EventArgs());
                                break;
                            case GHRequestType.RestoreTravelMode:
                                if (MapTravelMode != _savedMapTravelMode)
                                    ToggleTravelModeButton_Clicked(ToggleTravelModeButton, new EventArgs());
                                break;
                            case GHRequestType.SaveAndDisableTravelModeOnLevel:
                                _savedMapTravelModeOnLevel = MapTravelMode;
                                if (MapTravelMode)
                                    ToggleTravelModeButton_Clicked(ToggleTravelModeButton, new EventArgs());
                                break;
                            case GHRequestType.RestoreTravelModeOnLevel:
                                if (MapTravelMode != _savedMapTravelModeOnLevel)
                                    ToggleTravelModeButton_Clicked(ToggleTravelModeButton, new EventArgs());
                                break;
                            case GHRequestType.PostDiagnosticData:
                            case GHRequestType.PostGameStatus:
                                PostToForum(req.RequestType == GHRequestType.PostGameStatus, req.RequestInt, req.RequestInt2, req.RequestString);
                                break;
                            case GHRequestType.DebugLog:
                                DisplayDebugLog(req.RequestString, req.RequestInt, req.RequestInt2);
                                break;
                            case GHRequestType.CloseAllDialogs:
                                CloseAllDialogs();
                                break;
                        }
                    }
                }
            }
        }

        private void CloseAllDialogs()
        {
            if (MenuCanvas.AnimationIsRunning("GeneralAnimationCounter"))
                MenuCanvas.AbortAnimation("GeneralAnimationCounter");
            if (TextCanvas.AnimationIsRunning("GeneralAnimationCounter"))
                TextCanvas.AbortAnimation("GeneralAnimationCounter");

            TextGrid.IsVisible = false;
            MenuGrid.IsVisible = false;
            MenuWindowGlyphImage.StopAnimation();
            MenuCountBackgroundGrid.IsVisible = false;
            GetLineGrid.IsVisible = false;
            PopupGrid.IsVisible = false;
            TextWindowGlyphImage.StopAnimation();
            YnGrid.IsVisible = false;

            if (!LoadingGrid.IsVisible && (!MainGrid.IsVisible || !canvasView.AnimationIsRunning("GeneralAnimationCounter")))
            {
                MainGrid.IsVisible = true;
                lock (RefreshScreenLock)
                {
                    RefreshScreen = true;
                }
                StartMainCanvasAnimation();
            }
        }

        private async void DisplayDebugLog(string log_str, int log_type, int log_param)
        {
            if (log_str != null)
                Debug.WriteLine("DebugLog: " + log_str + ", Type: " + log_type + ", Param: " + log_param);
            else
                return;

            if (GHApp.DebugLogMessages && log_str != "")
            {
#if DEBUG
                string titlestring = "Debug Log (D)";
#else
                string titlestring = "Debug Log (R)";
#endif
                switch (log_type)
                {
                    default:
                    case (int)debug_log_types.DEBUGLOG_GENERAL: /* Both release and debug modes */
                        await DisplayAlert(titlestring, log_str, "OK");
                        break;
                    case (int)debug_log_types.DEBUGLOG_DEBUG_ONLY: /* Debug mode only */
#if DEBUG
                        await DisplayAlert(titlestring, log_str, "OK");
#endif
                        break;
                    case (int)debug_log_types.DEBUGLOG_FILE_DESCRIPTOR:
                        break;
                }
            }
        }

        private List<ForumPostAttachment> _forumPostAttachments = new List<ForumPostAttachment>();

        private async void PostToForum(bool is_game_status, int status_type, int status_datatype, string status_string)
        {
            if(!is_game_status && 
                (status_type == (int)diagnostic_data_types.DIAGNOSTIC_DATA_CREATE_ATTACHMENT_FROM_TEXT 
                 || status_type == (int)diagnostic_data_types.DIAGNOSTIC_DATA_ATTACHMENT))
            {
                /* Bypass send checks */
            }
            else if (!is_game_status && !GHApp.PostingDiagnosticData && status_type == (int)diagnostic_data_types.DIAGNOSTIC_DATA_CRITICAL )
            {
                /* Critical information -- Ask the player */
                bool sendok = await DisplayAlert("Critical Diagnostic Data", "GnollHack would like to send critical diagnostic data to the development team. Allow?", "Yes", "No");
                if (!sendok)
                {
                    goto cleanup;
                }
            }
            else
            {
                if (is_game_status ? !GHApp.PostingGameStatus : !GHApp.PostingDiagnosticData)
                    return;
            }

            if (is_game_status && status_string != null && status_string != "" && status_type == (int)game_status_types.GAME_STATUS_RESULT_ATTACHMENT)
            {
                switch(status_datatype)
                {
                    case (int)game_status_data_types.GAME_STATUS_ATTACHMENT_GENERIC:
                        _forumPostAttachments.Add(new ForumPostAttachment(status_string, "application/zip", "game data", !is_game_status, status_type, false));
                        break;
                    case (int)game_status_data_types.GAME_STATUS_ATTACHMENT_DUMPLOG_TEXT:
                        _forumPostAttachments.Add(new ForumPostAttachment(status_string, "text/plain", "dumplog", !is_game_status, status_type, false));
                        break;
                    case (int)game_status_data_types.GAME_STATUS_ATTACHMENT_DUMPLOG_HTML:
                        _forumPostAttachments.Add(new ForumPostAttachment(status_string, "text/html", "HTML dumplog", !is_game_status, status_type, false));
                        break;
                }
                return;
            }
            else if(!is_game_status && status_string != null && status_string != "" && status_type == (int)diagnostic_data_types.DIAGNOSTIC_DATA_ATTACHMENT)
            {
                switch (status_datatype)
                {
                    case (int)diagnostic_data_attachment_types.DIAGNOSTIC_DATA_ATTACHMENT_GENERIC:
                        _forumPostAttachments.Add(new ForumPostAttachment(status_string, "application/zip", "diagnostic data", !is_game_status, status_type, false));
                        break;
                    case (int)diagnostic_data_attachment_types.DIAGNOSTIC_DATA_ATTACHMENT_FILE_DESCRIPTOR_LIST:
                        _forumPostAttachments.Add(new ForumPostAttachment(status_string, "text/plain", "file descriptor list", !is_game_status, status_type, true));
                        break;
                }
                return;
            }
            else if (!is_game_status && status_string != null && status_string != "" && status_type == (int)diagnostic_data_types.DIAGNOSTIC_DATA_CREATE_ATTACHMENT_FROM_TEXT)
            {
                if (status_datatype == (int)diagnostic_data_attachment_types.DIAGNOSTIC_DATA_ATTACHMENT_FILE_DESCRIPTOR_LIST)
                    status_string = status_string.Replace(" | ", Environment.NewLine);

                string tempdirpath = Path.Combine(GHApp.GHPath, "temp");
                if (!Directory.Exists(tempdirpath))
                   GHApp.CheckCreateDirectory(tempdirpath);
                int number = 0;
                string temp_string;
                do
                {
                    temp_string = Path.Combine(tempdirpath, "tmp_attachment_" + number + ".txt");
                    number++;
                } while (File.Exists(temp_string));

                GHApp.GnollHackService.Chmod(tempdirpath, (uint)ChmodPermissions.S_IALL);
                try
                {
                    using (FileStream fs = new FileStream(temp_string, FileMode.Create))
                    {
                        using (StreamWriter sw = new StreamWriter(fs))
                        {
                            sw.Write(status_string);
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
                _forumPostAttachments.Add(new ForumPostAttachment(temp_string, "text/plain", "diagnostic data", !is_game_status, status_type, true));
                return;
            }

            string message = "";
            if (status_string != null)
                message = status_string;
            if (message == "")
                return;

            if (!is_game_status)
            {
                string ver = GHApp.GHVersionString + " / " + VersionTracking.CurrentVersion + " / " + VersionTracking.CurrentBuild;
                string manufacturer = DeviceInfo.Manufacturer;
                if (manufacturer.Length > 0)
                    manufacturer = manufacturer.Substring(0, 1).ToUpper() + manufacturer.Substring(1);
                string device_model = manufacturer + " " + DeviceInfo.Model;
                string platform = DeviceInfo.Platform + " " + DeviceInfo.VersionString;

                ulong TotalMemInBytes = GHApp.PlatformService.GetDeviceMemoryInBytes();
                ulong TotalMemInMB = (TotalMemInBytes / 1024) / 1024;
                ulong FreeDiskSpaceInBytes = GHApp.PlatformService.GetDeviceFreeDiskSpaceInBytes();
                ulong FreeDiskSpaceInGB = ((FreeDiskSpaceInBytes / 1024) / 1024) / 1024;
                ulong TotalDiskSpaceInBytes = GHApp.PlatformService.GetDeviceTotalDiskSpaceInBytes();
                ulong TotalDiskSpaceInGB = ((TotalDiskSpaceInBytes / 1024) / 1024) / 1024;

                string totmem = TotalMemInMB + " MB";
                string diskspace = FreeDiskSpaceInGB + " GB" + " / " + TotalDiskSpaceInGB + " GB";

                string player_name = Preferences.Get("LastUsedPlayerName", "Unknown Player");
                string info = ver + ", " + platform + ", " + device_model + ", " + totmem + ", " + diskspace;

                switch(status_type)
                {
                    case (int)diagnostic_data_types.DIAGNOSTIC_DATA_PANIC:
                        message = player_name + " - Panic: " + message + " [" + info + "]";
                        break;
                    case (int)diagnostic_data_types.DIAGNOSTIC_DATA_IMPOSSIBLE:
                        message = player_name + " - Impossible: " + message + " [" + info + "]";
                        break;
                    case (int)diagnostic_data_types.DIAGNOSTIC_DATA_CRITICAL:
                        message = player_name + " - Critical:\n" + message + "\n[" + info + "]";
                        break;
                    default:
                        message = player_name + " - Diagnostics: " + message + " [" + info + "]";
                        break;
                }
            }
            else
            {
                string portver = VersionTracking.CurrentVersion;
                DevicePlatform platform = DeviceInfo.Platform;
                string platstr = platform != null ? platform.ToString() : "";
                if (platstr == null)
                    platstr = "";
                string platid;
                if (platstr.Length > 0)
                    platid = platstr.Substring(0, 1).ToLower();
                else
                    platid = "";

                switch (status_type)
                {
                    default:
                        message = message + " [" + portver + platid + "]";
                        break;
                }
            }

            try
            {
                string postaddress = is_game_status ? GHApp.GetGameStatusPostAddress() : GHApp.GetDiagnosticDataPostAddress();
                if (postaddress != null && postaddress.Length > 8 && postaddress.Substring(0, 8) == "https://" && Uri.IsWellFormedUriString(postaddress, UriKind.Absolute))
                {
                    using (HttpClient client = new HttpClient { Timeout = TimeSpan.FromDays(1) })
                    {
                        HttpContent content = null;
                        if (_forumPostAttachments.Count > 0)
                        {
                            DiscordWebHookPostWithAttachment post = new DiscordWebHookPostWithAttachment(message);
                            foreach (ForumPostAttachment attachment in _forumPostAttachments)
                            {
                                FileInfo fileinfo = new FileInfo(attachment.FullPath);
                                string filename = fileinfo.Name;
                                if (File.Exists(attachment.FullPath))
                                    post.AddAttachment(attachment.Description, filename);
                            }
                            string json = JsonConvert.SerializeObject(post);
                            MultipartFormDataContent multicontent = new MultipartFormDataContent("--boundary");
                            StringContent content1 = new StringContent(json, Encoding.UTF8, "application/json");
                            ContentDispositionHeaderValue cdhv = new ContentDispositionHeaderValue("form-data");
                            cdhv.Name = "payload_json";
                            content1.Headers.ContentDisposition = cdhv;
                            multicontent.Add(content1);
                            int aidx = 0;
                            foreach (ForumPostAttachment attachment in _forumPostAttachments)
                            {
                                bool fileexists = File.Exists(attachment.FullPath);
                                if (fileexists)
                                {
                                    FileInfo fileinfo = new FileInfo(attachment.FullPath);
                                    string filename = fileinfo.Name;
                                    var stream = new FileStream(attachment.FullPath, FileMode.Open);
                                    StreamContent content2 = new StreamContent(stream);
                                    //byte[] bytes = new byte[stream.Length];
                                    //int bytesread = await stream.ReadAsync(bytes, 0, bytes.Length);
                                    //ByteArrayContent content2 = new ByteArrayContent(bytes);
                                    ContentDispositionHeaderValue cdhv2 = new ContentDispositionHeaderValue("form-data");
                                    cdhv2.Name = "files[" + aidx + "]";
                                    cdhv2.FileName = filename;
                                    content2.Headers.ContentDisposition = cdhv2;
                                    content2.Headers.ContentType = new MediaTypeHeaderValue(attachment.ContentType);
                                    multicontent.Add(content2);
                                    aidx++;
                                }
                            }
                            content = multicontent;
                        }
                        else
                        {
                            DiscordWebHookPost post = new DiscordWebHookPost(message);
                            string json = JsonConvert.SerializeObject(post);
                            content = new StringContent(json, Encoding.UTF8, "application/json");
                        }

                        using (var cts = new CancellationTokenSource())
                        {
                            cts.CancelAfter(is_game_status || _forumPostAttachments.Count == 0 ? 10000 : 120000);
                            string jsonResponse = "";
                            using (HttpResponseMessage response = await client.PostAsync(postaddress, content, cts.Token))
                            {
                                jsonResponse = await response.Content.ReadAsStringAsync();
                                Debug.WriteLine(jsonResponse);
                            }
                        }
                        content.Dispose();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }

        cleanup:
            foreach(var attachment in _forumPostAttachments)
            {
                if(attachment.IsTemporary)
                {
                    try
                    {
                        File.Delete(attachment.FullPath);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                    }
                }
            }
            _forumPostAttachments.Clear();
            return;
        }

        private void CreateWindowView(int winid)
        {

        }

        private void DestroyWindowView(int winid)
        {

        }

        private void ClearWindowView(int winid)
        {

        }

        private void DisplayWindowView(int winid, List<GHPutStrItem> strs)
        {
            GHWindow window;
            lock (_currentGame.WindowsLock)
            {
                window = _currentGame.Windows[winid];
            }
            if(window != null)
                ShowWindowCanvas(window, strs);
        }

        private void ShowWindowCanvas(GHWindow window, List<GHPutStrItem> strs)
        {
            /* Cancel delayed text hide */
            lock(_delayedTextHideLock)
            {
                _delayedTextHideCancelled = true;
            }

            /* Cancel delayed touch hide */
            bool dohidemenu = false;
            lock(_menuHideCancelledLock)
            {
                if (_menuHideOn)
                {
                    _menuHideCancelled = true;
                    dohidemenu = true;
                }
            }

            /* On iOS, hide TextStack to start fade in */
            if (GHApp.IsiOS)
            {
                TextStack.IsVisible = false;
            }

            lock (RefreshScreenLock)
            {
                RefreshScreen = false;
            }

            lock (_textScrollLock)
            {
                _textScrollOffset = 0;
                _textScrollSpeed = 0;
                _textScrollSpeedOn = false;
                _textScrollSpeedRecords.Clear();
            }

            TextWindowGlyphImage.Source = null;

            _textGlyphImageSource.ReferenceGamePage = this;
            _textGlyphImageSource.AutoSize = true;
            _textGlyphImageSource.ObjData = window.ObjData;
            _textGlyphImageSource.Glyph = window.Glyph;
            _textGlyphImageSource.UseUpperSide = window.UseUpperSide;

            TextWindowGlyphImage.ActiveGlyphImageSource = TextGlyphImage;
            TextWindowGlyphImage.IsVisible = IsTextGlyphVisible;

            List<GHPutStrItem> items = null;
            if (window.WindowStyle == ghwindow_styles.GHWINDOW_STYLE_PAGER_GENERAL || window.WindowStyle == ghwindow_styles.GHWINDOW_STYLE_PAGER_SPEAKER 
                || window.WindowStyle == ghwindow_styles.GHWINDOW_STYLE_HAS_INDENTED_TEXT || window.WindowStyle == ghwindow_styles.GHWINDOW_STYLE_DISPLAY_FILE_WITH_INDENTED_TEXT)
            {
                items = new List<GHPutStrItem>();
                UIUtils.ProcessAdjustedItems(items, strs);
            }
            else
                items = strs;

            lock (TextCanvas.TextItemLock)
            {
                TextTouchDictionary.Clear();
                TextCanvas.GHWindow = window;

                TextCanvas.PutStrItems = items;
            }

            if (GHApp.IsiOS)
            {
                /* On iOS, fade in the text window. NOTE: this was originally a work-around for bad layout performance on iOS */
                Device.StartTimer(TimeSpan.FromSeconds(1.0 / 20), () =>
                {
                    if (TextStack.AnimationIsRunning("TextHideAnimation"))
                        TextStack.AbortAnimation("TextHideAnimation");
                    TextStack.Opacity = 0.0;
                    TextStack.IsVisible = true;
                    Animation textAnimation = new Animation(v => TextStack.Opacity = (double)v, 0.0, 1.0);
                    textAnimation.Commit(TextStack, "TextShowAnimation", length: 256,
                        rate: 16, repeat: () => false);

                    TextGrid.IsVisible = true;
                    MainGrid.IsVisible = false;
                    if (dohidemenu)
                    {
                        MenuGrid.IsVisible = false;
                    }
#if !GNH_MAUI
                    TextStack.ForceLayout();
#endif
                    return false;
                });
            }
            else
            {
                TextGrid.IsVisible = true;
                MainGrid.IsVisible = false;
                if (dohidemenu)
                {
                    MenuGrid.IsVisible = false;
                }
            }

            if (canvasView.AnimationIsRunning("GeneralAnimationCounter"))
                canvasView.AbortAnimation("GeneralAnimationCounter");
            _mapUpdateStopWatch.Stop();
            StartTextCanvasAnimation();
        }


        private GlyphImageSource _menuGlyphImageSource = new GlyphImageSource();

        public GlyphImageSource MenuGlyphImage
        {
            get
            {
                return _menuGlyphImageSource;
            }
        }

        public bool IsMenuGlyphVisible
        {
            get
            {
                return (Math.Abs(_menuGlyphImageSource.Glyph) > 0 && _menuGlyphImageSource.Glyph != GHApp.NoGlyph);
            }
        }


        private void PrintTopLine(string str, uint attributes)
        {

        }

        private GlyphImageSource _ynImageSource = new GlyphImageSource();
        private void ShowYnResponses(int style, int attr, int color, int glyph, string title, string question, string responses, string descriptions, string introline, ulong ynflags)
        {
            string[] descr_list = null;
            if (descriptions != null)
            {
                descr_list = descriptions.Split('\n');
            }

            /* Title Label */
            if (title == null)
            {
                YnTitleLabel.IsVisible = false;
                YnTitleLabel.Text = "";
                YnTitleLabel.TextColor = GHColors.White;
            }
            else
            {
                YnTitleLabel.Text = title;
                YnTitleLabel.IsVisible = true;
                YnQuestionLabel.TextColor = GHColors.White;
                switch (style)
                {
                    case (int)yn_function_styles.YN_STYLE_MONSTER_QUESTION:
                        YnTitleLabel.TextColor = _titleGoldColor;
                        break;
                    default:
                        YnTitleLabel.TextColor = UIUtils.NHColor2XColor(color, attr, false, true);
                        break;
                }
            }

            /* Title Glyph */
            YnImage.Source = null;
            if (glyph != 0 && glyph != GHApp.NoGlyph)
            {
                YnTitleLayout.HorizontalOptions = LayoutOptions.StartAndExpand;
                _ynImageSource.ReferenceGamePage = this;
                _ynImageSource.UseUpperSide = (ynflags & 1) != 0;
                _ynImageSource.Glyph = glyph;
                _ynImageSource.AutoSize = true;
                YnImage.ActiveGlyphImageSource = _ynImageSource;
                YnImage.IsVisible = true;
            }
            else
            {
                YnTitleLayout.HorizontalOptions = LayoutOptions.CenterAndExpand;
                YnImage.IsVisible = false;
            }

            if (!YnImage.IsVisible && !YnTitleLabel.IsVisible)
                YnTitleLayout.IsVisible = false;
            else
                YnTitleLayout.IsVisible = true;

            /* Question */
            if(string.IsNullOrWhiteSpace(introline))
                YnQuestionLabel.Text = "";
            else
                YnQuestionLabel.Text = introline + " ";

            YnQuestionLabel.Text += question;

            /* Buttons */
            LabeledImageButton[] btnList = { ZeroButton, FirstButton, SecondButton, ThirdButton, FourthButton };
            if (responses.Length == 0)
                return;
            else
            {
                for (int i = 0; i < 5; i++)
                {
                    if (i < responses.Length)
                    {
                        btnList[i].BtnLetter = responses[i];
                        if (descriptions != null && descr_list.Length > i)
                            btnList[i].LblText = descr_list[i];
                        else
                            btnList[i].LblText = btnList[i].BtnLetter.ToString();
                        btnList[i].IsVisible = true;
                    }
                    else
                    {
                        btnList[i].BtnLetter = '?';
                        btnList[i].LblText = "?";
                        btnList[i].IsVisible = false;
                    }
                    btnList[i].ImgSourcePath = GetYnImgSourcePath(btnList[i].BtnLetter, btnList[i].LblText);
                }
            }

            for (int i = 0; i < 5; i++)
                btnList[i].SetSideSize(_currentPageWidth, _currentPageHeight);

            YnButtonStack.HeightRequest = btnList[0].GridHeight;
            switch(style)
            {
                case (int)yn_function_styles.YN_STYLE_END:
                    YnGrid.BackgroundColor = GHColors.VeryTransparentBlack;
                    YnFrame.BackgroundColor = GHColors.LessTransparentBlack;
                    break;
                default:
                    YnGrid.BackgroundColor = GHColors.SemiTransparentBlack;
                    YnFrame.BackgroundColor = GHColors.SemiTransparentBlack;
                    break;
            }
            YnGrid.IsVisible = true;
        }

        private string GetYnImgSourcePath(char ch, string desc)
        {
            string res = "resource://" + GHApp.AppResourceName + ".Assets.UI.missing_icon.png";
            switch (ch)
            {
                case 'm':
                    res = "resource://" + GHApp.AppResourceName + ".Assets.UI.name.png";
                    break;
                case 's':
                case 'i':
                    res = "resource://" + GHApp.AppResourceName + ".Assets.UI.inventory.png";
                    break;
                case 'd':
                    if (desc != null && desc.Length >= 4 && desc.Substring(0, 4) == "Drop")
                        res = "resource://" + GHApp.AppResourceName + ".Assets.UI.dropmany.png";
                    else if (desc == "Disarm")
                        res = "resource://" + GHApp.AppResourceName + ".Assets.UI.yes.png";
                    break;
                case 'y':
                    res = "resource://" + GHApp.AppResourceName + ".Assets.UI.yes.png";
                    break;
                case 'n':
                    res = "resource://" + GHApp.AppResourceName + ".Assets.UI.no.png";
                    break;
                case 'q':
                    res = "resource://" + GHApp.AppResourceName + ".Assets.UI.cancel.png";
                    break;
                case 'a':
                    if (desc == "Auto")
                        res = "resource://" + GHApp.AppResourceName + ".Assets.UI.autostash.png";
                    else
                        res = "resource://" + GHApp.AppResourceName + ".Assets.UI.yestoall.png";
                    break;
                case 'r':
                    res = "resource://" + GHApp.AppResourceName + ".Assets.UI.rightring.png";
                    break;
                case 'l':
                    if (desc == "Load")
                        res = "resource://" + GHApp.AppResourceName + ".Assets.UI.load.png";
                    else
                        res = "resource://" + GHApp.AppResourceName + ".Assets.UI.leftring.png";
                    break;
                default:
                    break;
            }
            return res;
        }

        private void HideYnResponses()
        {
            YnGrid.IsVisible = false;
        }
        private void ShowDirections()
        {
            _showDirections = true;
            ShowNumberPad = false;
        }
        private void HideDirections()
        {
            _showDirections = false;
            ShowNumberPad = false;
        }
        public void DoShowNumberPad()
        {
            if (!_showDirections)
                ShowNumberPad = true;
        }

        public void ShowGUITips(bool is_game_start)
        {
            _blockingTipView = is_game_start;
            ShownTip = is_game_start ? 0 : 1;
            TipView.IsVisible = true;
            TipView.InvalidateSurface();
        }
        private readonly object _msgHistoryLock = new object();
        private List<GHMsgHistoryItem> _msgHistory = null;
        private void PrintHistory(List<GHMsgHistoryItem> msgHistory)
        {
            lock (_msgHistoryLock)
            {
                _msgHistory = msgHistory;
            }
        }

        private async void AskName(string modeName, string modeDescription)
        {
            var namePage = new NamePage(this, modeName, modeDescription);
            await App.Current.MainPage.Navigation.PushModalAsync(namePage);
        }

        private int _getLineStyle = 0;
        private Regex _getLineRegex = null;
        private void GetLine(string query, string placeholder, string linesuffix, string introline, int style, int attr, int color)
        {
            GetLineFrame.BorderColor = GHColors.Black;
            GetLineOkButton.IsEnabled = true;
            GetLineCancelButton.IsEnabled = true;
            GetLineQuestionMarkButton.IsEnabled = true;

            Color clr = UIUtils.NHColor2XColor(color, attr, false, false); /* Non-title / white coloring works better here */
            string PlaceHolderText = null;
            if (!string.IsNullOrWhiteSpace(placeholder) && placeholder.Length > 0)
            {
                PlaceHolderText = char.ToUpper(placeholder[0]) + placeholder.Substring(1);
            }

            if (!string.IsNullOrWhiteSpace(introline))
                GetLineCaption.Text = introline + " ";
            else
                GetLineCaption.Text = "";

            GetLineCaption.Text += query;
            if (!string.IsNullOrWhiteSpace(linesuffix) && linesuffix != " -")
                GetLineCaption.Text += " " + linesuffix;

            GetLineCaption.TextColor = clr;
            GetLineEntryText.Text = "";
            GetLineEntryText.MaxLength = GHConstants.BUFSZ - 1;
            GetLineQuestionMarkButton.IsVisible = false;
            GetLineEntryText.IsVisible = true;
            GetLineEntryText.Keyboard = Keyboard.Default;
            GetLineEntryText.WidthRequest = 320;
            GetLineAutoComplete.Text = "";
            GetLineAutoComplete.IsVisible = false;

            _getLineStyle = style;
            _getLineRegex = null;

            switch (style)
            {
                case (int)getline_types.GETLINE_EXTENDED_COMMAND:
                    GetLineEntryText.WidthRequest = 230;
                    GetLineQuestionMarkButton.IsVisible = true;
                    GetLineAutoComplete.IsVisible = true;
                    GetLineEntryText.Placeholder = "Type the command";
                    _getLineRegex = new Regex(@"^[A-Za-z0-9_]{0,64}$");
                    break;
                case (int)getline_types.GETLINE_LEVELPORT:
                    GetLineEntryText.Placeholder = "Type the level here";
                    GetLineEntryText.Keyboard = Keyboard.Numeric;
                    _getLineRegex = new Regex(@"^[A-Za-z0-9_? ]{0,32}$");
                    /* '*' could be possible as well, but not implemented at the moment */
                    break;
                case (int)getline_types.GETLINE_WIZ_LEVELPORT:
                    GetLineEntryText.WidthRequest = 230;
                    GetLineQuestionMarkButton.IsVisible = true;
                    GetLineEntryText.Placeholder = "Type the level";
                    GetLineEntryText.Keyboard = Keyboard.Numeric;
                    _getLineRegex = new Regex(@"^[A-Za-z0-9_? ]{0,32}$");
                    break;
                case (int)getline_types.GETLINE_LEVEL_CHANGE:
                case (int)getline_types.GETLINE_NUMBERS_ONLY:
                    GetLineEntryText.WidthRequest = 240;
                    GetLineEntryText.Keyboard = Keyboard.Numeric;
                    if (style == (int)getline_types.GETLINE_LEVEL_CHANGE)
                        GetLineEntryText.Placeholder = "Type the level here";
                    else
                        GetLineEntryText.Placeholder = "Type the number here";
                    _getLineRegex = new Regex(@"^[A-Za-z0-9_? ]{0,32}$");
                    break;
                case (int)getline_types.GETLINE_WISHING:
                    GetLineEntryText.Placeholder = "Type your wish here";
                    _getLineRegex = new Regex(@"^[A-Za-z0-9_ \(\:\)\+\-]{0,128}$");
                    break;
                case (int)getline_types.GETLINE_GENESIS:
                case (int)getline_types.GETLINE_POLYMORPH:
                case (int)getline_types.GETLINE_GENOCIDE:
                case (int)getline_types.GETLINE_MONSTER:
                    GetLineEntryText.Placeholder = "Type the monster here";
                    _getLineRegex = new Regex(@"^[A-Za-z0-9_ ]{0,64}$");
                    break;
                case (int)getline_types.GETLINE_MONSTER_CLASS:
                    GetLineEntryText.WidthRequest = 230;
                    GetLineEntryText.MaxLength = 1;
                    GetLineQuestionMarkButton.IsVisible = true;
                    GetLineEntryText.Placeholder = "Type the monster class";
                    _getLineRegex = new Regex(@"^[A-Za-z0-9_ \'\&\#\:\;]{0,64}$");
                    break;
                case (int)getline_types.GETLINE_TUNE:
                    GetLineEntryText.WidthRequest = 240;
                    GetLineEntryText.Placeholder = "Type the tune here";
                    _getLineRegex = new Regex(@"^[A-Za-z]{0,10}$");
                    break;
                case (int)getline_types.GETLINE_QUESTION:
                    GetLineEntryText.Placeholder = "Type the answer here";
                    _getLineRegex = new Regex(@"^[A-Za-z0-9_ \$\*\&\.\,\<\>\=\?\!\#\(\:\;\)\+\-]{0,128}$");
                    break;
                case (int)getline_types.GETLINE_MENU_SEARCH:
                    GetLineEntryText.Placeholder = "Type the search here";
                    _getLineRegex = new Regex(@"^[A-Za-z0-9_ \`\|\~\^\""\'\%\/\\\[\]\{\}\$\*\&\.\,\<\>\=\?\!\#\(\:\;\)\+\-]{0,128}$");
                    break;
                default:
                    if (PlaceHolderText != null)
                        GetLineEntryText.Placeholder = PlaceHolderText;
                    else
                        GetLineEntryText.Placeholder = "Type the text here";
                    _getLineRegex = new Regex(@"^[A-Za-z0-9_ åäöÅÄÖ\$\*\&\.\,\<\>\=\?\!\#\(\:\;\)\+\-]{0,128}$");
                    break;
            }
            GetLineGrid.IsVisible = true;
        }

        private void GetLineOkButton_Clicked(object sender, EventArgs e)
        {
            GetLineOkButton.IsEnabled = false;
            GetLineCancelButton.IsEnabled = false;
            GetLineQuestionMarkButton.IsEnabled = false;
            GHApp.PlayButtonClickedSound();

            string res = GetLineEntryText.Text;
            if (string.IsNullOrEmpty(GetLineEntryText.Text))
            {
                res = "";
            }
            else if (string.IsNullOrWhiteSpace(GetLineEntryText.Text))
            {
                res = " ";
            }
            else
            {
                res.Trim();
            }

            if(_getLineRegex != null && !_getLineRegex.IsMatch(res))
            {
                GetLineFrame.BorderColor = GHColors.Red;
                GetLineEntryText.Focus();
                GetLineOkButton.IsEnabled = true;
                GetLineCancelButton.IsEnabled = true;
                GetLineQuestionMarkButton.IsEnabled = true;
                return;
            }
            GetLineFrame.BorderColor = GHColors.Black;

            /* Style-dependent behavior */
            switch (_getLineStyle)
            {
                case (int)getline_types.GETLINE_EXTENDED_COMMAND:
                    res = res.ToLower();
                    break;
                default:
                    break;
            }

            ConcurrentQueue<GHResponse> queue;
            if (GHGame.ResponseDictionary.TryGetValue(_currentGame, out queue))
            {
                queue.Enqueue(new GHResponse(_currentGame, GHRequestType.GetLine, res));
            }

            GetLineGrid.IsVisible = false;
            GetLineEntryText.Text = "";
            GetLineCaption.Text = "";
        }

        private void GetLineQuestionMarkButton_Clicked(object sender, EventArgs e)
        {
            GetLineOkButton.IsEnabled = false;
            GetLineCancelButton.IsEnabled = false;
            GetLineQuestionMarkButton.IsEnabled = false;
            GHApp.PlayButtonClickedSound();

            string res = "?";
            ConcurrentQueue<GHResponse> queue;
            if (GHGame.ResponseDictionary.TryGetValue(_currentGame, out queue))
            {
                queue.Enqueue(new GHResponse(_currentGame, GHRequestType.GetLine, res));
            }

            GetLineGrid.IsVisible = false;
            GetLineEntryText.Text = "";
            GetLineCaption.Text = "";
        }

        private void GetLineCancelButton_Clicked(object sender, EventArgs e)
        {
            GetLineOkButton.IsEnabled = false;
            GetLineCancelButton.IsEnabled = false;
            GetLineQuestionMarkButton.IsEnabled = false;
            GHApp.PlayButtonClickedSound();
            
            ConcurrentQueue<GHResponse> queue;
            if (GHGame.ResponseDictionary.TryGetValue(_currentGame, out queue))
            {
                queue.Enqueue(new GHResponse(_currentGame, GHRequestType.GetLine, '\x1B'.ToString()));
            }

            GetLineGrid.IsVisible = false;
            GetLineEntryText.Text = "";
            GetLineCaption.Text = "";
        }

        private void GetChar()
        {
            // Set focus to GameViewPage
        }

        public bool MainPageBackgroundNeedsUpdate { get; set; }

        private async void ReturnToMainMenu()
        {
            if(MainPageBackgroundNeedsUpdate)
            {
                _mainPage.UpdateMainScreenBackgroundStyle();
                MainPageBackgroundNeedsUpdate = false;
            }
            _mainPage.ActivateLocalGameButton();
            _mainPage.PlayMainScreenVideoAndMusic(); /* Just to be doubly sure */
            if (GHApp.GameMuteMode)
                GHApp.GameMuteMode = false;
            GHApp.CurrentGamePage = null;
            await App.Current.MainPage.Navigation.PopModalAsync();
        }

        private readonly object _menuDrawOnlyLock = new object();
        private bool _menuDrawOnlyClear = true;
        private bool _menuRefresh = false;

        private void ShowMenuCanvas(GHMenuInfo menuinfo, GHWindow ghwindow)
        {
            /* Cancel delayed menu hide */
            lock (_menuHideCancelledLock)
            {
                if(_menuHideOn)
                {
                    _menuHideCancelled = true;
                }
            }

            MenuCancelButton.IsEnabled = true;
            /* Enabling OKButton is done below */

            /* On iOS, hide MenuStack to start fade in */
            if (GHApp.IsiOS)
            {
                MenuStack.IsVisible = false;
            }

            /* Cancel delayed text hide */
            bool dohidetext = false;
            lock(_delayedTextHideLock)
            {
                if(_delayedTextHideOn)
                {
                    _delayedTextHideCancelled = true;
                    dohidetext = true;
                }
            }

            lock (RefreshScreenLock)
            {
                RefreshScreen = false;
            }

            lock (_menuDrawOnlyLock)
            {
                _menuDrawOnlyClear = true;
                _menuRefresh = false;
            }

            GHApp.DebugWriteProfilingStopwatchTimeAndStart("ShowMenuCanvas Start");
            MenuTouchDictionary.Clear();
            lock(_menuScrollLock)
            {
                _menuScrollSpeed = 0;
                _menuScrollSpeedOn = false;
                _menuScrollSpeedRecords.Clear();
            }

            /* Set headers */
            if (menuinfo.Header == null)
            {
                MenuHeaderLabel.IsVisible = true;
                MenuHeaderLabel.Text = " ";
                MenuHeaderLabel.OutlineWidth = 0;
            }
            else
            {
                MenuHeaderLabel.IsVisible = true;
                MenuHeaderLabel.Text = menuinfo.Header;
                MenuHeaderLabel.OutlineWidth = UIUtils.MenuHeaderOutlineWidth(menuinfo.Style);
            }
            MenuHeaderLabel.FontFamily = UIUtils.MenuHeaderFontFamily(menuinfo.Style);
            MenuHeaderLabel.FontSize = UIUtils.MenuHeaderFontSize(menuinfo.Style);
            MenuHeaderLabel.TextColor = UIUtils.MenuHeaderTextColor(menuinfo.Style);
            MenuHeaderLabel.OutlineColor = UIUtils.MenuHeaderOutlineColor(menuinfo.Style);

            if (menuinfo.Subtitle == null)
            {
                MenuSubtitleLabel.IsVisible = false;
                MenuSubtitleLabel.Text = "";
                MenuSubtitleLabel.OutlineWidth = 0;
                MenuSubtitleLabel.Margin = new Thickness();
            }
            else
            {
                MenuSubtitleLabel.IsVisible = true;
                MenuSubtitleLabel.Text = menuinfo.Subtitle;
                MenuSubtitleLabel.FontFamily = UIUtils.MenuSubtitleFontFamily(menuinfo.Style);
                MenuSubtitleLabel.FontSize = UIUtils.MenuSubtitleFontSize(menuinfo.Style);
                MenuSubtitleLabel.UseSpecialSymbols = UIUtils.MenuSubtitleUsesSpecialSymbols(menuinfo.Style);
                MenuSubtitleLabel.WordWrapSeparator = UIUtils.MenuSubtitleWordWrapSeparator(menuinfo.Style);
                MenuSubtitleLabel.DisplayWrapSeparator = UIUtils.MenuSubtitleDisplayWrapSeparator(menuinfo.Style);
                MenuSubtitleLabel.TextColor = UIUtils.MenuSubtitleTextColor(menuinfo.Style);
                MenuSubtitleLabel.OutlineColor = UIUtils.MenuSubtitleOutlineColor(menuinfo.Style);
                MenuSubtitleLabel.OutlineWidth = UIUtils.MenuSubtitleOutlineWidth(menuinfo.Style);
                MenuSubtitleLabel.Margin = UIUtils.MenuSubtitleMargin(menuinfo.Style, CurrentPageWidth, CurrentPageHeight);
            }

            /* Update canvas */
            MenuCanvas.GHWindow = ghwindow;
            MenuCanvas.MenuStyle = menuinfo.Style;
            MenuCanvas.SelectionHow = menuinfo.SelectionHow;
            MenuCanvas.SelectionIndex = -1;
            if (MenuCanvas.SelectionHow == SelectionMode.Single)
            {
                int idx = -1;
                bool selectedFound = false;
                foreach (GHMenuItem mi in menuinfo.MenuItems)
                {
                    idx++;
                    if (mi.Selected)
                    {
                        mi.Selected = false; /* Clear out, with single selection we are using SelectionIndex */
                        MenuCanvas.SelectionIndex = idx;
                        selectedFound = true;
                        break;
                    }
                }
                MenuOKButton.IsEnabled = selectedFound;
            }
            else
            {
                MenuOKButton.IsEnabled = true;
            }

            switch(menuinfo.Style)
            {
                case ghmenu_styles.GHMENU_STYLE_START_GAME_MENU:
                    MenuBackground.BackgroundStyle = BackgroundStyles.FitToScreen;
                    MenuBackground.BackgroundBitmap = BackgroundBitmaps.LoadingScreen;
                    MenuBackground.BorderStyle = BorderStyles.None;
                    MenuCanvas.RevertBlackAndWhite = false;
                    MenuCanvas.UseTextOutline = true;
                    MenuCanvas.HideMenuLetters = true;
                    MenuCanvas.MenuButtonStyle = true;
                    MenuCanvas.ClickOKOnSelection = true;
                    MenuCanvas.MenuGlyphAtBottom = false;
                    MenuCanvas.AllowLongTap = false;
                    MenuCanvas.AllowHighlight = true;
                    break;
                case ghmenu_styles.GHMENU_STYLE_CHOOSE_DIFFICULTY:
                case ghmenu_styles.GHMENU_STYLE_ACCEPT_PLAYER:
                    MenuBackground.BackgroundStyle = BackgroundStyles.StretchedBitmap;
                    MenuBackground.BackgroundBitmap = BackgroundBitmaps.OldPaper;
                    MenuBackground.BorderStyle = BorderStyles.SimpleAlternative;
                    MenuCanvas.RevertBlackAndWhite = true;
                    MenuCanvas.UseTextOutline = false;
                    MenuCanvas.HideMenuLetters = true;
                    MenuCanvas.MenuButtonStyle = true;
                    MenuCanvas.ClickOKOnSelection = true;
                    MenuCanvas.MenuGlyphAtBottom = true;
                    MenuCanvas.AllowLongTap = false;
                    MenuCanvas.AllowHighlight = true;
                    break;
                case ghmenu_styles.GHMENU_STYLE_GENERAL_COMMAND:
                case ghmenu_styles.GHMENU_STYLE_ITEM_COMMAND:
                case ghmenu_styles.GHMENU_STYLE_SPELL_COMMAND:
                case ghmenu_styles.GHMENU_STYLE_SKILL_COMMAND:
                case ghmenu_styles.GHMENU_STYLE_CHARACTER:
                case ghmenu_styles.GHMENU_STYLE_VIEW_SPELL:
                case ghmenu_styles.GHMENU_STYLE_VIEW_SPELL_ALTERNATE:
                case ghmenu_styles.GHMENU_STYLE_CHAT:
                case ghmenu_styles.GHMENU_STYLE_SKILLS:
                case ghmenu_styles.GHMENU_STYLE_SKILLS_ALTERNATE:
                    MenuBackground.BackgroundStyle = BackgroundStyles.StretchedBitmap;
                    MenuBackground.BackgroundBitmap = BackgroundBitmaps.OldPaper;
                    MenuCanvas.RevertBlackAndWhite = true;
                    MenuCanvas.UseTextOutline = false;
                    MenuCanvas.HideMenuLetters = false;
                    MenuCanvas.MenuButtonStyle = false;
                    MenuCanvas.ClickOKOnSelection = menuinfo.SelectionHow == SelectionMode.Single;
                    MenuCanvas.MenuGlyphAtBottom = false;
                    MenuBackground.BorderStyle = MenuCanvas.ClickOKOnSelection ? BorderStyles.SimpleAlternative : BorderStyles.Simple;
                    MenuCanvas.AllowLongTap = false;
                    MenuCanvas.AllowHighlight = true;
                    break;
                case ghmenu_styles.GHMENU_STYLE_PICK_CATEGORY_LIST:
                    MenuBackground.BackgroundStyle = BackgroundStyles.StretchedBitmap;
                    MenuBackground.BackgroundBitmap = BackgroundBitmaps.OldPaper;
                    MenuBackground.BorderStyle = BorderStyles.Simple;
                    MenuCanvas.RevertBlackAndWhite = true;
                    MenuCanvas.UseTextOutline = false;
                    MenuCanvas.HideMenuLetters = false;
                    MenuCanvas.MenuButtonStyle = false;
                    MenuCanvas.ClickOKOnSelection = false;
                    MenuCanvas.MenuGlyphAtBottom = false;
                    MenuCanvas.AllowLongTap = false;
                    MenuCanvas.AllowHighlight = true;
                    break;
                case ghmenu_styles.GHMENU_STYLE_PICK_ITEM_LIST_AUTO_OK:
                    MenuBackground.BackgroundStyle = BackgroundStyles.StretchedBitmap;
                    MenuBackground.BackgroundBitmap = BackgroundBitmaps.OldPaper;
                    MenuBackground.BorderStyle = BorderStyles.SimpleAlternative;
                    MenuCanvas.RevertBlackAndWhite = true;
                    MenuCanvas.UseTextOutline = false;
                    MenuCanvas.HideMenuLetters = false;
                    MenuCanvas.MenuButtonStyle = false;
                    MenuCanvas.ClickOKOnSelection = true;
                    MenuCanvas.MenuGlyphAtBottom = false;
                    MenuCanvas.AllowLongTap = false;
                    MenuCanvas.AllowHighlight = true;
                    break;
                case ghmenu_styles.GHMENU_STYLE_PICK_ITEM_LIST:
                    MenuBackground.BackgroundStyle = BackgroundStyles.StretchedBitmap;
                    MenuBackground.BackgroundBitmap = BackgroundBitmaps.OldPaper;
                    MenuBackground.BorderStyle = BorderStyles.Simple;
                    MenuCanvas.RevertBlackAndWhite = true;
                    MenuCanvas.UseTextOutline = false;
                    MenuCanvas.HideMenuLetters = false;
                    MenuCanvas.MenuButtonStyle = false;
                    MenuCanvas.ClickOKOnSelection = false;
                    MenuCanvas.MenuGlyphAtBottom = false;
                    MenuCanvas.AllowLongTap = true;
                    MenuCanvas.AllowHighlight = true;
                    break;
                default:
                    MenuBackground.BackgroundStyle = BackgroundStyles.StretchedBitmap;
                    MenuBackground.BackgroundBitmap = BackgroundBitmaps.OldPaper;
                    MenuBackground.BorderStyle = BorderStyles.Simple;
                    MenuCanvas.RevertBlackAndWhite = true;
                    MenuCanvas.UseTextOutline = false;
                    MenuCanvas.HideMenuLetters = false;
                    MenuCanvas.MenuButtonStyle = false;
                    MenuCanvas.ClickOKOnSelection = false;
                    MenuCanvas.MenuGlyphAtBottom = false;
                    MenuCanvas.AllowLongTap = true;
                    MenuCanvas.AllowHighlight = false;
                    break;
            }

            if (MenuCanvas.ClickOKOnSelection && !MenuCanvas.MenuButtonStyle)
                MenuOKButton.Text = "Auto";
            else
                MenuOKButton.Text = "OK";

            /* Reset glyph */
            MenuWindowGlyphImage.Source = null;

            _menuGlyphImageSource.ReferenceGamePage = this;
            _menuGlyphImageSource.AutoSize = true;
            _menuGlyphImageSource.ObjData = ghwindow.ObjData;
            _menuGlyphImageSource.Glyph = ghwindow.Glyph;
            _menuGlyphImageSource.UseUpperSide = ghwindow.UseUpperSide;

            MenuWindowGlyphImage.ActiveGlyphImageSource = MenuGlyphImage;
            MenuWindowGlyphImage.VerticalOptions = MenuCanvas.MenuGlyphAtBottom ? LayoutOptions.End : LayoutOptions.Start;
            MenuWindowGlyphImage.IsVisible = IsMenuGlyphVisible;

            MenuHeaderLabel.Margin = UIUtils.GetHeaderMarginWithBorder(MenuBackground.BorderStyle, _currentPageWidth, _currentPageHeight);
            MenuCloseGrid.Margin = UIUtils.GetFooterMarginWithBorder(MenuBackground.BorderStyle, _currentPageWidth, _currentPageHeight);

            ObservableCollection<GHMenuItem> newmis = new ObservableCollection<GHMenuItem>();
            if (menuinfo != null)
            {
                foreach (GHMenuItem mi in menuinfo.MenuItems)
                {
                    newmis.Add(mi);
                }
            }

            //canvasView.MenuItems = newmis;
            lock (MenuCanvas.MenuItemLock)
            {
                MenuCanvas.MenuItems = newmis;
            }
            RefreshMenuRowCounts = true;

            lock (_menuDrawOnlyLock)
            {
                _menuDrawOnlyClear = false;
                _menuRefresh = true;
            }

            if (GHApp.IsiOS)
            {
                /* On iOS, fade in the menu. NOTE: this was originally a work-around for bad layout performance on iOS */
                Device.StartTimer(TimeSpan.FromSeconds(1.0 / 20), () =>
                {
                    if (MenuStack.AnimationIsRunning("MenuHideAnimation"))
                        MenuStack.AbortAnimation("MenuHideAnimation");
                    MenuStack.Opacity = 0.0;
                    MenuStack.IsVisible = true;
                    Animation menuAnimation = new Animation(v => MenuStack.Opacity = (double)v, 0.0, 1.0);
                    menuAnimation.Commit(MenuStack, "MenuShowAnimation", length: 256,
                        rate: 16, repeat: () => false);

                    MenuGrid.IsVisible = true;
                    MainGrid.IsVisible = false;
                    if (dohidetext)
                    {
                        TextGrid.IsVisible = false;
                    }
#if !GNH_MAUI
                    MenuStack.ForceLayout();
#endif
                    return false;
                });
            }
            else
            {
                MenuGrid.IsVisible = true;
                MainGrid.IsVisible = false;
                if (dohidetext)
                {
                    TextGrid.IsVisible = false;
                }
            }

            if (canvasView.AnimationIsRunning("GeneralAnimationCounter"))
                canvasView.AbortAnimation("GeneralAnimationCounter");
            _mapUpdateStopWatch.Stop();
            StartMenuCanvasAnimation();
            GHApp.DebugWriteProfilingStopwatchTimeAndStart("ShowMenuCanvas End");
        }

        private async void ShowOutRipPage(GHOutRipInfo outripinfo, GHWindow ghwindow)
        {
            var outRipPage = new OutRipPage(this, ghwindow, outripinfo);
            await App.Current.MainPage.Navigation.PushModalAsync(outRipPage);
        }

        private async Task<bool> BackButtonPressed(object sender, EventArgs e)
        {
            if (MoreCommandsGrid.IsVisible)
            {
                MoreCommandsGrid.IsVisible = false;
                MainGrid.IsVisible = true;
                if (CommandCanvas.AnimationIsRunning("GeneralAnimationCounter"))
                    CommandCanvas.AbortAnimation("GeneralAnimationCounter");
                lock (RefreshScreenLock)
                {
                    RefreshScreen = true;
                }
                StartMainCanvasAnimation();
            }
            else if (GetLineGrid.IsVisible)
            {
                GetLineCancelButton_Clicked(sender, e);
            }
            else if (PopupGrid.IsVisible)
            {
                PopupOkButton_Clicked(sender, e);
            }
            else if (YnGrid.IsVisible)
            {
                LabeledImageButton btn;
                if (FourthButton.IsVisible)
                    btn = FourthButton;
                else if (ThirdButton.IsVisible)
                    btn = ThirdButton;
                else if (SecondButton.IsVisible)
                    btn = SecondButton;
                else if (FirstButton.IsVisible)
                    btn = FirstButton;
                else
                    btn = ZeroButton;
                YnButton_Clicked(btn, e);
            }
            else if (TextGrid.IsVisible)
            {
                GenericButton_Clicked(sender, e, 27);
                TextGrid.IsVisible = false;
                MainGrid.IsVisible = true;
                if (TextCanvas.AnimationIsRunning("GeneralAnimationCounter"))
                    TextCanvas.AbortAnimation("GeneralAnimationCounter");
                lock (RefreshScreenLock)
                {
                    RefreshScreen = true;
                }
                StartMainCanvasAnimation();
            }
            else if (MenuGrid.IsVisible)
            {
                ConcurrentQueue<GHResponse> queue;
                if (GHGame.ResponseDictionary.TryGetValue(_currentGame, out queue))
                {
                    queue.Enqueue(new GHResponse(_currentGame, GHRequestType.ShowMenuPage, MenuCanvas.GHWindow, new List<GHMenuItem>(), true));
                }
                MenuGrid.IsVisible = false;
                MainGrid.IsVisible = true;
                if (MenuCanvas.AnimationIsRunning("GeneralAnimationCounter"))
                    MenuCanvas.AbortAnimation("GeneralAnimationCounter");
                lock (RefreshScreenLock)
                {
                    RefreshScreen = true;
                }
                StartMainCanvasAnimation();
            }
            else
            {
                var menu = new GameMenuPage(this);
                TallyRealTime();
                await App.Current.MainPage.Navigation.PushModalAsync(menu);
            }

            return false;
        }
        public async Task ShowGameMenu(object sender, EventArgs e)
        {
            var menu = new GameMenuPage(this);
            TallyRealTime();
            await App.Current.MainPage.Navigation.PushModalAsync(menu);
        }

        private void ContentPage_Disappearing(object sender, EventArgs e)
        {
            GHApp.BackButtonPressed -= BackButtonPressed;
            lock (RefreshScreenLock)
            {
                RefreshScreen = false;
            }

            Preferences.Set("MapFontSize", Math.Max(GHConstants.MinimumMapFontSize, MapFontSize));
            Preferences.Set("MapFontAlternateSize", Math.Max(GHConstants.MinimumMapFontSize, MapFontAlternateSize));
            Preferences.Set("MapFontMiniRelativeSize", Math.Min(GHConstants.MaximumMapMiniRelativeFontSize, Math.Max(GHConstants.MinimumMapMiniRelativeFontSize, MapFontMiniRelativeSize)));
            lock(_mapOffsetLock)
            {
                Preferences.Set("MapMiniOffsetX", _mapMiniOffsetX);
                Preferences.Set("MapMiniOffsetY", _mapMiniOffsetY);
            }
        }


        private SKMaskFilter _blur = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 3.4f);
        private SKMaskFilter _lookBlur = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 20.0f);

        public struct layer_draw_order_definition
        {
            public int layer;
            public int enlargement_position;
            public bool darken;
        }

        private readonly object _drawOrderLock = new object();
        private List<layer_draw_order_definition> _draw_order = new List<layer_draw_order_definition>();

        private void SetLayerDrawOrder()
        {
            lock (_drawOrderLock)
            {
                _draw_order.Clear();

                layer_draw_order_definition dodfloor = new layer_draw_order_definition();
                dodfloor.layer = (int)layer_types.LAYER_FLOOR;
                dodfloor.enlargement_position = -1;
                _draw_order.Add(dodfloor);

                layer_draw_order_definition dodcarpet = new layer_draw_order_definition();
                dodcarpet.layer = (int)layer_types.LAYER_CARPET;
                dodcarpet.enlargement_position = -1;
                _draw_order.Add(dodcarpet);

                for (int partition = 0; partition <= 1; partition++)
                {
                    int[] partition_start = { (int)layer_types.LAYER_CARPET + 1, (int)layer_types.LAYER_GENERAL_UI, (int)layer_types.MAX_LAYERS };
                    for (int enl_round = 0; enl_round <= 1; enl_round++)
                    {
                        for (int i = partition_start[partition]; i < partition_start[partition + 1]; i++)
                        {
                            for (int j = 0; j < 3; j++)
                            {
                                int[] pos0 = { 3, -1, 4 };
                                int[] pos1 = { 0, 1, 2 };
                                layer_draw_order_definition dod = new layer_draw_order_definition();
                                dod.layer = i;
                                dod.enlargement_position = enl_round == 0 ? pos0[j] : pos1[j];
                                if (i == partition_start[partition + 1] - 1 && dod.enlargement_position == -1)
                                    dod.darken = true;
                                _draw_order.Add(dod);
                            }
                        }
                    }
                }
                layer_draw_order_definition dodmax = new layer_draw_order_definition();
                dodmax.layer = (int)layer_types.MAX_LAYERS;
                dodmax.enlargement_position = -1;
                _draw_order.Add(dodmax);

                layer_draw_order_definition dodmax1 = new layer_draw_order_definition();
                dodmax1.layer = (int)layer_types.MAX_LAYERS + 1;
                dodmax1.enlargement_position = -1;
                _draw_order.Add(dodmax1);
            }
        }

        private GlyphImageSource _paintGlyphImageSource = new GlyphImageSource();
        private SKBitmap _paintBitmap = new SKBitmap(GHConstants.TileWidth, GHConstants.TileHeight);

        private void canvasView_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            PaintMainGamePage(sender, e);


            /* General stuff */
            SKImageInfo info = e.Info;
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;
            string str = "";
            float textWidth = 0;
            SKRect textBounds = new SKRect();
            float xText = 0;
            float yText = 0;
            float canvaswidth = canvasView.CanvasSize.Width;
            float canvasheight = canvasView.CanvasSize.Height;

            using (SKPaint textPaint = new SKPaint())
            {

                if (ShowMemoryUsage)
                {
                    long memusage = GC.GetTotalMemory(false);
                    str = "Memory: " + memusage / 1024 + " kB";
                    textPaint.Typeface = GHApp.LatoBold;
                    textPaint.TextSize = 26;
                    textWidth = textPaint.MeasureText(str, ref textBounds);
                    yText = -textPaint.FontMetrics.Ascent + 5 + (ShowFPS ? textPaint.FontSpacing : 0);
                    xText = canvaswidth - textWidth - 5;
                    textPaint.Color = SKColors.Black.WithAlpha(128);
                    float textmargin = (textPaint.FontSpacing - (textPaint.FontMetrics.Descent - textPaint.FontMetrics.Ascent)) / 2;
                    SKRect bkrect = new SKRect(xText - textmargin, yText + textPaint.FontMetrics.Ascent - textmargin, xText + textWidth + textmargin, yText + textPaint.FontMetrics.Ascent - textmargin + textPaint.FontSpacing);
                    canvas.DrawRect(bkrect, textPaint);
                    textPaint.Color = SKColors.Yellow;
                    canvas.DrawText(str, xText, yText, textPaint);
                }

                if (ShowFPS)
                {
                    lock (_fpslock)
                    {
                        str = "FPS: " + string.Format("{0:0.0}", _fps) + ", D:" + _counterValueDiff;
                    }
                    textPaint.Typeface = GHApp.LatoBold;
                    textPaint.TextSize = 26;
                    textWidth = textPaint.MeasureText(str, ref textBounds);
                    yText = -textPaint.FontMetrics.Ascent + 5.0f;
                    xText = canvaswidth - textWidth - 5.0f;
                    textPaint.Color = SKColors.Black.WithAlpha(128);
                    float textmargin = (textPaint.FontSpacing - (textPaint.FontMetrics.Descent - textPaint.FontMetrics.Ascent)) / 2;
                    SKRect bkrect = new SKRect(xText - textmargin, 5.0f - textmargin, xText + textWidth + textmargin, 5.0f - textmargin + textPaint.FontSpacing);
                    canvas.DrawRect(bkrect, textPaint);
                    textPaint.Color = SKColors.Yellow;
                    canvas.DrawText(str, xText, yText, textPaint);
                }

                if (ShowBattery)
                {
                    str = "Battery: " + string.Format("{0:0.0}", GHApp.BatteryChargeLevel) + "%" + ", " + string.Format("{0:0.0}", GHApp.BatteryConsumption);
                    textPaint.Typeface = GHApp.LatoBold;
                    textPaint.TextSize = 26;
                    textWidth = textPaint.MeasureText(str, ref textBounds);
                    yText = -textPaint.FontMetrics.Ascent + 5.0f + (ShowFPS ? textPaint.FontSpacing : 0) + (ShowMemoryUsage ? textPaint.FontSpacing : 0);
                    xText = canvaswidth - textWidth - 5.0f;
                    textPaint.Color = SKColors.Black.WithAlpha(128);
                    float textmargin = (textPaint.FontSpacing - (textPaint.FontMetrics.Descent - textPaint.FontMetrics.Ascent)) / 2;
                    SKRect bkrect = new SKRect(xText - textmargin, yText + textPaint.FontMetrics.Ascent - textmargin, xText + textWidth + textmargin, yText + textPaint.FontMetrics.Ascent - textmargin + textPaint.FontSpacing);
                    canvas.DrawRect(bkrect, textPaint);
                    textPaint.Color = SKColors.Yellow;
                    canvas.DrawText(str, xText, yText, textPaint);
                }
            }

            lock (_mainFPSCounterLock)
            {
                _mainFPSCounterValue++;
                if (_mainFPSCounterValue < 0)
                    _mainFPSCounterValue = 0;
            }

            /* Finally, flush */
            canvas.Flush();
        }

        private float[] _gridIntervals = { 2.0f, 2.0f };

        public float GetTextScale()
        {
            return (float)((lAbilitiesButton.Width <= 0 ? lAbilitiesButton.WidthRequest : lAbilitiesButton.Width) / 50.0f) / (float)GetCanvasScale();
        }

#if GNH_MAP_PROFILING && DEBUG
        private static Stopwatch _profilingStopwatchBmp = new Stopwatch();
        private static Stopwatch _profilingStopwatchText = new Stopwatch();
        private static Stopwatch _profilingStopwatchRect = new Stopwatch();

        private enum GHProfilingStyle
        {
            Bitmap,
            Text,
            Rect
        }

        private void StartProfiling(GHProfilingStyle style)
        {
            lock(GHApp.ProfilingStopwatchLock)
            {
                switch (style)
                {
                    case GHProfilingStyle.Bitmap:
                        _profilingStopwatchBmp.Start();
                        break;
                    case GHProfilingStyle.Text:
                        _profilingStopwatchText.Start();
                        break;
                    case GHProfilingStyle.Rect:
                        _profilingStopwatchRect.Start();
                        break;
                }
            }
        }

        private void StopProfiling(GHProfilingStyle style)
        {
            lock (GHApp.ProfilingStopwatchLock)
            {
                switch(style)
                {
                    case GHProfilingStyle.Bitmap:
                        _profilingStopwatchBmp.Stop();
                        break;
                    case GHProfilingStyle.Text:
                        _profilingStopwatchText.Stop();
                        break;
                    case GHProfilingStyle.Rect:
                        _profilingStopwatchRect.Stop();
                        break;
                }
            }
        }
#endif

        private void PaintMapUIElements(SKCanvas canvas, SKPaint textPaint, SKPaint paint, SKPathEffect pathEffect, int mapx, int mapy, float width, float height, float offsetX, float offsetY, float usedOffsetX, float usedOffsetY, float base_move_offset_x, float base_move_offset_y, float targetscale, long generalcountervalue, float usedFontSize, int monster_height, bool loc_is_you, bool canspotself)
        {
            float scaled_y_height_change = 0;
            float mapFontAscent = UsedMapFontAscent;
            float tx = 0, ty = 0;
            if (monster_height > 0)
                scaled_y_height_change = (float)-monster_height * height / (float)GHConstants.TileHeight;

            /* Grid */
            if (MapGrid)
            {
                tx = (offsetX + usedOffsetX + width * (float)mapx);
                ty = (offsetY + usedOffsetY + mapFontAscent + height * (float)mapy);

                textPaint.Style = SKPaintStyle.Stroke;
                textPaint.StrokeWidth = 2.0f;
                textPaint.Color = SKColors.Black;
                textPaint.PathEffect = pathEffect;
                SKPoint p0 = new SKPoint(tx, ty);
                SKPoint p1 = new SKPoint(tx, ty + height);
                canvas.DrawLine(p0, p1, textPaint);
                SKPoint p2 = new SKPoint(tx + width, ty + height);
                canvas.DrawLine(p1, p2, textPaint);
                textPaint.PathEffect = null;
                textPaint.Style = SKPaintStyle.Fill;
            }

            /* Chain */
            if(loc_is_you && _uBall != null && _uChain != null)
            {
                tx = (offsetX + usedOffsetX + base_move_offset_x + width * (float)mapx);
                ty = (offsetY + usedOffsetY + base_move_offset_y + mapFontAscent + height * (float)mapy); /* No scaled_y_height_change */
                DrawChain(canvas, paint, mapx, mapy, 0, true, width, height, ty, tx, 1.0f, targetscale);
            }

            /* Cursor */
            bool cannotseeself = (loc_is_you && !canspotself);
            if ((!loc_is_you || (loc_is_you && (cannotseeself || _show_cursor_on_u)))
                && (mapx == _mapCursorX && mapy == _mapCursorY)
                )
            {
                int cidx = (cannotseeself && _cursorType == game_cursor_types.CURSOR_STYLE_GENERIC_CURSOR ?
                    (int)game_cursor_types.CURSOR_STYLE_INVISIBLE :
                    (int)_cursorType);
                int cglyph = cidx + GHApp.CursorOff;
                int ctile = GHApp.Glyph2Tile[cglyph];
                int animation = GHApp.Tile2Animation[ctile];
                int autodraw = GHApp.Tile2Autodraw[ctile];
                int anim_frame_idx = 0, main_tile_idx = 0;
                sbyte mapAnimated = 0;
                int tile_animation_idx = _gnollHackService.GetTileAnimationIndexFromGlyph(cglyph);
                ctile = _gnollHackService.GetAnimatedTile(ctile, tile_animation_idx, (int)animation_play_types.ANIMATION_PLAY_TYPE_ALWAYS, generalcountervalue, out anim_frame_idx, out main_tile_idx, out mapAnimated, ref autodraw);
                int sheet_idx = GHApp.TileSheetIdx(ctile);
                int tile_x = GHApp.TileSheetX(ctile);
                int tile_y = GHApp.TileSheetY(ctile);

                tx = (offsetX + usedOffsetX + (loc_is_you ? base_move_offset_x : 0) + width * (float)mapx);
                ty = (offsetY + usedOffsetY + (loc_is_you ? base_move_offset_y : 0) + scaled_y_height_change + mapFontAscent + height * (float)mapy);
                SKRect targetrect = new SKRect(tx, ty, tx + width, ty + height);
                SKRect sourcerect = new SKRect(tile_x, tile_y, tile_x + GHConstants.TileWidth, tile_y + GHConstants.TileHeight);
#if GNH_MAP_PROFILING && DEBUG
                StartProfiling(GHProfilingStyle.Bitmap);
#endif
                canvas.DrawBitmap(TileMap[sheet_idx], sourcerect, targetrect);
#if GNH_MAP_PROFILING && DEBUG
                StopProfiling(GHProfilingStyle.Bitmap);
#endif
            }

            /* General tx, ty for all others, except cursors */
            tx = (offsetX + usedOffsetX + base_move_offset_x + width * (float)mapx);
            ty = (offsetY + usedOffsetY + base_move_offset_y + scaled_y_height_change + mapFontAscent + height * (float)mapy);

            if (HitPointBars)
            {
                /* Draw hit point bars */
                if (((_mapData[mapx, mapy].Layers.monster_flags & (ulong)(LayerMonsterFlags.LMFLAGS_YOU | LayerMonsterFlags.LMFLAGS_CANSPOTMON)) != 0 || (_mapData[mapx, mapy].Layers.layer_flags & (ulong)LayerFlags.LFLAGS_UXUY) != 0)
                && (_mapData[mapx, mapy].Layers.monster_flags & (ulong)(LayerMonsterFlags.LMFLAGS_WORM_TAIL)) == 0
                && _mapData[mapx, mapy].Layers.monster_maxhp > 0)
                {
                    int hp = _mapData[mapx, mapy].Layers.monster_hp;
                    int hpmax = _mapData[mapx, mapy].Layers.monster_maxhp;
                    float fraction = (hpmax == 0 ? 0 : Math.Max(0, Math.Min(1, (float)hp / (float)hpmax)));
                    float r_mult = fraction <= 0.25f ? fraction * 2.0f + 0.5f : fraction <= 0.5f ? 1.0f : (1.0f - fraction) * 2.0f;
                    float g_mult = fraction <= 0.25f ? 0 : fraction <= 0.5f ? (fraction - 0.25f) * 4.0f : 1.0f;
                    SKColor clr = new SKColor((byte)(255.0f * r_mult), (byte)(255.0f * g_mult), 0);
                    SKRect smaller_rect = new SKRect();
                    SKRect even_smaller_rect = new SKRect();
                    smaller_rect.Bottom = ty + height;
                    smaller_rect.Top = ty + height - Math.Max(1, (height) / 12);
                    smaller_rect.Left = tx;
                    smaller_rect.Right = tx + width;
                    even_smaller_rect.Bottom = smaller_rect.Bottom - 1 * targetscale;
                    even_smaller_rect.Top = smaller_rect.Top + 1 * targetscale;
                    even_smaller_rect.Left = smaller_rect.Left + 1 * targetscale;
                    even_smaller_rect.Right = even_smaller_rect.Left + (fraction * (smaller_rect.Right - 1 * targetscale - even_smaller_rect.Left));

                    paint.Style = SKPaintStyle.Fill;
                    paint.Color = SKColors.Black;
                    canvas.DrawRect(smaller_rect, paint);
                    paint.Color = clr;
                    canvas.DrawRect(even_smaller_rect, paint);
                }
            }

            /* Chain lock mark */
            if (loc_is_you && _uBall != null && _uChain != null)
            {
                int mglyph = (int)game_ui_tile_types.ITEM_AUTODRAW_GRAPHICS + GHApp.UITileOff;
                int mtile = GHApp.Glyph2Tile[mglyph];
                int m_sheet_idx = GHApp.TileSheetIdx(mtile);
                int source_x = GHApp.TileSheetX(mtile) + 0;
                int source_y = GHApp.TileSheetY(mtile) + 64;
                int source_width = 32;
                int source_height = 32;
                float target_x = tx + 2.0f * targetscale;
                float target_y = ty + height - (float)(source_height + 2) * targetscale;
                float target_width = (float)source_width * targetscale;
                float target_height = (float)source_height * targetscale;
                SKRect sourcerect = new SKRect(source_x, source_y, source_x + source_width, source_y + source_height);
                SKRect targetrect = new SKRect(target_x, target_y, target_x + target_width, target_y + target_height);
#if GNH_MAP_PROFILING && DEBUG
                StartProfiling(GHProfilingStyle.Bitmap);
#endif
                canvas.DrawBitmap(TileMap[m_sheet_idx], sourcerect, targetrect);
#if GNH_MAP_PROFILING && DEBUG
                StopProfiling(GHProfilingStyle.Bitmap);
#endif
            }

            bool draw_character = false;
            /* Player mark */
            if (PlayerMark && loc_is_you)
            {
                int cglyph = (int)game_ui_tile_types.U_TILE_MARK + GHApp.UITileOff;
                int ctile = GHApp.Glyph2Tile[cglyph];
                int animation = GHApp.Tile2Animation[ctile];
                int autodraw = GHApp.Tile2Autodraw[ctile];
                int anim_frame_idx = 0, main_tile_idx = 0;
                sbyte mapAnimated = 0;
                int tile_animation_idx = _gnollHackService.GetTileAnimationIndexFromGlyph(cglyph);
                ctile = _gnollHackService.GetAnimatedTile(ctile, tile_animation_idx, (int)animation_play_types.ANIMATION_PLAY_TYPE_ALWAYS, generalcountervalue, out anim_frame_idx, out main_tile_idx, out mapAnimated, ref autodraw);
                int sheet_idx = GHApp.TileSheetIdx(ctile);
                int tile_x = GHApp.TileSheetX(ctile);
                int tile_y = GHApp.TileSheetY(ctile);

                SKRect targetrect = new SKRect(tx, ty, tx + width, ty + height);
                SKRect sourcerect = new SKRect(tile_x, tile_y, tile_x + GHConstants.TileWidth, tile_y + GHConstants.TileHeight);
                canvas.DrawBitmap(TileMap[sheet_idx], sourcerect, targetrect);

                if (_mapData[mapx, mapy].Symbol != null && _mapData[mapx, mapy].Symbol != "")
                {
                    draw_character = true;
                }
            }

            /* Monster targeting mark */
            if (MonsterTargeting && !loc_is_you && (_mapData[mapx, mapy].Layers.monster_flags & (ulong)(LayerMonsterFlags.LMFLAGS_CANSPOTMON)) != 0)
            {
                int cglyph = (int)game_ui_tile_types.MAIN_TILE_MARK + GHApp.UITileOff;
                int ctile = GHApp.Glyph2Tile[cglyph];
                int animation = GHApp.Tile2Animation[ctile];
                int autodraw = GHApp.Tile2Autodraw[ctile];
                int anim_frame_idx = 0, main_tile_idx = 0;
                sbyte mapAnimated = 0;
                int tile_animation_idx = _gnollHackService.GetTileAnimationIndexFromGlyph(cglyph);
                ctile = _gnollHackService.GetAnimatedTile(ctile, tile_animation_idx, (int)animation_play_types.ANIMATION_PLAY_TYPE_ALWAYS, generalcountervalue, out anim_frame_idx, out main_tile_idx, out mapAnimated, ref autodraw);
                int sheet_idx = GHApp.TileSheetIdx(ctile);
                int tile_x = GHApp.TileSheetX(ctile);
                int tile_y = GHApp.TileSheetY(ctile);

                SKRect targetrect = new SKRect(tx, ty, tx + width, ty + height);
                SKRect sourcerect = new SKRect(tile_x, tile_y, tile_x + GHConstants.TileWidth, tile_y + GHConstants.TileHeight);
                canvas.DrawBitmap(TileMap[sheet_idx], sourcerect, targetrect);

                if (_mapData[mapx, mapy].Symbol != null && _mapData[mapx, mapy].Symbol != "")
                {
                    draw_character = true;
                }
            }

            if (draw_character)
            {
                textPaint.TextSize = usedFontSize / 4;
                textPaint.Typeface = GHApp.DejaVuSansMonoTypeface;
                textPaint.Color = _mapData[mapx, mapy].Color;
                textPaint.TextAlign = SKTextAlign.Center;
                float textheight = textPaint.FontSpacing; // FontMetrics.Descent - textPaint.FontMetrics.Ascent;
                float texttx = tx + width / 2;
                float textty = ty + height / 2 - textheight / 2 - textPaint.FontMetrics.Ascent - 1f / 96f * height;
                canvas.DrawText(_mapData[mapx, mapy].Symbol, texttx, textty, textPaint);
                textPaint.TextAlign = SKTextAlign.Left;
            }

            if (((_mapData[mapx, mapy].Layers.monster_flags & (ulong)(LayerMonsterFlags.LMFLAGS_YOU | LayerMonsterFlags.LMFLAGS_CANSPOTMON)) != 0 || (_mapData[mapx, mapy].Layers.layer_flags & (ulong)LayerFlags.LFLAGS_UXUY) != 0)
                && (_mapData[mapx, mapy].Layers.monster_flags & (ulong)(LayerMonsterFlags.LMFLAGS_WORM_TAIL)) == 0)
            {
                /* Draw condition and status marks */
                float x_scaling_factor = width / (float)(GHConstants.TileWidth);
                float y_scaling_factor = height / (float)(GHConstants.TileHeight);
                int max_fitted_rows = (GHConstants.TileHeight - 4) / (GHConstants.StatusMarkHeight + 2);
                int status_count = 0;

                ulong status_bits = _mapData[mapx, mapy].Layers.status_bits;
                if (status_bits != 0)
                {
                    int tiles_per_row = GHConstants.TileWidth / GHConstants.StatusMarkWidth;
                    int mglyph = (int)game_ui_tile_types.STATUS_MARKS + GHApp.UITileOff;
                    int mtile = GHApp.Glyph2Tile[mglyph];
                    int sheet_idx = GHApp.TileSheetIdx(mtile);
                    int tile_x = GHApp.TileSheetX(mtile);
                    int tile_y = GHApp.TileSheetY(mtile);
                    foreach (int status_mark in _statusmarkorder)
                    {
                        if (status_count >= max_fitted_rows)
                            break;

                        ulong statusbit = 1UL << status_mark;
                        if ((status_bits & statusbit) != 0)
                        {
                            int within_tile_x = status_mark % tiles_per_row;
                            int within_tile_y = status_mark / tiles_per_row;
                            int c_x = tile_x + within_tile_x * GHConstants.StatusMarkWidth;
                            int c_y = tile_y + within_tile_y * GHConstants.StatusMarkHeight;

                            SKRect source_rt = new SKRect();
                            source_rt.Left = c_x;
                            source_rt.Right = c_x + GHConstants.StatusMarkWidth;
                            source_rt.Top = c_y;
                            source_rt.Bottom = c_y + GHConstants.StatusMarkHeight;

                            /* Define draw location in target */
                            int unscaled_left = GHConstants.TileWidth - 2 - GHConstants.StatusMarkWidth;
                            int unscaled_right = unscaled_left + GHConstants.StatusMarkWidth;
                            int unscaled_top = 2 + (2 + GHConstants.StatusMarkWidth) * status_count;
                            int unscaled_bottom = unscaled_top + GHConstants.StatusMarkHeight;

                            SKRect target_rt = new SKRect();
                            target_rt.Left = tx + (int)(x_scaling_factor * (double)unscaled_left);
                            target_rt.Right = tx + (int)(x_scaling_factor * (double)unscaled_right);
                            target_rt.Top = ty + (int)(y_scaling_factor * (double)unscaled_top);
                            target_rt.Bottom = ty + (int)(y_scaling_factor * (double)unscaled_bottom);
#if GNH_MAP_PROFILING && DEBUG
                            StartProfiling(GHProfilingStyle.Bitmap);
#endif
                            canvas.DrawBitmap(TileMap[sheet_idx], source_rt, target_rt);
#if GNH_MAP_PROFILING && DEBUG
                            StopProfiling(GHProfilingStyle.Bitmap);
#endif
                            status_count++;
                        }
                    }
                }

                ulong condition_bits = _mapData[mapx, mapy].Layers.condition_bits;
                if (condition_bits != 0)
                {
                    int tiles_per_row = GHConstants.TileWidth / GHConstants.StatusMarkWidth;
                    int mglyph = (int)game_ui_tile_types.CONDITION_MARKS + GHApp.UITileOff;
                    int mtile = GHApp.Glyph2Tile[mglyph];
                    int sheet_idx = GHApp.TileSheetIdx(mtile);
                    int tile_x = GHApp.TileSheetX(mtile);
                    int tile_y = GHApp.TileSheetY(mtile);
                    for (int condition_mark = 0; condition_mark < (int)bl_conditions.NUM_BL_CONDITIONS; condition_mark++)
                    {
                        if (status_count >= max_fitted_rows)
                            break;

                        ulong conditionbit = 1UL << condition_mark;
                        if ((condition_bits & conditionbit) != 0)
                        {
                            int within_tile_x = condition_mark % tiles_per_row;
                            int within_tile_y = condition_mark / tiles_per_row;
                            int c_x = tile_x + within_tile_x * GHConstants.StatusMarkWidth;
                            int c_y = tile_y + within_tile_y * GHConstants.StatusMarkHeight;

                            SKRect source_rt = new SKRect();
                            source_rt.Left = c_x;
                            source_rt.Right = c_x + GHConstants.StatusMarkWidth;
                            source_rt.Top = c_y;
                            source_rt.Bottom = c_y + GHConstants.StatusMarkHeight;

                            /* Define draw location in target */
                            int unscaled_left = GHConstants.TileWidth - 2 - GHConstants.StatusMarkWidth;
                            int unscaled_right = unscaled_left + GHConstants.StatusMarkWidth;
                            int unscaled_top = 2 + (2 + GHConstants.StatusMarkWidth) * status_count;
                            int unscaled_bottom = unscaled_top + GHConstants.StatusMarkHeight;

                            SKRect target_rt = new SKRect();
                            target_rt.Left = tx + (int)(x_scaling_factor * (double)unscaled_left);
                            target_rt.Right = tx + (int)(x_scaling_factor * (double)unscaled_right);
                            target_rt.Top = ty + (int)(y_scaling_factor * (double)unscaled_top);
                            target_rt.Bottom = ty + (int)(y_scaling_factor * (double)unscaled_bottom);
#if GNH_MAP_PROFILING && DEBUG
                            StartProfiling(GHProfilingStyle.Bitmap);
#endif
                            canvas.DrawBitmap(TileMap[sheet_idx], source_rt, target_rt);
#if GNH_MAP_PROFILING && DEBUG
                            StopProfiling(GHProfilingStyle.Bitmap);
#endif
                            status_count++;
                        }
                    }
                }

                for (int buff_ulong = 0; buff_ulong < GHConstants.NUM_BUFF_BIT_ULONGS; buff_ulong++)
                {
                    if (status_count >= max_fitted_rows)
                        break;

                    ulong buff_bits = _mapData[mapx, mapy].Layers.buff_bits[buff_ulong];
                    int tiles_per_row = GHConstants.TileWidth / GHConstants.StatusMarkWidth;
                    if (buff_bits != 0)
                    {
                        for (int buff_idx = 0; buff_idx < 32; buff_idx++)
                        {
                            if (status_count >= max_fitted_rows)
                                break;

                            ulong buffbit = 1UL << buff_idx;
                            if ((buff_bits & buffbit) != 0)
                            {
                                int propidx = buff_ulong * 32 + buff_idx;
                                if (propidx > GHConstants.LAST_PROP)
                                    break;
                                int mglyph = (propidx - 1) / GHConstants.BUFFS_PER_TILE + GHApp.BuffTileOff;
                                int mtile = GHApp.Glyph2Tile[mglyph];
                                int sheet_idx = GHApp.TileSheetIdx(mtile);
                                int tile_x = GHApp.TileSheetX(mtile);
                                int tile_y = GHApp.TileSheetY(mtile);

                                int buff_mark = (propidx - 1) % GHConstants.BUFFS_PER_TILE;
                                int within_tile_x = buff_mark % tiles_per_row;
                                int within_tile_y = buff_mark / tiles_per_row;
                                int c_x = tile_x + within_tile_x * GHConstants.StatusMarkWidth;
                                int c_y = tile_y + within_tile_y * GHConstants.StatusMarkHeight;

                                SKRect source_rt = new SKRect();
                                source_rt.Left = c_x;
                                source_rt.Right = c_x + GHConstants.StatusMarkWidth;
                                source_rt.Top = c_y;
                                source_rt.Bottom = c_y + GHConstants.StatusMarkHeight;

                                /* Define draw location in target */
                                int unscaled_left = GHConstants.TileWidth - 2 - GHConstants.StatusMarkWidth;
                                int unscaled_right = unscaled_left + GHConstants.StatusMarkWidth;
                                int unscaled_top = 2 + (2 + GHConstants.StatusMarkWidth) * status_count;
                                int unscaled_bottom = unscaled_top + GHConstants.StatusMarkHeight;

                                SKRect target_rt = new SKRect();
                                target_rt.Left = tx + (int)(x_scaling_factor * (double)unscaled_left);
                                target_rt.Right = tx + (int)(x_scaling_factor * (double)unscaled_right);
                                target_rt.Top = ty + (int)(y_scaling_factor * (double)unscaled_top);
                                target_rt.Bottom = ty + (int)(y_scaling_factor * (double)unscaled_bottom);

#if GNH_MAP_PROFILING && DEBUG
                                StartProfiling(GHProfilingStyle.Bitmap);
#endif
                                canvas.DrawBitmap(TileMap[sheet_idx], source_rt, target_rt);
#if GNH_MAP_PROFILING && DEBUG
                                StopProfiling(GHProfilingStyle.Bitmap);
#endif
                                status_count++;
                            }
                        }
                    }
                }

            }

            /* Draw death and hit markers */
            if ((_mapData[mapx, mapy].Layers.monster_flags & (ulong)LayerMonsterFlags.LMFLAGS_KILLED) != 0)
            {
                int mglyph = (int)general_tile_types.GENERAL_TILE_DEATH + GHApp.GeneralTileOff;
                int mtile = GHApp.Glyph2Tile[mglyph];
                int sheet_idx = GHApp.TileSheetIdx(mtile);
                int tile_x = GHApp.TileSheetX(mtile);
                int tile_y = GHApp.TileSheetY(mtile);

                SKRect targetrect = new SKRect(tx, ty, tx + width, ty + height);
                SKRect sourcerect = new SKRect(tile_x, tile_y, tile_x + GHConstants.TileWidth, tile_y + GHConstants.TileHeight);
                canvas.DrawBitmap(TileMap[sheet_idx], sourcerect, targetrect);
            }
            else if ((_mapData[mapx, mapy].Layers.monster_flags & (ulong)LayerMonsterFlags.LMFLAGS_BEING_HIT) != 0)
            {
                short hit_text_num = _mapData[mapx, mapy].Layers.hit_tile;
                int mglyph = Math.Max(0, Math.Min((int)hit_tile_types.MAX_HIT_TILES - 1, (int)hit_text_num)) + GHApp.HitTileOff;
                int mtile = GHApp.Glyph2Tile[mglyph];
                int sheet_idx = GHApp.TileSheetIdx(mtile);
                int tile_x = GHApp.TileSheetX(mtile);
                int tile_y = GHApp.TileSheetY(mtile);

                SKRect targetrect = new SKRect(tx, ty, tx + width, ty + height);
                SKRect sourcerect = new SKRect(tile_x, tile_y, tile_x + GHConstants.TileWidth, tile_y + GHConstants.TileHeight);
                canvas.DrawBitmap(TileMap[sheet_idx], sourcerect, targetrect);
            }
        }


        private void PaintMapTile(SKCanvas canvas, bool delayedDraw, SKPaint textPaint, SKPaint paint, int layer_idx, int mapx, int mapy, int draw_map_x, int draw_map_y, int dx, int dy, int ntile, float width, float height, 
            float offsetX, float offsetY, float usedOffsetX, float usedOffsetY, float base_move_offset_x, float base_move_offset_y, float object_move_offset_x, float object_move_offset_y,
            float scaled_y_height_change, float pit_border,
            float targetscale, long generalcountervalue, float usedFontSize, int monster_height, 
            bool is_monster_like_layer, bool is_object_like_layer, bool obj_in_pit, int obj_height, bool is_missile_layer, int missile_height,
            bool loc_is_you, bool canspotself, bool tileflag_halfsize, bool tileflag_normalobjmissile, bool tileflag_fullsizeditem, bool tileflag_floortile, bool tileflag_height_is_clipping,
            bool hflip_glyph, bool vflip_glyph,
            ObjectDataItem otmp_round, int autodraw, bool drawwallends, bool breatheanimations, long generalcounterdiff, float canvaswidth, float canvasheight, int enlargement,
            ref short[,] draw_shadow) //, ref float minDrawX, ref float maxDrawX, ref float minDrawY, ref float maxDrawY,
            //ref float enlMinDrawX, ref float enlMaxDrawX, ref float enlMinDrawY, ref float enlMaxDrawY)
        {
            if (!GHUtils.isok(draw_map_x, draw_map_y))
                return;

            float tx = 0, ty = 0;
            if(draw_shadow != null)
            {
                if (dx != 0 || dy != 0)
                {
                    draw_shadow[draw_map_x, draw_map_y] |= 1;
                }
            }

            int sheet_idx = GHApp.TileSheetIdx(ntile);
            int tile_x = GHApp.TileSheetX(ntile);
            int tile_y = GHApp.TileSheetY(ntile);

            SKRect sourcerect;
            float scaled_tile_width = width;
            float scaled_tile_height = tileflag_halfsize || (tileflag_normalobjmissile && !tileflag_fullsizeditem) ? height / 2 : height;
            float scaled_x_padding = 0;
            float scaled_y_padding = 0;
            int source_y_added = 0;
            int source_height_deducted = 0;
            int source_height = tileflag_halfsize ? GHConstants.TileHeight / 2 : GHConstants.TileHeight;
            float mapFontAscent = UsedMapFontAscent;

            float scale = 1.0f;
            if (tileflag_halfsize && !tileflag_normalobjmissile)
            {
                if ((layer_idx == (int)layer_types.LAYER_OBJECT || layer_idx == (int)layer_types.LAYER_COVER_OBJECT))
                {
                    if (obj_in_pit)
                        scale *= GHConstants.OBJECT_PIT_SCALING_FACTOR;
                }

                if (monster_height < 0 && is_monster_like_layer)
                {
                    scale *= Math.Min(1.0f, Math.Max(0.1f, 1.0f - (1.0f - (float)GHConstants.OBJECT_PIT_SCALING_FACTOR) * (float)monster_height / (float)GHConstants.SPECIAL_HEIGHT_IN_PIT));
                }

                if (tileflag_floortile || tileflag_height_is_clipping)
                {
                    if (layer_idx == (int)layer_types.LAYER_OBJECT || layer_idx == (int)layer_types.LAYER_OBJECT)
                    {
                        source_y_added = tileflag_floortile ? 0 : GHConstants.TileHeight / 2;
                        if (obj_height > 0 && obj_height < 48)
                        {
                            source_y_added += (GHConstants.TileHeight / 2 - obj_height) / 2;
                            source_height_deducted = GHConstants.TileHeight / 2 - obj_height;
                            source_height = GHConstants.TileHeight / 2 - source_height_deducted;
                            scaled_tile_width = scale * width;
                            scaled_x_padding = (width - scaled_tile_width) / 2;
                            scaled_tile_height = scale * (float)source_height * height / (float)GHConstants.TileHeight;
                            scaled_y_padding = Math.Max(0, scale * (float)source_height_deducted * height / (float)GHConstants.TileHeight - pit_border);
                        }
                    }
                    sourcerect = new SKRect(tile_x, tile_y + source_y_added, tile_x + GHConstants.TileWidth, tile_y + source_y_added + source_height);
                }
                else
                {
                    if ((layer_idx == (int)layer_types.LAYER_OBJECT || layer_idx == (int)layer_types.LAYER_COVER_OBJECT))
                    {
                        if (obj_height > 0 && obj_height < 48)
                            scale *= ((float)obj_height) / 48.0f;
                    }
                    scaled_tile_width = scale * width;
                    scaled_tile_height = scale * height / 2;
                    scaled_x_padding = (width - scaled_tile_width) / 2;
                    scaled_y_padding = Math.Max(0, height / 2 - scaled_tile_height - pit_border);
                    sourcerect = new SKRect(tile_x, tile_y + GHConstants.TileHeight / 2, tile_x + GHConstants.TileWidth, tile_y + GHConstants.TileHeight);
                }
            }
            else
            {
                if (tileflag_normalobjmissile && !tileflag_fullsizeditem)
                {
                    if (tileflag_floortile)
                    {
                        sourcerect = new SKRect(tile_x, tile_y, tile_x + GHConstants.TileWidth, tile_y + GHConstants.TileHeight / 2);
                    }
                    else if (tileflag_height_is_clipping)
                    {
                        sourcerect = new SKRect(tile_x, tile_y + GHConstants.TileHeight / 2, tile_x + GHConstants.TileWidth, tile_y + GHConstants.TileHeight);
                    }
                    else
                    {
                        if (missile_height > 0 && missile_height < 48)
                        {
                            scale = ((float)missile_height) / 48.0f;
                        }
                        scaled_tile_width = scale * width;
                        scaled_tile_height = scale * height / 2;
                        scaled_x_padding = (width - scaled_tile_width) / 2;
                        scaled_y_padding = (height / 2 - scaled_tile_height) / 2;

                        sourcerect = new SKRect(tile_x, tile_y + GHConstants.TileHeight / 2, tile_x + GHConstants.TileWidth, tile_y + GHConstants.TileHeight);
                    }
                }
                else
                {
                    if (monster_height < 0 && dy == 0 && is_monster_like_layer)
                    {
                        sourcerect = new SKRect(tile_x, tile_y, tile_x + GHConstants.TileWidth, tile_y + GHConstants.TileHeight + monster_height);
                        source_height_deducted = -monster_height;
                        source_height = GHConstants.TileHeight - source_height_deducted;
                        scaled_tile_height = (float)source_height * height / (float)GHConstants.TileHeight;
                    }
                    else
                    {
                        sourcerect = new SKRect(tile_x, tile_y, tile_x + GHConstants.TileWidth, tile_y + GHConstants.TileHeight);
                        if (is_missile_layer && !tileflag_floortile && !tileflag_height_is_clipping)
                        {
                            if (missile_height > 0 && missile_height < 48)
                            {
                                scale = ((float)missile_height) / 48.0f;
                            }
                            scaled_tile_width = scale * width;
                            scaled_tile_height = scale * height;
                            scaled_x_padding = (width - scaled_tile_width) / 2;
                            scaled_y_padding = (height - scaled_tile_height) / 2;
                        }
                    }
                }
            }

            float move_offset_x = 0, move_offset_y = 0;
            float opaqueness = 1.0f;
            if (is_monster_like_layer)
            {
                move_offset_x = base_move_offset_x;
                move_offset_y = base_move_offset_y;
                if (layer_idx == (int)layer_types.MAX_LAYERS)
                {
                    if((draw_shadow[mapx, mapy] & 2) != 0)
                        opaqueness = (_mapData[mapx, mapy].Layers.monster_flags & (ulong)LayerMonsterFlags.LMFLAGS_RADIAL_TRANSPARENCY) != 0 ? 1.0f : (_mapData[mapx, mapy].Layers.monster_flags & (ulong)LayerMonsterFlags.LMFLAGS_GLASS_TRANSPARENCY) != 0 ? 0.65f : 0.5f;
                    else
                        opaqueness = 0.5f;
                }
                else if ((_mapData[mapx, mapy].Layers.monster_flags & (ulong)(LayerMonsterFlags.LMFLAGS_INVISIBLE_TRANSPARENT | LayerMonsterFlags.LMFLAGS_SEMI_TRANSPARENT | LayerMonsterFlags.LMFLAGS_RADIAL_TRANSPARENCY)) != 0)
                {
                    draw_shadow[mapx, mapy] |= 2;
                    return; /* Draw only the transparent shadow in the max_layers shadow layer; otherwise, if drawn twice, the result will be nontransparent */
                }

                /* Death transparency */
                if ((_mapData[mapx, mapy].Layers.monster_flags & (ulong)LayerMonsterFlags.LMFLAGS_KILLED) != 0 
                    && (_mapData[mapx, mapy].Layers.monster_flags & (ulong)LayerMonsterFlags.LMFLAGS_STONED) == 0
                    && (_mapData[mapx, mapy].Layers.monster_flags & (ulong)LayerMonsterFlags.LMFLAGS_FADES_UPON_DEATH) != 0)
                {
                    opaqueness = opaqueness * ((float)(20L - Math.Min(20L, generalcounterdiff))) / 20;
                }

                /* Hovering effect */
                if ((_mapData[mapx, mapy].Layers.monster_flags & (ulong)LayerMonsterFlags.LMFLAGS_KILLED) == 0)
                {
                    if((_mapData[mapx, mapy].Layers.monster_flags & (ulong)LayerMonsterFlags.LMFLAGS_LEVITATING) != 0)
                    {
                        long animationframe = generalcountervalue % _hoverAnimation.Length;
                        move_offset_x += _hoverAnimation[animationframe].X * scale * targetscale;
                        move_offset_y += (-2.5f + _hoverAnimation[animationframe].Y) * scale * targetscale;
                    }
                    else if ((_mapData[mapx, mapy].Layers.monster_flags & (ulong)LayerMonsterFlags.LMFLAGS_FLYING) != 0)
                    {
                        long animationframe = generalcountervalue % _flyingAnimation.Length;
                        move_offset_x += _flyingAnimation[animationframe].X * scale * targetscale;
                        move_offset_y += (-5f + _flyingAnimation[animationframe].Y) * scale * targetscale;
                    }
                    else if ((_mapData[mapx, mapy].Layers.monster_flags & (ulong)LayerMonsterFlags.LMFLAGS_SWIM_ANIMATION) != 0)
                    {
                        long animationframe = generalcountervalue % _swimAnimation.Length;
                        move_offset_x += _swimAnimation[animationframe].X * scale * targetscale;
                        move_offset_y += _swimAnimation[animationframe].Y * scale * targetscale;
                    }
                    else if ((_mapData[mapx, mapy].Layers.monster_flags & (ulong)LayerMonsterFlags.LMFLAGS_SHARK_ANIMATION) != 0)
                    {
                        long animationframe = generalcountervalue % _sharkAnimation.Length;
                        float target_y_change = _sharkAnimation[animationframe].Y * scale * targetscale;
                        move_offset_x += _sharkAnimation[animationframe].X * scale * targetscale;
                        move_offset_y += target_y_change;
                        if(mapy == draw_map_y)
                        {
                            scaled_tile_height -= target_y_change;
                            sourcerect = new SKRect(sourcerect.Left, sourcerect.Top, sourcerect.Right, sourcerect.Bottom - _sharkAnimation[animationframe].Y);
                        }
                    }
                }
            }
            else if (is_object_like_layer && otmp_round != null)
            {
                move_offset_x = object_move_offset_x;
                move_offset_y = object_move_offset_y;
            }
            else if (layer_idx == (int)layer_types.LAYER_COVER_TRAP)
            {
                opaqueness = 0.5f;
            }
            else if (layer_idx == (int)layer_types.LAYER_BACKGROUND_EFFECT)
            {
                if((_mapData[mapx, mapy].Layers.layer_flags & (ulong)LayerFlags.LFLAGS_E_BKG_FADE_IN) != 0)
                {
                    opaqueness = opaqueness * ((float)(Math.Min(20L, generalcounterdiff))) / 20;
                }
                else if ((_mapData[mapx, mapy].Layers.layer_flags & (ulong)LayerFlags.LFLAGS_E_BKG_FADE_OUT) != 0)
                {
                    opaqueness = opaqueness * ((float)(20L - Math.Min(20L, generalcounterdiff))) / 20;
                }
            }
            else if (layer_idx == (int)layer_types.LAYER_GENERAL_EFFECT)
            {
                if ((_mapData[mapx, mapy].Layers.layer_flags & (ulong)LayerFlags.LFLAGS_E_GEN_FADE_OUT) != 0)
                {
                    opaqueness = opaqueness * ((float)(20L - Math.Min(20L, generalcounterdiff))) / 20;
                }
            }

            float dscalex = 1.0f;
            float dscaley = 1.0f;
            float correction_x = 0f;
            float correction_y = 0f;
            if (is_monster_like_layer)
            {
                if ((_mapData[mapx, mapy].Layers.monster_flags & (ulong)LayerMonsterFlags.LMFLAGS_KILLED) != 0
                    && (_mapData[mapx, mapy].Layers.monster_flags & (ulong)LayerMonsterFlags.LMFLAGS_STONED) == 0
                    && (_mapData[mapx, mapy].Layers.monster_flags & (ulong)LayerMonsterFlags.LMFLAGS_FADES_UPON_DEATH) == 0)
                {
                    if (enlargement > 0)
                    {
                        int enlarea = GHApp._enlargementDefs[enlargement].width_in_tiles * GHApp._enlargementDefs[enlargement].height_in_tiles;
                        int maxenltiles = Math.Max(GHApp._enlargementDefs[enlargement].width_in_tiles, GHApp._enlargementDefs[enlargement].height_in_tiles);
                        long param = Math.Max(1L, enlarea > 0 ? 180L / enlarea : 60L);
                        long param2 = (param * (maxenltiles - 1)) / maxenltiles;
                        dscalex = dscaley = ((float)(param - Math.Min(param2 - 1, generalcounterdiff))) / (float)param;
                    }
                    else
                        dscalex = dscaley = ((float)(90 - Math.Min(44L, generalcounterdiff))) / 90;
                }

                if ((_mapData[mapx, mapy].Layers.monster_flags & (ulong)LayerMonsterFlags.LMFLAGS_KILLED) == 0)                   
                {
                    if((_mapData[mapx, mapy].Layers.monster_flags & (ulong)LayerMonsterFlags.LMFLAGS_BLOBBY_ANIMATION) != 0)
                    {
                        long animationframe = generalcountervalue % _blobAnimation.Length;
                        dscalex *= _blobAnimation[animationframe].X;
                        dscaley *= _blobAnimation[animationframe].Y;
                    }
                    else if(breatheanimations)
                    {
                        if ((_mapData[mapx, mapy].Layers.monster_flags & (ulong)LayerMonsterFlags.LMFLAGS_HUMAN_BREATHE_ANIMATION) != 0)
                        {
                            long animationframe = generalcountervalue % _humanBreatheAnimation.Length;
                            dscalex *= _humanBreatheAnimation[animationframe].X;
                            dscaley *= _humanBreatheAnimation[animationframe].Y;
                        }
                        else if ((_mapData[mapx, mapy].Layers.monster_flags & (ulong)LayerMonsterFlags.LMFLAGS_ANIMAL_BREATHE_ANIMATION) != 0)
                        {
                            long animationframe = generalcountervalue % _animalBreatheAnimation.Length;
                            dscalex *= _animalBreatheAnimation[animationframe].X;
                            dscaley *= _animalBreatheAnimation[animationframe].Y;
                        }
                    }
                }

                correction_x = width * (mapx - draw_map_x) + width * dscalex * (draw_map_x - mapx) + width * (1.0f - dscalex) / 2;
                correction_y = height * (mapy - draw_map_y) + height * dscaley * (draw_map_y - mapy) + height * (1.0f - dscaley);
            }

            tx = (offsetX + usedOffsetX + move_offset_x + width * (float)draw_map_x + correction_x);
            ty = (offsetY + usedOffsetY + move_offset_y + scaled_y_height_change + mapFontAscent + height * (float)draw_map_y + correction_y);
            float splitY = -(move_offset_y + scaled_y_height_change + correction_y);
            float dx2 = dx * (hflip_glyph ? -1 : 1) * width * dscalex;
            float dy2 = dy * (vflip_glyph ? -1 : 1) * height * dscaley;
            using (new SKAutoCanvasRestore(canvas, true))
            {
                float tr_x = tx + (hflip_glyph ? width * dscalex : 0);
                float tr_y = ty + (vflip_glyph ? height * dscaley : 0);
                float sc_x = hflip_glyph ? -1 : 1;
                float sc_y = vflip_glyph ? -1 : 1;
                TranslateAndScaleCanvas(canvas, tr_x, tr_y, sc_x, sc_y, is_monster_like_layer, _mapData[mapx, mapy].Layers,
                    dx2, dy2, dscalex, dscaley, width, height, generalcounterdiff);
                //SKAutoCanvasRestore enlRestore = null;
                //if (enlCanvas != canvas)
                //{
                //    enlRestore = new SKAutoCanvasRestore(enlCanvas, true);
                //    TranslateAndScaleCanvas(enlCanvas, tr_x, tr_y, sc_x, sc_y, is_monster_like_layer, _mapData[mapx, mapy].Layers,
                //        dx2, dy2, dscalex, dscaley, width, height, generalcounterdiff);
                //}
                SKRect targetrect;
                if (tileflag_halfsize && !tileflag_normalobjmissile)
                {
                    targetrect = new SKRect(scaled_x_padding, height / 2 + scaled_y_padding, scaled_x_padding + scaled_tile_width, height / 2 + scaled_y_padding + scaled_tile_height);
                }
                else
                {
                    if (tileflag_normalobjmissile && !tileflag_fullsizeditem)
                        targetrect = new SKRect(scaled_x_padding, height / 4 + scaled_y_padding, scaled_x_padding + scaled_tile_width, height / 4 + scaled_y_padding + scaled_tile_height);
                    else
                        targetrect = new SKRect(scaled_x_padding, scaled_y_padding, scaled_x_padding + scaled_tile_width, scaled_y_padding + scaled_tile_height);
                }

                //SKRect baseUpdateRect = new SKRect();
                //SKRect enlUpdateRect = new SKRect();
                paint.Color = paint.Color.WithAlpha((byte)(0xFF * opaqueness));
                if (is_monster_like_layer && (_mapData[mapx, mapy].Layers.monster_flags & (ulong)LayerMonsterFlags.LMFLAGS_RADIAL_TRANSPARENCY) != 0)
                {
                    DrawTileWithRadialTransparency(canvas, delayedDraw, TileMap[sheet_idx], sourcerect, targetrect, _mapData[mapx, mapy].Layers, splitY, opaqueness, paint);
                        //, ref baseUpdateRect, ref enlUpdateRect);
                }
                else
                {
#if GNH_MAP_PROFILING && DEBUG
                    StartProfiling(GHProfilingStyle.Bitmap);
#endif
                    DrawSplitBitmap(canvas, delayedDraw, splitY, TileMap[sheet_idx], sourcerect, targetrect, paint); //, ref baseUpdateRect, ref enlUpdateRect);
                    //canvas.DrawBitmap(TileMap[sheet_idx], sourcerect, targetrect, paint);
#if GNH_MAP_PROFILING && DEBUG
                    StopProfiling(GHProfilingStyle.Bitmap);
#endif
                }

                //SKRect mBaseUpdateRect = canvas.TotalMatrix.MapRect(baseUpdateRect);
                //SKRect mEnlUpdateRect = enlCanvas.TotalMatrix.MapRect(enlUpdateRect);
                //UpdateDrawBounds(mBaseUpdateRect, ref minDrawX, ref maxDrawX, ref minDrawY, ref maxDrawY);
                //if(canvas != enlCanvas)
                //    UpdateDrawBounds(mEnlUpdateRect, ref enlMinDrawX, ref enlMaxDrawX, ref enlMinDrawY, ref enlMaxDrawY);

                //if (enlRestore != null)
                //    enlRestore.Dispose();
            }

            DrawAutoDraw(autodraw, canvas, paint, otmp_round,
                layer_idx, mapx, mapy,
                tileflag_halfsize, tileflag_normalobjmissile, tileflag_fullsizeditem,
                tx, ty, width, height,
                scale, targetscale, scaled_x_padding, scaled_y_padding, scaled_tile_height,
                false, drawwallends);
        }

        public void UpdateDrawBounds(SKRect mUpdateRect, ref float minDrawX, ref float maxDrawX, ref float minDrawY, ref float maxDrawY)
        {
            if (mUpdateRect.Left < minDrawX)
                minDrawX = mUpdateRect.Left;
            if (mUpdateRect.Right > maxDrawX)
                maxDrawX = mUpdateRect.Right;
            if (mUpdateRect.Top < minDrawY)
                minDrawY = mUpdateRect.Top;
            if (mUpdateRect.Bottom > maxDrawY)
                maxDrawY = mUpdateRect.Bottom;
        }

        struct SavedRect
        {
            public SKBitmap Bitmap;
            public SKRect Rect;
            public SavedRect(SKBitmap bitmap, SKRect rect)
            {
                Bitmap = bitmap;
                Rect = rect;
            }
        }
        Dictionary<SavedRect, SKBitmap> _savedRects = new Dictionary<SavedRect, SKBitmap>();
        public void DrawTileWithRadialTransparency(SKCanvas canvas, bool delayedDraw, SKBitmap tileSheet, SKRect sourcerect, SKRect targetrect, LayerInfo layers, float destSplitY, float opaqueness, SKPaint paint)
            //, ref SKRect baseUpdateRect, ref SKRect enlUpdateRect)
        {
            bool cache = false;
            if (sourcerect.Left % GHConstants.TileWidth == 0 && sourcerect.Top % GHConstants.TileHeight == 0 
                && sourcerect.Width == GHConstants.TileWidth && sourcerect.Height == GHConstants.TileHeight)
                cache = true;

            if(cache)
            {
                SavedRect sr = new SavedRect(tileSheet, sourcerect);
                SKBitmap bmp = null;
                if (_savedRects.TryGetValue(sr, out bmp) && bmp != null)
                {
                    SKRect bmpsourcerect = new SKRect(0, 0, (float)bmp.Width, (float)bmp.Height);
                    DrawSplitBitmap(canvas, delayedDraw, destSplitY, bmp, bmpsourcerect, targetrect, paint);
                    return;
                }
            }

            IntPtr tempptraddr = _tempBitmap.GetPixels();
            IntPtr tileptraddr = tileSheet.GetPixels();
            double mid_x = (double)GHConstants.TileWidth / 2.0 - 0.5;
            double mid_y = (double)GHConstants.TileHeight / 2.0 - 0.5;
            double r = 0, semi_transparency = 0;
            byte radial_opacity = 0x00;
            int bytesperpixel = tileSheet.BytesPerPixel;
            int copywidth = Math.Min((int)sourcerect.Width, _tempBitmap.Width);
            int copyheight = Math.Min((int)sourcerect.Height, _tempBitmap.Height);
            int tilemapwidth = tileSheet.Width;
            unsafe
            {
                byte* tempptr = (byte*)tempptraddr.ToPointer();
                byte* tileptr = (byte*)tileptraddr.ToPointer();
                tileptr += ((int)sourcerect.Left + (int)sourcerect.Top * tilemapwidth) * bytesperpixel;

                for (int row = 0; row < copyheight; row++)
                {
                    for (int col = 0; col < copywidth; col++)
                    {
                        r = Math.Sqrt(Math.Pow((double)col - mid_x, 2.0) + Math.Pow((double)row - mid_y, 2.0));
                        semi_transparency = r * 0.0375; //r_constant
                        if (semi_transparency > 0.98)
                            semi_transparency = 0.98;

                        *tempptr++ = *tileptr;       // red
                        tileptr++;
                        *tempptr++ = *tileptr;       // green
                        tileptr++;
                        *tempptr++ = *tileptr;       // blue
                        tileptr++;
                        radial_opacity = (byte)((double)0xFF * (1.0 - semi_transparency) * ((double)(*tileptr) / (double)0xFF));
                        *tempptr++ = radial_opacity; // alpha
                        tileptr++;
                    }
                    tileptr += (tilemapwidth - copywidth) * bytesperpixel;
                }
            }
            SKRect tempsourcerect = new SKRect(0, 0, copywidth, copyheight);

            if ((layers.monster_flags & (ulong)LayerMonsterFlags.LMFLAGS_INVISIBLE_TRANSPARENT) != 0)
                paint.Color = paint.Color.WithAlpha((byte)(0xFF * opaqueness));

            if (cache)
            {
                SavedRect sr = new SavedRect(tileSheet, sourcerect);
                if (!_savedRects.ContainsKey(sr))
                {
                    try
                    {
                        if (_savedRects.Count >= GHConstants.MaxBitmapCacheSize)
                        {
                            foreach (SKBitmap bmp in _savedRects.Values)
                                bmp.Dispose();
                            _savedRects.Clear(); /* Clear the whole dictonary for the sake of ease; should almost never happen normally anyway */
                        }

                        SKBitmap newbmp = new SKBitmap(GHConstants.TileWidth, GHConstants.TileHeight, SKColorType.Rgba8888, SKAlphaType.Unpremul);
                        _tempBitmap.CopyTo(newbmp);
                        newbmp.SetImmutable();
                        _savedRects.Add(sr, newbmp);
                        DrawSplitBitmap(canvas, delayedDraw, destSplitY, newbmp, tempsourcerect, targetrect, paint); //, ref baseUpdateRect, ref enlUpdateRect);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
            }
            else
            {
#if GNH_MAP_PROFILING && DEBUG
            StartProfiling(GHProfilingStyle.Bitmap);
#endif
                DrawSplitBitmap(canvas, delayedDraw, destSplitY, _tempBitmap, tempsourcerect, targetrect, paint); //, ref baseUpdateRect, ref enlUpdateRect);
                                                                                                                  //canvas.DrawBitmap(_tempBitmap, tempsourcerect, targetrect, paint);
#if GNH_MAP_PROFILING && DEBUG
            StopProfiling(GHProfilingStyle.Bitmap);
#endif
            }
        }

        private List<GHDrawCommand> _drawCommandList = new List<GHDrawCommand>();

        public void DrawSplitBitmap(SKCanvas canvas, bool delayedDraw, float destSplitY, SKBitmap bitmap, SKRect source, SKRect dest, SKPaint paint) //, ref SKRect baseUpdateRect, ref SKRect enlUpdateRect)
        {
            if (destSplitY <= dest.Top || delayedDraw)
            {
                if(delayedDraw)
                    _drawCommandList.Add(new GHDrawCommand(canvas.TotalMatrix, source, dest, bitmap, paint.Color));
                else
                    canvas.DrawBitmap(bitmap, source, dest, paint);
                //baseUpdateRect = dest;
                //enlUpdateRect = new SKRect();
            }
            else if (destSplitY >= dest.Bottom)
            {
                _drawCommandList.Add(new GHDrawCommand(canvas.TotalMatrix, source, dest, bitmap, paint.Color));
                //enlCanvas.DrawBitmap(bitmap, source, dest, paint);
                //baseUpdateRect = new SKRect();
                //enlUpdateRect = dest;
            }
            else
            {
                float destHeight = dest.Bottom - dest.Top;
                if (destHeight <= 0)
                    return;
                SKRect enlDest = new SKRect(dest.Left, dest.Top, dest.Right, destSplitY);
                SKRect baseDest = new SKRect(dest.Left, destSplitY, dest.Right, dest.Bottom);
                float topDestScale = (destSplitY - dest.Top) / destHeight;
                float sourceSplitY = source.Top + (source.Bottom - source.Top) * topDestScale;
                SKRect enlSource = new SKRect(source.Left, source.Top, source.Right, sourceSplitY);
                SKRect baseSource = new SKRect(source.Left, sourceSplitY, source.Right, source.Bottom);
                canvas.DrawBitmap(bitmap, baseSource, baseDest, paint);
                //enlCanvas.DrawBitmap(bitmap, enlSource, enlDest, paint);
                _drawCommandList.Add(new GHDrawCommand(canvas.TotalMatrix, enlSource, enlDest, bitmap, paint.Color));
                //baseUpdateRect = baseDest;
                //enlUpdateRect = enlDest;
            }
        }

        public void TranslateAndScaleCanvas(SKCanvas canvas, float tr_x, float tr_y, float sc_x, float sc_y, bool is_monster_like_layer, LayerInfo layers,
                    float dx2, float dy2, float dscalex, float dscaley, float width, float height, long generalcounterdiff)
        {
            canvas.Translate(tr_x, tr_y);
            canvas.Scale(sc_x, sc_y, 0, 0);
            if (is_monster_like_layer)
            {
                if ((layers.monster_flags & (ulong)LayerMonsterFlags.LMFLAGS_KILLED) != 0
                    && (layers.monster_flags & (ulong)LayerMonsterFlags.LMFLAGS_STONED) == 0
                    && (layers.monster_flags & (ulong)LayerMonsterFlags.LMFLAGS_FADES_UPON_DEATH) == 0)
                {
                    /* Death rotation */
                    float rotateheight = 0.5f * height * dscaley; //((float)(enlargement > 0 ? GHApp._enlargementDefs[enlargement].height_in_tiles - 1 : 0) * -0.5f + 0.75f)
                    canvas.Translate(-dx2 + 0.5f * width * dscalex, -dy2 + rotateheight);
                    canvas.RotateDegrees(generalcounterdiff * 15);
                    canvas.Translate(dx2 - 0.5f * width * dscalex, dy2 - rotateheight);
                }
                if (dscalex != 1.0f || dscaley != 1.0f)
                    canvas.Scale(dscalex, dscaley, 0, 0);
            }
        }
    
        private int GetSubLayerCount(int mapx, int mapy, int layer_idx, out bool is_source_dir)
        {
            int sub_layer_cnt = 1;
            switch (layer_idx)
            {
                case (int)layer_types.LAYER_OBJECT:
                    if ((_mapData[mapx, mapy].Layers.layer_flags & (ulong)LayerFlags.LFLAGS_SHOWING_MEMORY) != 0)
                        sub_layer_cnt = _objectData[mapx, mapy].MemoryObjectList == null ? 0 : Math.Min(GHConstants.MaxObjectsDrawn, _objectData[mapx, mapy].MemoryObjectList.Count);
                    else if ((_mapData[mapx, mapy].Layers.layer_flags & (ulong)LayerFlags.LFLAGS_CAN_SEE) != 0)
                        sub_layer_cnt = _objectData[mapx, mapy].FloorObjectList == null ? 0 : Math.Min(GHConstants.MaxObjectsDrawn, _objectData[mapx, mapy].FloorObjectList.Count);
                    else
                        sub_layer_cnt = 1; /* As a backup, show layer glyph (probably often NoGlyph) */
                    is_source_dir = false;
                    break;
                case (int)layer_types.LAYER_COVER_OBJECT:
                    if ((_mapData[mapx, mapy].Layers.layer_flags & (ulong)LayerFlags.LFLAGS_SHOWING_MEMORY) != 0)
                        sub_layer_cnt = _objectData[mapx, mapy].CoverMemoryObjectList == null ? 0 : Math.Min(GHConstants.MaxObjectsDrawn, _objectData[mapx, mapy].CoverMemoryObjectList.Count);
                    else if ((_mapData[mapx, mapy].Layers.layer_flags & (ulong)LayerFlags.LFLAGS_CAN_SEE) != 0)
                        sub_layer_cnt = _objectData[mapx, mapy].CoverFloorObjectList == null ? 0 : Math.Min(GHConstants.MaxObjectsDrawn, _objectData[mapx, mapy].CoverFloorObjectList.Count);
                    else
                        sub_layer_cnt = 1; /* As a backup, show layer glyph (probably often NoGlyph) */
                    is_source_dir = false;
                    break;
                case (int)layer_types.LAYER_MONSTER:
                    sub_layer_cnt = GHConstants.NUM_WORM_SOURCE_DIRS + 1;
                    is_source_dir = true;
                    break;
                case (int)layer_types.LAYER_CHAIN:
                    sub_layer_cnt = GHConstants.NUM_CHAIN_SOURCE_DIRS + 1;
                    is_source_dir = true;
                    break;
                case (int)layer_types.LAYER_ZAP:
                    sub_layer_cnt = GHConstants.NUM_ZAP_SOURCE_DIRS + 1;
                    is_source_dir = true;
                    break;
                default:
                    is_source_dir = false;
                    break;
            }
            return sub_layer_cnt;
        }

        int GetSourceDirIndex(int layer_idx, int source_dir_main_idx)
        {
            int source_dir_idx = source_dir_main_idx;
            switch (layer_idx)
            {
                case (int)layer_types.LAYER_CHAIN:
                case (int)layer_types.LAYER_MONSTER:
                    source_dir_idx = source_dir_main_idx * 2;
                    break;
            }
            return source_dir_idx;
        }

        int GetUsedEnlargementIndex(int enlarg_idx)
        {
            int used_enl_idx = -1;
            switch(enlarg_idx)
            {
                default:
                case -1:
                    break;
                case 0:
                    used_enl_idx = 4;
                    break;
                case 1:
                    used_enl_idx = 3;
                    break;
                case 2:
                    used_enl_idx = 2;
                    break;
                case 3:
                    used_enl_idx = 1;
                    break;
                case 4:
                    used_enl_idx = 0;
                    break;
            }
            return used_enl_idx;
        }

        private bool GetEnlargementTile(int enlargement, int enlarg_idx, bool hflip_glyph, bool vflip_glyph, int main_tile_idx, int anim_frame_idx, ref int ntile, ref int autodraw, ref int dx, ref int dy)
        {
            if (enlargement == 0 && enlarg_idx >= 0)
                return false;

            int position_index = -1;
            int orig_position_index = -1;
            if (enlargement > 0)
            {
                orig_position_index = -1;
                /* Set position_index */
                if (enlarg_idx == -1)
                {
                    if (vflip_glyph)
                        position_index = 1;
                    else
                        position_index = -1;
                }
                else if (enlarg_idx == 0)
                {
                    orig_position_index = 4;
                    if (vflip_glyph)
                        position_index = hflip_glyph ? 0 : 2;
                    else
                        position_index = hflip_glyph ? 3 : 4;
                }
                else if (enlarg_idx == 1)
                {
                    orig_position_index = 3;
                    if (vflip_glyph)
                        position_index = hflip_glyph ? 2 : 0;
                    else
                        position_index = hflip_glyph ? 4 : 3;
                }
                else if (enlarg_idx == 2)
                {
                    orig_position_index = 2;
                    if (vflip_glyph)
                        position_index = hflip_glyph ? 3 : 4;
                    else
                        position_index = hflip_glyph ? 0 : 2;
                }
                else if (enlarg_idx == 3)
                {
                    orig_position_index = 1;
                    if (vflip_glyph)
                        position_index = -1;
                    else
                        position_index = 1;
                }
                else if (enlarg_idx == 4)
                {
                    orig_position_index = 0;
                    if (vflip_glyph)
                        position_index = hflip_glyph ? 4 : 3;
                    else
                        position_index = hflip_glyph ? 2 : 0;
                }

            }

            if (enlargement > 0 && orig_position_index >= 0)
            {
                int enl_tile_idx = GHApp._enlargementDefs[enlargement].position2tile[orig_position_index];
                if (enl_tile_idx >= 0)
                {
                    int addedindex = 0;
                    if (GHApp._enlargementDefs[enlargement].number_of_animation_frames > 0)
                    {
                        if (main_tile_idx == -1
                            && anim_frame_idx >= 0
                            && anim_frame_idx < GHApp._enlargementDefs[enlargement].number_of_animation_frames
                            )
                        {
                            addedindex = anim_frame_idx * GHApp._enlargementDefs[enlargement].number_of_enlargement_tiles;
                        }
                        else if (main_tile_idx == 0
                            && anim_frame_idx > 0
                            && anim_frame_idx <= GHApp._enlargementDefs[enlargement].number_of_animation_frames)
                        {
                            addedindex = (anim_frame_idx - 1) * GHApp._enlargementDefs[enlargement].number_of_enlargement_tiles;
                        }
                        else if (main_tile_idx == GHApp._enlargementDefs[enlargement].number_of_animation_frames
                            && anim_frame_idx >= 0
                            && anim_frame_idx < GHApp._enlargementDefs[enlargement].number_of_animation_frames
                            )
                        {
                            addedindex = anim_frame_idx * GHApp._enlargementDefs[enlargement].number_of_enlargement_tiles;
                        }
                    }
                    int enl_glyph = enl_tile_idx + addedindex + GHApp.EnlargementOffsets[enlargement] + GHApp.EnlargementOff;
                    ntile = GHApp.Glyph2Tile[enl_glyph]; /* replace */
                    autodraw = GHApp.Tile2Autodraw[ntile];
                }
                else
                    return false;
            }

            switch (position_index)
            {
                case 0:
                    dx = -1;
                    dy = -1;
                    break;
                case 1:
                    dx = 0;
                    dy = -1;
                    break;
                case 2:
                    dx = 1;
                    dy = -1;
                    break;
                case 3:
                    dx = -1;
                    dy = 0;
                    break;
                case 4:
                    dx = 1;
                    dy = 0;
                    break;
            }
            return true;
        }

        private bool GetEnlargementTileB(int enlargement, int enlarg_idx, bool hflip_glyph, bool vflip_glyph, int main_tile_idx, int anim_frame_idx, ref int ntile, ref int autodraw, ref int dx, ref int dy)
        {
            if (enlargement == 0 && enlarg_idx >= 0)
                return false;

            int position_index = -1;
            if (enlargement > 0)
            {
                /* Set position_index */
                switch(enlarg_idx)
                {
                    default:
                    case -1:
                        if (vflip_glyph)
                            position_index = 1;
                        else
                            position_index = -1;
                        break;
                    case 0:
                        if (vflip_glyph)
                            position_index = hflip_glyph ? 4 : 3;
                        else
                            position_index = hflip_glyph ? 2 : 0;
                        break;
                    case 1:
                        if (vflip_glyph)
                            position_index = -1;
                        else
                            position_index = 1;
                        break;
                    case 2:
                        if (vflip_glyph)
                            position_index = hflip_glyph ? 3 : 4;
                        else
                            position_index = hflip_glyph ? 0 : 2;

                        break;
                    case 3:
                        if (vflip_glyph)
                            position_index = hflip_glyph ? 2 : 0;
                        else
                            position_index = hflip_glyph ? 4 : 3;
                        break;
                    case 4:
                        if (vflip_glyph)
                            position_index = hflip_glyph ? 0 : 2;
                        else
                            position_index = hflip_glyph ? 3 : 4;
                        break;
                }
            }

            if (enlargement > 0 && position_index >= 0)
            {
                int enl_tile_idx = GHApp._enlargementDefs[enlargement].position2tile[position_index];
                if (enl_tile_idx >= 0)
                {
                    int addedindex = 0;
                    if (GHApp._enlargementDefs[enlargement].number_of_animation_frames > 0)
                    {
                        if (main_tile_idx == -1
                            && anim_frame_idx >= 0
                            && anim_frame_idx < GHApp._enlargementDefs[enlargement].number_of_animation_frames
                            )
                        {
                            addedindex = anim_frame_idx * GHApp._enlargementDefs[enlargement].number_of_enlargement_tiles;
                        }
                        else if (main_tile_idx == 0
                            && anim_frame_idx > 0
                            && anim_frame_idx <= GHApp._enlargementDefs[enlargement].number_of_animation_frames)
                        {
                            addedindex = (anim_frame_idx - 1) * GHApp._enlargementDefs[enlargement].number_of_enlargement_tiles;
                        }
                        else if (main_tile_idx == GHApp._enlargementDefs[enlargement].number_of_animation_frames
                            && anim_frame_idx >= 0
                            && anim_frame_idx < GHApp._enlargementDefs[enlargement].number_of_animation_frames
                            )
                        {
                            addedindex = anim_frame_idx * GHApp._enlargementDefs[enlargement].number_of_enlargement_tiles;
                        }
                    }
                    int enl_glyph = enl_tile_idx + addedindex + GHApp.EnlargementOffsets[enlargement] + GHApp.EnlargementOff;
                    ntile = GHApp.Glyph2Tile[enl_glyph]; /* replace */
                    autodraw = GHApp.Tile2Autodraw[ntile];
                }
                else
                    return false;
            }

            switch (enlarg_idx)
            {
                case 0:
                    dx = -1;
                    dy = -1;
                    break;
                case 1:
                    dx = 0;
                    dy = -1;
                    break;
                case 2:
                    dx = 1;
                    dy = -1;
                    break;
                case 3:
                    dx = -1;
                    dy = 0;
                    break;
                case 4:
                    dx = 1;
                    dy = 0;
                    break;
            }
            return true;
        }

        private bool GetLayerGlyph(int mapx, int mapy, int layer_idx, int sub_layer_idx, int source_dir_idx,
            ref int signed_glyph, ref int adj_x, ref int adj_y, ref bool manual_hflip, ref bool manual_vflip,
            ref ObjectDataItem otmp_round, ref short obj_height, ref sbyte object_origin_x, ref sbyte object_origin_y, ref bool foundthisturn)
        {
            if (layer_idx == (int)layer_types.LAYER_OBJECT)
            {
                if ((_mapData[mapx, mapy].Layers.layer_flags & (ulong)LayerFlags.LFLAGS_SHOWING_MEMORY) != 0)
                {
                    otmp_round = _objectData[mapx, mapy].MemoryObjectList[sub_layer_idx];
                    signed_glyph = _objectData[mapx, mapy].MemoryObjectList[sub_layer_idx].ObjData.gui_glyph;
                    obj_height = _objectData[mapx, mapy].MemoryObjectList[sub_layer_idx].TileHeight;
                    object_origin_x = _objectData[mapx, mapy].MemoryObjectList[sub_layer_idx].ObjData.ox0;
                    object_origin_y = _objectData[mapx, mapy].MemoryObjectList[sub_layer_idx].ObjData.oy0;
                    foundthisturn = _objectData[mapx, mapy].MemoryObjectList[sub_layer_idx].FoundThisTurn;
                }
                else if ((_mapData[mapx, mapy].Layers.layer_flags & (ulong)LayerFlags.LFLAGS_CAN_SEE) != 0)
                {
                    otmp_round = _objectData[mapx, mapy].FloorObjectList[sub_layer_idx];
                    signed_glyph = _objectData[mapx, mapy].FloorObjectList[sub_layer_idx].ObjData.gui_glyph;
                    obj_height = _objectData[mapx, mapy].FloorObjectList[sub_layer_idx].TileHeight;
                    object_origin_x = _objectData[mapx, mapy].FloorObjectList[sub_layer_idx].ObjData.ox0;
                    object_origin_y = _objectData[mapx, mapy].FloorObjectList[sub_layer_idx].ObjData.oy0;
                    foundthisturn = _objectData[mapx, mapy].FloorObjectList[sub_layer_idx].FoundThisTurn;
                }
                else
                {
                    signed_glyph = _mapData[mapx, mapy].Layers.layer_gui_glyphs == null ? GHApp.NoGlyph : _mapData[mapx, mapy].Layers.layer_gui_glyphs[layer_idx];
                }
            }
            else if (layer_idx == (int)layer_types.LAYER_COVER_OBJECT)
            {
                if ((_mapData[mapx, mapy].Layers.layer_flags & (ulong)LayerFlags.LFLAGS_SHOWING_MEMORY) != 0)
                {
                    otmp_round = _objectData[mapx, mapy].CoverMemoryObjectList[sub_layer_idx];
                    signed_glyph = _objectData[mapx, mapy].CoverMemoryObjectList[sub_layer_idx].ObjData.gui_glyph;
                    obj_height = _objectData[mapx, mapy].CoverMemoryObjectList[sub_layer_idx].TileHeight;
                    object_origin_x = _objectData[mapx, mapy].CoverMemoryObjectList[sub_layer_idx].ObjData.ox0;
                    object_origin_y = _objectData[mapx, mapy].CoverMemoryObjectList[sub_layer_idx].ObjData.oy0;
                }
                else if ((_mapData[mapx, mapy].Layers.layer_flags & (ulong)LayerFlags.LFLAGS_CAN_SEE) != 0)
                {
                    otmp_round = _objectData[mapx, mapy].CoverFloorObjectList[sub_layer_idx];
                    signed_glyph = _objectData[mapx, mapy].CoverFloorObjectList[sub_layer_idx].ObjData.gui_glyph;
                    obj_height = _objectData[mapx, mapy].CoverFloorObjectList[sub_layer_idx].TileHeight;
                    object_origin_x = _objectData[mapx, mapy].CoverFloorObjectList[sub_layer_idx].ObjData.ox0;
                    object_origin_y = _objectData[mapx, mapy].CoverFloorObjectList[sub_layer_idx].ObjData.oy0;
                }
                else
                {
                    signed_glyph = _mapData[mapx, mapy].Layers.layer_gui_glyphs == null ? GHApp.NoGlyph : _mapData[mapx, mapy].Layers.layer_gui_glyphs[layer_idx];
                }
            }
            else if (source_dir_idx > 0)
            {
                switch ((source_dir_idx - 1) % GHConstants.NUM_ZAP_SOURCE_BASE_DIRS + 1)
                {
                    case 1:
                        adj_x = mapx + 1;
                        adj_y = mapy + 1;
                        break;
                    case 2:
                        adj_x = mapx;
                        adj_y = mapy + 1;
                        break;
                    case 3:
                        adj_x = mapx - 1;
                        adj_y = mapy + 1;
                        break;
                    case 4:
                        adj_x = mapx - 1;
                        adj_y = mapy;
                        break;
                    case 5:
                        adj_x = mapx - 1;
                        adj_y = mapy - 1;
                        break;
                    case 6:
                        adj_x = mapx;
                        adj_y = mapy - 1;
                        break;
                    case 7:
                        adj_x = mapx + 1;
                        adj_y = mapy - 1;
                        break;
                    case 8:
                        adj_x = mapx + 1;
                        adj_y = mapy;
                        break;
                    default:
                        break;

                }

                switch (layer_idx)
                {
                    case (int)layer_types.LAYER_ZAP:
                        {
                            int adjacent_zap_glyph = _mapData[mapx, mapy].Layers.layer_gui_glyphs[(int)layer_types.LAYER_ZAP];
                            ulong adjacent_layer_flags = (ulong)_mapData[mapx, mapy].Layers.layer_flags;

                            if (adjacent_zap_glyph == GHApp.NoGlyph) // || !glyph_is_zap(adjacent_zap_glyph))
                                signed_glyph = GHApp.NoGlyph;
                            else
                                signed_glyph = _gnollHackService.ZapGlyphToCornerGlyph(adjacent_zap_glyph, adjacent_layer_flags, source_dir_idx);
                            break;
                        }
                    case (int)layer_types.LAYER_MONSTER:
                        {
                            /* Worm */
                            uint worm_id_stored = _mapData[mapx, mapy].Layers.m_id;
                            if (worm_id_stored == 0)
                                return false;

                            bool is_long_worm_with_tail = (_mapData[mapx, mapy].Layers.monster_flags & (ulong)LayerMonsterFlags.LMFLAGS_LONG_WORM_WITH_TAIL) != 0;
                            bool is_long_worm_tail = (_mapData[mapx, mapy].Layers.monster_flags & (ulong)LayerMonsterFlags.LMFLAGS_LONG_WORM_TAIL) != 0;
                            bool is_adj_worm_tail = (_mapData[mapx, mapy].Layers.monster_flags & (ulong)LayerMonsterFlags.LMFLAGS_WORM_TAIL) != 0;
                            bool is_adj_worm_seen = (_mapData[mapx, mapy].Layers.monster_flags & (ulong)LayerMonsterFlags.LMFLAGS_WORM_SEEN) != 0;
                            bool worm = !is_adj_worm_tail ? false : is_adj_worm_seen ? (worm_id_stored > 0 ? true : false) : true;
                            signed_glyph = GHApp.NoGlyph;

                            if (worm && (_mapData[mapx, mapy].Layers.monster_flags & (ulong)LayerMonsterFlags.LMFLAGS_WORM_SEEN) != 0
                                && ((
                                _mapData[mapx, mapy].Layers.layer_flags & (ulong)LayerFlags.LFLAGS_CAN_SEE) != 0
                                || is_adj_worm_seen || (_mapData[mapx, mapy].Layers.monster_flags & (ulong)LayerMonsterFlags.LMFLAGS_WORM_SEEN) != 0))
                            {
                                if (is_long_worm_with_tail && !is_adj_worm_tail)
                                {
                                    signed_glyph = GHApp.NoGlyph;
                                }
                                else if (is_long_worm_tail || (is_long_worm_with_tail && is_adj_worm_tail))
                                {
                                    int signed_main_glyph = _mapData[mapx, mapy].Layers.layer_gui_glyphs[layer_idx];
                                    int main_glyph = Math.Abs(signed_main_glyph);
                                    //int tile_animation_index = _gnollHackService.GetTileAnimationIndexFromGlyph(main_glyph);
                                    int main_tile = GHApp.Glyph2Tile[main_glyph];
                                    int wormautodraw = GHApp.Tile2Autodraw[main_tile];
                                    int base_source_glyph = GHApp.NoGlyph;
                                    if (wormautodraw > 0)
                                    {
                                        base_source_glyph = GHApp._autodraws[wormautodraw].source_glyph4;
                                    }

                                    int wdir = _mapData[mapx, mapy].Layers.wsegdir;
                                    int tilenum = -1;
                                    if (wdir % 2 == 1)
                                    {
                                        switch (source_dir_idx)
                                        {
                                            case 2:
                                                if (wdir == 7)
                                                {
                                                    //tilenum = GENERAL_TILE_WORM_IS_UP_GOING_DOWN_LEFT;
                                                    tilenum = 1; //GENERAL_TILE_WORM_IS_DOWN_GOING_UP_LEFT;
                                                    manual_vflip = true;
                                                }
                                                else if (wdir == 5)
                                                {
                                                    //tilenum = GENERAL_TILE_WORM_IS_UP_GOING_DOWN_RIGHT;
                                                    tilenum = 3; // GENERAL_TILE_WORM_IS_UP_GOING_DOWN_RIGHT;
                                                    manual_hflip = false;
                                                    manual_vflip = false;
                                                }
                                                break;
                                            case 4:
                                                if (wdir == 1)
                                                {
                                                    //tilenum = GENERAL_TILE_WORM_IS_RIGHT_GOING_UP_LEFT;
                                                    tilenum = 0;  //GENERAL_TILE_WORM_IS_RIGHT_GOING_UP_LEFT;
                                                    manual_hflip = false;
                                                    manual_vflip = false;
                                                }
                                                else if (wdir == 7)
                                                {
                                                    //tilenum = GENERAL_TILE_WORM_IS_RIGHT_GOING_DOWN_LEFT;
                                                    tilenum = 0; // GENERAL_TILE_WORM_IS_RIGHT_GOING_UP_LEFT;
                                                    manual_hflip = false;
                                                    manual_vflip = true;
                                                }
                                                break;
                                            case 6:
                                                if (wdir == 1)
                                                {
                                                    //tilenum = GENERAL_TILE_WORM_IS_DOWN_GOING_UP_LEFT;
                                                    tilenum = 1; // GENERAL_TILE_WORM_IS_DOWN_GOING_UP_LEFT;
                                                    manual_hflip = false;
                                                    manual_vflip = false;
                                                }
                                                else if (wdir == 3)
                                                {
                                                    //tilenum = GENERAL_TILE_WORM_IS_DOWN_GOING_UP_RIGHT;
                                                    tilenum = 3; // GENERAL_TILE_WORM_IS_UP_GOING_DOWN_RIGHT;
                                                    manual_hflip = false;
                                                    manual_vflip = true;
                                                }
                                                break;
                                            case 8:
                                                if (wdir == 3)
                                                {
                                                    //tilenum = GENERAL_TILE_WORM_IS_LEFT_GOING_UP_RIGHT;
                                                    tilenum = 2; // GENERAL_TILE_WORM_IS_LEFT_GOING_DOWN_RIGHT;
                                                    manual_hflip = false;
                                                    manual_vflip = true;
                                                }
                                                else if (wdir == 5)
                                                {
                                                    //tilenum = GENERAL_TILE_WORM_IS_LEFT_GOING_DOWN_RIGHT;
                                                    tilenum = 2; // GENERAL_TILE_WORM_IS_LEFT_GOING_DOWN_RIGHT;
                                                    manual_hflip = false;
                                                    manual_vflip = false;
                                                }
                                                break;
                                            default:
                                                break;
                                        }
                                        if (tilenum > -1)
                                            signed_glyph = tilenum + base_source_glyph;
                                    }
                                }
                            }
                            break;
                        }
                    case (int)layer_types.LAYER_CHAIN:
                        {
                            /* Chain */
                            if ((_mapData[mapx, mapy].Layers.layer_flags & (ulong)LayerFlags.LFLAGS_O_CHAIN) != 0)
                            {
                                signed_glyph = (source_dir_idx / 2 - 1) + (int)general_tile_types.GENERAL_TILE_CHAIN_IS_UP + GHApp.GeneralTileOff;
                            }
                            else
                                signed_glyph = GHApp.NoGlyph;
                            break;
                        }
                }
            }
            else
            {
                int used_layer_idx = layer_idx;
                if (layer_idx == (int)layer_types.MAX_LAYERS)
                    used_layer_idx = (int)layer_types.LAYER_MONSTER;
                signed_glyph = _mapData[mapx, mapy].Layers.layer_gui_glyphs == null ? GHApp.NoGlyph : _mapData[mapx, mapy].Layers.layer_gui_glyphs[used_layer_idx];
            }

            if (signed_glyph == GHApp.NoGlyph)
                return false;

            return true;
        }

        float GetScaledYHeightChange(int layer_idx, int sub_layer_idx, int sub_layer_cnt, float height, int monster_height, int feature_doodad_height, float targetscale, bool is_monster_like_layer, bool tileflag_halfsize, ObjectDataItem otmp_round)
        {
            float scaled_y_height_change = 0;
            if ((!tileflag_halfsize || monster_height > 0) && is_monster_like_layer)
            {
                scaled_y_height_change = (float)-monster_height * height / (float)GHConstants.TileHeight;
                if (monster_height < 0)
                    scaled_y_height_change -= GHConstants.PIT_BOTTOM_BORDER * targetscale;
            }
            else if (tileflag_halfsize && (layer_idx == (int)layer_types.LAYER_OBJECT || layer_idx == (int)layer_types.LAYER_COVER_OBJECT))
            {
                if(otmp_round != null && (otmp_round.OtypData.is_uball != 0 || otmp_round.OtypData.is_uchain != 0))
                    scaled_y_height_change = 0;
                else
                    scaled_y_height_change = (float)(-(sub_layer_cnt - 1 - sub_layer_idx) * GHConstants.OBJECT_PILE_HEIGHT_DIFFERENCE - GHConstants.OBJECT_PILE_START_HEIGHT) * targetscale;
            }
            else if (feature_doodad_height != 0 && layer_idx == (int)layer_types.LAYER_FEATURE_DOODAD)
            {
                scaled_y_height_change = (float)-feature_doodad_height * height / (float)GHConstants.TileHeight;
            }
            return scaled_y_height_change;
        }

        void GetFlips(int signed_glyph, bool manual_hflip, bool manual_vflip, ref bool hflip_glyph, ref bool vflip_glyph)
        {
            int glyph = Math.Abs(signed_glyph);
            /* Tile flips */
            bool tileflag_hflip = (GHApp.GlyphTileFlags[glyph] & (byte)glyph_tile_flags.GLYPH_TILE_FLAG_FLIP_HORIZONTALLY) != 0;
            bool tileflag_vflip = (GHApp.GlyphTileFlags[glyph] & (byte)glyph_tile_flags.GLYPH_TILE_FLAG_FLIP_VERTICALLY) != 0;

            /* Base flips */
            bool hflip = (signed_glyph < 0);

            /* Final glyph flips */
            if ((hflip != tileflag_hflip) != manual_hflip) /* XOR */
                hflip_glyph = true;
            else
                hflip_glyph = false;

            if (tileflag_vflip != manual_vflip) /* XOR */
                vflip_glyph = true;
            else
                vflip_glyph = false;
        }

        void CheckShowingDetection(bool showing_detection, ref short obj_height, ref bool tileflag_floortile, ref bool tileflag_height_is_clipping)
        {
            if (showing_detection)
            {
                obj_height = 0;
                tileflag_floortile = false;
                tileflag_height_is_clipping = false;
            }
        }

        void GetBaseMoveOffsets(int mapx, int mapy, sbyte monster_origin_x, sbyte monster_origin_y, float width, float height, long maincounterdiff, long moveIntervals, ref float base_move_offset_x, ref float base_move_offset_y)
        {
            int movediffx = (int)monster_origin_x - mapx;
            int movediffy = (int)monster_origin_y - mapy;
            if (GHUtils.isok(monster_origin_x, monster_origin_y)
                && (movediffx != 0 || movediffy != 0)
                && maincounterdiff >= 0 && maincounterdiff < moveIntervals)
            {
                base_move_offset_x = width * (float)movediffx * (float)(moveIntervals - maincounterdiff) / (float)moveIntervals;
                base_move_offset_y = height * (float)movediffy * (float)(moveIntervals - maincounterdiff) / (float)moveIntervals;
            }

        }

        //private float[] _foundAnimationNormal = { 10f, 20f, 30f, 40f, 45f, 50f, 55f, 57.5f, 60f, 57.5f, 55f, 50f, 45f, 40f, 30f, 20f, 10f, 0f };
        //private float[] _foundAnimationHigh = { 10f, 20f, 30f, 40f, 50f, 60f, 70f, 80f, 88f, 96f, 102f, 108f, 114f, 118f, 120f, 118f, 114f, 108f, 102f, 96f, 88f, 80f, 70f, 60f, 50f, 40f, 30f, 20f, 10f, 0f };
        private float[] _foundAnimationFactor = { 0.10f, 0.20f, 0.30f, 0.40f, 0.50f, 0.60f, 0.70f, 0.80f, 0.88f, 0.94f, 0.98f, 1.0f, 0.98f, 0.94f, 0.88f, 0.80f, 0.70f, 0.60f, 0.50f, 0.40f, 0.30f, 0.20f, 0.10f, 0f };

        void GetObjectMoveOffsets(int mapx, int mapy, sbyte object_origin_x, sbyte object_origin_y, float width, float height, long objectcounterdiff, long moveIntervals, long generalcounterdiff, bool foundthisturn, int sub_layer_idx, int sub_layer_cnt, float targetscale, bool loc_is_you, float obj_height, ObjectDataItem otmp_round, ref float object_move_offset_x, ref float object_move_offset_y)
        {
            if(GHUtils.isok(object_origin_x, object_origin_y))
            {
                int objectmovediffx = (int)object_origin_x - mapx;
                int objectmovediffy = (int)object_origin_y - mapy;

                if (objectmovediffx != 0 || objectmovediffy != 0)
                {
                    bool use_objcounter = otmp_round == null || (otmp_round.ObjData.o_id > 0 && otmp_round.ObjData.o_id == _mapData[mapx, mapy].Layers.o_id);
                    long usedcounterdiff = use_objcounter ? objectcounterdiff : generalcounterdiff;
                    if (usedcounterdiff >= 0 && usedcounterdiff < moveIntervals)
                    {
                        object_move_offset_x = width * (float)objectmovediffx * (float)(moveIntervals - usedcounterdiff) / (float)moveIntervals;
                        object_move_offset_y = height * (float)objectmovediffy * (float)(moveIntervals - usedcounterdiff) / (float)moveIntervals;
                    }
                }
            }
            if (foundthisturn)
            {
                long usedcounterdiff = generalcounterdiff - 3L * sub_layer_idx;
                float usedobjheight = obj_height == 0 ? GHConstants.TileHeight / 2 : obj_height;
                float highestpoint = (loc_is_you ? 1.5f : 1.0f) * GHConstants.TileHeight - usedobjheight - (sub_layer_cnt - 1 - sub_layer_idx) * GHConstants.OBJECT_PILE_HEIGHT_DIFFERENCE - GHConstants.OBJECT_PILE_START_HEIGHT;
                if (usedcounterdiff >= 0 && usedcounterdiff < _foundAnimationFactor.Length)
                    object_move_offset_y -= highestpoint * _foundAnimationFactor[usedcounterdiff] * targetscale;
            }
        }

        int GetTileFromAnimation(int ntile, int glyph, int mapx, int mapy, int layer_idx, long generalcountervalue, bool is_monster_or_shadow_layer,
            ref int anim_frame_idx, ref int main_tile_idx, ref int autodraw)
        {
            sbyte mapAnimated = 0;
            int tile_animation_idx = _gnollHackService.GetTileAnimationIndexFromGlyph(glyph);
            bool is_dropping_piercer = (_mapData[mapx, mapy].Layers.monster_flags & (ulong)LayerMonsterFlags.LMFLAGS_DROPPING_PIERCER) != 0;
            lock (AnimationTimerLock)
            {
                if (AnimationTimers.u_action_animation_counter_on && is_monster_or_shadow_layer && ((_mapData[mapx, mapy].Layers.layer_flags & (ulong)LayerFlags.LFLAGS_UXUY) != 0))
                    ntile = _gnollHackService.GetAnimatedTile(ntile, tile_animation_idx, (int)animation_play_types.ANIMATION_PLAY_TYPE_PLAYED_SEPARATELY, AnimationTimers.u_action_animation_counter, out anim_frame_idx, out main_tile_idx, out mapAnimated, ref autodraw);
                else if (AnimationTimers.m_action_animation_counter_on && ((!is_dropping_piercer && is_monster_or_shadow_layer) || (is_dropping_piercer && layer_idx == (int)layer_types.LAYER_MISSILE)) && AnimationTimers.m_action_animation_x == mapx && AnimationTimers.m_action_animation_y == mapy)
                    ntile = _gnollHackService.GetAnimatedTile(ntile, tile_animation_idx, (int)animation_play_types.ANIMATION_PLAY_TYPE_PLAYED_SEPARATELY, AnimationTimers.m_action_animation_counter, out anim_frame_idx, out main_tile_idx, out mapAnimated, ref autodraw);
                else if (_gnollHackService.GlyphIsExplosion(glyph))
                    ntile = _gnollHackService.GetAnimatedTile(ntile, tile_animation_idx, (int)animation_play_types.ANIMATION_PLAY_TYPE_PLAYED_SEPARATELY, AnimationTimers.explosion_animation_counter, out anim_frame_idx, out main_tile_idx, out mapAnimated, ref autodraw);
                else if (_gnollHackService.GlyphIsZap(glyph))
                {
                    for (int zap_anim_idx = 0; zap_anim_idx < GHConstants.MaxPlayedZapAnimations; zap_anim_idx++)
                    {
                        if (AnimationTimers.zap_animation_counter_on[zap_anim_idx]
                            && mapx == AnimationTimers.zap_animation_x[zap_anim_idx]
                            && mapy == AnimationTimers.zap_animation_y[zap_anim_idx])
                        {
                            ntile = _gnollHackService.GetAnimatedTile(ntile, tile_animation_idx, (int)animation_play_types.ANIMATION_PLAY_TYPE_PLAYED_SEPARATELY, AnimationTimers.zap_animation_counter[zap_anim_idx], out anim_frame_idx, out main_tile_idx, out mapAnimated, ref autodraw);
                            break;
                        }
                    }
                }
                else
                {
                    /* Check for special effect animations */
                    bool spef_found = false;
                    for (int spef_idx = 0; spef_idx < GHConstants.MaxPlayedSpecialEffects; spef_idx++)
                    {
                        if (AnimationTimers.special_effect_animation_counter_on[spef_idx]
                            && layer_idx == (int)AnimationTimers.spef_action_animation_layer[spef_idx]
                            && mapx == AnimationTimers.spef_action_animation_x[spef_idx]
                            && mapy == AnimationTimers.spef_action_animation_y[spef_idx])
                        {
                            ntile = _gnollHackService.GetAnimatedTile(ntile, tile_animation_idx, (int)animation_play_types.ANIMATION_PLAY_TYPE_PLAYED_SEPARATELY, AnimationTimers.special_effect_animation_counter[spef_idx], out anim_frame_idx, out main_tile_idx, out mapAnimated, ref autodraw);
                            spef_found = true;
                            break;
                        }
                    }

                    /* Otherwise, normal animation check */
                    if (!spef_found)
                        ntile = _gnollHackService.GetAnimatedTile(ntile, tile_animation_idx, (int)animation_play_types.ANIMATION_PLAY_TYPE_ALWAYS, generalcountervalue, out anim_frame_idx, out main_tile_idx, out mapAnimated, ref autodraw);
                }
            }
            return ntile;
        }

        private void GetMapOffsets(float canvaswidth, float canvasheight, float mapwidth, float mapheight, float width, float height, out float offsetX, out float offsetY, out float usedOffsetX, out float usedOffsetY)
        {
            offsetX = (canvaswidth - mapwidth) / 2;
            offsetY = (canvasheight - mapheight) / 2;
            lock (_mapOffsetLock)
            {
                usedOffsetX = _mapOffsetX;
                usedOffsetY = _mapOffsetY;
            }

            if (ZoomMiniMode)
            {
                //offsetX -= usedOffsetX;
                //offsetY -= usedOffsetY;
                lock (_mapOffsetLock)
                {
                    usedOffsetX = _mapMiniOffsetX;
                    usedOffsetY = _mapMiniOffsetY;
                }
            }
            else
            {
                lock (ClipLock)
                {
                    if (ClipX > 0 && (mapwidth > canvaswidth || mapheight > canvasheight))
                    {
                        offsetX -= (ClipX - (GHConstants.MapCols - 1) / 2) * width;
                        offsetY -= (ClipY - GHConstants.MapRows / 2) * height;
                    }
                }
            }
        }


        //private SKBitmap _enlargementBitmap = null;

#if GNH_MAP_PROFILING && DEBUG
        long _totalFrames = 0L;
#endif

        private void PaintMainGamePage(object sender, SKPaintSurfaceEventArgs e)
        {
            if (!MainGrid.IsVisible)
                return;

            SKImageInfo info = e.Info;
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;
            float canvaswidth = canvasView.CanvasSize.Width;
            float canvasheight = canvasView.CanvasSize.Height;
            bool drawwallends = DrawWallEnds;
            bool breatheanimations = BreatheAnimations;

            canvas.Clear(SKColors.Black);
            if (canvaswidth <= 16 || canvasheight <= 16)
                return;

            _drawCommandList.Clear();

#if GNH_MAP_PROFILING && DEBUG
            _totalFrames++;
            if (_totalFrames <= 0)
                _totalFrames = 1;
#endif
            //if (_enlargementBitmap == null || _enlargementBitmap.Width != (int)canvaswidth || _enlargementBitmap.Height != (int)canvasheight)
            //    _enlargementBitmap = new SKBitmap(Math.Max(1, (int)canvaswidth), Math.Max(1, (int)canvasheight));

            double canvas_scale = GetCanvasScale();
            float inverse_canvas_scale = canvas_scale == 0 ? 0.0f : 1.0f / (float)canvas_scale;
            long generalcountervalue, maincountervalue;
            lock (AnimationTimerLock)
            {
                generalcountervalue = AnimationTimers.general_animation_counter;
            }
            lock (_mainCounterLock)
            {
                maincountervalue = _mainCounterValue;
            }
            long moveIntervals = Math.Max(2, (long)Math.Ceiling((double)UIUtils.GetMainCanvasAnimationFrequency(MapRefreshRate) / 10.0));
            bool lighter_darkening = LighterDarkening;

            using (SKPaint textPaint = new SKPaint())
            {
                string str = "";
                SKRect textBounds = new SKRect();
                SKPathEffect pathEffect = SKPathEffect.CreateDash(_gridIntervals, 0);

                textPaint.Color = SKColors.White;
                textPaint.Typeface = GHApp.LatoRegular;

                /* Map */
                float textscale = GetTextScale();
                float usedFontSize = ZoomAlternateMode ? MapFontAlternateSize : MapFontSize;
                textPaint.Typeface = GHApp.DejaVuSansMonoTypeface;
                textPaint.TextSize = usedFontSize;
                if (ZoomMiniMode)
                {
                    float tmpwidth = textPaint.MeasureText("A"); //textPaint.FontMetrics.AverageCharacterWidth;
                    float tmpheight = textPaint.FontMetrics.Descent - textPaint.FontMetrics.Ascent;
                    if (GraphicsStyle == GHGraphicsStyle.Tiles && !ForceAscii)
                    {
                        tmpwidth = GHConstants.TileWidth * usedFontSize / GHConstants.MapFontDefaultSize;
                        tmpheight = GHConstants.TileHeight * usedFontSize / GHConstants.MapFontDefaultSize;
                    }
                    float tmpmapwidth = tmpwidth * (GHConstants.MapCols - 1);
                    float tmpmapheight = tmpheight * GHConstants.MapRows;
                    float xscale = tmpmapwidth > 0 ? canvaswidth / tmpmapwidth : 0;
                    float yscale = tmpmapheight > 0 ? canvasheight / tmpmapheight : 0;
                    float cscale = Math.Min(xscale, yscale);
                    usedFontSize = Math.Max(2.0f, usedFontSize * cscale);
                    usedFontSize = usedFontSize * MapFontMiniRelativeSize;
                    textPaint.TextSize = usedFontSize;
                }
                float width = textPaint.MeasureText("A"); //textPaint.FontMetrics.AverageCharacterWidth;
                float height = textPaint.FontMetrics.Descent - textPaint.FontMetrics.Ascent;

                if (GraphicsStyle == GHGraphicsStyle.Tiles && !ForceAscii)
                {
                    width = GHConstants.TileWidth * usedFontSize / GHConstants.MapFontDefaultSize;
                    height = GHConstants.TileHeight * usedFontSize / GHConstants.MapFontDefaultSize;
                }

                float mapwidth = width * (GHConstants.MapCols - 1);
                float mapheight = height * (GHConstants.MapRows);

                UsedTileWidth = width;
                UsedTileHeight = height;
                _mapWidth = mapwidth;
                _mapHeight = mapheight;
                float mapFontAscent = textPaint.FontMetrics.Ascent;
                UsedMapFontAscent = mapFontAscent;
                float targetscale = height / (float)GHConstants.TileHeight;

                int startX = 1;
                int endX = GHConstants.MapCols - 1;
                int startY = 0;
                int endY = GHConstants.MapRows - 1;

                float offsetX;
                float offsetY;
                float usedOffsetX;
                float usedOffsetY;
                GetMapOffsets(canvaswidth, canvasheight, mapwidth, mapheight, width, height, out offsetX, out offsetY, out usedOffsetX, out usedOffsetY);

                float tx = 0, ty = 0;
                float startx = 0, starty = 0;
                if (_currentGame != null)
                {
                    lock (_currentGame.WindowsLock)
                    {
                        if (_currentGame.Windows[_currentGame.MapWindowId] != null)
                        {
                            startx = _currentGame.Windows[_currentGame.MapWindowId].Left;
                            starty = _currentGame.Windows[_currentGame.MapWindowId].Top;
                        }
                    }
                }

                if (!ForceAllMessages || HasAllMessagesTransparentBackground)
                {
                    if (_useMapBitmap)
                    {
                        lock (_mapBitmapLock)
                        {
                            if (_mapBitmap != null)
                            {
                                float sourcewidth = (float)(GHConstants.MapCols * GHConstants.TileWidth);
                                float sourceheight = (float)(GHConstants.MapRows * GHConstants.TileHeight);
                                SKRect sourcerect = new SKRect(0, 0, sourcewidth, sourceheight);
                                tx = offsetX + usedOffsetX;
                                ty = offsetY + usedOffsetY + mapFontAscent;
                                SKRect targetrect = new SKRect(tx, ty, tx + sourcewidth * width / (float)GHConstants.TileWidth, ty + sourceheight * height / GHConstants.TileHeight);
                                canvas.DrawBitmap(_mapBitmap, sourcerect, targetrect);
                            }
                        }
                    }
                    else
                    {
                        lock (GHApp.Glyph2TileLock)
                        {
                            lock (_mapDataLock)
                            {
                                int u_x;
                                int u_y;
                                lock (_uLock)
                                {
                                    u_x = _ux;
                                    u_y = _uy;
                                }
                                if (GraphicsStyle == GHGraphicsStyle.ASCII || ForceAscii)
                                {
                                    for (int mapx = startX; mapx <= endX; mapx++)
                                    {
                                        for (int mapy = startY; mapy <= endY; mapy++)
                                        {
                                            if (_mapData[mapx, mapy].Symbol != null && _mapData[mapx, mapy].Symbol != "")
                                            {
                                                str = _mapData[mapx, mapy].Symbol;
                                                textPaint.Color = _mapData[mapx, mapy].Color;
                                                tx = (offsetX + usedOffsetX + width * (float)mapx);
                                                ty = (offsetY + usedOffsetY + height * (float)mapy);
                                                if (CursorStyle == TTYCursorStyle.GreenBlock && _mapCursorX == mapx && _mapCursorY == mapy)
                                                {
                                                    textPaint.Style = SKPaintStyle.Fill;
                                                    textPaint.Color = _cursorDefaultGreen;
                                                    SKRect winRect = new SKRect(tx, ty + textPaint.FontMetrics.Ascent, tx + width, ty + textPaint.FontMetrics.Ascent + height);
                                                    canvas.DrawRect(winRect, textPaint);
                                                    textPaint.Color = SKColors.Black;
                                                }
                                                else if ((_mapData[mapx, mapy].Special & (uint)MapSpecial.Pet) != 0)
                                                {
                                                    textPaint.Style = SKPaintStyle.Fill;
                                                    SKRect winRect = new SKRect(tx, ty + textPaint.FontMetrics.Ascent, tx + width, ty + textPaint.FontMetrics.Ascent + height);
                                                    canvas.DrawRect(winRect, textPaint);
                                                    textPaint.Color = SKColors.Black;
                                                }

                                                canvas.DrawText(str, tx, ty, textPaint);

                                                if ((_mapData[mapx, mapy].Special & (uint)MapSpecial.Peaceful) != 0)
                                                {
                                                    canvas.DrawText("_", tx, ty, textPaint);
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (GHApp.Glyph2Tile != null && GHApp._tilesPerRow[0] > 0 && GHApp.UsedTileSheets > 0)
                                    {
                                        using (SKPaint paint = new SKPaint())
                                        {
                                            paint.FilterQuality = SKFilterQuality.None;

                                            short[,] draw_shadow = new short[GHConstants.MapCols, GHConstants.MapRows];
                                            float pit_border = (float)GHConstants.PIT_BOTTOM_BORDER * height / (float)GHConstants.TileHeight;
                                            long currentcountervalue = generalcountervalue;
                                            float altStartX = -(offsetX + usedOffsetX) / width - 1;
                                            float altEndX = (canvaswidth - (offsetX + usedOffsetX)) / width;
                                            float altStartY = -(offsetY + usedOffsetY) / height - 1;
                                            float altEndY = (canvasheight - (offsetY + usedOffsetY)) / height;
                                            altStartX -= 3;
                                            altEndX += 3;
                                            altStartY -= 1;
                                            altEndY += 3;
                                            startX = Math.Max(startX, (int)(Math.Sign(altStartX) * Math.Floor(Math.Abs(altStartX))));
                                            endX = Math.Min(endX, (int)Math.Ceiling(altEndX));
                                            startY = Math.Max(startY, (int)(Math.Sign(altStartY) * Math.Floor(Math.Abs(altStartY))));
                                            endY = Math.Min(endY, (int)Math.Ceiling(altEndY));

                                            if (AlternativeLayerDrawing)
                                            {
                                                lock (_drawOrderLock)
                                                {
                                                    for (int mapy = startY; mapy <= endY; mapy++)
                                                    {
                                                        for (int mapx = startX; mapx <= endX; mapx++)
                                                        {
                                                            if (_mapData[mapx, mapy].Layers.layer_glyphs == null || _mapData[mapx, mapy].Layers.layer_gui_glyphs == null)
                                                                continue;
                                                            int draw_cnt = _draw_order.Count;
                                                            for (int draw_idx = 0; draw_idx < draw_cnt; draw_idx++)
                                                            {
                                                                int enl_idx = _draw_order[draw_idx].enlargement_position;
                                                                int layer_idx = _draw_order[draw_idx].layer;
                                                                bool is_monster_or_shadow_layer = (layer_idx == (int)layer_types.LAYER_MONSTER || layer_idx == (int)layer_types.MAX_LAYERS);
                                                                bool is_monster_like_layer = (is_monster_or_shadow_layer || layer_idx == (int)layer_types.LAYER_MONSTER_EFFECT);
                                                                bool is_object_like_layer = (layer_idx == (int)layer_types.LAYER_OBJECT || layer_idx == (int)layer_types.LAYER_COVER_OBJECT);
                                                                bool is_missile_layer = (layer_idx == (int)layer_types.LAYER_MISSILE);

                                                                if (layer_idx == (int)layer_types.MAX_LAYERS && (draw_shadow[mapx, mapy] == 0 || _mapData[mapx, mapy].Layers.layer_gui_glyphs[(int)layer_types.LAYER_MONSTER] == GHApp.NoGlyph))
                                                                    continue;

                                                                int source_x = mapx, source_y = mapy;
                                                                switch(enl_idx)
                                                                {
                                                                    default:
                                                                    case -1:
                                                                        break;
                                                                    case 0:
                                                                        source_x = mapx + 1;
                                                                        source_y = mapy + 1;
                                                                        break;
                                                                    case 1:
                                                                        source_y = mapy + 1;
                                                                        break;
                                                                    case 2:
                                                                        source_x = mapx - 1;
                                                                        source_y = mapy + 1;
                                                                        break;
                                                                    case 3:
                                                                        source_x = mapx + 1;
                                                                        break;
                                                                    case 4:
                                                                        source_x = mapx - 1;
                                                                        break;
                                                                }
                                                                if (!GHUtils.isok(source_x, source_y))
                                                                    continue;

                                                                if (enl_idx >= 0 && !_mapData[source_x, source_y].HasEnlargementOrAnimationOrSpecialHeight)
                                                                    continue;

                                                                bool loc_is_you = (_mapData[source_x, source_y].Layers.layer_flags & (ulong)LayerFlags.LFLAGS_UXUY) != 0;
                                                                bool showing_detection = (_mapData[source_x, source_y].Layers.layer_flags & (ulong)LayerFlags.LFLAGS_SHOWING_DETECTION) != 0;
                                                                bool canspotself = (_mapData[source_x, source_y].Layers.monster_flags & (ulong)LayerMonsterFlags.LMFLAGS_CAN_SPOT_SELF) != 0;
                                                                sbyte monster_height = _mapData[source_x, source_y].Layers.special_monster_layer_height;
                                                                sbyte feature_doodad_height = _mapData[source_x, source_y].Layers.special_feature_doodad_layer_height;
                                                                short missile_special_quality = _mapData[source_x, source_y].Layers.missile_special_quality;
                                                                sbyte monster_origin_x = _mapData[source_x, source_y].Layers.monster_origin_x;
                                                                sbyte monster_origin_y = _mapData[source_x, source_y].Layers.monster_origin_y;
                                                                long glyphprintmaincountervalue = _mapData[source_x, source_y].GlyphPrintMainCounterValue;
                                                                int movediffx = (int)monster_origin_x - source_x;
                                                                int movediffy = (int)monster_origin_y - source_y;
                                                                long maincounterdiff = maincountervalue - glyphprintmaincountervalue;
                                                                long glyphobjectprintmaincountervalue = _mapData[source_x, source_y].GlyphObjectPrintMainCounterValue;
                                                                long objectcounterdiff = maincountervalue - glyphobjectprintmaincountervalue;
                                                                long glyphgeneralprintmaincountervalue = _mapData[source_x, source_y].GlyphGeneralPrintMainCounterValue;
                                                                long generalcounterdiff = maincountervalue - glyphgeneralprintmaincountervalue;
                                                                short missile_height = _mapData[source_x, source_y].Layers.missile_height;
                                                                bool obj_in_pit = (_mapData[source_x, source_y].Layers.layer_flags & (ulong)LayerFlags.LFLAGS_O_IN_PIT) != 0;

                                                                float base_move_offset_x = 0, base_move_offset_y = 0;
                                                                //GetBaseMoveOffsets(source_x, source_y, monster_origin_x, monster_origin_y, width, height, maincounterdiff, moveIntervals, ref base_move_offset_x, ref base_move_offset_y);

                                                                lock (_objectDataLock)
                                                                {
                                                                    if (layer_idx == (int)layer_types.MAX_LAYERS + 1)
                                                                    {
                                                                        PaintMapUIElements(canvas, textPaint, paint, pathEffect, mapx, mapy, width, height, offsetX, offsetY, usedOffsetX, usedOffsetY, base_move_offset_x, base_move_offset_y, targetscale, generalcountervalue, usedFontSize, monster_height, loc_is_you, canspotself);
                                                                    }
                                                                    else
                                                                    {
                                                                        bool is_source_dir;
                                                                        int sub_layer_cnt = GetSubLayerCount(source_x, source_y, layer_idx, out is_source_dir);
                                                                        for (int sub_layer_idx = sub_layer_cnt - 1; sub_layer_idx >= 0; sub_layer_idx--)
                                                                        {
                                                                            int signed_glyph = GHApp.NoGlyph; //Default
                                                                            short obj_height = _mapData[source_x, source_y].Layers.object_height; //Default
                                                                            sbyte object_origin_x = 0; //Default
                                                                            sbyte object_origin_y = 0; //Default
                                                                            ObjectDataItem otmp_round = null;

                                                                            int source_dir_main_idx = sub_layer_idx;
                                                                            int source_dir_idx = is_source_dir ? GetSourceDirIndex(layer_idx, source_dir_main_idx) : 0;

                                                                            bool manual_hflip = false;
                                                                            bool manual_vflip = false;
                                                                            int adj_x = source_x;
                                                                            int adj_y = source_y;
                                                                            bool foundthisturn = false;

                                                                            if (!GetLayerGlyph(source_x, source_y, layer_idx, sub_layer_idx, source_dir_idx, ref signed_glyph,
                                                                                ref adj_x, ref adj_y, ref manual_hflip, ref manual_vflip, ref otmp_round, ref obj_height,
                                                                                ref object_origin_x, ref object_origin_y, ref foundthisturn))
                                                                                continue;

                                                                            int glyph = Math.Abs(signed_glyph);
                                                                            if (glyph == 0 || glyph >= GHApp.Glyph2Tile.Length)
                                                                                continue;

                                                                            float object_move_offset_x = 0, object_move_offset_y = 0;
                                                                            //GetObjectMoveOffsets(source_x, source_y, object_origin_x, object_origin_y, width, height, objectcounterdiff, moveIntervals, ref object_move_offset_x, ref object_move_offset_y);

                                                                            bool vflip_glyph = false;
                                                                            bool hflip_glyph = false;
                                                                            GetFlips(signed_glyph, manual_hflip, manual_vflip, ref hflip_glyph, ref vflip_glyph);

                                                                            /* Tile flags */
                                                                            bool tileflag_halfsize = (GHApp.GlyphTileFlags[glyph] & (byte)glyph_tile_flags.GLYPH_TILE_FLAG_HALF_SIZED_TILE) != 0;
                                                                            bool tileflag_floortile = (GHApp.GlyphTileFlags[glyph] & (byte)glyph_tile_flags.GLYPH_TILE_FLAG_HAS_FLOOR_TILE) != 0;
                                                                            bool tileflag_normalobjmissile = (GHApp.GlyphTileFlags[glyph] & (byte)glyph_tile_flags.GLYPH_TILE_FLAG_NORMAL_ITEM_AS_MISSILE) != 0 && layer_idx == (int)layer_types.LAYER_MISSILE;
                                                                            bool tileflag_fullsizeditem = (GHApp.GlyphTileFlags[glyph] & (byte)glyph_tile_flags.GLYPH_TILE_FLAG_FULL_SIZED_ITEM) != 0;
                                                                            bool tileflag_height_is_clipping = (GHApp.GlyphTileFlags[glyph] & (byte)glyph_tile_flags.GLYPH_TILE_FLAG_HEIGHT_IS_CLIPPING) != 0;

                                                                            /* All items are big when showing detection */
                                                                            CheckShowingDetection(showing_detection, ref obj_height, ref tileflag_floortile, ref tileflag_height_is_clipping);

                                                                            /*Determine y move for tiles */
                                                                            float scaled_y_height_change = GetScaledYHeightChange(layer_idx, sub_layer_idx, sub_layer_cnt, height, monster_height, feature_doodad_height, targetscale, is_monster_like_layer, tileflag_halfsize, otmp_round);

                                                                            int ntile = GHApp.Glyph2Tile[glyph];
                                                                            int autodraw = GHApp.Tile2Autodraw[ntile];

                                                                            /* Determine animation tile here */
                                                                            int anim_frame_idx = 0, main_tile_idx = 0;
                                                                            ntile = GetTileFromAnimation(ntile, glyph, source_x, source_y, layer_idx, generalcountervalue, is_monster_or_shadow_layer, ref anim_frame_idx, ref main_tile_idx, ref autodraw);

                                                                            /* Draw enlargement tiles */
                                                                            int enlargement = GHApp.Tile2Enlargement[ntile];
                                                                            int dx = 0, dy = 0;
                                                                            if (!GetEnlargementTileB(enlargement, enl_idx, hflip_glyph, vflip_glyph, main_tile_idx, anim_frame_idx, ref ntile, ref autodraw, ref dx, ref dy))
                                                                                continue;

                                                                            int draw_map_x = source_x + dx + (adj_x - source_x);
                                                                            int draw_map_y = source_y + dy + (adj_y - source_y);

                                                                            //float minDrawX = 0, maxDrawX = 0, minDrawY = 0, maxDrawY = 0;
                                                                            //float enlMinDrawX = 0, enlMaxDrawX = 0, enlMinDrawY = 0, enlMaxDrawY = 0;

                                                                            PaintMapTile(canvas, false, textPaint, paint, layer_idx, source_x, source_y, draw_map_x, draw_map_y, dx, dy, ntile, width, height,
                                                                                offsetX, offsetY, usedOffsetX, usedOffsetY, base_move_offset_x, base_move_offset_y, object_move_offset_x, object_move_offset_y,
                                                                                scaled_y_height_change, pit_border, targetscale, generalcountervalue, usedFontSize,
                                                                                monster_height, is_monster_like_layer, is_object_like_layer, obj_in_pit, obj_height, is_missile_layer, missile_height,
                                                                                loc_is_you, canspotself, tileflag_halfsize, tileflag_normalobjmissile, tileflag_fullsizeditem, tileflag_floortile, tileflag_height_is_clipping,
                                                                                hflip_glyph, vflip_glyph, otmp_round, autodraw, drawwallends, breatheanimations, generalcounterdiff, canvaswidth, canvasheight, enlargement,
                                                                                ref draw_shadow); //, ref minDrawX, ref maxDrawX, ref minDrawY, ref maxDrawY, ref enlMinDrawX, ref enlMaxDrawX, ref enlMinDrawY, ref enlMaxDrawY);
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                //using (SKCanvas enlCanvas = new SKCanvas(_enlargementBitmap))
                                                //{
                                                //    enlCanvas.Clear(SKColors.Transparent);
                                                //    float _enlBmpMinX, _enlBmpMinY, _enlBmpMaxX, _enlBmpMaxY;
                                                //    _enlBmpMaxX = _enlBmpMaxY = 0;
                                                //    _enlBmpMinX = (float)_enlargementBitmap.Width;
                                                //    _enlBmpMinY = (float)_enlargementBitmap.Height;

                                                    for (int layer_idx = 0; layer_idx < (int)layer_types.MAX_LAYERS + 2; layer_idx++)
                                                    {
                                                        bool is_monster_or_shadow_layer = (layer_idx == (int)layer_types.LAYER_MONSTER || layer_idx == (int)layer_types.MAX_LAYERS);
                                                        bool is_monster_like_layer = (is_monster_or_shadow_layer || layer_idx == (int)layer_types.LAYER_MONSTER_EFFECT);
                                                        bool is_object_like_layer = (layer_idx == (int)layer_types.LAYER_OBJECT || layer_idx == (int)layer_types.LAYER_COVER_OBJECT);
                                                        bool is_missile_layer = (layer_idx == (int)layer_types.LAYER_MISSILE);
                                                        for (int mapy = startY; mapy <= endY; mapy++)
                                                        {
                                                            for (int mapx = startX; mapx <= endX; mapx++)
                                                            {
                                                                if (_mapData[mapx, mapy].Layers.layer_glyphs == null || _mapData[mapx, mapy].Layers.layer_gui_glyphs == null)
                                                                    continue;

                                                                if (layer_idx == (int)layer_types.MAX_LAYERS
                                                                    && (draw_shadow[mapx, mapy] == 0 || _mapData[mapx, mapy].Layers.layer_gui_glyphs[(int)layer_types.LAYER_MONSTER] == GHApp.NoGlyph)
                                                                    )
                                                                    continue;

                                                                bool loc_is_you = (_mapData[mapx, mapy].Layers.layer_flags & (ulong)LayerFlags.LFLAGS_UXUY) != 0;
                                                                bool showing_detection = (_mapData[mapx, mapy].Layers.layer_flags & (ulong)LayerFlags.LFLAGS_SHOWING_DETECTION) != 0;
                                                                bool canspotself = (_mapData[mapx, mapy].Layers.monster_flags & (ulong)LayerMonsterFlags.LMFLAGS_CAN_SPOT_SELF) != 0;
                                                                sbyte monster_height = _mapData[mapx, mapy].Layers.special_monster_layer_height;
                                                                sbyte feature_doodad_height = _mapData[mapx, mapy].Layers.special_feature_doodad_layer_height;
                                                                short missile_special_quality = _mapData[mapx, mapy].Layers.missile_special_quality;
                                                                sbyte monster_origin_x = _mapData[mapx, mapy].Layers.monster_origin_x;
                                                                sbyte monster_origin_y = _mapData[mapx, mapy].Layers.monster_origin_y;
                                                                long glyphprintmaincountervalue = _mapData[mapx, mapy].GlyphPrintMainCounterValue;
                                                                long maincounterdiff = maincountervalue - glyphprintmaincountervalue;
                                                                long glyphobjectprintmaincountervalue = _mapData[mapx, mapy].GlyphObjectPrintMainCounterValue;
                                                                long objectcounterdiff = maincountervalue - glyphobjectprintmaincountervalue;
                                                                long glyphgeneralprintmaincountervalue = _mapData[mapx, mapy].GlyphGeneralPrintMainCounterValue;
                                                                long generalcounterdiff = maincountervalue - glyphgeneralprintmaincountervalue;
                                                                short missile_height = _mapData[mapx, mapy].Layers.missile_height;
                                                                bool obj_in_pit = (_mapData[mapx, mapy].Layers.layer_flags & (ulong)LayerFlags.LFLAGS_O_IN_PIT) != 0;

                                                                float base_move_offset_x = 0, base_move_offset_y = 0;
                                                                GetBaseMoveOffsets(mapx, mapy, monster_origin_x, monster_origin_y, width, height, maincounterdiff, moveIntervals, ref base_move_offset_x, ref base_move_offset_y);

                                                                lock (_objectDataLock)
                                                                {
                                                                    if (layer_idx == (int)layer_types.MAX_LAYERS + 1)
                                                                    {
                                                                        PaintMapUIElements(canvas, textPaint, paint, pathEffect, mapx, mapy, width, height, offsetX, offsetY, usedOffsetX, usedOffsetY, base_move_offset_x, base_move_offset_y, targetscale, generalcountervalue, usedFontSize, monster_height, loc_is_you, canspotself);
                                                                    }
                                                                    else
                                                                    {
                                                                        bool is_source_dir;
                                                                        int sub_layer_cnt = GetSubLayerCount(mapx, mapy, layer_idx, out is_source_dir);
                                                                        for (int sub_layer_idx = sub_layer_cnt - 1; sub_layer_idx >= 0; sub_layer_idx--)
                                                                        {
                                                                            int signed_glyph = GHApp.NoGlyph; //Default
                                                                            short obj_height = _mapData[mapx, mapy].Layers.object_height; //Default
                                                                            sbyte object_origin_x = 0; //Default
                                                                            sbyte object_origin_y = 0; //Default
                                                                            ObjectDataItem otmp_round = null;

                                                                            int source_dir_main_idx = sub_layer_idx;
                                                                            int source_dir_idx = is_source_dir ? GetSourceDirIndex(layer_idx, source_dir_main_idx) : 0;

                                                                            bool manual_hflip = false;
                                                                            bool manual_vflip = false;
                                                                            int adj_x = mapx;
                                                                            int adj_y = mapy;
                                                                            bool foundthisturn = false;

                                                                            if (!GetLayerGlyph(mapx, mapy, layer_idx, sub_layer_idx, source_dir_idx, ref signed_glyph,
                                                                                ref adj_x, ref adj_y, ref manual_hflip, ref manual_vflip, ref otmp_round, ref obj_height,
                                                                                ref object_origin_x, ref object_origin_y, ref foundthisturn))
                                                                                continue;

                                                                            int glyph = Math.Abs(signed_glyph);
                                                                            if (glyph == 0 || glyph >= GHApp.Glyph2Tile.Length)
                                                                                continue;

                                                                            float object_move_offset_x = 0, object_move_offset_y = 0;
                                                                            GetObjectMoveOffsets(mapx, mapy, object_origin_x, object_origin_y, width, height, objectcounterdiff, moveIntervals, generalcounterdiff, 
                                                                                foundthisturn, sub_layer_idx, sub_layer_cnt, targetscale, loc_is_you, obj_height, otmp_round, ref object_move_offset_x, ref object_move_offset_y);

                                                                            bool vflip_glyph = false;
                                                                            bool hflip_glyph = false;
                                                                            GetFlips(signed_glyph, manual_hflip, manual_vflip, ref hflip_glyph, ref vflip_glyph);

                                                                            /* Tile flags */
                                                                            bool tileflag_halfsize = (GHApp.GlyphTileFlags[glyph] & (byte)glyph_tile_flags.GLYPH_TILE_FLAG_HALF_SIZED_TILE) != 0;
                                                                            bool tileflag_floortile = (GHApp.GlyphTileFlags[glyph] & (byte)glyph_tile_flags.GLYPH_TILE_FLAG_HAS_FLOOR_TILE) != 0;
                                                                            bool tileflag_normalobjmissile = (GHApp.GlyphTileFlags[glyph] & (byte)glyph_tile_flags.GLYPH_TILE_FLAG_NORMAL_ITEM_AS_MISSILE) != 0 && layer_idx == (int)layer_types.LAYER_MISSILE;
                                                                            bool tileflag_fullsizeditem = (GHApp.GlyphTileFlags[glyph] & (byte)glyph_tile_flags.GLYPH_TILE_FLAG_FULL_SIZED_ITEM) != 0;
                                                                            bool tileflag_height_is_clipping = (GHApp.GlyphTileFlags[glyph] & (byte)glyph_tile_flags.GLYPH_TILE_FLAG_HEIGHT_IS_CLIPPING) != 0;

                                                                            /* All items are big when showing detection */
                                                                            CheckShowingDetection(showing_detection, ref obj_height, ref tileflag_floortile, ref tileflag_height_is_clipping);

                                                                            /*Determine y move for tiles */
                                                                            float scaled_y_height_change = GetScaledYHeightChange(layer_idx, sub_layer_idx, sub_layer_cnt, height, monster_height, feature_doodad_height, targetscale, is_monster_like_layer, tileflag_halfsize, otmp_round);

                                                                            int ntile = GHApp.Glyph2Tile[glyph];
                                                                            int autodraw = GHApp.Tile2Autodraw[ntile];

                                                                            /* Determine animation tile here */
                                                                            int anim_frame_idx = 0, main_tile_idx = 0;
                                                                            ntile = GetTileFromAnimation(ntile, glyph, mapx, mapy, layer_idx, generalcountervalue, is_monster_or_shadow_layer, ref anim_frame_idx, ref main_tile_idx, ref autodraw);

                                                                            /* Draw enlargement tiles */
                                                                            int enlargement = GHApp.Tile2Enlargement[ntile];
                                                                            for (int order_idx = -1; order_idx < 5; order_idx++)
                                                                            {
                                                                                if (enlargement == 0 && order_idx >= 0)
                                                                                    break;
                                                                                int enl_idx = GetUsedEnlargementIndex(order_idx);
                                                                                int dx = 0, dy = 0;
                                                                                if (!GetEnlargementTileB(enlargement, enl_idx, hflip_glyph, vflip_glyph, main_tile_idx, anim_frame_idx, ref ntile, ref autodraw, ref dx, ref dy))
                                                                                    continue;

                                                                                int draw_map_x = mapx + dx + (adj_x - mapx);
                                                                                int draw_map_y = mapy + dy + (adj_y - mapy);

                                                                                if ((enlargement > 0 && enl_idx >= 0 && enl_idx <= 2) || layer_idx == (int)layer_types.MAX_LAYERS)
                                                                                {
                                                                                    PaintMapTile(canvas, true, textPaint, paint, layer_idx, mapx, mapy, draw_map_x, draw_map_y, dx, dy, ntile, width, height,
                                                                                        offsetX, offsetY, usedOffsetX, usedOffsetY, base_move_offset_x, base_move_offset_y, object_move_offset_x, object_move_offset_y,
                                                                                        scaled_y_height_change, pit_border, targetscale, generalcountervalue, usedFontSize,
                                                                                        monster_height, is_monster_like_layer, is_object_like_layer, obj_in_pit, obj_height, is_missile_layer, missile_height,
                                                                                        loc_is_you, canspotself, tileflag_halfsize, tileflag_normalobjmissile, tileflag_fullsizeditem, tileflag_floortile, tileflag_height_is_clipping,
                                                                                        hflip_glyph, vflip_glyph, otmp_round, autodraw, drawwallends, breatheanimations, generalcounterdiff, canvaswidth, canvasheight, enlargement,
                                                                                        ref draw_shadow); //, ref _enlBmpMinX, ref _enlBmpMaxX, ref _enlBmpMinY, ref _enlBmpMaxY, ref _enlBmpMinX, ref _enlBmpMaxX, ref _enlBmpMinY, ref _enlBmpMaxY);
                                                                                }
                                                                                else
                                                                                {
                                                                                    //float minDrawX = 0, maxDrawX = 0, minDrawY = 0, maxDrawY = 0;
                                                                                    PaintMapTile(canvas, false, textPaint, paint, layer_idx, mapx, mapy, draw_map_x, draw_map_y, dx, dy, ntile, width, height,
                                                                                        offsetX, offsetY, usedOffsetX, usedOffsetY, base_move_offset_x, base_move_offset_y, object_move_offset_x, object_move_offset_y,
                                                                                        scaled_y_height_change, pit_border, targetscale, generalcountervalue, usedFontSize,
                                                                                        monster_height, is_monster_like_layer, is_object_like_layer, obj_in_pit, obj_height, is_missile_layer, missile_height,
                                                                                        loc_is_you, canspotself, tileflag_halfsize, tileflag_normalobjmissile, tileflag_fullsizeditem, tileflag_floortile, tileflag_height_is_clipping,
                                                                                        hflip_glyph, vflip_glyph, otmp_round, autodraw, drawwallends, breatheanimations, generalcounterdiff, canvaswidth, canvasheight, enlargement,
                                                                                        ref draw_shadow); //, ref minDrawX, ref maxDrawX, ref minDrawY, ref maxDrawY, ref _enlBmpMinX, ref _enlBmpMaxX, ref _enlBmpMinY, ref _enlBmpMaxY);
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }

                                                        switch(layer_idx)
                                                        {
                                                            /* Darkening at the end of layers */
                                                            case (int)layer_types.LAYER_OBJECT:
                                                            {
                                                                for (int mapx = startX; mapx <= endX; mapx++)
                                                                {
                                                                    for (int mapy = startY; mapy <= endY; mapy++)
                                                                    {
                                                                        bool showing_detection = (_mapData[mapx, mapy].Layers.layer_flags & (ulong)LayerFlags.LFLAGS_SHOWING_DETECTION) != 0;
                                                                        bool darken = (!showing_detection && (_mapData[mapx, mapy].Layers.layer_flags & (ulong)LayerFlags.LFLAGS_CAN_SEE) == 0);
                                                                        bool validpos = (_mapData[mapx, mapy].Layers.layer_flags & (ulong)LayerFlags.LFLAGS_L_LEGAL) != 0 && (_mapData[mapx, mapy].Layers.layer_flags & (ulong)LayerFlags.LFLAGS_CAN_SEE) != 0;
                                                                        bool invalidpos = (_mapData[mapx, mapy].Layers.layer_flags & (ulong)LayerFlags.LFLAGS_L_ILLEGAL) != 0 && (_mapData[mapx, mapy].Layers.layer_flags & (ulong)LayerFlags.LFLAGS_CAN_SEE) != 0;

                                                                        if (_mapData[mapx, mapy].Layers.layer_gui_glyphs != null
                                                                            && (_mapData[mapx, mapy].Layers.layer_gui_glyphs[(int)layer_types.LAYER_FLOOR] == GHApp.UnexploredGlyph
                                                                                || _mapData[mapx, mapy].Layers.layer_gui_glyphs[(int)layer_types.LAYER_FLOOR] == GHApp.NoGlyph)
                                                                            )
                                                                            darken = false;

                                                                        // Draw rectangle with blend mode in bottom half
                                                                        if (darken)
                                                                        {
                                                                            bool uloc = ((_mapData[mapx, mapy].Layers.layer_flags & (ulong)LayerFlags.LFLAGS_UXUY) != 0);
                                                                            bool unlit = ((_mapData[mapx, mapy].Layers.layer_flags & (ulong)LayerFlags.LFLAGS_APPEARS_UNLIT) != 0);
                                                                            // Get values from XAML controls
                                                                            int darken_percentage = lighter_darkening ? (uloc ? 90 : unlit ? 65 : 80) : (uloc ? 85 : unlit ? 40 : 65);
                                                                            int val = (darken_percentage * 255) / 100;
                                                                            SKColor color = new SKColor((byte)val, (byte)val, (byte)val);

                                                                            paint.Color = color;
                                                                            SKBlendMode old_bm = paint.BlendMode;
                                                                            paint.BlendMode = SKBlendMode.Modulate;
                                                                            tx = (offsetX + usedOffsetX + width * (float)mapx);
                                                                            ty = (offsetY + usedOffsetY + mapFontAscent + height * (float)mapy);
                                                                            SKRect targetrect = new SKRect(tx, ty, tx + width, ty + height);
#if GNH_MAP_PROFILING && DEBUG
                                                                            StartProfiling(GHProfilingStyle.Rect);
#endif
                                                                            canvas.DrawRect(targetrect, paint);
                                                                            //enlCanvas.DrawRect(targetrect, paint);
#if GNH_MAP_PROFILING && DEBUG
                                                                            StopProfiling(GHProfilingStyle.Rect);
#endif
                                                                            paint.BlendMode = old_bm;
                                                                        }
                                                                        if (validpos)
                                                                        {
                                                                            paint.Color = new SKColor((byte)0, (byte)255, (byte)0, (byte)72);
                                                                            SKBlendMode old_bm = paint.BlendMode;
                                                                            paint.BlendMode = SKBlendMode.SrcOver;
                                                                            tx = (offsetX + usedOffsetX + width * (float)mapx);
                                                                            ty = (offsetY + usedOffsetY + mapFontAscent + height * (float)mapy);
                                                                            SKRect targetrect = new SKRect(tx, ty, tx + width, ty + height);
#if GNH_MAP_PROFILING && DEBUG
                                                                            StartProfiling(GHProfilingStyle.Rect);
#endif
                                                                            canvas.DrawRect(targetrect, paint);
                                                                            //enlCanvas.DrawRect(targetrect, paint);
#if GNH_MAP_PROFILING && DEBUG
                                                                            StopProfiling(GHProfilingStyle.Rect);
#endif
                                                                            paint.BlendMode = old_bm;
                                                                        }
                                                                        if (invalidpos)
                                                                        {
                                                                            paint.Color = new SKColor((byte)255, (byte)0, (byte)0, (byte)72);
                                                                            SKBlendMode old_bm = paint.BlendMode;
                                                                            paint.BlendMode = SKBlendMode.SrcOver;
                                                                            tx = (offsetX + usedOffsetX + width * (float)mapx);
                                                                            ty = (offsetY + usedOffsetY + mapFontAscent + height * (float)mapy);
                                                                            SKRect targetrect = new SKRect(tx, ty, tx + width, ty + height);
#if GNH_MAP_PROFILING && DEBUG
                                                                            StartProfiling(GHProfilingStyle.Rect);
#endif
                                                                            canvas.DrawRect(targetrect, paint);
                                                                            //enlCanvas.DrawRect(targetrect, paint);
#if GNH_MAP_PROFILING && DEBUG
                                                                            StopProfiling(GHProfilingStyle.Rect);
#endif
                                                                            paint.BlendMode = old_bm;
                                                                        }
                                                                    }
                                                                }
                                                                break;
                                                            }
                                                            /* Enlargement bitmaps */
                                                            case (int)layer_types.MAX_LAYERS:
                                                                //paint.Color = SKColors.Black;
//                                                                if (_enlargementBitmap != null && _enlBmpMinX < _enlBmpMaxX && _enlBmpMinY < _enlBmpMaxY)
//                                                                {
//                                                                    SKRect _copyRect = new SKRect(_enlBmpMinX, _enlBmpMinY, _enlBmpMaxX, _enlBmpMaxY);
//#if GNH_MAP_PROFILING && DEBUG
//                                                                    StartProfiling(GHProfilingStyle.Bitmap);
//#endif
//                                                                    canvas.DrawBitmap(_enlargementBitmap, _copyRect, _copyRect, paint);
//#if GNH_MAP_PROFILING && DEBUG
//                                                                    StopProfiling(GHProfilingStyle.Bitmap);
//#endif
//                                                                }

                                                                using(new SKAutoCanvasRestore(canvas))
                                                                {
                                                                    foreach (GHDrawCommand dc in _drawCommandList)
                                                                    {
                                                                        paint.Color = dc.PaintColor;
                                                                        canvas.SetMatrix(dc.Matrix);
                                                                        canvas.DrawBitmap(dc.SourceBitmap, dc.SourceRect, dc.DestinationRect, paint);
                                                                    }
                                                                }
                                                                _drawCommandList.Clear();

                                                                paint.Color = SKColors.Black;
                                                                for (int mapx = startX; mapx <= endX; mapx++)
                                                                {
                                                                    for (int mapy = startY; mapy <= endY; mapy++)
                                                                    {
                                                                        bool ascension_radiance = (_mapData[mapx, mapy].Layers.layer_flags & (ulong)LayerFlags.LFLAGS_ASCENSION_RADIANCE) != 0
                                                                            && (_mapData[mapx, mapy].Layers.layer_flags & (ulong)LayerFlags.LFLAGS_CAN_SEE) != 0;
                                                                        if (ascension_radiance)
                                                                        {
                                                                            float multiplier = 1.0f - Math.Min(1.0f, 0.3f + (float)Math.Sqrt(Math.Pow(mapx - u_x, 2) + Math.Pow(mapy - u_y, 2)) / 6.0f);
                                                                            int val = (int)(multiplier * 255);
                                                                            SKColor color = new SKColor((byte)val, (byte)val, (byte)val);

                                                                            paint.Color = color;
                                                                            SKBlendMode old_bm = paint.BlendMode;
                                                                            paint.BlendMode = SKBlendMode.Screen;
                                                                            tx = (offsetX + usedOffsetX + width * (float)mapx);
                                                                            ty = (offsetY + usedOffsetY + mapFontAscent + height * (float)mapy);
                                                                            SKRect targetrect = new SKRect(tx, ty, tx + width, ty + height);
#if GNH_MAP_PROFILING && DEBUG
                                                                            StartProfiling(GHProfilingStyle.Rect);
#endif
                                                                            canvas.DrawRect(targetrect, paint);
                                                                            //enlCanvas.DrawRect(targetrect, paint);
#if GNH_MAP_PROFILING && DEBUG
                                                                            StopProfiling(GHProfilingStyle.Rect);
#endif
                                                                            paint.BlendMode = old_bm;
                                                                            if((_mapData[mapx, mapy].Layers.layer_flags & (ulong)LayerFlags.LFLAGS_UXUY) != 0)
                                                                            {
                                                                                UIUtils.DrawSparkle(canvas, paint, tx + 0.5f * width, ty + 0.5f * height, 15f * targetscale, generalcountervalue, true);
                                                                                UIUtils.DrawSparkle(canvas, paint, tx + 0.25f * width, ty + 0.25f * height, 10f * targetscale, generalcountervalue - 10, true);
                                                                                UIUtils.DrawSparkle(canvas, paint, tx + 0.75f * width, ty + 0.25f * height, 10f * targetscale, generalcountervalue - 20, true);
                                                                                UIUtils.DrawSparkle(canvas, paint, tx + 0.25f * width, ty + 0.75f * height, 10f * targetscale, generalcountervalue - 30, true);
                                                                                UIUtils.DrawSparkle(canvas, paint, tx + 0.75f * width, ty + 0.75f * height, 10f * targetscale, generalcountervalue - 40, true);
                                                                                UIUtils.DrawSparkle(canvas, paint, tx + 0.5f * width, ty, 7f * targetscale, generalcountervalue - 50, true);
                                                                                UIUtils.DrawSparkle(canvas, paint, tx + 0.5f * width, ty + height, 7f * targetscale, generalcountervalue - 60, true);
                                                                                UIUtils.DrawSparkle(canvas, paint, tx, ty + 0.5f * height, 7f * targetscale, generalcountervalue - 70, true);
                                                                                UIUtils.DrawSparkle(canvas, paint, tx + width, ty + 0.5f * height, 7f * targetscale, generalcountervalue - 80, true);
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                                break;
                                                        }
                                                    }
                                                //}
                                            }
                                        }
                                    }
                                }
                            }

                            /* Cursor */
                            if ((GraphicsStyle == GHGraphicsStyle.ASCII || ForceAscii) && CursorStyle == TTYCursorStyle.BlinkingUnderline && _cursorIsOn && _mapCursorX >= 1 && _mapCursorY >= 0)
                            {
                                int cx = _mapCursorX, cy = _mapCursorY;
                                str = "_";
                                textPaint.Color = SKColors.White;
                                tx = (offsetX + usedOffsetX + width * (float)cx);
                                ty = (offsetY + usedOffsetY + height * (float)cy);
                                canvas.DrawText(str, tx, ty, textPaint);
                            }
                        }
                    }

                    /* Screen Filter */
                    lock (_screenFilterLock)
                    {
                        foreach (GHScreenFilter ft in _screenFilters)
                        {
                            SKColor fillcolor = SKColors.White;
                            fillcolor = ft.GetColor(generalcountervalue);
                            textPaint.Style = SKPaintStyle.Fill;
                            textPaint.Color = fillcolor;
                            SKRect filterrect = new SKRect(0, 0, canvaswidth, canvasheight);
#if GNH_MAP_PROFILING && DEBUG
                            StartProfiling(GHProfilingStyle.Rect);
#endif
                            canvas.DrawRect(filterrect, textPaint);
#if GNH_MAP_PROFILING && DEBUG
                            StopProfiling(GHProfilingStyle.Rect);
#endif
                        }
                    }

                    /* Floating Texts */
                    if (GraphicsStyle != GHGraphicsStyle.ASCII && !ForceAscii)
                    {
                        lock (_floatingTextLock)
                        {
                            foreach (GHFloatingText ft in _floatingTexts)
                            {
                                SKPoint p;
                                float relativestrokewidth = 0.0f;
                                SKColor strokecolor = SKColors.White;
                                SKColor fillcolor = SKColors.White;
                                p = ft.GetPosition(maincountervalue);
                                fillcolor = ft.GetColor(maincountervalue);
                                textPaint.Typeface = ft.GetTypeface(maincountervalue);
                                textPaint.TextSize = usedFontSize * ft.GetRelativeTextSize(maincountervalue);
                                relativestrokewidth = ft.GetRelativeOutlineWidth(maincountervalue);
                                strokecolor = ft.GetOutlineColor(maincountervalue);
                                str = ft.GetText(maincountervalue);
                                textPaint.MeasureText(str, ref textBounds);
                                tx = (offsetX + usedOffsetX + width * p.X - textBounds.Width / 2);
                                ty = (offsetY + usedOffsetY + height * p.Y - textBounds.Height / 2);
                                if (relativestrokewidth > 0)
                                {
                                    textPaint.Style = SKPaintStyle.Stroke;
                                    textPaint.StrokeWidth = textPaint.TextSize * relativestrokewidth;
                                    textPaint.Color = strokecolor;
#if GNH_MAP_PROFILING && DEBUG
                                    StartProfiling(GHProfilingStyle.Text);
#endif
                                    canvas.DrawText(str, tx, ty, textPaint);
#if GNH_MAP_PROFILING && DEBUG
                                    StopProfiling(GHProfilingStyle.Text);
#endif
                                }
                                textPaint.Style = SKPaintStyle.Fill;
                                textPaint.Color = fillcolor;
#if GNH_MAP_PROFILING && DEBUG
                                StartProfiling(GHProfilingStyle.Bitmap);
#endif
                                canvas.DrawText(str, tx, ty, textPaint);
#if GNH_MAP_PROFILING && DEBUG
                                StopProfiling(GHProfilingStyle.Bitmap);
#endif
                            }
                        }
                        lock (_screenTextLock)
                        {
                            if (_screenText != null)
                            {
                                float targetwidth = 0, yoffsetpct = 0, relativestrokewidth = 0, relativesuperstrokewidth = 0, relativesubstrokewidth = 0;
                                SKColor strokecolor = SKColors.White, superstrokecolor = SKColors.White, substrokecolor = SKColors.White;
                                SKColor fillcolor = SKColors.White;
                                float maxfontsize = 9999.0f;
                                double canvasheightscale = this.Height / canvasView.Height;
                                fillcolor = _screenText.GetTextColor(maincountervalue);
                                textPaint.Typeface = _screenText.GetTextTypeface(maincountervalue);
                                targetwidth = Math.Min(canvaswidth, canvasheight * (float)canvasheightscale) * _screenText.GetMainTextSizeRelativeToScreenWidth(maincountervalue);
                                maxfontsize = _screenText.GetMainTextMaxFontSize(maincountervalue);
                                yoffsetpct = _screenText.GetYOffsetPctOfScreen(maincountervalue);
                                relativestrokewidth = _screenText.GetRelativeTextOutlineWidth(maincountervalue);
                                strokecolor = _screenText.GetTextOutlineColor(maincountervalue);
                                str = _screenText.GetText(maincountervalue);
                                bool useFontSizeStr = str == null || str.Length < 5;
                                textPaint.TextSize = usedFontSize;
                                textPaint.MeasureText(useFontSizeStr ? _fontSizeString : str, ref textBounds);
                                if (textBounds.Width > 0)
                                {
                                    float relativesize = targetwidth / Math.Max(1.0f, textBounds.Width);
                                    //if (relativesize > maxfontsize)
                                    //    relativesize = maxfontsize;
                                    textPaint.TextSize = usedFontSize * relativesize;
                                }

                                textPaint.MeasureText(str, ref textBounds);
                                float maintextascent = textPaint.FontMetrics.Ascent;
                                float maintextdescent = textPaint.FontMetrics.Descent;

                                tx = (canvaswidth / 2 - textBounds.Width / 2);
                                ty = (canvasheight / 2 - textBounds.Height / 2 - (maintextascent + maintextdescent) / 2) + yoffsetpct * canvasheight;
#if GNH_MAP_PROFILING && DEBUG
                                StartProfiling(GHProfilingStyle.Text);
#endif
                                /* Shadow first */
                                {
                                    textPaint.Color = SKColors.Black.WithAlpha(fillcolor.Alpha);
                                    textPaint.MaskFilter = _blur;
                                    float offset = textPaint.TextSize / 15;
                                    canvas.DrawText(str, tx + offset, ty + offset, textPaint);
                                    textPaint.MaskFilter = null;
                                }

                                if (relativestrokewidth > 0)
                                {
                                    textPaint.Style = SKPaintStyle.Stroke;
                                    textPaint.StrokeWidth = textPaint.TextSize * relativestrokewidth;
                                    textPaint.Color = strokecolor;
                                    canvas.DrawText(str, tx, ty, textPaint);
                                }

                                textPaint.Style = SKPaintStyle.Fill;
                                textPaint.Color = fillcolor;
                                canvas.DrawText(str, tx, ty, textPaint);
#if GNH_MAP_PROFILING && DEBUG
                                StopProfiling(GHProfilingStyle.Text);
#endif

                                float maintextsize = textPaint.TextSize;
                                float maintextspacing = textPaint.FontSpacing;
                                float maintexty = ty;

                                if (_screenText.HasSuperText)
                                {
                                    fillcolor = _screenText.GetSuperTextColor(maincountervalue);
                                    textPaint.Typeface = _screenText.GetSuperTextTypeface(maincountervalue);
                                    textPaint.TextSize = maintextsize * _screenText.GetSuperTextSizeRelativeToMainText(maincountervalue);
                                    relativesuperstrokewidth = _screenText.GetRelativeSuperTextOutlineWidth(maincountervalue);
                                    superstrokecolor = _screenText.GetSuperTextOutlineColor(maincountervalue);
                                    str = _screenText.GetSuperText(maincountervalue);
                                    textPaint.MeasureText(str, ref textBounds);
                                    tx = (canvaswidth / 2 - textBounds.Width / 2);
                                    ty = maintexty + maintextascent - textPaint.FontMetrics.Descent;

#if GNH_MAP_PROFILING && DEBUG
                                    StartProfiling(GHProfilingStyle.Text);
#endif
                                    /* Shadow first */
                                    {
                                        SKMaskFilter oldfilter = textPaint.MaskFilter;
                                        textPaint.Color = SKColors.Black.WithAlpha(fillcolor.Alpha);
                                        textPaint.MaskFilter = _blur;
                                        float offset = textPaint.TextSize / 15;
                                        canvas.DrawText(str, tx + offset, ty + offset, textPaint);
                                        textPaint.MaskFilter = null;
                                    }

                                    if (relativesuperstrokewidth > 0)
                                    {
                                        textPaint.Style = SKPaintStyle.Stroke;
                                        textPaint.StrokeWidth = textPaint.TextSize * relativesuperstrokewidth;
                                        textPaint.Color = superstrokecolor;
                                        canvas.DrawText(str, tx, ty, textPaint);
                                    }

                                    textPaint.Style = SKPaintStyle.Fill;
                                    textPaint.Color = fillcolor;
                                    canvas.DrawText(str, tx, ty, textPaint);
#if GNH_MAP_PROFILING && DEBUG
                                    StopProfiling(GHProfilingStyle.Text);
#endif
                                }

                                if (_screenText.HasSubText)
                                {
                                    fillcolor = _screenText.GetSubTextColor(maincountervalue);
                                    textPaint.Typeface = _screenText.GetSubTextTypeface(maincountervalue);
                                    textPaint.TextSize = maintextsize * _screenText.GetSubTextSizeRelativeToMainText(maincountervalue);
                                    relativesubstrokewidth = _screenText.GetRelativeSubTextOutlineWidth(maincountervalue);
                                    substrokecolor = _screenText.GetSubTextOutlineColor(maincountervalue);
                                    str = _screenText.GetSubText(maincountervalue);
                                    textPaint.MeasureText(str, ref textBounds);
                                    tx = (canvaswidth / 2 - textBounds.Width / 2);
                                    ty = maintexty + maintextdescent - textPaint.FontMetrics.Ascent;

#if GNH_MAP_PROFILING && DEBUG
                                    StartProfiling(GHProfilingStyle.Text);
#endif
                                    /* Shadow first */
                                    {
                                        SKMaskFilter oldfilter = textPaint.MaskFilter;
                                        textPaint.Color = SKColors.Black.WithAlpha(fillcolor.Alpha);
                                        textPaint.MaskFilter = _blur;
                                        float offset = textPaint.TextSize / 15;
                                        canvas.DrawText(str, tx + offset, ty + offset, textPaint);
                                        textPaint.MaskFilter = null;
                                    }

                                    if (relativesubstrokewidth > 0)
                                    {
                                        textPaint.Style = SKPaintStyle.Stroke;
                                        textPaint.StrokeWidth = textPaint.TextSize * relativesubstrokewidth;
                                        textPaint.Color = substrokecolor;
                                        canvas.DrawText(str, tx, ty, textPaint);
                                        textPaint.Style = SKPaintStyle.Fill;
                                    }

                                    textPaint.Style = SKPaintStyle.Fill;
                                    textPaint.Color = fillcolor;
                                    canvas.DrawText(str, tx, ty, textPaint);
#if GNH_MAP_PROFILING && DEBUG
                                    StopProfiling(GHProfilingStyle.Text);
#endif
                                }
                            }
                        }
                        lock (_conditionTextLock)
                        {
                            foreach (GHConditionText ft in _conditionTexts)
                            {
                                float relativestrokewidth = 0.0f;
                                SKColor strokecolor = SKColors.White;
                                SKColor fillcolor = SKColors.White;
                                float relativetoscreenwidth = 0.0f;
                                string sampletext = "";
                                fillcolor = ft.GetColor(maincountervalue);
                                textPaint.Typeface = ft.GetTypeface(maincountervalue);
                                relativetoscreenwidth = ft.GetRelativeSampleTextSize(maincountervalue);
                                relativestrokewidth = ft.GetRelativeOutlineWidth(maincountervalue);
                                strokecolor = ft.GetOutlineColor(maincountervalue);
                                str = ft.GetText(maincountervalue);

                                textPaint.TextSize = usedFontSize;
                                sampletext = ft.GetSampleText();
                                textPaint.MeasureText(sampletext, ref textBounds);
                                if (textBounds.Width > 0)
                                {
                                    float relativesize = relativetoscreenwidth * Math.Min(canvaswidth, canvasheight) / textBounds.Width;
                                    textPaint.TextSize = usedFontSize * relativesize;
                                }

                                textPaint.TextAlign = SKTextAlign.Center;
                                tx = canvaswidth / 2;
                                ty = GetStatusBarSkiaHeight() + 1.5f * inverse_canvas_scale * (float)StandardMeasurementButton.Height - textPaint.FontMetrics.Ascent;
#if GNH_MAP_PROFILING && DEBUG
                                StartProfiling(GHProfilingStyle.Text);
#endif
                                if (relativestrokewidth > 0)
                                {
                                    textPaint.Style = SKPaintStyle.Stroke;
                                    textPaint.StrokeWidth = textPaint.TextSize * relativestrokewidth;
                                    textPaint.Color = strokecolor;
                                    canvas.DrawText(str, tx, ty, textPaint);
                                }
                                textPaint.Style = SKPaintStyle.Fill;
                                textPaint.Color = fillcolor;
                                canvas.DrawText(str, tx, ty, textPaint);
#if GNH_MAP_PROFILING && DEBUG
                                StopProfiling(GHProfilingStyle.Text);
#endif
                                textPaint.TextAlign = SKTextAlign.Left;
                            }
                        }
                        lock (_guiEffectLock)
                        {
                            foreach (GHGUIEffect eff in _guiEffects)
                            {
                                SKPoint p;
                                SKColor effcolor;
                                p = eff.GetPosition(maincountervalue);
                                effcolor = eff.GetColor(maincountervalue);
                                tx = offsetX + usedOffsetX + width * p.X;
                                ty = offsetY + usedOffsetY + height * p.Y + mapFontAscent;
                                textPaint.Color = effcolor;
#if GNH_MAP_PROFILING && DEBUG
                                StartProfiling(GHProfilingStyle.Bitmap);
#endif
                                switch (eff.Style)
                                {
                                    case (int)gui_effect_types.GUI_EFFECT_SEARCH:
                                        for (int search_x = -1; search_x <= 1; search_x++)
                                        {
                                            for (int search_y = -1; search_y <= 1; search_y++)
                                            {
                                                if (search_x == 0 && search_y == 0)
                                                    continue;
                                                if (p.X + search_x < 1 || p.X + search_x >= GHConstants.MapCols
                                                    || p.Y + search_y < 0 || p.Y + search_y >= GHConstants.MapRows)
                                                    continue;
                                                float rectsize = Math.Min(width, height);
                                                float rectxmargin = (width - rectsize) / 2;
                                                float rectymargin = (height - rectsize) / 2;
                                                float rectleft = tx + search_x * width + rectxmargin;
                                                float recttop = ty + search_y * height + rectymargin;
                                                SKRect effRect = new SKRect(rectleft, recttop, rectleft + rectsize, recttop + rectsize);
                                                canvas.DrawBitmap(GHApp._searchBitmap, effRect, textPaint);
                                            }
                                        }
                                        break;
                                    case (int)gui_effect_types.GUI_EFFECT_WAIT:
                                        {
                                            float rectsize = Math.Min(width, height);
                                            float rectxmargin = (width - rectsize) / 2;
                                            float rectymargin = (height - rectsize) / 2;
                                            float rectleft = tx + rectxmargin;
                                            float recttop = ty + rectymargin;
                                            SKRect effRect = new SKRect(rectleft, recttop, rectleft + rectsize, recttop + rectsize);
                                            canvas.DrawBitmap(GHApp._waitBitmap, effRect, textPaint);
                                        }
                                        break;
                                    case (int)gui_effect_types.GUI_EFFECT_POLEARM:
                                        {
                                            using(new SKAutoCanvasRestore(canvas))
                                            {
                                                int dx = eff.X2 - eff.X1;
                                                int dy = eff.Y2 - eff.Y1;
                                                if (dx == 0 && dy == 0)
                                                    break;

                                                float length;
                                                canvas.Translate(tx + width / 2, ty + height / 2);
                                                if (dx == 0)
                                                {
                                                    canvas.RotateDegrees(dy < 0 ? 0f: 180f);
                                                    length = Math.Abs(dy * height);
                                                }
                                                else if (dy == 0)
                                                {
                                                    canvas.RotateDegrees(dx < 0 ? -90f : 90f);
                                                    length = Math.Abs(dx * width);
                                                }
                                                else
                                                {
                                                    canvas.RotateRadians((float)Math.Atan2(-dx * width, dy * height) + (float)Math.PI);
                                                    length = (float)Math.Sqrt(Math.Pow(dx * width, 2) + Math.Pow(dy * height, 2));
                                                }
                                                /* Secondary drawing first */
                                                using (SKPath path = new SKPath())
                                                {
                                                    switch (eff.SubType)
                                                    {
                                                        case (int)gui_polearm_types.GUI_POLEARM_LANCE: /* Handle */
                                                            path.MoveTo(-0.04f * width, 0f);
                                                            path.LineTo(0.04f * width, 0f);
                                                            path.LineTo(0.04f * width, -0.52f * width);
                                                            path.LineTo(-0.04f * width, -0.52f * width);
                                                            path.LineTo(-0.04f * width, 0f);
                                                            path.Close();
                                                            textPaint.Style = SKPaintStyle.Fill;
                                                            textPaint.Color = eff.GetSecondaryColor(maincountervalue);
                                                            canvas.DrawPath(path, textPaint);
                                                            textPaint.Style = SKPaintStyle.Stroke;
                                                            textPaint.StrokeWidth = width * 0.02f;
                                                            textPaint.Color = eff.GetSecondaryOutlineColor(maincountervalue);
                                                            canvas.DrawPath(path, textPaint);
                                                            textPaint.Style = SKPaintStyle.Fill;
                                                            using (SKPath path2 = new SKPath())
                                                            {
                                                                path2.MoveTo(-0.015f * width, -0.05f * width);
                                                                path2.LineTo(0.015f * width, -0.05f * width);
                                                                path2.LineTo(0.015f * width, -0.47f * width);
                                                                path2.LineTo(-0.015f * width, -0.47f * width);
                                                                path2.LineTo(-0.015f * width, -0.05f * width);
                                                                path2.Close();
                                                                textPaint.Style = SKPaintStyle.Fill;
                                                                textPaint.Color = eff.GetSecondaryInnerColor(maincountervalue);
                                                                canvas.DrawPath(path2, textPaint);
                                                            }
                                                            break;
                                                        default:
                                                            break;
                                                    }
                                                }
                                                /* Primary drawing */
                                                using (SKPath path = new SKPath())
                                                {
                                                    switch(eff.SubType)
                                                    {
                                                        case (int)gui_polearm_types.GUI_POLEARM_SPEAR:
                                                            path.MoveTo(-0.04f * width, 0f);
                                                            path.LineTo(0.04f * width, 0f);
                                                            path.LineTo(0.04f * width, -length + 0.4f * width);
                                                            path.LineTo(-0.04f * width, -length + 0.4f * width);
                                                            path.LineTo(-0.04f * width, 0f);
                                                            path.Close();
                                                            textPaint.Color = eff.GetColor(maincountervalue);
                                                            textPaint.Style = SKPaintStyle.Fill;
                                                            canvas.DrawPath(path, textPaint);
                                                            textPaint.Style = SKPaintStyle.Stroke;
                                                            textPaint.StrokeWidth = width * 0.02f;
                                                            textPaint.Color = eff.GetOutlineColor(maincountervalue);
                                                            canvas.DrawPath(path, textPaint);
                                                            textPaint.Style = SKPaintStyle.Fill;
                                                            using (SKPath path2 = new SKPath())
                                                            {
                                                                path2.MoveTo(-0.015f * width, -0.05f * width);
                                                                path2.LineTo(0.015f * width, -0.05f * width);
                                                                path2.LineTo(0.015f * width, -length + 0.35f * width);
                                                                path2.LineTo(-0.015f * width, -length + 0.35f * width);
                                                                path2.LineTo(-0.015f * width, -0.05f * width);
                                                                path2.Close();
                                                                textPaint.Style = SKPaintStyle.Fill;
                                                                textPaint.Color = eff.GetInnerColor(maincountervalue);
                                                                canvas.DrawPath(path2, textPaint);
                                                            }
                                                            break;
                                                        case (int)gui_polearm_types.GUI_POLEARM_LANCE:
                                                            path.MoveTo(-0.12f * width, -0.52f * width);
                                                            path.LineTo(0.12f * width, -0.52f * width);
                                                            path.LineTo(0.05f * width, -0.68f * width);
                                                            path.LineTo(0f, -length - 0.2f * width);
                                                            path.LineTo(-0.05f * width, -0.68f * width);
                                                            path.LineTo(-0.12f * width, -0.52f * width);
                                                            path.Close();
                                                            textPaint.Color = eff.GetColor(maincountervalue);
                                                            textPaint.Style = SKPaintStyle.Fill;
                                                            canvas.DrawPath(path, textPaint);
                                                            textPaint.Style = SKPaintStyle.Stroke;
                                                            textPaint.StrokeWidth = width * 0.02f;
                                                            textPaint.Color = eff.GetOutlineColor(maincountervalue);
                                                            canvas.DrawPath(path, textPaint);
                                                            textPaint.Style = SKPaintStyle.Fill;
                                                            using (SKPath path2 = new SKPath())
                                                            {
                                                                path2.MoveTo(-0.08f * width, -0.58f * width);
                                                                path2.LineTo(0.08f * width, -0.58f * width);
                                                                path2.LineTo(0.02f * width, -0.74f * width);
                                                                path2.LineTo(0f, -length - 0.14f * width);
                                                                path2.LineTo(-0.02f * width, -0.74f * width);
                                                                path2.LineTo(-0.08f * width, -0.58f * width);
                                                                path2.Close();
                                                                textPaint.Style = SKPaintStyle.Fill;
                                                                textPaint.Color = eff.GetInnerColor(maincountervalue);
                                                                canvas.DrawPath(path2, textPaint);
                                                            }
                                                            break;
                                                        case (int)gui_polearm_types.GUI_POLEARM_THRUSTED:
                                                        case (int)gui_polearm_types.GUI_POLEARM_POLEAXE:
                                                        default:
                                                            path.MoveTo(-0.05f * width, 0f);
                                                            path.LineTo(0.05f * width, 0f);
                                                            path.LineTo(0.05f * width, -length);
                                                            path.LineTo(-0.05f * width, -length);
                                                            path.LineTo(-0.05f * width, 0f);
                                                            path.Close();
                                                            textPaint.Color = eff.GetColor(maincountervalue);
                                                            textPaint.Style = SKPaintStyle.Fill;
                                                            canvas.DrawPath(path, textPaint);
                                                            textPaint.Style = SKPaintStyle.Stroke;
                                                            textPaint.StrokeWidth = width * 0.02f;
                                                            textPaint.Color = eff.GetOutlineColor(maincountervalue);
                                                            canvas.DrawPath(path, textPaint);
                                                            textPaint.Style = SKPaintStyle.Fill;
                                                            using (SKPath path2 = new SKPath())
                                                            {
                                                                path2.MoveTo(-0.02f * width, -0.05f * width);
                                                                path2.LineTo(0.02f * width, -0.05f * width);
                                                                path2.LineTo(0.02f * width, -length - 0.05f * width);
                                                                path2.LineTo(-0.02f * width, -length - 0.05f * width);
                                                                path2.LineTo(-0.02f * width, -0.05f * width);
                                                                path2.Close();
                                                                textPaint.Style = SKPaintStyle.Fill;
                                                                textPaint.Color = eff.GetInnerColor(maincountervalue);
                                                                canvas.DrawPath(path2, textPaint);
                                                            }
                                                            break;
                                                    }
                                                }
                                                /* Secondary drawing last */
                                                using (SKPath path = new SKPath())
                                                {
                                                    switch (eff.SubType)
                                                    {
                                                        case (int)gui_polearm_types.GUI_POLEARM_SPEAR: /* Spearhead */
                                                            path.MoveTo(-0.06f * width, -length + 0.4f * width);
                                                            path.LineTo(0.06f * width, -length + 0.4f * width);
                                                            path.LineTo(0f, -length);
                                                            path.LineTo(-0.06f * width, -length + 0.4f * width);
                                                            path.Close();
                                                            textPaint.Style = SKPaintStyle.Fill;
                                                            textPaint.Color = eff.GetSecondaryColor(maincountervalue);
                                                            canvas.DrawPath(path, textPaint);
                                                            textPaint.Style = SKPaintStyle.Stroke;
                                                            textPaint.StrokeWidth = width * 0.02f;
                                                            textPaint.Color = eff.GetSecondaryOutlineColor(maincountervalue);
                                                            canvas.DrawPath(path, textPaint);
                                                            textPaint.Style = SKPaintStyle.Fill;
                                                            using (SKPath path2 = new SKPath())
                                                            {
                                                                path2.MoveTo(-0.025f * width, -length + 0.35f * width);
                                                                path2.LineTo(0.025f * width, -length + 0.35f * width);
                                                                path2.LineTo(0f, -length + 0.05f * width);
                                                                path2.LineTo(-0.025f * width, -length + 0.35f * width);
                                                                path2.Close();
                                                                textPaint.Style = SKPaintStyle.Fill;
                                                                textPaint.Color = eff.GetSecondaryInnerColor(maincountervalue);
                                                                canvas.DrawPath(path2, textPaint);
                                                            }
                                                            break;
                                                        case (int)gui_polearm_types.GUI_POLEARM_POLEAXE: /* Polearm head */
                                                        case (int)gui_polearm_types.GUI_POLEARM_THRUSTED: /* Polearm head */
                                                            /* Tip */
                                                            path.MoveTo(-0.04f * width, -length);
                                                            path.LineTo(0.04f * width, -length);
                                                            path.LineTo(0f, -length - 0.4f * width);
                                                            path.LineTo(-0.04f * width, -length);
                                                            path.Close();
                                                            textPaint.Style = SKPaintStyle.Fill;
                                                            textPaint.Color = eff.GetSecondaryColor(maincountervalue);
                                                            canvas.DrawPath(path, textPaint);
                                                            textPaint.Style = SKPaintStyle.Stroke;
                                                            textPaint.StrokeWidth = width * 0.02f;
                                                            textPaint.Color = eff.GetSecondaryOutlineColor(maincountervalue);
                                                            canvas.DrawPath(path, textPaint);
                                                            textPaint.Style = SKPaintStyle.Fill;
                                                            using (SKPath path2 = new SKPath())
                                                            {
                                                                path2.MoveTo(-0.02f * width, -length - 0.05f * width);
                                                                path2.LineTo(0.02f * width, -length - 0.05f * width);
                                                                path2.LineTo(0f, -length - 0.35f * width);
                                                                path2.LineTo(-0.02f * width, -length -0.05f * width);
                                                                path2.Close();
                                                                textPaint.Style = SKPaintStyle.Fill;
                                                                textPaint.Color = eff.GetSecondaryInnerColor(maincountervalue);
                                                                canvas.DrawPath(path2, textPaint);
                                                            }
                                                            /* Left side */
                                                            using (SKPath path2 = new SKPath())
                                                            {
                                                                /* Middle color part 1 */
                                                                path2.MoveTo(-0.04f * width, -length - 0.4f * width + 0.5f * width);
                                                                path2.LineTo(-0.1f * width, -length - 0.4f * width + 0.5f * width);
                                                                path2.LineTo(-0.1f * width, -length - 0.4f * width + 0.65f * width);
                                                                path2.LineTo(-0.16f * width, -length - 0.4f * width + 0.60f * width);
                                                                path2.LineTo(-0.16f * width, -length - 0.4f * width + 0.30f * width);
                                                                path2.LineTo(-0.1f * width, -length - 0.4f * width + 0.25f * width);
                                                                path2.LineTo(-0.1f * width, -length - 0.4f * width + 0.4f * width);
                                                                path2.LineTo(-0.04f * width, -length - 0.4f * width + 0.4f * width);
                                                                path2.LineTo(-0.04f * width, -length - 0.4f * width + 0.5f * width);
                                                                path2.Close();
                                                                textPaint.Style = SKPaintStyle.Fill;
                                                                textPaint.Color = eff.GetSecondaryInner2Color(maincountervalue);
                                                                canvas.DrawPath(path2, textPaint);
                                                            }
                                                            using (SKPath path2 = new SKPath())
                                                            {
                                                                /* Middle color part 2 */
                                                                path2.MoveTo(-0.1f * width, -length - 0.4f * width + 0.65f * width);
                                                                path2.LineTo(-0.1f * width, -length - 0.4f * width + 0.75f * width);
                                                                path2.LineTo(-0.3f * width, -length - 0.4f * width + 0.45f * width);
                                                                path2.LineTo(-0.1f * width, -length - 0.4f * width + 0.15f * width);
                                                                path2.LineTo(-0.1f * width, -length - 0.4f * width + 0.25f * width);
                                                                path2.LineTo(-0.16f * width, -length - 0.4f * width + 0.30f * width);
                                                                path2.LineTo(-0.24f * width, -length - 0.4f * width + 0.45f * width);
                                                                path2.LineTo(-0.16f * width, -length - 0.4f * width + 0.60f * width);
                                                                path2.LineTo(-0.1f * width, -length - 0.4f * width + 0.65f * width);
                                                                path2.Close();
                                                                textPaint.Style = SKPaintStyle.Fill;
                                                                textPaint.Color = eff.GetSecondaryColor(maincountervalue);
                                                                canvas.DrawPath(path2, textPaint);
                                                            }
                                                            using (SKPath path2 = new SKPath())
                                                            {
                                                                /* Outline */
                                                                path2.MoveTo(-0.04f * width, -length - 0.4f * width + 0.5f * width);
                                                                path2.LineTo(-0.1f * width, -length - 0.4f * width + 0.5f * width);
                                                                path2.LineTo(-0.1f * width, -length - 0.4f * width + 0.75f * width);
                                                                path2.LineTo(-0.3f * width, -length - 0.4f * width + 0.45f * width);
                                                                path2.LineTo(-0.1f * width, -length - 0.4f * width + 0.15f * width);
                                                                path2.LineTo(-0.1f * width, -length - 0.4f * width + 0.4f * width);
                                                                path2.LineTo(-0.04f * width, -length - 0.4f * width + 0.4f * width);
                                                                path2.LineTo(-0.04f * width, -length - 0.4f * width + 0.5f * width);
                                                                path2.Close();
                                                                textPaint.Style = SKPaintStyle.Stroke;
                                                                textPaint.StrokeWidth = width * 0.03f;
                                                                textPaint.Color = eff.GetSecondaryOutlineColor(maincountervalue);
                                                                canvas.DrawPath(path2, textPaint);
                                                                textPaint.Style = SKPaintStyle.Fill;
                                                            }
                                                            using (SKPath path2 = new SKPath())
                                                            {
                                                                /* Inner color */
                                                                path2.MoveTo(-0.16f * width, -length - 0.4f * width + 0.60f * width);
                                                                path2.LineTo(-0.24f * width, -length - 0.4f * width + 0.45f * width);
                                                                path2.LineTo(-0.16f * width, -length - 0.4f * width + 0.30f * width);
                                                                path2.LineTo(-0.16f * width, -length - 0.4f * width + 0.60f * width);
                                                                path2.Close();
                                                                textPaint.Style = SKPaintStyle.Fill;
                                                                textPaint.Color = eff.GetSecondaryInnerColor(maincountervalue);
                                                                canvas.DrawPath(path2, textPaint);
                                                            }
                                                            /* Right side */
                                                            //using (SKPath path2 = new SKPath())
                                                            //{
                                                            //    /* Middle color */
                                                            //    path2.MoveTo(0.04f * width, -length - 0.4f * width + 0.5f * width);
                                                            //    path2.LineTo(0.1f * width, -length - 0.4f * width + 0.5f * width);
                                                            //    path2.LineTo(0.1f * width, -length - 0.4f * width + 0.75f * width);
                                                            //    path2.LineTo(0.3f * width, -length - 0.4f * width + 0.45f * width);
                                                            //    path2.LineTo(0.1f * width, -length - 0.4f * width + 0.15f * width);
                                                            //    path2.LineTo(0.1f * width, -length - 0.4f * width + 0.4f * width);
                                                            //    path2.LineTo(0.04f * width, -length - 0.4f * width + 0.4f * width);
                                                            //    path2.LineTo(0.04f * width, -length - 0.4f * width + 0.5f * width);
                                                            //    path2.Close();
                                                            //    textPaint.Style = SKPaintStyle.Fill;
                                                            //    textPaint.Color = eff.GetSecondaryColor(maincountervalue);
                                                            //    canvas.DrawPath(path2, textPaint);
                                                            //    textPaint.Style = SKPaintStyle.Stroke;
                                                            //    textPaint.StrokeWidth = width * 0.03f;
                                                            //    textPaint.Color = eff.GetSecondaryOutlineColor(maincountervalue);
                                                            //    canvas.DrawPath(path2, textPaint);
                                                            //    textPaint.Style = SKPaintStyle.Fill;
                                                            //}
                                                            using (SKPath path2 = new SKPath())
                                                            {
                                                                /* Middle color part 1 */
                                                                path2.MoveTo(0.04f * width, -length - 0.4f * width + 0.5f * width);
                                                                path2.LineTo(0.1f * width, -length - 0.4f * width + 0.5f * width);
                                                                path2.LineTo(0.1f * width, -length - 0.4f * width + 0.65f * width);
                                                                path2.LineTo(0.16f * width, -length - 0.4f * width + 0.60f * width);
                                                                path2.LineTo(0.16f * width, -length - 0.4f * width + 0.30f * width);
                                                                path2.LineTo(0.1f * width, -length - 0.4f * width + 0.25f * width);
                                                                path2.LineTo(0.1f * width, -length - 0.4f * width + 0.4f * width);
                                                                path2.LineTo(0.04f * width, -length - 0.4f * width + 0.4f * width);
                                                                path2.LineTo(0.04f * width, -length - 0.4f * width + 0.5f * width);
                                                                path2.Close();
                                                                textPaint.Style = SKPaintStyle.Fill;
                                                                textPaint.Color = eff.GetSecondaryInner2Color(maincountervalue);
                                                                canvas.DrawPath(path2, textPaint);
                                                            }
                                                            using (SKPath path2 = new SKPath())
                                                            {
                                                                /* Middle color part 2 */
                                                                path2.MoveTo(0.1f * width, -length - 0.4f * width + 0.65f * width);
                                                                path2.LineTo(0.1f * width, -length - 0.4f * width + 0.75f * width);
                                                                path2.LineTo(0.3f * width, -length - 0.4f * width + 0.45f * width);
                                                                path2.LineTo(0.1f * width, -length - 0.4f * width + 0.15f * width);
                                                                path2.LineTo(0.1f * width, -length - 0.4f * width + 0.25f * width);
                                                                path2.LineTo(0.16f * width, -length - 0.4f * width + 0.30f * width);
                                                                path2.LineTo(0.24f * width, -length - 0.4f * width + 0.45f * width);
                                                                path2.LineTo(0.16f * width, -length - 0.4f * width + 0.60f * width);
                                                                path2.LineTo(0.1f * width, -length - 0.4f * width + 0.65f * width);
                                                                path2.Close();
                                                                textPaint.Style = SKPaintStyle.Fill;
                                                                textPaint.Color = eff.GetSecondaryColor(maincountervalue);
                                                                canvas.DrawPath(path2, textPaint);
                                                            }
                                                            using (SKPath path2 = new SKPath())
                                                            {
                                                                /* Outline */
                                                                path2.MoveTo(0.04f * width, -length - 0.4f * width + 0.5f * width);
                                                                path2.LineTo(0.1f * width, -length - 0.4f * width + 0.5f * width);
                                                                path2.LineTo(0.1f * width, -length - 0.4f * width + 0.75f * width);
                                                                path2.LineTo(0.3f * width, -length - 0.4f * width + 0.45f * width);
                                                                path2.LineTo(0.1f * width, -length - 0.4f * width + 0.15f * width);
                                                                path2.LineTo(0.1f * width, -length - 0.4f * width + 0.4f * width);
                                                                path2.LineTo(0.04f * width, -length - 0.4f * width + 0.4f * width);
                                                                path2.LineTo(0.04f * width, -length - 0.4f * width + 0.5f * width);
                                                                path2.Close();
                                                                textPaint.Style = SKPaintStyle.Stroke;
                                                                textPaint.StrokeWidth = width * 0.03f;
                                                                textPaint.Color = eff.GetSecondaryOutlineColor(maincountervalue);
                                                                canvas.DrawPath(path2, textPaint);
                                                                textPaint.Style = SKPaintStyle.Fill;
                                                            }
                                                            using (SKPath path2 = new SKPath())
                                                            {
                                                                /* Inner color */
                                                                path2.MoveTo(0.16f * width, -length - 0.4f * width + 0.60f * width);
                                                                path2.LineTo(0.24f * width, -length - 0.4f * width + 0.45f * width);
                                                                path2.LineTo(0.16f * width, -length - 0.4f * width + 0.30f * width);
                                                                path2.LineTo(0.16f * width, -length - 0.4f * width + 0.60f * width);
                                                                path2.Close();
                                                                textPaint.Style = SKPaintStyle.Fill;
                                                                textPaint.Color = eff.GetSecondaryInnerColor(maincountervalue);
                                                                canvas.DrawPath(path2, textPaint);
                                                            }
                                                            break;
                                                        default:
                                                            break;
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                    default:
                                        break;
                                }
#if GNH_MAP_PROFILING && DEBUG
                                StopProfiling(GHProfilingStyle.Bitmap);
#endif

                            }
                        }
                    }

                    /* Look mode rectangle */
                    if (MapLookMode)
                    {
                        SKColor oldcolor = textPaint.Color;
                        SKMaskFilter oldfilter = textPaint.MaskFilter;
                        SKPaintStyle oldstyle = textPaint.Style;
                        textPaint.MaskFilter = _lookBlur;
                        textPaint.Style = SKPaintStyle.Stroke;
                        textPaint.StrokeWidth = Math.Max(3, Math.Min(canvasheight, canvaswidth) / 15);
                        textPaint.Color = SKColors.Purple.WithAlpha(128);
#if GNH_MAP_PROFILING && DEBUG
                        StartProfiling(GHProfilingStyle.Rect);
#endif
                        canvas.DrawRect(0, 0, canvaswidth, canvasheight, textPaint);
#if GNH_MAP_PROFILING && DEBUG
                        StopProfiling(GHProfilingStyle.Rect);
#endif
                        textPaint.Style = oldstyle;
                        textPaint.Color = oldcolor;
                        textPaint.MaskFilter = oldfilter;
                    }
                }

                /* Darkening background */
                if (ForceAllMessages || ShowNumberPad || ShownTip >= 0)
                {
                    textPaint.Style = SKPaintStyle.Fill;
                    textPaint.Color = ForceAllMessages && !HasAllMessagesTransparentBackground ? SKColors.Black : SKColors.Black.WithAlpha(128);
#if GNH_MAP_PROFILING && DEBUG
                    StartProfiling(GHProfilingStyle.Rect);
#endif
                    canvas.DrawRect(0, 0, canvaswidth, canvasheight, textPaint);
#if GNH_MAP_PROFILING && DEBUG
                    StopProfiling(GHProfilingStyle.Rect);
#endif
                }

                /* Window strings */
                float lastStatusRowPrintY = 0.0f;
                float lastStatusRowFontSpacing = 0.0f;

                lock (_canvasButtonLock)
                {
                    _canvasButtonRect.Top = 0; /* Maybe overrwritten below */
                    _canvasButtonRect.Bottom = canvasheight; /* Maybe overrwritten below */
                    if (_currentGame != null)
                    {
                        lock (_currentGame.WindowsLock)
                        {
                            for (int i = 0; _currentGame.Windows[i] != null && i < GHConstants.MaxGHWindows; i++)
                            {
                                if (_currentGame.Windows[i].Visible && (
                                    _currentGame.Windows[i].WindowPrintStyle == GHWindowPrintLocations.PrintToMap
                                    || _currentGame.Windows[i].WindowPrintStyle == GHWindowPrintLocations.RawPrint))
                                {
                                    if (_currentGame.Windows[i].WindowType == GHWinType.Status && !ClassicStatusBar)
                                        continue;

                                    textPaint.Typeface = _currentGame.Windows[i].Typeface;
                                    textPaint.TextSize = _currentGame.Windows[i].TextSize * textscale;
                                    textPaint.Color = _currentGame.Windows[i].TextColor;
                                    width = textPaint.MeasureText("A"); // textPaint.FontMetrics.AverageCharacterWidth;
                                    height = textPaint.FontSpacing; // textPaint.FontMetrics.Descent - textPaint.FontMetrics.Ascent;

                                    if (_currentGame.Windows[i].AutoPlacement)
                                    {
                                        if (_currentGame.Windows[i].WindowType == GHWinType.Message)
                                        {
                                            float newleft = 0;
                                            float newtop = canvasheight - height * ActualDisplayedMessages - canvasheight * (float)UsedButtonRowStack.Height / Math.Max(1.0f, (float)canvasView.Height) - 30;
                                            _currentGame.Windows[i].Left = newleft;
                                            _currentGame.Windows[i].Top = newtop;
                                        }
                                        else if (_currentGame.Windows[i].WindowType == GHWinType.Here)
                                        {
                                            float newleft = 0;
                                            float messagetop = _currentGame.Windows[_currentGame.MessageWindowId].Top;
                                            float newtop = messagetop - _currentGame.Windows[i].Height - 10 * textscale;
                                            _currentGame.Windows[i].Left = newleft;
                                            _currentGame.Windows[i].Top = newtop;
                                        }
                                    }

                                    SKRect winRect = new SKRect(_currentGame.Windows[i].Left, _currentGame.Windows[i].Top,
                                        _currentGame.Windows[i].Right,
                                        _currentGame.Windows[i].Bottom);

                                    if (_currentGame.Windows[i].CenterHorizontally && winRect.Right - winRect.Left < canvaswidth)
                                    {
                                        float newleft = (canvaswidth - (winRect.Right - winRect.Left)) / 2;
                                        float addition = newleft - winRect.Left;
                                        winRect.Left += addition;
                                        winRect.Right += addition;
                                    }

                                    using (SKPaint winPaint = new SKPaint())
                                    {
                                        winPaint.FilterQuality = SKFilterQuality.None;

                                        winPaint.Color = _currentGame.Windows[i].BackgroundColor;
                                        winPaint.Style = SKPaintStyle.Fill;

                                        if (winPaint.Color != SKColors.Transparent)
                                        {
#if GNH_MAP_PROFILING && DEBUG
                                            StartProfiling(GHProfilingStyle.Rect);
#endif
                                            canvas.DrawRect(winRect, winPaint);
#if GNH_MAP_PROFILING && DEBUG
                                            StopProfiling(GHProfilingStyle.Rect);
#endif

                                        }

                                        if (i == _currentGame.StatusWindowId && ClassicStatusBar)
                                            _canvasButtonRect.Top = winRect.Bottom;
                                        else if (i == _currentGame.MessageWindowId)
                                            _canvasButtonRect.Bottom = winRect.Top;
                                    }

                                    if (_currentGame.Windows[i].WindowType != GHWinType.Message && !ForceAllMessages)
                                    {
                                        lock (_currentGame.Windows[i].PutStrsLock)
                                        {
                                            int j = 0;
                                            foreach (GHPutStrItem putstritem in _currentGame.Windows[i].PutStrs)
                                            {
                                                int pos = 0;
                                                float xpos = 0;
                                                float totwidth = 0;
                                                foreach (GHPutStrInstructions instr in putstritem.InstructionList)
                                                {
                                                    if (putstritem.Text == null)
                                                        str = "";
                                                    else if (pos + instr.PrintLength <= putstritem.Text.Length)
                                                        str = putstritem.Text.Substring(pos, instr.PrintLength);
                                                    else if (putstritem.Text.Length - pos > 0)
                                                        str = putstritem.Text.Substring(pos, putstritem.Text.Length - pos);
                                                    else
                                                        str = "";
                                                    pos += str.Length;
                                                    totwidth = textPaint.MeasureText(str, ref textBounds);

                                                    /* attributes */
                                                    tx = xpos + winRect.Left + _currentGame.Windows[i].Padding.Left;
                                                    ty = winRect.Top + _currentGame.Windows[i].Padding.Top - textPaint.FontMetrics.Ascent + j * height;

#if GNH_MAP_PROFILING && DEBUG
                                                    StartProfiling(GHProfilingStyle.Text);
#endif
                                                    if (_currentGame.Windows[i].HasShadow)
                                                    {
                                                        textPaint.Style = SKPaintStyle.Fill;
                                                        textPaint.Color = SKColors.Black;
                                                        textPaint.MaskFilter = _blur;
                                                        float shadow_offset = 0.15f * textPaint.TextSize;
                                                        canvas.DrawText(str, tx + shadow_offset, ty + shadow_offset, textPaint);
                                                        textPaint.MaskFilter = null;
                                                    }
                                                    if (_currentGame.Windows[i].StrokeWidth > 0)
                                                    {
                                                        textPaint.Style = SKPaintStyle.Stroke;
                                                        textPaint.StrokeWidth = _currentGame.Windows[i].StrokeWidth * textscale;
                                                        textPaint.Color = SKColors.Black;
                                                        canvas.DrawText(str, tx, ty, textPaint);
                                                    }
                                                    textPaint.Style = SKPaintStyle.Fill;
                                                    textPaint.Color = UIUtils.NHColor2SKColor(instr.Color < (int)nhcolor.CLR_MAX ? instr.Color : (int)nhcolor.CLR_WHITE, instr.Attributes);
                                                    canvas.DrawText(str, tx, ty, textPaint);
                                                    textPaint.Style = SKPaintStyle.Fill;
                                                    xpos += totwidth;
#if GNH_MAP_PROFILING && DEBUG
                                                    StopProfiling(GHProfilingStyle.Text);
#endif

                                                    if (_currentGame.Windows[i].WindowType == GHWinType.Status && lastStatusRowPrintY < ty + textPaint.FontMetrics.Descent)
                                                    {
                                                        lastStatusRowPrintY = ty + textPaint.FontMetrics.Descent;
                                                        lastStatusRowFontSpacing = textPaint.FontSpacing;
                                                    }
                                                }
                                                j++;
                                            }
                                        }
                                    }

                                    if (_currentGame.Windows[i].WindowType == GHWinType.Message)
                                    {
                                        lock (_msgHistoryLock)
                                        {
                                            if (_msgHistory != null)
                                            {
                                                int j = ActualDisplayedMessages - 1, idx;
                                                float lineLengthLimit = 0.85f * canvaswidth;
                                                float spaceLength = textPaint.MeasureText(" ");

                                                lock (_refreshMsgHistoryRowCountLock)
                                                {
                                                    bool refreshsmallesttop = true;
                                                    for (idx = _msgHistory.Count - 1; idx >= 0 && j >= 0; idx--)
                                                    {
                                                        GHMsgHistoryItem msgHistoryItem = _msgHistory[idx];
                                                        //longLine = msgHistoryItem.Text;
                                                        SKColor printColor = UIUtils.NHColor2SKColor(
                                                            msgHistoryItem.Colors != null && msgHistoryItem.Colors.Length > 0 ? msgHistoryItem.Colors[0] : msgHistoryItem.NHColor < (int)nhcolor.CLR_MAX ? msgHistoryItem.NHColor : (int)nhcolor.CLR_WHITE, 
                                                            msgHistoryItem.Attributes != null && msgHistoryItem.Attributes.Length > 0 ? msgHistoryItem.Attributes[0] : msgHistoryItem.Attribute);

                                                        bool use_one_color = msgHistoryItem.Colors == null && msgHistoryItem.Attributes == null;
                                                        int char_idx = 0;

                                                        if (_refreshMsgHistoryRowCounts || msgHistoryItem.WrappedTextRows.Count == 0)
                                                        {
                                                            refreshsmallesttop = true;
                                                            msgHistoryItem.WrappedTextRows.Clear();
                                                            float lineLength = 0.0f;
                                                            string line = "";
                                                            string[] txtsplit = msgHistoryItem.TextSplit;
                                                            bool firstonline = true;
                                                            for (int widx = 0; widx < txtsplit.Length; widx++)
                                                            {
                                                                string word = txtsplit[widx];
                                                                string wordWithSpace = word + " ";
                                                                float wordLength = textPaint.MeasureText(wordWithSpace);
                                                                float wordWithSpaceLength = wordLength + spaceLength;
                                                                if (lineLength + wordLength > lineLengthLimit && !firstonline)
                                                                {
                                                                    msgHistoryItem.WrappedTextRows.Add(line);
                                                                    line = wordWithSpace;
                                                                    lineLength = wordWithSpaceLength;
                                                                    firstonline = true;
                                                                }
                                                                else
                                                                {
                                                                    line += wordWithSpace;
                                                                    lineLength += wordWithSpaceLength;
                                                                    firstonline = false;
                                                                }
                                                            }
                                                            msgHistoryItem.WrappedTextRows.Add(line);
                                                        }

                                                        int lineidx;
                                                        for (lineidx = 0; lineidx < msgHistoryItem.WrappedTextRows.Count; lineidx++)
                                                        {
                                                            string wrappedLine = msgHistoryItem.WrappedTextRows[lineidx];
                                                            int window_row_idx = j + lineidx - msgHistoryItem.WrappedTextRows.Count + 1;
                                                            if (window_row_idx < 0)
                                                            {
                                                                char_idx += wrappedLine.Length;
                                                                continue;
                                                            }
                                                            tx = winRect.Left + _currentGame.Windows[i].Padding.Left;
                                                            ty = winRect.Top + _currentGame.Windows[i].Padding.Top - textPaint.FontMetrics.Ascent + window_row_idx * height;
                                                            if (ForceAllMessages)
                                                            {
                                                                ty += _messageScrollOffset;
                                                            }
                                                            if (ty + textPaint.FontMetrics.Descent < 0)
                                                            {
                                                                char_idx += wrappedLine.Length;
                                                                continue;
                                                            }
                                                            if (ty - textPaint.FontMetrics.Ascent > canvasheight)
                                                            {
                                                                char_idx += wrappedLine.Length;
                                                                continue;
                                                            }

                                                            if (use_one_color)
                                                            {
#if GNH_MAP_PROFILING && DEBUG
                                                                StartProfiling(GHProfilingStyle.Text);
#endif
                                                                textPaint.Style = SKPaintStyle.Stroke;
                                                                textPaint.StrokeWidth = _currentGame.Windows[i].StrokeWidth * textscale;
                                                                textPaint.Color = SKColors.Black;
                                                                canvas.DrawText(wrappedLine, tx, ty, textPaint);
                                                                textPaint.Style = SKPaintStyle.Fill;
                                                                textPaint.StrokeWidth = 0;
                                                                textPaint.Color = printColor;
                                                                canvas.DrawText(wrappedLine, tx, ty, textPaint);
                                                                textPaint.Style = SKPaintStyle.Fill;
                                                                textPaint.StrokeWidth = 0;
                                                                textPaint.Color = SKColors.White;
                                                                char_idx += wrappedLine.Length;
#if GNH_MAP_PROFILING && DEBUG
                                                                StopProfiling(GHProfilingStyle.Text);
#endif
                                                            }
                                                            else
                                                            {
                                                                int charidx_start = 0;
                                                                while (char_idx < msgHistoryItem.Text.Length && charidx_start < wrappedLine.Length)
                                                                {
                                                                    int charidx_len = 0;
                                                                    int new_nhcolor = msgHistoryItem.Colors != null && msgHistoryItem.Colors.Length > 0 && char_idx < msgHistoryItem.Colors.Length ? msgHistoryItem.Colors[char_idx] : msgHistoryItem.NHColor < (int)nhcolor.CLR_MAX ? msgHistoryItem.NHColor : (int)nhcolor.CLR_WHITE;
                                                                    int new_nhattr = msgHistoryItem.Attributes != null && msgHistoryItem.Attributes.Length > 0 && char_idx < msgHistoryItem.Attributes.Length ? msgHistoryItem.Attributes[char_idx] : msgHistoryItem.Attribute;
                                                                    int char_idx2 = char_idx;
                                                                    int new_nhcolor2 = new_nhcolor;
                                                                    int new_nhattr2 = new_nhattr;

                                                                    while (char_idx2 < msgHistoryItem.Text.Length && charidx_start + charidx_len < wrappedLine.Length && new_nhcolor == new_nhcolor2 && new_nhattr == new_nhattr2)
                                                                    {
                                                                        char_idx2++;
                                                                        new_nhcolor2 = msgHistoryItem.Colors != null && msgHistoryItem.Colors.Length > 0 && char_idx2 < msgHistoryItem.Colors.Length ? msgHistoryItem.Colors[char_idx2] : msgHistoryItem.NHColor < (int)nhcolor.CLR_MAX ? msgHistoryItem.NHColor : (int)nhcolor.CLR_WHITE;
                                                                        new_nhattr2 = msgHistoryItem.Attributes != null && msgHistoryItem.Attributes.Length > 0 && char_idx2 < msgHistoryItem.Attributes.Length ? msgHistoryItem.Attributes[char_idx2] : msgHistoryItem.Attribute;
                                                                        charidx_len = char_idx2 - char_idx;
                                                                    }

#if GNH_MAP_PROFILING && DEBUG
                                                                    StartProfiling(GHProfilingStyle.Text);
#endif
                                                                    SKColor new_skcolor = UIUtils.NHColor2SKColor(new_nhcolor, new_nhattr);
                                                                    string printedsubline = wrappedLine.Substring(charidx_start, charidx_len);
                                                                    textPaint.Style = SKPaintStyle.Stroke;
                                                                    textPaint.StrokeWidth = _currentGame.Windows[i].StrokeWidth * textscale;
                                                                    textPaint.Color = SKColors.Black;
                                                                    canvas.DrawText(printedsubline, tx, ty, textPaint);
                                                                    textPaint.Style = SKPaintStyle.Fill;
                                                                    textPaint.StrokeWidth = 0;
                                                                    textPaint.Color = new_skcolor;
                                                                    canvas.DrawText(printedsubline, tx, ty, textPaint);
                                                                    float twidth = textPaint.MeasureText(printedsubline);
                                                                    textPaint.Style = SKPaintStyle.Fill;
                                                                    textPaint.StrokeWidth = 0;
                                                                    textPaint.Color = SKColors.White;
#if GNH_MAP_PROFILING && DEBUG
                                                                    StopProfiling(GHProfilingStyle.Text);
#endif

                                                                    tx += twidth;
                                                                    char_idx += charidx_len;
                                                                    charidx_start += charidx_len;
                                                                }
                                                            }
                                                        }
                                                        j -= msgHistoryItem.WrappedTextRows.Count;
                                                    }
                                                    _refreshMsgHistoryRowCounts = false;

                                                    /* Calculate smallest top */
                                                    if(refreshsmallesttop)
                                                    {
                                                        lock (_messageScrollLock)
                                                        {
                                                            _messageSmallestTop = canvasheight;
                                                            j = ActualDisplayedMessages - 1;
                                                            for (idx = _msgHistory.Count - 1; idx >= 0 && j >= 0; idx--)
                                                            {
                                                                GHMsgHistoryItem msgHistoryItem = _msgHistory[idx];
                                                                int lineidx;
                                                                for (lineidx = 0; lineidx < msgHistoryItem.WrappedTextRows.Count; lineidx++)
                                                                {
                                                                    string wrappedLine = msgHistoryItem.WrappedTextRows[lineidx];
                                                                    int window_row_idx = j + lineidx - msgHistoryItem.WrappedTextRows.Count + 1;
                                                                    if (window_row_idx < 0)
                                                                        continue;
                                                                    ty = winRect.Top + _currentGame.Windows[i].Padding.Top - textPaint.FontMetrics.Ascent + window_row_idx * height;
                                                                    if (ty + textPaint.FontMetrics.Ascent < _messageSmallestTop)
                                                                        _messageSmallestTop = ty + textPaint.FontMetrics.Ascent;
                                                                }
                                                                j -= msgHistoryItem.WrappedTextRows.Count;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    float abilitybuttonbottom = (float)((lAbilitiesButton.Y + lAbilitiesButton.Height) / canvasView.Height) * canvasheight;
                    float escbuttonbottom = (float)((StandardMeasurementButton.Y + StandardMeasurementButton.Height) / canvasView.Height) * canvasheight;
                    if (_canvasButtonRect.Top < escbuttonbottom)
                        _canvasButtonRect.Top = escbuttonbottom;
                    if (_canvasButtonRect.Top < abilitybuttonbottom)
                        _canvasButtonRect.Top = abilitybuttonbottom;

                    bool statusfieldsok = false;
                    lock (StatusFieldLock)
                    {
                        statusfieldsok = StatusFields != null;
                    }

                    _statusBarRectDrawn = false;
                    _healthRectDrawn = false;
                    _manaRectDrawn = false;
                    _skillRectDrawn = false;
                    float orbleft = 5.0f;
                    float orbbordersize = (float)(lAbilitiesButton.Width / canvasView.Width) * canvaswidth;

                    if (statusfieldsok && !ForceAllMessages)
                    {
                        float statusbarheight = 0;
                        if (!ClassicStatusBar)
                        {
                            float hmargin = _statusbar_hmargin;
                            float vmargin = _statusbar_vmargin;
                            float rowmargin = _statusbar_rowmargin;
                            float basefontsize = _statusbar_basefontsize * textscale;
                            float shieldfontsize = _statusbar_shieldfontsize * textscale;
                            float diffontsize = _statusbar_diffontsize * textscale;

                            float curx = hmargin;
                            float cury = vmargin;
                            textPaint.TextSize = basefontsize;
                            textPaint.Color = SKColors.Black.WithAlpha(128);
                            float rowheight = textPaint.FontSpacing;
                            float stdspacing = rowheight / 3;
                            float innerspacing = rowheight / 20;
                            statusbarheight = rowheight * 2 + vmargin * 2 + rowmargin;
                            SKRect darkenrect = new SKRect(0, 0, canvaswidth, statusbarheight);
                            StatusBarRect = darkenrect;
                            _statusBarRectDrawn = true;
                            _canvasButtonRect.Top = StatusBarRect.Bottom + 1.25f * inverse_canvas_scale * (float)StandardMeasurementButton.Width;
#if GNH_MAP_PROFILING && DEBUG
                            StartProfiling(GHProfilingStyle.Rect);
#endif
                            canvas.DrawRect(darkenrect, textPaint);
#if GNH_MAP_PROFILING && DEBUG
                            StopProfiling(GHProfilingStyle.Rect);
#endif
                            textPaint.Color = SKColors.White;
                            textPaint.TextAlign = SKTextAlign.Left;
                            textPaint.Typeface = GHApp.LatoRegular;
                            float target_scale = rowheight / GHApp._statusWizardBitmap.Height; // All are 64px high

                            string valtext;
                            SKRect statusDest;
                            SKRect bounds = new SKRect();


                            valtext = "";
                            lock (StatusFieldLock)
                            {
                                if (StatusFields[(int)statusfields.BL_MODE] != null && StatusFields[(int)statusfields.BL_MODE].IsEnabled && StatusFields[(int)statusfields.BL_MODE].Text != null)
                                {
                                    valtext = StatusFields[(int)statusfields.BL_MODE].Text;
                                }
                            }

                            float target_width = 0;
                            float target_height = 0;
                            if (valtext.Contains("W"))
                            {
                                target_width = target_scale * GHApp._statusWizardBitmap.Width;
                                target_height = target_scale * GHApp._statusWizardBitmap.Height;
                                statusDest = new SKRect(curx, cury, curx + target_width, cury + target_height);
#if GNH_MAP_PROFILING && DEBUG
                                StartProfiling(GHProfilingStyle.Bitmap);
#endif
                                canvas.DrawBitmap(GHApp._statusWizardBitmap, statusDest, textPaint);
#if GNH_MAP_PROFILING && DEBUG
                                StopProfiling(GHProfilingStyle.Bitmap);
#endif
                                curx += target_width;
                                curx += innerspacing;
                            }

                            if (valtext.Contains("C"))
                            {
                                target_width = target_scale * GHApp._statusCasualBitmap.Width;
                                target_height = target_scale * GHApp._statusCasualBitmap.Height;
                                statusDest = new SKRect(curx, cury, curx + target_width, cury + target_height);
#if GNH_MAP_PROFILING && DEBUG
                                StartProfiling(GHProfilingStyle.Bitmap);
#endif
                                canvas.DrawBitmap(GHApp._statusCasualBitmap, statusDest, textPaint);
#if GNH_MAP_PROFILING && DEBUG
                                StopProfiling(GHProfilingStyle.Bitmap);
#endif
                                curx += target_width;
                                curx += innerspacing;
                            }
                            else if (valtext.Contains("R"))
                            {
                                target_width = target_scale * GHApp._statusCasualClassicBitmap.Width;
                                target_height = target_scale * GHApp._statusCasualClassicBitmap.Height;
                                statusDest = new SKRect(curx, cury, curx + target_width, cury + target_height);
#if GNH_MAP_PROFILING && DEBUG
                                StartProfiling(GHProfilingStyle.Bitmap);
#endif
                                canvas.DrawBitmap(GHApp._statusCasualClassicBitmap, statusDest, textPaint);
#if GNH_MAP_PROFILING && DEBUG
                                StopProfiling(GHProfilingStyle.Bitmap);
#endif
                                curx += target_width;
                                curx += innerspacing;
                            }
                            else if (valtext.Contains("M"))
                            {
                                target_width = target_scale * GHApp._statusModernBitmap.Width;
                                target_height = target_scale * GHApp._statusModernBitmap.Height;
                                statusDest = new SKRect(curx, cury, curx + target_width, cury + target_height);
#if GNH_MAP_PROFILING && DEBUG
                                StartProfiling(GHProfilingStyle.Bitmap);
#endif
                                canvas.DrawBitmap(GHApp._statusModernBitmap, statusDest, textPaint);
#if GNH_MAP_PROFILING && DEBUG
                                StopProfiling(GHProfilingStyle.Bitmap);
#endif
                                curx += target_width;
                                curx += innerspacing;
                            }

                            SKBitmap difbmp = GHApp._statusDifficultyBitmap;
                            string diftext = "";
                            if (valtext.Contains("s"))
                            {
                                diftext = "s";
                                difbmp = GHApp._statusDifficultyVeryEasyBitmap;
                            }
                            else if (valtext.Contains("e"))
                            {
                                diftext = "e";
                                difbmp = GHApp._statusDifficultyEasyBitmap;
                            }
                            else if (valtext.Contains("a"))
                            {
                                diftext = "a";
                                difbmp = GHApp._statusDifficultyAverageBitmap;
                            }
                            else if (valtext.Contains("v"))
                            {
                                diftext = "v";
                                difbmp = GHApp._statusDifficultyHardBitmap;
                            }
                            else if (valtext.Contains("x"))
                            {
                                diftext = "x";
                                difbmp = GHApp._statusDifficultyExpertBitmap;
                            }
                            else if (valtext.Contains("m"))
                            {
                                diftext = "m";
                                difbmp = GHApp._statusDifficultyMasterBitmap;
                            }
                            else if (valtext.Contains("g"))
                            {
                                diftext = "g";
                                difbmp = GHApp._statusDifficultyGrandMasterBitmap;
                            }

                            if (diftext != "")
                            {
                                target_width = target_scale * difbmp.Width;
                                target_height = target_scale * difbmp.Height;
                                statusDest = new SKRect(curx, cury, curx + target_width, cury + target_height);
#if GNH_MAP_PROFILING && DEBUG
                                StartProfiling(GHProfilingStyle.Bitmap);
#endif
                                canvas.DrawBitmap(difbmp, statusDest, textPaint);
#if GNH_MAP_PROFILING && DEBUG
                                StopProfiling(GHProfilingStyle.Bitmap);
#endif
                                textPaint.MeasureText(diftext, ref bounds);
                                textPaint.TextAlign = SKTextAlign.Center;
                                textPaint.Color = SKColors.Black;
                                textPaint.TextSize = diffontsize;
#if GNH_MAP_PROFILING && DEBUG
                                StartProfiling(GHProfilingStyle.Text);
#endif
                                canvas.DrawText(diftext, curx + target_width / 2, cury + (rowheight - (textPaint.FontSpacing)) / 2 - textPaint.FontMetrics.Ascent, textPaint);
#if GNH_MAP_PROFILING && DEBUG
                                StopProfiling(GHProfilingStyle.Text);
#endif
                                curx += target_width;
                                curx += stdspacing;
                                textPaint.TextAlign = SKTextAlign.Left;
                                textPaint.Color = SKColors.White;
                                textPaint.TextSize = basefontsize;
                            }

                            valtext = "";
                            lock (StatusFieldLock)
                            {
                                if (StatusFields[(int)statusfields.BL_XP] != null && StatusFields[(int)statusfields.BL_XP].IsEnabled && StatusFields[(int)statusfields.BL_XP].Text != null)
                                {
                                    valtext = StatusFields[(int)statusfields.BL_XP].Text;
                                }
                            }
                            if (valtext != "")
                            {
                                target_width = target_scale * GHApp._statusXPLevelBitmap.Width;
                                target_height = target_scale * GHApp._statusXPLevelBitmap.Height;
                                statusDest = new SKRect(curx, cury, curx + target_width, cury + target_height);
#if GNH_MAP_PROFILING && DEBUG
                                StartProfiling(GHProfilingStyle.Bitmap);
#endif
                                canvas.DrawBitmap(GHApp._statusXPLevelBitmap, statusDest, textPaint);
#if GNH_MAP_PROFILING && DEBUG
                                StopProfiling(GHProfilingStyle.Bitmap);
#endif
                                curx += target_width;
                                curx += innerspacing;
                                float print_width = textPaint.MeasureText(valtext);
#if GNH_MAP_PROFILING && DEBUG
                                StartProfiling(GHProfilingStyle.Text);
#endif
                                canvas.DrawText(valtext, curx, cury - textPaint.FontMetrics.Ascent, textPaint);
#if GNH_MAP_PROFILING && DEBUG
                                StopProfiling(GHProfilingStyle.Text);
#endif
                                curx += print_width + stdspacing;
                            }

                            valtext = "";
                            lock (StatusFieldLock)
                            {
                                if (StatusFields[(int)statusfields.BL_HD] != null && StatusFields[(int)statusfields.BL_HD].IsEnabled && StatusFields[(int)statusfields.BL_HD].Text != null)
                                {
                                    valtext = StatusFields[(int)statusfields.BL_HD].Text;
                                }
                            }
                            if (valtext != "")
                            {
                                target_width = target_scale * GHApp._statusHDBitmap.Width;
                                target_height = target_scale * GHApp._statusHDBitmap.Height;
                                statusDest = new SKRect(curx, cury, curx + target_width, cury + target_height);
#if GNH_MAP_PROFILING && DEBUG
                                StartProfiling(GHProfilingStyle.Bitmap);
#endif
                                canvas.DrawBitmap(GHApp._statusHDBitmap, statusDest, textPaint);
#if GNH_MAP_PROFILING && DEBUG
                                StopProfiling(GHProfilingStyle.Bitmap);
#endif
                                curx += target_width;
                                curx += innerspacing;
                                float print_width = textPaint.MeasureText(valtext);
#if GNH_MAP_PROFILING && DEBUG
                                StartProfiling(GHProfilingStyle.Text);
#endif
                                canvas.DrawText(valtext, curx, cury - textPaint.FontMetrics.Ascent, textPaint);
#if GNH_MAP_PROFILING && DEBUG
                                StopProfiling(GHProfilingStyle.Text);
#endif
                                curx += print_width + stdspacing;
                            }

                            valtext = "";
                            lock (StatusFieldLock)
                            {
                                if (StatusFields[(int)statusfields.BL_AC] != null && StatusFields[(int)statusfields.BL_AC].IsEnabled && StatusFields[(int)statusfields.BL_AC].Text != null)
                                {
                                    valtext = StatusFields[(int)statusfields.BL_AC].Text;
                                }
                            }
                            if (valtext != "")
                            {
                                target_width = target_scale * GHApp._statusACBitmap.Width;
                                target_height = target_scale * GHApp._statusACBitmap.Height;
                                statusDest = new SKRect(curx, cury, curx + target_width, cury + target_height);
#if GNH_MAP_PROFILING && DEBUG
                                StartProfiling(GHProfilingStyle.Bitmap);
#endif
                                canvas.DrawBitmap(GHApp._statusACBitmap, statusDest, textPaint);
#if GNH_MAP_PROFILING && DEBUG
                                StopProfiling(GHProfilingStyle.Bitmap);
#endif
                                textPaint.TextAlign = SKTextAlign.Center;
                                textPaint.Color = SKColors.Black;
                                textPaint.TextSize = shieldfontsize;
#if GNH_MAP_PROFILING && DEBUG
                                StartProfiling(GHProfilingStyle.Text);
#endif
                                canvas.DrawText(valtext, curx + target_width / 2, cury + (rowheight - textPaint.FontSpacing) / 2 - textPaint.FontMetrics.Ascent, textPaint);
#if GNH_MAP_PROFILING && DEBUG
                                StopProfiling(GHProfilingStyle.Text);
#endif
                                curx += target_width;
                                curx += stdspacing;
                                textPaint.TextAlign = SKTextAlign.Left;
                                textPaint.Color = SKColors.White;
                                textPaint.TextSize = basefontsize;
                            }

                            valtext = "";
                            string valtext2 = "";
                            lock (StatusFieldLock)
                            {
                                if (StatusFields[(int)statusfields.BL_MC_LVL] != null && StatusFields[(int)statusfields.BL_MC_LVL].IsEnabled && StatusFields[(int)statusfields.BL_MC_LVL].Text != null)
                                {
                                    valtext = StatusFields[(int)statusfields.BL_MC_LVL].Text;
                                }
                                if (StatusFields[(int)statusfields.BL_MC_PCT] != null && StatusFields[(int)statusfields.BL_MC_PCT].IsEnabled && StatusFields[(int)statusfields.BL_MC_PCT].Text != null)
                                {
                                    valtext2 = StatusFields[(int)statusfields.BL_MC_PCT].Text;
                                }
                            }
                            if (valtext != "")
                            {
                                target_width = target_scale * GHApp._statusMCBitmap.Width;
                                target_height = target_scale * GHApp._statusMCBitmap.Height;
                                statusDest = new SKRect(curx, cury, curx + target_width, cury + target_height);
#if GNH_MAP_PROFILING && DEBUG
                                StartProfiling(GHProfilingStyle.Bitmap);
#endif
                                canvas.DrawBitmap(GHApp._statusMCBitmap, statusDest, textPaint);
#if GNH_MAP_PROFILING && DEBUG
                                StopProfiling(GHProfilingStyle.Bitmap);
#endif
                                textPaint.TextAlign = SKTextAlign.Center;
                                textPaint.Color = SKColors.Black;
                                textPaint.TextSize = shieldfontsize;
#if GNH_MAP_PROFILING && DEBUG
                                StartProfiling(GHProfilingStyle.Text);
#endif
                                canvas.DrawText(valtext, curx + target_width / 2, cury + (rowheight - textPaint.FontSpacing) / 2 - textPaint.FontMetrics.Ascent, textPaint);
#if GNH_MAP_PROFILING && DEBUG
                                StopProfiling(GHProfilingStyle.Text);
#endif
                                curx += target_width;
                                curx += innerspacing;
                                textPaint.TextAlign = SKTextAlign.Left;
                                textPaint.Color = SKColors.White;
                                textPaint.TextSize = basefontsize;
                                string printtext = valtext2 + "%";
                                float print_width = textPaint.MeasureText(printtext);
                                canvas.DrawText(printtext, curx, cury - textPaint.FontMetrics.Ascent, textPaint);
                                curx += print_width + stdspacing;
                            }

                            valtext = "";
                            lock (StatusFieldLock)
                            {
                                if (StatusFields[(int)statusfields.BL_MOVE] != null && StatusFields[(int)statusfields.BL_MOVE].IsEnabled && StatusFields[(int)statusfields.BL_MOVE].Text != null)
                                {
                                    valtext = StatusFields[(int)statusfields.BL_MOVE].Text;
                                }
                            }
                            if (valtext != "")
                            {
                                target_width = target_scale * GHApp._statusMoveBitmap.Width;
                                target_height = target_scale * GHApp._statusMoveBitmap.Height;
                                statusDest = new SKRect(curx, cury, curx + target_width, cury + target_height);
#if GNH_MAP_PROFILING && DEBUG
                                StartProfiling(GHProfilingStyle.Bitmap);
#endif
                                canvas.DrawBitmap(GHApp._statusMoveBitmap, statusDest, textPaint);
#if GNH_MAP_PROFILING && DEBUG
                                StopProfiling(GHProfilingStyle.Bitmap);
#endif
                                curx += target_width;
                                curx += innerspacing;
                                float print_width = textPaint.MeasureText(valtext);
                                canvas.DrawText(valtext, curx, cury - textPaint.FontMetrics.Ascent, textPaint);
                                curx += print_width + stdspacing;
                            }

                            valtext = "";
                            valtext2 = "";
                            string valtext3 = "";
                            bool isenabled1 = false;
                            bool isenabled2 = false;
                            bool isenabled3 = false;
                            lock (StatusFieldLock)
                            {
                                if (StatusFields[(int)statusfields.BL_UWEP] != null && StatusFields[(int)statusfields.BL_UWEP].IsEnabled && StatusFields[(int)statusfields.BL_UWEP].Text != null)
                                {
                                    valtext = StatusFields[(int)statusfields.BL_UWEP].Text;
                                    isenabled1 = StatusFields[(int)statusfields.BL_UWEP].IsEnabled;
                                }
                                if (StatusFields[(int)statusfields.BL_UWEP2] != null && StatusFields[(int)statusfields.BL_UWEP2].IsEnabled && StatusFields[(int)statusfields.BL_UWEP2].Text != null)
                                {
                                    valtext2 = StatusFields[(int)statusfields.BL_UWEP2].Text;
                                    isenabled2 = StatusFields[(int)statusfields.BL_UWEP2].IsEnabled;
                                }
                                if (StatusFields[(int)statusfields.BL_UQUIVER] != null && StatusFields[(int)statusfields.BL_UQUIVER].IsEnabled && StatusFields[(int)statusfields.BL_UQUIVER].Text != null)
                                {
                                    valtext3 = StatusFields[(int)statusfields.BL_UQUIVER].Text;
                                    isenabled3 = StatusFields[(int)statusfields.BL_UQUIVER].IsEnabled;
                                }
                            }
                            if (valtext != "" || valtext2 != "" || valtext3 != "")
                            {
                                target_width = target_scale * GHApp._statusWeaponStyleBitmap.Width;
                                target_height = target_scale * GHApp._statusWeaponStyleBitmap.Height;
                                statusDest = new SKRect(curx, cury, curx + target_width, cury + target_height);
#if GNH_MAP_PROFILING && DEBUG
                                StartProfiling(GHProfilingStyle.Bitmap);
#endif
                                canvas.DrawBitmap(GHApp._statusWeaponStyleBitmap, statusDest, textPaint);
#if GNH_MAP_PROFILING && DEBUG
                                StopProfiling(GHProfilingStyle.Bitmap);
#endif
                                curx += target_width;
                                curx += innerspacing;
                                float print_width = 0;
                                if (_drawWeaponStyleAsGlyphs)
                                {
                                    SKTypeface savedtypeface = textPaint.Typeface;
                                    float savedfontsize = textPaint.TextSize;
                                    lock (_weaponStyleObjDataItemLock)
                                    {
                                        if (isenabled1 && valtext != "")
                                        {
                                            /* Right-hand weapon */
                                            if (_weaponStyleObjDataItem[0] != null)
                                            {
                                                float startpicturex = curx;
                                                using (new SKAutoCanvasRestore(canvas, true))
                                                {
                                                    GlyphImageSource gis = _paintGlyphImageSource;
                                                    gis.ReferenceGamePage = this;
                                                    gis.UseUpperSide = false;
                                                    gis.AutoSize = true;
                                                    gis.Glyph = Math.Abs(_weaponStyleObjDataItem[0].ObjData.gui_glyph);
                                                    gis.ObjData = _weaponStyleObjDataItem[0];
                                                    gis.DoAutoSize();
                                                    float wep_scale = gis.Height == 0 ? 1.0f : target_height / gis.Height;
                                                    float weppicturewidth = wep_scale * gis.Width;
                                                    float weppictureheight = wep_scale * gis.Height;
                                                    canvas.Translate(curx + 0, cury + (target_height - weppictureheight) / 2);
                                                    canvas.Scale(wep_scale);
#if GNH_MAP_PROFILING && DEBUG
                                                    StartProfiling(GHProfilingStyle.Bitmap);
#endif
                                                    gis.DrawOnCanvas(canvas);
#if GNH_MAP_PROFILING && DEBUG
                                                    StopProfiling(GHProfilingStyle.Bitmap);
#endif
                                                    curx += weppicturewidth;
                                                    curx += innerspacing;
                                                }
                                                float endpicturex = curx;
#if GNH_MAP_PROFILING && DEBUG
                                                StartProfiling(GHProfilingStyle.Text);
#endif
                                                if (_weaponStyleObjDataItem[0].OutOfAmmo)
                                                {
                                                    textPaint.TextSize = basefontsize;
                                                    string printtext = "X";
                                                    SKColor oldcolor = textPaint.Color;
                                                    print_width = textPaint.MeasureText(printtext);
                                                    float font_height = textPaint.FontMetrics.Descent - textPaint.FontMetrics.Ascent;
                                                    float ontopx = ((endpicturex - startpicturex) - print_width) / 2 + startpicturex;
                                                    textPaint.Color = SKColors.Black;
                                                    textPaint.Style = SKPaintStyle.Stroke;
                                                    textPaint.StrokeWidth = textPaint.TextSize / 5;
                                                    canvas.DrawText(printtext, ontopx, cury - textPaint.FontMetrics.Ascent + (target_height - font_height) / 2, textPaint);
                                                    textPaint.Style = SKPaintStyle.Fill;
                                                    textPaint.Color = SKColors.Red;
                                                    canvas.DrawText(printtext, ontopx, cury - textPaint.FontMetrics.Ascent + (target_height - font_height) / 2, textPaint);
                                                    textPaint.Color = oldcolor;

                                                }
                                                if (_weaponStyleObjDataItem[0].WrongAmmoType)
                                                {
                                                    textPaint.Typeface = GHApp.LatoBold;
                                                    textPaint.TextSize = basefontsize;
                                                    string printtext = "?";
                                                    SKColor oldcolor = textPaint.Color;
                                                    print_width = textPaint.MeasureText(printtext);
                                                    float font_height = textPaint.FontMetrics.Descent - textPaint.FontMetrics.Ascent;
                                                    float ontopx = ((endpicturex - startpicturex) - print_width) / 2 + startpicturex;
                                                    textPaint.Color = SKColors.Black;
                                                    textPaint.Style = SKPaintStyle.Stroke;
                                                    textPaint.StrokeWidth = textPaint.TextSize / 5;
                                                    canvas.DrawText(printtext, ontopx, cury - textPaint.FontMetrics.Ascent + (target_height - font_height) / 2, textPaint);
                                                    textPaint.Style = SKPaintStyle.Fill;
                                                    textPaint.Color = SKColors.Red;
                                                    canvas.DrawText(printtext, ontopx, cury - textPaint.FontMetrics.Ascent + (target_height - font_height) / 2, textPaint);
                                                    textPaint.Color = oldcolor;
                                                    textPaint.Typeface = GHApp.LatoRegular;
                                                }
                                                if (_weaponStyleObjDataItem[0].NotBeingUsed || _weaponStyleObjDataItem[0].NotWeapon)
                                                {
                                                    textPaint.Typeface = GHApp.LatoBold;
                                                    textPaint.TextSize = basefontsize;
                                                    string printtext = "!";
                                                    SKColor oldcolor = textPaint.Color;
                                                    print_width = textPaint.MeasureText(printtext);
                                                    float ontopx = ((endpicturex - startpicturex) - print_width) / 2 + startpicturex;
                                                    float font_height = textPaint.FontMetrics.Descent - textPaint.FontMetrics.Ascent;
                                                    textPaint.Color = SKColors.Black;
                                                    textPaint.Style = SKPaintStyle.Stroke;
                                                    textPaint.StrokeWidth = textPaint.TextSize / 5;
                                                    canvas.DrawText(printtext, ontopx, cury - textPaint.FontMetrics.Ascent + (target_height - font_height) / 2, textPaint);
                                                    textPaint.Style = SKPaintStyle.Fill;
                                                    textPaint.Color = SKColors.Orange;
                                                    canvas.DrawText(printtext, ontopx, cury - textPaint.FontMetrics.Ascent + (target_height - font_height) / 2, textPaint);
                                                    textPaint.Color = oldcolor;
                                                    textPaint.Typeface = GHApp.LatoRegular;
                                                }
#if GNH_MAP_PROFILING && DEBUG
                                                StopProfiling(GHProfilingStyle.Text);
#endif
                                            }
                                            else
                                            {
                                                SKRect emptyHandedSource = new SKRect(0, 0, GHApp._statusEmptyHandedBitmap.Width, GHApp._statusEmptyHandedBitmap.Height);
                                                float empty_handed_scale = rowheight / GHApp._statusEmptyHandedBitmap.Height;
                                                if (valtext2 != "")
                                                {
                                                    emptyHandedSource = new SKRect(0, 0, GHApp._statusEmptyHandedBitmap.Width / 2, GHApp._statusEmptyHandedBitmap.Height);
                                                    target_width = empty_handed_scale * GHApp._statusEmptyHandedBitmap.Width / 2;
                                                    target_height = empty_handed_scale * GHApp._statusEmptyHandedBitmap.Height;
                                                }
                                                else
                                                {
                                                    target_width = empty_handed_scale * GHApp._statusEmptyHandedBitmap.Width;
                                                    target_height = empty_handed_scale * GHApp._statusEmptyHandedBitmap.Height;
                                                }
                                                statusDest = new SKRect(curx, cury, curx + target_width, cury + target_height);
#if GNH_MAP_PROFILING && DEBUG
                                                StartProfiling(GHProfilingStyle.Bitmap);
#endif
                                                canvas.DrawBitmap(GHApp._statusEmptyHandedBitmap, emptyHandedSource, statusDest, textPaint);
#if GNH_MAP_PROFILING && DEBUG
                                                StopProfiling(GHProfilingStyle.Bitmap);
#endif
                                                curx += target_width;
                                                curx += innerspacing;
                                            }
                                            ///* Ammo */
                                            //if (_weaponStyleObjDataItem[2] != null)
                                            //{
                                            //    string printtext = "+";
                                            //    print_width = textPaint.MeasureText(printtext);
                                            //    canvas.DrawText(printtext, curx, cury - textPaint.FontMetrics.Ascent, textPaint);
                                            //    curx += print_width;
                                            //    using (new SKAutoCanvasRestore(canvas, true))
                                            //    {
                                            //        GlyphImageSource gis = _paintGlyphImageSource;
                                            //        gis.ReferenceGamePage = this;
                                            //        gis.UseUpperSide = false;
                                            //        gis.AutoSize = true;
                                            //        gis.Glyph = Math.Abs(_weaponStyleObjDataItem[2].ObjData.gui_glyph);
                                            //        gis.ObjData = _weaponStyleObjDataItem[2];
                                            //        gis.DoAutoSize();
                                            //        float wep_scale = gis.Height == 0 ? 1.0f : target_height / gis.Height;
                                            //        float weppicturewidth = wep_scale * gis.Width;
                                            //        float weppictureheight = wep_scale * gis.Height;
                                            //        canvas.Translate(curx + 0, cury + (target_height - weppictureheight) / 2);
                                            //        canvas.Scale(wep_scale);
                                            //        gis.DrawOnCanvas(canvas);
                                            //        curx += weppicturewidth;
                                            //        curx += innerspacing;
                                            //    }
                                            //}
                                        }
                                        if (isenabled2 && valtext2 != "")
                                        {
                                            /* Left-hand weapon */
                                            if (_weaponStyleObjDataItem[1] != null)
                                            {
                                                textPaint.TextSize = shieldfontsize;
                                                string printtext = "+";
                                                print_width = textPaint.MeasureText(printtext);
                                                canvas.DrawText(printtext, curx, cury - textPaint.FontMetrics.Ascent, textPaint);
                                                curx += print_width;

                                                float startpicturex = curx;
                                                using (new SKAutoCanvasRestore(canvas, true))
                                                {
                                                    GlyphImageSource gis = _paintGlyphImageSource;
                                                    gis.ReferenceGamePage = this;
                                                    gis.UseUpperSide = false;
                                                    gis.AutoSize = true;
                                                    gis.Glyph = Math.Abs(_weaponStyleObjDataItem[1].ObjData.gui_glyph);
                                                    gis.ObjData = _weaponStyleObjDataItem[1];
                                                    gis.DoAutoSize();
                                                    float wep_scale = gis.Height == 0 ? 1.0f : target_height / gis.Height;
                                                    float weppicturewidth = wep_scale * gis.Width;
                                                    float weppictureheight = wep_scale * gis.Height;
                                                    canvas.Translate(curx + 0, cury + (target_height - weppictureheight) / 2);
                                                    canvas.Scale(wep_scale);
#if GNH_MAP_PROFILING && DEBUG
                                                    StartProfiling(GHProfilingStyle.Bitmap);
#endif
                                                    gis.DrawOnCanvas(canvas);
#if GNH_MAP_PROFILING && DEBUG
                                                    StopProfiling(GHProfilingStyle.Bitmap);
#endif
                                                    curx += weppicturewidth;
                                                    curx += innerspacing;
                                                }
                                                float endpicturex = curx;
#if GNH_MAP_PROFILING && DEBUG
                                                StartProfiling(GHProfilingStyle.Text);
#endif
                                                if (_weaponStyleObjDataItem[1].OutOfAmmo)
                                                {
                                                    textPaint.TextSize = basefontsize;
                                                    printtext = "X";
                                                    SKColor oldcolor = textPaint.Color;
                                                    print_width = textPaint.MeasureText(printtext);
                                                    float ontopx = ((endpicturex - startpicturex) - print_width) / 2 + startpicturex;
                                                    float font_height = textPaint.FontMetrics.Descent - textPaint.FontMetrics.Ascent;
                                                    textPaint.Color = SKColors.Black;
                                                    textPaint.Style = SKPaintStyle.Stroke;
                                                    textPaint.StrokeWidth = textPaint.TextSize / 5;
                                                    canvas.DrawText(printtext, ontopx, cury - textPaint.FontMetrics.Ascent + (target_height - font_height) / 2, textPaint);
                                                    textPaint.Style = SKPaintStyle.Fill;
                                                    textPaint.Color = SKColors.Red;
                                                    canvas.DrawText(printtext, ontopx, cury - textPaint.FontMetrics.Ascent + (target_height - font_height) / 2, textPaint);
                                                    textPaint.Color = oldcolor;

                                                }
                                                if (_weaponStyleObjDataItem[1].WrongAmmoType)
                                                {
                                                    textPaint.Typeface = GHApp.LatoBold;
                                                    textPaint.TextSize = basefontsize;
                                                    printtext = "?";
                                                    SKColor oldcolor = textPaint.Color;
                                                    print_width = textPaint.MeasureText(printtext);
                                                    float ontopx = ((endpicturex - startpicturex) - print_width) / 2 + startpicturex;
                                                    float font_height = textPaint.FontMetrics.Descent - textPaint.FontMetrics.Ascent;
                                                    textPaint.Color = SKColors.Black;
                                                    textPaint.Style = SKPaintStyle.Stroke;
                                                    textPaint.StrokeWidth = textPaint.TextSize / 5;
                                                    canvas.DrawText(printtext, ontopx, cury - textPaint.FontMetrics.Ascent + (target_height - font_height) / 2, textPaint);
                                                    textPaint.Style = SKPaintStyle.Fill;
                                                    textPaint.Color = SKColors.Red;
                                                    canvas.DrawText(printtext, ontopx, cury - textPaint.FontMetrics.Ascent + (target_height - font_height) / 2, textPaint);
                                                    textPaint.Color = oldcolor;
                                                    textPaint.Typeface = GHApp.LatoRegular;
                                                }
                                                if (_weaponStyleObjDataItem[1].NotBeingUsed || _weaponStyleObjDataItem[1].NotWeapon)
                                                {
                                                    textPaint.Typeface = GHApp.LatoBold;
                                                    textPaint.TextSize = basefontsize;
                                                    printtext = "!";
                                                    SKColor oldcolor = textPaint.Color;
                                                    textPaint.Color = SKColors.Orange;
                                                    print_width = textPaint.MeasureText(printtext);
                                                    float ontopx = ((endpicturex - startpicturex) - print_width) / 2 + startpicturex;
                                                    float font_height = textPaint.FontMetrics.Descent - textPaint.FontMetrics.Ascent;
                                                    textPaint.Color = SKColors.Black;
                                                    textPaint.Style = SKPaintStyle.Stroke;
                                                    textPaint.StrokeWidth = textPaint.TextSize / 5;
                                                    canvas.DrawText(printtext, ontopx, cury - textPaint.FontMetrics.Ascent + (target_height - font_height) / 2, textPaint);
                                                    textPaint.Style = SKPaintStyle.Fill;
                                                    textPaint.Color = SKColors.Orange;
                                                    canvas.DrawText(printtext, ontopx, cury - textPaint.FontMetrics.Ascent + (target_height - font_height) / 2, textPaint);
                                                    textPaint.Color = oldcolor;
                                                    textPaint.Typeface = GHApp.LatoRegular;
                                                }
#if GNH_MAP_PROFILING && DEBUG
                                                StopProfiling(GHProfilingStyle.Text);
#endif
                                            }
                                            else
                                            {
                                                SKRect emptyHandedSource = new SKRect(0, 0, GHApp._statusEmptyHandedBitmap.Width, GHApp._statusEmptyHandedBitmap.Height);
                                                float empty_handed_scale = rowheight / GHApp._statusEmptyHandedBitmap.Height;
                                                if (valtext != "")
                                                {
                                                    string printtext = "+";
                                                    print_width = textPaint.MeasureText(printtext);
                                                    canvas.DrawText(printtext, curx, cury - textPaint.FontMetrics.Ascent, textPaint);
                                                    curx += print_width;

                                                    emptyHandedSource = new SKRect(GHApp._statusEmptyHandedBitmap.Width / 2, 0, GHApp._statusEmptyHandedBitmap.Width, GHApp._statusEmptyHandedBitmap.Height);
                                                    target_width = empty_handed_scale * GHApp._statusEmptyHandedBitmap.Width / 2;
                                                    target_height = empty_handed_scale * GHApp._statusEmptyHandedBitmap.Height;
                                                }
                                                else
                                                {
                                                    target_width = empty_handed_scale * GHApp._statusEmptyHandedBitmap.Width;
                                                    target_height = empty_handed_scale * GHApp._statusEmptyHandedBitmap.Height;
                                                }
                                                statusDest = new SKRect(curx, cury, curx + target_width, cury + target_height);
#if GNH_MAP_PROFILING && DEBUG
                                                StartProfiling(GHProfilingStyle.Bitmap);
#endif
                                                canvas.DrawBitmap(GHApp._statusEmptyHandedBitmap, emptyHandedSource, statusDest, textPaint);
#if GNH_MAP_PROFILING && DEBUG
                                                StopProfiling(GHProfilingStyle.Bitmap);
#endif
                                                curx += target_width;
                                                curx += innerspacing;
                                            }
                                        }

                                        if (isenabled3 && valtext3 != "")
                                        {
                                            /* Throwing weapons in quiver (which are not ammo by definition) */
                                            if (_weaponStyleObjDataItem[2] != null && _weaponStyleObjDataItem[2].IsThrowingWeapon && !_weaponStyleObjDataItem[2].IsAmmo)
                                            {
                                                curx += innerspacing; /* More space to other weapon styles */
                                                target_width = target_scale * GHApp._statusQuiveredWeaponStyleBitmap.Width;
                                                target_height = target_scale * GHApp._statusQuiveredWeaponStyleBitmap.Height;
                                                statusDest = new SKRect(curx, cury, curx + target_width, cury + target_height);
#if GNH_MAP_PROFILING && DEBUG
                                                StartProfiling(GHProfilingStyle.Bitmap);
#endif
                                                canvas.DrawBitmap(GHApp._statusQuiveredWeaponStyleBitmap, statusDest, textPaint);
#if GNH_MAP_PROFILING && DEBUG
                                                StopProfiling(GHProfilingStyle.Bitmap);
#endif
                                                curx += target_width;
                                                curx += innerspacing;

                                                //SKColor oldcolor = textPaint.Color;
                                                //textPaint.Color = SKColors.Cyan;
                                                //textPaint.TextSize = shieldfontsize;
                                                //string printtext = "+";
                                                //print_width = textPaint.MeasureText(printtext);
                                                //canvas.DrawText(printtext, curx, cury - textPaint.FontMetrics.Ascent, textPaint);
                                                //textPaint.Color = oldcolor;
                                                //curx += print_width;
                                                using (new SKAutoCanvasRestore(canvas, true))
                                                {
                                                    GlyphImageSource gis = _paintGlyphImageSource;
                                                    gis.ReferenceGamePage = this;
                                                    gis.UseUpperSide = false;
                                                    gis.AutoSize = true;
                                                    gis.Glyph = Math.Abs(_weaponStyleObjDataItem[2].ObjData.gui_glyph);
                                                    gis.ObjData = _weaponStyleObjDataItem[2];
                                                    gis.DoAutoSize();
                                                    float wep_scale = gis.Height == 0 ? 1.0f : target_height / gis.Height;
                                                    float weppicturewidth = wep_scale * gis.Width;
                                                    float weppictureheight = wep_scale * gis.Height;
                                                    canvas.Translate(curx + 0, cury + (target_height - weppictureheight) / 2);
                                                    canvas.Scale(wep_scale);
                                                    gis.DrawOnCanvas(canvas);
                                                    curx += weppicturewidth;
                                                    curx += innerspacing;
                                                }
                                            }
                                        }
                                    }
                                    textPaint.TextSize = savedfontsize;
                                    textPaint.Typeface = savedtypeface;
                                }
                                else
                                {
#if GNH_MAP_PROFILING && DEBUG
                                    StartProfiling(GHProfilingStyle.Text);
#endif
                                    if (valtext != "")
                                    {
                                        print_width = textPaint.MeasureText(valtext);
                                        canvas.DrawText(valtext, curx, cury - textPaint.FontMetrics.Ascent, textPaint);
                                        curx += print_width;
                                    }
                                    if (valtext2 != "")
                                    {
                                        string printtext = "/" + valtext2;
                                        print_width = textPaint.MeasureText(printtext);
                                        canvas.DrawText(printtext, curx, cury - textPaint.FontMetrics.Ascent, textPaint);
                                        curx += print_width;
                                    }
#if GNH_MAP_PROFILING && DEBUG
                                    StopProfiling(GHProfilingStyle.Text);
#endif
                                }
                                curx += stdspacing;
                            }

                            /* Right aligned */
                            bool turnsprinted = false;
                            float finalleftcurx = curx;
                            curx = canvaswidth - hmargin;
                            float turnsleft = curx;

                            /* Turns */
                            valtext = "";
                            lock (StatusFieldLock)
                            {
                                if (StatusFields[(int)statusfields.BL_TIME] != null && StatusFields[(int)statusfields.BL_TIME].IsEnabled && StatusFields[(int)statusfields.BL_TIME].Text != null)
                                {
                                    valtext = StatusFields[(int)statusfields.BL_TIME].Text;
                                }
                            }
                            if (valtext != "")
                            {
                                target_width = target_scale * GHApp._statusTurnsBitmap.Width;
                                target_height = target_scale * GHApp._statusTurnsBitmap.Height;
                                float print_width = textPaint.MeasureText(valtext);
                                float newcurx = canvaswidth - hmargin - print_width - innerspacing - target_width;
                                if (newcurx >= finalleftcurx) /* Avoid printing status bar elements over each other */
                                {
                                    turnsprinted = true;
                                    curx = newcurx;
                                    turnsleft = curx;
                                    statusDest = new SKRect(curx, cury, curx + target_width, cury + target_height);
#if GNH_MAP_PROFILING && DEBUG
                                StartProfiling(GHProfilingStyle.Bitmap);
#endif
                                    canvas.DrawBitmap(GHApp._statusTurnsBitmap, statusDest, textPaint);
#if GNH_MAP_PROFILING && DEBUG
                                StopProfiling(GHProfilingStyle.Bitmap);
#endif
                                    curx += target_width;
                                    curx += innerspacing;
#if GNH_MAP_PROFILING && DEBUG
                                StartProfiling(GHProfilingStyle.Text);
#endif
                                    canvas.DrawText(valtext, curx, cury - textPaint.FontMetrics.Ascent, textPaint);
#if GNH_MAP_PROFILING && DEBUG
                                StopProfiling(GHProfilingStyle.Text);
#endif
                                    curx += print_width;
                                }
                            }

                            /* Gold */
                            valtext = "";
                            lock (StatusFieldLock)
                            {
                                if (StatusFields[(int)statusfields.BL_GOLD] != null && StatusFields[(int)statusfields.BL_GOLD].IsEnabled && StatusFields[(int)statusfields.BL_GOLD].Text != null)
                                {
                                    valtext = StatusFields[(int)statusfields.BL_GOLD].Text;
                                }
                            }
                            if (valtext != "")
                            {
                                string printtext;
                                if (valtext.Length > 11 && valtext.Substring(0, 1) == "\\")
                                    printtext = valtext.Substring(11);
                                else
                                    printtext = valtext;

                                target_width = target_scale * GHApp._statusGoldBitmap.Width;
                                target_height = target_scale * GHApp._statusGoldBitmap.Height;
                                float print_width = textPaint.MeasureText(printtext);
                                float newcurx = turnsleft - (turnsprinted ? stdspacing : 0) - print_width - innerspacing - target_width;
                                if (newcurx >= finalleftcurx) /* Avoid printing status bar elements over each other */
                                {
                                    curx = newcurx;
                                    statusDest = new SKRect(curx, cury, curx + target_width, cury + target_height);
#if GNH_MAP_PROFILING && DEBUG
                                StartProfiling(GHProfilingStyle.Bitmap);
#endif
                                    canvas.DrawBitmap(GHApp._statusGoldBitmap, statusDest, textPaint);
#if GNH_MAP_PROFILING && DEBUG
                                StopProfiling(GHProfilingStyle.Bitmap);
#endif
                                    curx += target_width;
                                    curx += innerspacing;
#if GNH_MAP_PROFILING && DEBUG
                                StartProfiling(GHProfilingStyle.Text);
#endif
                                    canvas.DrawText(printtext, curx, cury - textPaint.FontMetrics.Ascent, textPaint);
#if GNH_MAP_PROFILING && DEBUG
                                StopProfiling(GHProfilingStyle.Text);
#endif
                                    curx += print_width;
                                }
                            }


                            /* Second row */
                            curx = hmargin;
                            cury += rowheight + rowmargin;

                            /* Title */
                            valtext = "";
                            lock (StatusFieldLock)
                            {
                                if (StatusFields[(int)statusfields.BL_TITLE] != null && StatusFields[(int)statusfields.BL_TITLE].IsEnabled && StatusFields[(int)statusfields.BL_TITLE].Text != null)
                                {
                                    valtext = StatusFields[(int)statusfields.BL_TITLE].Text;
                                }
                            }
                            valtext = valtext.Trim();
                            if (valtext != "")
                            {
#if GNH_MAP_PROFILING && DEBUG
                                StartProfiling(GHProfilingStyle.Text);
#endif
                                canvas.DrawText(valtext, curx, cury - textPaint.FontMetrics.Ascent, textPaint);
#if GNH_MAP_PROFILING && DEBUG
                                StopProfiling(GHProfilingStyle.Text);
#endif
                                float textprint_length = textPaint.MeasureText(valtext);
                                curx += textprint_length;
                                curx += stdspacing;
                            }

                            {
                                /* Condition, status and buff marks */
                                float marksize = rowheight * 0.80f;
                                float markpadding = marksize / 8;
                                ulong status_bits;
                                lock (_uLock)
                                {
                                    status_bits = _u_status_bits;
                                }
                                if (status_bits != 0)
                                {
                                    int tiles_per_row = GHConstants.TileWidth / GHConstants.StatusMarkWidth;
                                    int mglyph = (int)game_ui_tile_types.STATUS_MARKS + GHApp.UITileOff;
                                    int mtile = GHApp.Glyph2Tile[mglyph];
                                    int sheet_idx = GHApp.TileSheetIdx(mtile);
                                    int tile_x = GHApp.TileSheetX(mtile);
                                    int tile_y = GHApp.TileSheetY(mtile);
                                    foreach (int status_mark in _statusmarkorder)
                                    {
                                        ulong statusbit = 1UL << status_mark;
                                        if ((status_bits & statusbit) != 0)
                                        {
                                            int within_tile_x = status_mark % tiles_per_row;
                                            int within_tile_y = status_mark / tiles_per_row;
                                            int c_x = tile_x + within_tile_x * GHConstants.StatusMarkWidth;
                                            int c_y = tile_y + within_tile_y * GHConstants.StatusMarkHeight;

                                            SKRect source_rt = new SKRect();
                                            source_rt.Left = c_x;
                                            source_rt.Right = c_x + GHConstants.StatusMarkWidth;
                                            source_rt.Top = c_y;
                                            source_rt.Bottom = c_y + GHConstants.StatusMarkHeight;

                                            SKRect target_rt = new SKRect();
                                            target_rt.Left = curx;
                                            target_rt.Right = target_rt.Left + marksize;
                                            target_rt.Top = cury + (rowheight - marksize) / 2;
                                            target_rt.Bottom = target_rt.Top + marksize;
#if GNH_MAP_PROFILING && DEBUG
                                            StartProfiling(GHProfilingStyle.Bitmap);
#endif
                                            canvas.DrawBitmap(TileMap[sheet_idx], source_rt, target_rt);
#if GNH_MAP_PROFILING && DEBUG
                                            StopProfiling(GHProfilingStyle.Bitmap);
#endif

                                            curx += marksize;
                                            curx += markpadding;
                                        }
                                    }
                                }

                                ulong condition_bits;
                                lock (_uLock)
                                {
                                    condition_bits = _u_condition_bits;
                                }
                                if (condition_bits != 0)
                                {
                                    int tiles_per_row = GHConstants.TileWidth / GHConstants.StatusMarkWidth;
                                    int mglyph = (int)game_ui_tile_types.CONDITION_MARKS + GHApp.UITileOff;
                                    int mtile = GHApp.Glyph2Tile[mglyph];
                                    int sheet_idx = GHApp.TileSheetIdx(mtile);
                                    int tile_x = GHApp.TileSheetX(mtile);
                                    int tile_y = GHApp.TileSheetY(mtile);
                                    for (int condition_mark = 0; condition_mark < (int)bl_conditions.NUM_BL_CONDITIONS; condition_mark++)
                                    {
                                        ulong conditionbit = 1UL << condition_mark;
                                        if ((condition_bits & conditionbit) != 0)
                                        {
                                            int within_tile_x = condition_mark % tiles_per_row;
                                            int within_tile_y = condition_mark / tiles_per_row;
                                            int c_x = tile_x + within_tile_x * GHConstants.StatusMarkWidth;
                                            int c_y = tile_y + within_tile_y * GHConstants.StatusMarkHeight;

                                            SKRect source_rt = new SKRect();
                                            source_rt.Left = c_x;
                                            source_rt.Right = c_x + GHConstants.StatusMarkWidth;
                                            source_rt.Top = c_y;
                                            source_rt.Bottom = c_y + GHConstants.StatusMarkHeight;

                                            SKRect target_rt = new SKRect();
                                            target_rt.Left = curx;
                                            target_rt.Right = target_rt.Left + marksize;
                                            target_rt.Top = cury + (rowheight - marksize) / 2;
                                            target_rt.Bottom = target_rt.Top + marksize;
#if GNH_MAP_PROFILING && DEBUG
                                            StartProfiling(GHProfilingStyle.Bitmap);
#endif
                                            canvas.DrawBitmap(TileMap[sheet_idx], source_rt, target_rt);
#if GNH_MAP_PROFILING && DEBUG
                                            StopProfiling(GHProfilingStyle.Bitmap);
#endif

                                            curx += marksize;
                                            curx += markpadding;
                                        }
                                    }
                                }

                                ulong buff_bits;
                                for (int buff_ulong = 0; buff_ulong < GHConstants.NUM_BUFF_BIT_ULONGS; buff_ulong++)
                                {
                                    lock (_uLock)
                                    {
                                        buff_bits = _u_buff_bits[buff_ulong];
                                    }
                                    int tiles_per_row = GHConstants.TileWidth / GHConstants.StatusMarkWidth;
                                    if (buff_bits != 0)
                                    {
                                        for (int buff_idx = 0; buff_idx < 32; buff_idx++)
                                        {
                                            ulong buffbit = 1UL << buff_idx;
                                            if ((buff_bits & buffbit) != 0)
                                            {
                                                int propidx = buff_ulong * 32 + buff_idx;
                                                if (propidx > GHConstants.LAST_PROP)
                                                    break;
                                                int mglyph = (propidx - 1) / GHConstants.BUFFS_PER_TILE + GHApp.BuffTileOff;
                                                int mtile = GHApp.Glyph2Tile[mglyph];
                                                int sheet_idx = GHApp.TileSheetIdx(mtile);
                                                int tile_x = GHApp.TileSheetX(mtile);
                                                int tile_y = GHApp.TileSheetY(mtile);

                                                int buff_mark = (propidx - 1) % GHConstants.BUFFS_PER_TILE;
                                                int within_tile_x = buff_mark % tiles_per_row;
                                                int within_tile_y = buff_mark / tiles_per_row;
                                                int c_x = tile_x + within_tile_x * GHConstants.StatusMarkWidth;
                                                int c_y = tile_y + within_tile_y * GHConstants.StatusMarkHeight;

                                                SKRect source_rt = new SKRect();
                                                source_rt.Left = c_x;
                                                source_rt.Right = c_x + GHConstants.StatusMarkWidth;
                                                source_rt.Top = c_y;
                                                source_rt.Bottom = c_y + GHConstants.StatusMarkHeight;

                                                SKRect target_rt = new SKRect();
                                                target_rt.Left = curx;
                                                target_rt.Right = target_rt.Left + marksize;
                                                target_rt.Top = cury + (rowheight - marksize) / 2;
                                                target_rt.Bottom = target_rt.Top + marksize;
#if GNH_MAP_PROFILING && DEBUG
                                                StartProfiling(GHProfilingStyle.Bitmap);
#endif
                                                canvas.DrawBitmap(TileMap[sheet_idx], source_rt, target_rt);
#if GNH_MAP_PROFILING && DEBUG
                                                StopProfiling(GHProfilingStyle.Bitmap);
#endif

                                                curx += marksize;
                                                curx += markpadding;
                                            }
                                        }
                                    }
                                }

                                bool colorfound = false;
                                for (int i = (int)nhcolor.CLR_BLACK + 1; i < (int)nhcolor.CLR_WHITE; i++)
                                {
                                    if (i == (int)nhcolor.NO_COLOR || i == (int)nhcolor.CLR_GRAY)
                                        continue;

                                    colorfound = false;
                                    for (int j = 0; j < 6; j++)
                                    {
                                        lock (StatusFieldLock)
                                        {
                                            if (StatusFields[(int)statusfields.BL_STR + j] != null && StatusFields[(int)statusfields.BL_STR + j].IsEnabled && StatusFields[(int)statusfields.BL_STR + j].Text != null)
                                            {
                                                if (StatusFields[(int)statusfields.BL_STR + j].Color == i)
                                                {
                                                    colorfound = true;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    if (colorfound)
                                    {
                                        SKColor dotcolor = UIUtils.NHColor2SKColorCore(i, 0, true, false);
                                        SKPoint dotpoint = new SKPoint(curx + marksize / 4, cury + (rowheight - marksize) / 2 + marksize / 2);
                                        float dotradius = marksize / 8;
                                        textPaint.Color = dotcolor;
                                        textPaint.Style = SKPaintStyle.Fill;
                                        canvas.DrawCircle(dotpoint, dotradius, textPaint);
                                        curx += marksize / 2;
                                        curx += markpadding;
                                    }
                                }
                                textPaint.Color = SKColors.White;
                            }

                            /* Dungeon level */
                            valtext = "";
                            lock (StatusFieldLock)
                            {
                                if (StatusFields[(int)statusfields.BL_LEVELDESC] != null && StatusFields[(int)statusfields.BL_LEVELDESC].IsEnabled && StatusFields[(int)statusfields.BL_LEVELDESC].Text != null)
                                {
                                    valtext = StatusFields[(int)statusfields.BL_LEVELDESC].Text;
                                }
                            }
                            if (valtext != "")
                            {
                                string printtext;
                                if (valtext.Length > 3 && valtext.Substring(0, 3) == "DL:")
                                    printtext = valtext.Substring(3);
                                else if (valtext.Length > 5 && valtext.Substring(0, 5) == "Dlvl:")
                                    printtext = valtext.Substring(5);
                                else
                                    printtext = valtext;

                                target_width = target_scale * GHApp._statusDungeonLevelBitmap.Width;
                                target_height = target_scale * GHApp._statusDungeonLevelBitmap.Height;
                                float print_width = textPaint.MeasureText(printtext);
                                curx = canvaswidth - hmargin - print_width - innerspacing - target_width;
                                statusDest = new SKRect(curx, cury, curx + target_width, cury + target_height);
#if GNH_MAP_PROFILING && DEBUG
                                StartProfiling(GHProfilingStyle.Bitmap);
#endif
                                canvas.DrawBitmap(GHApp._statusDungeonLevelBitmap, statusDest, textPaint);
#if GNH_MAP_PROFILING && DEBUG
                                StopProfiling(GHProfilingStyle.Bitmap);
#endif
                                curx += target_width;
                                curx += innerspacing;
#if GNH_MAP_PROFILING && DEBUG
                                StartProfiling(GHProfilingStyle.Text);
#endif
                                canvas.DrawText(printtext, curx, cury - textPaint.FontMetrics.Ascent, textPaint);
#if GNH_MAP_PROFILING && DEBUG
                                StopProfiling(GHProfilingStyle.Text);
#endif
                                curx += print_width;
                            }

                            /* Pets */
                            if (ShowPets)
                            {
                                lock (_petDataLock)
                                {
                                    textPaint.Color = SKColors.White;
                                    textPaint.Typeface = GHApp.LatoRegular;
                                    textPaint.TextSize = 36;
                                    float pet_target_height = inverse_canvas_scale * (float)(StandardMeasurementButton.Height + lAbilitiesButton.Width) / 2;
                                    //float pet_name_target_height = pet_target_height * 0.4f;
                                    float pet_picture_target_height = pet_target_height * 0.56f;
                                    float pet_hp_target_height = pet_target_height * 0.24f;
                                    float pet_status_target_height = pet_target_height * 0.2f;
                                    //float pet_name_size = textPaint.TextSize * pet_name_target_height / textPaint.FontSpacing;
                                    float pet_hp_size = textPaint.TextSize * pet_hp_target_height / textPaint.FontSpacing; //pet_name_size * pet_hp_target_height / pet_name_target_height;
                                                                                                                            //textPaint.TextSize = pet_name_size;
                                                                                                                            //string pet_test_text = "Large Dog";
                                                                                                                            //float pet_target_width = textPaint.MeasureText(pet_test_text);
                                                                                                                            //pet_target_width += textPaint.FontSpacing; // For picture
                                    float pet_target_width = pet_target_height; // inverse_canvas_scale * (float)StandardMeasurementButton.Width;

                                    SKRect menubuttonrect = GetViewScreenRect(UseSimpleCmdLayout ? SimpleGameMenuButton : GameMenuButton);
                                    SKRect canvasrect = GetViewScreenRect(canvasView);
                                    SKRect adjustedrect = new SKRect(menubuttonrect.Left - canvasrect.Left, menubuttonrect.Top - canvasrect.Top, menubuttonrect.Right - canvasrect.Left, menubuttonrect.Bottom - canvasrect.Top);
                                    float menu_button_left = adjustedrect.Left;
                                    float pet_tx_start = orbleft + orbbordersize * 1.1f;
                                    tx = pet_tx_start;
                                    ty = statusbarheight + 5.0f;
                                    int petrownum = 0;

                                    foreach (GHPetDataItem pdi in _petData)
                                    {
                                        monst_info mi = pdi.Data;
                                        using (new SKAutoCanvasRestore(canvas, true))
                                        {
                                            canvas.ClipRect(new SKRect(tx - 1, ty - 1, tx + pet_target_width + 1, ty + pet_target_height + 2));
                                            pdi.Rect = new SKRect(tx, ty, tx + pet_target_width, ty + pet_target_height);

                                            float petpicturewidth = 0f;
                                            float petpictureheight = 0f;
                                            using (new SKAutoCanvasRestore(canvas, true))
                                            {
                                                GlyphImageSource gis = new GlyphImageSource();
                                                gis.ReferenceGamePage = this;
                                                gis.UseUpperSide = false; /* Monsters are generally full-sized */
                                                gis.AutoSize = true;
                                                gis.Glyph = Math.Abs(mi.gui_glyph);
                                                gis.DoAutoSize();
                                                float pet_scale = Math.Min(gis.Width == 0 ? 1.0f : pet_target_width / gis.Width, gis.Height == 0 ? 1.0f : pet_picture_target_height / gis.Height);
                                                petpicturewidth = pet_scale * gis.Width;
                                                petpictureheight = pet_scale * gis.Height;
                                                canvas.Translate(tx + (pet_target_width - petpicturewidth) / 2, ty + (pet_picture_target_height - petpictureheight));
                                                canvas.Scale(pet_scale);
#if GNH_MAP_PROFILING && DEBUG
                                                StartProfiling(GHProfilingStyle.Bitmap);
#endif
                                                gis.DrawOnCanvas(canvas);
#if GNH_MAP_PROFILING && DEBUG
                                                StopProfiling(GHProfilingStyle.Bitmap);
#endif
                                            }

                                            float curpety = ty + pet_picture_target_height;
                                            textPaint.TextSize = pet_hp_size;
                                            textPaint.TextAlign = SKTextAlign.Center;
                                            float petHPHeight = textPaint.FontSpacing;
                                            float barpadding = (textPaint.FontSpacing - (textPaint.FontMetrics.Descent - textPaint.FontMetrics.Ascent)) / 2;
                                            SKRect petHPRect = new SKRect(tx, curpety, tx + pet_target_width, curpety + petHPHeight);
                                            float petpct = mi.mhpmax <= 0 ? 0.0f : (float)mi.mhp / (float)mi.mhpmax;
                                            SKRect petHPFill = new SKRect(tx, curpety, tx + pet_target_width * petpct, curpety + petHPHeight);
                                            textPaint.Color = SKColors.Red.WithAlpha(144);
#if GNH_MAP_PROFILING && DEBUG
                                            StartProfiling(GHProfilingStyle.Rect);
#endif
                                            canvas.DrawRect(petHPFill, textPaint);
                                            SKRect petHPNonFill = new SKRect(tx + pet_target_width * petpct, curpety, tx + pet_target_width, curpety + petHPHeight);
                                            textPaint.Color = SKColors.Gray.WithAlpha(144);
                                            canvas.DrawRect(petHPNonFill, textPaint);
                                            textPaint.Color = SKColors.Black.WithAlpha(144);
                                            textPaint.Style = SKPaintStyle.Stroke;
                                            textPaint.StrokeWidth = 2;
                                            canvas.DrawRect(petHPRect, textPaint);
#if GNH_MAP_PROFILING && DEBUG
                                            StopProfiling(GHProfilingStyle.Rect);
#endif

                                            curpety += barpadding - textPaint.FontMetrics.Ascent;
                                            textPaint.Style = SKPaintStyle.Fill;
                                            textPaint.Color = SKColors.White;
#if GNH_MAP_PROFILING && DEBUG
                                            StartProfiling(GHProfilingStyle.Text);
#endif
                                            canvas.DrawText(mi.mhp + "(" + mi.mhpmax + ")", tx + pet_target_width / 2, curpety, textPaint);
#if GNH_MAP_PROFILING && DEBUG
                                            StopProfiling(GHProfilingStyle.Text);
#endif

                                            {
                                                /* Condition, status and buff marks */
                                                curx = tx;
                                                cury = petHPRect.Bottom;
                                                rowheight = pet_status_target_height;

                                                float marksize = rowheight * 0.95f;
                                                float markpadding = marksize / 8;
                                                ulong status_bits;
                                                status_bits = mi.status_bits;
                                                if (status_bits != 0)
                                                {
                                                    int tiles_per_row = GHConstants.TileWidth / GHConstants.StatusMarkWidth;
                                                    int mglyph = (int)game_ui_tile_types.STATUS_MARKS + GHApp.UITileOff;
                                                    int mtile = GHApp.Glyph2Tile[mglyph];
                                                    int sheet_idx = GHApp.TileSheetIdx(mtile);
                                                    int tile_x = GHApp.TileSheetX(mtile);
                                                    int tile_y = GHApp.TileSheetY(mtile);
                                                    foreach (int status_mark in _statusmarkorder)
                                                    {
                                                        ulong statusbit = 1UL << status_mark;
                                                        if ((status_bits & statusbit) != 0)
                                                        {
                                                            int within_tile_x = status_mark % tiles_per_row;
                                                            int within_tile_y = status_mark / tiles_per_row;
                                                            int c_x = tile_x + within_tile_x * GHConstants.StatusMarkWidth;
                                                            int c_y = tile_y + within_tile_y * GHConstants.StatusMarkHeight;

                                                            SKRect source_rt = new SKRect();
                                                            source_rt.Left = c_x;
                                                            source_rt.Right = c_x + GHConstants.StatusMarkWidth;
                                                            source_rt.Top = c_y;
                                                            source_rt.Bottom = c_y + GHConstants.StatusMarkHeight;

                                                            SKRect target_rt = new SKRect();
                                                            target_rt.Left = curx;
                                                            target_rt.Right = target_rt.Left + marksize;
                                                            target_rt.Top = cury + (rowheight - marksize) / 2;
                                                            target_rt.Bottom = target_rt.Top + marksize;
#if GNH_MAP_PROFILING && DEBUG
                                                            StartProfiling(GHProfilingStyle.Bitmap);
#endif
                                                            canvas.DrawBitmap(TileMap[sheet_idx], source_rt, target_rt);
#if GNH_MAP_PROFILING && DEBUG
                                                            StopProfiling(GHProfilingStyle.Bitmap);
#endif

                                                            curx += marksize;
                                                            curx += markpadding;
                                                        }
                                                    }
                                                }

                                                ulong condition_bits;
                                                condition_bits = mi.condition_bits;
                                                if (condition_bits != 0)
                                                {
                                                    int tiles_per_row = GHConstants.TileWidth / GHConstants.StatusMarkWidth;
                                                    int mglyph = (int)game_ui_tile_types.CONDITION_MARKS + GHApp.UITileOff;
                                                    int mtile = GHApp.Glyph2Tile[mglyph];
                                                    int sheet_idx = GHApp.TileSheetIdx(mtile);
                                                    int tile_x = GHApp.TileSheetX(mtile);
                                                    int tile_y = GHApp.TileSheetY(mtile);
                                                    for (int condition_mark = 0; condition_mark < (int)bl_conditions.NUM_BL_CONDITIONS; condition_mark++)
                                                    {
                                                        ulong conditionbit = 1UL << condition_mark;
                                                        if ((condition_bits & conditionbit) != 0)
                                                        {
                                                            int within_tile_x = condition_mark % tiles_per_row;
                                                            int within_tile_y = condition_mark / tiles_per_row;
                                                            int c_x = tile_x + within_tile_x * GHConstants.StatusMarkWidth;
                                                            int c_y = tile_y + within_tile_y * GHConstants.StatusMarkHeight;

                                                            SKRect source_rt = new SKRect();
                                                            source_rt.Left = c_x;
                                                            source_rt.Right = c_x + GHConstants.StatusMarkWidth;
                                                            source_rt.Top = c_y;
                                                            source_rt.Bottom = c_y + GHConstants.StatusMarkHeight;

                                                            SKRect target_rt = new SKRect();
                                                            target_rt.Left = curx;
                                                            target_rt.Right = target_rt.Left + marksize;
                                                            target_rt.Top = cury + (rowheight - marksize) / 2;
                                                            target_rt.Bottom = target_rt.Top + marksize;
#if GNH_MAP_PROFILING && DEBUG
                                                            StartProfiling(GHProfilingStyle.Bitmap);
#endif
                                                            canvas.DrawBitmap(TileMap[sheet_idx], source_rt, target_rt);
#if GNH_MAP_PROFILING && DEBUG
                                                            StopProfiling(GHProfilingStyle.Bitmap);
#endif

                                                            curx += marksize;
                                                            curx += markpadding;
                                                        }
                                                    }
                                                }

                                                ulong buff_bits;
                                                for (int buff_ulong = 0; buff_ulong < GHConstants.NUM_BUFF_BIT_ULONGS; buff_ulong++)
                                                {
                                                    buff_bits = mi.buff_bits[buff_ulong];
                                                    int tiles_per_row = GHConstants.TileWidth / GHConstants.StatusMarkWidth;
                                                    if (buff_bits != 0)
                                                    {
                                                        for (int buff_idx = 0; buff_idx < 32; buff_idx++)
                                                        {
                                                            ulong buffbit = 1UL << buff_idx;
                                                            if ((buff_bits & buffbit) != 0)
                                                            {
                                                                int propidx = buff_ulong * 32 + buff_idx;
                                                                if (propidx > GHConstants.LAST_PROP)
                                                                    break;
                                                                int mglyph = (propidx - 1) / GHConstants.BUFFS_PER_TILE + GHApp.BuffTileOff;
                                                                int mtile = GHApp.Glyph2Tile[mglyph];
                                                                int sheet_idx = GHApp.TileSheetIdx(mtile);
                                                                int tile_x = GHApp.TileSheetX(mtile);
                                                                int tile_y = GHApp.TileSheetY(mtile);

                                                                int buff_mark = (propidx - 1) % GHConstants.BUFFS_PER_TILE;
                                                                int within_tile_x = buff_mark % tiles_per_row;
                                                                int within_tile_y = buff_mark / tiles_per_row;
                                                                int c_x = tile_x + within_tile_x * GHConstants.StatusMarkWidth;
                                                                int c_y = tile_y + within_tile_y * GHConstants.StatusMarkHeight;

                                                                SKRect source_rt = new SKRect();
                                                                source_rt.Left = c_x;
                                                                source_rt.Right = c_x + GHConstants.StatusMarkWidth;
                                                                source_rt.Top = c_y;
                                                                source_rt.Bottom = c_y + GHConstants.StatusMarkHeight;

                                                                SKRect target_rt = new SKRect();
                                                                target_rt.Left = curx;
                                                                target_rt.Right = target_rt.Left + marksize;
                                                                target_rt.Top = cury + (rowheight - marksize) / 2;
                                                                target_rt.Bottom = target_rt.Top + marksize;
#if GNH_MAP_PROFILING && DEBUG
                                                                StartProfiling(GHProfilingStyle.Bitmap);
#endif
                                                                canvas.DrawBitmap(TileMap[sheet_idx], source_rt, target_rt);
#if GNH_MAP_PROFILING && DEBUG
                                                                StopProfiling(GHProfilingStyle.Bitmap);
#endif

                                                                curx += marksize;
                                                                curx += markpadding;
                                                            }
                                                        }
                                                    }
                                                }
                                            }

                                            /* Next pet */
                                            tx += pet_target_width;
                                            if (tx + pet_target_width * 1.08f > menu_button_left)
                                            {
                                                tx = pet_tx_start;
                                                ty += pet_target_height + textPaint.FontSpacing / 2;
                                                petrownum++;
                                                if (petrownum >= NumDisplayedPetRows)
                                                    break;
                                            }
                                            else
                                                tx += pet_target_width * 0.08f;
                                        }

                                        textPaint.TextAlign = SKTextAlign.Left;
                                    }
                                }
                            }
                            else
                            {
                                lock (_petDataLock)
                                {
                                    foreach (GHPetDataItem pdi in _petData)
                                    {
                                        pdi.Rect = new SKRect();
                                    }
                                }
                            }
                        }

                        bool orbsok = false;
                        bool skillbuttonok = false;
                        lock (StatusFieldLock)
                        {
                            orbsok = StatusFields[(int)statusfields.BL_HPMAX] != null && StatusFields[(int)statusfields.BL_HPMAX].Text != "" && StatusFields[(int)statusfields.BL_HPMAX].Text != "0";
                            skillbuttonok = StatusFields[(int)statusfields.BL_SKILL] != null && StatusFields[(int)statusfields.BL_SKILL].Text != null && StatusFields[(int)statusfields.BL_SKILL].Text == "Skill";
                        }

                        float lastdrawnrecty = ClassicStatusBar ? Math.Max(abilitybuttonbottom, lastStatusRowPrintY + 0.0f * lastStatusRowFontSpacing) : statusbarheight;
                        tx = orbleft;
                        ty = lastdrawnrecty + 5.0f;

                        /* HP and MP */
                        if ((ShowOrbs | !ClassicStatusBar) && orbsok)
                        {
                            float orbfillpercentage = 0.0f;
                            string valtext = "";
                            string maxtext = "";
                            lock (StatusFieldLock)
                            {
                                bool pctset = false;
                                if (StatusFields[(int)statusfields.BL_HP] != null && StatusFields[(int)statusfields.BL_HP].Text != null && StatusFields[(int)statusfields.BL_HP].Text != "" && StatusFields[(int)statusfields.BL_HPMAX] != null && StatusFields[(int)statusfields.BL_HPMAX].Text != null && StatusFields[(int)statusfields.BL_HPMAX].Text != "")
                                {
                                    valtext = StatusFields[(int)statusfields.BL_HP].Text;
                                    maxtext = StatusFields[(int)statusfields.BL_HPMAX].Text;
                                    int hp = 0, hpmax = 1;
                                    if (int.TryParse(StatusFields[(int)statusfields.BL_HP].Text, out hp) && int.TryParse(StatusFields[(int)statusfields.BL_HPMAX].Text, out hpmax))
                                    {
                                        if (hpmax > 0)
                                        {
                                            orbfillpercentage = (float)hp / (float)hpmax;
                                            pctset = true;
                                        }
                                    }
                                    if (!pctset)
                                        orbfillpercentage = ((float)StatusFields[(int)statusfields.BL_HP].Percent) / 100.0f;
                                }
                            }
                            SKRect orbBorderDest = new SKRect(tx, ty, tx + orbbordersize, ty + orbbordersize);
                            HealthRect = orbBorderDest;
                            _healthRectDrawn = true;
                            DrawOrb(canvas, textPaint, orbBorderDest, SKColors.Red, valtext, maxtext, orbfillpercentage, ShowMaxHealthInOrb);

                            orbfillpercentage = 0.0f;
                            valtext = "";
                            maxtext = "";
                            lock (StatusFieldLock)
                            {
                                if (StatusFields[(int)statusfields.BL_ENE] != null && StatusFields[(int)statusfields.BL_ENE].Text != null && StatusFields[(int)statusfields.BL_ENEMAX] != null && StatusFields[(int)statusfields.BL_ENE].Text != "" && StatusFields[(int)statusfields.BL_ENEMAX].Text != null && StatusFields[(int)statusfields.BL_ENEMAX].Text != "")
                                {
                                    valtext = StatusFields[(int)statusfields.BL_ENE].Text;
                                    maxtext = StatusFields[(int)statusfields.BL_ENEMAX].Text;
                                    int en = 0, enmax = 1;
                                    if (int.TryParse(StatusFields[(int)statusfields.BL_ENE].Text, out en) && int.TryParse(StatusFields[(int)statusfields.BL_ENEMAX].Text, out enmax))
                                    {
                                        if (enmax > 0)
                                        {
                                            orbfillpercentage = (float)en / (float)enmax;
                                        }
                                    }
                                }
                            }
                            orbBorderDest = new SKRect(tx, ty + orbbordersize + 5, tx + orbbordersize, ty + orbbordersize + 5 + orbbordersize);
                            ManaRect = orbBorderDest;
                            _manaRectDrawn = true;
                            DrawOrb(canvas, textPaint, orbBorderDest, SKColors.Blue, valtext, maxtext, orbfillpercentage, ShowMaxManaInOrb);
                            lastdrawnrecty = orbBorderDest.Bottom;
                        }

                        if (skillbuttonok)
                        {
                            SKRect skillDest = new SKRect(tx, lastdrawnrecty + 15.0f, tx + orbbordersize, lastdrawnrecty + 15.0f + orbbordersize);
                            SkillRect = skillDest;
                            _skillRectDrawn = true;
                            textPaint.Color = SKColors.White;
                            textPaint.Typeface = GHApp.LatoRegular;
                            textPaint.TextSize = 9.5f * skillDest.Width / 50.0f;
                            textPaint.TextAlign = SKTextAlign.Center;
#if GNH_MAP_PROFILING && DEBUG
                            StartProfiling(GHProfilingStyle.Bitmap);
#endif
                            canvas.DrawBitmap(GHApp._skillBitmap, skillDest, textPaint);
#if GNH_MAP_PROFILING && DEBUG
                            StopProfiling(GHProfilingStyle.Bitmap);
#endif
                            float text_x = (skillDest.Left + skillDest.Right) / 2;
                            float text_y = skillDest.Bottom - textPaint.FontMetrics.Ascent;
#if GNH_MAP_PROFILING && DEBUG
                            StartProfiling(GHProfilingStyle.Text);
#endif
                            canvas.DrawText("Skills", text_x, text_y, textPaint);
#if GNH_MAP_PROFILING && DEBUG
                            StopProfiling(GHProfilingStyle.Text);
#endif
                            textPaint.TextAlign = SKTextAlign.Left;
                        }
                    }
                    
                    if(!_statusBarRectDrawn)
                        StatusBarRect = new SKRect();
                    if (!_healthRectDrawn)
                        HealthRect = new SKRect();
                    if (!_manaRectDrawn)
                        ManaRect = new SKRect();
                    if (!_skillRectDrawn)
                        SkillRect = new SKRect();

                    /* Number Pad and Direction Arrows */
                    _canvasButtonRect.Right = canvaswidth * (float)(0.8);
                    _canvasButtonRect.Left = canvaswidth * (float)(0.2);
                }

                if (_showDirections || (MapWalkMode && WalkArrows))
                {
                    SKRect targetrect;
                    float buttonsize = _showDirections ? GHConstants.ArrowButtonSize : GHConstants.MoveArrowButtonSize;
                    SKColor oldcolor = textPaint.Color;
                    textPaint.Color = _showDirections ? textPaint.Color.WithAlpha(170) : textPaint.Color.WithAlpha(85);


                    for (int i = 0; i < 9; i++)
                    {
                        lock (_canvasButtonLock)
                        {
                            switch (i)
                            {
                                case 0:
                                    tx = _canvasButtonRect.Left;
                                    ty = _canvasButtonRect.Top + _canvasButtonRect.Height / 2 - _canvasButtonRect.Height * (buttonsize / 2);
                                    break;
                                case 1:
                                    tx = _canvasButtonRect.Left + _canvasButtonRect.Width / 2 - _canvasButtonRect.Width * (buttonsize / 2);
                                    ty = _canvasButtonRect.Top;
                                    break;
                                case 2:
                                    tx = _canvasButtonRect.Left + _canvasButtonRect.Width - _canvasButtonRect.Width * buttonsize;
                                    ty = _canvasButtonRect.Top + _canvasButtonRect.Height / 2 - _canvasButtonRect.Height * (buttonsize / 2);
                                    break;
                                case 3:
                                    tx = _canvasButtonRect.Left + _canvasButtonRect.Width / 2 - _canvasButtonRect.Width * (buttonsize / 2);
                                    ty = _canvasButtonRect.Top + _canvasButtonRect.Height - _canvasButtonRect.Height * buttonsize;
                                    break;
                                case 4:
                                    tx = _canvasButtonRect.Left;
                                    ty = _canvasButtonRect.Top;
                                    break;
                                case 5:
                                    continue;
                                case 6:
                                    tx = _canvasButtonRect.Left + _canvasButtonRect.Width * (1.0f - buttonsize);
                                    ty = _canvasButtonRect.Top;
                                    break;
                                case 7:
                                    tx = _canvasButtonRect.Left + _canvasButtonRect.Width * (1.0f - buttonsize);
                                    ty = _canvasButtonRect.Top + _canvasButtonRect.Height * (1.0f - buttonsize);
                                    break;
                                case 8:
                                    tx = _canvasButtonRect.Left;
                                    ty = _canvasButtonRect.Top + _canvasButtonRect.Height * (1.0f - buttonsize);
                                    break;
                                default:
                                    continue;
                            }

                            float px = Math.Max(0, _canvasButtonRect.Width - _canvasButtonRect.Height) * buttonsize / 2;
                            float py = Math.Max(0, _canvasButtonRect.Height - _canvasButtonRect.Width) * buttonsize / 2;
                            float truesize = Math.Min(_canvasButtonRect.Width, _canvasButtonRect.Height) * buttonsize;
                            targetrect = new SKRect(tx + px, ty + py, tx + px + truesize, ty + py + truesize);
                        }
                        canvas.DrawBitmap(GHApp._arrowBitmap[i], targetrect, textPaint);
                    }
                    textPaint.Color = oldcolor;
                }
                else if (ShowNumberPad)
                {
                    for (int j = 0; j <= 2; j++)
                    {
                        float buttonsize = GHConstants.NumberButtonSize;
                        float offset = 0;
                        SKColor oldcolor = textPaint.Color;
                        SKMaskFilter oldfilter = textPaint.MaskFilter;
                        textPaint.Typeface = GHApp.DiabloTypeface;
                        textPaint.TextSize = 225;
                        if (j == 0)
                        {
                            textPaint.Color = SKColors.Black.WithAlpha(oldcolor.Alpha);
                            textPaint.MaskFilter = _blur;
                            offset = textPaint.TextSize / 15;
                            textPaint.Style = SKPaintStyle.Fill;
                        }
                        else if (j == 1)
                        {
                            textPaint.Color = new SKColor(255, 255, 0xD7, 255);
                            textPaint.Style = SKPaintStyle.Fill;
                        }
                        else
                        {
                            textPaint.Color = new SKColor(80, 80, 50, 255);
                            textPaint.Style = SKPaintStyle.Stroke;
                            textPaint.StrokeWidth = textPaint.TextSize / 20;
                        }
                        float avgwidth = textPaint.MeasureText("A");
                        for (int i = 0; i <= 9; i++)
                        {
                            lock (_canvasButtonLock)
                            {
                                switch (i)
                                {
                                    case 0:
                                        str = "4";
                                        tx = _canvasButtonRect.Left + _canvasButtonRect.Width * (buttonsize / 2) - avgwidth / 2;
                                        ty = _canvasButtonRect.Top + _canvasButtonRect.Height / 2 + textPaint.FontMetrics.Descent;
                                        break;
                                    case 1:
                                        str = "8";
                                        tx = _canvasButtonRect.Left + _canvasButtonRect.Width / 2 - avgwidth / 2;
                                        ty = _canvasButtonRect.Top + _canvasButtonRect.Height * (buttonsize / 2) + textPaint.FontMetrics.Descent;
                                        break;
                                    case 2:
                                        str = "6";
                                        tx = _canvasButtonRect.Left + _canvasButtonRect.Width * (1.0f - buttonsize / 2) - avgwidth / 2;
                                        ty = _canvasButtonRect.Top + _canvasButtonRect.Height / 2 + textPaint.FontMetrics.Descent;
                                        break;
                                    case 3:
                                        str = "2";
                                        tx = _canvasButtonRect.Left + _canvasButtonRect.Width / 2 - avgwidth / 2;
                                        ty = _canvasButtonRect.Top + _canvasButtonRect.Height * (1.0f - buttonsize / 2) + textPaint.FontMetrics.Descent;
                                        break;
                                    case 4:
                                        str = "7";
                                        tx = _canvasButtonRect.Left + _canvasButtonRect.Width * (buttonsize / 2) - avgwidth / 2;
                                        ty = _canvasButtonRect.Top + _canvasButtonRect.Height * (buttonsize / 2) + textPaint.FontMetrics.Descent;
                                        break;
                                    case 5:
                                        str = "5";
                                        tx = _canvasButtonRect.Left + _canvasButtonRect.Width / 2 - avgwidth / 2;
                                        ty = _canvasButtonRect.Top + _canvasButtonRect.Height / 2 + textPaint.FontMetrics.Descent;
                                        break;
                                    case 6:
                                        str = "9";
                                        tx = _canvasButtonRect.Left + _canvasButtonRect.Width * (1.0f - buttonsize / 2) - avgwidth / 2;
                                        ty = _canvasButtonRect.Top + _canvasButtonRect.Height * (buttonsize / 2) + textPaint.FontMetrics.Descent;
                                        break;
                                    case 7:
                                        str = "3";
                                        tx = _canvasButtonRect.Left + _canvasButtonRect.Width * (1.0f - buttonsize / 2) - avgwidth / 2;
                                        ty = _canvasButtonRect.Top + _canvasButtonRect.Height * (1.0f - buttonsize / 2) + textPaint.FontMetrics.Descent;
                                        break;
                                    case 8:
                                        str = "1";
                                        tx = _canvasButtonRect.Left + _canvasButtonRect.Width * (buttonsize / 2) - avgwidth / 2;
                                        ty = _canvasButtonRect.Top + _canvasButtonRect.Height * (1.0f - buttonsize / 2) + textPaint.FontMetrics.Descent;
                                        break;
                                    case 9:
                                        str = "0";
                                        tx = 0 + _canvasButtonRect.Left / 2 - avgwidth / 2;
                                        //ty = _canvasButtonRect.Top + _canvasButtonRect.Height * (buttonsize / 2) + textPaint.FontMetrics.Descent;
                                        ty = _canvasButtonRect.Top + _canvasButtonRect.Height * (1.0f - buttonsize / 2) + textPaint.FontMetrics.Descent;
                                        textPaint.TextSize = Math.Max(10.0f, textPaint.TextSize * Math.Min(1.0f, _canvasButtonRect.Left / (_canvasButtonRect.Width * buttonsize)));
                                        break;
                                }
                            }
                            canvas.DrawText(str, tx + offset, ty + offset, textPaint);
                        }
                        textPaint.Color = oldcolor;
                        textPaint.MaskFilter = oldfilter;
                    }
                    textPaint.Style = SKPaintStyle.Fill;
                }

                _youRectDrawn = false;
                /* Status Screen */
                if (ShowExtendedStatusBar)
                {
                    textPaint.Style = SKPaintStyle.Fill;
                    textPaint.Color = SKColors.Black.WithAlpha(200);
                    canvas.DrawRect(0, 0, canvaswidth, canvasheight, textPaint);
                    textPaint.Color = SKColors.White;

                    float box_left = canvaswidth < canvasheight ? 1.25f * inverse_canvas_scale * (float)StandardMeasurementButton.Width :
                        3.25f * inverse_canvas_scale * (float)StandardMeasurementButton.Width;
                    float box_right = canvaswidth - box_left;
                    if (box_right < box_left)
                        box_right = box_left;
                    float box_top = canvaswidth < canvasheight ? GetStatusBarSkiaHeight() + 1.25f * inverse_canvas_scale * (float)StandardMeasurementButton.Height :
                        GetStatusBarSkiaHeight() + 0.25f * inverse_canvas_scale * (float)StandardMeasurementButton.Height;
                    float box_bottom = canvasheight - 1.25f * inverse_canvas_scale * (float)UsedButtonRowStack.Height;
                    if (box_bottom < box_top)
                        box_bottom = box_top;

                    /* Window Background */
                    textPaint.Color = SKColors.Black;
                    SKRect bkgrect = new SKRect(box_left, box_top, box_right, box_bottom);
                    canvas.DrawBitmap(GHApp.ScrollBitmap, bkgrect, textPaint);

                    float youmargin = Math.Min((box_right - box_left), (box_bottom - box_top)) / 14;
                    float yousize = Math.Min((box_right - box_left), (box_bottom - box_top)) / 8;
                    float youtouchsize = Math.Min((box_right - box_left), (box_bottom - box_top)) / 6;
                    float youtouchmargin = Math.Max(0, (youtouchsize - yousize) / 2);
                    SKRect urect = new SKRect(box_right - youmargin - yousize, box_top + youmargin, box_right - youmargin, box_top + youmargin + yousize);
                    SKRect utouchrect = new SKRect(urect.Left - youtouchmargin, urect.Top - youtouchmargin, urect.Right + youtouchmargin, urect.Bottom + youtouchmargin);
                    canvas.DrawBitmap(GHApp.YouBitmap, urect, textPaint);
                    YouRect = utouchrect;
                    _youRectDrawn = true;

                    textPaint.Style = SKPaintStyle.Fill;
                    textPaint.Typeface = GHApp.UnderwoodTypeface;
                    textPaint.Color = SKColors.Black;
                    textPaint.TextSize = 36;

                    float twidth = textPaint.MeasureText("Strength");
                    float theight = textPaint.FontSpacing;
                    float tscale_one_column = Math.Max(0.1f, Math.Min((bkgrect.Width * (1 - 2f / 12.6f) / 4) / twidth, (bkgrect.Height * (1 - 2f / 8.5f) / 21) / theight));
                    float tscale_two_columns = Math.Max(0.1f, Math.Min((bkgrect.Width * (1 - 2f / 12.6f) / 4) / twidth, (bkgrect.Height * (1 - 2f / 8.5f) / 18) / theight));
                    //float strwidth_one_column = twidth * tscale_one_column;
                    float strwidth_two_columns = twidth * tscale_two_columns;
                    //float indentation_one_column = strwidth_one_column * 20f / 8f;
                    float indentation_two_columns = strwidth_two_columns * 20f / 8f;
                    bool use_two_columns = bkgrect.Width - bkgrect.Width * 2f / 12.6f >= indentation_two_columns * 2.5f;

                    float tscale = use_two_columns ? tscale_two_columns : tscale_one_column;
                    float basefontsize = textPaint.TextSize * tscale;
                    textPaint.TextSize = basefontsize;
                    float strwidth = twidth * tscale;
                    float indentation = strwidth * 20f / 8f;

                    string valtext, valtext2;
                    ty = bkgrect.Top + bkgrect.Height / 8.5f - textPaint.FontMetrics.Ascent;
                    float base_ty = ty;
                    float box_bottom_draw_threshold = box_bottom - bkgrect.Height / 8.5f;
                    float icon_height = textPaint.FontSpacing * 0.85f;
                    float icon_max_width = icon_height * 2f;
                    float icon_base_left = bkgrect.Left - icon_max_width; //bkgrect.Right - bkgrect.Width * 1f / 12.6f - icon_max_width
                    float icon_tx = icon_base_left;
                    float icon_ty;
                    icon_ty = ty + textPaint.FontMetrics.Ascent + (textPaint.FontSpacing - icon_height) / 2;
                    float icon_width = icon_height;
                    SKRect icon_rect = new SKRect(tx, ty, tx + icon_width, ty + icon_height);
                    int valcolor = (int)nhcolor.CLR_WHITE;

                    valtext = "";
                    lock (StatusFieldLock)
                    {
                        if (StatusFields[(int)statusfields.BL_TITLE] != null && StatusFields[(int)statusfields.BL_TITLE].IsEnabled && StatusFields[(int)statusfields.BL_TITLE].Text != null)
                        {
                            valtext = StatusFields[(int)statusfields.BL_TITLE].Text.Trim();
                        }
                    }
                    if (valtext != "")
                    {
                        textPaint.Typeface = GHApp.ImmortalTypeface;
                        textPaint.TextSize = basefontsize * 1.1f;
                        tx = (bkgrect.Left + bkgrect.Right) / 2;
                        textPaint.TextAlign = SKTextAlign.Center;
                        canvas.DrawText(valtext, tx, ty, textPaint);
                        textPaint.TextAlign = SKTextAlign.Left;
                        textPaint.Typeface = GHApp.UnderwoodTypeface;
                        textPaint.TextSize = basefontsize;
                        tx = bkgrect.Left + bkgrect.Width / 12.6f;
                        ty += textPaint.FontSpacing;
                        ty += textPaint.FontSpacing * 0.5f;
                    }

                    using (new SKAutoCanvasRestore(canvas, true))
                    {
                        SKRect cliprect = new SKRect(0, ty + textPaint.FontMetrics.Ascent, canvaswidth, bkgrect.Bottom - bkgrect.Height / 8.5f);
                        canvas.ClipRect(cliprect);

                        ty += _statusOffsetY;
                        base_ty = ty;

                        lock (_mapOffsetLock)
                        {
                            _statusClipBottom = cliprect.Bottom;
                        }
                        for (int i = 0; i < 6; i++)
                        {
                            valtext = "";
                            lock (StatusFieldLock)
                            {
                                if (StatusFields[(int)statusfields.BL_STR + i] != null && StatusFields[(int)statusfields.BL_STR + i].IsEnabled && StatusFields[(int)statusfields.BL_STR + i].Text != null)
                                {
                                    valtext = StatusFields[(int)statusfields.BL_STR + i].Text;
                                    valcolor = StatusFields[(int)statusfields.BL_STR + i].Color;
                                }
                            }
                            if (valtext != "" && ty < box_bottom_draw_threshold)
                            {
                                string[] statstring = new string[6] { "Strength", "Dexterity", "Constitution", "Intelligence", "Wisdom", "Charisma" };
                                string printtext = statstring[i] + ":";
                                canvas.DrawText(printtext, tx, ty, textPaint);
                                textPaint.Color = UIUtils.NHColor2SKColorCore(valcolor, 0, true, false);
                                canvas.DrawText(valtext, tx + indentation, ty, textPaint);
                                textPaint.Color = SKColors.Black;
                                ty += textPaint.FontSpacing;
                            }
                        }
                        ty += textPaint.FontSpacing * 0.5f;

                        valtext = "";
                        lock (StatusFieldLock)
                        {
                            if (StatusFields[(int)statusfields.BL_XP] != null && StatusFields[(int)statusfields.BL_XP].IsEnabled && StatusFields[(int)statusfields.BL_XP].Text != null)
                            {
                                valtext = StatusFields[(int)statusfields.BL_XP].Text;
                            }
                        }
                        if (valtext != "" && ty < box_bottom_draw_threshold)
                        {
                            canvas.DrawText("Level:", tx, ty, textPaint);
                            canvas.DrawText(valtext, tx + indentation, ty, textPaint);
                            icon_width = icon_height * (float)GHApp._statusXPLevelBitmap.Width / (float)GHApp._statusXPLevelBitmap.Height;
                            icon_tx = icon_base_left + (icon_max_width - icon_width) / 2f;
                            icon_ty = ty + textPaint.FontMetrics.Ascent - textPaint.FontMetrics.Descent / 2 + (textPaint.FontSpacing - icon_height) / 2;
                            icon_rect = new SKRect(icon_tx, icon_ty, icon_tx + icon_width, icon_ty + icon_height);
                            canvas.DrawBitmap(GHApp._statusXPLevelBitmap, icon_rect);
                            ty += textPaint.FontSpacing;
                        }

                        valtext = "";
                        lock (StatusFieldLock)
                        {
                            if (StatusFields[(int)statusfields.BL_EXP] != null && StatusFields[(int)statusfields.BL_EXP].IsEnabled && StatusFields[(int)statusfields.BL_EXP].Text != null)
                            {
                                valtext = StatusFields[(int)statusfields.BL_EXP].Text;
                            }
                        }
                        if (valtext != "" && ty < box_bottom_draw_threshold)
                        {
                            canvas.DrawText("Experience:", tx, ty, textPaint);
                            canvas.DrawText(valtext, tx + indentation, ty, textPaint);
                            ty += textPaint.FontSpacing;
                        }

                        valtext = "";
                        lock (StatusFieldLock)
                        {
                            if (StatusFields[(int)statusfields.BL_HD] != null && StatusFields[(int)statusfields.BL_HD].IsEnabled && StatusFields[(int)statusfields.BL_HD].Text != null)
                            {
                                valtext = StatusFields[(int)statusfields.BL_HD].Text;
                            }
                        }
                        if (valtext != "" && ty < box_bottom_draw_threshold)
                        {
                            canvas.DrawText("Hit dice:", tx, ty, textPaint);
                            canvas.DrawText(valtext, tx + indentation, ty, textPaint);
                            icon_width = icon_height * (float)GHApp._statusHDBitmap.Width / (float)GHApp._statusHDBitmap.Height;
                            icon_tx = icon_base_left + (icon_max_width - icon_width) / 2f;
                            icon_ty = ty + textPaint.FontMetrics.Ascent - textPaint.FontMetrics.Descent / 2 + (textPaint.FontSpacing - icon_height) / 2;
                            icon_rect = new SKRect(icon_tx, icon_ty, icon_tx + icon_width, icon_ty + icon_height);
                            canvas.DrawBitmap(GHApp._statusHDBitmap, icon_rect);
                            ty += textPaint.FontSpacing;
                        }

                        valtext = "";
                        lock (StatusFieldLock)
                        {
                            if (StatusFields[(int)statusfields.BL_ALIGN] != null && StatusFields[(int)statusfields.BL_ALIGN].IsEnabled && StatusFields[(int)statusfields.BL_ALIGN].Text != null)
                            {
                                valtext = StatusFields[(int)statusfields.BL_ALIGN].Text;
                            }
                        }
                        if (valtext != "" && ty < box_bottom_draw_threshold)
                        {
                            canvas.DrawText("Alignment:", tx, ty, textPaint);
                            canvas.DrawText(valtext, tx + indentation, ty, textPaint);
                            ty += textPaint.FontSpacing;
                        }

                        valtext = "";
                        lock (StatusFieldLock)
                        {
                            if (StatusFields[(int)statusfields.BL_SCORE] != null && StatusFields[(int)statusfields.BL_SCORE].IsEnabled && StatusFields[(int)statusfields.BL_SCORE].Text != null)
                            {
                                valtext = StatusFields[(int)statusfields.BL_SCORE].Text;
                            }
                        }
                        if (valtext != "" && ty < box_bottom_draw_threshold)
                        {
                            canvas.DrawText("Score:", tx, ty, textPaint);
                            canvas.DrawText(valtext, tx + indentation, ty, textPaint);
                            ty += textPaint.FontSpacing;
                        }

                        ty += textPaint.FontSpacing * 0.5f;

                        valtext = "";
                        lock (StatusFieldLock)
                        {
                            if (StatusFields[(int)statusfields.BL_AC] != null && StatusFields[(int)statusfields.BL_AC].IsEnabled && StatusFields[(int)statusfields.BL_AC].Text != null)
                            {
                                valtext = StatusFields[(int)statusfields.BL_AC].Text;
                            }
                        }
                        if (valtext != "")
                        {
                            lock (_mapOffsetLock)
                            {
                                _statusLargestBottom = ty + textPaint.FontMetrics.Descent;
                            }
                            canvas.DrawText("Armor class:", tx, ty, textPaint);
                            canvas.DrawText(valtext, tx + indentation, ty, textPaint);
                            icon_width = icon_height * (float)GHApp._statusACBitmap.Width / (float)GHApp._statusACBitmap.Height;
                            icon_tx = icon_base_left + (icon_max_width - icon_width) / 2f;
                            icon_ty = ty + textPaint.FontMetrics.Ascent - textPaint.FontMetrics.Descent / 2 + (textPaint.FontSpacing - icon_height) / 2;
                            icon_rect = new SKRect(icon_tx, icon_ty, icon_tx + icon_width, icon_ty + icon_height);
                            canvas.DrawBitmap(GHApp._statusACBitmap, icon_rect);
                            ty += textPaint.FontSpacing;
                        }

                        valtext = "";
                        valtext2 = "";
                        lock (StatusFieldLock)
                        {
                            if (StatusFields[(int)statusfields.BL_MC_LVL] != null && StatusFields[(int)statusfields.BL_MC_LVL].IsEnabled && StatusFields[(int)statusfields.BL_MC_LVL].Text != null)
                            {
                                valtext = StatusFields[(int)statusfields.BL_MC_LVL].Text;
                            }
                            if (StatusFields[(int)statusfields.BL_MC_PCT] != null && StatusFields[(int)statusfields.BL_MC_PCT].IsEnabled && StatusFields[(int)statusfields.BL_MC_PCT].Text != null)
                            {
                                valtext2 = StatusFields[(int)statusfields.BL_MC_PCT].Text;
                            }
                        }
                        if (valtext != "")
                        {
                            lock (_mapOffsetLock)
                            {
                                _statusLargestBottom = ty + textPaint.FontMetrics.Descent;
                            }
                            canvas.DrawText("Magic cancellation:", tx, ty, textPaint);
                            string printtext = valtext2 != "" ? valtext + "/" + valtext2 + "%" : valtext;
                            canvas.DrawText(printtext, tx + indentation, ty, textPaint);
                            icon_width = icon_height * (float)GHApp._statusMCBitmap.Width / (float)GHApp._statusMCBitmap.Height;
                            icon_tx = icon_base_left + (icon_max_width - icon_width) / 2f;
                            icon_ty = ty + textPaint.FontMetrics.Ascent - textPaint.FontMetrics.Descent / 2 + (textPaint.FontSpacing - icon_height) / 2;
                            icon_rect = new SKRect(icon_tx, icon_ty, icon_tx + icon_width, icon_ty + icon_height);
                            canvas.DrawBitmap(GHApp._statusMCBitmap, icon_rect);
                            ty += textPaint.FontSpacing;
                        }

                        valtext = "";
                        lock (StatusFieldLock)
                        {
                            if (StatusFields[(int)statusfields.BL_MOVE] != null && StatusFields[(int)statusfields.BL_MOVE].IsEnabled && StatusFields[(int)statusfields.BL_MOVE].Text != null)
                            {
                                valtext = StatusFields[(int)statusfields.BL_MOVE].Text;
                            }
                        }
                        if (valtext != "")
                        {
                            lock (_mapOffsetLock)
                            {
                                _statusLargestBottom = ty + textPaint.FontMetrics.Descent;
                            }
                            canvas.DrawText("Move:", tx, ty, textPaint);
                            canvas.DrawText(valtext, tx + indentation, ty, textPaint);
                            icon_width = icon_height * (float)GHApp._statusMoveBitmap.Width / (float)GHApp._statusMoveBitmap.Height;
                            icon_tx = icon_base_left + (icon_max_width - icon_width) / 2f;
                            icon_ty = ty + textPaint.FontMetrics.Ascent - textPaint.FontMetrics.Descent / 2 + (textPaint.FontSpacing - icon_height) / 2;
                            icon_rect = new SKRect(icon_tx, icon_ty, icon_tx + icon_width, icon_ty + icon_height);
                            canvas.DrawBitmap(GHApp._statusMoveBitmap, icon_rect);
                            ty += textPaint.FontSpacing;
                        }

                        valtext = "";
                        valtext2 = "";
                        string valtext3 = "";
                        lock (StatusFieldLock)
                        {
                            if (StatusFields[(int)statusfields.BL_UWEP] != null && StatusFields[(int)statusfields.BL_UWEP].IsEnabled && StatusFields[(int)statusfields.BL_UWEP].Text != null)
                            {
                                valtext = StatusFields[(int)statusfields.BL_UWEP].Text;
                            }
                            if (StatusFields[(int)statusfields.BL_UWEP2] != null && StatusFields[(int)statusfields.BL_UWEP2].IsEnabled && StatusFields[(int)statusfields.BL_UWEP2].Text != null)
                            {
                                valtext2 = StatusFields[(int)statusfields.BL_UWEP2].Text;
                            }
                            if (StatusFields[(int)statusfields.BL_UQUIVER] != null && StatusFields[(int)statusfields.BL_UQUIVER].IsEnabled && StatusFields[(int)statusfields.BL_UQUIVER].Text != null)
                            {
                                valtext3 = StatusFields[(int)statusfields.BL_UQUIVER].Text;
                            }
                        }
                        if (valtext != "" || valtext2 != "" || valtext3 != "")
                        {
                            lock (_mapOffsetLock)
                            {
                                _statusLargestBottom = ty + textPaint.FontMetrics.Descent;
                            }
                            canvas.DrawText("Weapon style:", tx, ty, textPaint);
                            string printtext = valtext;
                            if(valtext2 != "")
                                printtext += "/" + valtext2;
                            if (valtext3 != "")
                                printtext += "/" + valtext3;
                            canvas.DrawText(printtext, tx + indentation, ty, textPaint);
                            icon_width = icon_height * (float)GHApp._statusWeaponStyleBitmap.Width / (float)GHApp._statusWeaponStyleBitmap.Height;
                            icon_tx = icon_base_left + (icon_max_width - icon_width) / 2f;
                            icon_ty = ty + textPaint.FontMetrics.Ascent - textPaint.FontMetrics.Descent / 2 + (textPaint.FontSpacing - icon_height) / 2;
                            icon_rect = new SKRect(icon_tx, icon_ty, icon_tx + icon_width, icon_ty + icon_height);
                            canvas.DrawBitmap(GHApp._statusWeaponStyleBitmap, icon_rect);
                            ty += textPaint.FontSpacing;
                        }

                        ty += textPaint.FontSpacing * 0.5f;

                        valtext = "";
                        lock (StatusFieldLock)
                        {
                            if (StatusFields[(int)statusfields.BL_GOLD] != null && StatusFields[(int)statusfields.BL_GOLD].IsEnabled && StatusFields[(int)statusfields.BL_GOLD].Text != null)
                            {
                                valtext = StatusFields[(int)statusfields.BL_GOLD].Text;
                            }
                        }
                        if (valtext != "")
                        {
                            lock (_mapOffsetLock)
                            {
                                _statusLargestBottom = ty + textPaint.FontMetrics.Descent;
                            }
                            string printtext;
                            if (valtext.Length > 11 && valtext.Substring(0, 1) == "\\")
                                printtext = valtext.Substring(11);
                            else
                                printtext = valtext;

                            canvas.DrawText("Gold:", tx, ty, textPaint);
                            canvas.DrawText(printtext, tx + indentation, ty, textPaint);
                            icon_width = icon_height * (float)GHApp._statusGoldBitmap.Width / (float)GHApp._statusGoldBitmap.Height;
                            icon_tx = icon_base_left + (icon_max_width - icon_width) / 2f;
                            icon_ty = ty + textPaint.FontMetrics.Ascent - textPaint.FontMetrics.Descent / 2 + (textPaint.FontSpacing - icon_height) / 2;
                            icon_rect = new SKRect(icon_tx, icon_ty, icon_tx + icon_width, icon_ty + icon_height);
                            canvas.DrawBitmap(GHApp._statusGoldBitmap, icon_rect);
                            ty += textPaint.FontSpacing;
                        }

                        valtext = "";
                        lock (StatusFieldLock)
                        {
                            if (StatusFields[(int)statusfields.BL_TIME] != null && StatusFields[(int)statusfields.BL_TIME].IsEnabled && StatusFields[(int)statusfields.BL_TIME].Text != null)
                            {
                                valtext = StatusFields[(int)statusfields.BL_TIME].Text;
                            }
                        }
                        if (valtext != "")
                        {
                            lock (_mapOffsetLock)
                            {
                                _statusLargestBottom = ty + textPaint.FontMetrics.Descent;
                            }
                            canvas.DrawText("Turns:", tx, ty, textPaint);
                            canvas.DrawText(valtext, tx + indentation, ty, textPaint);
                            icon_width = icon_height * (float)GHApp._statusTurnsBitmap.Width / (float)GHApp._statusTurnsBitmap.Height;
                            icon_tx = icon_base_left + (icon_max_width - icon_width) / 2f;
                            icon_ty = ty + textPaint.FontMetrics.Ascent - textPaint.FontMetrics.Descent / 2 + (textPaint.FontSpacing - icon_height) / 2;
                            icon_rect = new SKRect(icon_tx, icon_ty, icon_tx + icon_width, icon_ty + icon_height);
                            canvas.DrawBitmap(GHApp._statusTurnsBitmap, icon_rect);
                            ty += textPaint.FontSpacing;
                        }

                        ty += textPaint.FontSpacing * 0.5f;

                        /* Condition, status and buff marks */
                        if (use_two_columns)
                        {
                            tx += indentation * 1.75f;
                            ty = base_ty;
                        }

                        float marksize = textPaint.FontSpacing * 0.85f;
                        float markpadding = marksize / 4;
                        ulong status_bits;
                        lock (_uLock)
                        {
                            status_bits = _u_status_bits;
                        }
                        if (status_bits != 0)
                        {
                            int tiles_per_row = GHConstants.TileWidth / GHConstants.StatusMarkWidth;
                            int mglyph = (int)game_ui_tile_types.STATUS_MARKS + GHApp.UITileOff;
                            int mtile = GHApp.Glyph2Tile[mglyph];
                            int sheet_idx = GHApp.TileSheetIdx(mtile);
                            int tile_x = GHApp.TileSheetX(mtile);
                            int tile_y = GHApp.TileSheetY(mtile);
                            foreach (int status_mark in _statusmarkorder)
                            {
                                ulong statusbit = 1UL << status_mark;
                                if ((status_bits & statusbit) != 0)
                                {
                                    string statusname = _status_names[status_mark];
                                    int within_tile_x = status_mark % tiles_per_row;
                                    int within_tile_y = status_mark / tiles_per_row;
                                    int c_x = tile_x + within_tile_x * GHConstants.StatusMarkWidth;
                                    int c_y = tile_y + within_tile_y * GHConstants.StatusMarkHeight;

                                    SKRect source_rt = new SKRect();
                                    source_rt.Left = c_x;
                                    source_rt.Right = c_x + GHConstants.StatusMarkWidth;
                                    source_rt.Top = c_y;
                                    source_rt.Bottom = c_y + GHConstants.StatusMarkHeight;

                                    SKRect target_rt = new SKRect();
                                    target_rt.Left = tx;
                                    target_rt.Right = target_rt.Left + marksize;
                                    target_rt.Top = ty + textPaint.FontMetrics.Ascent - textPaint.FontMetrics.Descent / 2 + (textPaint.FontSpacing - marksize) / 2;
                                    target_rt.Bottom = target_rt.Top + marksize;

                                    canvas.DrawBitmap(TileMap[sheet_idx], source_rt, target_rt);
                                    canvas.DrawText(statusname, tx + marksize + markpadding, ty, textPaint);
                                    lock (_mapOffsetLock)
                                    {
                                        _statusLargestBottom = ty + textPaint.FontMetrics.Descent;
                                    }
                                    ty += textPaint.FontSpacing;
                                }
                            }
                        }

                        ulong condition_bits;
                        lock (_uLock)
                        {
                            condition_bits = _u_condition_bits;
                        }
                        if (condition_bits != 0)
                        {
                            int tiles_per_row = GHConstants.TileWidth / GHConstants.StatusMarkWidth;
                            int mglyph = (int)game_ui_tile_types.CONDITION_MARKS + GHApp.UITileOff;
                            int mtile = GHApp.Glyph2Tile[mglyph];
                            int sheet_idx = GHApp.TileSheetIdx(mtile);
                            int tile_x = GHApp.TileSheetX(mtile);
                            int tile_y = GHApp.TileSheetY(mtile);
                            for (int condition_mark = 0; condition_mark < (int)bl_conditions.NUM_BL_CONDITIONS; condition_mark++)
                            {
                                ulong conditionbit = 1UL << condition_mark;
                                if ((condition_bits & conditionbit) != 0)
                                {
                                    string conditionname = _condition_names[condition_mark];
                                    int within_tile_x = condition_mark % tiles_per_row;
                                    int within_tile_y = condition_mark / tiles_per_row;
                                    int c_x = tile_x + within_tile_x * GHConstants.StatusMarkWidth;
                                    int c_y = tile_y + within_tile_y * GHConstants.StatusMarkHeight;

                                    SKRect source_rt = new SKRect();
                                    source_rt.Left = c_x;
                                    source_rt.Right = c_x + GHConstants.StatusMarkWidth;
                                    source_rt.Top = c_y;
                                    source_rt.Bottom = c_y + GHConstants.StatusMarkHeight;

                                    SKRect target_rt = new SKRect();
                                    target_rt.Left = tx;
                                    target_rt.Right = target_rt.Left + marksize;
                                    target_rt.Top = ty + textPaint.FontMetrics.Ascent - textPaint.FontMetrics.Descent / 2 + (textPaint.FontSpacing - marksize) / 2;
                                    target_rt.Bottom = target_rt.Top + marksize;

                                    canvas.DrawBitmap(TileMap[sheet_idx], source_rt, target_rt);
                                    canvas.DrawText(conditionname, tx + marksize + markpadding, ty, textPaint);
                                    lock (_mapOffsetLock)
                                    {
                                        _statusLargestBottom = ty + textPaint.FontMetrics.Descent;
                                    }
                                    ty += textPaint.FontSpacing;
                                }
                            }
                        }

                        ulong buff_bits;
                        for (int buff_ulong = 0; buff_ulong < GHConstants.NUM_BUFF_BIT_ULONGS; buff_ulong++)
                        {
                            lock (_uLock)
                            {
                                buff_bits = _u_buff_bits[buff_ulong];
                            }
                            int tiles_per_row = GHConstants.TileWidth / GHConstants.StatusMarkWidth;
                            if (buff_bits != 0)
                            {
                                for (int buff_idx = 0; buff_idx < 32; buff_idx++)
                                {
                                    ulong buffbit = 1UL << buff_idx;
                                    if ((buff_bits & buffbit) != 0)
                                    {
                                        int propidx = buff_ulong * 32 + buff_idx;
                                        if (propidx > GHConstants.LAST_PROP)
                                            break;
                                        string propname = GHApp.GnollHackService.GetPropertyName(propidx);
                                        if (propname != null && propname.Length > 0)
                                            propname = propname[0].ToString().ToUpper() + (propname.Length == 1 ? "" : propname.Substring(1));

                                        int mglyph = (propidx - 1) / GHConstants.BUFFS_PER_TILE + GHApp.BuffTileOff;
                                        int mtile = GHApp.Glyph2Tile[mglyph];
                                        int sheet_idx = GHApp.TileSheetIdx(mtile);
                                        int tile_x = GHApp.TileSheetX(mtile);
                                        int tile_y = GHApp.TileSheetY(mtile);

                                        int buff_mark = (propidx - 1) % GHConstants.BUFFS_PER_TILE;
                                        int within_tile_x = buff_mark % tiles_per_row;
                                        int within_tile_y = buff_mark / tiles_per_row;
                                        int c_x = tile_x + within_tile_x * GHConstants.StatusMarkWidth;
                                        int c_y = tile_y + within_tile_y * GHConstants.StatusMarkHeight;

                                        SKRect source_rt = new SKRect();
                                        source_rt.Left = c_x;
                                        source_rt.Right = c_x + GHConstants.StatusMarkWidth;
                                        source_rt.Top = c_y;
                                        source_rt.Bottom = c_y + GHConstants.StatusMarkHeight;

                                        SKRect target_rt = new SKRect();
                                        target_rt.Left = tx;
                                        target_rt.Right = target_rt.Left + marksize;
                                        target_rt.Top = ty + textPaint.FontMetrics.Ascent - textPaint.FontMetrics.Descent / 2 + (textPaint.FontSpacing - marksize) / 2;
                                        target_rt.Bottom = target_rt.Top + marksize;

                                        canvas.DrawBitmap(TileMap[sheet_idx], source_rt, target_rt);
                                        if (propname != null)
                                            canvas.DrawText(propname, tx + marksize + markpadding, ty, textPaint);
                                        lock (_mapOffsetLock)
                                        {
                                            _statusLargestBottom = ty + textPaint.FontMetrics.Descent;
                                        }
                                        ty += textPaint.FontSpacing;
                                    }
                                }
                            }
                        }
                    }                   

                    if(!_youRectDrawn)
                        YouRect = new SKRect();
                }

                if (ShowWaitIcon)
                {
                    SKRect targetrect;
                    float size = canvaswidth / 5.0f;
                    targetrect = new SKRect(canvaswidth / 2 - size / 2, canvasheight / 2 - size / 2, canvaswidth / 2 + size / 2, canvasheight / 2 + size / 2);
                    canvas.DrawBitmap(GHApp._logoBitmap, targetrect);
                }
            }

#if GNH_MAP_PROFILING && DEBUG
            if ((_totalFrames % 120) == 0)
                Debug.WriteLine("Frames: " + _totalFrames + ", bmp: " + _profilingStopwatchBmp.ElapsedTicks / _totalFrames + ", text: " + _profilingStopwatchText.ElapsedTicks / _totalFrames + ", rect: " + _profilingStopwatchRect.ElapsedTicks / _totalFrames);
#endif
        }

        bool IsNoWallEndAutoDraw(int x, int y)
        {
            if (!GHUtils.isok(x, y))
                return true;

            if (_mapData[x, y].Layers.layer_gui_glyphs[(int)layer_types.LAYER_FLOOR] == GHApp.UnexploredGlyph
                || _mapData[x, y].Layers.layer_gui_glyphs[(int)layer_types.LAYER_FLOOR] == GHApp.NoGlyph)
                return true;

            if ((_mapData[x, y].Layers.layer_flags & (ulong)LayerFlags.LFLAGS_NO_WALL_END_AUTODRAW) != 0)
                return true;

            return false;
        }

        struct SavedAutodrawBitmap
        {
            public int Autodraw;
            public double FillPercentage;
            public int Stage;

            public SavedAutodrawBitmap(int autodraw, double fillPercentage, int stage)
            {
                Autodraw = autodraw;
                FillPercentage = fillPercentage;
                Stage = stage;
            }
        }

        Dictionary<SavedAutodrawBitmap, SKBitmap> _savedAutoDrawBitmaps = new Dictionary<SavedAutodrawBitmap, SKBitmap>();

        public void DrawAutoDraw(int autodraw, SKCanvas canvas, SKPaint paint, ObjectDataItem otmp_round,
            int layer_idx, int mapx, int mapy,
            bool tileflag_halfsize, bool tileflag_normalobjmissile, bool tileflag_fullsizeditem,
            float tx, float ty, float width, float height,
            float scale, float targetscale, float scaled_x_padding, float scaled_y_padding, float scaled_tile_height,
            bool is_inventory, bool drawwallends)
        {
            /******************/
            /* AUTODRAW START */
            /******************/
            if (GHApp._autodraws != null)
            {
                float opaqueness = 1;
                int sheet_idx = 0;
                float extra_y_padding = 0;
                if (!is_inventory)
                {
                    if (tileflag_normalobjmissile)
                    {
                        if (!tileflag_fullsizeditem)
                            extra_y_padding += (height - scaled_tile_height) / 2;
                    }
                    else if (tileflag_halfsize)
                    {
                        extra_y_padding += height / 2;
                    }
                }

                if (drawwallends && GHApp._autodraws[autodraw].draw_type == (int)autodraw_drawing_types.AUTODRAW_DRAW_REPLACE_WALL_ENDS)
                {
                    for (byte dir = 0; dir < 4; dir++)
                    {
                        byte dir_bit = (byte)(1 << dir);
                        if ((GHApp._autodraws[autodraw].flags & dir_bit) != 0)
                        {
                            int rx = 0;
                            int ry = 0;
                            int[] corner_x = new int[2];
                            int[] corner_y = new int[2];
                            switch (dir)
                            {
                                case 0:
                                    rx = mapx - 1;
                                    ry = mapy;
                                    corner_x[0] = mapx;
                                    corner_y[0] = mapy - 1;
                                    corner_x[1] = mapx;
                                    corner_y[1] = mapy + 1;
                                    break;
                                case 1:
                                    rx = mapx + 1;
                                    ry = mapy;
                                    corner_x[0] = mapx;
                                    corner_y[0] = mapy - 1;
                                    corner_x[1] = mapx;
                                    corner_y[1] = mapy + 1;
                                    break;
                                case 2:
                                    rx = mapx;
                                    ry = mapy - 1;
                                    corner_x[0] = mapx - 1;
                                    corner_y[0] = mapy;
                                    corner_x[1] = mapx + 1;
                                    corner_y[1] = mapy;
                                    break;
                                case 3:
                                    rx = mapx;
                                    ry = mapy + 1;
                                    corner_x[0] = mapx - 1;
                                    corner_y[0] = mapy;
                                    corner_x[1] = mapx + 1;
                                    corner_y[1] = mapy;
                                    break;
                                default:
                                    break;
                            }

                            if (IsNoWallEndAutoDraw(rx, ry)) // NO_WALL_END_AUTODRAW(rx, ry))
                            {
                                /* No action */
                            }
                            else
                            {
                                for (int corner = 0; corner <= 1; corner++)
                                {
                                    int source_glyph = GHApp._autodraws[autodraw].source_glyph;
                                    int atile = GHApp.Glyph2Tile[source_glyph];
                                    int a_sheet_idx = GHApp.TileSheetIdx(atile);
                                    int at_x = GHApp.TileSheetX(atile);
                                    int at_y = GHApp.TileSheetY(atile);

                                    SKRect source_rt = new SKRect();
                                    switch (dir)
                                    {
                                        case 0: /* left */
                                            if (IsNoWallEndAutoDraw(corner_x[corner], corner_y[corner])) // NO_WALL_END_AUTODRAW(corner_x[corner], corner_y[corner]))
                                            {
                                                source_glyph = GHApp._autodraws[autodraw].source_glyph2; /* S_vwall */
                                                atile = GHApp.Glyph2Tile[source_glyph];
                                                a_sheet_idx = GHApp.TileSheetIdx(atile);
                                                at_x = GHApp.TileSheetX(atile);
                                                at_y = GHApp.TileSheetY(atile);
                                            }
                                            source_rt.Left = at_x;
                                            source_rt.Right = source_rt.Left + 12;
                                            if (corner == 0)
                                            {
                                                source_rt.Top = at_y;
                                                source_rt.Bottom = at_y + 18;
                                            }
                                            else
                                            {
                                                source_rt.Top = at_y + 18;
                                                source_rt.Bottom = at_y + GHConstants.TileHeight;
                                            }
                                            break;
                                        case 1: /* right */
                                            //if (NO_WALL_END_AUTODRAW(corner_x[corner], corner_y[corner]))
                                            if (IsNoWallEndAutoDraw(corner_x[corner], corner_y[corner]))
                                            {
                                                source_glyph = GHApp._autodraws[autodraw].source_glyph2; /* S_vwall */
                                                atile = GHApp.Glyph2Tile[source_glyph];
                                                a_sheet_idx = GHApp.TileSheetIdx(atile);
                                                at_x = GHApp.TileSheetX(atile);
                                                at_y = GHApp.TileSheetY(atile);
                                            }
                                            source_rt.Right = at_x + GHConstants.TileWidth;
                                            source_rt.Left = source_rt.Right - 12;
                                            if (corner == 0)
                                            {
                                                source_rt.Top = at_y;
                                                source_rt.Bottom = at_y + 18;
                                            }
                                            else
                                            {
                                                source_rt.Top = at_y + 18;
                                                source_rt.Bottom = at_y + GHConstants.TileHeight;
                                            }
                                            break;
                                        case 2: /* up */
                                            //if (NO_WALL_END_AUTODRAW(corner_x[corner], corner_y[corner]))
                                            if (IsNoWallEndAutoDraw(corner_x[corner], corner_y[corner]))
                                            {
                                                source_glyph = GHApp._autodraws[autodraw].source_glyph3; /* S_hwall */
                                                atile = GHApp.Glyph2Tile[source_glyph];
                                                a_sheet_idx = GHApp.TileSheetIdx(atile);
                                                at_x = GHApp.TileSheetX(atile);
                                                at_y = GHApp.TileSheetY(atile);
                                            }
                                            if (corner == 0)
                                            {
                                                source_rt.Left = at_x;
                                                source_rt.Right = at_x + GHConstants.TileWidth / 2;
                                            }
                                            else
                                            {
                                                source_rt.Left = at_x + GHConstants.TileWidth / 2;
                                                source_rt.Right = at_x + GHConstants.TileWidth;
                                            }
                                            source_rt.Top = at_y;
                                            source_rt.Bottom = source_rt.Top + 12;
                                            break;
                                        case 3: /* down */
                                            //if (NO_WALL_END_AUTODRAW(corner_x[corner], corner_y[corner]))
                                            if (IsNoWallEndAutoDraw(corner_x[corner], corner_y[corner]))
                                            {
                                                source_glyph = GHApp._autodraws[autodraw].source_glyph3; /* S_hwall */
                                                atile = GHApp.Glyph2Tile[source_glyph];
                                                a_sheet_idx = GHApp.TileSheetIdx(atile);
                                                at_x = GHApp.TileSheetX(atile);
                                                at_y = GHApp.TileSheetY(atile);
                                            }
                                            if (corner == 0)
                                            {
                                                source_rt.Left = at_x;
                                                source_rt.Right = at_x + GHConstants.TileWidth / 2;
                                            }
                                            else
                                            {
                                                source_rt.Left = at_x + GHConstants.TileWidth / 2;
                                                source_rt.Right = at_x + GHConstants.TileWidth;
                                            }
                                            source_rt.Top = at_y + 12;
                                            source_rt.Bottom = at_y + GHConstants.TileHeight;
                                            break;
                                        default:
                                            break;
                                    }

                                    SKRect target_rt = new SKRect();
                                    target_rt.Left = tx + (targetscale * (float)(source_rt.Left - at_x));
                                    target_rt.Right = tx + (targetscale * (float)(source_rt.Right - at_x));
                                    target_rt.Top = ty + (targetscale * (float)(source_rt.Top - at_y));
                                    target_rt.Bottom = ty + (targetscale * (float)(source_rt.Bottom - at_y));
#if GNH_MAP_PROFILING && DEBUG
                                    StartProfiling(GHProfilingStyle.Bitmap);
#endif
                                    canvas.DrawBitmap(TileMap[a_sheet_idx], source_rt, target_rt, paint);
#if GNH_MAP_PROFILING && DEBUG
                                    StopProfiling(GHProfilingStyle.Bitmap);
#endif
                                }
                            }
                        }
                    }
                }
                else if (
                    ((GHApp._autodraws[autodraw].draw_type == (int)autodraw_drawing_types.AUTODRAW_DRAW_CHAIN || GHApp._autodraws[autodraw].draw_type == (int)autodraw_drawing_types.AUTODRAW_DRAW_BALL) 
                      && (otmp_round == null || otmp_round.OtypData.is_uchain != 0 || otmp_round.OtypData.is_uball != 0) /* Currently a small kludge to ensure that the autodraw applies only to uchain and uball */
                    )
                    /*|| autodraw_u_punished*/)
                {
                    DrawChain(canvas, paint, mapx, mapy, autodraw, false, width, height, ty, tx, scale, targetscale);
                }
                else if (GHApp._autodraws[autodraw].draw_type == (int)autodraw_drawing_types.AUTODRAW_DRAW_LONG_WORM)
                {
                    /* Long worm here */

                    int source_glyph_seg_end = GHApp._autodraws[autodraw].source_glyph;
                    int source_glyph_seg_dir_out = GHApp._autodraws[autodraw].source_glyph2;
                    int source_glyph_seg_dir_in = GHApp._autodraws[autodraw].source_glyph2 + 4;
                    int source_glyph_seg_layer = GHApp._autodraws[autodraw].source_glyph3;
                    int drawing_tail = GHApp._autodraws[autodraw].flags;
                    int wdir_out = _mapData[mapx, mapy].Layers.wsegdir;
                    int wdir_in = _mapData[mapx, mapy].Layers.reverse_prev_wsegdir;
                    bool is_head = (_mapData[mapx, mapy].Layers.monster_flags & (ulong)LayerMonsterFlags.LMFLAGS_WORM_HEAD) != 0;
                    bool is_tailend = (_mapData[mapx, mapy].Layers.monster_flags & (ulong)LayerMonsterFlags.LMFLAGS_WORM_TAILEND) != 0;
                    for (int wlayer = 0; wlayer < 5; wlayer++)
                    {
                        int source_glyph = GHApp.NoGlyph;
                        bool hflip_seg = false;
                        bool vflip_seg = false;
                        switch (wlayer)
                        {
                            case 0:
                            case 2:
                            case 4:
                                if (is_head || is_tailend)
                                    continue;
                                source_glyph = source_glyph_seg_layer + wlayer / 2;
                                break;
                            case 1:
                                source_glyph = is_tailend ? GHApp.NoGlyph : is_head ? source_glyph_seg_end : source_glyph_seg_dir_in;
                                break;
                            case 3:
                                source_glyph = is_tailend ? source_glyph_seg_end : is_head ? GHApp.NoGlyph : source_glyph_seg_dir_out;
                                break;
                            default:
                                break;
                        }

                        if (source_glyph != GHApp.NoGlyph)
                        {
                            int wdir = (wlayer == 1 ? wdir_in : wlayer == 3 ? wdir_out : 0);
                            switch (wdir)
                            {
                                case 1:
                                    source_glyph += 2;
                                    hflip_seg = false;
                                    vflip_seg = false;
                                    break;
                                case 2:
                                    source_glyph += 0;
                                    hflip_seg = false;
                                    vflip_seg = false;
                                    break;
                                case 3:
                                    source_glyph += 3;
                                    hflip_seg = false;
                                    vflip_seg = true;
                                    break;
                                case 4:
                                    source_glyph += 1;
                                    hflip_seg = true;
                                    vflip_seg = false;
                                    break;
                                case 5:
                                    source_glyph += 3;
                                    hflip_seg = false;
                                    vflip_seg = false;
                                    break;
                                case 6:
                                    source_glyph += 0;
                                    hflip_seg = false;
                                    vflip_seg = true;
                                    break;
                                case 7:
                                    source_glyph += 2;
                                    hflip_seg = false;
                                    vflip_seg = true;
                                    break;
                                case 8:
                                    source_glyph += 1;
                                    hflip_seg = false;
                                    vflip_seg = false;
                                    break;
                                default:
                                    break;
                            }

                            int atile = GHApp.Glyph2Tile[source_glyph];
                            int a_sheet_idx = GHApp.TileSheetIdx(atile);
                            int at_x = GHApp.TileSheetX(atile);
                            int at_y = GHApp.TileSheetY(atile);

                            int worm_source_x = at_x;
                            int worm_source_y = at_y;
                            int worm_source_width = GHConstants.TileWidth;
                            int worm_source_height = GHConstants.TileHeight;
                            SKRect sourcerect = new SKRect(worm_source_x, worm_source_y, worm_source_x + worm_source_width, worm_source_y + worm_source_height);

                            float target_x = tx;
                            float target_y = ty;
                            float target_width = ((float)worm_source_width * targetscale);
                            float target_height = ((float)worm_source_height * targetscale);
                            SKRect targetrect;
                            targetrect = new SKRect(0, 0, target_width, target_height);

                            using (new SKAutoCanvasRestore(canvas, true))
                            {
                                canvas.Translate(target_x + (hflip_seg ? width : 0), target_y + (vflip_seg ? height : 0));
                                canvas.Scale(hflip_seg ? -1 : 1, vflip_seg ? -1 : 1, 0, 0);
                                paint.Color = paint.Color.WithAlpha((byte)(0xFF * opaqueness));
                                canvas.DrawBitmap(TileMap[sheet_idx], sourcerect, targetrect, paint);
                            }
                        }
                    }
                }
                else if (GHApp._autodraws[autodraw].draw_type == (int)autodraw_drawing_types.AUTODRAW_DRAW_BOOKSHELF_CONTENTS && otmp_round != null && otmp_round.ContainedObjs != null)
                {
                    int num_shelves = 4;
                    int y_to_first_shelf = 49;
                    int shelf_start = 8;
                    int shelf_width = 50;
                    int shelf_height = 10;
                    int shelf_border_height = 2;
                    int shelf_item_width = 5;
                    int src_book_x = 0;
                    int src_book_y = 0;
                    int src_scroll_x = 5;
                    int src_scroll_y = 0;
                    int cnt = 0;
                    int items_per_row = shelf_width / shelf_item_width;

                    foreach (ObjectDataItem contained_obj in otmp_round.ContainedObjs)
                    {
                        int src_x = 0, src_y = 0;
                        float dest_x = 0, dest_y = 0;
                        if (contained_obj.ObjData.oclass == (int)obj_class_types.SPBOOK_CLASS)
                        {
                            src_x = src_book_x;
                            src_y = src_book_y;
                        }
                        else if (contained_obj.ObjData.oclass == (int)obj_class_types.SCROLL_CLASS)
                        {
                            src_x = src_scroll_x;
                            src_y = src_scroll_y;
                        }
                        else
                            continue;

                        for (int item_idx = 0; item_idx < contained_obj.ObjData.quan; item_idx++)
                        {
                            int item_row = cnt / items_per_row;
                            int item_xpos = cnt % items_per_row;

                            if (item_row >= num_shelves)
                                break;

                            dest_y = (y_to_first_shelf + item_row * (shelf_height + shelf_border_height)) * scale * targetscale;
                            dest_x = (shelf_start + item_xpos * shelf_item_width) * scale * targetscale;

                            int source_glyph = GHApp._autodraws[autodraw].source_glyph;
                            int atile = GHApp.Glyph2Tile[source_glyph];
                            int a_sheet_idx = GHApp.TileSheetIdx(atile);
                            int at_x = GHApp.TileSheetX(atile);
                            int at_y = GHApp.TileSheetY(atile);

                            SKRect source_rt = new SKRect();
                            source_rt.Left = at_x + src_x;
                            source_rt.Right = source_rt.Left + shelf_item_width;
                            source_rt.Top = at_y + src_y;
                            source_rt.Bottom = source_rt.Top + shelf_height;

                            float target_x = tx + dest_x;
                            float target_y = ty + dest_y;
                            float target_width = targetscale * scale * source_rt.Width;
                            float target_height = targetscale * scale * source_rt.Height;
                            SKRect target_rt;
                            target_rt = new SKRect(0, 0, target_width, target_height);

                            using (new SKAutoCanvasRestore(canvas, true))
                            {
                                canvas.Translate(target_x, target_y);
                                canvas.Scale(1, 1, 0, 0);
                                paint.Color = paint.Color.WithAlpha((byte)(0xFF * opaqueness));
                                canvas.DrawBitmap(TileMap[a_sheet_idx], source_rt, target_rt, paint);
                            }

                            cnt++;
                        }
                    }
                }
                else if (GHApp._autodraws[autodraw].draw_type == (int)autodraw_drawing_types.AUTODRAW_DRAW_WEAPON_RACK_CONTENTS && otmp_round != null && otmp_round.ContainedObjs != null)
                {
                    int y_to_rack_top = 31;
                    int rack_start = 0; /* Assume weapons are drawn reasonably well in the center */
                    int rack_width = 48;
                    int rack_height = GHConstants.TileHeight - y_to_rack_top;
                    int rack_item_spacing = 6;

                    int cnt = 0;

                    foreach (ObjectDataItem contained_obj in otmp_round.ContainedObjs)
                    {
                        int source_glyph = Math.Abs(contained_obj.ObjData.gui_glyph);
                        if (source_glyph <= 0 || source_glyph == GHApp.NoGlyph)
                            continue;
                        bool has_floor_tile = (GHApp.GlyphTileFlags[source_glyph] & (byte)glyph_tile_flags.GLYPH_TILE_FLAG_HAS_FLOOR_TILE) != 0; // artidx > 0 ? has_artifact_floor_tile(artidx) : has_obj_floor_tile(contained_obj);
                        bool is_height_clipping = (GHApp.GlyphTileFlags[source_glyph] & (byte)glyph_tile_flags.GLYPH_TILE_FLAG_HEIGHT_IS_CLIPPING) != 0;
                        bool fullsizeditem = (GHApp.GlyphTileFlags[source_glyph] & (byte)glyph_tile_flags.GLYPH_TILE_FLAG_FULL_SIZED_ITEM) != 0;
                        int cobj_height = contained_obj.OtypData.tile_height; // artidx ? artilist[artidx].tile_floor_height : OBJ_TILE_HEIGHT(contained_obj->otyp);
                        int artidx = contained_obj.ObjData.oartifact;
                        float dest_x = 0, dest_y = 0;
                        int src_x = 0, src_y = fullsizeditem || has_floor_tile ? 0 : GHConstants.TileHeight / 2;
                        int item_width = has_floor_tile || is_height_clipping ? GHConstants.TileHeight / 2 : cobj_height > 0 ? cobj_height : GHConstants.TileHeight / 2;
                        int item_height = has_floor_tile || is_height_clipping ? GHConstants.TileWidth : (item_width * GHConstants.TileWidth) / (GHConstants.TileHeight / 2);
                        int padding = (GHConstants.TileHeight / 2 - item_width) / 2;
                        int vertical_padding = (GHConstants.TileWidth - item_height) / 2;
                        if (contained_obj.ObjData.oclass != (int)obj_class_types.WEAPON_CLASS)
                            continue;

                        int item_xpos = cnt / 2 * rack_item_spacing;
                        if (item_xpos >= rack_width / 2)
                            break;

                        dest_y = (y_to_rack_top + vertical_padding) * scale * targetscale;
                        dest_x = (cnt % 2 == 0 ? rack_start + item_xpos + padding : GHConstants.TileWidth - item_width - rack_start - item_xpos - padding) * scale * targetscale;

                        int atile = GHApp.Glyph2Tile[source_glyph];
                        int a_sheet_idx = GHApp.TileSheetIdx(atile);
                        int at_x = GHApp.TileSheetX(atile);
                        int at_y = GHApp.TileSheetY(atile);

                        SKRect source_rt = new SKRect();
                        source_rt.Left = at_x + src_x;
                        source_rt.Right = source_rt.Left + GHConstants.TileWidth;
                        source_rt.Top = at_y + src_y;
                        source_rt.Bottom = source_rt.Top + (fullsizeditem ? GHConstants.TileHeight : GHConstants.TileHeight / 2);

                        float original_width = source_rt.Right - source_rt.Left;
                        float original_height = source_rt.Bottom - source_rt.Top;
                        float rotated_width = original_height;
                        float rotated_height = original_width;

                        float content_scale = fullsizeditem || has_floor_tile || is_height_clipping ? 1.0f : item_width / 48.0f;

                        float target_x = tx + dest_x;
                        float target_y = ty + dest_y;
                        float target_width = targetscale * scale * content_scale * original_width; //(float)item_width;
                        float target_height = targetscale * scale * content_scale * original_height; //((float)item_width * rotated_height) / rotated_width;
                        SKRect target_rt;
                        target_rt = new SKRect(0, 0, target_width, target_height);

                        using (new SKAutoCanvasRestore(canvas, true))
                        {
                            canvas.Translate(target_x, target_y);
                            canvas.Scale(1, 1, 0, 0);
                            canvas.RotateDegrees(-90);
                            canvas.Translate(-target_width, 0);
                            paint.Color = paint.Color.WithAlpha((byte)(0xFF * opaqueness));
                            canvas.DrawBitmap(TileMap[a_sheet_idx], source_rt, target_rt, paint);
                        }

                        cnt++;
                    }
                }
                else if (GHApp._autodraws[autodraw].draw_type == (int)autodraw_drawing_types.AUTODRAW_DRAW_CANDELABRUM_CANDLES && (otmp_round != null || layer_idx == (int)layer_types.LAYER_MISSILE))
                {
                    float y_start = scaled_y_padding + extra_y_padding;
                    float x_start = scaled_x_padding;
                    int x_padding = 13;
                    int item_width = 6;
                    int item_height = 13;
                    int src_unlit_x = 0;
                    int src_unlit_y = 10;
                    int src_lit_x = 6 * (1 + (int)GHApp._autodraws[autodraw].flags);
                    int src_lit_y = 10;
                    int cnt = 0;
                    short missile_special_quality = _mapData[mapx, mapy].Layers.missile_special_quality;
                    bool missile_lamplit = (_mapData[mapx, mapy].Layers.missile_flags & (ulong)LayerMissileFlags.MISSILE_FLAGS_LIT) != 0;

                    for (int cidx = 0; cidx < Math.Min((short)7, otmp_round != null ? otmp_round.ObjData.special_quality : missile_special_quality); cidx++)
                    {
                        int src_x = 0, src_y = 0;
                        float dest_x = 0, dest_y = 0;
                        if (otmp_round != null ? otmp_round.LampLit : missile_lamplit)
                        {
                            src_x = src_lit_x;
                            src_y = src_lit_y;
                        }
                        else
                        {
                            src_x = src_unlit_x;
                            src_y = src_unlit_y;
                        }

                        int item_xpos = cnt;

                        dest_y = y_start;
                        dest_x = x_start + ((float)(x_padding + item_xpos * item_width) * scale * targetscale);

                        int source_glyph = GHApp._autodraws[autodraw].source_glyph;
                        int atile = GHApp.Glyph2Tile[source_glyph];
                        int a_sheet_idx = GHApp.TileSheetIdx(atile);
                        int at_x = GHApp.TileSheetX(atile);
                        int at_y = GHApp.TileSheetY(atile);

                        SKRect source_rt = new SKRect();
                        source_rt.Left = at_x + src_x;
                        source_rt.Right = source_rt.Left + item_width;
                        source_rt.Top = at_y + src_y;
                        source_rt.Bottom = source_rt.Top + item_height;

                        float target_x = tx + dest_x;
                        float target_y = ty + dest_y;
                        float target_width = targetscale * scale * source_rt.Width;
                        float target_height = targetscale * scale * source_rt.Height;
                        SKRect target_rt;
                        target_rt = new SKRect(0, 0, target_width, target_height);

                        using (new SKAutoCanvasRestore(canvas, true))
                        {
                            canvas.Translate(target_x, target_y);
                            canvas.Scale(1, 1, 0, 0);
                            paint.Color = paint.Color.WithAlpha((byte)(0xFF * opaqueness));
                            canvas.DrawBitmap(TileMap[a_sheet_idx], source_rt, target_rt, paint);
                        }

                        cnt++;
                    }
                }
                else if (GHApp._autodraws[autodraw].draw_type == (int)autodraw_drawing_types.AUTODRAW_DRAW_LARGE_FIVE_BRANCHED_CANDELABRUM_CANDLES && (otmp_round != null || layer_idx == (int)layer_types.LAYER_MISSILE))
                {
                    float y_start = scaled_y_padding;
                    float x_start = scaled_x_padding;
                    int item_width = 9;
                    int item_height = 31;
                    int src_unlit_x = 0;
                    int src_unlit_y = 0;
                    int src_lit_x = 9 * (1 + (int)GHApp._autodraws[autodraw].flags);
                    int src_lit_y = 0;
                    int cnt = 0;
                    short missile_special_quality = _mapData[mapx, mapy].Layers.missile_special_quality;
                    bool missile_lamplit = (_mapData[mapx, mapy].Layers.missile_flags & (ulong)LayerMissileFlags.MISSILE_FLAGS_LIT) != 0;

                    for (int cidx = 0; cidx < (otmp_round != null ? Math.Min((short)otmp_round.OtypData.special_quality, otmp_round.ObjData.special_quality) : missile_special_quality); cidx++)
                    {
                        int src_x = 0, src_y = 0;
                        float dest_x = 0, dest_y = 0;
                        if (otmp_round != null ? otmp_round.LampLit : missile_lamplit)
                        {
                            src_x = src_lit_x;
                            src_y = src_lit_y;
                        }
                        else
                        {
                            src_x = src_unlit_x;
                            src_y = src_unlit_y;
                        }

                        switch (cidx)
                        {
                            case 0:
                                dest_x = x_start + ((float)(29) * scale * targetscale);
                                dest_y = y_start + ((float)(0) * scale * targetscale);
                                break;
                            case 1:
                                dest_x = x_start + ((float)(18) * scale * targetscale);
                                dest_y = y_start + ((float)(4) * scale * targetscale);
                                break;
                            case 2:
                                dest_x = x_start + ((float)(40) * scale * targetscale);
                                dest_y = y_start + ((float)(3) * scale * targetscale);
                                break;
                            case 3:
                                dest_x = x_start + ((float)(8) * scale * targetscale);
                                dest_y = y_start + ((float)(14) * scale * targetscale);
                                break;
                            case 4:
                                dest_x = x_start + ((float)(50) * scale * targetscale);
                                dest_y = y_start + ((float)(15) * scale * targetscale);
                                break;
                            default:
                                break;
                        }

                        int source_glyph = GHApp._autodraws[autodraw].source_glyph;
                        int atile = GHApp.Glyph2Tile[source_glyph];
                        int a_sheet_idx = GHApp.TileSheetIdx(atile);
                        int at_x = GHApp.TileSheetX(atile);
                        int at_y = GHApp.TileSheetY(atile);

                        SKRect source_rt = new SKRect();
                        source_rt.Left = at_x + src_x;
                        source_rt.Right = source_rt.Left + item_width;
                        source_rt.Top = at_y + src_y;
                        source_rt.Bottom = source_rt.Top + item_height;

                        float target_x = tx + dest_x;
                        float target_y = ty + dest_y;
                        float target_width = targetscale * scale * source_rt.Width;
                        float target_height = targetscale * scale * source_rt.Height;
                        SKRect target_rt;
                        target_rt = new SKRect(0, 0, target_width, target_height);

                        using (new SKAutoCanvasRestore(canvas, true))
                        {
                            canvas.Translate(target_x, target_y);
                            canvas.Scale(1, 1, 0, 0);
                            paint.Color = paint.Color.WithAlpha((byte)(0xFF * opaqueness));
                            canvas.DrawBitmap(TileMap[a_sheet_idx], source_rt, target_rt, paint);
                        }
                        cnt++;
                    }
                }
                else if (GHApp._autodraws[autodraw].draw_type == (int)autodraw_drawing_types.AUTODRAW_DRAW_JAR_CONTENTS && otmp_round != null)
                {
                    short max_charge = otmp_round.OtypData.max_charges;
                    double fill_percentage = (max_charge > 0 ? (double)otmp_round.ObjData.charges / (double)max_charge : 0.0);
                    if (fill_percentage >= 0.0)
                    {
                        SKRect source_rt = new SKRect();
                        SKRect target_rt = new SKRect();
                        float jar_width = GHConstants.TileWidth;
                        float jar_height = GHConstants.TileHeight / 2;

                        float dest_x, dest_y;
                        dest_x = tx + scaled_x_padding;
                        dest_y = ty + scaled_y_padding + extra_y_padding;

                        /* First, background */
                        int source_glyph = GHApp._autodraws[autodraw].source_glyph;
                        int atile = GHApp.Glyph2Tile[source_glyph];
                        int a_sheet_idx = GHApp.TileSheetIdx(atile);
                        int at_x = GHApp.TileSheetX(atile);
                        int at_y = GHApp.TileSheetY(atile);

                        int source_glyph2 = GHApp._autodraws[autodraw].source_glyph2;
                        int atile2 = GHApp.Glyph2Tile[source_glyph2];
                        int a2_sheet_idx = GHApp.TileSheetIdx(atile2);
                        int a2t_x = GHApp.TileSheetX(atile2);
                        int a2t_y = GHApp.TileSheetY(atile2);

                        source_rt.Left = at_x;
                        source_rt.Right = source_rt.Left + jar_width;
                        source_rt.Top = at_y;
                        source_rt.Bottom = source_rt.Top + jar_height;

                        target_rt.Left = 0;
                        target_rt.Right = jar_width * scale * targetscale;
                        target_rt.Top = 0;
                        target_rt.Bottom = jar_height * scale * targetscale;

                        using (new SKAutoCanvasRestore(canvas, true))
                        {
                            canvas.Translate(dest_x, dest_y);
                            paint.Color = paint.Color.WithAlpha((byte)(0xFF * opaqueness));
                            canvas.DrawBitmap(TileMap[a_sheet_idx], source_rt, target_rt, paint);
                        }

                        /* Color */
                        ulong draw_color = GHApp._autodraws[autodraw].parameter1;
                        byte blue = (byte)(draw_color & 0xFFUL);
                        byte green = (byte)((draw_color & 0xFF00UL) >> 8);
                        byte red = (byte)((draw_color & 0xFF0000UL) >> 16);
                        SKColor fillcolor = new SKColor(red, green, blue);

                        double semi_transparency;
                        SKBlendMode oldbm;
                        if (fill_percentage > 0.0)
                        {
                            /* Second, contents */
                            source_rt.Left = at_x;
                            source_rt.Right = source_rt.Left + GHConstants.TileWidth;
                            source_rt.Top = at_y + GHConstants.TileHeight / 2;
                            source_rt.Bottom = source_rt.Top + GHConstants.TileHeight / 2;
                            float source_width = source_rt.Right - source_rt.Left;
                            float source_height = source_rt.Bottom - source_rt.Top;

                            target_rt.Left = 0;
                            target_rt.Right = source_width;
                            target_rt.Top = 0;
                            target_rt.Bottom = source_height;

                            /* Draw to _paintBitmap */
                            semi_transparency = 0.0;

                            SavedAutodrawBitmap cachekey = new SavedAutodrawBitmap(autodraw, fill_percentage, 0);
                            SKBitmap usedContentsBitmap;
                            SKBitmap cachedBitmap;
                            if (_savedAutoDrawBitmaps.TryGetValue(cachekey, out cachedBitmap) && cachedBitmap != null)
                            {
                                usedContentsBitmap = cachedBitmap;
                            }
                            else
                            {
                                oldbm = paint.BlendMode;
                                using (SKCanvas _paintCanvas = new SKCanvas(_paintBitmap))
                                {
                                    _paintCanvas.Clear(SKColors.Transparent);
                                    paint.Color = SKColors.Black.WithAlpha((byte)(0xFF * (1 - semi_transparency)));
                                    _paintCanvas.DrawBitmap(TileMap[a_sheet_idx], source_rt, target_rt, paint);
                                    if ((GHApp._autodraws[autodraw].parameter3 & 1) != 0)
                                    {
                                        paint.BlendMode = SKBlendMode.Modulate;
                                        paint.Color = fillcolor;
                                        _paintCanvas.DrawRect(target_rt, paint);
                                    }
                                }
                                paint.BlendMode = oldbm;
                                paint.Color = SKColors.Black;
                                if (!_savedAutoDrawBitmaps.ContainsKey(cachekey))
                                {
                                    try
                                    {
                                        if (_savedAutoDrawBitmaps.Count >= GHConstants.MaxBitmapCacheSize)
                                        {
                                            foreach (SKBitmap bmp in _savedAutoDrawBitmaps.Values)
                                                bmp.Dispose();
                                            _savedAutoDrawBitmaps.Clear(); /* Clear the whole dictonary for the sake of ease; should almost never happen normally anyway */
                                        }

                                        SKBitmap newbmp = new SKBitmap(GHConstants.TileWidth, GHConstants.TileHeight);
                                        _paintBitmap.CopyTo(newbmp);
                                        newbmp.SetImmutable();
                                        _savedAutoDrawBitmaps.Add(cachekey, newbmp);
                                        usedContentsBitmap = newbmp;
                                    }
                                    catch (Exception ex)
                                    {
                                        Debug.WriteLine(ex.Message);
                                        usedContentsBitmap = _paintBitmap;
                                    }
                                }
                                else
                                    usedContentsBitmap = _paintBitmap;
                            }

                            /* Bottom contents */
                            int bottom_x = 20; // 21;
                            int bottom_y = 40; // 38;
                            int bottom_width = 23; // 21;
                            int bottom_height = 7; // 10;

                            int bottom_tx = 20; // 21;
                            int bottom_ty = 38; // 35;

                            source_rt.Left = bottom_x;
                            source_rt.Right = source_rt.Left + bottom_width;
                            source_rt.Top = bottom_y;
                            source_rt.Bottom = source_rt.Top + bottom_height;

                            target_rt.Left = bottom_tx * scale * targetscale;
                            target_rt.Right = (bottom_tx + bottom_width) * scale * targetscale;
                            target_rt.Top = bottom_ty * scale * targetscale;
                            target_rt.Bottom = (bottom_ty + bottom_height) * scale * targetscale;

                            semi_transparency = 0.0;
                            paint.Color = SKColors.Black.WithAlpha((byte)(0xFF * (1 - semi_transparency)));
                            using (new SKAutoCanvasRestore(canvas, true))
                            {
                                canvas.Translate(dest_x, dest_y);
                                canvas.DrawBitmap(usedContentsBitmap, source_rt, target_rt, paint);
                            }

                            /* Middle contents */
                            int full_y = 17; // 11;
                            int empty_y = 38; // 35;
                            int fill_pixel_top = (int)((double)(empty_y - full_y) * (1.0 - fill_percentage)) + full_y;
                            int fill_pixels = empty_y - fill_pixel_top;
                            if (fill_pixels > 0)
                            {
                                int middle_x = 18; // 21;
                                int middle_y = 15; // 15;
                                int middle_width = 27; //  21;
                                int middle_height = 22; // 17;

                                int middle_tx = 18; // 21;
                                int middle_ty = fill_pixel_top + 4;
                                int middle_twidth = middle_width;
                                int middle_theight = fill_pixels + 1;

                                source_rt.Left = middle_x;
                                source_rt.Right = source_rt.Left + middle_width;
                                source_rt.Top = middle_y;
                                source_rt.Bottom = source_rt.Top + middle_height;

                                target_rt.Left = middle_tx * scale * targetscale;
                                target_rt.Right = (middle_tx + middle_twidth) * scale * targetscale;
                                target_rt.Top = middle_ty * scale * targetscale;
                                target_rt.Bottom = (middle_ty + middle_theight) * scale * targetscale;

                                semi_transparency = 0.2;
                                paint.Color = SKColors.Black.WithAlpha((byte)(0xFF * (1 - semi_transparency)));
                                using (new SKAutoCanvasRestore(canvas, true))
                                {
                                    canvas.Translate(dest_x, dest_y);
                                    canvas.DrawBitmap(usedContentsBitmap, source_rt, target_rt, paint);
                                }

                                /* Top contents */
                                int top_x = 18; // 21;
                                int top_y = 4; // 0;
                                int top_width = 27; // 21;
                                int top_height = 8; // 8;

                                float top_tx_full = 17; // 21;
                                float top_tx_empty = bottom_tx; // 21;
                                float top_tx = top_tx_empty + (float)(top_tx_full - top_tx_empty) * (float)fill_percentage;

                                float top_twidth_full = top_width;
                                float top_twidth_empty = bottom_width; ;
                                float top_twidth = top_twidth_empty + (float)(top_twidth_full - top_twidth_empty) * (float)fill_percentage;

                                float top_ty = fill_pixel_top;
                                float top_theight_full = top_height;
                                float top_theight_empty = bottom_height;
                                float top_theight = top_theight_empty + (float)(top_theight_full - top_theight_empty) * (float)fill_percentage;

                                source_rt.Left = top_x;
                                source_rt.Right = source_rt.Left + top_width;
                                source_rt.Top = top_y;
                                source_rt.Bottom = source_rt.Top + top_height;

                                target_rt.Left = top_tx * scale * targetscale;
                                target_rt.Right = (top_tx + top_twidth) * scale * targetscale;
                                target_rt.Top = top_ty * scale * targetscale;
                                target_rt.Bottom = (top_ty + top_theight) * scale * targetscale;

                                semi_transparency = 0.0;
                                paint.Color = SKColors.Black.WithAlpha((byte)(0xFF * (1 - semi_transparency)));
                                using (new SKAutoCanvasRestore(canvas, true))
                                {
                                    canvas.Translate(dest_x, dest_y);
                                    canvas.DrawBitmap(usedContentsBitmap, source_rt, target_rt, paint);
                                }
                            }
                        }

                        /* Third, transparent foreground */
                        source_rt.Left = a2t_x;
                        source_rt.Right = source_rt.Left + GHConstants.TileWidth;
                        source_rt.Top = a2t_y;
                        source_rt.Bottom = source_rt.Top + GHConstants.TileHeight / 2;

                        target_rt.Left = 0;
                        target_rt.Right = GHConstants.TileWidth * scale * targetscale;
                        target_rt.Top = 0;
                        target_rt.Bottom = GHConstants.TileHeight / 2 * scale * targetscale;

                        /* Draw */
                        semi_transparency = 0.70;
                        paint.Color = SKColors.Black.WithAlpha((byte)(0xFF * (1 - semi_transparency)));
                        using (new SKAutoCanvasRestore(canvas, true))
                        {
                            canvas.Translate(dest_x, dest_y);
                            canvas.DrawBitmap(TileMap[a2_sheet_idx], source_rt, target_rt, paint);
                        }

                        /* Fourth, opaque foreground */
                        paint.Color = SKColors.Black;

                        source_rt.Left = a2t_x;
                        source_rt.Right = source_rt.Left + GHConstants.TileWidth;
                        source_rt.Top = a2t_y + GHConstants.TileHeight / 2;
                        source_rt.Bottom = source_rt.Top + GHConstants.TileHeight / 2;

                        target_rt.Left = 0;
                        target_rt.Right = jar_width;
                        target_rt.Top = 0;
                        target_rt.Bottom = jar_height;

                        /* Draw to _paintBitmap */
                        SavedAutodrawBitmap cachekey2 = new SavedAutodrawBitmap(autodraw, fill_percentage, 1);
                        SKBitmap usedForegroundBitmap;
                        SKBitmap cachedFGBitmap;
                        if (_savedAutoDrawBitmaps.TryGetValue(cachekey2, out cachedFGBitmap) && cachedFGBitmap != null)
                        {
                            usedForegroundBitmap = cachedFGBitmap;
                        }
                        else
                        {
                            oldbm = paint.BlendMode;
                            draw_color = GHApp._autodraws[autodraw].parameter2;
                            blue = (byte)(draw_color & 0xFFUL);
                            green = (byte)((draw_color & 0xFF00UL) >> 8);
                            red = (byte)((draw_color & 0xFF0000UL) >> 16);
                            SKColor capcolor = new SKColor(red, green, blue);
                            using (SKCanvas _paintCanvas = new SKCanvas(_paintBitmap))
                            {
                                _paintCanvas.Clear(SKColors.Transparent);
                                _paintCanvas.DrawBitmap(TileMap[a2_sheet_idx], source_rt, target_rt, paint);
                                paint.Color = capcolor;
                                paint.BlendMode = SKBlendMode.Modulate;
                                _paintCanvas.DrawRect(target_rt, paint);
                            }
                            paint.BlendMode = oldbm;
                            paint.Color = SKColors.Black;
                            if (!_savedAutoDrawBitmaps.ContainsKey(cachekey2))
                            {
                                try
                                {
                                    if (_savedAutoDrawBitmaps.Count >= GHConstants.MaxBitmapCacheSize)
                                    {
                                        foreach(SKBitmap bmp in _savedAutoDrawBitmaps.Values)
                                            bmp.Dispose();
                                        _savedAutoDrawBitmaps.Clear(); /* Clear the whole dictonary for the sake of ease; should almost never happen normally anyway */
                                    }

                                    SKBitmap newbmp = new SKBitmap(GHConstants.TileWidth, GHConstants.TileHeight);
                                    _paintBitmap.CopyTo(newbmp);
                                    newbmp.SetImmutable();
                                    _savedAutoDrawBitmaps.Add(cachekey2, newbmp);
                                    usedForegroundBitmap = newbmp;
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine(ex.Message);
                                    usedForegroundBitmap = _paintBitmap;
                                }
                            }
                            else
                                usedForegroundBitmap = _paintBitmap;
                        }

                        source_rt.Left = 0;
                        source_rt.Right = source_rt.Left + GHConstants.TileWidth;
                        source_rt.Top = 0;
                        source_rt.Bottom = source_rt.Top + GHConstants.TileHeight / 2;

                        target_rt.Left = 0;
                        target_rt.Right = jar_width * scale * targetscale;
                        target_rt.Top = 0;
                        target_rt.Bottom = jar_height * scale * targetscale;

                        using (new SKAutoCanvasRestore(canvas, true))
                        {
                            canvas.Translate(dest_x, dest_y);
                            canvas.DrawBitmap(usedForegroundBitmap, source_rt, target_rt, paint);
                        }

                    }
                    paint.Color = SKColors.Black;
                }

                /*
                * AUTODRAW END
                */
            }

            /* Item property marks */
            if (((layer_idx == (int)layer_types.LAYER_OBJECT || layer_idx == (int)layer_types.LAYER_COVER_OBJECT) && otmp_round != null &&
            (otmp_round.Poisoned || otmp_round.ElementalEnchantment > 0 || otmp_round.MythicPrefix > 0 || otmp_round.MythicSuffix > 0 || otmp_round.Eroded != 0 || otmp_round.Eroded2 != 0 || otmp_round.Exceptionality > 0))
            ||
            ((layer_idx == (int)layer_types.LAYER_MISSILE) &&
                (_mapData[mapx, mapy].Layers.missile_poisoned != 0 || _mapData[mapx, mapy].Layers.missile_elemental_enchantment > 0
                    || _mapData[mapx, mapy].Layers.missile_eroded != 0 || _mapData[mapx, mapy].Layers.missile_eroded2 != 0 ||
                    _mapData[mapx, mapy].Layers.missile_exceptionality > 0 || _mapData[mapx, mapy].Layers.missile_mythic_prefix > 0 || _mapData[mapx, mapy].Layers.missile_mythic_suffix > 0))
            )
            {
                float y_start = scaled_y_padding;
                if (!is_inventory)
                {
                    if (tileflag_halfsize)
                    {
                        y_start += height / 2;
                    }
                    else
                    {
                        if (tileflag_normalobjmissile && !tileflag_fullsizeditem)
                            y_start += height / 4;
                        else
                            y_start += 0;
                    }
                }
                float x_start = scaled_x_padding;
                int mark_width = 8;
                int marks_per_row = GHConstants.TileWidth / mark_width;
                int mark_height = 24;
                int src_x = 0;
                int src_y = 0;
                int cnt = 0;
                bool poisoned = (layer_idx == (int)layer_types.LAYER_MISSILE ? _mapData[mapx, mapy].Layers.missile_poisoned != 0 : otmp_round.Poisoned);
                byte elemental_enchantment = (layer_idx == (int)layer_types.LAYER_MISSILE ? _mapData[mapx, mapy].Layers.missile_elemental_enchantment : otmp_round.ElementalEnchantment);
                byte exceptionality = (layer_idx == (int)layer_types.LAYER_MISSILE ? _mapData[mapx, mapy].Layers.missile_exceptionality : otmp_round.Exceptionality);
                byte mythic_prefix = (layer_idx == (int)layer_types.LAYER_MISSILE ? _mapData[mapx, mapy].Layers.missile_mythic_prefix : otmp_round.MythicPrefix);
                byte mythic_suffix = (layer_idx == (int)layer_types.LAYER_MISSILE ? _mapData[mapx, mapy].Layers.missile_mythic_suffix : otmp_round.MythicSuffix);
                byte eroded = (layer_idx == (int)layer_types.LAYER_MISSILE ? _mapData[mapx, mapy].Layers.missile_eroded : otmp_round.Eroded);
                byte eroded2 = (layer_idx == (int)layer_types.LAYER_MISSILE ? _mapData[mapx, mapy].Layers.missile_eroded2 : otmp_round.Eroded2);
                bool corrodeable = (layer_idx == (int)layer_types.LAYER_MISSILE ? (_mapData[mapx, mapy].Layers.missile_flags & (ulong)LayerMissileFlags.MISSILE_FLAGS_CORRODEABLE) != 0 : otmp_round.OtypData.corrodeable != 0);
                bool rottable = (layer_idx == (int)layer_types.LAYER_MISSILE ? (_mapData[mapx, mapy].Layers.missile_flags & (ulong)LayerMissileFlags.MISSILE_FLAGS_ROTTABLE) != 0 : otmp_round.OtypData.rottable != 0);
                bool flammable = (layer_idx == (int)layer_types.LAYER_MISSILE ? (_mapData[mapx, mapy].Layers.missile_flags & (ulong)LayerMissileFlags.MISSILE_FLAGS_FLAMMABLE) != 0 : otmp_round.OtypData.flammable != 0);
                bool rustprone = (layer_idx == (int)layer_types.LAYER_MISSILE ? (_mapData[mapx, mapy].Layers.missile_flags & (ulong)LayerMissileFlags.MISSILE_FLAGS_RUSTPRONE) != 0 : otmp_round.OtypData.rustprone != 0);
                bool poisonable = (layer_idx == (int)layer_types.LAYER_MISSILE ? (_mapData[mapx, mapy].Layers.missile_flags & (ulong)LayerMissileFlags.MISSILE_FLAGS_POISONABLE) != 0 : otmp_round.OtypData.poisonable != 0);
                float dest_x = 0, dest_y = 0;

                for (item_property_mark_types ipm_idx = 0; ipm_idx < item_property_mark_types.MAX_ITEM_PROPERTY_MARKS; ipm_idx++)
                {
                    if (cnt >= 8)
                        break;

                    src_x = ((int)ipm_idx % marks_per_row) * mark_width;
                    src_y = ((int)ipm_idx / marks_per_row) * mark_height;
                    dest_x = 0;
                    dest_y = 0;

                    switch (ipm_idx)
                    {
                        case item_property_mark_types.ITEM_PROPERTY_MARK_POISONED:
                            if (!(poisoned && poisonable))
                                continue;
                            break;
                        case item_property_mark_types.ITEM_PROPERTY_MARK_DEATH_MAGICAL:
                            if (elemental_enchantment != (byte)elemental_enchantment_types.DEATH_ENCHANTMENT)
                                continue;
                            break;
                        case item_property_mark_types.ITEM_PROPERTY_MARK_FLAMING:
                            if (elemental_enchantment != (byte)elemental_enchantment_types.FIRE_ENCHANTMENT)
                                continue;
                            break;
                        case item_property_mark_types.ITEM_PROPERTY_MARK_FREEZING:
                            if (elemental_enchantment != (byte)elemental_enchantment_types.COLD_ENCHANTMENT)
                                continue;
                            break;
                        case item_property_mark_types.ITEM_PROPERTY_MARK_ELECTRIFIED:
                            if (elemental_enchantment != (byte)elemental_enchantment_types.LIGHTNING_ENCHANTMENT)
                                continue;
                            break;
                        case item_property_mark_types.ITEM_PROPERTY_MARK_EXCEPTIONAL:
                            if (exceptionality != (byte)exceptionality_types.EXCEPTIONALITY_EXCEPTIONAL)
                                continue;
                            break;
                        case item_property_mark_types.ITEM_PROPERTY_MARK_ELITE:
                            if (exceptionality != (byte)exceptionality_types.EXCEPTIONALITY_ELITE)
                                continue;
                            break;
                        case item_property_mark_types.ITEM_PROPERTY_MARK_CELESTIAL:
                            if (exceptionality != (byte)exceptionality_types.EXCEPTIONALITY_CELESTIAL)
                                continue;
                            break;
                        case item_property_mark_types.ITEM_PROPERTY_MARK_PRIMORDIAL:
                            if (exceptionality != (byte)exceptionality_types.EXCEPTIONALITY_PRIMORDIAL)
                                continue;
                            break;
                        case item_property_mark_types.ITEM_PROPERTY_MARK_INFERNAL:
                            if (exceptionality != (byte)exceptionality_types.EXCEPTIONALITY_INFERNAL)
                                continue;
                            break;
                        case item_property_mark_types.ITEM_PROPERTY_MARK_MYTHIC:
                            if ((mythic_prefix == 0 && mythic_suffix == 0) || (mythic_prefix > 0 && mythic_suffix > 0))
                                continue;
                            break;
                        case item_property_mark_types.ITEM_PROPERTY_MARK_LEGENDARY:
                            if (mythic_prefix == 0 || mythic_suffix == 0)
                                continue;
                            break;
                        case item_property_mark_types.ITEM_PROPERTY_MARK_CORRODED:
                            if (!(eroded2 == 1 && corrodeable))
                                continue;
                            break;
                        case item_property_mark_types.ITEM_PROPERTY_MARK_ROTTED:
                            if (!(eroded2 == 1 && rottable))
                                continue;
                            break;
                        case item_property_mark_types.ITEM_PROPERTY_MARK_BURNT:
                            if (!(eroded == 1 && flammable))
                                continue;
                            break;
                        case item_property_mark_types.ITEM_PROPERTY_MARK_RUSTY:
                            if (!(eroded == 1 && rustprone))
                                continue;
                            break;
                        case item_property_mark_types.ITEM_PROPERTY_MARK_VERY_CORRODED:
                            if (!(eroded2 == 2 && corrodeable))
                                continue;
                            break;
                        case item_property_mark_types.ITEM_PROPERTY_MARK_VERY_ROTTED:
                            if (!(eroded2 == 2 && rottable))
                                continue;
                            break;
                        case item_property_mark_types.ITEM_PROPERTY_MARK_VERY_BURNT:
                            if (!(eroded == 2 && flammable))
                                continue;
                            break;
                        case item_property_mark_types.ITEM_PROPERTY_MARK_VERY_RUSTY:
                            if (!(eroded == 2 && rustprone))
                                continue;
                            break;
                        case item_property_mark_types.ITEM_PROPERTY_MARK_THOROUGHLY_CORRODED:
                            if (!(eroded2 == 3 && corrodeable))
                                continue;
                            break;
                        case item_property_mark_types.ITEM_PROPERTY_MARK_THOROUGHLY_ROTTED:
                            if (!(eroded2 == 3 && rottable))
                                continue;
                            break;
                        case item_property_mark_types.ITEM_PROPERTY_MARK_THOROUGHLY_BURNT:
                            if (!(eroded == 3 && flammable))
                                continue;
                            break;
                        case item_property_mark_types.ITEM_PROPERTY_MARK_THOROUGHLY_RUSTY:
                            if (!(eroded == 3 && rustprone))
                                continue;
                            break;
                        case item_property_mark_types.MAX_ITEM_PROPERTY_MARKS:
                        default:
                            continue;
                    }

                    int item_xpos = ((int)GHConstants.TileWidth) / 2 - mark_width + (cnt % 2 != 0 ? 1 : -1) * ((cnt + 1) / 2) * mark_width;

                    dest_y = y_start + scaled_tile_height / 2 - (targetscale * scale * (float)(mark_height / 2));
                    dest_x = x_start + (targetscale * scale * (float)item_xpos);

                    int source_glyph = (int)game_ui_tile_types.ITEM_PROPERTY_MARKS + GHApp.UITileOff;
                    int atile = GHApp.Glyph2Tile[source_glyph];
                    int a_sheet_idx = GHApp.TileSheetIdx(atile);
                    int at_x = GHApp.TileSheetX(atile);
                    int at_y = GHApp.TileSheetY(atile);

                    SKRect source_rt = new SKRect();
                    source_rt.Left = at_x + src_x;
                    source_rt.Right = source_rt.Left + mark_width;
                    source_rt.Top = at_y + src_y;
                    source_rt.Bottom = source_rt.Top + mark_height;

                    SKRect target_rt = new SKRect();

                    target_rt.Left = tx + dest_x;
                    target_rt.Right = target_rt.Left + targetscale * scale * source_rt.Width;
                    target_rt.Top = ty + dest_y;
                    target_rt.Bottom = target_rt.Top + targetscale * scale * source_rt.Height;

#if GNH_MAP_PROFILING && DEBUG
                    StartProfiling(GHProfilingStyle.Bitmap);
#endif
                    canvas.DrawBitmap(TileMap[a_sheet_idx], source_rt, target_rt);
#if GNH_MAP_PROFILING && DEBUG
                    StopProfiling(GHProfilingStyle.Bitmap);
#endif

                    cnt++;
                }
            }
        }

        private void DrawChain(SKCanvas canvas, SKPaint paint, int mapx, int mapy, int autodraw, bool autodraw_u_punished, float width, float height, float ty, float tx, float scale, float targetscale)
        {
            int u_x;
            int u_y;
            lock (_uLock)
            {
                u_x = _ux;
                u_y = _uy;
            }
            if (_uChain != null && _uBall != null && (_mapData[mapx, mapy].Layers.layer_flags & (ulong)LayerFlags.LFLAGS_CAN_SEE) != 0)
            {
                int chain_x = _uChain.OtypData.obj_loc_x;
                int chain_y = _uChain.OtypData.obj_loc_y;
                int ball_x = _uBall.OtypData.obj_loc_x;
                int ball_y = _uBall.OtypData.obj_loc_y;
                if (GHUtils.isok(u_x, u_y) && GHUtils.isok(chain_x, chain_y) && GHUtils.isok(ball_x, ball_y))
                {
                    bool is_chain = (GHApp._autodraws[autodraw].draw_type == (int)autodraw_drawing_types.AUTODRAW_DRAW_CHAIN);
                    int chain_u_dx = (int)(u_x - chain_x);
                    int chain_u_dy = (int)(u_y - chain_y);
                    int chain_ball_dx = (int)(ball_x - chain_x);
                    int chain_ball_dy = (int)(ball_y - chain_y);
                    int u_ball_dx = (int)(u_x - chain_x);
                    int u_ball_dy = (int)(u_y - chain_y);

                    int source_glyph = autodraw_u_punished || autodraw == 0 ? (int)game_ui_tile_types.ITEM_AUTODRAW_GRAPHICS + GHApp.UITileOff : GHApp._autodraws[autodraw].source_glyph;
                    int dir_idx = GHApp._autodraws[autodraw].flags;
                    int atile = GHApp.Glyph2Tile[source_glyph];
                    int a_sheet_idx = GHApp.TileSheetIdx(atile);
                    int at_x = GHApp.TileSheetX(atile);
                    int at_y = GHApp.TileSheetY(atile);
                    float adscale = scale * targetscale;

                    for (int n = 0; n < 2; n++)
                    {
                        int relevant_dx = autodraw_u_punished ? Math.Sign(n == 0 ? -chain_u_dx : 0) : is_chain ? Math.Sign(n == 0 ? chain_u_dx : chain_ball_dx) : Math.Sign(n == 0 ? 0 : -chain_ball_dx);
                        int relevant_dy = autodraw_u_punished ? Math.Sign(n == 0 ? -chain_u_dy : 0) : is_chain ? Math.Sign(n == 0 ? chain_u_dy : chain_ball_dy) : Math.Sign(n == 0 ? 1 : -chain_ball_dy);
                        bool hflip_link = !((relevant_dx > 0) != (relevant_dy > 0));
                        bool vflip_link = false;
                        int link_source_width = 16;
                        int link_source_height = 16;
                        float link_diff_x = relevant_dx != 0 && relevant_dy != 0 ? 5.35f : 10.0f;
                        float link_diff_y = relevant_dx != 0 && relevant_dy != 0 ? link_diff_x * 1.5f : 10.0f;
                        int mid_x = GHConstants.TileWidth / 2;
                        int mid_y = GHConstants.TileHeight / 2;
                        int dist_x = relevant_dx > 0 ? GHConstants.TileWidth - mid_x : mid_x;
                        int dist_y = relevant_dy > 0 ? GHConstants.TileHeight - mid_y : mid_y;
                        int ball_scale_additional_dist_y = (int)((float)(GHConstants.TileHeight / 2) * (1.0f - scale));
                        int ball_additional_scale_links = (int)Math.Ceiling((float)ball_scale_additional_dist_y / (float)(link_source_height / 2));
                        int links = 2 + 1 + ball_additional_scale_links + (int)Math.Min((float)(dist_y - link_source_height / 2) / link_diff_y, (float)(dist_x - link_source_width / 2) / link_diff_x);

                        if (!is_chain && !autodraw_u_punished && n == 0 && links > 1)
                            links = 1 + ball_additional_scale_links;
                        else if (autodraw_u_punished && n == 1 && links > 1)
                            links = 1;

                        if (dir_idx == 0)
                        {
                            if (relevant_dx != 0 || relevant_dy != 0)
                            {
                                for (int m = 0; m < links; m++)
                                {
                                    bool used_hflip_link = hflip_link;
                                    if (m >= links && (relevant_dx < 0 || relevant_dy < 0))
                                        used_hflip_link = !((-relevant_dx > 0) != (-relevant_dy > 0));

                                    int source_width = link_source_width;
                                    int source_height = link_source_height;
                                    int within_tile_source_x = relevant_dx != 0 && relevant_dy != 0 ? 32 : relevant_dy != 0 ? 16 : 0;
                                    int within_tile_source_y = 23 + ((m % 2) == 1 ? link_source_height : 0);
                                    float target_left_added = width / 2 - ((float)source_width * adscale / 2.0f) + (((float)relevant_dx * link_diff_x * (float)m) * adscale);
                                    float target_top_added = height / 2 - ((float)source_height * adscale / 2.0f) + (((float)relevant_dy * link_diff_y * (float)m) * adscale);
                                    if (target_left_added < 0)
                                    {
                                        /* Cut off from left ==> Move source x right and reduce width to fix, flipped: just reduce width */
                                        if (!used_hflip_link)
                                            within_tile_source_x += (int)((float)-target_left_added / adscale);

                                        source_width -= (int)(-target_left_added / adscale);
                                        if (source_width <= 0)
                                            continue;
                                        target_left_added = 0;
                                    }
                                    if (target_top_added < 0)
                                    {
                                        within_tile_source_y += (int)((float)-target_top_added / adscale);
                                        source_height -= (int)((float)-target_top_added / adscale);
                                        if (source_height <= 0)
                                            continue;
                                        target_top_added = 0;
                                    }
                                    float target_x = tx + target_left_added;
                                    float target_y = ty + target_top_added;
                                    float target_width = ((float)source_width * adscale);
                                    float target_height = ((float)source_height * adscale);
                                    if (target_x + target_width > tx + width)
                                    {
                                        /* Cut off from right ==>Just reduce width to fix, flipped: Move source x right and reduce width to fix */
                                        int source_diff = (int)((target_x + target_width - (tx + width)) / adscale);
                                        if (used_hflip_link)
                                            within_tile_source_x += source_diff;

                                        source_width -= source_diff;
                                        if (source_width <= 0)
                                            continue;
                                        target_width -= (target_x + target_width - (tx + width));
                                    }
                                    if (target_y + target_height > (ty + height))
                                    {
                                        int source_diff = (int)((target_y + target_height - (ty + height)) / adscale);
                                        source_height -= source_diff;
                                        if (source_height <= 0)
                                            continue;
                                        target_height -= (target_y + target_height - (ty + height));
                                    }

                                    int source_x = at_x + within_tile_source_x;
                                    int source_y = at_y + within_tile_source_y;
                                    SKRect sourcerect = new SKRect(source_x, source_y, source_x + source_width, source_y + source_height);
                                    SKRect targetrect = new SKRect(0, 0, target_width, target_height);
                                    using (SKAutoCanvasRestore autorestore = new SKAutoCanvasRestore(canvas))
                                    {
                                        canvas.Translate(target_x + (hflip_link ? target_width : 0), target_y + (vflip_link ? target_height : 0));
                                        canvas.Scale(hflip_link ? -1 : 1, vflip_link ? -1 : 1, 0, 0);
                                        canvas.DrawBitmap(TileMap[a_sheet_idx], sourcerect, targetrect, paint);
                                    }
                                }
                            }
                        }
                        else if (dir_idx > 0)
                        {
                            if (relevant_dx != 0 && relevant_dy != 0)
                            {
                                int added_source_x = 0, added_source_y = 0;
                                float added_target_x = 0, added_target_y = 0;
                                bool draw_link = false;

                                if (relevant_dx < 0 && relevant_dy < 0)
                                {
                                    if (dir_idx == 2)
                                    {
                                        added_source_x = 8;
                                        added_source_y = 8;
                                        added_target_x = GHConstants.TileWidth - 8;
                                        added_target_y = 0;
                                        draw_link = true;
                                    }
                                    else if (dir_idx == 3)
                                    {
                                        added_source_x = 0;
                                        added_source_y = 0;
                                        added_target_x = 0;
                                        added_target_y = GHConstants.TileHeight - 8;
                                        draw_link = true;
                                    }
                                }
                                else if (relevant_dx > 0 && relevant_dy < 0)
                                {
                                    if (dir_idx == 4)
                                    {
                                        added_source_x = 8;
                                        added_source_y = 8;
                                        added_target_x = 0;
                                        added_target_y = 0;
                                        draw_link = true;
                                    }
                                    else if (dir_idx == 3)
                                    {
                                        added_source_x = 0;
                                        added_source_y = 0;
                                        added_target_x = GHConstants.TileWidth - 8;
                                        added_target_y = GHConstants.TileHeight - 8;
                                        draw_link = true;
                                    }
                                }
                                else if (relevant_dx < 0 && relevant_dy > 0)
                                {
                                    if (dir_idx == 2)
                                    {
                                        added_source_x = 0;
                                        added_source_y = 0;
                                        added_target_x = GHConstants.TileWidth - 8;
                                        added_target_y = GHConstants.TileHeight - 8;
                                        draw_link = true;
                                    }
                                    else if (dir_idx == 1)
                                    {
                                        added_source_x = 8;
                                        added_source_y = 8;
                                        added_target_x = 0;
                                        added_target_y = 0;
                                        draw_link = true;
                                    }
                                }
                                else if (relevant_dx > 0 && relevant_dy > 0)
                                {
                                    if (dir_idx == 4)
                                    {
                                        added_source_x = 0;
                                        added_source_y = 0;
                                        added_target_x = 0;
                                        added_target_y = GHConstants.TileHeight - 8;
                                        draw_link = true;
                                    }
                                    else if (dir_idx == 1)
                                    {
                                        added_source_x = 8;
                                        added_source_y = 8;
                                        added_target_x = GHConstants.TileWidth - 8;
                                        added_target_y = 0;
                                        draw_link = true;
                                    }
                                }
                                if (draw_link)
                                {
                                    int source_width = 8;
                                    int source_height = 8;
                                    int within_tile_source_x = 32 + added_source_x;
                                    int within_tile_source_y = 23 + ((links + 1 % 2) == 1 ? link_source_height : 0) + added_source_y;
                                    float target_x = tx + ((float)(added_target_x) * adscale);
                                    float target_y = ty + ((float)(added_target_y) * adscale);
                                    float target_width = ((float)source_width * adscale);
                                    float target_height = ((float)source_height * adscale);
                                    int source_x = at_x + within_tile_source_x;
                                    int source_y = at_y + within_tile_source_y;

                                    SKRect sourcerect = new SKRect(source_x, source_y, source_x + source_width, source_y + source_height);
                                    SKRect targetrect = new SKRect(0, 0, target_width, target_height);
                                    using (SKAutoCanvasRestore autorestore = new SKAutoCanvasRestore(canvas))
                                    {
                                        canvas.Translate(target_x + (hflip_link ? target_width : 0), target_y + (vflip_link ? target_height : 0));
                                        canvas.Scale(hflip_link ? -1 : 1, vflip_link ? -1 : 1, 0, 0);
                                        canvas.DrawBitmap(TileMap[a_sheet_idx], sourcerect, targetrect, paint);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public double CurrentPageWidth { get { return _currentPageWidth; } }
        public double CurrentPageHeight { get { return _currentPageHeight; } }

        private double _currentPageWidth = 0;
        private double _currentPageHeight = 0;

        private readonly object _isLandScapeLock = new object();
        private bool _isLandScape = false;
        public bool IsLandscape { get { lock (_isLandScapeLock) { return _isLandScape; } } set { lock (_isLandScapeLock) { _isLandScape = value; } } }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            if (width != _currentPageWidth || height != _currentPageHeight)
            {
                _currentPageWidth = width;
                _currentPageHeight = height;

                IsLandscape = width > height;

                if (TipView.IsVisible)
                    TipView.InvalidateSurface();

                GameMenuButton.SetSideSize(width, height);
                ESCButton.SetSideSize(width, height);
                ToggleAutoCenterModeButton.SetSideSize(width, height);
                LookModeButton.SetSideSize(width, height);
                ToggleTravelModeButton.SetSideSize(width, height);
                ToggleZoomMiniButton.SetSideSize(width, height);
                ToggleZoomAlternateButton.SetSideSize(width, height);

                SimpleGameMenuButton.SetSideSize(width, height);
                SimpleESCButton.SetSideSize(width, height);
                SimpleToggleAutoCenterModeButton.SetSideSize(width, height);
                SimpleLookModeButton.SetSideSize(width, height);
                SimpleToggleZoomMiniButton.SetSideSize(width, height);

                ZeroButton.SetSideSize(width, height);
                FirstButton.SetSideSize(width, height);
                SecondButton.SetSideSize(width, height);
                ThirdButton.SetSideSize(width, height);
                FourthButton.SetSideSize(width, height);

                lAbilitiesButton.SetSideSize(width, height);
                lWornItemsButton.SetSideSize(width, height);
                double statusbarheight = GetStatusBarHeight();
                lAbilitiesButton.HeightRequest = statusbarheight;
                lWornItemsButton.HeightRequest = statusbarheight;
                //lSkillButton.SetSideSize(width, height);

                UpperCmdLayout.Margin = new Thickness(0, statusbarheight, 0, 0);
                SimpleUpperCmdLayout.Margin = new Thickness(0, statusbarheight, 0, 0);

                foreach (View v in UpperCmdGrid.Children)
                {
                    LabeledImageButton lib = (LabeledImageButton)v;
                    lib.SetSideSize(width, height);
                }
                foreach (View v in LowerCmdGrid.Children)
                {
                    LabeledImageButton lib = (LabeledImageButton)v;
                    lib.SetSideSize(width, height);
                }
                foreach (View v in SimpleCmdGrid.Children)
                {
                    LabeledImageButton lib = (LabeledImageButton)v;
                    lib.SetSideSize(width, height);
                }

                LabeledImageButton firstchild = (LabeledImageButton)UpperCmdGrid.Children[0];
                UpperCmdGrid.HeightRequest = firstchild.GridHeight;
                LowerCmdGrid.HeightRequest = firstchild.GridHeight;

                LabeledImageButton simplefirstchild = (LabeledImageButton)SimpleCmdGrid.Children[0];
                SimpleCmdGrid.HeightRequest = simplefirstchild.GridHeight;

                MenuHeaderLabel.Margin = UIUtils.GetHeaderMarginWithBorder(MenuBackground.BorderStyle, width, height);
                MenuCloseGrid.Margin = UIUtils.GetFooterMarginWithBorder(MenuBackground.BorderStyle, width, height);
                Thickness smallthick = UIUtils.GetSmallBorderThickness(width, height, 1.5);
                TextCanvas.Margin = smallthick;
                TextWindowGlyphImage.Margin = smallthick;
                Thickness subthick = smallthick;
                subthick.Top = MenuSubtitleLabel.Margin.Top;
                subthick.Bottom = MenuSubtitleLabel.Margin.Bottom;
                MenuSubtitleLabel.Margin = subthick;
                Thickness glyphthick = smallthick;
                glyphthick.Top = MenuWindowGlyphImage.Margin.Top;
                glyphthick.Bottom = MenuWindowGlyphImage.Margin.Bottom;
                MenuWindowGlyphImage.Margin = glyphthick;

                lock (_mapOffsetLock)
                {
                    _statusOffsetY = 0;
                }
                lock (_messageScrollLock)
                {
                    _messageScrollOffset = 0;
                }
                

                if (width > height)
                {
                    /* Landscape */
                    ButtonRowStack.Orientation = StackOrientation.Horizontal;
                    ModeLayout.Orientation = StackOrientation.Vertical;
                    ModeSubLayout1.Orientation = StackOrientation.Horizontal;
                    ModeSubLayout2.Orientation = StackOrientation.Horizontal;
                    GameMenuLayout.Orientation = StackOrientation.Horizontal;
                    //UpperCmdLayout.Orientation = StackOrientation.Vertical;

                    SimpleButtonRowStack.Orientation = StackOrientation.Horizontal;
                    SimpleModeLayout.Orientation = StackOrientation.Vertical;
                    SimpleModeSubLayout1.Orientation = StackOrientation.Horizontal;
                    SimpleModeSubLayout2.Orientation = StackOrientation.Horizontal;
                    SimpleGameMenuLayout.Orientation = StackOrientation.Horizontal;
                }
                else
                {
                    /* Portrait */
                    ButtonRowStack.Orientation = StackOrientation.Vertical;
                    ModeLayout.Orientation = StackOrientation.Vertical;
                    ModeSubLayout1.Orientation = StackOrientation.Vertical;
                    ModeSubLayout2.Orientation = StackOrientation.Vertical;
                    GameMenuLayout.Orientation = StackOrientation.Horizontal;
                    //UpperCmdLayout.Orientation = StackOrientation.Horizontal;

                    SimpleButtonRowStack.Orientation = StackOrientation.Vertical;
                    SimpleModeLayout.Orientation = StackOrientation.Vertical;
                    SimpleModeSubLayout1.Orientation = StackOrientation.Vertical;
                    SimpleModeSubLayout2.Orientation = StackOrientation.Vertical;
                    SimpleGameMenuLayout.Orientation = StackOrientation.Horizontal;
                }

                RefreshMenuRowCounts = true;
                RefreshMsgHistoryRowCounts = true;
                IsSizeAllocatedProcessed = true;
            }
        }

        public float GetStatusBarSkiaHeight()
        {
            float statusbarheight;
            using (SKPaint textPaint = new SKPaint())
            {
                textPaint.Typeface = GHApp.LatoRegular;
                textPaint.TextSize = _statusbar_basefontsize * GetTextScale();
                float rowheight = textPaint.FontSpacing;
                statusbarheight = rowheight * 2 + _statusbar_vmargin * 2 + _statusbar_rowmargin;
            }
            return statusbarheight;
        }
        public double GetCanvasScale()
        {
            if (canvasView.CanvasSize.Width <= 0 || canvasView.CanvasSize.Height <= 0)
                return 1.0;
            return Math.Sqrt(canvasView.Width / (double)canvasView.CanvasSize.Width * canvasView.Height / (double)canvasView.CanvasSize.Height);
        }

        public double GetStatusBarHeight()
        {
            double sb_xheight;
            float statusbarheight = GetStatusBarSkiaHeight();
            double scale = GetCanvasScale();
            sb_xheight = scale * (double)statusbarheight;
            return sb_xheight;
        }

        private Dictionary<long, TouchEntry> TouchDictionary = new Dictionary<long, TouchEntry>();
        private readonly object _mapOffsetLock = new object();
        public float _mapOffsetX = 0;
        public float _mapOffsetY = 0;
        public float _mapMiniOffsetX = 0;
        public float _mapMiniOffsetY = 0;
        public float _statusOffsetY = 0;
        public float _statusLargestBottom = 0;
        public float _statusClipBottom = 0;
        private bool _touchMoved = false;
        private bool _touchWithinSkillButton = false;
        private bool _touchWithinHealthOrb = false;
        private bool _touchWithinManaOrb = false;
        private bool _touchWithinStatusBar = false;
        private uint _touchWithinPet = 0;
        private bool _touchWithinYouButton = false;
        private object _savedSender = null;
        private SKTouchEventArgs _savedEventArgs = null;

        private readonly object _messageScrollLock = new object();
        public float _messageScrollOffset = 0;
        public float _messageSmallestTop = 0;
        private float _messageScrollSpeed = 0; /* pixels per second */
        private bool _messageScrollSpeedRecordOn = false;
        private DateTime _messageScrollSpeedStamp;
        List<TouchSpeedRecord> _messageScrollSpeedRecords = new List<TouchSpeedRecord>();
        private bool _messageScrollSpeedOn = false;
        private DateTime _messageScrollSpeedReleaseStamp;


        private void canvasView_Touch(object sender, SKTouchEventArgs e)
        {
            canvasView_Touch_MainPage(sender, e);
        }

        private void canvasView_Touch_MainPage(object sender, SKTouchEventArgs e)
        {
            if (_currentGame != null)
            {
                lock (TargetClipLock)
                {
                    _targetClipOn = false;
                }

                switch (e?.ActionType)
                {
                    case SKTouchAction.Entered:
                        break;
                    case SKTouchAction.Pressed:
                        _savedSender = null;
                        _savedEventArgs = null;
                        _touchWithinSkillButton = false;
                        _touchWithinHealthOrb = false;
                        _touchWithinManaOrb = false;
                        _touchWithinStatusBar = false;
                        _touchWithinPet = 0;
                        _touchWithinYouButton = false;

                        if (TouchDictionary.ContainsKey(e.Id))
                            TouchDictionary[e.Id] = new TouchEntry(e.Location, DateTime.Now);
                        else
                            TouchDictionary.Add(e.Id, new TouchEntry(e.Location, DateTime.Now));

                        if (TouchDictionary.Count > 1)
                            _touchMoved = true;
                        else if (!ForceAllMessages && !ShowExtendedStatusBar)
                        {
                            uint m_id = 0;
                            if (SkillRect.Contains(e.Location))
                            {
                                _touchWithinSkillButton = true;
                            }
                            else if (HealthRect.Contains(e.Location))
                            {
                                _touchWithinHealthOrb = true;
                            }
                            else if (ManaRect.Contains(e.Location))
                            {
                                _touchWithinManaOrb = true;
                            }
                            else if (StatusBarRect.Contains(e.Location))
                            {
                                _touchWithinStatusBar = true;
                            }
                            else if (!_showDirections && !_showNumberPad && ShowPets && (m_id = PetRectContains(e.Location)) != 0)
                            {
                                _touchWithinPet = m_id;
                            }
                            else if (!MapLookMode && !MapTravelMode)
                            {
                                _savedSender = sender;
                                _savedEventArgs = e;
                                Device.StartTimer(TimeSpan.FromSeconds(GHConstants.MoveByHoldingDownThreshold), () =>
                                {
                                    if (_savedSender == null || _savedEventArgs == null)
                                        return false;

                                    IssueNHCommandViaTouch(_savedSender, _savedEventArgs);
                                    return true; /* Continue until cancelled */
                                });
                            }
                        }
                        else if (ShowExtendedStatusBar)
                        {
                            if (YouRect.Contains(e.Location))
                            {
                                _touchWithinYouButton = true;
                            }
                        }
                        else if (ForceAllMessages)
                        {
                            lock (_messageScrollLock)
                            {
                                _messageScrollSpeed = 0;
                                _messageScrollSpeedOn = false;
                                _messageScrollSpeedRecordOn = false;
                                _messageScrollSpeedRecords.Clear();
                            }
                        }
                        e.Handled = true;
                        break;
                    case SKTouchAction.Moved:
                        {
                            TouchEntry entry;
                            bool res = TouchDictionary.TryGetValue(e.Id, out entry);
                            if (res)
                            {
                                SKPoint anchor = entry.Location;

                                float diffX = e.Location.X - anchor.X;
                                float diffY = e.Location.Y - anchor.Y;
                                float dist = (float)Math.Sqrt((Math.Pow(diffX, 2) + Math.Pow(diffY, 2)));

                                if (TouchDictionary.Count == 1)
                                {
                                    if (_touchWithinSkillButton || _touchWithinHealthOrb || _touchWithinManaOrb || _touchWithinStatusBar || (_touchWithinPet > 0 && !_showDirections && !_showNumberPad) || _touchWithinYouButton)
                                    {
                                        /* Do nothing */
                                    }
                                    else if (!MapLookMode && !MapTravelMode && !ForceAllMessages && !ShowExtendedStatusBar)
                                    {
                                        /* Move the save location */
                                        _savedSender = sender;
                                        _savedEventArgs = e;
                                    }
                                    else if (/*!ZoomMiniMode && */ (dist > GHConstants.MoveDistanceThreshold ||
                                        (DateTime.Now.Ticks - entry.PressTime.Ticks) / TimeSpan.TicksPerMillisecond > GHConstants.MoveOrPressTimeThreshold
                                           ))
                                    {
                                        /* Just one finger => Move the map */
                                        if (diffX != 0 || diffY != 0)
                                        {
                                            if (ShowExtendedStatusBar)
                                            {
                                                lock (_mapOffsetLock)
                                                {
                                                    if (diffY < 0)
                                                    {
                                                        if (_statusLargestBottom > _statusClipBottom)
                                                        {
                                                            _statusOffsetY += -Math.Min(_statusLargestBottom - _statusClipBottom, -diffY);
                                                        }
                                                    }
                                                    else
                                                        _statusOffsetY += diffY;

                                                    if (_statusOffsetY > 0)
                                                    {
                                                        _statusOffsetY = 0;
                                                    }
                                                }
                                            }
                                            else if (ForceAllMessages)
                                            {
                                                //lock (_messageScrollLock)
                                                //{
                                                //    if (diffY > 0)
                                                //    {
                                                //        if (_messageSmallestTop < 0)
                                                //        {
                                                //            _messageScrollOffset += Math.Min(-_messageSmallestTop, diffY);
                                                //        }
                                                //    }
                                                //    else
                                                //        _messageScrollOffset += diffY;

                                                //    if (_messageScrollOffset < 0)
                                                //    {
                                                //        _messageScrollOffset = 0;
                                                //    }
                                                //}

                                                DateTime now = DateTime.Now;
                                                /* Do not scroll within button press time threshold, unless large move */
                                                long millisecs_elapsed = (now.Ticks - entry.PressTime.Ticks) / TimeSpan.TicksPerMillisecond;
                                                if (dist > GHConstants.MoveDistanceThreshold || millisecs_elapsed > GHConstants.MoveOrPressTimeThreshold)
                                                {
                                                    lock (_messageScrollLock)
                                                    {
                                                        float topScrollLimit = Math.Max(0, -_messageSmallestTop);
                                                        float stretchLimit = GHConstants.ScrollStretchLimit * canvasView.CanvasSize.Height;
                                                        float stretchConstant = GHConstants.ScrollConstantStretch * canvasView.CanvasSize.Height;
                                                        float adj_factor = 1.0f;
                                                        if (_messageScrollOffset > topScrollLimit)
                                                            adj_factor = _messageScrollOffset >= topScrollLimit + stretchLimit ? 0 : (1 - ((_messageScrollOffset - topScrollLimit + stretchConstant) / (stretchLimit + stretchConstant)));
                                                        else if (_messageScrollOffset < 0)
                                                            adj_factor = _messageScrollOffset < 0 - stretchLimit ? 0 : (1 - ((0 - (_messageScrollOffset - stretchConstant)) / (stretchLimit + stretchConstant)));

                                                        float adj_diffY = diffY * adj_factor;
                                                        _messageScrollOffset += adj_diffY;

                                                        if (_messageScrollOffset > stretchLimit + topScrollLimit)
                                                            _messageScrollOffset = stretchLimit;
                                                        else if (_messageScrollOffset < 0 - stretchLimit)
                                                            _messageScrollOffset = 0 - stretchLimit;
                                                        else
                                                        {
                                                            /* Calculate duration since last touch move */
                                                            float duration = 0;
                                                            if (!_messageScrollSpeedRecordOn)
                                                            {
                                                                duration = (float)millisecs_elapsed / 1000f;
                                                                _messageScrollSpeedRecordOn = true;
                                                            }
                                                            else
                                                            {
                                                                duration = ((float)(now.Ticks - _messageScrollSpeedStamp.Ticks) / TimeSpan.TicksPerMillisecond) / 1000f;
                                                            }
                                                            _messageScrollSpeedStamp = now;

                                                            /* Discard speed records to the opposite direction */
                                                            if (_messageScrollSpeedRecords.Count > 0)
                                                            {
                                                                int prevsgn = Math.Sign(_messageScrollSpeedRecords[0].Distance);
                                                                if (diffY != 0 && prevsgn != 0 && Math.Sign(diffY) != prevsgn)
                                                                    _messageScrollSpeedRecords.Clear();
                                                            }

                                                            /* Add a new speed record */
                                                            _messageScrollSpeedRecords.Insert(0, new TouchSpeedRecord(diffY, duration, now));

                                                            /* Discard too old records */
                                                            while (_messageScrollSpeedRecords.Count > 0)
                                                            {
                                                                long lastrecord_ms = (now.Ticks - _messageScrollSpeedRecords[_messageScrollSpeedRecords.Count - 1].TimeStamp.Ticks) / TimeSpan.TicksPerMillisecond;
                                                                if (lastrecord_ms > GHConstants.ScrollRecordThreshold)
                                                                    _messageScrollSpeedRecords.RemoveAt(_messageScrollSpeedRecords.Count - 1);
                                                                else
                                                                    break;
                                                            }

                                                            /* Sum up the distances and durations of current records to get an average */
                                                            float totaldistance = 0;
                                                            float totalsecs = 0;
                                                            foreach (TouchSpeedRecord r in _messageScrollSpeedRecords)
                                                            {
                                                                totaldistance += r.Distance;
                                                                totalsecs += r.Duration;
                                                            }
                                                            _messageScrollSpeed = totaldistance / Math.Max(0.001f, totalsecs);
                                                            _messageScrollSpeedOn = false;
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                lock (_mapOffsetLock)
                                                {
                                                    if (ZoomMiniMode)
                                                    {
                                                        _mapMiniOffsetX += diffX;
                                                        _mapMiniOffsetY += diffY;
                                                        if (_mapWidth > 0 && Math.Abs(_mapMiniOffsetX) > 1 * _mapWidth)
                                                        {
                                                            _mapMiniOffsetX = 1 * _mapWidth * Math.Sign(_mapMiniOffsetX);
                                                        }
                                                        if (_mapHeight > 0 && Math.Abs(_mapMiniOffsetY) > 1 * _mapHeight)
                                                        {
                                                            _mapMiniOffsetY = 1 * _mapHeight * Math.Sign(_mapMiniOffsetY);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        _mapOffsetX += diffX;
                                                        _mapOffsetY += diffY;
                                                        if (_mapWidth > 0 && Math.Abs(_mapOffsetX) > 10 * _mapWidth)
                                                        {
                                                            _mapOffsetX = 10 * _mapWidth * Math.Sign(_mapOffsetX);
                                                        }
                                                        if (_mapHeight > 0 && Math.Abs(_mapOffsetY) > 10 * _mapHeight)
                                                        {
                                                            _mapOffsetY = 10 * _mapHeight * Math.Sign(_mapOffsetY);
                                                        }
                                                    }
                                                }
                                            }

                                            TouchDictionary[e.Id].Location = e.Location;
                                            _touchMoved = true;
                                        }
                                    }

                                }
                                else if (TouchDictionary.Count == 2)
                                {
                                    _savedSender = null;
                                    _savedEventArgs = null;
                                    _touchWithinSkillButton = false;
                                    _touchWithinHealthOrb = false;
                                    _touchWithinManaOrb = false;
                                    _touchWithinStatusBar = false;
                                    _touchWithinPet = 0;
                                    _touchWithinYouButton = false;

                                    SKPoint prevloc = TouchDictionary[e.Id].Location;
                                    SKPoint curloc = e.Location;
                                    SKPoint otherloc;

                                    Dictionary<long, TouchEntry>.KeyCollection keys = TouchDictionary.Keys;
                                    long other_key = 0;
                                    foreach (long key in keys)
                                    {
                                        if (key != e.Id)
                                        {
                                            other_key = key;
                                            break;
                                        }
                                    }

                                    if (other_key != 0 /* && !ZoomMiniMode */)
                                    {
                                        otherloc = TouchDictionary[other_key].Location;
                                        float prevdist = (float)Math.Sqrt((Math.Pow((double)otherloc.X - (double)prevloc.X, 2) + Math.Pow((double)otherloc.Y - (double)prevloc.Y, 2)));
                                        float curdist = (float)Math.Sqrt((Math.Pow((double)otherloc.X - (double)curloc.X, 2) + Math.Pow((double)otherloc.Y - (double)curloc.Y, 2)));
                                        if (prevdist > 0 && curdist > 0)
                                        {
                                            float ratio = curdist / prevdist;
                                            float curfontsize = ZoomMiniMode ? MapFontMiniRelativeSize : ZoomAlternateMode ? MapFontAlternateSize : MapFontSize;
                                            float newfontsize = curfontsize * ratio;
                                            if(ZoomMiniMode)
                                            {
                                                if (newfontsize > GHConstants.MaximumMapMiniRelativeFontSize)
                                                    newfontsize = GHConstants.MaximumMapMiniRelativeFontSize;
                                                if (newfontsize < GHConstants.MinimumMapMiniRelativeFontSize)
                                                    newfontsize = GHConstants.MinimumMapMiniRelativeFontSize;
                                            }
                                            else
                                            {
                                                if (newfontsize > GHConstants.MaximumMapFontSize)
                                                    newfontsize = GHConstants.MaximumMapFontSize;
                                                if (newfontsize < GHConstants.MinimumMapFontSize)
                                                    newfontsize = GHConstants.MinimumMapFontSize;
                                            }

                                            float newratio = newfontsize / Math.Max(1, curfontsize);
                                            float mapFontAscent = UsedMapFontAscent;
                                            if (ZoomMiniMode)
                                                MapFontMiniRelativeSize = newfontsize;
                                            else if (ZoomAlternateMode)
                                                MapFontAlternateSize = newfontsize;
                                            else
                                                MapFontSize = newfontsize;

                                            if (ZoomMiniMode)
                                            {
                                                lock (_mapOffsetLock)
                                                {
                                                    _mapMiniOffsetX *= newratio;
                                                    _mapMiniOffsetY *= newratio;
                                                    if (_mapWidth > 0 && Math.Abs(_mapMiniOffsetX) > 1 * _mapWidth)
                                                    {
                                                        _mapMiniOffsetX = 1 * _mapWidth * Math.Sign(_mapMiniOffsetX);
                                                    }
                                                    if (_mapHeight > 0 && Math.Abs(_mapMiniOffsetY) > 1 * _mapHeight)
                                                    {
                                                        _mapMiniOffsetY = 1 * _mapHeight * Math.Sign(_mapMiniOffsetY);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                float width = UsedTileWidth;
                                                float height = UsedTileHeight;
                                                float mapwidth = width * (GHConstants.MapCols - 1);
                                                float mapheight = height * (GHConstants.MapRows);
                                                float canvaswidth = canvasView.CanvasSize.Width;
                                                float canvasheight = canvasView.CanvasSize.Height;
                                                float offsetX, offsetY, usedOffsetX, usedOffsetY;
                                                GetMapOffsets(canvaswidth, canvasheight, mapwidth, mapheight, width, height, out offsetX, out offsetY, out usedOffsetX, out usedOffsetY);
                                                float totalOffsetX = offsetX + usedOffsetX;
                                                float totalOffsetY = offsetY + usedOffsetY + mapFontAscent;
                                                SKPoint oldLoc = new SKPoint((prevloc.X + otherloc.X) / 2, (prevloc.Y + otherloc.Y) / 2);
                                                SKPoint newLoc = new SKPoint((curloc.X + otherloc.X) / 2, (curloc.Y + otherloc.Y) / 2);
                                                float newTotalOffsetX = newLoc.X - (oldLoc.X - totalOffsetX) * newratio;
                                                float newTotalOffsetY = newLoc.Y - (oldLoc.Y - totalOffsetY) * newratio;
                                                float newWidth = width * newratio;
                                                float newHeight = height * newratio;
                                                float newMapwidth = newWidth * (GHConstants.MapCols - 1);
                                                float newMapheight = newHeight * (GHConstants.MapRows);
                                                float newMapFontAscent = mapFontAscent * newratio;
                                                float newOffsetX, newOffsetY, newUsedOffsetX, newUsedOffsetY;
                                                GetMapOffsets(canvaswidth, canvasheight, newMapwidth, newMapheight, newWidth, newHeight, out newOffsetX, out newOffsetY, out newUsedOffsetX, out newUsedOffsetY);

                                                lock (_mapOffsetLock)
                                                {
                                                    _mapOffsetX = newTotalOffsetX - newOffsetX;
                                                    _mapOffsetY = newTotalOffsetY - newOffsetY - newMapFontAscent;
                                                    if (_mapWidth > 0 && Math.Abs(_mapOffsetX) > 10 * _mapWidth)
                                                    {
                                                        _mapOffsetX = 10 * _mapWidth * Math.Sign(_mapOffsetX);
                                                    }
                                                    if (_mapHeight > 0 && Math.Abs(_mapOffsetY) > 10 * _mapHeight)
                                                    {
                                                        _mapOffsetY = 10 * _mapHeight * Math.Sign(_mapOffsetY);
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    TouchDictionary[e.Id].Location = e.Location;
                                    _touchMoved = true;
                                }
                            }
                            e.Handled = true;
                        }
                        break;
                    case SKTouchAction.Released:
                        {
                            _savedSender = null;
                            _savedEventArgs = null;

                            if(ForceAllMessages)
                            {
                                TouchEntry entry;
                                bool res = TouchDictionary.TryGetValue(e.Id, out entry);
                                if (res)
                                {
                                    long elapsedms = (DateTime.Now.Ticks - entry.PressTime.Ticks) / TimeSpan.TicksPerMillisecond;
                                    if (elapsedms <= GHConstants.MoveOrPressTimeThreshold && !_touchMoved)
                                    {
                                        ToggleMessageNumberButton_Clicked(sender, e);
                                    }
                                    else if (TouchDictionary.Count == 1) /* Not removed yet */
                                    {
                                        lock (_messageScrollLock)
                                        {
                                            float topScrollLimit = Math.Max(0, -_messageSmallestTop);
                                            long lastrecord_ms = 0;
                                            if (_messageScrollSpeedRecords.Count > 0)
                                            {
                                                lastrecord_ms = (DateTime.Now.Ticks - _messageScrollSpeedRecords[_messageScrollSpeedRecords.Count - 1].TimeStamp.Ticks) / TimeSpan.TicksPerMillisecond;
                                            }

                                            if (_messageScrollOffset > topScrollLimit || _messageScrollOffset < 0)
                                            {
                                                if (lastrecord_ms > GHConstants.ScrollRecordThreshold
                                                    || Math.Abs(_messageScrollSpeed) < GHConstants.ScrollSpeedThreshold * MenuCanvas.CanvasSize.Height)
                                                    _messageScrollSpeed = 0;

                                                _messageScrollSpeedOn = true;
                                                _messageScrollSpeedReleaseStamp = DateTime.Now;
                                            }
                                            else if (lastrecord_ms > GHConstants.ScrollRecordThreshold)
                                            {
                                                _messageScrollSpeedOn = false;
                                                _messageScrollSpeed = 0;
                                            }
                                            else if (Math.Abs(_messageScrollSpeed) >= GHConstants.ScrollSpeedThreshold * canvasView.CanvasSize.Height)
                                            {
                                                _messageScrollSpeedOn = true;
                                                _messageScrollSpeedReleaseStamp = DateTime.Now;
                                            }
                                            else
                                            {
                                                _messageScrollSpeedOn = false;
                                                _messageScrollSpeed = 0;
                                            }
                                            _messageScrollSpeedRecordOn = false;
                                            _messageScrollSpeedRecords.Clear();
                                        }
                                        _touchMoved = false;
                                    }
                                }
                            }
                            else if (_touchWithinSkillButton)
                            {
                                GenericButton_Clicked(sender, e, (int)'S');
                            }
                            else if (_touchWithinHealthOrb)
                            {
                                ShowMaxHealthInOrb = !ShowMaxHealthInOrb;
                            }
                            else if (_touchWithinManaOrb)
                            {
                                ShowMaxManaInOrb = !ShowMaxManaInOrb;
                            }
                            else if (_touchWithinStatusBar)
                            {
                                ShowExtendedStatusBar = !ShowExtendedStatusBar;
                                lock (_mapOffsetLock)
                                {
                                    _statusOffsetY = 0.0f;
                                }
                            }
                            else if (_touchWithinYouButton)
                            {
                                ShowExtendedStatusBar = false;
                                GenericButton_Clicked(sender, e, (int)'}');
                            }
                            else if (_touchWithinPet > 0 && !_showDirections && !_showNumberPad)
                            {
                                ConcurrentQueue<GHResponse> queue;
                                if (GHGame.ResponseDictionary.TryGetValue(_currentGame, out queue))
                                {
                                    queue.Enqueue(new GHResponse(_currentGame, GHRequestType.SetPetMID, _touchWithinPet));
                                    queue.Enqueue(new GHResponse(_currentGame, GHRequestType.GetChar, (int)'{'));
                                }
                            }
                            else
                            {
                                TouchEntry entry;
                                bool res = TouchDictionary.TryGetValue(e.Id, out entry);
                                if (res)
                                {
                                    long elapsedms = (DateTime.Now.Ticks - entry.PressTime.Ticks) / TimeSpan.TicksPerMillisecond;

                                    if (elapsedms <= GHConstants.MoveOrPressTimeThreshold && !_touchMoved)
                                    {
                                        if (ShowExtendedStatusBar)
                                        {
                                            ShowExtendedStatusBar = false;
                                            lock (_mapOffsetLock)
                                            {
                                                _statusOffsetY = 0.0f;
                                            }
                                        }
                                        else
                                            IssueNHCommandViaTouch(sender, e);
                                    }
                                }
                            }
                            if (TouchDictionary.ContainsKey(e.Id))
                                TouchDictionary.Remove(e.Id);
                            else
                                TouchDictionary.Clear(); /* Something's wrong; reset the touch dictionary */

                            if (TouchDictionary.Count == 0)
                                _touchMoved = false;

                            e.Handled = true;
                        }
                        break;
                    case SKTouchAction.Cancelled:
                        if (TouchDictionary.ContainsKey(e.Id))
                            TouchDictionary.Remove(e.Id);
                        else
                            TouchDictionary.Clear(); /* Something's wrong; reset the touch dictionary */

                        if(ForceAllMessages)
                        {
                            lock (_messageScrollLock)
                            {
                                float topScrollLimit = Math.Max(0, -_messageSmallestTop);
                                if (_messageScrollOffset > topScrollLimit || _messageScrollOffset < 0)
                                {
                                    long lastrecord_ms = 0;
                                    if (_messageScrollSpeedRecords.Count > 0)
                                    {
                                        lastrecord_ms = (DateTime.Now.Ticks - _messageScrollSpeedRecords[_messageScrollSpeedRecords.Count - 1].TimeStamp.Ticks) / TimeSpan.TicksPerMillisecond;
                                    }

                                    if (lastrecord_ms > GHConstants.ScrollRecordThreshold
                                        || Math.Abs(_messageScrollSpeed) < GHConstants.ScrollSpeedThreshold * MenuCanvas.CanvasSize.Height)
                                        _messageScrollSpeed = 0;

                                    _messageScrollSpeedOn = true;
                                    _messageScrollSpeedReleaseStamp = DateTime.Now;
                                }
                            }
                        }
                        e.Handled = true;
                        break;
                    case SKTouchAction.Exited:
                        break;
                    case SKTouchAction.WheelChanged:
                        break;
                    default:
                        break;
                }
            }
        }

        public uint PetRectContains(SKPoint p)
        {
            lock(_petDataLock)
            {
                foreach (GHPetDataItem pdi in _petData)
                {
                    if (pdi.Rect.Contains(p))
                        return pdi.Data.m_id;
                }
            }
            return 0;
        }

        public void IssueNHCommandViaTouch(object sender, SKTouchEventArgs e)
        {
            int x = 0, y = 0, mod = 0;
            float canvaswidth = canvasView.CanvasSize.Width;
            float canvasheight = canvasView.CanvasSize.Height;
            float offsetX = (canvaswidth - _mapWidth) / 2;
            float offsetY = (canvasheight - _mapHeight) / 2;

            if (ZoomMiniMode)
            {
                lock (_mapOffsetLock)
                {
                    offsetX -= _mapOffsetX;
                    offsetY -= _mapOffsetY;
                    offsetX += _mapMiniOffsetX;
                    offsetY += _mapMiniOffsetY;
                }
            }
            else
            {
                lock (ClipLock)
                {
                    if (ClipX > 0 && (_mapWidth > canvaswidth || _mapHeight > canvasheight))
                    {
                        offsetX -= (ClipX - (GHConstants.MapCols - 1) / 2) * UsedTileWidth;
                        offsetY -= (ClipY - GHConstants.MapRows / 2) * UsedTileHeight;
                    }
                }
            }

            lock (_mapOffsetLock)
            {
                offsetX += _mapOffsetX;
                offsetY += _mapOffsetY + UsedMapFontAscent;
            }

            if (UsedTileWidth > 0)
                x = (int)((e.Location.X - offsetX) / UsedTileWidth);
            if (UsedTileHeight > 0)
                y = (int)((e.Location.Y - offsetY) / UsedTileHeight);

            if (!_showDirections && !_showNumberPad && !(MapWalkMode && WalkArrows))
            {
                if (x > 0 && x < GHConstants.MapCols && y >= 0 && y < GHConstants.MapRows)
                {
                    if (MapLookMode)
                        mod = (int)NhGetPosMods.Click2;
                    else if (MapTravelMode)
                        mod = (int)NhGetPosMods.Click1;
                    else
                        mod = (int)NhGetPosMods.Click3;

                    ConcurrentQueue<GHResponse> queue;
                    if (GHGame.ResponseDictionary.TryGetValue(_currentGame, out queue))
                    {
                        queue.Enqueue(new GHResponse(_currentGame, GHRequestType.Location, x, y, mod));
                    }
                }
            }
            else
            {
                float buttonsize = ShowNumberPad ? GHConstants.NumberButtonSize : _showDirections ? GHConstants.ArrowButtonSize : GHConstants.MoveArrowButtonSize;
                lock (_canvasButtonLock)
                {
                    if (e.Location.X >= _canvasButtonRect.Left && e.Location.X <= _canvasButtonRect.Right && e.Location.Y >= _canvasButtonRect.Top && e.Location.Y <= _canvasButtonRect.Bottom)
                    {
                        int resp = 0;
                        SKPoint RectLoc = new SKPoint(e.Location.X - _canvasButtonRect.Left, e.Location.Y - _canvasButtonRect.Top);

                        if (RectLoc.Y < _canvasButtonRect.Height * buttonsize && RectLoc.X < _canvasButtonRect.Width * buttonsize)
                            resp += -7;
                        else if (RectLoc.Y < _canvasButtonRect.Height * buttonsize && RectLoc.X > _canvasButtonRect.Width * (1.0f - buttonsize))
                            resp += -9;
                        else if (RectLoc.Y > _canvasButtonRect.Height * (1.0f - buttonsize) && RectLoc.X < _canvasButtonRect.Width * buttonsize)
                            resp += -1;
                        else if (RectLoc.Y > _canvasButtonRect.Height * (1.0f - buttonsize) && RectLoc.X > _canvasButtonRect.Width * (1.0f - buttonsize))
                            resp += -3;
                        else if (RectLoc.Y < _canvasButtonRect.Height * buttonsize)
                            resp += -8; //ch = "k";
                        else if (RectLoc.Y > _canvasButtonRect.Height * (1.0f - buttonsize))
                            resp += -2; // ch = "j";
                        else if (RectLoc.X < _canvasButtonRect.Width * buttonsize)
                            resp += -4; // ch = "h";
                        else if (RectLoc.X > _canvasButtonRect.Width * (1.0f - buttonsize))
                            resp += -6; // ch = "l";
                        else
                        {
                            lock (_uLock)
                            {
                                if (_showDirections && GHUtils.isok(_ux, _uy) && GHUtils.isok(x, y))
                                {
                                    int dx = x - _ux;
                                    int dy = y - _uy;
                                    if (Math.Abs(x - _ux) <= 1 && Math.Abs(y - _uy) <= 1)
                                    {
                                        int dres = -1 * (5 + dx - 3 * dy);
                                        if (dres == 5)
                                            resp = 46; /* '.', or self */
                                        else
                                            resp += dres;
                                    }
                                    else
                                        return;
                                }
                                else
                                {
                                    if (ShowNumberPad)
                                        resp += -5;
                                    else
                                        resp = 46; /* '.', or self */
                                }
                            }
                        }

                        if (ShowNumberPad)
                            resp -= 10;

                        ConcurrentQueue<GHResponse> queue;
                        if (GHGame.ResponseDictionary.TryGetValue(_currentGame, out queue))
                        {
                            queue.Enqueue(new GHResponse(_currentGame, GHRequestType.GetChar, resp));
                        }
                    }
                    else if (ShowNumberPad && e.Location.X < _canvasButtonRect.Left
                        && e.Location.Y >= _canvasButtonRect.Top + _canvasButtonRect.Height * (1.0f - buttonsize)
                        && e.Location.Y <= _canvasButtonRect.Top + _canvasButtonRect.Height)
                    {
                        int resp = -10;
                        ConcurrentQueue<GHResponse> queue;
                        if (GHGame.ResponseDictionary.TryGetValue(_currentGame, out queue))
                        {
                            queue.Enqueue(new GHResponse(_currentGame, GHRequestType.GetChar, resp));
                        }
                    }
                }
            }
        }

        bool DetermineHasEnlargementOrAnimationOrSpecialHeight(LayerInfo layers)
        {
            int gui_glyph, ntile;
            for (int i = 0; i < (int)layer_types.MAX_LAYERS; i++)
            {
                if (layers.special_monster_layer_height != 0 || layers.special_feature_doodad_layer_height != 0)
                    return true;
                gui_glyph = Math.Abs(layers.layer_gui_glyphs[i]);
                if(gui_glyph != GHApp.NoGlyph)
                {
                    ntile = GHApp.Glyph2Tile[gui_glyph];
                    if (GHApp.Tile2Enlargement[ntile] > 0 || GHApp.Tile2Animation[ntile] > 0)
                        return true;
                }
            }
            return false;
        }

        public void SetMapSymbol(int x, int y, int glyph, int bkglyph, int c, int color, uint special, LayerInfo layers)
        {
            lock (_mapDataLock)
            {
                if (((layers.layer_flags & (ulong)LayerFlags.LFLAGS_UXUY) != 0 && (_mapData[x, y].Layers.layer_flags & (ulong)LayerFlags.LFLAGS_UXUY) == 0) ||
                    (layers.m_id != 0 && layers.m_id != _mapData[x, y].Layers.m_id))
                {
                    /* Update counter value only if the monster just moved here, not, e.g. if it changes action in the same square,
                     * or is printed in the same square again with the same origin coordinates. This way, the movement action is played only once. 
                     */
                    lock (AnimationTimerLock)
                    {
                        _mapData[x, y].GlyphPrintAnimationCounterValue = AnimationTimers.general_animation_counter;
                    }
                    lock (_mainCounterLock)
                    {
                        _mapData[x, y].GlyphPrintMainCounterValue = _mainCounterValue;
                    }
                }
                if ((layers.layer_flags & (ulong)LayerFlags.LFLAGS_UXUY) != 0)
                {
                    lock(_uLock)
                    {
                        _ux = x;
                        _uy = y;
                        _u_condition_bits = layers.condition_bits;
                        _u_status_bits = layers.status_bits;
                        if(layers.buff_bits != null)
                        {
                            for (int i = 0; i < GHConstants.NUM_BUFF_BIT_ULONGS; i++)
                            {
                                _u_buff_bits[i] = layers.buff_bits[i];
                            }
                        }
                    }
                }
                if (layers.o_id != 0 && layers.o_id != _mapData[x, y].Layers.o_id)
                {
                    /* Update counter value only if the object just moved here, not, e.g. if it changes action in the same square,
                     * or is printed in the same square again with the same origin coordinates. This way, the movement action is played only once. 
                     */
                    lock (AnimationTimerLock)
                    {
                        _mapData[x, y].GlyphObjectPrintAnimationCounterValue = AnimationTimers.general_animation_counter;
                    }
                    lock (_mainCounterLock)
                    {
                        _mapData[x, y].GlyphObjectPrintMainCounterValue = _mainCounterValue;
                    }
                }
                /* General counter that gets always set */
                lock (AnimationTimerLock)
                {
                    _mapData[x, y].GlyphGeneralPrintAnimationCounterValue = AnimationTimers.general_animation_counter;
                }
                lock (_mainCounterLock)
                {
                    _mapData[x, y].GlyphGeneralPrintMainCounterValue = _mainCounterValue;
                }
                _mapData[x, y].Glyph = glyph;
                _mapData[x, y].BkGlyph = bkglyph;
                _mapData[x, y].Symbol = Char.ConvertFromUtf32(c);
                _mapData[x, y].Color = UIUtils.NHColor2SKColor(color, (special & 0x00002000UL) != 0 ? (int)MenuItemAttributes.AltColors : 0);
                _mapData[x, y].Special = special;
                _mapData[x, y].Layers = layers;

                _mapData[x, y].NeedsUpdate = true;
                _mapData[x, y].HasEnlargementOrAnimationOrSpecialHeight = AlternativeLayerDrawing ? DetermineHasEnlargementOrAnimationOrSpecialHeight(layers) : false;
            }
        }
        public void SetMapCursor(int x, int y)
        {
            lock (_mapDataLock)
            {
                _mapCursorX = x;
                _mapCursorY = y;
            }
        }
        public void UpdateCursor(int style, int force_paint, int show_on_u)
        {
            lock (_mapDataLock)
            {
                _cursorType = (game_cursor_types)style;
                _force_paint_at_cursor = (force_paint != 0);
                _show_cursor_on_u = (show_on_u != 0);
            }
        }

        public void ClearMap()
        {
            lock (_mapDataLock)
            {
                for (int x = 1; x < GHConstants.MapCols; x++)
                {
                    for (int y = 0; y < GHConstants.MapRows; y++)
                    {
                        _mapData[x, y].Glyph = GHApp.UnexploredGlyph;
                        _mapData[x, y].BkGlyph = GHApp.NoGlyph;
                        _mapData[x, y].Symbol = "";
                        _mapData[x, y].Color = SKColors.Black;// default(MapData);
                        _mapData[x, y].Special = 0;
                        _mapData[x, y].NeedsUpdate = true;
                        _mapData[x, y].GlyphPrintAnimationCounterValue = 0;
                        _mapData[x, y].GlyphPrintMainCounterValue = 0;
                        _mapData[x, y].GlyphObjectPrintAnimationCounterValue = 0;
                        _mapData[x, y].GlyphObjectPrintMainCounterValue = 0;
                        _mapData[x, y].GlyphGeneralPrintMainCounterValue = 0;

                        _mapData[x, y].Layers = new LayerInfo();
                        _mapData[x, y].Layers.layer_glyphs = new int[(int)layer_types.MAX_LAYERS];
                        _mapData[x, y].Layers.layer_gui_glyphs = new int[(int)layer_types.MAX_LAYERS];
                        _mapData[x, y].Layers.leash_mon_x = new sbyte[GHConstants.MaxLeashed + 1];
                        _mapData[x, y].Layers.leash_mon_y = new sbyte[GHConstants.MaxLeashed + 1];

                        _mapData[x, y].Layers.layer_glyphs[0] = GHApp.UnexploredGlyph;
                        _mapData[x, y].Layers.layer_gui_glyphs[0] = GHApp.UnexploredGlyph;
                        for (int i = 1; i < (int)layer_types.MAX_LAYERS; i++)
                        {
                            _mapData[x, y].Layers.layer_glyphs[i] = GHApp.NoGlyph;
                            _mapData[x, y].Layers.layer_gui_glyphs[i] = GHApp.NoGlyph;
                        }

                        _mapData[x, y].Layers.glyph = GHApp.UnexploredGlyph;
                        _mapData[x, y].Layers.bkglyph = GHApp.NoGlyph;
                    }
                }
            }
        }


        public void SetTargetClip(int x, int y, bool immediate_pan)
        {
            long curtimervalue = 0;
            long pantime = Math.Max(2, (long)Math.Ceiling((double)UIUtils.GetMainCanvasAnimationFrequency(MapRefreshRate) / 8.0));
            //lock (AnimationTimerLock)
            //{
            //    curtimervalue = AnimationTimers.general_animation_counter;
            //}
            lock (_mainCounterLock)
            {
                curtimervalue = _mainCounterValue;
            }

            lock (TargetClipLock)
            {
                if (immediate_pan || GraphicsStyle == GHGraphicsStyle.ASCII || ForceAscii)
                {
                    _targetClipOn = false;
                    _originMapOffsetWithNewClipX = 0;
                    _originMapOffsetWithNewClipY = 0;
                }
                else
                {
                    _targetClipOn = true;
                    lock (_mapOffsetLock)
                    {
                        _originMapOffsetWithNewClipX = _mapOffsetX + (float)(x - ClipX) * UsedTileWidth;
                        _originMapOffsetWithNewClipY = _mapOffsetY + (float)(y - ClipY) * UsedTileHeight;
                    }
                    _targetClipStartCounterValue = curtimervalue;
                    _targetClipPanTime = pantime; // GHConstants.DefaultPanTime;
                }
            }
            lock (ClipLock)
            {
                _clipX = x;
                _clipY = y;
            }
            lock (_mapOffsetLock)
            {
                _mapOffsetX = _originMapOffsetWithNewClipX;
                _mapOffsetY = _originMapOffsetWithNewClipY;
            }
        }

        public void ClearAllObjectData(int x, int y)
        {
            lock (_objectDataLock)
            {
                if (_objectData[x, y] != null)
                {
                    if (_objectData[x, y].FloorObjectList != null)
                        _objectData[x, y].FloorObjectList.Clear();
                    if (_objectData[x, y].CoverFloorObjectList != null)
                        _objectData[x, y].CoverFloorObjectList.Clear();
                    if (_objectData[x, y].MemoryObjectList != null)
                        _objectData[x, y].MemoryObjectList.Clear();
                    if (_objectData[x, y].CoverMemoryObjectList != null)
                        _objectData[x, y].CoverMemoryObjectList.Clear();
                }
            }
        }

        public void AddObjectData(int x, int y, obj otmp, int cmdtype, int where, objclassdata otypdata, ulong oflags)
        {
            bool is_uwep = (oflags & (ulong)objdata_flags.OBJDATA_FLAGS_UWEP) != 0UL;
            bool is_uwep2 = (oflags & (ulong)objdata_flags.OBJDATA_FLAGS_UWEP2) != 0UL;
            bool is_uquiver = (oflags & (ulong)objdata_flags.OBJDATA_FLAGS_UQUIVER) != 0UL;
            bool is_equipped = is_uwep | is_uwep2 | is_uquiver;
            bool hallucinated = (oflags & (ulong)objdata_flags.OBJDATA_FLAGS_HALLUCINATION) != 0UL;
            bool foundthisturn = (oflags & (ulong)objdata_flags.OBJDATA_FLAGS_FOUND_THIS_TURN) != 0UL;
            bool isuchain = (oflags & (ulong)objdata_flags.OBJDATA_FLAGS_UCHAIN) != 0UL;
            bool isuball = (oflags & (ulong)objdata_flags.OBJDATA_FLAGS_UBALL) != 0UL;

            if (is_equipped)
            {
                bool outofammo1 = (oflags & (ulong)objdata_flags.OBJDATA_FLAGS_OUT_OF_AMMO1) != 0UL;
                bool wrongammo1 = (oflags & (ulong)objdata_flags.OBJDATA_FLAGS_WRONG_AMMO_TYPE1) != 0UL;
                bool notbeingused1 = (oflags & (ulong)objdata_flags.OBJDATA_FLAGS_NOT_BEING_USED1) != 0UL;
                bool notweapon1 = (oflags & (ulong)objdata_flags.OBJDATA_FLAGS_NOT_WEAPON1) != 0UL;
                bool outofammo2 = (oflags & (ulong)objdata_flags.OBJDATA_FLAGS_OUT_OF_AMMO2) != 0UL;
                bool wrongammo2 = (oflags & (ulong)objdata_flags.OBJDATA_FLAGS_WRONG_AMMO_TYPE2) != 0UL;
                bool notbeingused2 = (oflags & (ulong)objdata_flags.OBJDATA_FLAGS_NOT_BEING_USED2) != 0UL;
                bool notweapon2 = (oflags & (ulong)objdata_flags.OBJDATA_FLAGS_NOT_WEAPON2) != 0UL;
                bool outofammo = is_uwep ? outofammo1 : is_uwep2 ? outofammo2 : false;
                bool wrongammo = is_uwep ? wrongammo1 : is_uwep2 ? wrongammo2 : false;
                bool notbeingused = is_uwep ? notbeingused1 : is_uwep2 ? notbeingused2 : false;
                bool notweapon = is_uwep ? notweapon1 : is_uwep2 ? notweapon2 : false;
                bool isammo = (oflags & (ulong)objdata_flags.OBJDATA_FLAGS_IS_AMMO) != 0UL;
                bool isthrowingweapon = (oflags & (ulong)objdata_flags.OBJDATA_FLAGS_THROWING_WEAPON) != 0UL;

                int idx = is_uwep ? 0 : is_uwep2 ? 1 : 2;
                lock (_weaponStyleObjDataItemLock)
                {
                    switch (cmdtype)
                    {
                        case 1: /* Clear */
                            _weaponStyleObjDataItem[idx] = null;
                            break;
                        case 2: /* Add item */
                            _weaponStyleObjDataItem[idx] = new ObjectDataItem(otmp, otypdata, hallucinated, outofammo, wrongammo, notbeingused, notweapon, foundthisturn, isammo, isthrowingweapon);
                            break;
                        case 3: /* Add container item to previous item */
                            _weaponStyleObjDataItem[idx].ContainedObjs.Add(new ObjectDataItem(otmp, otypdata, hallucinated));
                            break;
                    }
                }
            }
            else
            {
                lock (_objectDataLock)
                {
                    if (_objectData[x, y] != null)
                    {
                        bool is_memoryobj = (where == (int)obj_where_types.OBJ_HEROMEMORY);
                        bool is_drawn_in_front = (oflags & (ulong)objdata_flags.OBJDATA_FLAGS_DRAWN_IN_FRONT) != 0UL;
                        List<ObjectDataItem> objectList = is_memoryobj ? (is_drawn_in_front ? _objectData[x, y].CoverMemoryObjectList : _objectData[x, y].MemoryObjectList) : (is_drawn_in_front ? _objectData[x, y].CoverFloorObjectList : _objectData[x, y].FloorObjectList);
                        ObjectDataItem newItem;
                        switch (cmdtype)
                        {
                            case 1: /* Clear */
                                if (objectList != null)
                                    objectList.Clear();
                                break;
                            case 2: /* Add item */
                                if (objectList == null)
                                {
                                    if (is_memoryobj)
                                    {
                                        if (is_drawn_in_front)
                                            _objectData[x, y].CoverMemoryObjectList = new List<ObjectDataItem>();
                                        else
                                            _objectData[x, y].MemoryObjectList = new List<ObjectDataItem>();
                                    }
                                    else
                                    {
                                        if (is_drawn_in_front)
                                            _objectData[x, y].CoverFloorObjectList = new List<ObjectDataItem>();
                                        else
                                            _objectData[x, y].FloorObjectList = new List<ObjectDataItem>();
                                    }

                                    objectList = is_memoryobj ? (is_drawn_in_front ? _objectData[x, y].CoverMemoryObjectList : _objectData[x, y].MemoryObjectList) : (is_drawn_in_front ? _objectData[x, y].CoverFloorObjectList : _objectData[x, y].FloorObjectList);
                                }
                                newItem = new ObjectDataItem(otmp, otypdata, hallucinated, foundthisturn);
                                objectList.Add(newItem);
                                break;
                            case 3: /* Add container item to previous item */
                                if (objectList == null || objectList.Count == 0)
                                    break;
                                if (objectList[objectList.Count - 1].ContainedObjs == null)
                                    objectList[objectList.Count - 1].ContainedObjs = new List<ObjectDataItem>();
                                objectList[objectList.Count - 1].ContainedObjs.Add(new ObjectDataItem(otmp, otypdata, hallucinated));
                                break;
                            case 4: /* Clear uchain and uball */
                                _uChain = null;
                                _uBall = null;
                                break;
                            case 5: /* Add uchain or uball */
                                if (!is_memoryobj && (isuchain || isuball))
                                {
                                    newItem = new ObjectDataItem(otmp, otypdata, hallucinated, foundthisturn);
                                    if (isuchain)
                                        _uChain = newItem;
                                    if (isuball)
                                        _uBall = newItem;
                                }
                                break;
                        }
                    }
                }
            }
        }

        public void ClearPetData()
        {
            lock(_petDataLock)
            {
                _petData.Clear();
            }
        }

        public void AddPetData(monst_info monster_data)
        {
            lock (_petDataLock)
            {
                _petData.Add(new GHPetDataItem(monster_data));
            }
        }

        public void ClearConditionTexts()
        {
            lock (_conditionTextLock)
            {
                _conditionTexts.Clear();
            }
        }

        public void ClearFloatingTexts()
        {
            lock (_floatingTextLock)
            {
                _floatingTexts.Clear();
            }
        }

        public void ClearGuiEffects()
        {
            lock (_guiEffectLock)
            {
                _guiEffects.Clear();
            }
        }

        public void FadeToBlack(uint milliseconds)
        {
            MainGrid.IsEnabled = false;
            uint timeToAnimate = milliseconds;
            Animation animation = new Animation(v => canvasView.Opacity = v, 1.0, 0.0);
            animation.Commit(canvasView, "Opacity", length: timeToAnimate, rate: 25, repeat: () => false);
        }

        public void FadeFromBlack(uint milliseconds)
        {
            MainGrid.IsEnabled = true;
            uint timeToAnimate = milliseconds;
            Animation animation = new Animation(v => canvasView.Opacity = v, 0.0, 1.0);
            animation.Commit(canvasView, "Opacity", length: timeToAnimate, rate: 25, repeat: () => false);
        }

        private void PickupButton_Clicked(object sender, EventArgs e)
        {
            GenericButton_Clicked(sender, e, ',');
        }
        private void SearchButton_Clicked(object sender, EventArgs e)
        {
            GenericButton_Clicked(sender, e, 's');
        }
        private void KickButton_Clicked(object sender, EventArgs e)
        {
            GenericButton_Clicked(sender, e, 'k');
        }
        private void UpButton_Clicked(object sender, EventArgs e)
        {
            GenericButton_Clicked(sender, e, '<');
        }
        private void DownButton_Clicked(object sender, EventArgs e)
        {
            GenericButton_Clicked(sender, e, '>');
        }

        public void GenericButton_Clicked(object sender, EventArgs e, int resp)
        {
            if (!((resp >= '0' && resp <= '9') || (resp <= -1 && resp >= -19)))
                ShowNumberPad = false;

            if (_currentGame != null)
            {
                ConcurrentQueue<GHResponse> queue;
                if (GHGame.ResponseDictionary.TryGetValue(_currentGame, out queue))
                {
                    queue.Enqueue(new GHResponse(_currentGame, GHRequestType.GetChar, resp));
                }
            }
        }

        private void InventoryButton_Clicked(object sender, EventArgs e)
        {
            GHApp.DebugWriteRestart("Inventory");
            GenericButton_Clicked(sender, e, 'i');
        }
        private void LookHereButton_Clicked(object sender, EventArgs e)
        {
            GenericButton_Clicked(sender, e, ':');
        }
        private void WaitButton_Clicked(object sender, EventArgs e)
        {
            GenericButton_Clicked(sender, e, '.');
        }
        private void FireButton_Clicked(object sender, EventArgs e)
        {
            GenericButton_Clicked(sender, e, 'f');
        }

        private void MoreButton_Clicked(object sender, EventArgs e)
        {
            //lMoreButton.IsEnabled = false;
            ShowMoreCanvas(sender, e);
        }

        private void ShowMoreCanvas(object sender, EventArgs e)
        {
            lock (RefreshScreenLock)
            {
                RefreshScreen = false;
            }

            MoreCommandsGrid.IsVisible = true;
            MainGrid.IsVisible = false;
            if (canvasView.AnimationIsRunning("GeneralAnimationCounter"))
                canvasView.AbortAnimation("GeneralAnimationCounter");
            _mapUpdateStopWatch.Stop();
            StartCommandCanvasAnimation();
        }

        private void YnButton_Clicked(object sender, EventArgs e)
        {
            LabeledImageButton ghb = (LabeledImageButton)sender;

            /* This is slightly slower and flickers less with two consecutive yn questions than a direct call to HideYnResponses() */
            ConcurrentQueue<GHRequest> queue;
            if (GHGame.RequestDictionary.TryGetValue(CurrentGame, out queue))
            {
                queue.Enqueue(new GHRequest(CurrentGame, GHRequestType.HideYnResponses));
            }

            GenericButton_Clicked(sender, e, ghb.GHCommand);
        }

        private void RepeatButton_Clicked(object sender, EventArgs e)
        {
            GenericButton_Clicked(sender, e, GHUtils.Ctrl('A'));
        }

        private void CastButton_Clicked(object sender, EventArgs e)
        {
            GenericButton_Clicked(sender, e, 'Z');
        }

        private void LootButton_Clicked(object sender, EventArgs e)
        {
            GenericButton_Clicked(sender, e, 'l');
        }

        private void EatButton_Clicked(object sender, EventArgs e)
        {
            GenericButton_Clicked(sender, e, 'e');
        }

        private void CountButton_Clicked(object sender, EventArgs e)
        {
            GenericButton_Clicked(sender, e, 'n');
        }

        private void RunButton_Clicked(object sender, EventArgs e)
        {
            GenericButton_Clicked(sender, e, 'G');
        }

        private void ESCButton_Clicked(object sender, EventArgs e)
        {
            GHApp.PlayButtonClickedSound();
            TouchDictionary.Clear();
            GenericButton_Clicked(sender, e, 27);
        }
        public void ToggleAutoCenterMode()
        {
            ToggleAutoCenterModeButton_Clicked(ToggleAutoCenterModeButton, new EventArgs());
        }

        private void ToggleAutoCenterModeButton_Clicked(object sender, EventArgs e)
        {
            GHApp.PlayMenuSelectSound();
            MapNoClipMode = !MapNoClipMode;
            if (MapNoClipMode)
            {
                ToggleAutoCenterModeButton.ImgSourcePath = "resource://" + GHApp.AppResourceName + ".Assets.UI.stone-autocenter-off.png";
                SimpleToggleAutoCenterModeButton.ImgSourcePath = "resource://" + GHApp.AppResourceName + ".Assets.UI.stone-autocenter-off.png";
            }
            else
            {
                ToggleAutoCenterModeButton.ImgSourcePath = "resource://" + GHApp.AppResourceName + ".Assets.UI.stone-autocenter-on.png";
                SimpleToggleAutoCenterModeButton.ImgSourcePath = "resource://" + GHApp.AppResourceName + ".Assets.UI.stone-autocenter-on.png";
                if (sender != null && GHUtils.isok(_ux, _uy))
                {
                    SetTargetClip(_ux, _uy, false);
                }
            }
        }
        private void ToggleTravelModeButton_Clicked(object sender, EventArgs e)
        {
            GHApp.PlayMenuSelectSound();
            MapTravelMode = !MapTravelMode;
            if (MapTravelMode)
            {
                ToggleTravelModeButton.ImgSourcePath = "resource://" + GHApp.AppResourceName + ".Assets.UI.stone-travel-on.png";
            }
            else
            {
                ToggleTravelModeButton.ImgSourcePath = "resource://" + GHApp.AppResourceName + ".Assets.UI.stone-travel-off.png";
            }
        }
        private void LookModeButton_Clicked(object sender, EventArgs e)
        {
            GHApp.PlayMenuSelectSound();
            MapLookMode = !MapLookMode;
            if (MapLookMode)
            {
                LookModeButton.ImgSourcePath = "resource://" + GHApp.AppResourceName + ".Assets.UI.stone-look-on.png";
                SimpleLookModeButton.ImgSourcePath = "resource://" + GHApp.AppResourceName + ".Assets.UI.stone-look-on.png";
            }
            else
            {
                LookModeButton.ImgSourcePath = "resource://" + GHApp.AppResourceName + ".Assets.UI.stone-look-off.png";
                SimpleLookModeButton.ImgSourcePath = "resource://" + GHApp.AppResourceName + ".Assets.UI.stone-look-off.png";
            }
        }

        private void ToggleZoomMiniButton_Clicked(object sender, EventArgs e)
        {
            GHApp.PlayMenuSelectSound();
            ZoomMiniMode = !ZoomMiniMode;
            if (ZoomMiniMode)
            {
                ToggleZoomMiniButton.ImgSourcePath = "resource://" + GHApp.AppResourceName + ".Assets.UI.stone-minimap-on.png";
                SimpleToggleZoomMiniButton.ImgSourcePath = "resource://" + GHApp.AppResourceName + ".Assets.UI.stone-minimap-on.png";
            }
            else
            {
                ToggleZoomMiniButton.ImgSourcePath = "resource://" + GHApp.AppResourceName + ".Assets.UI.stone-minimap-off.png";
                SimpleToggleZoomMiniButton.ImgSourcePath = "resource://" + GHApp.AppResourceName + ".Assets.UI.stone-minimap-off.png";
                if (sender != null && GHUtils.isok(_ux, _uy) && !MapNoClipMode)
                {
                    SetTargetClip(_ux, _uy, true);
                }
            }
        }

        private void ToggleZoomAlternateButton_Clicked(object sender, EventArgs e)
        {
            GHApp.PlayMenuSelectSound();
            ZoomAlternateMode = !ZoomAlternateMode;
            if (ZoomAlternateMode)
            {
                ToggleZoomAlternateButton.ImgSourcePath = "resource://" + GHApp.AppResourceName + ".Assets.UI.stone-altmap-on.png";
                lock(_mapOffsetLock)
                {
                    if(MapFontSize > 0)
                    {
                        _mapOffsetX = _mapOffsetX * MapFontAlternateSize / MapFontSize;
                        _mapOffsetY = _mapOffsetY * MapFontAlternateSize / MapFontSize;
                    }
                }
            }
            else
            {
                ToggleZoomAlternateButton.ImgSourcePath = "resource://" + GHApp.AppResourceName + ".Assets.UI.stone-altmap-off.png";
                lock (_mapOffsetLock)
                {
                    if (MapFontAlternateSize > 0)
                    {
                        _mapOffsetX = _mapOffsetX * MapFontSize / MapFontAlternateSize;
                        _mapOffsetY = _mapOffsetY * MapFontSize / MapFontAlternateSize;
                    }
                }

                if (sender != null && GHUtils.isok(_ux, _uy) && !MapNoClipMode)
                {
                    SetTargetClip(_ux, _uy, true);
                }
            }

        }

        private async void GameMenuButton_Clicked(object sender, EventArgs e)
        {
            GameMenuButton.IsEnabled = false;
            SimpleGameMenuButton.IsEnabled = false;
            GHApp.PlayButtonClickedSound();

            if (canvasView.AnimationIsRunning("GeneralAnimationCounter"))
                canvasView.AbortAnimation("GeneralAnimationCounter");
            _mapUpdateStopWatch.Stop();
            TouchDictionary.Clear();

            await ShowGameMenu(sender, e);

            if (!canvasView.AnimationIsRunning("GeneralAnimationCounter"))
                StartMainCanvasAnimation();

            GameMenuButton.IsEnabled = true;
            SimpleGameMenuButton.IsEnabled = true;
        }

        private void PopupOkButton_Clicked(object sender, EventArgs e)
        {
            GenericButton_Clicked(sender, e, 27);
        }

        private void HidePopupGrid()
        {
            PopupGrid.IsVisible = false;
        }

        private void GHButton_Clicked(object sender, EventArgs e)
        {
            GHApp.DebugWriteRestart("GHButton_Clicked");
            LabeledImageButton ghbutton = (LabeledImageButton)sender;
            switch ((int)ghbutton.GHCommand)
            {
                case -102:
                    GenericButton_Clicked(sender, e, 'n');
                    GenericButton_Clicked(sender, e, -12);
                    GenericButton_Clicked(sender, e, -10);
                    GenericButton_Clicked(sender, e, 's');
                    break;
                case -103:
                    GenericButton_Clicked(sender, e, 'n');
                    GenericButton_Clicked(sender, e, -12);
                    GenericButton_Clicked(sender, e, -10);
                    GenericButton_Clicked(sender, e, -10);
                    GenericButton_Clicked(sender, e, '.');
                    break;
                case -104:
                    GameMenuButton_Clicked(sender, e);
                    break;
                case -105:
                    GenericButton_Clicked(sender, e, 'n');
                    DoShowNumberPad();
                    break;
                default:
                    GenericButton_Clicked(sender, e, (int)ghbutton.GHCommand);
                    break;
            }
        }


        private readonly SKColor _suffixTextColor = new SKColor(220, 220, 220);
        private readonly SKColor _suffixTextColorReverted = new SKColor(35, 35, 35);
        private readonly SKColor _menuHighlightColor = new SKColor(0xFF, 0x88, 0x00, 0x88);
        private readonly SKColor _menuHighlight2Color = new SKColor(0xFF, 0xAA, 0x00, 0xAA);
        private int _firstDrawnMenuItemIdx = -1;
        private int _lastDrawnMenuItemIdx = -1;
        private readonly object _totalMenuHeightLock = new object();
        private float _totalMenuHeight = 0;
        private float TotalMenuHeight { get { lock (_totalMenuHeightLock) { return _totalMenuHeight; } } set { lock (_totalMenuHeightLock) { _totalMenuHeight = value; } } }

        private bool _refreshMenuRowCounts = true;
        private readonly object _refreshMenuRowCountLock = new object();
        private bool RefreshMenuRowCounts { get { lock (_refreshMenuRowCountLock) { return _refreshMenuRowCounts; } } set { lock (_refreshMenuRowCountLock) { _refreshMenuRowCounts = value; } } }

        private void MenuCanvas_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;
            SwitchableCanvasView referenceCanvasView = MenuCanvas;
            float canvaswidth = referenceCanvasView.CanvasSize.Width;
            float canvasheight = referenceCanvasView.CanvasSize.Height;
            float x, y;
            string str;
            SKRect textBounds = new SKRect();
            float scale = (float)Math.Sqrt((double)(canvaswidth * canvasheight / (float)(referenceCanvasView.Width * referenceCanvasView.Height)));

            canvas.Clear();
            lock (_menuDrawOnlyLock)
            {
                if (_menuDrawOnlyClear)
                    return;
            }

            if (canvaswidth <= 16 || canvasheight <= 16)
                return;

            lock (MenuCanvas.MenuItemLock)
            {
                if (referenceCanvasView.MenuItems == null)
                    return;
            }

            using (SKPaint textPaint = new SKPaint())
            {
                textPaint.Typeface = GHApp.UnderwoodTypeface;
                textPaint.TextSize = GHConstants.MenuDefaultRowHeight * scale;
                float picturewidth = 64.0f * textPaint.FontSpacing / 48.0f;
                float picturepadding = 9 * scale;
                float leftinnerpadding = 5;
                float curmenuoffset = 0;
                lock (_menuScrollLock)
                {
                    curmenuoffset = _menuScrollOffset;
                }
                y = curmenuoffset;
                double menumarginx = MenuCanvas.MenuButtonStyle ? 30.0 : 15.0;
                double menuwidth = Math.Max(1, Math.Min(MenuCanvas.Width - menumarginx * 2, UIUtils.MenuViewWidthRequest(referenceCanvasView.MenuStyle)));
                float menuwidthoncanvas = (float)(menuwidth * scale);
                float leftmenupadding = Math.Max(0, (canvaswidth - menuwidthoncanvas) / 2);
                float rightmenupadding = leftmenupadding;
                float accel_fixed_width = 10;
                bool first = true;
                float bottomPadding = 0;
                float topPadding = 0;
                float maintext_x_start = 0;
                float fontspacingpadding = 0;
                lock (MenuCanvas.MenuItemLock)
                {
                    bool has_pictures = false;
                    bool has_identifiers = false;
                    _firstDrawnMenuItemIdx = -1;
                    _lastDrawnMenuItemIdx = -1;
                    foreach (GHMenuItem mi in referenceCanvasView.MenuItems)
                    {
                        if (mi.Identifier != 0 || mi.SpecialMark != '\0')
                            has_identifiers = true;

                        if (mi.IsGlyphVisible)
                            has_pictures = true;

                        if (has_identifiers && has_pictures)
                            break;
                    }

                    lock (_refreshMenuRowCountLock)
                    {
                        for (int idx = 0; idx < referenceCanvasView.MenuItems.Count; idx++)
                        {
                            GHMenuItem mi = referenceCanvasView.MenuItems[idx];
                            bool IsMiButton = mi.IsButton;
                            float extra_vertical_padding = IsMiButton ? 12f : 0f;

                            /* Padding */
                            bottomPadding = (mi.BottomPadding + extra_vertical_padding) * scale;
                            topPadding = (mi.TopPadding + extra_vertical_padding) * scale;

                            /* Text Size and Minimum Row Height */
                            if ((mi.Attributes & (int)MenuItemAttributes.HalfSize) != 0)
                                textPaint.TextSize = (mi.MinimumTouchableTextSize / 2) * scale;
                            else
                                textPaint.TextSize = mi.MinimumTouchableTextSize * scale;
                            float minrowheight = mi.MinimumRowHeight(textPaint.FontSpacing, bottomPadding, topPadding, canvaswidth, canvasheight);

                            x = leftmenupadding;
                            mi.DrawBounds.Left = x;
                            float mainfontsize = (float)mi.FontSize * scale;
                            float relsuffixsize = (float)mi.RelativeSuffixFontSize;
                            float suffixfontsize = relsuffixsize * mainfontsize;
                            textPaint.Typeface = GHApp.GetTypefaceByName(mi.FontFamily);
                            textPaint.TextSize = mainfontsize;
                            textPaint.TextAlign = SKTextAlign.Left;

                            mi.DrawBounds.Top = y;
                            //if (mi.DrawBounds.Top >= canvasheight)
                            //    break;

                            if (first)
                            {
                                accel_fixed_width = textPaint.MeasureText("A"); // textPaint.FontMetrics.AverageCharacterWidth; // + 3 * textPaint.MeasureText(" ");
                                _firstDrawnMenuItemIdx = idx;
                                maintext_x_start = leftmenupadding + leftinnerpadding + (has_identifiers && !MenuCanvas.HideMenuLetters ? accel_fixed_width : 0) + (has_pictures ? picturepadding + picturewidth + picturepadding : !MenuCanvas.HideMenuLetters ? accel_fixed_width : 0 /*textPaint.FontMetrics.AverageCharacterWidth*/);
                                first = false;
                            }

                            int maintextrows = 1;
                            int suffixtextrows = 0;
                            int suffix2textrows = 0;

                            string[] maintextsplit = mi.MainTextSplit;
                            string[] suffixtextsplit = mi.SuffixTextSplit;
                            string[] suffix2textsplit = mi.Suffix2TextSplit;

                            List<float> mainrowwidths = null, suffixrowwidths = null, suffix2rowwidths = null;

                            if (RefreshMenuRowCounts || !mi.TextRowCountsSet)
                            {
                                maintextrows = CountTextSplitRows(maintextsplit, maintext_x_start, canvaswidth, rightmenupadding, textPaint, mi.UseSpecialSymbols, out mainrowwidths);
                                mi.MainTextRows = maintextrows;
                                mi.MainTextRowWidths = mainrowwidths;

                                textPaint.TextSize = suffixfontsize;
                                suffixtextrows = CountTextSplitRows(suffixtextsplit, maintext_x_start, canvaswidth, rightmenupadding, textPaint, mi.UseSpecialSymbols, out suffixrowwidths);
                                mi.SuffixTextRows = suffixtextrows;
                                mi.SuffixTextRowWidths = suffixrowwidths;

                                suffix2textrows = CountTextSplitRows(suffix2textsplit, maintext_x_start, canvaswidth, rightmenupadding, textPaint, mi.UseSpecialSymbols, out suffix2rowwidths);
                                mi.Suffix2TextRows = suffix2textrows;
                                mi.Suffix2TextRowWidths = suffix2rowwidths;

                                mi.TextRowCountsSet = true;
                            }
                            else
                            {
                                maintextrows = mi.MainTextRows;
                                suffixtextrows = mi.SuffixTextRows;
                                suffix2textrows = mi.Suffix2TextRows;
                                mainrowwidths = mi.MainTextRowWidths;
                                suffixrowwidths = mi.SuffixTextRowWidths;
                                suffix2rowwidths = mi.Suffix2TextRowWidths;
                            }
                            textPaint.TextSize = mainfontsize;

                            fontspacingpadding = (textPaint.FontSpacing - (textPaint.FontMetrics.Descent - textPaint.FontMetrics.Ascent)) / 2;
                            float generallinepadding = Math.Max(0.0f, (minrowheight - (textPaint.FontSpacing) * ((float)maintextrows + suffixtextrows * (mi.IsSuffixTextVisible ? relsuffixsize : 0.0f) + (mi.IsSuffix2TextVisible ? relsuffixsize : 0.0f))) / 2);

                            bool isselected = referenceCanvasView.SelectionHow == SelectionMode.Multiple ? mi.Selected :
                                referenceCanvasView.SelectionHow == SelectionMode.Single ? idx == referenceCanvasView.SelectionIndex : false;

                            float totalRowHeight = topPadding + bottomPadding + ((float)maintextrows + suffixtextrows * (mi.IsSuffixTextVisible ? relsuffixsize : 0.0f) + (mi.IsSuffix2TextVisible ? relsuffixsize : 0.0f)) * (textPaint.FontSpacing) + 2 * generallinepadding;
                            float totalRowWidth = canvaswidth - leftmenupadding - rightmenupadding;
                            float totalRowExtraSpacing = IsMiButton ? 12.0f * scale : 0f;

                            if (y + totalRowHeight <= 0 || y >= canvasheight)
                            {
                                /* Just add the total row height */
                                y += totalRowHeight;
                                mi.DrawBounds.Right = mi.DrawBounds.Left + totalRowWidth;
                                mi.DrawBounds.Bottom = mi.DrawBounds.Top + totalRowHeight;
                                y += totalRowExtraSpacing;
                            }
                            else
                            {
                                /* Selection rectangle */
                                SKRect selectionrect = new SKRect(x, y, x + totalRowWidth, y + totalRowHeight);
                                if (IsMiButton)
                                {
                                    canvas.DrawBitmap(isselected || mi.Highlighted ? GHApp.ButtonSelectedBitmap : GHApp.ButtonNormalBitmap, selectionrect, textPaint);
                                }
                                else
                                {
                                    if (isselected)
                                    {
                                        textPaint.Color = _menuHighlightColor;
                                        textPaint.Style = SKPaintStyle.Fill;
                                        canvas.DrawRect(selectionrect, textPaint);
                                    }
                                    else if (mi.Highlighted)
                                    {
                                        textPaint.Color = _menuHighlight2Color;
                                        textPaint.Style = SKPaintStyle.Fill;
                                        canvas.DrawRect(selectionrect, textPaint);
                                    }
                                }

                                float singlelinepadding = Math.Max(0.0f, ((float)(maintextrows - 1) * (textPaint.FontSpacing)) / 2);
                                y += topPadding;
                                y += generallinepadding;
                                y += fontspacingpadding;
                                y -= textPaint.FontMetrics.Ascent;
                                x += leftinnerpadding;

                                if (has_identifiers && !MenuCanvas.HideMenuLetters)
                                {
                                    if (mi.Identifier == 0 && mi.SpecialMark != '\0')
                                        str = mi.FormattedSpecialMark;
                                    else
                                        str = mi.FormattedAccelerator;
                                    textPaint.Color = SKColors.Gray;
                                    str = str.Trim();
                                    float identifier_y =
                                        mi.IsSuffixTextVisible || mi.IsSuffix2TextVisible ? (selectionrect.Top + selectionrect.Bottom) / 2 - (textPaint.FontMetrics.Descent - textPaint.FontMetrics.Ascent) / 2 - textPaint.FontMetrics.Ascent
                                        : y + singlelinepadding;
                                    if (!(y + singlelinepadding + textPaint.FontSpacing + textPaint.FontMetrics.Ascent <= 0 || y + singlelinepadding + textPaint.FontMetrics.Ascent >= canvasheight))
                                        canvas.DrawText(str, x, identifier_y, textPaint);
                                    x += accel_fixed_width;
                                }

                                if (has_pictures)
                                {
                                    x += picturepadding;

                                    /* Icon */
                                    float glyph_start_y = mi.DrawBounds.Top + Math.Max(0, (totalRowHeight - minrowheight) / 2);
                                    if (mi.IsGlyphVisible && !(glyph_start_y + minrowheight <= 0 || glyph_start_y >= canvasheight))
                                    {
                                        using (new SKAutoCanvasRestore(canvas, true))
                                        {
                                            mi.GlyphImageSource.AutoSize = true;
                                            mi.GlyphImageSource.DoAutoSize();
                                            if (mi.GlyphImageSource.Height > 0)
                                            {
                                                float glyphxcenterpadding = (picturewidth - minrowheight * mi.GlyphImageSource.Width / mi.GlyphImageSource.Height) / 2;
                                                canvas.Translate(x + glyphxcenterpadding, glyph_start_y);
                                                canvas.Scale(minrowheight / mi.GlyphImageSource.Height);
                                                mi.GlyphImageSource.DrawOnCanvas(canvas);
                                            }
                                        }
                                    }
                                    x += picturewidth + picturepadding;
                                }
                                else if (!MenuCanvas.HideMenuLetters)
                                {
                                    x += accel_fixed_width; // textPaint.FontMetrics.AverageCharacterWidth;
                                }

                                /* Main text */
                                SKColor maincolor = UIUtils.NHColor2SKColorCore(mi.NHColor, mi.Attributes, MenuCanvas.RevertBlackAndWhite && !IsMiButton, IsMiButton && isselected);
                                textPaint.Color = maincolor;

                                //int split_idx_on_row = -1;
                                bool firstprintonrow = true;
                                float start_x = x;
                                float indent_start_x = start_x;
                                string trimmed_maintext = mi.MainText.Trim();
                                string indentstr = GHUtils.GetIndentationString(trimmed_maintext, mi.Attributes);
                                if (indentstr != "")
                                {
                                    indent_start_x += textPaint.MeasureText(indentstr);
                                }
                                DrawTextSplit(canvas, maintextsplit, mainrowwidths, ref x, ref y, ref firstprintonrow, indent_start_x, canvaswidth, canvasheight, rightmenupadding, textPaint, mi.UseSpecialSymbols, MenuCanvas.UseTextOutline || IsMiButton, MenuCanvas.RevertBlackAndWhite && !IsMiButton, IsMiButton, totalRowWidth, 0, 0, 0, 0);
                                /* Rewind and next line */
                                x = start_x;
                                y += textPaint.FontMetrics.Descent + fontspacingpadding;
                                firstprintonrow = true;

                                /* Suffix text */
                                if (mi.IsSuffixTextVisible)
                                {
                                    textPaint.Color = mi.UseColorForSuffixes ? maincolor : MenuCanvas.RevertBlackAndWhite && !IsMiButton ? _suffixTextColorReverted : _suffixTextColor;
                                    textPaint.TextSize = suffixfontsize;
                                    y += fontspacingpadding;
                                    y -= textPaint.FontMetrics.Ascent;
                                    DrawTextSplit(canvas, suffixtextsplit, suffixrowwidths, ref x, ref y, ref firstprintonrow, start_x, canvaswidth, canvasheight, rightmenupadding, textPaint, mi.UseSpecialSymbols, MenuCanvas.UseTextOutline || IsMiButton, MenuCanvas.RevertBlackAndWhite && !IsMiButton, IsMiButton, totalRowWidth, 0, 0, 0, 0);
                                    /* Rewind and next line */
                                    x = start_x;
                                    y += textPaint.FontMetrics.Descent + fontspacingpadding;
                                    firstprintonrow = true;
                                }

                                /* Suffix 2 text */
                                if (mi.IsSuffix2TextVisible)
                                {
                                    textPaint.Color = mi.UseColorForSuffixes ? maincolor : MenuCanvas.RevertBlackAndWhite && !IsMiButton ? _suffixTextColorReverted : _suffixTextColor;
                                    textPaint.TextSize = suffixfontsize;
                                    fontspacingpadding = (textPaint.FontSpacing - (textPaint.FontMetrics.Descent - textPaint.FontMetrics.Ascent)) / 2;
                                    y += fontspacingpadding;
                                    y -= textPaint.FontMetrics.Ascent;
                                    DrawTextSplit(canvas, suffix2textsplit, suffix2rowwidths, ref x, ref y, ref firstprintonrow, start_x, canvaswidth, canvasheight, rightmenupadding, textPaint, mi.UseSpecialSymbols, MenuCanvas.UseTextOutline || IsMiButton, MenuCanvas.RevertBlackAndWhite && !IsMiButton, IsMiButton, totalRowWidth, 0, 0, 0, 0);
                                    /* Rewind and next line */
                                    x = start_x;
                                    y += textPaint.FontMetrics.Descent + fontspacingpadding;
                                    firstprintonrow = true;
                                }

                                y += generallinepadding;

                                y += bottomPadding;
                                mi.DrawBounds.Bottom = y;
                                mi.DrawBounds.Right = canvaswidth - rightmenupadding;
                                _lastDrawnMenuItemIdx = idx;

                                /* Count circle */
                                if (mi.Count > 0 && !(mi.DrawBounds.Bottom <= 0 || mi.DrawBounds.Top >= canvasheight))
                                {
                                    float circleradius = mi.DrawBounds.Height * 0.90f / 2;
                                    float circlex = mi.DrawBounds.Right - circleradius - 5;
                                    float circley = (mi.DrawBounds.Top + mi.DrawBounds.Bottom) / 2;
                                    textPaint.Color = SKColors.Red;
                                    canvas.DrawCircle(circlex, circley, circleradius, textPaint);
                                    textPaint.TextAlign = SKTextAlign.Center;
                                    textPaint.Color = SKColors.White;
                                    str = mi.Count.ToString();
                                    float maxsize = 1.0f * 2.0f * circleradius / (float)Math.Sqrt(2);
                                    textPaint.TextSize = (float)mi.FontSize * scale;
                                    textPaint.MeasureText(str, ref textBounds);
                                    float scalex = textBounds.Width / maxsize;
                                    float scaley = textBounds.Height / maxsize;
                                    float totscale = Math.Max(scalex, scaley);
                                    textPaint.TextSize = textPaint.TextSize / Math.Max(1.0f, totscale);
                                    canvas.DrawText(str, circlex, circley - (textPaint.FontMetrics.Descent - textPaint.FontMetrics.Ascent) / 2 - textPaint.FontMetrics.Ascent, textPaint);
                                }
                                /* Num items circle */
                                else if (mi.UseNumItems && !(mi.DrawBounds.Bottom <= 0 || mi.DrawBounds.Top >= canvasheight))
                                {
                                    float circleradius = mi.DrawBounds.Height * 0.90f / 2;
                                    float circlex = mi.DrawBounds.Right - circleradius - 5;
                                    float circley = (mi.DrawBounds.Top + mi.DrawBounds.Bottom) / 2;
                                    textPaint.Color = _numItemsBackgroundColor;
                                    textPaint.Style = SKPaintStyle.Fill;
                                    canvas.DrawCircle(circlex, circley, circleradius, textPaint);
                                    textPaint.Style = SKPaintStyle.Fill;
                                    textPaint.TextAlign = SKTextAlign.Center;
                                    textPaint.Color = SKColors.Black;
                                    str = mi.NumItems.ToString();
                                    float maxsize = 1.0f * 2.0f * circleradius / (float)Math.Sqrt(2);
                                    textPaint.TextSize = (float)mi.FontSize * scale;
                                    textPaint.MeasureText(str, ref textBounds);
                                    float scalex = textBounds.Width / maxsize;
                                    float scaley = textBounds.Height / maxsize;
                                    float totscale = Math.Max(scalex, scaley);
                                    textPaint.TextSize = textPaint.TextSize / Math.Max(1.0f, totscale);
                                    canvas.DrawText(str, circlex, circley - (textPaint.FontMetrics.Descent - textPaint.FontMetrics.Ascent) / 2 - textPaint.FontMetrics.Ascent, textPaint);
                                }

                                /* Space between buttons / rows */
                                y += totalRowExtraSpacing;
                            }
                        }
                        if(IsLandscape ? canvaswidth > canvasheight : canvaswidth <= canvasheight)
                            RefreshMenuRowCounts = false;
                    }
                    TotalMenuHeight = y - curmenuoffset;
                }
            }
        }
        private readonly SKColor _numItemsBackgroundColor = new SKColor(228, 203, 158);

        private int CountTextSplitRows(string[] textsplit, float x_start, float canvaswidth, float rightmenupadding, SKPaint textPaint, bool usespecialsymbols, out List<float> rowWidths)
        {
            rowWidths = new List<float>();
            if (textsplit == null)
                return 0;

            int rows = 1;
            float calc_x_start = x_start;
            int rowidx = -1;
            float spacelength = textPaint.MeasureText(" ");
            float curendpos = calc_x_start;

            foreach (string s in textsplit)
            {
                bool nowrap = false;
                if (string.IsNullOrWhiteSpace(s))
                    nowrap = true;
                rowidx++;
                SKBitmap symbolbitmap = null;
                float printlength = 0;
                float marginlength = 0;
                SKRect source_rect = new SKRect();
                if (usespecialsymbols && (symbolbitmap = GHApp.GetSpecialSymbol(s, out source_rect)) != null)
                {
                    float bmpheight = textPaint.FontMetrics.Descent / 2 - textPaint.FontMetrics.Ascent;
                    float bmpwidth = bmpheight * source_rect.Width / Math.Max(1f, source_rect.Height);
                    float bmpmargin = bmpheight / 8;
                    printlength = bmpwidth;
                    marginlength = bmpmargin;
                }
                else
                {
                    printlength = textPaint.MeasureText(s);
                    marginlength = spacelength;
                }
                float endposition = calc_x_start + printlength;
                bool pastend = endposition > canvaswidth - rightmenupadding;
                if (pastend && rowidx > 0 & !nowrap)
                {
                    rowWidths.Add(curendpos - x_start);
                    rows++;
                    curendpos = x_start + printlength;
                    rowidx = 0;
                }
                else
                {
                    curendpos = endposition;
                }
                calc_x_start = curendpos + marginlength;
            }
            rowWidths.Add(curendpos - x_start);
            return rows;
        }

        public SKBitmap GetGameSpecialSymbol(string str, out SKRect source_rect)
        {
            source_rect = new SKRect();
            if (str == null || !str.StartsWith("&"))
                return null;
            else if (str.StartsWith("&status-") && str.Length > 8)
            {
                int status_mark = 0;
                if (int.TryParse(str.Substring(8).Substring(0, str.Length - 8 - 1), out status_mark))
                {
                    int tiles_per_row = GHConstants.TileWidth / GHConstants.StatusMarkWidth;
                    int mglyph = (int)game_ui_tile_types.STATUS_MARKS + GHApp.UITileOff;
                    int mtile = GHApp.Glyph2Tile[mglyph];
                    int sheet_idx = GHApp.TileSheetIdx(mtile);
                    int tile_x = GHApp.TileSheetX(mtile);
                    int tile_y = GHApp.TileSheetY(mtile);
                    int within_tile_x = status_mark % tiles_per_row;
                    int within_tile_y = status_mark / tiles_per_row;
                    int c_x = tile_x + within_tile_x * GHConstants.StatusMarkWidth;
                    int c_y = tile_y + within_tile_y * GHConstants.StatusMarkHeight;

                    source_rect.Left = c_x;
                    source_rect.Right = c_x + GHConstants.StatusMarkWidth;
                    source_rect.Top = c_y;
                    source_rect.Bottom = c_y + GHConstants.StatusMarkHeight;

                    return TileMap[sheet_idx];
                }
                return null;
            }
            else if (str.StartsWith("&cond-") && str.Length > 6)
            {
                int status_mark = 0;
                if (int.TryParse(str.Substring(6).Substring(0, str.Length - 6 - 1), out status_mark))
                {
                    int tiles_per_row = GHConstants.TileWidth / GHConstants.StatusMarkWidth;
                    int mglyph = (int)game_ui_tile_types.CONDITION_MARKS + GHApp.UITileOff;
                    int mtile = GHApp.Glyph2Tile[mglyph];
                    int sheet_idx = GHApp.TileSheetIdx(mtile);
                    int tile_x = GHApp.TileSheetX(mtile);
                    int tile_y = GHApp.TileSheetY(mtile);
                    int within_tile_x = status_mark % tiles_per_row;
                    int within_tile_y = status_mark / tiles_per_row;
                    int c_x = tile_x + within_tile_x * GHConstants.StatusMarkWidth;
                    int c_y = tile_y + within_tile_y * GHConstants.StatusMarkHeight;

                    source_rect.Left = c_x;
                    source_rect.Right = c_x + GHConstants.StatusMarkWidth;
                    source_rect.Top = c_y;
                    source_rect.Bottom = c_y + GHConstants.StatusMarkHeight;

                    return TileMap[sheet_idx];
                }
                return null;
            }
            else if (str.StartsWith("&buff-") && str.Length > 6)
            {
                int propidx = 0;
                if (int.TryParse(str.Substring(6).Substring(0, str.Length - 6 - 1), out propidx))
                {
                    if (propidx <= GHConstants.LAST_PROP)
                    {
                        int tiles_per_row = GHConstants.TileWidth / GHConstants.StatusMarkWidth;
                        int mglyph = (propidx - 1) / GHConstants.BUFFS_PER_TILE + GHApp.BuffTileOff;
                        int mtile = GHApp.Glyph2Tile[mglyph];
                        int sheet_idx = GHApp.TileSheetIdx(mtile);
                        int tile_x = GHApp.TileSheetX(mtile);
                        int tile_y = GHApp.TileSheetY(mtile);

                        int buff_mark = (propidx - 1) % GHConstants.BUFFS_PER_TILE;
                        int within_tile_x = buff_mark % tiles_per_row;
                        int within_tile_y = buff_mark / tiles_per_row;
                        int c_x = tile_x + within_tile_x * GHConstants.StatusMarkWidth;
                        int c_y = tile_y + within_tile_y * GHConstants.StatusMarkHeight;

                        source_rect.Left = c_x;
                        source_rect.Right = c_x + GHConstants.StatusMarkWidth;
                        source_rect.Top = c_y;
                        source_rect.Bottom = c_y + GHConstants.StatusMarkHeight;

                        return TileMap[sheet_idx];
                    }
                }
                return null;
            }
            else
            {
                SKBitmap bitmap = GHApp.GetSpecialSymbol(str, out source_rect);
                return bitmap;
            }
        }

        private void DrawTextSplit(SKCanvas canvas, string[] textsplit, List<float> rowwidths, ref float x, ref float y, ref bool isfirstprintonrow, float indent_start_x, float canvaswidth, float canvasheight, float rightmenupadding, SKPaint textPaint, bool usespecialsymbols, bool usetextoutline, bool revertblackandwhite, bool centertext, float totalrowwidth, float curmenuoffset, float glyphystart, float glyphyend, float glyphpadding)
        {
            if (textsplit == null)
                return;

            float spacelength = textPaint.MeasureText(" ");
            int idx = 0;
            int rowidx = 0;
            foreach (string split_str in textsplit)
            {
                bool nowrap = false;
                if (string.IsNullOrWhiteSpace(split_str))
                    nowrap = true;

                float centering_padding = 0.0f;
                if(centertext && rowwidths != null && rowidx < rowwidths.Count)
                {
                    centering_padding = (totalrowwidth - rowwidths[rowidx]) / 2;
                }

                if(isfirstprintonrow)
                    x += centering_padding;

                float endposition = x;
                float usedglyphpadding = 0.0f;
                if (y - curmenuoffset + textPaint.FontMetrics.Ascent <= glyphyend
                    && y - curmenuoffset + textPaint.FontMetrics.Descent >= glyphystart)
                    usedglyphpadding = glyphpadding;

                SKBitmap symbolbitmap = null;
                SKRect source_rect = new SKRect();
                if(usespecialsymbols && (symbolbitmap = GetGameSpecialSymbol(split_str, out source_rect)) != null)
                {
                    float bmpheight = textPaint.FontMetrics.Descent / 2 - textPaint.FontMetrics.Ascent;
                    float bmpwidth = bmpheight * (float)symbolbitmap.Width / (float)Math.Max(1, symbolbitmap.Height);
                    float bmpmargin = bmpheight / 8;
                    endposition = x + bmpwidth + bmpmargin;
                    bool pastend = x + bmpwidth > canvaswidth - usedglyphpadding - rightmenupadding;
                    if (pastend && !isfirstprintonrow && !nowrap)
                    {
                        x = indent_start_x;
                        y += textPaint.FontSpacing;
                        isfirstprintonrow = true;
                        endposition = x + bmpwidth + bmpmargin;
                    }
                    if (!(y + textPaint.FontSpacing + textPaint.FontMetrics.Ascent <= 0 || y + textPaint.FontMetrics.Ascent >= canvasheight))
                    {
                        float bmpx = x;
                        float bmpy = y + textPaint.FontMetrics.Ascent;
                        SKRect bmptargetrect = new SKRect(bmpx, bmpy, bmpx + bmpwidth, bmpy + bmpheight);
                        canvas.DrawBitmap(symbolbitmap, source_rect, bmptargetrect, textPaint);
                    }
                    isfirstprintonrow = false;
                }
                else
                {
                    float printlength = textPaint.MeasureText(split_str);
                    endposition = x + printlength;
                    if (idx < textsplit.Length - 1)
                        endposition += spacelength;
                    bool pastend = x + printlength > canvaswidth - usedglyphpadding - rightmenupadding;
                    if (pastend && !isfirstprintonrow && !nowrap)
                    {
                        rowidx++;
                        isfirstprintonrow = true;

                        x = indent_start_x;

                        if (centertext && rowwidths != null && rowidx < rowwidths.Count)
                            centering_padding = (totalrowwidth - rowwidths[rowidx]) / 2;
                        x += centering_padding;

                        y += textPaint.FontSpacing;
                        endposition = x + printlength;
                        if(idx < textsplit.Length - 1)
                            endposition += spacelength;
                    }

                    if (!(y + textPaint.FontSpacing + textPaint.FontMetrics.Ascent <= 0 || y + textPaint.FontMetrics.Ascent >= canvasheight))
                    {
                        if(usetextoutline)
                        {
                            SKColor oldcolor = textPaint.Color;
                            textPaint.Color = revertblackandwhite ? SKColors.White : SKColors.Black;
                            textPaint.StrokeWidth = textPaint.TextSize / 10;
                            textPaint.Style = SKPaintStyle.Stroke;
                            canvas.DrawText(split_str, x, y, textPaint);
                            textPaint.Color = oldcolor;
                            textPaint.Style = SKPaintStyle.Fill;
                            textPaint.StrokeWidth = 0;
                        }
                        canvas.DrawText(split_str, x, y, textPaint);
                    }

                    isfirstprintonrow = false;
                }

                x = endposition;
                idx++;
            }
        }


        private readonly object _menuScrollLock = new object();
        private float _menuScrollOffset = 0;
        private float _menuScrollSpeed = 0; /* pixels per second */
        private bool _menuScrollSpeedRecordOn = false;
        private DateTime _menuScrollSpeedStamp;
        List<TouchSpeedRecord> _menuScrollSpeedRecords = new List<TouchSpeedRecord>();
        private bool _menuScrollSpeedOn = false;
        private DateTime _menuScrollSpeedReleaseStamp;

        private Dictionary<long, TouchEntry> MenuTouchDictionary = new Dictionary<long, TouchEntry>();
        private object _savedMenuSender = null;
        private SKTouchEventArgs _savedMenuEventArgs = null;
        private DateTime _savedMenuTimeStamp;
        private bool _menuTouchMoved = false;
        private void MenuCanvas_Touch(object sender, SKTouchEventArgs e)
        {
            lock (_menuDrawOnlyLock)
            {
                if (_menuDrawOnlyClear)
                    return;
            }
            float bottomScrollLimit = Math.Min(0, MenuCanvas.CanvasSize.Height - TotalMenuHeight);
            switch (e?.ActionType)
            {
                case SKTouchAction.Entered:
                    break;
                case SKTouchAction.Pressed:
                    _savedMenuSender = null;
                    _savedMenuEventArgs = null;
                    _savedMenuTimeStamp = DateTime.Now;

                    if (MenuTouchDictionary.ContainsKey(e.Id))
                        MenuTouchDictionary[e.Id] = new TouchEntry(e.Location, DateTime.Now);
                    else
                        MenuTouchDictionary.Add(e.Id, new TouchEntry(e.Location, DateTime.Now));

                    lock(_menuScrollLock)
                    {
                        _menuScrollSpeed = 0;
                        _menuScrollSpeedOn = false;
                        _menuScrollSpeedRecordOn = false;
                        _menuScrollSpeedRecords.Clear();
                    }

                    if (MenuTouchDictionary.Count > 1)
                        _menuTouchMoved = true;
                    else
                    {
                        _savedMenuSender = sender;
                        _savedMenuEventArgs = e;

                        HighlightMenuItems(e.Location);

                        if (MenuCanvas.AllowLongTap)
                        {
                            Device.StartTimer(TimeSpan.FromSeconds(GHConstants.LongMenuTapThreshold), () =>
                            {
                                if (_savedMenuSender == null || _savedMenuEventArgs == null)
                                    return false;
                                DateTime curtime = DateTime.Now;
                                if (curtime - _savedMenuTimeStamp < TimeSpan.FromSeconds(GHConstants.LongMenuTapThreshold * 0.8))
                                    return false; /* Changed touch position */

                                MenuCanvas_LongTap(_savedMenuSender, _savedMenuEventArgs);
                                return false;
                            });
                        }
                    }

                    e.Handled = true;
                    break;
                case SKTouchAction.Moved:
                    {
                        TouchEntry entry;
                        bool res = MenuTouchDictionary.TryGetValue(e.Id, out entry);
                        if (res)
                        {
                            SKPoint anchor = entry.Location;

                            float diffX = e.Location.X - anchor.X;
                            float diffY = e.Location.Y - anchor.Y;
                            float dist = (float)Math.Sqrt((Math.Pow(diffX, 2) + Math.Pow(diffY, 2)));

                            if (MenuTouchDictionary.Count == 1)
                            {
                                /* Just one finger => Scroll the menu */
                                if (diffX != 0 || diffY != 0)
                                {
                                    HighlightMenuItems(e.Location);

                                    DateTime now = DateTime.Now;
                                    /* Do not scroll within button press time threshold, unless large move */
                                    long millisecs_elapsed = (now.Ticks - entry.PressTime.Ticks) / TimeSpan.TicksPerMillisecond;
                                    if (dist > GHConstants.MoveDistanceThreshold || millisecs_elapsed > GHConstants.MoveOrPressTimeThreshold)
                                    {
                                        lock (_menuScrollLock)
                                        {
                                            float stretchLimit = GHConstants.ScrollStretchLimit * MenuCanvas.CanvasSize.Height;
                                            float stretchConstant = GHConstants.ScrollConstantStretch * MenuCanvas.CanvasSize.Height;
                                            float adj_factor = 1.0f;
                                            if (_menuScrollOffset > 0)
                                                adj_factor = _menuScrollOffset >= stretchLimit ? 0 : (1 - ((_menuScrollOffset + stretchConstant) / (stretchLimit + stretchConstant)));
                                            else if (_menuScrollOffset < bottomScrollLimit)
                                                adj_factor = _menuScrollOffset < bottomScrollLimit - stretchLimit ? 0 : (1 - ((bottomScrollLimit - (_menuScrollOffset - stretchConstant)) / (stretchLimit + stretchConstant)));
                                            
                                            float adj_diffY = diffY * adj_factor;
                                            _menuScrollOffset += adj_diffY;
                                            
                                            if (_menuScrollOffset > stretchLimit)
                                                _menuScrollOffset = stretchLimit;
                                            else if (_menuScrollOffset < bottomScrollLimit - stretchLimit)
                                                _menuScrollOffset = bottomScrollLimit - stretchLimit;
                                            else
                                            {
                                                /* Calculate duration since last touch move */
                                                float duration = 0;
                                                if (!_menuScrollSpeedRecordOn)
                                                {
                                                    duration = (float)millisecs_elapsed / 1000f;
                                                    _menuScrollSpeedRecordOn = true;
                                                }
                                                else
                                                {
                                                    duration = ((float)(now.Ticks - _menuScrollSpeedStamp.Ticks) / TimeSpan.TicksPerMillisecond) / 1000f;
                                                }
                                                _menuScrollSpeedStamp = now;

                                                /* Discard speed records to the opposite direction */
                                                if (_menuScrollSpeedRecords.Count > 0)
                                                {
                                                    int prevsgn = Math.Sign(_menuScrollSpeedRecords[0].Distance);
                                                    if (diffY != 0 && prevsgn != 0 && Math.Sign(diffY) != prevsgn)
                                                        _menuScrollSpeedRecords.Clear();
                                                }

                                                /* Add a new speed record */
                                                _menuScrollSpeedRecords.Insert(0, new TouchSpeedRecord(diffY, duration, now));

                                                /* Discard too old records */
                                                while (_menuScrollSpeedRecords.Count > 0)
                                                {
                                                    long lastrecord_ms = (now.Ticks - _menuScrollSpeedRecords[_menuScrollSpeedRecords.Count - 1].TimeStamp.Ticks) / TimeSpan.TicksPerMillisecond;
                                                    if (lastrecord_ms > GHConstants.ScrollRecordThreshold)
                                                        _menuScrollSpeedRecords.RemoveAt(_menuScrollSpeedRecords.Count - 1);
                                                    else
                                                        break;
                                                }

                                                /* Sum up the distances and durations of current records to get an average */
                                                float totaldistance = 0;
                                                float totalsecs = 0;
                                                foreach(TouchSpeedRecord r in _menuScrollSpeedRecords)
                                                {
                                                    totaldistance += r.Distance;
                                                    totalsecs += r.Duration;
                                                }
                                                _menuScrollSpeed = totaldistance / Math.Max(0.001f, totalsecs);
                                                _menuScrollSpeedOn = false;
                                            }
                                        }
                                        MenuTouchDictionary[e.Id].Location = e.Location;
                                        MenuTouchDictionary[e.Id].UpdateTime = DateTime.Now;
                                        if (dist > GHConstants.MoveDistanceThreshold)
                                        {  /* Cancel any press, if long move */
                                            _menuTouchMoved = true;
                                            _savedMenuTimeStamp = DateTime.Now;
                                        }
                                    }
                                }
                            }
                        }
                        e.Handled = true;
                    }
                    break;
                case SKTouchAction.Released:
                    {
                        _savedMenuSender = null;
                        _savedMenuEventArgs = null;
                        _savedMenuTimeStamp = DateTime.Now;

                        ClearHighlightMenuItems();

                        TouchEntry entry;
                        bool res = MenuTouchDictionary.TryGetValue(e.Id, out entry);
                        if (res)
                        {
                            long elapsedms = (DateTime.Now.Ticks - entry.PressTime.Ticks) / TimeSpan.TicksPerMillisecond;

                            if (elapsedms <= GHConstants.MoveOrPressTimeThreshold && !_menuTouchMoved && MenuCanvas.SelectionHow != SelectionMode.None)
                            {
                                MenuCanvas_NormalClickRelease(sender, e);
                            }
                            if (MenuTouchDictionary.ContainsKey(e.Id))
                                MenuTouchDictionary.Remove(e.Id);
                            else
                                MenuTouchDictionary.Clear(); /* Something's wrong; reset the touch dictionary */

                            if (MenuTouchDictionary.Count == 0)
                            {
                                lock(_menuScrollLock)
                                {
                                    long lastrecord_ms = 0;
                                    if(_menuScrollSpeedRecords.Count > 0)
                                    {
                                        lastrecord_ms = (DateTime.Now.Ticks - _menuScrollSpeedRecords[_menuScrollSpeedRecords.Count - 1].TimeStamp.Ticks) / TimeSpan.TicksPerMillisecond;
                                    }

                                    if (_menuScrollOffset > 0 || _menuScrollOffset < bottomScrollLimit)
                                    {
                                        if(lastrecord_ms > GHConstants.ScrollRecordThreshold
                                            || Math.Abs(_menuScrollSpeed) < GHConstants.ScrollSpeedThreshold * MenuCanvas.CanvasSize.Height)
                                            _menuScrollSpeed = 0;

                                        _menuScrollSpeedOn = true;
                                        _menuScrollSpeedReleaseStamp = DateTime.Now;
                                    }
                                    else if(lastrecord_ms > GHConstants.ScrollRecordThreshold)
                                    {
                                        _menuScrollSpeedOn = false;
                                        _menuScrollSpeed = 0;
                                    }
                                    else if (Math.Abs(_menuScrollSpeed) >= GHConstants.ScrollSpeedThreshold * MenuCanvas.CanvasSize.Height)
                                    {
                                        _menuScrollSpeedOn = true;
                                        _menuScrollSpeedReleaseStamp = DateTime.Now;
                                    }
                                    else
                                    {
                                        _menuScrollSpeedOn = false;
                                        _menuScrollSpeed = 0;
                                    }
                                    _menuScrollSpeedRecordOn = false;
                                    _menuScrollSpeedRecords.Clear();
                                }
                                _menuTouchMoved = false;
                            }
                        }
                        e.Handled = true;
                    }
                    break;
                case SKTouchAction.Cancelled:
                    if (MenuTouchDictionary.ContainsKey(e.Id))
                        MenuTouchDictionary.Remove(e.Id);
                    else
                        MenuTouchDictionary.Clear(); /* Something's wrong; reset the touch dictionary */

                    ClearHighlightMenuItems();

                    lock (_menuScrollLock)
                    {
                        if (_menuScrollOffset > 0 || _menuScrollOffset < bottomScrollLimit)
                        {
                            long lastrecord_ms = 0;
                            if (_menuScrollSpeedRecords.Count > 0)
                            {
                                lastrecord_ms = (DateTime.Now.Ticks - _menuScrollSpeedRecords[_menuScrollSpeedRecords.Count - 1].TimeStamp.Ticks) / TimeSpan.TicksPerMillisecond;
                            }

                            if (lastrecord_ms > GHConstants.ScrollRecordThreshold
                                || Math.Abs(_menuScrollSpeed) < GHConstants.ScrollSpeedThreshold * MenuCanvas.CanvasSize.Height)
                                _menuScrollSpeed = 0;

                            _menuScrollSpeedOn = true;
                            _menuScrollSpeedReleaseStamp = DateTime.Now;
                        }
                    }

                    e.Handled = true;
                    break;
                case SKTouchAction.Exited:
                    ClearHighlightMenuItems();
                    break;
                case SKTouchAction.WheelChanged:
                    break;
                default:
                    break;
            }
        }

        private GHMenuItem _countMenuItem = null;
        List<GHNumberPickItem> _countPickList = new List<GHNumberPickItem>();
        private void MenuCanvas_LongTap(object sender, SKTouchEventArgs e)
        {
            int selectedidx = -1;
            bool menuItemSelected = false;
            int menuItemMaxCount = 0;
            string menuItemMainText = "";

            lock (MenuCanvas.MenuItemLock)
            {
                for (int idx = _firstDrawnMenuItemIdx; idx >= 0 && idx <= _lastDrawnMenuItemIdx; idx++)
                {
                    if (idx >= MenuCanvas.MenuItems.Count)
                        return;
                    if (e.Location.Y >= MenuCanvas.MenuItems[idx].DrawBounds.Top && e.Location.Y <= MenuCanvas.MenuItems[idx].DrawBounds.Bottom)
                    {
                        selectedidx = idx;
                        break;
                    }
                }

                if (selectedidx < 0)
                    return;

                if (MenuCanvas.SelectionHow == SelectionMode.None)
                    return;

                if (MenuCanvas.MenuItems[selectedidx].Identifier == 0)
                    return;

                menuItemMaxCount = MenuCanvas.MenuItems[selectedidx].MaxCount;
                if (menuItemMaxCount <= 1)
                    return;

                _countMenuItem = MenuCanvas.MenuItems[selectedidx];
                menuItemSelected = MenuCanvas.MenuItems[selectedidx].Selected;
                menuItemMainText = MenuCanvas.MenuItems[selectedidx].MainText;
            }

            _menuTouchMoved = true; /* No further action upon release */
            if ((MenuCanvas.SelectionHow == SelectionMode.Multiple && !menuItemSelected)
                || (MenuCanvas.SelectionHow == SelectionMode.Single && selectedidx != MenuCanvas.SelectionIndex))
                MenuCanvas_NormalClickRelease(sender, e); /* Normal click selection first */

            if (_countMenuItem.MaxCount > 100)
            {
                MenuCountForegroundGrid.VerticalOptions = LayoutOptions.StartAndExpand;
                MenuCountForegroundGrid.Margin = new Thickness(0, 50, 0, 0);
                CountPicker.IsVisible = false;
                MenuCountEntry.IsVisible = true;
                if (_countMenuItem.Count == -1)
                    MenuCountEntry.Text = _countMenuItem.MaxCount.ToString();
                else
                    MenuCountEntry.Text = _countMenuItem.Count.ToString();
            }
            else
            {
                MenuCountForegroundGrid.VerticalOptions = LayoutOptions.CenterAndExpand;
                MenuCountForegroundGrid.Margin = new Thickness(0, 0, 0, 0);
                CountPicker.IsVisible = true;
                MenuCountEntry.IsVisible = false;
                _countPickList.Clear();
                _countPickList.Add(new GHNumberPickItem(-1, "All"));
                int countselindex = -1;
                if (_countMenuItem.Count == -1)
                    countselindex = 0;
                for (int i = 0; i <= menuItemMaxCount; i++)
                {
                    _countPickList.Add(new GHNumberPickItem(i));
                    if (_countMenuItem.Count == i)
                        countselindex = i + 1;
                }
                CountPicker.ItemsSource = _countPickList;
                CountPicker.ItemDisplayBinding = new Binding("Name");
                CountPicker.SelectedIndex = countselindex;
            }

            MenuCountCaption.Text = (MenuCountEntry.IsVisible ? "Type" : "Select") + " Count for " + menuItemMainText;
            MenuCountBackgroundGrid.IsVisible = true;
        }

        private void ClearHighlightMenuItems()
        {
            if (!MenuCanvas.AllowHighlight)
                return;
            lock (MenuCanvas.MenuItemLock)
            {
                if (MenuCanvas.MenuItems == null)
                    return;

                for (int idx = 0; idx < MenuCanvas.MenuItems.Count; idx++)
                {
                    MenuCanvas.MenuItems[idx].Highlighted = false;
                }
            }
        }

        private void HighlightMenuItems(SKPoint p)
        {
            if (!MenuCanvas.AllowHighlight)
                return;
            lock (MenuCanvas.MenuItemLock)
            {
                if (MenuCanvas.MenuItems == null)
                    return;

                for (int idx = _firstDrawnMenuItemIdx; idx >= 0 && idx <= _lastDrawnMenuItemIdx; idx++)
                {
                    if (idx >= MenuCanvas.MenuItems.Count)
                        break;
                    MenuCanvas.MenuItems[idx].Highlighted = false;
                    if (MenuCanvas.MenuItems[idx].DrawBounds.Contains(p))
                    {
                        GHMenuItem mi = MenuCanvas.MenuItems[idx];
                        if (mi.Identifier != 0 && (mi.IsAutoClickOk || MenuCanvas.ClickOKOnSelection))
                        {
                            if (MenuCanvas.SelectionHow == SelectionMode.Multiple)
                            {
                                if(!mi.Selected)
                                    mi.Highlighted = true;
                            }
                            else if (MenuCanvas.SelectionHow == SelectionMode.Single)
                            {
                                mi.Highlighted = true;
                            }
                        }
                        break;
                    }
                }
            }
        }

        private void MenuCanvas_NormalClickRelease(object sender, SKTouchEventArgs e)
        {
            bool doclickok = false;
            lock (MenuCanvas.MenuItemLock)
            {
                if (MenuCanvas.MenuItems == null)
                    return;

                for (int idx = _firstDrawnMenuItemIdx; idx >= 0 && idx <= _lastDrawnMenuItemIdx; idx++)
                {
                    if (idx >= MenuCanvas.MenuItems.Count)
                        break;
                    if (MenuCanvas.MenuItems[idx].DrawBounds.Contains(e.Location))
                    {
                        GHMenuItem mi = MenuCanvas.MenuItems[idx];
                        if (mi.Identifier == 0)
                        {
                            if (MenuCanvas.SelectionHow == SelectionMode.Multiple && (mi.Flags & (ulong)MenuFlags.MENU_FLAGS_IS_GROUP_HEADING) != 0)
                            {
                                foreach (GHMenuItem o in MenuCanvas.MenuItems)
                                {
                                    if (o.GroupAccelerator == mi.HeadingGroupAccelerator)
                                    {
                                        if (!mi.HeadingUnselectGroup)
                                        {
                                            o.Selected = true;
                                            o.Count = -1;
                                        }
                                        else
                                        {
                                            o.Selected = false;
                                            o.Count = 0;
                                        }
                                    }
                                }
                                mi.HeadingUnselectGroup = !mi.HeadingUnselectGroup;
                            }
                        }
                        else
                        {
                            if (MenuCanvas.SelectionHow == SelectionMode.Multiple)
                            {
                                MenuCanvas.MenuItems[idx].Selected = !MenuCanvas.MenuItems[idx].Selected;
                                if (MenuCanvas.MenuItems[idx].Selected)
                                {
                                    MenuCanvas.MenuItems[idx].Count = -1;
                                    if(MenuCanvas.MenuItems[idx].IsAutoClickOk)
                                        doclickok = true;
                                }
                                else
                                    MenuCanvas.MenuItems[idx].Count = 0;
                            }
                            else
                            {
                                if (idx != MenuCanvas.SelectionIndex && MenuCanvas.SelectionIndex >= 0 && MenuCanvas.SelectionIndex < MenuCanvas.MenuItems.Count)
                                    MenuCanvas.MenuItems[MenuCanvas.SelectionIndex].Count = 0;

                                int oldselidx = MenuCanvas.SelectionIndex;
                                MenuCanvas.SelectionIndex = idx;
                                if (MenuCanvas.MenuItems[idx].Count == 0)
                                    MenuCanvas.MenuItems[idx].Count = -1;

                                /* Else keep the current selection number */
                                if(!MenuOKButton.IsEnabled)
                                    MenuOKButton.IsEnabled = true;

                                if (MenuCanvas.MenuItems[idx].IsAutoClickOk || MenuCanvas.ClickOKOnSelection)
                                    doclickok = true;
                            }
                        }
                        break;
                    }
                }
            }

            if (doclickok)
            {
                MenuCanvas.InvalidateSurface();
                MenuOKButton_Clicked(sender, e);
            }
        }

        private readonly object _menuHideCancelledLock = new object();
        private bool _menuHideCancelled = false;
        private bool _menuHideOn = false;
        private void MenuOKButton_Clicked(object sender, EventArgs e)
        {
            MenuOKButton.IsEnabled = false;
            MenuCancelButton.IsEnabled = false;
            GHApp.PlayButtonClickedSound();

            lock (_menuDrawOnlyLock)
            {
                _menuRefresh = false;
                _menuDrawOnlyClear = true;
            }

            lock (_menuScrollLock)
            {
                _menuScrollOffset = 0;
                _menuScrollSpeed = 0;
                _menuScrollSpeedOn = false;
                _menuScrollSpeedRecords.Clear();
            }

            ConcurrentQueue<GHResponse> queue;
            List<GHMenuItem> resultlist = new List<GHMenuItem>();
            lock (MenuCanvas.MenuItemLock)
            {
                if (MenuCanvas.SelectionHow == SelectionMode.Multiple)
                {
                    foreach (GHMenuItem mi in MenuCanvas.MenuItems)
                    {
                        if (mi.Selected && mi.Count != 0)
                        {
                            resultlist.Add(mi);
                        }
                    }
                }
                else if (MenuCanvas.SelectionHow == SelectionMode.Single)
                {
                    if (MenuCanvas.SelectionIndex > -1 && MenuCanvas.SelectionIndex < MenuCanvas.MenuItems.Count)
                    {
                        GHMenuItem mi = MenuCanvas.MenuItems[MenuCanvas.SelectionIndex];
                        if (mi.Count != 0)
                        {
                            resultlist.Add(mi);
                        }
                    }
                }
            }

            if (GHGame.ResponseDictionary.TryGetValue(_currentGame, out queue))
            {
                queue.Enqueue(new GHResponse(_currentGame, GHRequestType.ShowMenuPage, MenuCanvas.GHWindow, resultlist, false));
            }

            if(!UIUtils.StyleClosesMenuUponDestroy(MenuCanvas.MenuStyle))
                DelayedMenuHide();
        }

        private void MenuCancelButton_Clicked(object sender, EventArgs e)
        {
            MenuOKButton.IsEnabled = false;
            MenuCancelButton.IsEnabled = false;
            GHApp.PlayButtonClickedSound();

            lock (_menuDrawOnlyLock)
            {
                _menuRefresh = false;
                _menuDrawOnlyClear = true;
            }

            lock (_menuScrollLock)
            {
                _menuScrollOffset = 0;
                _menuScrollSpeed = 0;
                _menuScrollSpeedOn = false;
                _menuScrollSpeedRecords.Clear();
            }

            ConcurrentQueue<GHResponse> queue;
            if (GHGame.ResponseDictionary.TryGetValue(_currentGame, out queue))
            {
                queue.Enqueue(new GHResponse(_currentGame, GHRequestType.ShowMenuPage, MenuCanvas.GHWindow, new List<GHMenuItem>(), true));
            }

            if (!UIUtils.StyleClosesMenuUponDestroy(MenuCanvas.MenuStyle))
                DelayedMenuHide();
        }

        private void DelayedMenuHide()
        {
            lock(_menuHideCancelledLock)
            {
                _menuHideCancelled = false;
                _menuHideOn = true;
            }
            if(GHApp.IsiOS)
            {
                if (MenuStack.AnimationIsRunning("MenuShowAnimation"))
                    MenuStack.AbortAnimation("MenuShowAnimation");
                double currentOpacity = MenuStack.Opacity;
                Animation menuAnimation = new Animation(v => MenuStack.Opacity = (double)v, currentOpacity, 0.0);
                menuAnimation.Commit(MenuStack, "MenuHideAnimation", length: 64,
                    rate: 16, repeat: () => false);
                //MenuStack.IsVisible = false;
            }
            Device.StartTimer(TimeSpan.FromSeconds(UIUtils.GetWindowHideSecs()), () =>
            {
                lock (_menuHideCancelledLock)
                {
                    _menuHideOn = false;
                    if (_menuHideCancelled)
                    {
                        _menuHideCancelled = false;
                        return false;
                    }
                }

                MenuGrid.IsVisible = false;
                MainGrid.IsVisible = true;
                if (MenuCanvas.AnimationIsRunning("GeneralAnimationCounter"))
                    MenuCanvas.AbortAnimation("GeneralAnimationCounter");
                MenuWindowGlyphImage.StopAnimation();
                lock (RefreshScreenLock)
                {
                    RefreshScreen = true;
                }
                StartMainCanvasAnimation();

                return false;
            });
        }

        private readonly object _delayedTextHideLock = new object();
        private bool _delayedTextHideOn = false;
        private bool _delayedTextHideCancelled = false;
        private void DelayedTextHide()
        {
            lock (_delayedTextHideLock)
            {
                _delayedTextHideOn = true;
                _delayedTextHideCancelled = false;
            }
            if (GHApp.IsiOS)
            {
                if (TextStack.AnimationIsRunning("TextShowAnimation"))
                    TextStack.AbortAnimation("TextShowAnimation");
                double currentOpacity = TextStack.Opacity;
                Animation textAnimation = new Animation(v => TextStack.Opacity = (double)v, currentOpacity, 0.0);
                textAnimation.Commit(TextStack, "TextHideAnimation", length: 64,
                    rate: 16, repeat: () => false);
                //TextStack.IsVisible = false;
            }
            Device.StartTimer(TimeSpan.FromSeconds(UIUtils.GetWindowHideSecs()), () =>
            {
                lock(_delayedTextHideLock)
                {
                    _delayedTextHideOn = false;
                    if (_delayedTextHideCancelled)
                    {
                        _delayedTextHideCancelled = false;
                        return false;
                    }
                }
                TextGrid.IsVisible = false;
                MainGrid.IsVisible = true;
                TextWindowGlyphImage.StopAnimation();
                lock (_textScrollLock)
                {
                    _textScrollOffset = 0;
                    _textScrollSpeed = 0;
                    _textScrollSpeedOn = false;
                }
                if (TextCanvas.AnimationIsRunning("GeneralAnimationCounter"))
                    TextCanvas.AbortAnimation("GeneralAnimationCounter");
                lock (RefreshScreenLock)
                {
                    RefreshScreen = true;
                }
                StartMainCanvasAnimation();
                return false;
            });
        }

        private bool unselect_on_tap = false;
        private void MenuTapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            if (MenuCanvas.SelectionHow == SelectionMode.Multiple)
            {
                lock (MenuCanvas.MenuItemLock)
                {
                    foreach (GHMenuItem o in MenuCanvas.MenuItems)
                    {
                        if (o.Identifier != 0)
                        {
                            if (!unselect_on_tap)
                            {
                                o.Selected = true;
                                o.Count = -1;
                            }
                            else
                            {
                                o.Selected = false;
                                o.Count = 0;
                            }
                        }
                    }
                    unselect_on_tap = !unselect_on_tap;
                }
            }
        }

        private void MenuCountOkButton_Clicked(object sender, EventArgs e)
        {
            if (_countMenuItem != null)
            {
                if (MenuCountEntry.IsVisible)
                {
                    string str = MenuCountEntry.Text;
                    int value;
                    bool res = int.TryParse(str, out value);
                    if (res)
                    {
                        if (value < 0 || value > _countMenuItem.MaxCount)
                            _countMenuItem.Count = -1;
                        else
                            _countMenuItem.Count = value;
                    }
                    else
                    {
                        MenuCountEntry.TextColor = GHColors.Red;
                        MenuCountEntry.Focus();
                        return;
                    }
                }
                else
                {
                    if (CountPicker.SelectedIndex >= 0 && CountPicker.SelectedIndex < _countPickList.Count)
                    {
                        lock (MenuCanvas.MenuItemLock)
                        {
                            _countMenuItem.Count = _countPickList[CountPicker.SelectedIndex].Number;
                            _countMenuItem.Selected = _countMenuItem.Count != 0;
                        }
                    }
                }
            }
            MenuCountBackgroundGrid.IsVisible = false;
        }

        private void MenuCountCancelButton_Clicked(object sender, EventArgs e)
        {
            MenuCountBackgroundGrid.IsVisible = false;
        }

        private void MenuEntry_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (_countMenuItem != null)
            {
                MenuCountEntry.TextColor = GHColors.White;
            }
        }

        private void MenuEntry_Completed(object sender, EventArgs e)
        {
            if (_countMenuItem != null)
            {
                string str = MenuCountEntry.Text;
                int value;
                bool res = int.TryParse(str, out value);
                if (res)
                {
                    MenuCountEntry.TextColor = GHColors.Green;
                }
                else
                {
                    MenuCountEntry.TextColor = GHColors.Red;
                }
            }
        }


        private GlyphImageSource _textGlyphImageSource = new GlyphImageSource();

        public GlyphImageSource TextGlyphImage
        {
            get
            {
                return _textGlyphImageSource;
            }
        }

        public bool IsTextGlyphVisible
        {
            get
            {
                return (Math.Abs(_textGlyphImageSource.Glyph) > 0 && _textGlyphImageSource.Glyph != GHApp.NoGlyph);
            }
        }

        private readonly object _totalTextHeightLock = new object();
        private float _totalTextHeight = 0;
        private float TotalTextHeight { get { lock (_totalTextHeightLock) { return _totalTextHeight; } } set { lock (_totalTextHeightLock) { _totalTextHeight = value; } } }


        private readonly object _textScrollLock = new object();
        private float _textScrollOffset = 0;
        private float _textScrollSpeed = 0; /* pixels per second */
        private bool _textScrollSpeedRecordOn = false;
        private DateTime _textScrollSpeedStamp;
        List<TouchSpeedRecord> _textScrollSpeedRecords = new List<TouchSpeedRecord>();
        private bool _textScrollSpeedOn = false;
        private DateTime _textScrollSpeedReleaseStamp;

        private Dictionary<long, TouchEntry> TextTouchDictionary = new Dictionary<long, TouchEntry>();
        private object _savedTextSender = null;
        private SKTouchEventArgs _savedTextEventArgs = null;
        private DateTime _savedTextTimeStamp;
        private bool _textTouchMoved = false;

        private void TextCanvas_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            SKImageInfo info = e.Info;
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;
            float canvaswidth = TextCanvas.CanvasSize.Width;
            float canvasheight = TextCanvas.CanvasSize.Height;
            float x = 0, y = 0;
            string str;
            float scale = canvaswidth / (float)TextCanvas.Width;
            canvas.Clear();

            if (canvaswidth <= 16 || canvasheight <= 16)
                return;

            lock (TextCanvas.MenuItemLock)
            {
                if (TextCanvas.PutStrItems == null || TextCanvas.PutStrItems.Count == 0)
                    return;
            }

            using (SKPaint textPaint = new SKPaint())
            {
                if (TextCanvas.GHWindow != null && TextCanvas.GHWindow.Ascension)
                {
                    //float ssize = 10 * scale;
                    //float padding = ssize / 2;
                    //float sspacing = ssize * 5;
                    //int wsparkes = Math.Max(1, (int)Math.Ceiling((canvaswidth - 2 * padding) / sspacing));
                    //int hsparkes = Math.Max(1, (int)Math.Ceiling((canvasheight - 2 * padding) / sspacing));
                    //float wspacing = (canvaswidth - 2 * padding) / wsparkes;
                    //float hspacing = (canvasheight - 2 * padding) / hsparkes;
                    //long df = 2;
                    //long counter;
                    //lock (AnimationTimerLock)
                    //{
                    //    counter = AnimationTimers.general_animation_counter;
                    //}
                    //long ctr_diff = 0;
                    //for (int i = 0; i <= wsparkes; i++)
                    //    UIUtils.DrawSparkle(canvas, textPaint, padding + i * wspacing, padding, ssize, counter - (ctr_diff += df), true);
                    //for (int i = 0; i <= wsparkes; i++)
                    //    UIUtils.DrawSparkle(canvas, textPaint, padding + i * wspacing, canvasheight - padding, ssize, counter - (ctr_diff += df), true);
                    //for (int j = 1; j < hsparkes; j++)
                    //    UIUtils.DrawSparkle(canvas, textPaint, padding, padding + j * hspacing, ssize, counter - (ctr_diff += df), true);
                    //for (int j = 1; j < hsparkes; j++)
                    //    UIUtils.DrawSparkle(canvas, textPaint, canvaswidth - padding, padding + j * hspacing, ssize, counter - (ctr_diff += df), true);

                    long counter;
                    lock (AnimationTimerLock)
                    {
                        counter = AnimationTimers.general_animation_counter;
                    }
                    UIUtils.DrawRandomSparkles(canvas, textPaint, canvaswidth, canvasheight, scale, counter);
                }

                textPaint.Typeface = GHApp.UnderwoodTypeface;
                textPaint.TextSize = 30 * scale;
                textPaint.Style = SKPaintStyle.Fill;
                float minrowheight = textPaint.FontSpacing;
                float leftinnerpadding = 5;
                float curmenuoffset = 0;
                lock (_textScrollLock)
                {
                    curmenuoffset = _textScrollOffset;
                }
                y += curmenuoffset;
                double canvasmaxwidth = TextCanvas.GHWindow != null ? TextCanvas.GHWindow.TextWindowMaximumWidth : GHConstants.DefaultTextWindowMaxWidth;
                double menuwidth = Math.Max(1, Math.Min(TextCanvas.Width, canvasmaxwidth));
                float menuwidthoncanvas = (float)(menuwidth * scale);
                float leftmenupadding = Math.Max(0, (canvaswidth - menuwidthoncanvas) / 2);
                float rightmenupadding = leftmenupadding;
                float topPadding = 0;
                bool wrapglyph = TextCanvas.GHWindow != null ? TextCanvas.GHWindow.WrapGlyph : false;
                float glyphpadding = 0;
                float glyphystart = scale * (float)Math.Max(0.0, TextWindowGlyphImage.Y - TextCanvas.Y);
                float glyphyend = scale * (float)Math.Max(0.0, TextWindowGlyphImage.Y + TextWindowGlyphImage.Height - TextCanvas.Y);

                lock (TextCanvas.TextItemLock)
                {
                    int j = 0;
                    y += topPadding;
                    foreach (GHPutStrItem putstritem in TextCanvas.PutStrItems)
                    {
                        int pos = 0;
                        x = leftmenupadding + leftinnerpadding;
                        x += (float)putstritem.LeftPaddingWidth * scale;
                        textPaint.Typeface = GHApp.GetTypefaceByName(putstritem.TextWindowFontFamily);
                        textPaint.TextSize = (float)putstritem.TextWindowFontSize * scale;
                        /* Heading margin, except on the first row */
                        if(putstritem.InstructionList.Count > 0 && j > 0)
                        {
                            if ((putstritem.InstructionList[0].Attributes & (int)MenuItemAttributes.HalfSize) != 0)
                            {
                                textPaint.TextSize /= 2;
                            }
                            if ((putstritem.InstructionList[0].Attributes & (int)MenuItemAttributes.Heading) != 0)
                            {
                                if ((putstritem.InstructionList[0].Attributes & (int)MenuItemAttributes.Sub) != 0)
                                    y += textPaint.FontSpacing / 3.0f;
                                else
                                    y += textPaint.FontSpacing / 2.0f;
                            }
                            else if ((putstritem.InstructionList[0].Attributes & (int)MenuItemAttributes.Title) != 0)
                            {
                                if ((putstritem.InstructionList[0].Attributes & (int)MenuItemAttributes.Sub) != 0)
                                    y += 0.0f;
                                else
                                    y += textPaint.FontSpacing / 2.0f;
                            }
                        }
                        float fontspacingpadding = (textPaint.FontSpacing - (textPaint.FontMetrics.Descent - textPaint.FontMetrics.Ascent)) / 2;
                        y += fontspacingpadding;
                        y -= textPaint.FontMetrics.Ascent;
                        bool firstprintonrow = true;
                        float start_x = x;
                        float indent_start_x = start_x;
                        string indentstr = putstritem.GetIndentationString();
                        if(indentstr != "")
                        {
                            indent_start_x += textPaint.MeasureText(indentstr);
                        }

                        if (TextWindowGlyphImage.IsVisible && (wrapglyph || (putstritem.InstructionList.Count > 0 && (putstritem.InstructionList[0].Attributes & (int)MenuItemAttributes.Title) != 0)))
                            glyphpadding = scale * (float)Math.Max(0.0, TextCanvas.X + TextCanvas.Width - TextWindowGlyphImage.X);
                        else
                            glyphpadding = 0;

                        foreach (GHPutStrInstructions instr in putstritem.InstructionList)
                        {
                            if (putstritem.Text == null)
                                str = "";
                            else if (pos + instr.PrintLength <= putstritem.Text.Length)
                                str = putstritem.Text.Substring(pos, instr.PrintLength);
                            else if (putstritem.Text.Length - pos > 0)
                                str = putstritem.Text.Substring(pos, putstritem.Text.Length - pos);
                            else
                                str = "";

                            pos += str.Length;

                            textPaint.Color = UIUtils.NHColor2SKColorCore(
                                instr.Color < (int)nhcolor.CLR_MAX ? instr.Color : TextCanvas.RevertBlackAndWhite ? (int)nhcolor.CLR_BLACK : (int)nhcolor.CLR_WHITE, 
                                instr.Attributes,
                                TextCanvas.RevertBlackAndWhite, false);

                            string[] split = str.Split(' ');
                            DrawTextSplit(canvas, split, null, ref x, ref y, ref firstprintonrow, indent_start_x, canvaswidth, canvasheight, rightmenupadding, textPaint, TextCanvas.GHWindow.UseSpecialSymbols, TextCanvas.UseTextOutline, TextCanvas.RevertBlackAndWhite, false, 0, curmenuoffset, glyphystart, glyphyend, glyphpadding);
                        }
                        j++;
                        y += textPaint.FontMetrics.Descent + fontspacingpadding;
                    }
                    TotalTextHeight = y - curmenuoffset;
                }
            }
        }

        private void TextCanvas_Touch(object sender, SKTouchEventArgs e)
        {
            lock (TextCanvas.TextItemLock)
            {
                float bottomScrollLimit = Math.Min(0, TextCanvas.CanvasSize.Height - TotalTextHeight);
                switch (e?.ActionType)
                {
                    case SKTouchAction.Entered:
                        break;
                    case SKTouchAction.Pressed:
                        _savedTextSender = null;
                        _savedTextEventArgs = null;
                        _savedTextTimeStamp = DateTime.Now;

                        if (TextTouchDictionary.ContainsKey(e.Id))
                            TextTouchDictionary[e.Id] = new TouchEntry(e.Location, DateTime.Now);
                        else
                            TextTouchDictionary.Add(e.Id, new TouchEntry(e.Location, DateTime.Now));

                        lock (_textScrollLock)
                        {
                            _textScrollSpeed = 0;
                            _textScrollSpeedOn = false;
                            _textScrollSpeedRecordOn = false;
                            _textScrollSpeedRecords.Clear();
                        }

                        if (TextTouchDictionary.Count > 1)
                            _textTouchMoved = true;
                        else
                        {
                            _savedTextSender = sender;
                            _savedTextEventArgs = e;
                        }

                        e.Handled = true;
                        break;
                    case SKTouchAction.Moved:
                        {
                            TouchEntry entry;
                            bool res = TextTouchDictionary.TryGetValue(e.Id, out entry);
                            if (res)
                            {
                                SKPoint anchor = entry.Location;

                                float diffX = e.Location.X - anchor.X;
                                float diffY = e.Location.Y - anchor.Y;
                                float dist = (float)Math.Sqrt((Math.Pow(diffX, 2) + Math.Pow(diffY, 2)));

                                if (TextTouchDictionary.Count == 1)
                                {
                                    if ((dist > 25 ||
                                        (DateTime.Now.Ticks - entry.PressTime.Ticks) / TimeSpan.TicksPerMillisecond > GHConstants.MoveOrPressTimeThreshold
                                           ))
                                    {
                                        /* Just one finger => Move the map */
                                        if (diffX != 0 || diffY != 0)
                                        {
                                            //lock (_textScrollLock)
                                            //{
                                            //    _textScrollOffset += diffY;
                                            //    if (_textScrollOffset > 0)
                                            //        _textScrollOffset = 0;
                                            //    else if (_textScrollOffset < bottomScrollLimit)
                                            //        _textScrollOffset = bottomScrollLimit;
                                            //}

                                            DateTime now = DateTime.Now;
                                            /* Do not scroll within button press time threshold, unless large move */
                                            long millisecs_elapsed = (now.Ticks - entry.PressTime.Ticks) / TimeSpan.TicksPerMillisecond;
                                            if (dist > GHConstants.MoveDistanceThreshold || millisecs_elapsed > GHConstants.MoveOrPressTimeThreshold)
                                            {
                                                lock (_textScrollLock)
                                                {
                                                    float stretchLimit = GHConstants.ScrollStretchLimit * TextCanvas.CanvasSize.Height;
                                                    float stretchConstant = GHConstants.ScrollConstantStretch * TextCanvas.CanvasSize.Height;
                                                    float adj_factor = 1.0f;
                                                    if (_textScrollOffset > 0)
                                                        adj_factor = _textScrollOffset >= stretchLimit ? 0 : (1 - ((_textScrollOffset + stretchConstant) / (stretchLimit + stretchConstant)));
                                                    else if (_textScrollOffset < bottomScrollLimit)
                                                        adj_factor = _textScrollOffset < bottomScrollLimit - stretchLimit ? 0 : (1 - ((bottomScrollLimit - (_textScrollOffset - stretchConstant)) / (stretchLimit + stretchConstant)));

                                                    float adj_diffY = diffY * adj_factor;
                                                    _textScrollOffset += adj_diffY;

                                                    if (_textScrollOffset > stretchLimit)
                                                        _textScrollOffset = stretchLimit;
                                                    else if (_textScrollOffset < bottomScrollLimit - stretchLimit)
                                                        _textScrollOffset = bottomScrollLimit - stretchLimit;
                                                    else
                                                    {
                                                        /* Calculate duration since last touch move */
                                                        float duration = 0;
                                                        if (!_textScrollSpeedRecordOn)
                                                        {
                                                            duration = (float)millisecs_elapsed / 1000f;
                                                            _textScrollSpeedRecordOn = true;
                                                        }
                                                        else
                                                        {
                                                            duration = ((float)(now.Ticks - _textScrollSpeedStamp.Ticks) / TimeSpan.TicksPerMillisecond) / 1000f;
                                                        }
                                                        _textScrollSpeedStamp = now;

                                                        /* Discard speed records to the opposite direction */
                                                        if (_textScrollSpeedRecords.Count > 0)
                                                        {
                                                            int prevsgn = Math.Sign(_textScrollSpeedRecords[0].Distance);
                                                            if (diffY != 0 && prevsgn != 0 && Math.Sign(diffY) != prevsgn)
                                                                _textScrollSpeedRecords.Clear();
                                                        }

                                                        /* Add a new speed record */
                                                        _textScrollSpeedRecords.Insert(0, new TouchSpeedRecord(diffY, duration, now));

                                                        /* Discard too old records */
                                                        while (_textScrollSpeedRecords.Count > 0)
                                                        {
                                                            long lastrecord_ms = (now.Ticks - _textScrollSpeedRecords[_textScrollSpeedRecords.Count - 1].TimeStamp.Ticks) / TimeSpan.TicksPerMillisecond;
                                                            if (lastrecord_ms > GHConstants.ScrollRecordThreshold)
                                                                _textScrollSpeedRecords.RemoveAt(_textScrollSpeedRecords.Count - 1);
                                                            else
                                                                break;
                                                        }

                                                        /* Sum up the distances and durations of current records to get an average */
                                                        float totaldistance = 0;
                                                        float totalsecs = 0;
                                                        foreach (TouchSpeedRecord r in _textScrollSpeedRecords)
                                                        {
                                                            totaldistance += r.Distance;
                                                            totalsecs += r.Duration;
                                                        }
                                                        _textScrollSpeed = totaldistance / Math.Max(0.001f, totalsecs);
                                                        _textScrollSpeedOn = false;
                                                    }
                                                }
                                                TextTouchDictionary[e.Id].Location = e.Location;
                                                _textTouchMoved = true;
                                                _savedTextTimeStamp = DateTime.Now;
                                            }
                                        }
                                    }
                                }
                            }
                            e.Handled = true;
                        }
                        break;
                    case SKTouchAction.Released:
                        {
                            _savedTextSender = null;
                            _savedTextEventArgs = null;
                            _savedTextTimeStamp = DateTime.Now;

                            TouchEntry entry;
                            bool res = TextTouchDictionary.TryGetValue(e.Id, out entry);
                            if (res)
                            {
                                long elapsedms = (DateTime.Now.Ticks - entry.PressTime.Ticks) / TimeSpan.TicksPerMillisecond;

                                if (elapsedms <= GHConstants.MoveOrPressTimeThreshold && !_textTouchMoved)
                                {
                                    /* Normal click -- Hide the canvas */
                                    GenericButton_Clicked(sender, e, 27);
                                    DelayedTextHide();
                                }
                                if (TextTouchDictionary.ContainsKey(e.Id))
                                    TextTouchDictionary.Remove(e.Id);
                                else
                                    TextTouchDictionary.Clear(); /* Something's wrong; reset the touch dictionary */

                                //if (TextTouchDictionary.Count == 0)
                                //    _textTouchMoved = false;

                                if (TextTouchDictionary.Count == 0)
                                {
                                    lock (_textScrollLock)
                                    {
                                        long lastrecord_ms = 0;
                                        if (_textScrollSpeedRecords.Count > 0)
                                        {
                                            lastrecord_ms = (DateTime.Now.Ticks - _textScrollSpeedRecords[_textScrollSpeedRecords.Count - 1].TimeStamp.Ticks) / TimeSpan.TicksPerMillisecond;
                                        }

                                        if (_textScrollOffset > 0 || _textScrollOffset < bottomScrollLimit)
                                        {
                                            if (lastrecord_ms > GHConstants.ScrollRecordThreshold
                                                || Math.Abs(_textScrollSpeed) < GHConstants.ScrollSpeedThreshold * TextCanvas.CanvasSize.Height)
                                                _textScrollSpeed = 0;

                                            _textScrollSpeedOn = true;
                                            _textScrollSpeedReleaseStamp = DateTime.Now;
                                        }
                                        else if (lastrecord_ms > GHConstants.ScrollRecordThreshold)
                                        {
                                            _textScrollSpeedOn = false;
                                            _textScrollSpeed = 0;
                                        }
                                        else if (Math.Abs(_textScrollSpeed) >= GHConstants.ScrollSpeedThreshold * TextCanvas.CanvasSize.Height)
                                        {
                                            _textScrollSpeedOn = true;
                                            _textScrollSpeedReleaseStamp = DateTime.Now;
                                        }
                                        else
                                        {
                                            _textScrollSpeedOn = false;
                                            _textScrollSpeed = 0;
                                        }
                                        _textScrollSpeedRecordOn = false;
                                        _textScrollSpeedRecords.Clear();
                                    }
                                    _textTouchMoved = false;
                                }
                            }
                            e.Handled = true;
                        }
                        break;
                    case SKTouchAction.Cancelled:
                        if (TextTouchDictionary.ContainsKey(e.Id))
                            TextTouchDictionary.Remove(e.Id);
                        else
                            TextTouchDictionary.Clear(); /* Something's wrong; reset the touch dictionary */

                        lock (_textScrollLock)
                        {
                            if (_textScrollOffset > 0 || _textScrollOffset < bottomScrollLimit)
                            {
                                long lastrecord_ms = 0;
                                if (_textScrollSpeedRecords.Count > 0)
                                {
                                    lastrecord_ms = (DateTime.Now.Ticks - _textScrollSpeedRecords[_textScrollSpeedRecords.Count - 1].TimeStamp.Ticks) / TimeSpan.TicksPerMillisecond;
                                }

                                if (lastrecord_ms > GHConstants.ScrollRecordThreshold
                                    || Math.Abs(_textScrollSpeed) < GHConstants.ScrollSpeedThreshold * TextCanvas.CanvasSize.Height)
                                    _textScrollSpeed = 0;

                                _textScrollSpeedOn = true;
                                _textScrollSpeedReleaseStamp = DateTime.Now;
                            }
                        }

                        e.Handled = true;
                        break;
                    case SKTouchAction.Exited:
                        break;
                    case SKTouchAction.WheelChanged:
                        break;
                    default:
                        break;
                }
            }
        }


        private readonly object _moreCmdLock = new object();
        private int _moreCmdPage = 1;
        private float _moreCmdOffsetX = 0.0f;
        private float _moreCmdOffsetY = 0.0f;
        public int MoreCmdPage { get { lock (_moreCmdLock) { return _moreCmdPage; } } set { lock (_moreCmdLock) { _moreCmdPage = value; } } }
        public float MoreCmdOffsetX { get { lock (_moreCmdLock) { return _moreCmdOffsetX; } } set { lock (_moreCmdLock) { _moreCmdOffsetX = value; } } }
        public float MoreCmdOffsetY { get { lock (_moreCmdLock) { return _moreCmdOffsetY; } } set { lock (_moreCmdLock) { _moreCmdOffsetY = value; } } }
        private float _moreCmdOffsetAutoSpeed = 5.0f; /* Screen widths per second */


        public readonly object CommandButtonLock = new object();
        private Dictionary<long, TouchEntry> CommandTouchDictionary = new Dictionary<long, TouchEntry>();
        private object _savedCommandSender = null;
        private SKTouchEventArgs _savedCommandEventArgs = null;
        private DateTime _savedCommandTimeStamp;
        private bool _commandTouchMoved = false;
        private bool _commandChangedPage = false;

        private readonly object _cmdBtnMatrixRectLock = new object();
        private SKRect _cmdBtnMatrixRect = new SKRect();
        public SKRect CmdBtnMatrixRect { get { SKRect val; lock (_cmdBtnMatrixRectLock) { val = _cmdBtnMatrixRect; } return val; } set { lock (_cmdBtnMatrixRectLock) { _cmdBtnMatrixRect = value; } } }

        private readonly object _mainCounterLock = new object();
        private long _mainCounterValue = 0;
        public long MainCounterValue { get { lock (_mainCounterLock) { return _mainCounterValue; } } }

        private readonly object _mainFPSCounterLock = new object();
        private long _mainFPSCounterValue = 0;

        private readonly object _commandFPSCounterLock = new object();
        private long _commandFPSCounterValue = 0;

        public int CurrentMoreButtonPageMaxNumber { get { return UseSimpleCmdLayout ? GHConstants.MoreButtonPages - 1 : GHConstants.MoreButtonPages; } }

        private void CommandCanvas_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            SKImageInfo info = e.Info;
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;
            float canvaswidth = CommandCanvas.CanvasSize.Width;
            float canvasheight = CommandCanvas.CanvasSize.Height;
            float scale = canvaswidth / (float)CommandCanvas.Width;
            bool isLandscape = canvaswidth > canvasheight;

            canvas.Clear();
            if (canvaswidth <= 16 || canvasheight <= 16)
                return;

            CmdBtnMatrixRect = new SKRect();

            using (SKPaint textPaint = new SKPaint())
            {
                float cmdOffsetX = MoreCmdOffsetX;
                int curpage = MoreCmdPage;
                int pagemin = cmdOffsetX > 0 ? Math.Max(EnableWizardMode ? 0 : 1, curpage - 1) : curpage;
                int pagemax = cmdOffsetX < 0 ? Math.Min(CurrentMoreButtonPageMaxNumber - 1, curpage + 1) : curpage;

                float smalldotheight = Math.Min(canvaswidth, canvasheight) / 120 * scale;
                float largedotheight = smalldotheight * 2;
                float dotmargin = smalldotheight;

                textPaint.Style = SKPaintStyle.Fill;
                for (int i = (EnableWizardMode ? 0 : 1); i < CurrentMoreButtonPageMaxNumber; i++)
                {
                    int numdots = CurrentMoreButtonPageMaxNumber - (EnableWizardMode ? 0 : 1);
                    int dotidx = (EnableWizardMode ? i : i - 1);
                    float dotspacing = dotmargin + largedotheight;
                    float dotoffsetx = ((float)dotidx - ((float)(numdots - 1) / 2)) * dotspacing;
                    SKPoint dotpoint = new SKPoint(canvaswidth / 2 + dotoffsetx, canvasheight - dotmargin - largedotheight / 2);
                    float dotradius = (i == curpage ? largedotheight : smalldotheight) / 2;
                    textPaint.Color = i == curpage ? SKColors.LightGreen : SKColors.White;

                    canvas.DrawCircle(dotpoint, dotradius, textPaint);
                }
                textPaint.Color = SKColors.White;

                float btnMatrixEnd = canvasheight - dotmargin * 2 - largedotheight;
                float titlesize = Math.Min(48f * scale, 19f * 3.0f * Math.Min(canvaswidth, canvasheight) / 1080f);

                for (int page = pagemin; page <= pagemax; page++)
                {
                    float btnOffsetX = cmdOffsetX + canvaswidth * (page - curpage);

                    textPaint.Color = SKColors.White;
                    textPaint.Typeface = GHApp.ImmortalTypeface;
                    textPaint.TextSize = titlesize;
                    textPaint.TextAlign = SKTextAlign.Center;

                    string titlestr = GHApp._moreButtonPageTitle[page];
                    float titletopmargin = 5f * scale;
                    float titley = titletopmargin + textPaint.FontSpacing - textPaint.FontMetrics.Descent;
                    canvas.DrawText(titlestr, new SKPoint(canvaswidth / 2 + btnOffsetX, titley), textPaint);

                    float btnMatrixStart = titletopmargin * 2 + textPaint.FontSpacing;

                    float btnMatrixAreaWidth = canvaswidth;
                    float btnMatrixAreaHeight = btnMatrixEnd - btnMatrixStart;

                    if(page == curpage)
                        CmdBtnMatrixRect = new SKRect(0, btnMatrixStart, btnMatrixAreaWidth, btnMatrixEnd);

                    int usedButtonsPerRow = isLandscape ? GHConstants.MoreButtonsPerColumn : GHConstants.MoreButtonsPerRow;
                    int usedButtonsPerColumn = isLandscape ? GHConstants.MoreButtonsPerRow : GHConstants.MoreButtonsPerColumn;
                    float btnAreaWidth = btnMatrixAreaWidth / usedButtonsPerRow;
                    float btnAreaHeight = btnMatrixAreaHeight / usedButtonsPerColumn;

                    float btnImgRawWidth = Math.Min(256, Math.Min(btnAreaWidth * 0.925f, 128 * scale));

                    textPaint.Color = SKColors.White;
                    textPaint.Typeface = GHApp.LatoRegular;
                    textPaint.TextSize = 12.0f * 3.0f * btnImgRawWidth / 256.0f;
                    textPaint.TextAlign = SKTextAlign.Center;

                    float btnImgRawHeight = Math.Min(256, Math.Min(btnAreaHeight * 0.925f - textPaint.FontSpacing, 128 * scale));

                    float btnImgWidth = Math.Min(btnImgRawWidth, btnImgRawHeight);
                    float btnImgHeight = btnImgWidth;

                    lock (GHApp._moreBtnLock)
                    {
                        for (int i = 0; i < GHConstants.MoreButtonsPerRow; i++)
                        {
                            int pos_j = 0;
                            for (int j = 0; j < GHConstants.MoreButtonsPerColumn; j++)
                            {
                                if (GHApp._moreBtnMatrix[page, i, j] != null && GHApp._moreBtnBitmaps[page, i, j] != null)
                                {
                                    SKRect targetrect = new SKRect();
                                    int x = isLandscape ? pos_j : i;
                                    int y = isLandscape ? i : pos_j;
                                    targetrect.Left = btnOffsetX + x * btnAreaWidth + Math.Max(0, (btnAreaWidth - btnImgWidth) / 2);
                                    targetrect.Top = btnMatrixStart + y * btnAreaHeight + Math.Max(0, (btnAreaHeight - btnImgHeight - textPaint.FontSpacing) / 2);
                                    targetrect.Right = targetrect.Left + btnImgWidth;
                                    targetrect.Bottom = targetrect.Top + btnImgHeight;
                                    float text_x = (targetrect.Left + targetrect.Right) / 2;
                                    float text_y = targetrect.Bottom - textPaint.FontMetrics.Ascent;

                                    canvas.DrawBitmap(GHApp._moreBtnBitmaps[page, i, j], targetrect);
                                    canvas.DrawText(GHApp._moreBtnMatrix[page, i, j].Text, text_x, text_y, textPaint);
                                }
                                pos_j++;
                            }
                        }
                    }
                }

                if (ShowFPS)
                {
                    string str;
                    float textWidth, xText, yText;

                    lock (_fpslock)
                    {
                        str = "FPS: " + string.Format("{0:0.0}", _fps) + ", D:" + _counterValueDiff;
                    }
                    textPaint.Typeface = GHApp.LatoBold;
                    textPaint.TextSize = 26;
                    textPaint.Color = SKColors.Yellow;
                    textWidth = textPaint.MeasureText(str);
                    yText = -textPaint.FontMetrics.Ascent + 5;
                    xText = canvaswidth - textWidth - 5;
                    canvas.DrawText(str, xText, yText, textPaint);
                }
            }
            lock (_commandFPSCounterLock)
            {
                _commandFPSCounterValue++;
                if (_commandFPSCounterValue < 0)
                    _commandFPSCounterValue = 0;
            }
        }

        private void CommandCanvas_Touch(object sender, SKTouchEventArgs e)
        {
            SKRect btnRect = CmdBtnMatrixRect;
            float btnMatrixStart = btnRect.Top;
            float btnMatrixEnd = btnRect.Bottom;
            float btnMatrixWidth = btnRect.Width;
            float btnMatrixHeight = btnRect.Height;
            float canvaswidth = CommandCanvas.CanvasSize.Width;
            float canvasheight = CommandCanvas.CanvasSize.Height;
            float scale = canvaswidth / (float)CommandCanvas.Width;
            bool isLandscape = canvaswidth > canvasheight;

            lock (CommandButtonLock)
            {
                switch (e?.ActionType)
                {
                    case SKTouchAction.Entered:
                        break;
                    case SKTouchAction.Pressed:
                        _savedCommandSender = null;
                        _savedCommandEventArgs = null;
                        _savedCommandTimeStamp = DateTime.Now;

                        if (CommandTouchDictionary.ContainsKey(e.Id))
                            CommandTouchDictionary[e.Id] = new TouchEntry(e.Location, DateTime.Now);
                        else
                            CommandTouchDictionary.Add(e.Id, new TouchEntry(e.Location, DateTime.Now));

                        if (CommandTouchDictionary.Count > 1)
                            _commandTouchMoved = true;
                        else
                        {
                            _savedCommandSender = sender;
                            _savedCommandEventArgs = e;
                            _commandChangedPage = false;
                        }

                        e.Handled = true;
                        break;
                    case SKTouchAction.Moved:
                        {
                            TouchEntry entry;
                            bool res = CommandTouchDictionary.TryGetValue(e.Id, out entry);
                            if (res && !_commandChangedPage)
                            {
                                SKPoint anchor = entry.Location;
                                SKPoint origanchor = entry.OriginalLocation;

                                float diffX = e.Location.X - anchor.X;
                                float diffY = e.Location.Y - anchor.Y;
                                //float dist = (float)Math.Sqrt((Math.Pow(diffX, 2) + Math.Pow(diffY, 2)));
                                float xdist = (float)Math.Abs(diffX);
                                long elapsedms = (DateTime.Now.Ticks - entry.PressTime.Ticks) / TimeSpan.TicksPerMillisecond;
                                int cmdPage = MoreCmdPage;
                                float cmdOffset = MoreCmdOffsetX;

                                if (CommandTouchDictionary.Count == 1)
                                {
                                    if (xdist > 25 || elapsedms > GHConstants.MoveOrPressTimeThreshold)
                                    {
                                        /* Just one finger */
                                        if (diffX != 0 || diffY != 0)
                                        {
                                            int minpage = EnableWizardMode ? 0 : 1;
                                            int maxpage = CurrentMoreButtonPageMaxNumber - 1;
                                            cmdOffset += diffX;
                                            if(cmdPage == minpage && cmdOffset > 0)
                                                MoreCmdOffsetX = cmdOffset = 0;
                                            else if (cmdPage == maxpage && cmdOffset < 0)
                                                MoreCmdOffsetX = cmdOffset = 0;
                                            else
                                                MoreCmdOffsetX = cmdOffset;


                                            CommandTouchDictionary[e.Id].Location = e.Location;
                                            _commandTouchMoved = true;
                                            _savedCommandTimeStamp = DateTime.Now;
                                        }
                                    }
                                }
                            }
                            e.Handled = true;
                        }
                        break;
                    case SKTouchAction.Released:
                        {
                            _savedCommandSender = null;
                            _savedCommandEventArgs = null;
                            _savedCommandTimeStamp = DateTime.Now;
                            _commandChangedPage = false;

                            TouchEntry entry;
                            bool res = CommandTouchDictionary.TryGetValue(e.Id, out entry);
                            if (res)
                            {
                                long elapsedms = (DateTime.Now.Ticks - entry.PressTime.Ticks) / TimeSpan.TicksPerMillisecond;
                                float swipelengththreshold = 30;

                                SKPoint origanchor = entry.OriginalLocation;

                                float origdiffX = e.Location.X - origanchor.X;
                                float origdiffY = e.Location.Y - origanchor.Y;
                                int cmdPage = MoreCmdPage;
                                float cmdOffset = MoreCmdOffsetX;

                                if (elapsedms <= GHConstants.MoveOrPressTimeThreshold && !_commandTouchMoved)
                                {
                                    /* Normal click */
                                    /* Select command here*/
                                    int used_btnHeight = GHConstants.MoreButtonsPerColumn;
                                    int usedButtonsPerRow = isLandscape ? used_btnHeight : GHConstants.MoreButtonsPerRow;
                                    int usedButtonsPerColumn = isLandscape ? GHConstants.MoreButtonsPerRow : used_btnHeight;
                                    float btnAreaWidth = btnMatrixWidth / usedButtonsPerRow;
                                    float btnAreaHeight = btnMatrixHeight / usedButtonsPerColumn;
                                    int btnX = (int)((e.Location.X - MoreCmdOffsetX) / btnAreaWidth);
                                    int btnY = (int)((e.Location.Y - btnMatrixStart) / btnAreaHeight);

                                    if (e.Location.Y >= btnMatrixStart && e.Location.Y <= btnMatrixEnd
                                        && e.Location.X - MoreCmdOffsetX >= 0 && e.Location.X - MoreCmdOffsetX <= canvaswidth
                                        && btnX >= 0 && btnX < usedButtonsPerRow && btnY >= 0 && btnY < usedButtonsPerColumn)
                                    {
                                        int i, j;
                                        if (isLandscape)
                                        {
                                            i = btnY;
                                            j = btnX;
                                        }
                                        else
                                        {
                                            i = btnX;
                                            j = btnY;
                                        }

                                        GHCommandButtonItem cbi = null;
                                        int cbi_cmd = 0;
                                        lock (_moreCmdLock)
                                        {
                                            cbi = GHApp._moreBtnMatrix[MoreCmdPage, i, j];
                                            if (cbi != null)
                                                cbi_cmd = cbi.Command;
                                        }
                                        if (cbi != null)
                                        {
                                            if (cbi_cmd >= 0)
                                                GenericButton_Clicked(CommandCanvas, e, cbi_cmd);
                                            else
                                            {
                                                switch (cbi_cmd)
                                                {
                                                    case -102:
                                                        GenericButton_Clicked(sender, e, 'n');
                                                        GenericButton_Clicked(sender, e, -12);
                                                        GenericButton_Clicked(sender, e, -10);
                                                        GenericButton_Clicked(sender, e, 's');
                                                        break;
                                                    case -103:
                                                        GenericButton_Clicked(sender, e, 'n');
                                                        GenericButton_Clicked(sender, e, -12);
                                                        GenericButton_Clicked(sender, e, -10);
                                                        GenericButton_Clicked(sender, e, -10);
                                                        GenericButton_Clicked(sender, e, '.');
                                                        break;
                                                    case -104:
                                                        GameMenuButton_Clicked(sender, e);
                                                        break;
                                                    case -105:
                                                        GenericButton_Clicked(sender, e, 'n');
                                                        DoShowNumberPad();
                                                        break;
                                                    default:
                                                        break;
                                                }
                                            }

                                            /* Hide the canvas */
                                            MoreCommandsGrid.IsVisible = false;
                                            MainGrid.IsVisible = true;
                                            if (CommandCanvas.AnimationIsRunning("GeneralAnimationCounter"))
                                                CommandCanvas.AbortAnimation("GeneralAnimationCounter");
                                            lock (RefreshScreenLock)
                                            {
                                                RefreshScreen = true;
                                            }
                                            StartMainCanvasAnimation();
                                        }

                                    }
                                }
                                else if (elapsedms <= GHConstants.SwipeTimeThreshold && Math.Abs(origdiffX) > swipelengththreshold)
                                {
                                    /* It is a swipe */
                                    if (origdiffX > swipelengththreshold)
                                    {
                                        if (cmdPage > (EnableWizardMode ? 0 : 1))
                                        {
                                            MoreCmdPage = cmdPage - 1;
                                            MoreCmdOffsetX = cmdOffset - btnMatrixWidth;
                                        }

                                        _commandChangedPage = true;
                                    }
                                    else if (origdiffX < -swipelengththreshold)
                                    {
                                        if (cmdPage < CurrentMoreButtonPageMaxNumber - 1)
                                        {
                                            MoreCmdPage = cmdPage + 1;
                                            MoreCmdOffsetX = cmdOffset + btnMatrixWidth;
                                        }

                                        _commandChangedPage = true;
                                    }
                                }
                                else
                                {
                                    if (cmdOffset > btnMatrixWidth / 2)
                                    {
                                        if (cmdPage > (EnableWizardMode ? 0 : 1))
                                        {
                                            MoreCmdPage = cmdPage - 1;
                                            MoreCmdOffsetX = cmdOffset - btnMatrixWidth;
                                        }

                                        _commandChangedPage = true;
                                    }
                                    else if (cmdOffset < -btnMatrixWidth / 2)
                                    {
                                        if (cmdPage < CurrentMoreButtonPageMaxNumber - 1)
                                        {
                                            MoreCmdPage = cmdPage + 1;
                                            MoreCmdOffsetX = cmdOffset + btnMatrixWidth;
                                        }

                                        _commandChangedPage = true;
                                    }
                                }

                                if (CommandTouchDictionary.ContainsKey(e.Id))
                                    CommandTouchDictionary.Remove(e.Id);
                                else
                                    CommandTouchDictionary.Clear(); /* Something's wrong; reset the touch dictionary */

                                if (CommandTouchDictionary.Count == 0)
                                    _commandTouchMoved = false;
                            }
                            e.Handled = true;
                        }
                        break;
                    case SKTouchAction.Cancelled:
                        if (CommandTouchDictionary.ContainsKey(e.Id))
                            CommandTouchDictionary.Remove(e.Id);
                        else
                            CommandTouchDictionary.Clear(); /* Something's wrong; reset the touch dictionary */
                        e.Handled = true;
                        break;
                    case SKTouchAction.Exited:
                        break;
                    case SKTouchAction.WheelChanged:
                        break;
                    default:
                        break;
                }
            }
        }

        private void ToggleMessageNumberButton_Clicked(object sender, EventArgs e)
        {
            lock(_messageScrollLock)
            {
                _messageScrollOffset = 0.0f;
                _messageScrollSpeed = 0;
                _messageScrollSpeedOn = false;
                _messageScrollSpeedRecords.Clear();
            }

            ForceAllMessages = !ForceAllMessages;
        }

        private readonly object _tipLock = new object();
        private int _shownTip = -1;
        public int ShownTip { get { int val; lock (_tipLock) { val = _shownTip; } return val; } set { lock (_tipLock) { _shownTip = value; } } }
        private bool _blockingTipView = true;
        private void TipView_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            SKImageInfo info = e.Info;
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear(SKColors.Transparent);

            using (SKPaint textPaint = new SKPaint())
            {
                float canvaswidth = canvasView.CanvasSize.Width;
                float canvasheight = canvasView.CanvasSize.Height;
                bool landscape = (canvaswidth > canvasheight);
                float tx = 0, ty = 0;
                SKRect bounds = new SKRect();
                textPaint.Color = SKColors.White;

                string str = "TestText";
                textPaint.Typeface = GHApp.LatoRegular;
                textPaint.TextSize = 36;
                textPaint.MeasureText(str, ref bounds);
                float fontsize = Math.Min(48, 36 * 0.18f / Math.Max(0.01f, (bounds.Width / Math.Max(1, Math.Min(canvaswidth, canvasheight)))));

                str = "This is an explanation.";
                textPaint.Typeface = GHApp.UnderwoodTypeface;
                textPaint.TextSize = 36;
                textPaint.MeasureText(str, ref bounds);
                float centerfontsize = Math.Min(72, 36 * 0.62f / Math.Max(0.01f, (bounds.Width / Math.Max(1, Math.Min(canvaswidth, canvasheight)))));

                float scale_canvas = 1.0f;
                float target_scale_canvas = 1.0f;
                float mult_canvas = 1.0f;
                float prev_bottom = 0;
                float sbheight = GetStatusBarSkiaHeight();
                SKRect statusBarCenterRect = new SKRect(canvaswidth / 2 - sbheight / 2, 0, canvaswidth / 2 + sbheight / 2, sbheight);

                switch (ShownTip)
                {
                    case 0:
                        textPaint.TextSize = 36;
                        textPaint.Typeface = GHApp.ARChristyTypeface;
                        str = "Welcome to GnollHack";
                        textPaint.MeasureText(str, ref bounds);
                        scale_canvas = bounds.Width / Math.Max(1, Math.Min(canvaswidth, canvasheight)); //Math.Max(bounds.Width / canvaswidth, bounds.Height / canvasheight);
                        target_scale_canvas = 0.8f;
                        mult_canvas = target_scale_canvas / scale_canvas;
                        textPaint.TextSize = textPaint.TextSize * mult_canvas;
                        textPaint.MeasureText(str, ref bounds);
                        tx = canvaswidth / 2 - bounds.Width / 2;
                        ty = canvasheight / 2 - bounds.Height / 2;
                        textPaint.Style = SKPaintStyle.Fill;
                        textPaint.Color = SKColors.Black;
                        textPaint.MaskFilter = _blur;
                        canvas.DrawText(str, tx + textPaint.TextSize / 15, ty + textPaint.TextSize / 15, textPaint);
                        textPaint.Style = SKPaintStyle.Fill;
                        textPaint.Color = SKColors.Gold;
                        textPaint.MaskFilter = null;
                        canvas.DrawText(str, tx, ty, textPaint);

                        prev_bottom = ty + textPaint.FontMetrics.Descent;

                        textPaint.TextSize = 36;
                        textPaint.Typeface = GHApp.UnderwoodTypeface;
                        str = "Let's review the user interface";
                        textPaint.MeasureText(str, ref bounds);
                        scale_canvas = bounds.Width / Math.Max(1, Math.Min(canvaswidth, canvasheight)); //Math.Max(bounds.Width / canvaswidth, bounds.Height / canvasheight);
                        target_scale_canvas = 0.8f;
                        mult_canvas = target_scale_canvas / scale_canvas;
                        textPaint.TextSize = textPaint.TextSize * mult_canvas;
                        textPaint.MeasureText(str, ref bounds);
                        tx = canvaswidth / 2 - bounds.Width / 2;
                        ty = prev_bottom - textPaint.FontMetrics.Ascent * 1.2f;
                        textPaint.Style = SKPaintStyle.Fill;
                        textPaint.Color = SKColors.Black;
                        textPaint.MaskFilter = _blur;
                        canvas.DrawText(str, tx + textPaint.TextSize / 15, ty + textPaint.TextSize / 15, textPaint);
                        textPaint.Style = SKPaintStyle.Fill;
                        textPaint.Color = SKColors.White;
                        textPaint.MaskFilter = null;
                        canvas.DrawText(str, tx, ty, textPaint);
                        break;
                    case 1:
                        PaintTipButton(canvas, textPaint, UseSimpleCmdLayout ? SimpleGameMenuButton : GameMenuButton, "This opens the main menu.", "Main Menu", 1.5f, centerfontsize, fontsize, false, -0.15f, 0);
                        break;
                    case 2:
                        PaintTipButton(canvas, textPaint, UseSimpleCmdLayout ? SimpleESCButton : ESCButton, "This cancels any command.", "Escape Button", 1.5f, centerfontsize, fontsize, false, -1.5f, 0);
                        break;
                    case 3:
                        PaintTipButton(canvas, textPaint, UseSimpleCmdLayout ? SimpleToggleAutoCenterModeButton : ToggleAutoCenterModeButton, "This toggles auto-center on player.", "Map Auto-Center", 1.5f, centerfontsize, fontsize, false, -1.5f, 0);
                        break;
                    case 4:
                        PaintTipButton(canvas, textPaint, UseSimpleCmdLayout ? SimpleToggleZoomMiniButton : ToggleZoomMiniButton, "This zoom shows the entire level.", "Minimap", 1.5f, centerfontsize, fontsize, false, landscape ? -0.15f : -0.5f, landscape ? 0 : 1.5f);
                        break;
                    case 5:
                        PaintTipButton(canvas, textPaint, ToggleZoomAlternateButton, "This is the secondary zoom.", "Alternative Zoom", 1.5f, centerfontsize, fontsize, false, landscape ? -1.5f : -0.15f, 0);
                        break;
                    case 6:
                        PaintTipButton(canvas, textPaint, UseSimpleCmdLayout ? SimpleLookModeButton : LookModeButton, "This allows you to inspect the map.", "Look Mode", 1.5f, centerfontsize, fontsize, false, -0.15f, landscape ? -0.5f : 0);
                        break;
                    case 7:
                        PaintTipButton(canvas, textPaint, ToggleTravelModeButton, "Use this to set how you move around.", "Travel Mode", 1.5f, centerfontsize, fontsize, false, landscape ? -1.5f : -0.15f, landscape ? -0.5f : 0);
                        break;
                    case 8:
                        PaintTipButtonByRect(canvas, textPaint, statusBarCenterRect, "You can tap the status bar.", "Open status screen", 1.0f, centerfontsize, fontsize, false, -0.15f, 1.0f);
                        break;
                    case 9:
                        PaintTipButton(canvas, textPaint, lAbilitiesButton, "Some commands do not have buttons.", "Character and game status", 1.0f, centerfontsize, fontsize, true, 0.15f, 1.0f);
                        break;
                    case 10:
                        PaintTipButton(canvas, textPaint, lWornItemsButton, "", "Tap here to access worn items", 1.0f, centerfontsize, fontsize, false, landscape ? -2.0f : -0.5f, 2.0f);
                        break;
                    case 11:
                        PaintTipButton(canvas, textPaint, ToggleMessageNumberButton, "", "Tap here to see more messages", 1.0f, centerfontsize, fontsize, true, 0.5f, -1.0f);
                        break;
                    case 12:
                        PaintTipButtonByRect(canvas, textPaint, HealthRect, "Tapping shows your maximum health.", "Health Orb", 1.1f, centerfontsize, fontsize, true, 0.15f, 0.0f);
                        break;
                    case 13:
                        PaintTipButtonByRect(canvas, textPaint, ManaRect, "Tapping reveals your maximum mana.", "Mana Orb", 1.1f, centerfontsize, fontsize, true, 0.15f, 0.0f);
                        break;
                    case 14:
                        textPaint.TextSize = 36;
                        textPaint.Typeface = GHApp.ARChristyTypeface;
                        str = "You are all set";
                        textPaint.MeasureText(str, ref bounds);
                        scale_canvas = bounds.Width / Math.Max(1, Math.Min(canvaswidth, canvasheight)); //Math.Max(bounds.Width / canvaswidth, bounds.Height / canvasheight);
                        target_scale_canvas = 0.8f;
                        mult_canvas = target_scale_canvas / scale_canvas;
                        textPaint.TextSize = textPaint.TextSize * mult_canvas;
                        textPaint.MeasureText(str, ref bounds);
                        tx = canvaswidth / 2 - bounds.Width / 2;
                        ty = canvasheight / 2 - bounds.Height / 2;
                        textPaint.Style = SKPaintStyle.Fill;
                        textPaint.Color = SKColors.Black;
                        textPaint.MaskFilter = _blur;
                        canvas.DrawText(str, tx + textPaint.TextSize / 15, ty + textPaint.TextSize / 15, textPaint);
                        textPaint.Style = SKPaintStyle.Fill;
                        textPaint.Color = SKColors.Gold;
                        textPaint.MaskFilter = null;
                        canvas.DrawText(str, tx, ty, textPaint);

                        prev_bottom = ty + textPaint.FontMetrics.Descent;

                        textPaint.TextSize = 36;
                        textPaint.Typeface = GHApp.UnderwoodTypeface;
                        str = "Tap to start playing";
                        textPaint.MeasureText(str, ref bounds);
                        scale_canvas = bounds.Width / Math.Max(1, Math.Min(canvaswidth, canvasheight)); //Math.Max(bounds.Width / canvaswidth, bounds.Height / canvasheight);
                        target_scale_canvas = 0.8f;
                        mult_canvas = target_scale_canvas / scale_canvas;
                        textPaint.TextSize = textPaint.TextSize * mult_canvas;
                        textPaint.MeasureText(str, ref bounds);
                        tx = canvaswidth / 2 - bounds.Width / 2;
                        ty = prev_bottom - textPaint.FontMetrics.Ascent * 1.2f;
                        textPaint.Style = SKPaintStyle.Fill;
                        textPaint.Color = SKColors.Black;
                        textPaint.MaskFilter = _blur;
                        canvas.DrawText(str, tx + textPaint.TextSize / 15, ty + textPaint.TextSize / 15, textPaint);
                        textPaint.Style = SKPaintStyle.Fill;
                        textPaint.Color = SKColors.White;
                        textPaint.MaskFilter = null;
                        canvas.DrawText(str, tx, ty, textPaint);
                        break;
                    default:
                        canvas.DrawText(str, tx, ty, textPaint);
                        break;
                }
            }
        }

        private void TipView_Touch(object sender, SKTouchEventArgs e)
        {

            switch (e?.ActionType)
            {
                case SKTouchAction.Entered:
                    e.Handled = true;
                    break;
                case SKTouchAction.Pressed:
                    e.Handled = true;
                    break;
                case SKTouchAction.Moved:
                    e.Handled = true;
                    break;
                case SKTouchAction.Released:
                    ShownTip++;
                    if (ShownTip == 12 && HealthRect.Width == 0)
                        ShownTip++;
                    if (ShownTip == 12 && HealthRect.Width == 0)
                        ShownTip++;
                    if (UseSimpleCmdLayout && (ShownTip == 5 || ShownTip == 7))
                        ShownTip++;
                    TipView.InvalidateSurface();
                    if (ShownTip >= 15 - (_blockingTipView ? 0 : 1))
                    {
                        TipView.IsVisible = false;
                        ShownTip = -1;
                        Preferences.Set("GUITipsShown", true);
                        if (_blockingTipView)
                        {
                            ConcurrentQueue<GHResponse> queue;
                            if (GHGame.ResponseDictionary.TryGetValue(_currentGame, out queue))
                            {
                                queue.Enqueue(new GHResponse(_currentGame, GHRequestType.ShowGUITips));
                            }
                        }
                    }
                    e.Handled = true;
                    break;
                case SKTouchAction.Cancelled:
                    e.Handled = true;
                    break;
                case SKTouchAction.Exited:
                    e.Handled = true;
                    break;
                case SKTouchAction.WheelChanged:
                    e.Handled = true;
                    break;
                default:
                    break;
            }
        }

#if GNH_MAUI
        public SKRect GetViewScreenRect(Microsoft.Maui.Controls.VisualElement view)
#else
        public SKRect GetViewScreenRect(Xamarin.Forms.VisualElement view)
#endif
        {
            float canvaswidth = canvasView.CanvasSize.Width;
            float canvasheight = canvasView.CanvasSize.Height;

            double screenCoordinateX = view.X;
            double screenCoordinateY = view.Y;
            // Get the view's parent (if it has one...)
            if (view.Parent.GetType() != typeof(App))
            {
#if GNH_MAUI
                Microsoft.Maui.Controls.VisualElement parent = (Microsoft.Maui.Controls.VisualElement)view.Parent;
#else
                Xamarin.Forms.VisualElement parent = (Xamarin.Forms.VisualElement)view.Parent;
#endif
                // Loop through all parents
                while (parent != null)
                {
                    screenCoordinateX += parent.X;
                    screenCoordinateY += parent.Y;

                    // If the parent of this parent isn't the app itself, get the parent's parent.
#if GNH_MAUI
                    if (parent.Parent.GetType() == typeof(Microsoft.Maui.Controls.Window))
                        parent = null;
                    else
                        parent = (Microsoft.Maui.Controls.VisualElement)parent.Parent;
#else
                    if (parent.Parent.GetType() == typeof(App))
                        parent = null;
                    else
                        parent = (Xamarin.Forms.VisualElement)parent.Parent;
#endif
                }
            }
            float relX = (float)(screenCoordinateX / canvasView.Width) * canvaswidth;
            float relY = (float)(screenCoordinateY / canvasView.Height) * canvasheight;
            float relWidth = (float)(StandardMeasurementButton.Width / canvasView.Width) * canvaswidth;
            float relHeight = (float)(StandardMeasurementButton.Height / canvasView.Height) * canvasheight;

            SKRect res = new SKRect(relX, relY, relX + relWidth, relY + relHeight);
            return res;
        }

        public void PaintTipButton(SKCanvas canvas, SKPaint textPaint,
#if GNH_MAUI
            Microsoft.Maui.Controls.VisualElement view,
#else
            Xamarin.Forms.VisualElement view, 
#endif
            string centertext, string boxtext, float radius_mult, float centertextfontsize, float boxfontsize, bool linefromright, float lineoffsetx, float lineoffsety)
        {
            SKRect viewrect = GetViewScreenRect(view);
            SKRect tiprect = GetViewScreenRect(TipView);
            SKRect adjustedrect = new SKRect(viewrect.Left - tiprect.Left, viewrect.Top - tiprect.Top, viewrect.Right - tiprect.Left, viewrect.Bottom - tiprect.Top);
            PaintTipButtonByRect(canvas, textPaint, adjustedrect, centertext, boxtext, radius_mult, centertextfontsize, boxfontsize, linefromright, lineoffsetx, lineoffsety);
        }

        public void PaintTipButtonByRect(SKCanvas canvas, SKPaint textPaint, SKRect viewrect, string centertext, string boxtext, float radius_mult, float centertextfontsize, float boxfontsize, bool linefromright, float lineoffsetx, float lineoffsety)
        {
            float canvaswidth = canvasView.CanvasSize.Width;
            float canvasheight = canvasView.CanvasSize.Height;
            float tx = 0, ty = 0;
            SKRect bounds = new SKRect();
            float padding = 0.0f;
            float relX = 0;
            float relY = 0;
            float relWidth = 0;
            float relHeight = 0;
            SKRect rect = new SKRect();
            string str;
            float usedoffsetx = 0;
            float usedoffsety = 0;
            float usedboxwidth = 0;
            float usedboxheight = 0;
            float usedboxpadding = 0;
            float usedlinestartoffset = 0;

            textPaint.TextSize = centertextfontsize;
            textPaint.Typeface = GHApp.UnderwoodTypeface;
            str = centertext;
            textPaint.MeasureText(str, ref bounds);
            tx = canvaswidth / 2 - bounds.Width / 2;
            ty = canvasheight / 2 - bounds.Height / 2;
            textPaint.Style = SKPaintStyle.Fill;
            textPaint.Color = SKColors.Black;
            textPaint.MaskFilter = _blur;
            canvas.DrawText(str, tx + textPaint.TextSize / 15, ty + textPaint.TextSize / 15, textPaint);
            textPaint.Style = SKPaintStyle.Fill;
            textPaint.Color = SKColors.White;
            textPaint.MaskFilter = null;
            canvas.DrawText(str, tx, ty, textPaint);

            relX = viewrect.Left;
            relY = viewrect.Top;
            relWidth = viewrect.Width;
            relHeight = viewrect.Height;

            textPaint.Typeface = GHApp.LatoRegular;
            textPaint.TextSize = boxfontsize;
            str = boxtext;
            textPaint.MeasureText(str, ref bounds);

            usedoffsetx = relWidth * lineoffsetx;
            usedoffsety = relHeight * lineoffsety;
            usedboxpadding = bounds.Height * 0.75f;
            usedboxwidth = bounds.Width + usedboxpadding * 2;
            usedboxheight = bounds.Height + usedboxpadding * 2;
            usedlinestartoffset = linefromright  ? relWidth / 2 * radius_mult  : - relWidth / 2 * radius_mult;

            tx = relX + relWidth / 2;
            ty = relY + relHeight / 2;
            textPaint.Color = SKColors.Red;
            textPaint.Style = SKPaintStyle.Stroke;
            textPaint.StrokeWidth = relWidth / 15;
            canvas.DrawCircle(tx, ty, relWidth / 2 * radius_mult, textPaint);
            textPaint.StrokeWidth = relWidth / 15;
            canvas.DrawLine(tx + usedlinestartoffset + usedoffsetx, ty + usedoffsety, tx + usedlinestartoffset, ty, textPaint);
            textPaint.Color = SKColors.DarkRed;
            textPaint.Style = SKPaintStyle.Fill;
            rect = new SKRect(tx + usedoffsetx + usedlinestartoffset - (linefromright ? 0 : usedboxwidth),
                ty + usedoffsety - usedboxheight / 2,
                tx + usedoffsetx + usedlinestartoffset + (linefromright ? usedboxwidth : 0),
                ty + usedoffsety + usedboxheight / 2);
            canvas.DrawRect(rect, textPaint);
            textPaint.Color = SKColors.Red;
            textPaint.Style = SKPaintStyle.Stroke;
            textPaint.StrokeWidth = relWidth / 25;
            canvas.DrawRect(rect, textPaint);

            padding = (rect.Width - bounds.Width) / 2;
            textPaint.Color = SKColors.White;
            textPaint.Style = SKPaintStyle.Fill;
            canvas.DrawText(str, rect.Left + padding, ty + usedoffsety + (textPaint.FontMetrics.Ascent - textPaint.FontMetrics.Descent) / 2 - textPaint.FontMetrics.Ascent, textPaint);
        }

        private void DrawOrb(SKCanvas canvas, SKPaint textPaint, SKRect orbBorderDest, SKColor fillcolor, string val, string maxval, float orbfillpercentage, bool showmax)
        {
            float orbwidth = orbBorderDest.Width / 230.0f * 210.0f;
            float orbheight = orbBorderDest.Width / 230.0f * 210.0f;
            SKRect orbDest = new SKRect(orbBorderDest.Left + (orbBorderDest.Width - orbwidth) / 2,
                orbBorderDest.Top + (orbBorderDest.Height - orbheight) / 2,
                orbBorderDest.Left + (orbBorderDest.Width - orbwidth) / 2 + orbwidth,
                orbBorderDest.Top + (orbBorderDest.Height - orbheight) / 2 + orbheight);
            textPaint.Color = SKColors.White;
            textPaint.TextAlign = SKTextAlign.Center;
#if GNH_MAP_PROFILING && DEBUG
            StartProfiling(GHProfilingStyle.Bitmap);
#endif
            canvas.DrawBitmap(GHApp._orbBorderBitmap, orbBorderDest, textPaint);
            if (orbfillpercentage < 0)
                orbfillpercentage = 0;
            if (orbfillpercentage > 1)
                orbfillpercentage = 1;
            SKBitmap fillBitmap = fillcolor == SKColors.Red ? GHApp._orbFillBitmapRed : fillcolor == SKColors.Blue ? GHApp._orbFillBitmapBlue : GHApp._orbFillBitmap;
            SKRect orbFillSrc = new SKRect(0.0f, (float)fillBitmap.Height * (1.0f - orbfillpercentage), (float)fillBitmap.Width, (float)fillBitmap.Height);
            SKRect orbFillDest = new SKRect(orbDest.Left, orbDest.Top + orbDest.Height * (1.0f - orbfillpercentage), orbDest.Right, orbDest.Bottom);
            canvas.DrawBitmap(fillBitmap, orbFillSrc, orbFillDest, textPaint);
            canvas.DrawBitmap(GHApp._orbGlassBitmap, orbDest, textPaint);
#if GNH_MAP_PROFILING && DEBUG
            StopProfiling(GHProfilingStyle.Bitmap);
#endif
            if (val != null && val != "")
            {
                textPaint.TextSize = 36;
                textPaint.Typeface = GHApp.LatoBold;
                SKRect bounds = new SKRect();
                textPaint.MeasureText(val.Length > 4 ? val : "9999", ref bounds);
                float scale = bounds.Width / orbwidth;
                if (scale > 0)
                    textPaint.TextSize = textPaint.TextSize * 0.90f / scale;

                float tx = orbDest.Left + orbDest.Width / 2;
                float ty = orbDest.Top + (orbDest.Height - (textPaint.FontMetrics.Descent - textPaint.FontMetrics.Ascent)) / 2 - textPaint.FontMetrics.Ascent;
                textPaint.Style = SKPaintStyle.Stroke;
                textPaint.StrokeWidth = textPaint.TextSize / 10;
                textPaint.Color = SKColors.Black;
#if GNH_MAP_PROFILING && DEBUG
                StartProfiling(GHProfilingStyle.Text);
#endif
                canvas.DrawText(val, tx, ty, textPaint);
                textPaint.Style = SKPaintStyle.Fill;
                textPaint.Color = SKColors.White;
                canvas.DrawText(val, tx, ty, textPaint);
#if GNH_MAP_PROFILING && DEBUG
                StopProfiling(GHProfilingStyle.Text);
#endif
            }

            if (showmax && maxval != null && maxval != "")
            {
                textPaint.TextSize = 36;
                textPaint.Typeface = GHApp.LatoBold;
                SKRect bounds = new SKRect();
                textPaint.MeasureText(maxval.Length > 4 ? maxval : "9999", ref bounds);
                float scale = bounds.Width / orbwidth;
                if (scale > 0)
                    textPaint.TextSize = textPaint.TextSize * 0.50f / scale;

                float tx = orbDest.Left + orbDest.Width / 2;
                float ty = orbDest.Bottom - 0.07f * orbDest.Height - (textPaint.FontMetrics.Descent - textPaint.FontMetrics.Ascent) - textPaint.FontMetrics.Ascent;
                textPaint.Style = SKPaintStyle.Stroke;
                textPaint.StrokeWidth = textPaint.TextSize / 10;
                textPaint.Color = SKColors.Black;
#if GNH_MAP_PROFILING && DEBUG
                StartProfiling(GHProfilingStyle.Text);
#endif
                canvas.DrawText(maxval, tx, ty, textPaint);
                textPaint.Style = SKPaintStyle.Fill;
                textPaint.Color = SKColors.White;
                canvas.DrawText(maxval, tx, ty, textPaint);
#if GNH_MAP_PROFILING && DEBUG
                StopProfiling(GHProfilingStyle.Text);
#endif
            }
            textPaint.TextAlign = SKTextAlign.Left;
        }

        public async void ReportPanic(string text)
        {
            bool answer = await DisplayAlert("Panic", (text != null ? text : "GnollHack has panicked. See the Panic Log.") + 
                "\nDo you want to send a crash report to help the developer fix the cause? This will create a zip archive of the files in your game directory and ask it to be shared further.", 
                "Yes", "No");
            if (answer)
            {
                await GHApp.CreateCrashReport(this);
            }

            ConcurrentQueue<GHResponse> queue;
            if (GHGame.ResponseDictionary.TryGetValue(_currentGame, out queue))
            {
                queue.Enqueue(new GHResponse(_currentGame, GHRequestType.Panic));
            }
        }

        public async void ShowMessage(string text)
        {
            await DisplayAlert("Message", text != null ? text : "No message.", "OK");

            ConcurrentQueue<GHResponse> queue;
            if (GHGame.ResponseDictionary.TryGetValue(_currentGame, out queue))
            {
                queue.Enqueue(new GHResponse(_currentGame, GHRequestType.Message));
            }
        }

        public async void YnConfirmation(string title, string text, string accept, string cancel)
        {
            bool res = await DisplayAlert(title != null ? title : "Confirmation", text != null ? text : "Confirm?",
                accept != null ? accept : "Yes", cancel != null ? cancel : "No");

            ConcurrentQueue<GHResponse> queue;
            if (GHGame.ResponseDictionary.TryGetValue(_currentGame, out queue))
            {
                queue.Enqueue(new GHResponse(_currentGame, GHRequestType.YnConfirmation, res));
            }
        }

        public async void ReportCrashDetected()
        {
            if(GHApp.InformAboutCrashReport)
            {
                bool answer = await DisplayAlert("Crash Detected", "A crashed game has been detected. GnollHack will attempt to restore this game. Also, do you want to create a crash report? This will create a zip archive of the files in your game directory and ask it to be shared further." + (UseMainGLCanvas ? " If the problem persists, try switching GPU Acceleration off in Settings." : ""), "Yes", "No");
                if (answer)
                {
                    await GHApp.CreateCrashReport(this);
                }
            }

            ConcurrentQueue<GHResponse> queue;
            if (GHGame.ResponseDictionary.TryGetValue(_currentGame, out queue))
            {
                queue.Enqueue(new GHResponse(_currentGame, GHRequestType.CrashReport));
            }
        }

        public async Task RunPerformanceTests()
        {
            Debug.WriteLine("Starting Performance Tests");
            await Task.Delay(1000);
            Debug.WriteLine("Hide all Xamarin components");
            WornItemsLayout.IsVisible = false;
            AbilityLayout.IsVisible = false;
            UIGrid.IsVisible = false;
            await Task.Delay(5000);
            WornItemsLayout.IsVisible = true;
            AbilityLayout.IsVisible = true;
            UIGrid.IsVisible = true;
            Debug.WriteLine("Finished Performance Tests");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        uint MakePixel(byte red, byte green, byte blue, byte alpha) =>
        (uint)((alpha << 24) | (blue << 16) | (green << 8) | red);

        private void GetLineEntryText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (GetLineAutoComplete.IsVisible)
                UpdateGetLineAutoComplete();
        }

        private void GetLineAutoCompleteTapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            if(GetLineAutoComplete.Text != "")
            {
                GetLineEntryText.Text = GetLineAutoComplete.Text;
            }
        }

        private void UpdateGetLineAutoComplete()
        {
            if(_getLineStyle == (int)getline_types.GETLINE_EXTENDED_COMMAND)
            {
                if (string.IsNullOrEmpty(GetLineEntryText.Text))
                {
                    GetLineAutoComplete.Text = "";
                    return;
                }

                if (ExtendedCommands == null)
                    return;

                string searchstring = GetLineEntryText.Text.ToLower();
                for (int i = 0; i < ExtendedCommands.Count; i++)
                {
                    string command = ExtendedCommands[i];
                    if (command == null)
                        break;
                    if (command.ToLower().StartsWith(searchstring))
                    {
                        GetLineAutoComplete.Text = command;
                        break;
                    }
                }
            }
        }

        public void SetSimpleLayoutCommandButton(int btnCol, int btnSelectionIndex)
        {
            LabeledImageButton[] _simpleButtions = new LabeledImageButton[6] 
            { lSimpleInventoryButton, lSimpleSearchButton, lSimpleSwapWeaponButton, lSimpleKickButton, lSimpleCastButton, lSimpleRepeatButton };
            if (btnCol < 0 || btnCol >= _simpleButtions.Length) 
                return;
            LabeledImageButton targetButton = _simpleButtions[btnCol];
            if (btnSelectionIndex < 0 || btnSelectionIndex >= GHApp.SelectableShortcutButtons.Count)
                return;
            SelectableShortcutButton sourceButton = GHApp.SelectableShortcutButtons[btnSelectionIndex];
            targetButton.LblText = sourceButton.Label;
            targetButton.BtnCommand = sourceButton.RawCommand;
            targetButton.BtnLetter = sourceButton.Letter;
            targetButton.BtnCtrl = sourceButton.Ctrl;
            targetButton.BtnMeta = sourceButton.Meta;
            targetButton.ImgSourcePath = sourceButton.ImageSourcePath;
        }

        public void StopWaitAndResumeSavedGame()
        {
            if (_currentGame != null)
            {
                ConcurrentQueue<GHResponse> queue;
                if (GHGame.ResponseDictionary.TryGetValue(_currentGame, out queue))
                {
                    queue.Enqueue(new GHResponse(_currentGame, GHRequestType.StopWaitAndRestoreSavedGame));
                }
            }
        }

        public void SaveGameAndWaitForResume()
        {
            if (_currentGame != null)
            {
                ConcurrentQueue<GHResponse> queue;
                if (GHGame.ResponseDictionary.TryGetValue(_currentGame, out queue))
                {
                    queue.Enqueue(new GHResponse(_currentGame, GHRequestType.SaveGameAndWaitForResume));
                }
            }
        }

        public void TallyRealTime()
        {
            if (_currentGame != null)
            {
                ConcurrentQueue<GHResponse> queue;
                if (GHGame.ResponseDictionary.TryGetValue(_currentGame, out queue))
                {
                    queue.Enqueue(new GHResponse(_currentGame, GHRequestType.TallyRealTime));
                }
            }
        }

        public void Suspend()
        {
            if (canvasView.AnimationIsRunning("GeneralAnimationCounter"))
                canvasView.AbortAnimation("GeneralAnimationCounter");
            if (CommandCanvas.AnimationIsRunning("GeneralAnimationCounter"))
                CommandCanvas.AbortAnimation("GeneralAnimationCounter");
            if (MenuCanvas.AnimationIsRunning("GeneralAnimationCounter"))
                MenuCanvas.AbortAnimation("GeneralAnimationCounter");
            MenuWindowGlyphImage.StopAnimation();
            if (TextCanvas.AnimationIsRunning("GeneralAnimationCounter"))
                TextCanvas.AbortAnimation("GeneralAnimationCounter");
            TextWindowGlyphImage.StopAnimation();
            _mapUpdateStopWatch.Stop();
        }

        public void Resume()
        {
            if (MenuGrid.IsVisible)
            {
                if (!MenuCanvas.AnimationIsRunning("GeneralAnimationCounter"))
                    StartMenuCanvasAnimation();
                MenuWindowGlyphImage.CheckStartAnimation();
            }
            else if (TextGrid.IsVisible)
            {
                if (!TextCanvas.AnimationIsRunning("GeneralAnimationCounter"))
                    StartTextCanvasAnimation();
                TextWindowGlyphImage.CheckStartAnimation();
            }
            else if (MoreCommandsGrid.IsVisible && !CommandCanvas.AnimationIsRunning("GeneralAnimationCounter"))
                StartCommandCanvasAnimation();
            else if (!LoadingGrid.IsVisible && MainGrid.IsVisible && !canvasView.AnimationIsRunning("GeneralAnimationCounter"))
                StartMainCanvasAnimation();
        }
    }
}