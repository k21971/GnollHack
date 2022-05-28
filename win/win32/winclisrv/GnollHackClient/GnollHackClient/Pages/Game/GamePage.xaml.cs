﻿using GnollHackCommon;
using Microsoft.AspNetCore.SignalR.Client;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;
using System.Runtime.InteropServices;
using System.Reflection;
using System.IO;
using FFImageLoading.Forms;
using System.Diagnostics;
using System.Collections.ObjectModel;
using GnollHackClient.Controls;
using System.Runtime.CompilerServices;

namespace GnollHackClient.Pages.Game
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GamePage : ContentPage
    {
        private SKRect _canvasButtonRect = new SKRect(0, 0, 0, 0);
        private SKColor _cursorDefaultGreen = new SKColor(0, 255, 0);

        private const float _statusbar_hmargin = 5.0f;
        private const float _statusbar_vmargin = 5.0f;
        private const float _statusbar_rowmargin = 5.0f;
        private const float _statusbar_basefontsize = 14f;
        private const float _statusbar_shieldfontsize = _statusbar_basefontsize * 32f / 42f;
        private const float _statusbar_diffontsize = _statusbar_basefontsize * 24f / 42f;

        public List<string> ExtendedCommands { get; set; }

        private bool _connectionAttempted = false;
        private HubConnection _connection;
        private string _connection_status = "";
        //private string Message { get { lock (MessageLock) { return TopLineLabel.Text; } } set { lock (MessageLock) { TopLineLabel.Text = value; } } }
        //private object MessageLock = new object();
        //private string _message1 = "";

        private string _message = "";
        private string _message2 = "";
        private string _message3 = "";
        private string _message4 = "";
        private string _message5 = "";
        private int _result = 0;
        //private int _result2 = 0;
        private IGnollHackService _gnollHackService;
        public IGnollHackService GnollHackService { get { return _gnollHackService; } }
        private bool _isFirstAppearance = true;
        private Thread _gnhthread;
        private ClientGame _clientGame;
        public ClientGame ClientGame { get { return _clientGame; } }

        private MapData[,] _mapData = new MapData[GHConstants.MapCols, GHConstants.MapRows];
        private object _mapDataLock = new object();
        private int _mapCursorX;
        private int _mapCursorY;

        private object _uLock = new object();
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

        private object _isSizeAllocatedProcessedLock = new object();
        private bool _isSizeAllocatedProcessed = false;
        public bool IsSizeAllocatedProcessed { get { lock (_isSizeAllocatedProcessedLock) { return _isSizeAllocatedProcessed; } } set { lock (_isSizeAllocatedProcessedLock) { _isSizeAllocatedProcessed = value; } } }

        
        private object _forceAsciiLock = new object();
        private bool _forceAscii = false;
        public bool ForceAscii { get { lock (_forceAsciiLock) { return _forceAscii; } } set { lock (_forceAsciiLock) { _forceAscii = value; } } }

        private object _forceAllMessagesLock = new object();
        private bool _forceAllMessages = false;
        public bool ForceAllMessages { get { lock (_forceAllMessagesLock) { return _forceAllMessages; } } set { lock (_forceAllMessagesLock) { _forceAllMessages = value; } } }

        private object _showExtendedStatusBarLock = new object();
        private bool _showExtendedStatusBar = false;
        public bool ShowExtendedStatusBar { get { lock (_showExtendedStatusBarLock) { return _showExtendedStatusBar; } } set { lock (_showExtendedStatusBarLock) { _showExtendedStatusBar = value; } } }

        private object _muteSoundsLock = new object();
        private bool _muteSounds = false;
        public bool MuteSounds { get { lock (_muteSoundsLock) { return _muteSounds; } } set { lock (_muteSoundsLock) { _muteSounds = value; } } }

        public object RefreshScreenLock = new object();
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
        private object _objectDataLock = new object();

        private object _petDataLock = new object();
        private List<GHPetDataItem> _petData = new List<GHPetDataItem>();

        private int _shownMessageRows = GHConstants.DefaultMessageRows;
        public int NumDisplayedMessages { get { return _shownMessageRows; } set { _shownMessageRows = value; } }
        public int ActualDisplayedMessages { get { return ForceAllMessages ? GHConstants.AllMessageRows : NumDisplayedMessages; } }

        private int _shownPetRows = GHConstants.DefaultPetRows;
        public int NumDisplayedPetRows { get { return _shownPetRows; } set { _shownPetRows = value; } }

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

                if(!LoadingGrid.IsVisible)
                    StartMainCanvasAnimation();
            }
        }

        public bool ShowMemoryUsage { get; set; }
        public bool UseMainGLCanvas { get { return canvasView.UseGL; } set { canvasView.UseGL = value; } }
        public bool ShowFPS { get; set; }
        private double _fps;
        private long _counterValueDiff;
        private long _previousMainFPSCounterValue = 0L;
        private long _previousCommandFPSCounterValue = 0L;
        private object _fpslock = new object();
        private Stopwatch _stopWatch = new Stopwatch();

        private Stopwatch _animationStopwatch = new Stopwatch();
        private TimeSpan _previousTimeSpan;

        private object _mapGridLock = new object();
        private bool _mapGrid = false;
        public bool MapGrid { get { lock (_mapGridLock) { return _mapGrid; } } set { lock (_mapGridLock) { _mapGrid = value; } } }

        private object _hitPointBarLock = new object();
        private bool _hitPointBars = false;
        public bool HitPointBars { get { lock (_hitPointBarLock) { return _hitPointBars; } } set { lock (_hitPointBarLock) { _hitPointBars = value; } } }

        private object _orbLock = new object();
        private bool _showOrbs = true;
        public bool ShowOrbs { get { lock (_orbLock) { return _showOrbs; } } set { lock (_orbLock) { _showOrbs = value; } } }
        private bool _showMaxHealthInOrb = false;
        public bool ShowMaxHealthInOrb { get { lock (_orbLock) { return _showMaxHealthInOrb; } } set { lock (_orbLock) { _showMaxHealthInOrb = value; } } }
        private bool _showMaxManaInOrb = false;
        public bool ShowMaxManaInOrb { get { lock (_orbLock) { return _showMaxManaInOrb; } } set { lock (_orbLock) { _showMaxManaInOrb = value; } } }

        private object _playerMarkLock = new object();
        private bool _playerMark = false;
        public bool PlayerMark { get { lock (_playerMarkLock) { return _playerMark; } } set { lock (_playerMarkLock) { _playerMark = value; } } }

        private object _monsterTargetingLock = new object();
        private bool _monsterTargeting = false;
        public bool MonsterTargeting { get { lock (_monsterTargetingLock) { return _monsterTargeting; } } set { lock (_monsterTargetingLock) { _monsterTargeting = value; } } }

        private object _walkArrowLock = new object();
        private bool _walkArrows = true;
        public bool WalkArrows { get { lock (_walkArrowLock) { return _walkArrows; } } set { lock (_walkArrowLock) { _walkArrows = value; } } }

        private object _classicStatusBarLock = new object();
        private bool _classicStatusBar = true;
        public bool ClassicStatusBar { get { lock (_classicStatusBarLock) { return _classicStatusBar; } } set { lock (_classicStatusBarLock) { _classicStatusBar = value; } } }

        private object _showPetsLock = new object();
        private bool _showPets = false;
        public bool ShowPets { get { lock (_showPetsLock) { return _showPets; } } set { lock (_showPetsLock) { _showPets = value; } } }

        private bool _cursorIsOn;
        private bool _showDirections = false;
        private bool _showNumberPad = false;
        private bool ShowNumberPad { get { return _showNumberPad; } set { _showNumberPad = value; } }
        private bool _showWaitIcon = false;
        public bool ShowWaitIcon { get { return _showWaitIcon; } set { _showWaitIcon = value; } }

        public object StatusFieldLock = new object();
        public readonly GHStatusField[] StatusFields = new GHStatusField[(int)statusfields.MAXBLSTATS];

        private MainPage _mainPage;
        private SKBitmap _logoBitmap;
        private SKBitmap _skillBitmap;
        private SKBitmap[] _arrowBitmap = new SKBitmap[9];
        private SKBitmap _orbBorderBitmap;
        private SKBitmap _orbFillBitmap;
        private SKBitmap _orbFillBitmapRed;
        private SKBitmap _orbFillBitmapBlue;
        private SKBitmap _orbGlassBitmap;

        private SKBitmap _statusWizardBitmap;
        private SKBitmap _statusCasualBitmap;
        private SKBitmap _statusCasualClassicBitmap;
        private SKBitmap _statusModernBitmap;

        private SKBitmap _statusDifficultyBitmap;
        private SKBitmap _statusDifficultyVeryEasyBitmap;
        private SKBitmap _statusDifficultyEasyBitmap;
        private SKBitmap _statusDifficultyAverageBitmap;
        private SKBitmap _statusDifficultyHardBitmap;
        private SKBitmap _statusDifficultyExpertBitmap;
        private SKBitmap _statusDifficultyMasterBitmap;
        private SKBitmap _statusDifficultyGrandMasterBitmap;

        private SKBitmap _statusXPLevelBitmap;
        private SKBitmap _statusHDBitmap;
        private SKBitmap _statusACBitmap;
        private SKBitmap _statusMCBitmap;
        private SKBitmap _statusMoveBitmap;
        private SKBitmap _statusWeaponStyleBitmap;

        private SKBitmap _statusGoldBitmap;
        private SKBitmap _statusTurnsBitmap;

        private SKBitmap _statusDungeonLevelBitmap;

        private SKBitmap _searchBitmap;
        private SKBitmap _waitBitmap;

        private SKBitmap _successBitmap;
        private SKBitmap _manaBitmap;
        private SKBitmap _cooldownBitmap;
        private SKBitmap _castsBitmap;
        private SKBitmap _addsBitmap;
        private SKBitmap _foodBitmap;

        private SKBitmap _spellAbjurationBitmap;
        private SKBitmap _spellArcaneBitmap;
        private SKBitmap _spellCelestialBitmap;
        private SKBitmap _spellClericalBitmap;
        private SKBitmap _spellConjurationBitmap;
        private SKBitmap _spellDivinationBitmap;
        private SKBitmap _spellEnchantmentBitmap;
        private SKBitmap _spellHealingBitmap;
        private SKBitmap _spellMovementBitmap;
        private SKBitmap _spellNatureBitmap;
        private SKBitmap _spellNecromancyBitmap;
        private SKBitmap _spellTransmutationBitmap;


        /* Persistent temporary bitmap */
        SKBitmap _tempBitmap = new SKBitmap(GHConstants.TileWidth, GHConstants.TileHeight, SKColorType.Rgba8888, SKAlphaType.Unpremul);

        private object _rectLock = new object(); /* This is to make sure that tipView does not draw domething between clearing the rect and setting it */

        private object _skillRectLock = new object();
        private SKRect _skillRect = new SKRect();
        public SKRect SkillRect { get { SKRect val; lock (_skillRectLock) { val = _skillRect; } return val;  } set { lock (_skillRectLock) { _skillRect = value; } } }

        private object _healthRectLock = new object();
        private SKRect _healthRect = new SKRect();
        public SKRect HealthRect { get { SKRect val; lock (_healthRectLock) { val = _healthRect; } return val; } set { lock (_healthRectLock) { _healthRect = value; } } }

        private object _manaRectLock = new object();
        private SKRect _manaRect = new SKRect();
        public SKRect ManaRect { get { SKRect val; lock (_manaRectLock) { val = _manaRect; } return val; } set { lock (_manaRectLock) { _manaRect = value; } } }

        private object _statusBarRectLock = new object();
        private SKRect _statusBarRect = new SKRect();
        public SKRect StatusBarRect { get { SKRect val; lock (_statusBarRectLock) { val = _statusBarRect; } return val; } set { lock (_statusBarRectLock) { _statusBarRect = value; } } }

        private object _youRectLock = new object();
        private SKRect _youRect = new SKRect();
        public SKRect YouRect { get { SKRect val; lock (_youRectLock) { val = _youRect; } return val; } set { lock (_youRectLock) { _youRect = value; } } }

        public object TargetClipLock = new object();
        public float _originMapOffsetWithNewClipX;
        public float _originMapOffsetWithNewClipY;
        public bool _targetClipOn;
        public long _targetClipStartCounterValue;
        public long _targetClipPanTime;

        private int _clipX;
        private int _clipY;
        public object ClipLock = new object();
        public int ClipX { get { return _clipX; } set { _clipX = value; lock (_mapOffsetLock) { _mapOffsetX = 0; } } }
        public int ClipY { get { return _clipY; } set { _clipY = value; lock (_mapOffsetLock) { _mapOffsetY = 0; } } }
        
        private object _mapNoClipModeLock = new object();
        private bool _mapNoClipMode = false;
        public bool MapNoClipMode { get { lock (_mapNoClipModeLock) { return _mapNoClipMode; } } set { lock (_mapNoClipModeLock) { _mapNoClipMode = value; } } }

        //private object _mapAlternateNoClipModeLock = new object();
        //private bool _mapAlternateNoClipMode = false;
        //public bool MapAlternateNoClipMode { get { lock (_mapAlternateNoClipModeLock) { return _mapAlternateNoClipMode; } } set { lock (_mapAlternateNoClipModeLock) { _mapAlternateNoClipMode = value; } } }

        //private object _zoomChangeCenterModeLock = new object();
        //private bool _zoomChangeCenterMode = false;
        //public bool ZoomChangeCenterMode { get { lock (_zoomChangeCenterModeLock) { return _zoomChangeCenterMode; } } set { lock (_zoomChangeCenterModeLock) { _zoomChangeCenterMode = value; } } }

        private object _mapLookModeLock = new object();
        private bool _mapLookMode = false;
        public bool MapLookMode { get { lock (_mapLookModeLock) { return _mapLookMode; } } set { lock (_mapLookModeLock) { _mapLookMode = value; } } }

        private bool _savedMapTravelMode = false;
        private object _mapTravelModeLock = new object();
        private bool _mapTravelMode = false;
        public bool MapTravelMode { get { lock (_mapTravelModeLock) { return _mapTravelMode; } } set { lock (_mapTravelModeLock) { _mapTravelMode = value; } } }

        public bool MapWalkMode { get { return (!MapTravelMode && !MapLookMode); } }
        public bool ZoomMiniMode { get; set; }
        public bool ZoomAlternateMode { get; set; }


        private float _mapFontSize = GHConstants.MapFontDefaultSize;
        private float _mapFontAlternateSize = GHConstants.MapFontDefaultSize * GHConstants.MapFontRelativeAlternateSize;
        private float _mapFontMiniRelativeSize = 1.0f;
        private object _mapFontSizeLock = new object();
        public float MapFontSize { get { lock (_mapFontSizeLock) { return _mapFontSize; } } set { lock (_mapFontSizeLock) { _mapFontSize = value; } } }
        public float MapFontAlternateSize { get { lock (_mapFontSizeLock) { return _mapFontAlternateSize; } } set { lock (_mapFontSizeLock) { _mapFontAlternateSize = value; } } }
        public float MapFontMiniRelativeSize { get { lock (_mapFontSizeLock) { return _mapFontMiniRelativeSize; } } set { lock (_mapFontSizeLock) { _mapFontMiniRelativeSize = value; } } }

        private float _tileWidth;
        private float _tileHeight;
        private float _mapWidth;
        private float _mapHeight;
        private float _mapFontAscent;
        public object AnimationTimerLock = new object();
        public GHAnimationTimerList AnimationTimers = new GHAnimationTimerList();

        public object Glyph2TileLock = new object();
        public int[] Glyph2Tile { get; set; }
        public byte[] GlyphTileFlags { get; set; }
        public short[] Tile2Animation { get; set; }
        public short[] Tile2Enlargement { get; set; }
        public short[] Tile2Autodraw { get; set; }
        public int[] AnimationOffsets { get; set; }
        public int[] EnlargementOffsets { get; set; }
        public int[] ReplacementOffsets { get; set; }
        public int Glyph2TileSize { get; set; }
        private SKBitmap[] _tileMap = new SKBitmap[GHConstants.MaxTileSheets];
        public SKBitmap[] TileMap { get { return _tileMap; } }
        public int UsedTileSheets { get; set; }
        public int TotalTiles { get; set; }
        public int UnexploredGlyph { get; set; }
        public int NoGlyph { get; set; }
        public int AnimationOff { get; set; }
        public int EnlargementOff { get; set; }
        public int ReplacementOff { get; set; }
        public int GeneralTileOff { get; set; }
        public int HitTileOff { get; set; }
        public int UITileOff { get; set; }
        public int SpellTileOff { get; set; }
        public int SkillTileOff { get; set; }
        public int BuffTileOff { get; set; }
        public int CursorOff { get; set; }


        private int[] _tilesPerRow = new int[GHConstants.MaxTileSheets];
        public int[] TilesPerRow { get { return _tilesPerRow; } }

        private List<AnimationDefinition> _animationDefs = null;
        private List<EnlargementDefinition> _enlargementDefs = null;
        private List<ReplacementDefinition> _replacementDefs = null;
        private List<AutoDrawDefinition> _autodraws = null;

        public List<AnimationDefinition> Animations { get { return _animationDefs; } }
        public List<EnlargementDefinition> Enlargements { get { return _enlargementDefs; } }
        public List<ReplacementDefinition> Replacements { get { return _replacementDefs; } }
        public List<AutoDrawDefinition> Autodraws { get { return _autodraws; } }

        public object _floatingTextLock = new object();
        public List<GHFloatingText> _floatingTexts = new List<GHFloatingText>();
        public object _screenTextLock = new object();
        public GHScreenText _screenText = null;
        public object _conditionTextLock = new object();
        public List<GHConditionText> _conditionTexts = new List<GHConditionText>();
        public object _screenFilterLock = new object();
        public List<GHScreenFilter> _screenFilters = new List<GHScreenFilter>();
        public object _guiEffectLock = new object();
        public List<GHGUIEffect> _guiEffects = new List<GHGUIEffect>();

        private object _enableWizardModeLock = new object();
        private bool _enableWizardMode = false;
        public bool EnableWizardMode { get { lock (_enableWizardModeLock) { return _enableWizardMode; } } set { lock (_enableWizardModeLock) { _enableWizardMode = value; } } }

        private object _enableCasualModeLock = new object();
        private bool _enableCasualMode = false;
        public bool EnableCasualMode { get { lock (_enableCasualModeLock) { return _enableCasualMode; } } set { lock (_enableCasualModeLock) { _enableCasualMode = value; } } }

        private object _enableModernModeLock = new object();
        private bool _enableModernMode = false;
        public bool EnableModernMode { get { lock (_enableModernModeLock) { return _enableModernMode; } } set { lock (_enableModernModeLock) { _enableModernMode = value; } } }

        private List<AddContextMenuData> _contextMenuData = new List<AddContextMenuData>();

        private bool _useMapBitmap = false;
        private object _mapBitmapLock = new object();
        private SKBitmap _mapBitmap = null;

        public GamePage(MainPage mainPage)
        {
            InitializeComponent();
            _mainPage = mainPage;

            CursorStyle = (TTYCursorStyle)Preferences.Get("CursorStyle", 1);
            GraphicsStyle = (GHGraphicsStyle)Preferences.Get("GraphicsStyle", 1);
            MapRefreshRate = (MapRefreshRateStyle)Preferences.Get("MapRefreshRate", (int)ClientUtils.GetDefaultMapFPS());
            ShowFPS = Preferences.Get("ShowFPS", false);
            UseMainGLCanvas = Preferences.Get("UseMainGLCanvas", GHConstants.IsGPUDefault);
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
            Assembly assembly = GetType().GetTypeInfo().Assembly;
            var tasks = new List<Task>();
            tasks.Add(LoadingProgressBar.ProgressTo(0.3, 600, Easing.Linear));
            tasks.Add(Task.Run(() =>
            {
                using (Stream stream = assembly.GetManifestResourceStream("GnollHackClient.Assets.gnollhack_64x96_transparent_32bits.png"))
                {
                    _tileMap[0] = SKBitmap.Decode(stream);
                }
            }));
            await Task.WhenAll(tasks);
            tasks.Clear();

            tasks.Add(LoadingProgressBar.ProgressTo(0.4, 100, Easing.Linear));
            tasks.Add(Task.Run(() =>
            {
                using (Stream stream = assembly.GetManifestResourceStream("GnollHackClient.Assets.gnollhack_64x96_transparent_32bits-2.png"))
                {
                    _tileMap[1] = SKBitmap.Decode(stream);
                }
            }));
            await Task.WhenAll(tasks);
            tasks.Clear();

            tasks.Add(LoadingProgressBar.ProgressTo(0.5, 100, Easing.Linear));
            tasks.Add(Task.Run(() =>
            {
                using (Stream stream = assembly.GetManifestResourceStream("GnollHackClient.Assets.gnollhack-logo-test-2.png"))
                {
                    _logoBitmap = SKBitmap.Decode(stream);
                }
            }));
            await Task.WhenAll(tasks);
            tasks.Clear();

            tasks.Add(LoadingProgressBar.ProgressTo(0.6, 100, Easing.Linear));
            tasks.Add(Task.Run(() =>
            {
                using (Stream stream = assembly.GetManifestResourceStream("GnollHackClient.Assets.UI.skill.png"))
                {
                    _skillBitmap = SKBitmap.Decode(stream);
                }

                InitializeArrowButtons(assembly);
                InitializeUIBitmaps(assembly);
                InitializeMoreCommandButtons();

                _gnollHackService = DependencyService.Get<IGnollHackService>();
                _gnollHackService.InitializeGnollHack(null);
                UnexploredGlyph = _gnollHackService.GetUnexploredGlyph();
                NoGlyph = _gnollHackService.GetNoGlyph();

                int animoff, enloff, reoff, general_tile_off, hit_tile_off, ui_tile_off, spell_tile_off, skill_tile_off, buff_tile_off, cursor_off;
                _gnollHackService.GetOffs(out animoff, out enloff, out reoff, out general_tile_off, out hit_tile_off, out ui_tile_off, out spell_tile_off, out skill_tile_off, out buff_tile_off,
                    out cursor_off);
                AnimationOff = animoff;
                EnlargementOff = enloff;
                ReplacementOff = reoff;
                GeneralTileOff = general_tile_off;
                HitTileOff = hit_tile_off;
                UITileOff = ui_tile_off;
                SpellTileOff = spell_tile_off;
                SkillTileOff = skill_tile_off;
                BuffTileOff = buff_tile_off;
                CursorOff = cursor_off;

                ExtendedCommands = _gnollHackService.GetExtendedCommands();

            }));
            await Task.WhenAll(tasks);
            tasks.Clear();

            tasks.Add(LoadingProgressBar.ProgressTo(0.7, 100, Easing.Linear));
            tasks.Add(Task.Run(() =>
            {
                _animationDefs = _gnollHackService.GetAnimationArray();
                _enlargementDefs = _gnollHackService.GetEnlargementArray();
            }));
            await Task.WhenAll(tasks);
            tasks.Clear();

            tasks.Add(LoadingProgressBar.ProgressTo(0.80, 100, Easing.Linear));
            tasks.Add(Task.Run(() =>
            {
                _replacementDefs = _gnollHackService.GetReplacementArray();
                _autodraws = _gnollHackService.GetAutoDrawArray();
            }));
            await Task.WhenAll(tasks);
            tasks.Clear();

            tasks.Add(LoadingProgressBar.ProgressTo(0.90, 100, Easing.Linear));
            tasks.Add(Task.Run(() =>
            {

                SetLayerDrawOrder();

                for (int i = 0; i < GHConstants.MapCols; i++)
                {
                    for (int j = 0; j < GHConstants.MapRows; j++)
                    {
                        _mapData[i, j] = new MapData();
                        _mapData[i, j].Glyph = UnexploredGlyph;
                        _mapData[i, j].BkGlyph = NoGlyph;
                        _mapData[i, j].NeedsUpdate = true;

                        _objectData[i, j] = new ObjectData();
                    }
                }
            }));
            await Task.WhenAll(tasks);
            tasks.Clear();

            await LoadingProgressBar.ProgressTo(0.95, 50, Easing.Linear);

            if (App.IsServerGame)
            {
                _connectionAttempted = true;
                _connection_status = "Not connected";
                _message = "Please wait...";

                if (_connection == null)
                {
                    ConnectToServer();
                }
                else if (_connection.State != HubConnectionState.Connected)
                {
                    await _connection.StopAsync();
                    ConnectToServer();
                }
                else
                {
                    _connection_status = "Connected";
                }

                if (_connection != null)
                {
                    LoginToServer();
                }
            }
            else
            {
                Thread t = new Thread(new ThreadStart(GNHThreadProc));
                _gnhthread = t;
                _gnhthread.Start();
            }

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

            Device.StartTimer(TimeSpan.FromSeconds(1.0 / GHConstants.PollingFrequency), () =>
            {
                if(!StartingPositionsSet && !canvasView.CanvasSize.IsEmpty && IsSizeAllocatedProcessed && lAbilitiesButton.Width > 0)
                {
                    double statusbarheight = GetStatusBarHeight();
                    lAbilitiesButton.HeightRequest = statusbarheight;
                    lWornItemsButton.HeightRequest = statusbarheight;
                    UpperCmdLayout.Margin = new Thickness(0, statusbarheight, 0, 0);
                    StartingPositionsSet = true;
                }

                pollRequestQueue();

                return true;
            });

            Device.StartTimer(TimeSpan.FromSeconds(0.5), () =>
            {
                if (ClientGame != null)
                    _cursorIsOn = !_cursorIsOn;
                else
                    _cursorIsOn = false;

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

                return true;
            });

            await LoadingProgressBar.ProgressTo(1.0, 20, Easing.Linear);
        }

        public void UpdateCommandCanvas()
        {
            if (MoreCommandsGrid.IsVisible)
            {
                CommandCanvas.InvalidateSurface();
                float offx = MoreCmdOffsetX;
                if (offx != 0 && (CommandTouchDictionary.Count == 0 || _commandChangedPage))
                {
                    float delta = -1 * Math.Sign(offx) * CommandCanvas.CanvasSize.Width * _moreCmdOffsetAutoSpeed / ClientUtils.GetAuxiliaryCanvasAnimationFrequency();
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
                    MenuCanvas.InvalidateSurface();
            }
        }

        public void UpdateTextCanvas()
        {
            if (TextGrid.IsVisible)
            {
                TextCanvas.InvalidateSurface();
            }
        }

        private uint _auxAnimationLength = GHConstants.AuxiliaryCanvasAnimationTime / ClientUtils.GetAuxiliaryCanvasAnimationInterval();
        private void StartMainCanvasAnimation()
        {
            uint mainAnimationLength = 20; // GHConstants.MainCanvasAnimationTime / ClientUtils.GetMainCanvasAnimationInterval(MapRefreshRate);
            Animation canvasAnimation = new Animation(v => canvasView.GeneralAnimationCounter = (long)v, 1, mainAnimationLength);
            canvasAnimation.Commit(canvasView, "GeneralAnimationCounter", length: GHConstants.MainCanvasAnimationTime, 
                rate: ClientUtils.GetMainCanvasAnimationInterval(MapRefreshRate), repeat: () => true /* MainGrid.IsVisible */);
        }

        private void StartCommandCanvasAnimation()
        {
            Animation commandAnimation = new Animation(v => CommandCanvas.GeneralAnimationCounter = (long)v, 1, _auxAnimationLength);
            commandAnimation.Commit(CommandCanvas, "GeneralAnimationCounter", length: GHConstants.AuxiliaryCanvasAnimationTime, 
                rate: ClientUtils.GetAuxiliaryCanvasAnimationInterval(), repeat: () => true /* MoreCommandsGrid.IsVisible */);
        }

        private void StartMenuCanvasAnimation()
        {
            Animation commandAnimation = new Animation(v => MenuCanvas.GeneralAnimationCounter = (long)v, 1, _auxAnimationLength);
            commandAnimation.Commit(MenuCanvas, "GeneralAnimationCounter", length: GHConstants.AuxiliaryCanvasAnimationTime, 
                rate: ClientUtils.GetAuxiliaryCanvasAnimationInterval(), repeat: () => true /* MenuGrid.IsVisible */);
        }
        private void StartTextCanvasAnimation()
        {
            Animation commandAnimation = new Animation(v => TextCanvas.GeneralAnimationCounter = (long)v, 1, _auxAnimationLength);
            commandAnimation.Commit(TextCanvas, "GeneralAnimationCounter", length: GHConstants.AuxiliaryCanvasAnimationTime, 
                rate: ClientUtils.GetAuxiliaryCanvasAnimationInterval(), repeat: () => true /* TextGrid.IsVisible */);
        }

        private bool StartingPositionsSet { get; set; }
        private bool _useUnifromAnimationInterval = false;
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
            if (_useUnifromAnimationInterval)
            {
                _animationStopwatch.Stop();
                TimeSpan elapsed = _animationStopwatch.Elapsed;
                long intervals_elapsed = (long)(elapsed.TotalMilliseconds / (double)GHConstants.DefaultAnimationInterval);
                long prev_intervals_elapsed = (long)(_previousTimeSpan.TotalMilliseconds / (double)GHConstants.DefaultAnimationInterval);

                counter_increment = Math.Max(1, intervals_elapsed - prev_intervals_elapsed);
                if (intervals_elapsed >= 100000000)
                {
                    _animationStopwatch.Reset();
                }
                _previousTimeSpan = elapsed;
                _animationStopwatch.Start();
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
            LoadingGrid.IsVisible = false;
            MainGrid.IsVisible = true;
            StartMainCanvasAnimation();
        }

        public void ClearContextMenu()
        {
            ContextLayout.Children.Clear();
            _contextMenuData.Clear();
            ContextLayout.IsVisible = false;
        }
        public void AddContextMenu(AddContextMenuData data)
        {
            _contextMenuData.Add(data);
            string icon_string = "";
            int LastPickedCmd = GHUtils.Meta('<');
            int OfferCmd = GHUtils.Meta('o');
            int PrayCmd = GHUtils.Meta('p');
            int DipCmd = GHUtils.Meta('d');
            int DigCmd = GHUtils.Ctrl('g');
            int SitCmd = GHUtils.Ctrl('s');
            if (data.cmd_def_char < 0)
            {
                LastPickedCmd -= 256;
                OfferCmd -= 256;
                PrayCmd -= 256;
                DipCmd -= 256;
                DigCmd -= 256;
                SitCmd -= 256;
            }

            switch ((char)data.cmd_def_char)
            {
                case 'a':
                case 'm':
                    switch (data.style)
                    {
                        case (int)context_menu_styles.CONTEXT_MENU_STYLE_GETPOS: /* Next interesting / monster */
                            icon_string = "GnollHackClient.Assets.UI.next.png";
                            break;
                        default:
                            icon_string = "GnollHackClient.Assets.UI.next.png";
                            break;
                    }
                    break;
                case 'A':
                case 'M':
                    switch (data.style)
                    {
                        case (int)context_menu_styles.CONTEXT_MENU_STYLE_GETPOS: /* Previous interesting / monster */
                            icon_string = "GnollHackClient.Assets.UI.previous.png";
                            break;
                        default:
                            icon_string = "GnollHackClient.Assets.UI.previous.png";
                            break;
                    }
                    break;
                case 'e':
                    icon_string = "GnollHackClient.Assets.UI.eat.png";
                    break;
                case 'l':
                    icon_string = "GnollHackClient.Assets.UI.loot.png";
                    break;
                case 'p':
                    icon_string = "GnollHackClient.Assets.UI.pay.png";
                    break;
                case ',':
                    icon_string = "GnollHackClient.Assets.UI.pickup.png";
                    break;
                case '<':
                    switch (data.style)
                    {
                        case (int)context_menu_styles.CONTEXT_MENU_STYLE_GETDIR: /* Upwards */
                            icon_string = "GnollHackClient.Assets.UI.target-upwards.png";
                            break;
                        case (int)context_menu_styles.CONTEXT_MENU_STYLE_GETPOS:
                            icon_string = "GnollHackClient.Assets.UI.stairs-up.png";
                            break;
                        default:
                        case (int)context_menu_styles.CONTEXT_MENU_STYLE_GENERAL:
                            icon_string = "GnollHackClient.Assets.UI.stairs-up.png";
                            break;
                    }
                    break;
                case '>':
                    switch (data.style)
                    {
                        case (int)context_menu_styles.CONTEXT_MENU_STYLE_GETDIR: /* Downwards */
                            icon_string = "GnollHackClient.Assets.UI.target-downwards.png";
                            break;
                        case (int)context_menu_styles.CONTEXT_MENU_STYLE_GETPOS:
                            icon_string = "GnollHackClient.Assets.UI.stairs-down.png";
                            break;
                        default:
                        case (int)context_menu_styles.CONTEXT_MENU_STYLE_GENERAL:
                            icon_string = "GnollHackClient.Assets.UI.stairs-down.png";
                            break;
                    }
                    break;
                case ':':
                    icon_string = "GnollHackClient.Assets.UI.lookhere.png";
                    break;
                case 'q':
                    icon_string = "GnollHackClient.Assets.UI.quaff.png";
                    break;
                case 'r':
                    icon_string = "GnollHackClient.Assets.UI.read.png";
                    break;
                case '.':
                    switch(data.style)
                    {
                        case (int)context_menu_styles.CONTEXT_MENU_STYLE_GETPOS: /* Pick position in getpos */
                            icon_string = "GnollHackClient.Assets.UI.select.png";
                            break;
                        case (int)context_menu_styles.CONTEXT_MENU_STYLE_GETDIR: /* Self in getdir */
                            icon_string = "GnollHackClient.Assets.UI.target-self.png";
                            break;
                        default:
                        case (int)context_menu_styles.CONTEXT_MENU_STYLE_GENERAL:
                            icon_string = "GnollHackClient.Assets.UI.wait.png";
                            break;
                    }
                    break;
                case (char)27:
                    switch (data.style)
                    {
                        case (int)context_menu_styles.CONTEXT_MENU_STYLE_CLOSE_DISPLAY:
                            icon_string = "GnollHackClient.Assets.UI.exit-to-map.png";
                            break;
                        default:
                            icon_string = "GnollHackClient.Assets.UI.no.png";
                            break;
                    }
                    break;
                default:
                    if (data.cmd_def_char == LastPickedCmd)
                        icon_string = "GnollHackClient.Assets.UI.lastitem.png";
                    else if (data.cmd_def_char == OfferCmd)
                        icon_string = "GnollHackClient.Assets.UI.offer.png";
                    else if (data.cmd_def_char == PrayCmd)
                        icon_string = "GnollHackClient.Assets.UI.pray.png";
                    else if (data.cmd_def_char == DipCmd)
                        icon_string = "GnollHackClient.Assets.UI.dip.png";
                    else if (data.cmd_def_char == DigCmd)
                        icon_string = "GnollHackClient.Assets.UI.dig.png";
                    else if (data.cmd_def_char == SitCmd)
                        icon_string = "GnollHackClient.Assets.UI.sit.png";
                    else
                        icon_string = "GnollHackClient.Assets.Icons.missing_icon.png";
                    break;
            }

            LabeledImageButton lib = new LabeledImageButton();
            lib.ImgSourcePath = "resource://" + icon_string;
            lib.LargerFont = false;
            lib.LblText = data.cmd_text;
            lib.SetSideSize(_currentPageWidth, _currentPageHeight);
            lib.GridMargin = new Thickness(lib.ImgWidth / 15, lib.ImgWidth / 30);
            lib.BtnCommand = data.cmd_cur_char;
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
                    long ticks = (long)(secs * ClientUtils.GetMainCanvasAnimationFrequency(MapRefreshRate));
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

            if (_clientGame != null)
            {
                ConcurrentQueue<GHResponse> queue;
                if (ClientGame.ResponseDictionary.TryGetValue(_clientGame, out queue))
                {
                    queue.Enqueue(new GHResponse(_clientGame, GHRequestType.DisplayScreenText));
                }
            }
        }

        public void DisplayConditionText(DisplayConditionTextData data)
        {
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

                long counter = 0;
                //lock (AnimationTimerLock)
                //{
                //    counter = AnimationTimers.general_animation_counter;
                //}
                lock (_mainCounterLock)
                {
                    counter = _mainCounterValue;
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

                long counter = 0;
                //lock (AnimationTimerLock)
                //{
                //    counter = AnimationTimers.general_animation_counter;
                //}
                lock (_mainCounterLock)
                {
                    counter = _mainCounterValue;
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

                long counter = 0;
                //lock (AnimationTimerLock)
                //{
                //    counter = AnimationTimers.general_animation_counter;
                //}
                lock (_mainCounterLock)
                {
                    counter = _mainCounterValue;
                }

                _guiEffects.Add(new GHGUIEffect(data, counter, this));
            }
        }


        private Color _titleGoldColor = new Color((double)0xD4 / 255, (double)0xA0 / 255, (double)0x17 / 255);
        private Color _popupTransparentBlackColor = new Color(0, 0, 0, (double)0x66 / 255);
        private Color _popupDarkerTransparentBlackColor = new Color(0, 0, 0, (double)0xAA / 255);

        private GlyphImageSource _popupImageSource = new GlyphImageSource();
        public void DisplayPopupText(DisplayScreenTextData data)
        {
            PopupTitleLabel.Text = data.subtext;
            if ((data.tflags & 1UL) != 0)
                PopupLabel.Text = "\"" + data.text + "\"";
            else
                PopupLabel.Text = data.text;

            if (data.style == (int)popup_text_types.POPUP_TEXT_DIALOGUE ||
                data.style == (int)popup_text_types.POPUP_TEXT_ADVICE ||
                data.style == (int)popup_text_types.POPUP_TEXT_MESSAGE ||
                data.style == (int)popup_text_types.POPUP_TEXT_NO_MONSTERS_IN_LIST)
            {
                PopupTitleLabel.TextColor = _titleGoldColor;
                PopupGrid.BackgroundColor = Color.Transparent;
                PopupFrame.BackgroundColor = _popupDarkerTransparentBlackColor;
                if (data.glyph != 0 && data.glyph != NoGlyph)
                    PopupTitleLayout.HorizontalOptions = LayoutOptions.StartAndExpand;
                else
                    PopupTitleLayout.HorizontalOptions = LayoutOptions.CenterAndExpand;
            }
            else if (data.style == (int)popup_text_types.POPUP_TEXT_REVIVAL)
            {
                PopupTitleLabel.TextColor = _titleGoldColor;
                PopupGrid.BackgroundColor = _popupTransparentBlackColor;
                PopupFrame.BackgroundColor = _popupTransparentBlackColor;
                PopupTitleLayout.HorizontalOptions = LayoutOptions.CenterAndExpand;
            }
            else
            {
                PopupTitleLabel.TextColor = ClientUtils.NHColor2XColor(data.color, data.attr, false, true);
                PopupGrid.BackgroundColor = _popupTransparentBlackColor;
                PopupFrame.BackgroundColor = _popupTransparentBlackColor;
                PopupTitleLayout.HorizontalOptions = LayoutOptions.CenterAndExpand;
            }

            PopupImage.Source = null;
            if (data.glyph != 0 && data.glyph != NoGlyph)
            {
                _popupImageSource.ReferenceGamePage = this;
                _popupImageSource.UseUpperSide = (data.tflags & 1) != 0;
                _popupImageSource.Glyph = data.glyph;
                _popupImageSource.AutoSize = true;
                PopupImage.Source = _popupImageSource;
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
            idx = ContextLayout.Children.IndexOf((Xamarin.Forms.View)sender);
            if (idx < 0)
                idx = ContextLayout.Children.IndexOf((Xamarin.Forms.View)((Xamarin.Forms.View)sender).Parent);

            if (idx >= 0 && idx < _contextMenuData.Count)
            {
                int resp = _contextMenuData[idx].cmd_cur_char;
                GenericButton_Clicked(sender, e, resp);
            }
        }

        public int TileSheetIdx(int ntile)
        {
            return (Math.Min(UsedTileSheets - 1, Math.Max(0, (ntile / GHConstants.NumberOfTilesPerSheet))));
        }

        public int TileSheetX(int ntile)
        {
            return (((ntile % GHConstants.NumberOfTilesPerSheet) % _tilesPerRow[TileSheetIdx(ntile)]) * GHConstants.TileWidth);
        }
        public int TileSheetY(int ntile)
        {
            return (((ntile % GHConstants.NumberOfTilesPerSheet) / _tilesPerRow[TileSheetIdx(ntile)]) * GHConstants.TileHeight);
        }

        private /*async*/ void ContentPage_Appearing(object sender, EventArgs e)
        {
            App.BackButtonPressed += BackButtonPressed;
            lock (RefreshScreenLock)
            {
                RefreshScreen = true;
            }

            GameMenuButton.IsEnabled = true;
            lMoreButton.IsEnabled = true;

            if (_isFirstAppearance)
            {
                _isFirstAppearance = false;
            }
        }

        protected void GNHThreadProc()
        {
            _clientGame = new ClientGame(this);
            _gnollHackService.StartGnollHack(_clientGame);
        }

        private void pollRequestQueue()
        {
            if (_clientGame != null)
            {
                GHRequest req;
                ConcurrentQueue<GHRequest> queue;
                if (ClientGame.RequestDictionary.TryGetValue(_clientGame, out queue))
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
                                ShowYnResponses(req.RequestInt, req.RequestAttr, req.RequestNhColor, req.RequestGlyph, req.TitleString, req.RequestString, req.Responses, req.ResponseDescriptions, req.RequestFlags);
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
                                AskName();
                                break;
                            case GHRequestType.GetLine:
                                GetLine(req.RequestString, req.PlaceHolderString, req.DefValueString, req.RequestInt, req.RequestAttr, req.RequestNhColor);
                                break;
                            case GHRequestType.ReturnToMainMenu:
                                ClearMap();
                                _clientGame = null;
                                _mainPage.GameStarted = false;
                                ReturnToMainMenu();
                                break;
                            case GHRequestType.ShowMenuPage:
                                ShowMenuCanvas(req.RequestMenuInfo != null ? req.RequestMenuInfo : new GHMenuInfo(ghmenu_styles.GHMENU_STYLE_GENERAL), req.RequestingGHWindow);
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
                        }
                    }
                }
            }
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
            lock (_clientGame.WindowsLock)
            {
                window = _clientGame.Windows[winid];
            }
            ShowWindowCanvas(window, strs);
        }

        private void ShowWindowCanvas(GHWindow window, List<GHPutStrItem> strs)
        {
            /* Cancel delayed text hide */
            lock(_delayedTextHideLock)
            {
                _delayedTextHideCancelled = true;
            }

            /* Cancel delayed menu hide */
            bool dohidemenu = false;
            lock(_menuHideCancelledLock)
            {
                if (_menuHideOn)
                {
                    _menuHideCancelled = true;
                    dohidemenu = true;
                }
            }
            if(dohidemenu)
            {
                MenuGrid.IsVisible = false;
            }

            lock (RefreshScreenLock)
            {
                RefreshScreen = false;
            }

            lock (TextScrollLock)
            {
                _textScrollOffset = 0;
            }

            TextWindowGlyphImage.Source = null;

            _textGlyphImageSource.ReferenceGamePage = this;
            _textGlyphImageSource.AutoSize = true;
            _textGlyphImageSource.ObjData = window.ObjData;
            _textGlyphImageSource.Glyph = window.Glyph;
            _textGlyphImageSource.UseUpperSide = window.UseUpperSide;

            TextWindowGlyphImage.Source = TextGlyphImage;
            TextWindowGlyphImage.IsVisible = IsTextGlyphVisible;

            List<GHPutStrItem> items = null;
            if (window.WindowStyle == ghwindow_styles.GHWINDOW_STYLE_PAGER_GENERAL || window.WindowStyle == ghwindow_styles.GHWINDOW_STYLE_PAGER_SPEAKER)
            {
                items = new List<GHPutStrItem>();
                ClientUtils.ProcessAdjustedItems(items, strs);
            }
            else
                items = strs;

            lock (TextCanvas.TextItemLock)
            {
                TextTouchDictionary.Clear();
                TextCanvas.GHWindow = window;

                TextCanvas.PutStrItems = items;
            }

            TextGrid.IsVisible = true;
            MainGrid.IsVisible = false;
            if (canvasView.AnimationIsRunning("GeneralAnimationCounter"))
                canvasView.AbortAnimation("GeneralAnimationCounter");
            StartTextCanvasAnimation();
        }


        private GlyphImageSource _menuGlyphImageSource = new GlyphImageSource();

        public ImageSource MenuGlyphImage
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
                return (Math.Abs(_menuGlyphImageSource.Glyph) > 0 && _menuGlyphImageSource.Glyph != NoGlyph);
            }
        }


        private void PrintTopLine(string str, uint attributes)
        {

        }


        private GlyphImageSource _ynImageSource = new GlyphImageSource();
        private void ShowYnResponses(int style, int attr, int color, int glyph, string title, string question, string responses, string descriptions, ulong ynflags)
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
                YnTitleLabel.TextColor = Color.White;
            }
            else
            {
                YnTitleLabel.Text = title;
                YnTitleLabel.IsVisible = true;
                YnQuestionLabel.TextColor = Color.White;
                if (style == (int)yn_function_styles.YN_STYLE_MONSTER_QUESTION)
                {
                    YnTitleLabel.TextColor = _titleGoldColor;
                }
                else
                {
                    YnTitleLabel.TextColor = ClientUtils.NHColor2XColor(color, attr, false, true);
                }
            }

            /* Title Glyph */
            YnImage.Source = null;
            if (glyph != 0 && glyph != NoGlyph)
            {
                YnTitleLayout.HorizontalOptions = LayoutOptions.StartAndExpand;
                _ynImageSource.ReferenceGamePage = this;
                _ynImageSource.UseUpperSide = (ynflags & 1) != 0;
                _ynImageSource.Glyph = glyph;
                _ynImageSource.AutoSize = true;
                YnImage.Source = _ynImageSource;
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
            YnQuestionLabel.Text = question;

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
            YnGrid.IsVisible = true;
        }

        private string GetYnImgSourcePath(char ch, string desc)
        {
            string res = "resource://GnollHackClient.Assets.Icons.missing_icon.png";
            switch (ch)
            {
                case 'i':
                    res = "resource://GnollHackClient.Assets.UI.inventory.png";
                    break;
                case 'd':
                    if (desc.Substring(0, 4) == "Drop")
                        res = "resource://GnollHackClient.Assets.UI.dropmany.png";
                    else if (desc == "Disarm")
                        res = "resource://GnollHackClient.Assets.UI.yes.png";
                    break;
                case 'y':
                    res = "resource://GnollHackClient.Assets.UI.yes.png";
                    break;
                case 'n':
                    res = "resource://GnollHackClient.Assets.UI.no.png";
                    break;
                case 'q':
                    res = "resource://GnollHackClient.Assets.UI.cancel.png";
                    break;
                case 'a':
                    res = "resource://GnollHackClient.Assets.UI.yestoall.png";
                    break;
                case 'r':
                    res = "resource://GnollHackClient.Assets.UI.rightring.png";
                    break;
                case 'l':
                    res = "resource://GnollHackClient.Assets.UI.leftring.png";
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
        private object _msgHistoryLock = new object();
        private List<GHMsgHistoryItem> _msgHistory = null;
        private void PrintHistory(List<GHMsgHistoryItem> msgHistory)
        {
            lock (_msgHistoryLock)
            {
                _msgHistory = msgHistory;
            }
        }

        private async void AskName()
        {
            var namePage = new NamePage(this);
            await App.Current.MainPage.Navigation.PushModalAsync(namePage);
        }

        private int _getLineStyle = 0;
        private void GetLine(string query, string placeholder, string linesuffix, int style, int attr, int color)
        {
            Color clr = ClientUtils.NHColor2XColor(color, attr, false, false); /* Non-title / white coloring works better here */
            string PlaceHolderText = null;
            if (!string.IsNullOrWhiteSpace(placeholder) && placeholder.Length > 0)
            {
                PlaceHolderText = char.ToUpper(placeholder[0]) + placeholder.Substring(1);
            }

            GetLineCaption.Text = query;
            if (!string.IsNullOrWhiteSpace(linesuffix) && linesuffix != " -")
                GetLineCaption.Text += " " + linesuffix;

            GetLineCaption.TextColor = clr;
            GetLineEntryText.Text = "";
            GetLineEntryText.MaxLength = GHConstants.BUFSZ - 1;
            GetLineQuestionMarkGrid.IsVisible = false;
            GetLineEntryText.IsVisible = true;
            GetLineEntryText.Keyboard = Keyboard.Default;
            GetLineEntryText.WidthRequest = 320;
            GetLineAutoComplete.Text = "";
            GetLineAutoComplete.IsVisible = false;

            _getLineStyle = style;
            switch (style)
            {
                case (int)getline_types.GETLINE_EXTENDED_COMMAND:
                    GetLineEntryText.WidthRequest = 230;
                    GetLineQuestionMarkGrid.IsVisible = true;
                    GetLineAutoComplete.IsVisible = true;
                    GetLineEntryText.Placeholder = "Type the command";
                    break;
                case (int)getline_types.GETLINE_LEVELPORT:
                    GetLineEntryText.Placeholder = "Type the level here";
                    GetLineEntryText.Keyboard = Keyboard.Numeric;
                    /* '*' could be possible as well, but not implemented at the moment */
                    break;
                case (int)getline_types.GETLINE_WIZ_LEVELPORT:
                    GetLineEntryText.WidthRequest = 230;
                    GetLineQuestionMarkGrid.IsVisible = true;
                    GetLineEntryText.Placeholder = "Type the level";
                    GetLineEntryText.Keyboard = Keyboard.Numeric;
                    break;
                case (int)getline_types.GETLINE_LEVEL_CHANGE:
                case (int)getline_types.GETLINE_NUMBERS_ONLY:
                    GetLineEntryText.WidthRequest = 240;
                    GetLineEntryText.Keyboard = Keyboard.Numeric;
                    if (style == (int)getline_types.GETLINE_LEVEL_CHANGE)
                        GetLineEntryText.Placeholder = "Type the level here";
                    else
                        GetLineEntryText.Placeholder = "Type the number here";
                    break;
                case (int)getline_types.GETLINE_WISHING:
                    GetLineEntryText.Placeholder = "Type your wish here";
                    break;
                case (int)getline_types.GETLINE_GENESIS:
                case (int)getline_types.GETLINE_POLYMORPH:
                case (int)getline_types.GETLINE_GENOCIDE:
                case (int)getline_types.GETLINE_MONSTER:
                    GetLineEntryText.Placeholder = "Type the monster here";
                    break;
                case (int)getline_types.GETLINE_MONSTER_CLASS:
                    GetLineEntryText.WidthRequest = 230;
                    GetLineEntryText.MaxLength = 1;
                    GetLineQuestionMarkGrid.IsVisible = true;
                    GetLineEntryText.Placeholder = "Type the monster class";
                    break;
                case (int)getline_types.GETLINE_TUNE:
                    GetLineEntryText.WidthRequest = 240;
                    GetLineEntryText.Placeholder = "Type the tune here";
                    break;
                case (int)getline_types.GETLINE_QUESTION:
                    GetLineEntryText.Placeholder = "Type the answer here";
                    break;
                default:
                    if (PlaceHolderText != null)
                        GetLineEntryText.Placeholder = PlaceHolderText;
                    else
                        GetLineEntryText.Placeholder = "Type the text here";
                    break;
            }
            GetLineGrid.IsVisible = true;
        }

        private void GetLineOkButton_Clicked(object sender, EventArgs e)
        {
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

            ConcurrentQueue<GHResponse> queue;
            if (ClientGame.ResponseDictionary.TryGetValue(_clientGame, out queue))
            {
                queue.Enqueue(new GHResponse(_clientGame, GHRequestType.GetLine, res));
            }

            GetLineGrid.IsVisible = false;
            GetLineEntryText.Text = "";
            GetLineCaption.Text = "";
        }

        private void GetLineQuestionMarkButton_Clicked(object sender, EventArgs e)
        {
            string res = "?";
            ConcurrentQueue<GHResponse> queue;
            if (ClientGame.ResponseDictionary.TryGetValue(_clientGame, out queue))
            {
                queue.Enqueue(new GHResponse(_clientGame, GHRequestType.GetLine, res));
            }

            GetLineGrid.IsVisible = false;
            GetLineEntryText.Text = "";
            GetLineCaption.Text = "";
        }

        private void GetLineCancelButton_Clicked(object sender, EventArgs e)
        {
            ConcurrentQueue<GHResponse> queue;
            if (ClientGame.ResponseDictionary.TryGetValue(_clientGame, out queue))
            {
                queue.Enqueue(new GHResponse(_clientGame, GHRequestType.GetLine, '\x1B'.ToString()));
            }

            GetLineGrid.IsVisible = false;
            GetLineEntryText.Text = "";
            GetLineCaption.Text = "";
        }

        private void GetChar()
        {
            // Set focus to GameViewPage
        }
        private async void ReturnToMainMenu()
        {
            if (!App.IsServerGame)
                _mainPage.HideLocalGameButton();

            _mainPage.PlayMainScreenVideoAndMusic(); /* Just to be doubly sure */

            await App.Current.MainPage.Navigation.PopModalAsync();
            if (App.IsServerGame)
            {
                await App.Current.MainPage.Navigation.PopAsync(); //Login
            }
        }

        private object _menuDrawOnlyLock = new object();
        private bool _menuDrawOnlyClear = false;
        private bool _menuRefresh = true;

        private void ShowMenuCanvas(GHMenuInfo menuinfo, GHWindow ghwindow)
        {
            /* Cancel delayed menu hide */
            lock (_menuHideCancelledLock)
            {
                _menuHideCancelled = true;
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
            if(dohidetext)
            {
                TextGrid.IsVisible = false;
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

            App.DebugWriteProfilingStopwatchTimeAndStart("ShowMenuCanvas Start");

            MenuTouchDictionary.Clear();

            MenuWindowGlyphImage.Source = null;

            _menuGlyphImageSource.ReferenceGamePage = this;
            _menuGlyphImageSource.AutoSize = true;
            _menuGlyphImageSource.ObjData = ghwindow.ObjData;
            _menuGlyphImageSource.Glyph = ghwindow.Glyph;
            _menuGlyphImageSource.UseUpperSide = ghwindow.UseUpperSide;

            MenuWindowGlyphImage.Source = MenuGlyphImage;
            MenuWindowGlyphImage.IsVisible = IsMenuGlyphVisible;

            MenuCanvas.GHWindow = ghwindow;
            MenuCanvas.MenuStyle = menuinfo.Style;
            MenuCanvas.SelectionHow = menuinfo.SelectionHow;
            MenuCanvas.SelectionIndex = -1;
            if (MenuCanvas.SelectionHow == SelectionMode.Single)
            {
                int idx = -1;
                foreach (GHMenuItem mi in menuinfo.MenuItems)
                {
                    idx++;
                    if (mi.Selected)
                    {
                        mi.Selected = false; /* Clear out, with single selection we are using SelectionIndex */
                        MenuCanvas.SelectionIndex = idx;
                    }
                }
            }

            if (menuinfo.Header == null)
            {
                MenuHeaderLabel.IsVisible = true;
                MenuHeaderLabel.Text = " ";
                MenuHeaderLabel.FontFamily = ClientUtils.MenuHeaderFontFamily(MenuCanvas.MenuStyle);
                MenuHeaderLabel.FontSize = ClientUtils.MenuHeaderFontSize(MenuCanvas.MenuStyle);
            }
            else
            {
                MenuHeaderLabel.IsVisible = true;
                MenuHeaderLabel.Text = menuinfo.Header;
                MenuHeaderLabel.FontFamily = ClientUtils.MenuHeaderFontFamily(MenuCanvas.MenuStyle);
                MenuHeaderLabel.FontSize = ClientUtils.MenuHeaderFontSize(MenuCanvas.MenuStyle);
            }

            if (menuinfo.Subtitle == null)
            {
                MenuSubtitleLabel.IsVisible = false;
                MenuSubtitleLabel.Text = "";
            }
            else
            {
                MenuSubtitleLabel.IsVisible = true;
                MenuSubtitleLabel.Text = menuinfo.Subtitle;
                MenuSubtitleLabel.FontFamily = ClientUtils.MenuSubtitleFontFamily(MenuCanvas.MenuStyle);
                MenuSubtitleLabel.FontSize = ClientUtils.MenuSubtitleFontSize(MenuCanvas.MenuStyle);
            }

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

            lock (_menuDrawOnlyLock)
            {
                _menuDrawOnlyClear = false;
                _menuRefresh = true;
            }

            MenuGrid.IsVisible = true;
            MainGrid.IsVisible = false;
            if (canvasView.AnimationIsRunning("GeneralAnimationCounter"))
                canvasView.AbortAnimation("GeneralAnimationCounter");
            StartMenuCanvasAnimation();
            App.DebugWriteProfilingStopwatchTimeAndStart("ShowMenuCanvas End");
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
                if (ClientGame.ResponseDictionary.TryGetValue(_clientGame, out queue))
                {
                    queue.Enqueue(new GHResponse(_clientGame, GHRequestType.ShowMenuPage, MenuCanvas.GHWindow, new List<GHMenuItem>()));
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
                await App.Current.MainPage.Navigation.PushModalAsync(menu);
            }

            return false;
        }
        public async void ShowGameMenu(object sender, EventArgs e)
        {
            var menu = new GameMenuPage(this);
            await App.Current.MainPage.Navigation.PushModalAsync(menu);
        }

        private void ContentPage_Disappearing(object sender, EventArgs e)
        {
            App.BackButtonPressed -= BackButtonPressed;
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


        private List<string> wrappedLines = new List<string>();
        private string longLine;
        private string wordWithSpace;

        private SKMaskFilter _blur = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 3.4f);
        private SKMaskFilter _lookBlur = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 20.0f);

        public struct layer_draw_order_definition
        {
            public int layer;
            public int enlargement_position;
            public bool darken;
        }

        private object _drawOrderLock = new object();
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

                for (int partition = 0; partition <= 1; partition++)
                {
                    int[] partition_start = { (int)layer_types.LAYER_FLOOR + 1, (int)layer_types.LAYER_GENERAL_UI, (int)layer_types.MAX_LAYERS };
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

        private object _canvasPageLock = new object();
        private canvas_page_types _canvasPage = 0;

        private void canvasView_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            canvas_page_types page = 0;
            lock (_canvasPageLock)
            {
                page = _canvasPage;
            }

            switch (page)
            {
                case canvas_page_types.MainGamePage:
                    PaintMainGamePage(sender, e);
                    break;
                case canvas_page_types.MenuPage:
                    //MenuCanvas_PaintSurface(sender, e);
                    break;
            }


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
                    textPaint.Typeface = App.LatoBold;
                    textPaint.TextSize = 26;
                    textPaint.Color = SKColors.Yellow;
                    textWidth = textPaint.MeasureText(str, ref textBounds);
                    yText = -textPaint.FontMetrics.Ascent + 5 + (ShowFPS ? textPaint.FontSpacing : 0);
                    xText = canvaswidth - textWidth - 5;
                    canvas.DrawText(str, xText, yText, textPaint);
                }

                if (ShowFPS)
                {
                    lock (_fpslock)
                    {
                        str = "FPS: " + string.Format("{0:0.0}", _fps) + ", D:" + _counterValueDiff;
                    }
                    textPaint.Typeface = App.LatoBold;
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

        private void PaintMainGamePage(object sender, SKPaintSurfaceEventArgs e)
        {
            if (!MainGrid.IsVisible)
                return;

            SKImageInfo info = e.Info;
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear(SKColors.Black);
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
            long moveIntervals = Math.Max(2, (long)Math.Ceiling((double)ClientUtils.GetMainCanvasAnimationFrequency(MapRefreshRate) / 10.0));

            using (SKPaint textPaint = new SKPaint())
            {
                string str = "";
                SKRect textBounds = new SKRect();
                SKPathEffect _pathEffect = SKPathEffect.CreateDash(_gridIntervals, 0);

                textPaint.Color = SKColors.White;
                textPaint.Typeface = App.LatoRegular;

                /* if server game */
                //{
                //    xText = 10;
                //    yText = 0;
                //    string additional_info = "";
                //    if (_connection == null && _connectionAttempted)
                //        additional_info = ", no _connection";
                //    else if (_connection == null)
                //    {
                //        /* Do nothing */
                //    }
                //    else if (_connection.State == HubConnectionState.Connected)
                //        additional_info = ", connected";
                //    else if (_connection.State == HubConnectionState.Connecting)
                //        additional_info = ", connecting";
                //    else if (_connection.State == HubConnectionState.Disconnected)
                //        additional_info = ", disconnected";
                //    else if (_connection.State == HubConnectionState.Reconnecting)
                //        additional_info = ", reconnecting";

                //    str = _connection_status + additional_info;
                //    textPaint.TextSize = 36;
                //    yText = yText + 50;
                //    canvas.DrawText(str, xText, yText, textPaint);

                //    str = _message;
                //    textPaint.TextSize = 36;
                //    yText = yText + 50;
                //    canvas.DrawText(str, xText, yText, textPaint);

                //    str = _message2;
                //    textPaint.TextSize = 36;
                //    yText = yText + 50;
                //    canvas.DrawText(str, xText, yText, textPaint);

                //    if (_result != 0)
                //    {
                //        str = _result.ToString();
                //        yText = yText + 50;
                //        canvas.DrawText(str, xText, yText, textPaint);
                //    }

                //    if (_result2 != 0)
                //    {
                //        str = _result2.ToString();
                //        yText = yText + 50;
                //        canvas.DrawText(str, xText, yText, textPaint);
                //    }

                //    str = _message3;
                //    yText = yText + 50;
                //    canvas.DrawText(str, xText, yText, textPaint);

                //    str = _message4;
                //    yText = yText + 50;
                //    canvas.DrawText(str, xText, yText, textPaint);

                //    str = _message5;
                //    yText = yText + 50;
                //    canvas.DrawText(str, xText, yText, textPaint);

                //}


                /* Map */
                float canvaswidth = canvasView.CanvasSize.Width;
                float canvasheight = canvasView.CanvasSize.Height;
                float textscale = GetTextScale();
                float UsedFontSize = ZoomAlternateMode ? MapFontAlternateSize : MapFontSize;
                textPaint.Typeface = App.DejaVuSansMonoTypeface;
                textPaint.TextSize = UsedFontSize;
                if (ZoomMiniMode)
                {
                    float tmpwidth = textPaint.FontMetrics.AverageCharacterWidth;
                    float tmpheight = textPaint.FontMetrics.Descent - textPaint.FontMetrics.Ascent;
                    if (GraphicsStyle == GHGraphicsStyle.Tiles && !ForceAscii)
                    {
                        tmpwidth = GHConstants.TileWidth * UsedFontSize / GHConstants.MapFontDefaultSize;
                        tmpheight = GHConstants.TileHeight * UsedFontSize / GHConstants.MapFontDefaultSize;
                    }
                    float tmpmapwidth = tmpwidth * (GHConstants.MapCols - 1);
                    float tmpmapheight = tmpheight * GHConstants.MapRows;
                    float xscale = tmpmapwidth > 0 ? canvaswidth / tmpmapwidth : 0;
                    float yscale = tmpmapheight > 0 ? canvasheight / tmpmapheight : 0;
                    float cscale = Math.Min(xscale, yscale);
                    UsedFontSize = Math.Max(2.0f, UsedFontSize * cscale);
                    UsedFontSize = UsedFontSize * MapFontMiniRelativeSize;
                    textPaint.TextSize = UsedFontSize;
                }
                float width = textPaint.FontMetrics.AverageCharacterWidth;
                float height = textPaint.FontMetrics.Descent - textPaint.FontMetrics.Ascent;

                if (GraphicsStyle == GHGraphicsStyle.Tiles && !ForceAscii)
                {
                    width = GHConstants.TileWidth * UsedFontSize / GHConstants.MapFontDefaultSize;
                    height = GHConstants.TileHeight * UsedFontSize / GHConstants.MapFontDefaultSize;
                }

                float mapwidth = width * (GHConstants.MapCols - 1);
                float mapheight = height * (GHConstants.MapRows);

                _tileWidth = width;
                _tileHeight = height;
                _mapWidth = mapwidth;
                _mapHeight = mapheight;
                _mapFontAscent = textPaint.FontMetrics.Ascent;
                float targetscale = height / (float)GHConstants.TileHeight;

                int startX = 1;
                int endX = GHConstants.MapCols - 1;
                int startY = 0;
                int endY = GHConstants.MapRows - 1;

                float offsetX = (canvaswidth - mapwidth) / 2;
                float offsetY = (canvasheight - mapheight) / 2;

                float usedOffsetX = 0;
                float usedOffsetY = 0;
                lock (_mapOffsetLock)
                {
                    usedOffsetX = _mapOffsetX;
                    usedOffsetY = _mapOffsetY;
                }

                if (ZoomMiniMode)
                {
                    offsetX -= usedOffsetX;
                    offsetY -= usedOffsetY;
                    lock (_mapOffsetLock)
                    {
                        usedOffsetX += _mapMiniOffsetX;
                        usedOffsetY += _mapMiniOffsetY;
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

                float tx = 0, ty = 0;
                float startx = 0, starty = 0;
                if (_clientGame != null)
                {
                    lock (_clientGame.WindowsLock)
                    {
                        if (_clientGame.Windows[_clientGame.MapWindowId] != null)
                        {
                            startx = _clientGame.Windows[_clientGame.MapWindowId].Left;
                            starty = _clientGame.Windows[_clientGame.MapWindowId].Top;
                        }
                    }
                }


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
                            ty = offsetY + usedOffsetY + _mapFontAscent;
                            SKRect targetrect = new SKRect(tx, ty, tx + sourcewidth * width / (float)GHConstants.TileWidth, ty + sourceheight * height / GHConstants.TileHeight);
                            canvas.DrawBitmap(_mapBitmap, sourcerect, targetrect);
                        }
                    }
                }
                else
                {
                    lock (Glyph2TileLock)
                    {
                        lock (_mapDataLock)
                        {
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
                                if (Glyph2Tile != null && _tilesPerRow[0] > 0 && UsedTileSheets > 0)
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

                                        lock (_drawOrderLock)
                                        {
                                            int draw_cnt = _draw_order.Count;
                                            for (int layer_idx = 0; layer_idx < (int)layer_types.MAX_LAYERS + 2; layer_idx++)
                                            //for (int draw_idx = 0; draw_idx < draw_cnt; draw_idx++)
                                            {
                                                //int layer_idx = _draw_order[draw_idx].layer;
                                                bool is_monster_or_shadow_layer = (layer_idx == (int)layer_types.LAYER_MONSTER || layer_idx == (int)layer_types.MAX_LAYERS);
                                                bool is_monster_like_layer = (is_monster_or_shadow_layer || layer_idx == (int)layer_types.LAYER_MONSTER_EFFECT);
                                                bool is_object_like_layer = (layer_idx == (int)layer_types.LAYER_OBJECT || layer_idx == (int)layer_types.LAYER_COVER_OBJECT);
                                                for (int mapy = startY; mapy <= endY; mapy++)
                                                {
                                                    for (int mapx = startX; mapx <= endX; mapx++)
                                                    {
                                                        if (_mapData[mapx, mapy].Layers.layer_glyphs == null || _mapData[mapx, mapy].Layers.layer_gui_glyphs == null)
                                                            continue;

                                                        if (layer_idx == (int)layer_types.MAX_LAYERS
                                                            && (draw_shadow[mapx, mapy] == 0 || _mapData[mapx, mapy].Layers.layer_gui_glyphs[(int)layer_types.LAYER_MONSTER] == NoGlyph)
                                                            )
                                                            continue;

                                                        bool loc_is_you = (_mapData[mapx, mapy].Layers.layer_flags & (ulong)LayerFlags.LFLAGS_UXUY) != 0;
                                                        bool showing_detection = (_mapData[mapx, mapy].Layers.layer_flags & (ulong)LayerFlags.LFLAGS_SHOWING_DETECTION) != 0;
                                                        bool canspotself = (_mapData[mapx, mapy].Layers.monster_flags & (ulong)LayerMonsterFlags.LMFLAGS_CAN_SPOT_SELF) != 0;
                                                        short monster_height = _mapData[mapx, mapy].Layers.special_monster_layer_height;
                                                        float scaled_y_height_change = 0;
                                                        sbyte monster_origin_x = _mapData[mapx, mapy].Layers.monster_origin_x;
                                                        sbyte monster_origin_y = _mapData[mapx, mapy].Layers.monster_origin_y;
                                                        //long glyphprintanimcountervalue = _mapData[mapx, mapy].GlyphPrintAnimationCounterValue;
                                                        long glyphprintmaincountervalue = _mapData[mapx, mapy].GlyphPrintMainCounterValue;
                                                        float base_move_offset_x = 0, base_move_offset_y = 0;
                                                        int movediffx = (int)monster_origin_x - mapx;
                                                        int movediffy = (int)monster_origin_y - mapy;
                                                        //long animcounterdiff = currentcountervalue - glyphprintanimcountervalue;
                                                        long maincounterdiff = maincountervalue - glyphprintmaincountervalue;
                                                        //if (GHUtils.isok(monster_origin_x, monster_origin_y) && layer_idx == (int)layer_types.LAYER_MONSTER)
                                                        //    mapx = mapx;

                                                        if (GHUtils.isok(monster_origin_x, monster_origin_y)
                                                            && (movediffx != 0 || movediffy != 0)
                                                            && maincounterdiff >= 0 && maincounterdiff < moveIntervals)
                                                        {
                                                            base_move_offset_x = width * (float)movediffx * (float)(moveIntervals - maincounterdiff) / (float)moveIntervals;
                                                            base_move_offset_y = height * (float)movediffy * (float)(moveIntervals - maincounterdiff) / (float)moveIntervals;
                                                        }

                                                        if (layer_idx == (int)layer_types.MAX_LAYERS + 1)
                                                        {
                                                            if (monster_height > 0)
                                                                scaled_y_height_change = (float)-monster_height * height / (float)GHConstants.TileHeight;

                                                            /* Grid */
                                                            if (MapGrid)
                                                            {
                                                                tx = (offsetX + usedOffsetX + width * (float)mapx);
                                                                ty = (offsetY + usedOffsetY + _mapFontAscent + height * (float)mapy);

                                                                textPaint.Style = SKPaintStyle.Stroke;
                                                                textPaint.StrokeWidth = 2.0f;
                                                                textPaint.Color = SKColors.Black;
                                                                textPaint.PathEffect = _pathEffect;
                                                                SKPoint p0 = new SKPoint(tx, ty);
                                                                SKPoint p1 = new SKPoint(tx, ty + height);
                                                                canvas.DrawLine(p0, p1, textPaint);
                                                                SKPoint p2 = new SKPoint(tx + width, ty + height);
                                                                canvas.DrawLine(p1, p2, textPaint);
                                                                textPaint.PathEffect = null;
                                                                textPaint.Style = SKPaintStyle.Fill;
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
                                                                int cglyph = cidx + CursorOff;
                                                                int ctile = Glyph2Tile[cglyph];
                                                                int animation = Tile2Animation[ctile];
                                                                int autodraw = Tile2Autodraw[ctile];
                                                                int anim_frame_idx = 0, main_tile_idx = 0;
                                                                sbyte mapAnimated = 0;
                                                                int tile_animation_idx = _gnollHackService.GetTileAnimationIndexFromGlyph(cglyph);
                                                                ctile = _gnollHackService.GetAnimatedTile(ctile, tile_animation_idx, (int)animation_play_types.ANIMATION_PLAY_TYPE_ALWAYS, generalcountervalue, out anim_frame_idx, out main_tile_idx, out mapAnimated, ref autodraw);
                                                                int sheet_idx = TileSheetIdx(ctile);
                                                                int tile_x = TileSheetX(ctile);
                                                                int tile_y = TileSheetY(ctile);

                                                                tx = (offsetX + usedOffsetX + (loc_is_you ? base_move_offset_x : 0) + width * (float)mapx);
                                                                ty = (offsetY + usedOffsetY + (loc_is_you ? base_move_offset_y : 0) + scaled_y_height_change + _mapFontAscent + height * (float)mapy);
                                                                SKRect targetrect = new SKRect(tx, ty, tx + width, ty + height);
                                                                SKRect sourcerect = new SKRect(tile_x, tile_y, tile_x + GHConstants.TileWidth, tile_y + GHConstants.TileHeight);
                                                                canvas.DrawBitmap(TileMap[sheet_idx], sourcerect, targetrect);

                                                            }

                                                            /* General tx, ty for all others, except cursors */
                                                            tx = (offsetX + usedOffsetX + base_move_offset_x + width * (float)mapx);
                                                            ty = (offsetY + usedOffsetY + base_move_offset_y + scaled_y_height_change + _mapFontAscent + height * (float)mapy);

                                                            if (HitPointBars)
                                                            {
                                                                /* Draw hit point bars */
                                                                if ((_mapData[mapx, mapy].Layers.layer_flags & (ulong)(LayerFlags.LFLAGS_M_YOU | LayerFlags.LFLAGS_UXUY | LayerFlags.LFLAGS_M_CANSPOTMON)) != 0
                                                                && (_mapData[mapx, mapy].Layers.layer_flags & (ulong)(LayerFlags.LFLAGS_M_WORM_TAIL)) == 0
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

                                                            bool draw_character = false;
                                                            /* Player mark */
                                                            if (PlayerMark && loc_is_you)
                                                            {
                                                                int cglyph = (int)game_ui_tile_types.U_TILE_MARK + UITileOff;
                                                                int ctile = Glyph2Tile[cglyph];
                                                                int animation = Tile2Animation[ctile];
                                                                int autodraw = Tile2Autodraw[ctile];
                                                                int anim_frame_idx = 0, main_tile_idx = 0;
                                                                sbyte mapAnimated = 0;
                                                                int tile_animation_idx = _gnollHackService.GetTileAnimationIndexFromGlyph(cglyph);
                                                                ctile = _gnollHackService.GetAnimatedTile(ctile, tile_animation_idx, (int)animation_play_types.ANIMATION_PLAY_TYPE_ALWAYS, generalcountervalue, out anim_frame_idx, out main_tile_idx, out mapAnimated, ref autodraw);
                                                                int sheet_idx = TileSheetIdx(ctile);
                                                                int tile_x = TileSheetX(ctile);
                                                                int tile_y = TileSheetY(ctile);

                                                                SKRect targetrect = new SKRect(tx, ty, tx + width, ty + height);
                                                                SKRect sourcerect = new SKRect(tile_x, tile_y, tile_x + GHConstants.TileWidth, tile_y + GHConstants.TileHeight);
                                                                canvas.DrawBitmap(TileMap[sheet_idx], sourcerect, targetrect);

                                                                if (_mapData[mapx, mapy].Symbol != null && _mapData[mapx, mapy].Symbol != "")
                                                                {
                                                                    draw_character = true;
                                                                }
                                                            }

                                                            /* Monster targeting mark */
                                                            if (MonsterTargeting && !loc_is_you && (_mapData[mapx, mapy].Layers.layer_flags & (ulong)(LayerFlags.LFLAGS_M_CANSPOTMON)) != 0)
                                                            {
                                                                int cglyph = (int)game_ui_tile_types.MAIN_TILE_MARK + UITileOff;
                                                                int ctile = Glyph2Tile[cglyph];
                                                                int animation = Tile2Animation[ctile];
                                                                int autodraw = Tile2Autodraw[ctile];
                                                                int anim_frame_idx = 0, main_tile_idx = 0;
                                                                sbyte mapAnimated = 0;
                                                                int tile_animation_idx = _gnollHackService.GetTileAnimationIndexFromGlyph(cglyph);
                                                                ctile = _gnollHackService.GetAnimatedTile(ctile, tile_animation_idx, (int)animation_play_types.ANIMATION_PLAY_TYPE_ALWAYS, generalcountervalue, out anim_frame_idx, out main_tile_idx, out mapAnimated, ref autodraw);
                                                                int sheet_idx = TileSheetIdx(ctile);
                                                                int tile_x = TileSheetX(ctile);
                                                                int tile_y = TileSheetY(ctile);

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
                                                                textPaint.TextSize = UsedFontSize / 4;
                                                                textPaint.Typeface = App.DejaVuSansMonoTypeface;
                                                                textPaint.Color = _mapData[mapx, mapy].Color;
                                                                textPaint.TextAlign = SKTextAlign.Center;
                                                                float textheight = textPaint.FontSpacing; // FontMetrics.Descent - textPaint.FontMetrics.Ascent;
                                                                float texttx = tx + width / 2;
                                                                float textty = ty + height / 2 - textheight / 2 - textPaint.FontMetrics.Ascent - 1f / 96f * height;
                                                                canvas.DrawText(_mapData[mapx, mapy].Symbol, texttx, textty, textPaint);
                                                                textPaint.TextAlign = SKTextAlign.Left;
                                                            }

                                                            if ((_mapData[mapx, mapy].Layers.layer_flags & (ulong)(LayerFlags.LFLAGS_M_YOU | LayerFlags.LFLAGS_UXUY | LayerFlags.LFLAGS_M_CANSPOTMON)) != 0
                                                                && (_mapData[mapx, mapy].Layers.layer_flags & (ulong)(LayerFlags.LFLAGS_M_WORM_TAIL)) == 0)
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
                                                                    int mglyph = (int)game_ui_tile_types.STATUS_MARKS + UITileOff;
                                                                    int mtile = Glyph2Tile[mglyph];
                                                                    int sheet_idx = TileSheetIdx(mtile);
                                                                    int tile_x = TileSheetX(mtile);
                                                                    int tile_y = TileSheetY(mtile);
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

                                                                            canvas.DrawBitmap(TileMap[sheet_idx], source_rt, target_rt);

                                                                            status_count++;
                                                                        }
                                                                    }
                                                                }

                                                                ulong condition_bits = _mapData[mapx, mapy].Layers.condition_bits;
                                                                if (condition_bits != 0)
                                                                {
                                                                    int tiles_per_row = GHConstants.TileWidth / GHConstants.StatusMarkWidth;
                                                                    int mglyph = (int)game_ui_tile_types.CONDITION_MARKS + UITileOff;
                                                                    int mtile = Glyph2Tile[mglyph];
                                                                    int sheet_idx = TileSheetIdx(mtile);
                                                                    int tile_x = TileSheetX(mtile);
                                                                    int tile_y = TileSheetY(mtile);
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

                                                                            canvas.DrawBitmap(TileMap[sheet_idx], source_rt, target_rt);

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
                                                                                int mglyph = (propidx - 1) / GHConstants.BUFFS_PER_TILE + BuffTileOff;
                                                                                int mtile = Glyph2Tile[mglyph];
                                                                                int sheet_idx = TileSheetIdx(mtile);
                                                                                int tile_x = TileSheetX(mtile);
                                                                                int tile_y = TileSheetY(mtile);

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

                                                                                canvas.DrawBitmap(TileMap[sheet_idx], source_rt, target_rt);

                                                                                status_count++;
                                                                            }
                                                                        }
                                                                    }
                                                                }

                                                            }

                                                            /* Draw death and hit markers */
                                                            if ((_mapData[mapx, mapy].Layers.layer_flags & (ulong)LayerFlags.LFLAGS_M_KILLED) != 0)
                                                            {
                                                                int mglyph = (int)general_tile_types.GENERAL_TILE_DEATH + GeneralTileOff;
                                                                int mtile = Glyph2Tile[mglyph];
                                                                int sheet_idx = TileSheetIdx(mtile);
                                                                int tile_x = TileSheetX(mtile);
                                                                int tile_y = TileSheetY(mtile);

                                                                SKRect targetrect = new SKRect(tx, ty, tx + width, ty + height);
                                                                SKRect sourcerect = new SKRect(tile_x, tile_y, tile_x + GHConstants.TileWidth, tile_y + GHConstants.TileHeight);
                                                                canvas.DrawBitmap(TileMap[sheet_idx], sourcerect, targetrect);
                                                            }
                                                            else if ((_mapData[mapx, mapy].Layers.layer_flags & (ulong)LayerFlags.LFLAGS_M_BEING_HIT) != 0)
                                                            {
                                                                short hit_text_num = _mapData[mapx, mapy].Layers.hit_tile;
                                                                int mglyph = Math.Max(0, Math.Min((int)hit_tile_types.MAX_HIT_TILES - 1, (int)hit_text_num)) + HitTileOff;
                                                                int mtile = Glyph2Tile[mglyph];
                                                                int sheet_idx = TileSheetIdx(mtile);
                                                                int tile_x = TileSheetX(mtile);
                                                                int tile_y = TileSheetY(mtile);

                                                                SKRect targetrect = new SKRect(tx, ty, tx + width, ty + height);
                                                                SKRect sourcerect = new SKRect(tile_x, tile_y, tile_x + GHConstants.TileWidth, tile_y + GHConstants.TileHeight);
                                                                canvas.DrawBitmap(TileMap[sheet_idx], sourcerect, targetrect);
                                                            }

                                                        }
                                                        else
                                                        {
                                                            int sub_layer_cnt = 1;
                                                            lock (_objectDataLock)
                                                            {
                                                                if (layer_idx == (int)layer_types.LAYER_OBJECT)
                                                                {
                                                                    if ((_mapData[mapx, mapy].Layers.layer_flags & (ulong)LayerFlags.LFLAGS_SHOWING_MEMORY) != 0)
                                                                        sub_layer_cnt = _objectData[mapx, mapy].MemoryObjectList == null ? 0 : Math.Min(GHConstants.MaxObjectsDrawn, _objectData[mapx, mapy].MemoryObjectList.Count);
                                                                    else if ((_mapData[mapx, mapy].Layers.layer_flags & (ulong)LayerFlags.LFLAGS_CAN_SEE) != 0)
                                                                        sub_layer_cnt = _objectData[mapx, mapy].FloorObjectList == null ? 0 : Math.Min(GHConstants.MaxObjectsDrawn, _objectData[mapx, mapy].FloorObjectList.Count);
                                                                    else
                                                                        sub_layer_cnt = 1; /* As a backup, show layer glyph (probably often NoGlyph) */
                                                                }
                                                                else if (layer_idx == (int)layer_types.LAYER_COVER_OBJECT)
                                                                {
                                                                    if ((_mapData[mapx, mapy].Layers.layer_flags & (ulong)LayerFlags.LFLAGS_SHOWING_MEMORY) != 0)
                                                                        sub_layer_cnt = _objectData[mapx, mapy].CoverMemoryObjectList == null ? 0 : Math.Min(GHConstants.MaxObjectsDrawn, _objectData[mapx, mapy].CoverMemoryObjectList.Count);
                                                                    else if ((_mapData[mapx, mapy].Layers.layer_flags & (ulong)LayerFlags.LFLAGS_CAN_SEE) != 0)
                                                                        sub_layer_cnt = _objectData[mapx, mapy].CoverFloorObjectList == null ? 0 : Math.Min(GHConstants.MaxObjectsDrawn, _objectData[mapx, mapy].CoverFloorObjectList.Count);
                                                                    else
                                                                        sub_layer_cnt = 1; /* As a backup, show layer glyph (probably often NoGlyph) */
                                                                }
                                                                for (int sub_layer_idx = sub_layer_cnt - 1; sub_layer_idx >= 0; sub_layer_idx--)
                                                                {

                                                                    int signed_glyph = NoGlyph;
                                                                    short obj_height = _mapData[mapx, mapy].Layers.object_height;

                                                                    //long glyphobjectprintanimcountervalue = _mapData[mapx, mapy].GlyphObjectPrintAnimationCounterValue;
                                                                    long glyphobjectprintmaincountervalue = _mapData[mapx, mapy].GlyphObjectPrintMainCounterValue;
                                                                    long objectcounterdiff = maincountervalue - glyphobjectprintmaincountervalue;
                                                                    sbyte object_origin_x = 0;
                                                                    sbyte object_origin_y = 0;

                                                                    ObjectDataItem otmp_round = null;

                                                                    int source_main_dir_num = 1;
                                                                    switch (layer_idx)
                                                                    {
                                                                        case (int)layer_types.LAYER_MONSTER:
                                                                            source_main_dir_num = GHConstants.NUM_WORM_SOURCE_DIRS + 1;
                                                                            break;
                                                                        case (int)layer_types.LAYER_CHAIN:
                                                                            source_main_dir_num = GHConstants.NUM_CHAIN_SOURCE_DIRS + 1;
                                                                            break;
                                                                        case (int)layer_types.LAYER_ZAP:
                                                                            source_main_dir_num = GHConstants.NUM_ZAP_SOURCE_DIRS + 1;
                                                                            break;
                                                                    }

                                                                    for (int source_dir_main_idx = 0; source_dir_main_idx < source_main_dir_num; source_dir_main_idx++)
                                                                    {
                                                                        int source_dir_idx = source_dir_main_idx;
                                                                        switch (layer_idx)
                                                                        {
                                                                            case (int)layer_types.LAYER_CHAIN:
                                                                            case (int)layer_types.LAYER_MONSTER:
                                                                                source_dir_idx = source_dir_main_idx * 2;
                                                                                break;
                                                                        }

                                                                        bool manual_hflip = false;
                                                                        bool manual_vflip = false;
                                                                        int adj_x = mapx;
                                                                        int adj_y = mapy;
                                                                        if (source_dir_idx > 0)
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

                                                                            if (layer_idx == (int)layer_types.LAYER_ZAP)
                                                                            {
                                                                                int adjacent_zap_glyph = _mapData[mapx, mapy].Layers.layer_gui_glyphs[(int)layer_types.LAYER_ZAP];
                                                                                ulong adjacent_layer_flags = (ulong)_mapData[mapx, mapy].Layers.layer_flags;

                                                                                if (adjacent_zap_glyph == NoGlyph) // || !glyph_is_zap(adjacent_zap_glyph))
                                                                                    signed_glyph = NoGlyph;
                                                                                else
                                                                                    signed_glyph = _gnollHackService.ZapGlyphToCornerGlyph(adjacent_zap_glyph, adjacent_layer_flags, source_dir_idx);
                                                                            }
                                                                            else if (layer_idx == (int)layer_types.LAYER_MONSTER)
                                                                            {
                                                                                /* Worm */
                                                                                uint worm_id_stored = _mapData[mapx, mapy].Layers.m_id;
                                                                                if (worm_id_stored == 0)
                                                                                    continue;

                                                                                bool is_long_worm_with_tail = (_mapData[mapx, mapy].Layers.monster_flags & (ulong)LayerMonsterFlags.LMFLAGS_LONG_WORM_WITH_TAIL) != 0;
                                                                                bool is_long_worm_tail = (_mapData[mapx, mapy].Layers.monster_flags & (ulong)LayerMonsterFlags.LMFLAGS_LONG_WORM_TAIL) != 0;
                                                                                bool is_adj_worm_tail = (_mapData[mapx, mapy].Layers.layer_flags & (ulong)LayerFlags.LFLAGS_M_WORM_TAIL) != 0;
                                                                                bool is_adj_worm_seen = (_mapData[mapx, mapy].Layers.layer_flags & (ulong)LayerFlags.LFLAGS_M_WORM_SEEN) != 0;
                                                                                bool worm = !is_adj_worm_tail ? false : is_adj_worm_seen ? (worm_id_stored > 0 ? true : false) : true;
                                                                                signed_glyph = NoGlyph;

                                                                                if (worm && (_mapData[mapx, mapy].Layers.layer_flags & (ulong)LayerFlags.LFLAGS_M_WORM_SEEN) != 0
                                                                                    && ((
                                                                                    _mapData[mapx, mapy].Layers.layer_flags & (ulong)LayerFlags.LFLAGS_CAN_SEE) != 0
                                                                                    || is_adj_worm_seen || (_mapData[mapx, mapy].Layers.layer_flags & (ulong)LayerFlags.LFLAGS_M_WORM_SEEN) != 0))
                                                                                {
                                                                                    if (is_long_worm_with_tail && !is_adj_worm_tail)
                                                                                    {
                                                                                        signed_glyph = NoGlyph;
                                                                                    }
                                                                                    else if (is_long_worm_tail || (is_long_worm_with_tail && is_adj_worm_tail))
                                                                                    {
                                                                                        int signed_main_glyph = _mapData[mapx, mapy].Layers.layer_gui_glyphs[layer_idx];
                                                                                        int main_glyph = Math.Abs(signed_main_glyph);
                                                                                        int tile_animation_index = _gnollHackService.GetTileAnimationIndexFromGlyph(main_glyph);
                                                                                        int main_tile = Glyph2Tile[main_glyph];
                                                                                        int wormautodraw = Tile2Autodraw[main_tile];
                                                                                        int base_source_glyph = NoGlyph;
                                                                                        if (wormautodraw > 0)
                                                                                        {
                                                                                            base_source_glyph = _autodraws[wormautodraw].source_glyph4;
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
                                                                            }
                                                                            else if (layer_idx == (int)layer_types.LAYER_CHAIN)
                                                                            {
                                                                                /* Chain */
                                                                                if ((_mapData[mapx, mapy].Layers.layer_flags & (ulong)LayerFlags.LFLAGS_O_CHAIN) != 0)
                                                                                {
                                                                                    signed_glyph = (source_dir_idx / 2 - 1) + (int)general_tile_types.GENERAL_TILE_CHAIN_IS_UP + GeneralTileOff;
                                                                                }
                                                                                else
                                                                                    signed_glyph = NoGlyph;
                                                                            }
                                                                        }
                                                                        else if (layer_idx == (int)layer_types.LAYER_OBJECT)
                                                                        {
                                                                            if ((_mapData[mapx, mapy].Layers.layer_flags & (ulong)LayerFlags.LFLAGS_SHOWING_MEMORY) != 0)
                                                                            {
                                                                                otmp_round = _objectData[mapx, mapy].MemoryObjectList[sub_layer_idx];
                                                                                signed_glyph = _objectData[mapx, mapy].MemoryObjectList[sub_layer_idx].ObjData.gui_glyph;
                                                                                obj_height = _objectData[mapx, mapy].MemoryObjectList[sub_layer_idx].TileHeight;
                                                                                object_origin_x = _objectData[mapx, mapy].MemoryObjectList[sub_layer_idx].ObjData.ox0;
                                                                                object_origin_y = _objectData[mapx, mapy].MemoryObjectList[sub_layer_idx].ObjData.oy0;
                                                                            }
                                                                            else if ((_mapData[mapx, mapy].Layers.layer_flags & (ulong)LayerFlags.LFLAGS_CAN_SEE) != 0)
                                                                            {
                                                                                otmp_round = _objectData[mapx, mapy].FloorObjectList[sub_layer_idx];
                                                                                signed_glyph = _objectData[mapx, mapy].FloorObjectList[sub_layer_idx].ObjData.gui_glyph;
                                                                                obj_height = _objectData[mapx, mapy].FloorObjectList[sub_layer_idx].TileHeight;
                                                                                object_origin_x = _objectData[mapx, mapy].FloorObjectList[sub_layer_idx].ObjData.ox0;
                                                                                object_origin_y = _objectData[mapx, mapy].FloorObjectList[sub_layer_idx].ObjData.oy0;
                                                                            }
                                                                            else
                                                                            {
                                                                                signed_glyph = _mapData[mapx, mapy].Layers.layer_gui_glyphs == null ? NoGlyph : _mapData[mapx, mapy].Layers.layer_gui_glyphs[layer_idx];
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
                                                                                signed_glyph = _mapData[mapx, mapy].Layers.layer_gui_glyphs == null ? NoGlyph : _mapData[mapx, mapy].Layers.layer_gui_glyphs[layer_idx];
                                                                            }
                                                                        }
                                                                        else
                                                                        {
                                                                            int used_layer_idx = layer_idx;
                                                                            if (layer_idx == (int)layer_types.MAX_LAYERS)
                                                                                used_layer_idx = (int)layer_types.LAYER_MONSTER;
                                                                            signed_glyph = _mapData[mapx, mapy].Layers.layer_gui_glyphs == null ? NoGlyph : _mapData[mapx, mapy].Layers.layer_gui_glyphs[used_layer_idx];
                                                                        }

                                                                        if (signed_glyph == NoGlyph)
                                                                            continue;

                                                                        int glyph = Math.Abs(signed_glyph);
                                                                        if (glyph == 0 || glyph >= Glyph2Tile.Length)
                                                                            continue;

                                                                        float object_move_offset_x = 0, object_move_offset_y = 0;
                                                                        int objectmovediffx = (int)object_origin_x - mapx;
                                                                        int objectmovediffy = (int)object_origin_y - mapy;

                                                                        if (GHUtils.isok(object_origin_x, object_origin_y)
                                                                            && (objectmovediffx != 0 || objectmovediffy != 0)
                                                                            && objectcounterdiff >= 0 && objectcounterdiff < moveIntervals)
                                                                        {
                                                                            object_move_offset_x = width * (float)objectmovediffx * (float)(moveIntervals - objectcounterdiff) / (float)moveIntervals;
                                                                            object_move_offset_y = height * (float)objectmovediffy * (float)(moveIntervals - objectcounterdiff) / (float)moveIntervals;
                                                                        }



                                                                        short missile_height = _mapData[mapx, mapy].Layers.missile_height;
                                                                        bool obj_in_pit = (_mapData[mapx, mapy].Layers.layer_flags & (ulong)LayerFlags.LFLAGS_O_IN_PIT) != 0;

                                                                        /* Base flips */
                                                                        bool hflip = (signed_glyph < 0);

                                                                        /* Tile flips */
                                                                        bool tileflag_hflip = (GlyphTileFlags[glyph] & (byte)glyph_tile_flags.GLYPH_TILE_FLAG_FLIP_HORIZONTALLY) != 0;
                                                                        bool tileflag_vflip = (GlyphTileFlags[glyph] & (byte)glyph_tile_flags.GLYPH_TILE_FLAG_FLIP_VERTICALLY) != 0;
                                                                        bool tileflag_halfsize = (GlyphTileFlags[glyph] & (byte)glyph_tile_flags.GLYPH_TILE_FLAG_HALF_SIZED_TILE) != 0;
                                                                        bool tileflag_floortile = (GlyphTileFlags[glyph] & (byte)glyph_tile_flags.GLYPH_TILE_FLAG_HAS_FLOOR_TILE) != 0;
                                                                        bool tileflag_normalobjmissile = (GlyphTileFlags[glyph] & (byte)glyph_tile_flags.GLYPH_TILE_FLAG_NORMAL_ITEM_AS_MISSILE) != 0;
                                                                        bool tileflag_fullsizeditem = (GlyphTileFlags[glyph] & (byte)glyph_tile_flags.GLYPH_TILE_FLAG_FULL_SIZED_ITEM) != 0;

                                                                        /* All items are big when showing detection */
                                                                        if (showing_detection)
                                                                        {
                                                                            obj_height = 0;
                                                                            tileflag_floortile = false;
                                                                        }

                                                                        if ((!tileflag_halfsize || monster_height > 0) && is_monster_like_layer)
                                                                        {
                                                                            scaled_y_height_change = (float)-monster_height * height / (float)GHConstants.TileHeight;
                                                                            if (monster_height < 0)
                                                                                scaled_y_height_change -= GHConstants.PIT_BOTTOM_BORDER * targetscale;
                                                                        }
                                                                        else if (tileflag_halfsize && (layer_idx == (int)layer_types.LAYER_OBJECT || layer_idx == (int)layer_types.LAYER_COVER_OBJECT))
                                                                            scaled_y_height_change = (float)(-(sub_layer_cnt - 1 - sub_layer_idx) * GHConstants.OBJECT_PILE_HEIGHT_DIFFERENCE - GHConstants.OBJECT_PILE_START_HEIGHT) * targetscale;

                                                                        int ntile = Glyph2Tile[glyph];
                                                                        int animation = Tile2Animation[ntile];
                                                                        int autodraw = Tile2Autodraw[ntile];
                                                                        int anim_frame_idx = 0, main_tile_idx = 0;
                                                                        sbyte mapAnimated = 0;
                                                                        int tile_animation_idx = _gnollHackService.GetTileAnimationIndexFromGlyph(glyph);
                                                                        bool is_dropping_piercer = (_mapData[mapx, mapy].Layers.layer_flags & (ulong)LayerFlags.LFLAGS_M_DROPPING_PIERCER) != 0;

                                                                        /* Determine animation tile here */
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

                                                                        int enlargement = Tile2Enlargement[ntile];
                                                                        //int enl_idx = _draw_order[draw_idx].enlargement_position;

                                                                        for (int enl_idx = -1; enl_idx < 5; enl_idx++)
                                                                        {
                                                                            if (enlargement == 0 && enl_idx >= 0)
                                                                                break;

                                                                            bool vflip_glyph = false;
                                                                            bool hflip_glyph = false;
                                                                            if ((hflip != tileflag_hflip) != manual_hflip) /* XOR */
                                                                                hflip_glyph = true;
                                                                            else
                                                                                hflip_glyph = false;

                                                                            if (tileflag_vflip != manual_vflip) /* XOR */
                                                                                vflip_glyph = true;
                                                                            else
                                                                                vflip_glyph = false;


                                                                            int enlarg_idx = enl_idx;
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
                                                                                int enl_tile_idx = _enlargementDefs[enlargement].position2tile[orig_position_index];
                                                                                if (enl_tile_idx >= 0)
                                                                                {
                                                                                    int addedindex = 0;
                                                                                    if (_enlargementDefs[enlargement].number_of_animation_frames > 0)
                                                                                    {
                                                                                        if (main_tile_idx == -1
                                                                                            && anim_frame_idx >= 0
                                                                                            && anim_frame_idx < _enlargementDefs[enlargement].number_of_animation_frames
                                                                                            )
                                                                                        {
                                                                                            addedindex = anim_frame_idx * _enlargementDefs[enlargement].number_of_enlargement_tiles;
                                                                                        }
                                                                                        else if (main_tile_idx == 0
                                                                                            && anim_frame_idx > 0
                                                                                            && anim_frame_idx <= _enlargementDefs[enlargement].number_of_animation_frames)
                                                                                        {
                                                                                            addedindex = (anim_frame_idx - 1) * _enlargementDefs[enlargement].number_of_enlargement_tiles;
                                                                                        }
                                                                                        else if (main_tile_idx == _enlargementDefs[enlargement].number_of_animation_frames
                                                                                            && anim_frame_idx >= 0
                                                                                            && anim_frame_idx < _enlargementDefs[enlargement].number_of_animation_frames
                                                                                            )
                                                                                        {
                                                                                            addedindex = anim_frame_idx * _enlargementDefs[enlargement].number_of_enlargement_tiles;
                                                                                        }
                                                                                    }
                                                                                    int enl_glyph = enl_tile_idx + addedindex + EnlargementOffsets[enlargement] + EnlargementOff;
                                                                                    ntile = Glyph2Tile[enl_glyph]; /* replace */
                                                                                    autodraw = Tile2Autodraw[ntile];
                                                                                }
                                                                                else
                                                                                    continue;
                                                                            }

                                                                            int dx = 0, dy = 0;
                                                                            int darken_dx = 0, darken_dy = 0;
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

                                                                            int draw_map_x = mapx + dx + (adj_x - mapx);
                                                                            int draw_map_y = mapy + dy + (adj_y - mapy);
                                                                            if (!GHUtils.isok(draw_map_x, draw_map_y))
                                                                                continue;

                                                                            darken_dx = dx;
                                                                            darken_dy = 0;
                                                                            //int darken_x = mapx + darken_dx;
                                                                            //int darken_y = mapy + darken_dy;
                                                                            //bool darken = ((_mapData[darken_x, darken_y].Layers.layer_flags & (ulong)LayerFlags.LFLAGS_CAN_SEE) == 0);
                                                                            //if (_mapData[mapx, mapy].Layers.layer_gui_glyphs != null
                                                                            //    && (_mapData[mapx, mapy].Layers.layer_gui_glyphs[(int)layer_types.LAYER_FLOOR] == UnexploredGlyph
                                                                            //        || _mapData[mapx, mapy].Layers.layer_gui_glyphs[(int)layer_types.LAYER_FLOOR] == NoGlyph)
                                                                            //   )
                                                                            //    darken = false;

                                                                            if (dx != 0 || dy != 0)
                                                                            {
                                                                                draw_shadow[draw_map_x, draw_map_y] |= 1;
                                                                            }

                                                                            int sheet_idx = TileSheetIdx(ntile);
                                                                            int tile_x = TileSheetX(ntile);
                                                                            int tile_y = TileSheetY(ntile);

                                                                            SKRect sourcerect;
                                                                            float scaled_tile_width = width;
                                                                            float scaled_tile_height = tileflag_halfsize || (tileflag_normalobjmissile && !tileflag_fullsizeditem) ? height / 2 : height;
                                                                            float scaled_x_padding = 0;
                                                                            float scaled_y_padding = 0;
                                                                            int source_y_added = 0;
                                                                            int source_height_deducted = 0;
                                                                            int source_height = tileflag_halfsize ? GHConstants.TileHeight / 2 : GHConstants.TileHeight;

                                                                            float scale = 1.0f;
                                                                            if (tileflag_halfsize)
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

                                                                                if (tileflag_floortile)
                                                                                {
                                                                                    if ((layer_idx == (int)layer_types.LAYER_OBJECT || layer_idx == (int)layer_types.LAYER_OBJECT)
                                                                                        && obj_height > 0 && obj_height < 48)
                                                                                    {
                                                                                        source_y_added = (GHConstants.TileHeight / 2 - obj_height) / 2;
                                                                                        source_height_deducted = GHConstants.TileHeight / 2 - obj_height;
                                                                                        source_height = GHConstants.TileHeight / 2 - source_height_deducted;
                                                                                        scaled_tile_width = scale * width;
                                                                                        scaled_x_padding = (width - scaled_tile_width) / 2;
                                                                                        scaled_tile_height = scale * (float)source_height * height / (float)GHConstants.TileHeight;
                                                                                        scaled_y_padding = Math.Max(0, scale * (float)source_height_deducted * height / (float)GHConstants.TileHeight - pit_border);
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
                                                                                    opaqueness = (draw_shadow[mapx, mapy] & 2) != 0 && (_mapData[mapx, mapy].Layers.monster_flags & (ulong)LayerMonsterFlags.LMFLAGS_GLASS_TRANSPARENCY) != 0 ? 0.65f : 0.5f;
                                                                                }
                                                                                else if ((_mapData[mapx, mapy].Layers.monster_flags & (ulong)(LayerMonsterFlags.LMFLAGS_INVISIBLE_TRANSPARENT | LayerMonsterFlags.LMFLAGS_SEMI_TRANSPARENT | LayerMonsterFlags.LMFLAGS_RADIAL_TRANSPARENCY)) != 0)
                                                                                {
                                                                                    draw_shadow[mapx, mapy] |= 2;
                                                                                    continue; /* Draw only the transparent shadow in the max_layers shadow layer; otherwise, if drawn twice, the result will be nontransparent */
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

                                                                            tx = (offsetX + usedOffsetX + move_offset_x + width * (float)draw_map_x);
                                                                            ty = (offsetY + usedOffsetY + move_offset_y + scaled_y_height_change + _mapFontAscent + height * (float)draw_map_y);

                                                                            using (new SKAutoCanvasRestore(canvas, true))
                                                                            {
                                                                                canvas.Translate(tx + (hflip_glyph ? width : 0), ty + (vflip_glyph ? height : 0));
                                                                                canvas.Scale(hflip_glyph ? -1 : 1, vflip_glyph ? -1 : 1, 0, 0);
                                                                                SKRect targetrect;
                                                                                if (tileflag_halfsize)
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

                                                                                if (is_monster_like_layer && (_mapData[mapx, mapy].Layers.monster_flags & (ulong)LayerMonsterFlags.LMFLAGS_RADIAL_TRANSPARENCY) != 0)
                                                                                {
                                                                                    IntPtr tempptraddr = _tempBitmap.GetPixels();
                                                                                    IntPtr tileptraddr = TileMap[sheet_idx].GetPixels();
                                                                                    double mid_x = (double)GHConstants.TileWidth / 2.0 - 0.5;
                                                                                    double mid_y = (double)GHConstants.TileHeight / 2.0 - 0.5;
                                                                                    double r = 0, semi_transparency = 0;
                                                                                    byte radial_opacity = 0x00;
                                                                                    int bytesperpixel = TileMap[sheet_idx].BytesPerPixel;
                                                                                    int copywidth = Math.Min((int)sourcerect.Width, _tempBitmap.Width);
                                                                                    int copyheight = Math.Min((int)sourcerect.Height, _tempBitmap.Height);
                                                                                    int tilemapwidth = TileMap[sheet_idx].Width;
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
                                                                                                semi_transparency = r * 0.0375;
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

                                                                                    if ((_mapData[mapx, mapy].Layers.monster_flags & (ulong)LayerMonsterFlags.LMFLAGS_INVISIBLE_TRANSPARENT) != 0)
                                                                                        paint.Color = paint.Color.WithAlpha((byte)(0xFF * opaqueness));
                                                                                    canvas.DrawBitmap(_tempBitmap, tempsourcerect, targetrect, paint);
                                                                                }
                                                                                else
                                                                                {
                                                                                    paint.Color = paint.Color.WithAlpha((byte)(0xFF * opaqueness));
                                                                                    canvas.DrawBitmap(TileMap[sheet_idx], sourcerect, targetrect, paint);
                                                                                }
                                                                            }

                                                                            DrawAutoDraw(autodraw, canvas, paint, otmp_round,
                                                                                layer_idx, mapx, mapy,
                                                                                tileflag_halfsize, tileflag_normalobjmissile, tileflag_fullsizeditem,
                                                                                tx, ty, width, height,
                                                                                scale, targetscale, scaled_x_padding, scaled_y_padding, scaled_tile_height,
                                                                                false);

                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }

                                                /* Darkening at the end of layers */
                                                //if (_draw_order[draw_idx].darken) 
                                                if (layer_idx == (int)layer_types.LAYER_OBJECT)
                                                {
                                                    for (int mapx = startX; mapx <= endX; mapx++)
                                                    {
                                                        for (int mapy = startY; mapy <= endY; mapy++)
                                                        {
                                                            bool showing_detection = (_mapData[mapx, mapy].Layers.layer_flags & (ulong)LayerFlags.LFLAGS_SHOWING_DETECTION) != 0;
                                                            bool darken = (!showing_detection && (_mapData[mapx, mapy].Layers.layer_flags & (ulong)LayerFlags.LFLAGS_CAN_SEE) == 0);

                                                            if (_mapData[mapx, mapy].Layers.layer_gui_glyphs != null
                                                                && (_mapData[mapx, mapy].Layers.layer_gui_glyphs[(int)layer_types.LAYER_FLOOR] == UnexploredGlyph
                                                                    || _mapData[mapx, mapy].Layers.layer_gui_glyphs[(int)layer_types.LAYER_FLOOR] == NoGlyph)
                                                                )
                                                                darken = false;

                                                            // Draw rectangle with blend mode in bottom half
                                                            if (darken)
                                                            {
                                                                bool uloc = ((_mapData[mapx, mapy].Layers.layer_flags & (ulong)LayerFlags.LFLAGS_UXUY) != 0);
                                                                bool unlit = ((_mapData[mapx, mapy].Layers.layer_flags & (ulong)LayerFlags.LFLAGS_APPEARS_UNLIT) != 0);
                                                                // Get values from XAML controls
                                                                SKBlendMode blendMode = SKBlendMode.Modulate;
                                                                int val = ((uloc ? 85 : unlit ? 35 : 65) * 255) / 100;
                                                                SKColor color = new SKColor((byte)val, (byte)val, (byte)val);

                                                                paint.Color = color;
                                                                SKBlendMode old_bm = paint.BlendMode;
                                                                paint.BlendMode = blendMode;
                                                                tx = (offsetX + usedOffsetX + width * (float)mapx);
                                                                ty = (offsetY + usedOffsetY + _mapFontAscent + height * (float)mapy);
                                                                SKRect targetrect = new SKRect(tx, ty, tx + width, ty + height);
                                                                canvas.DrawRect(targetrect, paint);
                                                                paint.BlendMode = old_bm;
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
                        canvas.DrawRect(filterrect, textPaint);
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
                            textPaint.TextSize = UsedFontSize * ft.GetRelativeTextSize(maincountervalue);
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
                                canvas.DrawText(str, tx, ty, textPaint);
                            }
                            textPaint.Style = SKPaintStyle.Fill;
                            textPaint.Color = fillcolor;
                            canvas.DrawText(str, tx, ty, textPaint);
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
                            textPaint.TextSize = UsedFontSize;
                            textPaint.MeasureText(str, ref textBounds);
                            if (textBounds.Width > 0)
                            {
                                float relativesize = targetwidth / textBounds.Width;
                                //if (relativesize > maxfontsize)
                                //    relativesize = maxfontsize;
                                textPaint.TextSize = UsedFontSize * relativesize;
                            }

                            textPaint.MeasureText(str, ref textBounds);
                            float maintextascent = textPaint.FontMetrics.Ascent;
                            float maintextdescent = textPaint.FontMetrics.Descent;

                            tx = (canvaswidth / 2 - textBounds.Width / 2);
                            ty = (canvasheight / 2 - textBounds.Height / 2 - (maintextascent + maintextdescent) / 2) + yoffsetpct * canvasheight;

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

                            textPaint.TextSize = UsedFontSize;
                            sampletext = ft.GetSampleText();
                            textPaint.MeasureText(sampletext, ref textBounds);
                            if (textBounds.Width > 0)
                            {
                                float relativesize = relativetoscreenwidth * Math.Min(canvaswidth, canvasheight) / textBounds.Width;
                                textPaint.TextSize = UsedFontSize * relativesize;
                            }

                            textPaint.TextAlign = SKTextAlign.Center;
                            tx = canvaswidth / 2;
                            ty = GetStatusBarSkiaHeight() + 1.5f * inverse_canvas_scale * (float)ESCButton.Height - textPaint.FontMetrics.Ascent;
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
                            ty = offsetY + usedOffsetY + height * p.Y + _mapFontAscent;
                            textPaint.Color = effcolor;
                            switch(eff.Style)
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
                                            canvas.DrawBitmap(_searchBitmap, effRect, textPaint);
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
                                        canvas.DrawBitmap(_waitBitmap, effRect, textPaint);
                                    }
                                    break;
                                default:
                                    break;
                            }
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
                    canvas.DrawRect(0, 0, canvaswidth, canvasheight, textPaint);
                    textPaint.Style = oldstyle;
                    textPaint.Color = oldcolor;
                    textPaint.MaskFilter = oldfilter;
                }

                /* Darkening background */
                if (ForceAllMessages || ShowNumberPad || ShownTip >= 0)
                {
                    textPaint.Style = SKPaintStyle.Fill;
                    textPaint.Color = SKColors.Black.WithAlpha(128);
                    canvas.DrawRect(0, 0, canvaswidth, canvasheight, textPaint);
                }

                /* Window strings */
                float lastStatusRowPrintY = 0.0f;
                float lastStatusRowFontSpacing = 0.0f;
                _canvasButtonRect.Top = 0; /* Maybe overrwritten below */
                _canvasButtonRect.Bottom = canvasheight; /* Maybe overrwritten below */
                if (_clientGame != null)
                {
                    lock (_clientGame.WindowsLock)
                    {
                        for (int i = 0; _clientGame.Windows[i] != null && i < GHConstants.MaxGHWindows; i++)
                        {
                            if (_clientGame.Windows[i].Visible && (
                                _clientGame.Windows[i].WindowPrintStyle == GHWindowPrintLocations.PrintToMap
                                || _clientGame.Windows[i].WindowPrintStyle == GHWindowPrintLocations.RawPrint))
                            {
                                if (_clientGame.Windows[i].WindowType == GHWinType.Status && !ClassicStatusBar)
                                    continue;

                                textPaint.Typeface = _clientGame.Windows[i].Typeface;
                                textPaint.TextSize = _clientGame.Windows[i].TextSize * textscale;
                                textPaint.Color = _clientGame.Windows[i].TextColor;
                                width = textPaint.FontMetrics.AverageCharacterWidth;
                                height = textPaint.FontSpacing; // textPaint.FontMetrics.Descent - textPaint.FontMetrics.Ascent;

                                if (_clientGame.Windows[i].AutoPlacement)
                                {
                                    if (_clientGame.Windows[i].WindowType == GHWinType.Message)
                                    {
                                        float newleft = 0;
                                        float newtop = canvasheight - height * ActualDisplayedMessages - canvasheight * (float)ButtonRowStack.Height / Math.Max(1.0f, (float)canvasView.Height) - 30;
                                        _clientGame.Windows[i].Left = newleft;
                                        _clientGame.Windows[i].Top = newtop;
                                    }
                                    else if (_clientGame.Windows[i].WindowType == GHWinType.Here)
                                    {
                                        float newleft = 0;
                                        float messagetop = _clientGame.Windows[_clientGame.MessageWindowId].Top;
                                        float newtop = messagetop - _clientGame.Windows[i].Height - 10 * textscale;
                                        _clientGame.Windows[i].Left = newleft;
                                        _clientGame.Windows[i].Top = newtop;
                                    }
                                }

                                SKRect winRect = new SKRect(_clientGame.Windows[i].Left, _clientGame.Windows[i].Top,
                                    _clientGame.Windows[i].Right,
                                    _clientGame.Windows[i].Bottom);

                                if (_clientGame.Windows[i].CenterHorizontally && winRect.Right - winRect.Left < canvaswidth)
                                {
                                    float newleft = (canvaswidth - (winRect.Right - winRect.Left)) / 2;
                                    float addition = newleft - winRect.Left;
                                    winRect.Left += addition;
                                    winRect.Right += addition;
                                }

                                using (SKPaint winPaint = new SKPaint())
                                {
                                    winPaint.FilterQuality = SKFilterQuality.None;

                                    winPaint.Color = _clientGame.Windows[i].BackgroundColor;
                                    winPaint.Style = SKPaintStyle.Fill;

                                    if (winPaint.Color != SKColors.Transparent)
                                        canvas.DrawRect(winRect, winPaint);

                                    if (i == _clientGame.StatusWindowId && ClassicStatusBar)
                                        _canvasButtonRect.Top = winRect.Bottom;
                                    else if (i == _clientGame.MessageWindowId)
                                        _canvasButtonRect.Bottom = winRect.Top;
                                }

                                if (_clientGame.Windows[i].WindowType != GHWinType.Message && !ForceAllMessages)
                                {
                                    lock (_clientGame.Windows[i].PutStrsLock)
                                    {
                                        int j = 0;
                                        foreach (GHPutStrItem putstritem in _clientGame.Windows[i].PutStrs)
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
                                                tx = xpos + winRect.Left + _clientGame.Windows[i].Padding.Left;
                                                ty = winRect.Top + _clientGame.Windows[i].Padding.Top - textPaint.FontMetrics.Ascent + j * height;

                                                if (_clientGame.Windows[i].HasShadow)
                                                {
                                                    textPaint.Style = SKPaintStyle.Fill;
                                                    textPaint.Color = SKColors.Black;
                                                    textPaint.MaskFilter = _blur;
                                                    float shadow_offset = 0.15f * textPaint.TextSize;
                                                    canvas.DrawText(str, tx + shadow_offset, ty + shadow_offset, textPaint);
                                                    textPaint.MaskFilter = null;
                                                }
                                                if (_clientGame.Windows[i].StrokeWidth > 0)
                                                {
                                                    textPaint.Style = SKPaintStyle.Stroke;
                                                    textPaint.StrokeWidth = _clientGame.Windows[i].StrokeWidth * textscale;
                                                    textPaint.Color = SKColors.Black;
                                                    canvas.DrawText(str, tx, ty, textPaint);
                                                }
                                                textPaint.Style = SKPaintStyle.Fill;
                                                textPaint.Color = ClientUtils.NHColor2SKColor(instr.Color < (int)nhcolor.CLR_MAX ? instr.Color : (int)nhcolor.CLR_WHITE, instr.Attributes);
                                                canvas.DrawText(str, tx, ty, textPaint);
                                                textPaint.Style = SKPaintStyle.Fill;
                                                xpos += totwidth;

                                                if (_clientGame.Windows[i].WindowType == GHWinType.Status && lastStatusRowPrintY  < ty + textPaint.FontMetrics.Descent)
                                                {
                                                    lastStatusRowPrintY = ty + textPaint.FontMetrics.Descent;
                                                    lastStatusRowFontSpacing = textPaint.FontSpacing;
                                                }
                                            }
                                            j++;
                                        }
                                    }
                                }

                                if (_clientGame.Windows[i].WindowType == GHWinType.Message)
                                {
                                    lock(_mapOffsetLock)
                                    {
                                        _messageSmallestTop = canvasheight;
                                    }
                                    lock (_msgHistoryLock)
                                    {
                                        if (_msgHistory != null)
                                        {
                                            int j = ActualDisplayedMessages - 1, idx;
                                            float lineLengthLimit = 0.85f * canvaswidth;

                                            for (idx = _msgHistory.Count - 1; idx >= 0 && j >= 0; idx--)
                                            {
                                                GHMsgHistoryItem msgHistoryItem = _msgHistory[idx];
                                                longLine = msgHistoryItem.Text;
                                                SKColor printColor = ClientUtils.NHColor2SKColor(msgHistoryItem.NHColor < (int)nhcolor.CLR_MAX ? msgHistoryItem.NHColor : (int)nhcolor.CLR_WHITE, msgHistoryItem.Attributes);

                                                /* attributes */
                                                wrappedLines.Clear();
                                                float lineLength = 0.0f;
                                                string line = "";
                                                foreach (string word in longLine.Split(' '))
                                                {
                                                    wordWithSpace = word + " ";
                                                    float wordWithSpaceLength = textPaint.MeasureText(wordWithSpace);
                                                    if (lineLength + wordWithSpaceLength > lineLengthLimit)
                                                    {
                                                        wrappedLines.Add(line);
                                                        line = "" + wordWithSpace;
                                                        lineLength = wordWithSpaceLength;
                                                    }
                                                    else
                                                    {
                                                        line += wordWithSpace;
                                                        lineLength += wordWithSpaceLength;
                                                    }
                                                }
                                                wrappedLines.Add(line);

                                                int lineidx;
                                                for (lineidx = 0; lineidx < wrappedLines.Count; lineidx++)
                                                {
                                                    string wrappedLine = wrappedLines[lineidx];
                                                    int window_row_idx = j + lineidx - wrappedLines.Count + 1;
                                                    if (window_row_idx < 0)
                                                        continue;
                                                    tx = winRect.Left + _clientGame.Windows[i].Padding.Left;
                                                    ty = winRect.Top + _clientGame.Windows[i].Padding.Top - textPaint.FontMetrics.Ascent + window_row_idx * height;
                                                    if (ForceAllMessages)
                                                    {
                                                        lock(_mapOffsetLock)
                                                        {
                                                            ty += _messageOffsetY;
                                                            if(ty + textPaint.FontMetrics.Ascent < _messageSmallestTop)
                                                                _messageSmallestTop = ty + textPaint.FontMetrics.Ascent;
                                                        }
                                                    }
                                                    if (ty + textPaint.FontMetrics.Descent < 0)
                                                        continue;
                                                    if (ty - textPaint.FontMetrics.Ascent > canvasheight)
                                                        continue;
                                                    textPaint.Style = SKPaintStyle.Stroke;
                                                    textPaint.StrokeWidth = _clientGame.Windows[i].StrokeWidth * textscale;
                                                    textPaint.Color = SKColors.Black;
                                                    canvas.DrawText(wrappedLine, tx, ty, textPaint);
                                                    textPaint.Style = SKPaintStyle.Fill;
                                                    textPaint.StrokeWidth = 0;
                                                    textPaint.Color = printColor;
                                                    canvas.DrawText(wrappedLine, tx, ty, textPaint);
                                                    textPaint.Style = SKPaintStyle.Fill;
                                                    textPaint.StrokeWidth = 0;
                                                    textPaint.Color = printColor;
                                                }
                                                j -= wrappedLines.Count;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                float abilitybuttonbottom = (float)((lAbilitiesButton.Y + lAbilitiesButton.Height) / canvasView.Height) * canvasheight;
                float escbuttonbottom = (float)((ESCButton.Y + ESCButton.Height) / canvasView.Height) * canvasheight;
                if (_canvasButtonRect.Top < escbuttonbottom)
                    _canvasButtonRect.Top = escbuttonbottom;
                if (_canvasButtonRect.Top < abilitybuttonbottom)
                    _canvasButtonRect.Top = abilitybuttonbottom;

                SkillRect = new SKRect();
                StatusBarRect = new SKRect();
                YouRect = new SKRect();
                float orbleft = 5.0f;
                float orbbordersize = (float)(lAbilitiesButton.Width / canvasView.Width) * canvaswidth;

                bool statusfieldsok = false;
                lock (StatusFieldLock)
                {
                    statusfieldsok = StatusFields != null;
                }

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
                        _canvasButtonRect.Top = StatusBarRect.Bottom + 1.25f * inverse_canvas_scale * (float)ESCButton.Width;
                        canvas.DrawRect(darkenrect, textPaint);
                        textPaint.Color = SKColors.White;
                        textPaint.TextAlign = SKTextAlign.Left;
                        textPaint.Typeface = App.LatoRegular;
                        float target_scale = rowheight / _statusWizardBitmap.Height; // All are 64px high

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
                        if (valtext.StartsWith("W"))
                        {
                            target_width = target_scale * _statusWizardBitmap.Width;
                            target_height = target_scale * _statusWizardBitmap.Height;
                            statusDest = new SKRect(curx, cury, curx + target_width, cury + target_height);
                            canvas.DrawBitmap(_statusWizardBitmap, statusDest, textPaint);
                            curx += target_width;
                            curx += innerspacing;
                        }
                        else if (valtext.StartsWith("C"))
                        {
                            target_width = target_scale * _statusCasualBitmap.Width;
                            target_height = target_scale * _statusCasualBitmap.Height;
                            statusDest = new SKRect(curx, cury, curx + target_width, cury + target_height);
                            canvas.DrawBitmap(_statusCasualBitmap, statusDest, textPaint);
                            curx += target_width;
                            curx += innerspacing;
                        }
                        else if (valtext.StartsWith("S"))
                        {
                            target_width = target_scale * _statusCasualClassicBitmap.Width;
                            target_height = target_scale * _statusCasualClassicBitmap.Height;
                            statusDest = new SKRect(curx, cury, curx + target_width, cury + target_height);
                            canvas.DrawBitmap(_statusCasualClassicBitmap, statusDest, textPaint);
                            curx += target_width;
                            curx += innerspacing;
                        }
                        else if (valtext.StartsWith("M"))
                        {
                            target_width = target_scale * _statusModernBitmap.Width;
                            target_height = target_scale * _statusModernBitmap.Height;
                            statusDest = new SKRect(curx, cury, curx + target_width, cury + target_height);
                            canvas.DrawBitmap(_statusModernBitmap, statusDest, textPaint);
                            curx += target_width;
                            curx += innerspacing;
                        }

                        SKBitmap difbmp = _statusDifficultyBitmap;
                        string diftext = "";
                        if (valtext.Contains("s"))
                        {
                            diftext = "s";
                            difbmp = _statusDifficultyVeryEasyBitmap;
                        }
                        else if (valtext.Contains("e"))
                        {
                            diftext = "e";
                            difbmp = _statusDifficultyEasyBitmap;
                        }
                        else if (valtext.Contains("a"))
                        {
                            diftext = "a";
                            difbmp = _statusDifficultyAverageBitmap;
                        }
                        else if (valtext.Contains("v"))
                        {
                            diftext = "v";
                            difbmp = _statusDifficultyHardBitmap;
                        }
                        else if (valtext.Contains("x"))
                        {
                            diftext = "x";
                            difbmp = _statusDifficultyExpertBitmap;
                        }
                        else if (valtext.Contains("m"))
                        {
                            diftext = "m";
                            difbmp = _statusDifficultyMasterBitmap;
                        }
                        else if (valtext.Contains("g"))
                        {
                            diftext = "g";
                            difbmp = _statusDifficultyGrandMasterBitmap;
                        }

                        if (diftext != "")
                        {
                            target_width = target_scale * difbmp.Width;
                            target_height = target_scale * difbmp.Height;
                            statusDest = new SKRect(curx, cury, curx + target_width, cury + target_height);
                            canvas.DrawBitmap(difbmp, statusDest, textPaint);
                            textPaint.MeasureText(diftext, ref bounds);
                            textPaint.TextAlign = SKTextAlign.Center;
                            textPaint.Color = SKColors.Black;
                            textPaint.TextSize = diffontsize;
                            canvas.DrawText(diftext, curx + target_width / 2, cury + (rowheight - (textPaint.FontSpacing)) / 2 - textPaint.FontMetrics.Ascent, textPaint);
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
                            target_width = target_scale * _statusXPLevelBitmap.Width;
                            target_height = target_scale * _statusXPLevelBitmap.Height;
                            statusDest = new SKRect(curx, cury, curx + target_width, cury + target_height);
                            canvas.DrawBitmap(_statusXPLevelBitmap, statusDest, textPaint);
                            curx += target_width;
                            curx += innerspacing;
                            float print_width = textPaint.MeasureText(valtext);
                            canvas.DrawText(valtext, curx, cury - textPaint.FontMetrics.Ascent, textPaint);
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
                            target_width = target_scale * _statusHDBitmap.Width;
                            target_height = target_scale * _statusHDBitmap.Height;
                            statusDest = new SKRect(curx, cury, curx + target_width, cury + target_height);
                            canvas.DrawBitmap(_statusHDBitmap, statusDest, textPaint);
                            curx += target_width;
                            curx += innerspacing;
                            float print_width = textPaint.MeasureText(valtext);
                            canvas.DrawText(valtext, curx, cury - textPaint.FontMetrics.Ascent, textPaint);
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
                            target_width = target_scale * _statusACBitmap.Width;
                            target_height = target_scale * _statusACBitmap.Height;
                            statusDest = new SKRect(curx, cury, curx + target_width, cury + target_height);
                            canvas.DrawBitmap(_statusACBitmap, statusDest, textPaint);
                            textPaint.TextAlign = SKTextAlign.Center;
                            textPaint.Color = SKColors.Black;
                            textPaint.TextSize = shieldfontsize;
                            canvas.DrawText(valtext, curx + target_width / 2, cury + (rowheight - textPaint.FontSpacing) / 2 - textPaint.FontMetrics.Ascent, textPaint);
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
                            target_width = target_scale * _statusMCBitmap.Width;
                            target_height = target_scale * _statusMCBitmap.Height;
                            statusDest = new SKRect(curx, cury, curx + target_width, cury + target_height);
                            canvas.DrawBitmap(_statusMCBitmap, statusDest, textPaint);
                            textPaint.TextAlign = SKTextAlign.Center;
                            textPaint.Color = SKColors.Black;
                            textPaint.TextSize = shieldfontsize;
                            canvas.DrawText(valtext, curx + target_width / 2, cury + (rowheight - textPaint.FontSpacing) / 2 - textPaint.FontMetrics.Ascent, textPaint);
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
                            target_width = target_scale * _statusMoveBitmap.Width;
                            target_height = target_scale * _statusMoveBitmap.Height;
                            statusDest = new SKRect(curx, cury, curx + target_width, cury + target_height);
                            canvas.DrawBitmap(_statusMoveBitmap, statusDest, textPaint);
                            curx += target_width;
                            curx += innerspacing;
                            float print_width = textPaint.MeasureText(valtext);
                            canvas.DrawText(valtext, curx, cury - textPaint.FontMetrics.Ascent, textPaint);
                            curx += print_width + stdspacing;
                        }

                        valtext = "";
                        valtext2 = "";
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
                        }
                        if (valtext != "" || valtext2 != "")
                        {
                            target_width = target_scale * _statusWeaponStyleBitmap.Width;
                            target_height = target_scale * _statusWeaponStyleBitmap.Height;
                            statusDest = new SKRect(curx, cury, curx + target_width, cury + target_height);
                            canvas.DrawBitmap(_statusWeaponStyleBitmap, statusDest, textPaint);
                            curx += target_width;
                            curx += innerspacing;
                            float print_width = 0;
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
                            curx += stdspacing;
                        }

                        /* Right aligned */
                        float turnsleft = canvaswidth - hmargin;

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
                            target_width = target_scale * _statusTurnsBitmap.Width;
                            target_height = target_scale * _statusTurnsBitmap.Height;
                            float print_width = textPaint.MeasureText(valtext);
                            curx = canvaswidth - hmargin - print_width - innerspacing - target_width;
                            turnsleft = curx;
                            statusDest = new SKRect(curx, cury, curx + target_width, cury + target_height);
                            canvas.DrawBitmap(_statusTurnsBitmap, statusDest, textPaint);
                            curx += target_width;
                            curx += innerspacing;
                            canvas.DrawText(valtext, curx, cury - textPaint.FontMetrics.Ascent, textPaint);
                            curx += print_width;
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
                            if (valtext.Substring(0, 1) == "\\" && valtext.Length > 11)
                                printtext = valtext.Substring(11);
                            else
                                printtext = valtext;

                            target_width = target_scale * _statusGoldBitmap.Width;
                            target_height = target_scale * _statusGoldBitmap.Height;
                            float print_width = textPaint.MeasureText(printtext);
                            curx = turnsleft - stdspacing - print_width - innerspacing - target_width;
                            statusDest = new SKRect(curx, cury, curx + target_width, cury + target_height);
                            canvas.DrawBitmap(_statusGoldBitmap, statusDest, textPaint);
                            curx += target_width;
                            curx += innerspacing;
                            canvas.DrawText(printtext, curx, cury - textPaint.FontMetrics.Ascent, textPaint);
                            curx += print_width;
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
                            canvas.DrawText(valtext, curx, cury - textPaint.FontMetrics.Ascent, textPaint);
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
                                int mglyph = (int)game_ui_tile_types.STATUS_MARKS + UITileOff;
                                int mtile = Glyph2Tile[mglyph];
                                int sheet_idx = TileSheetIdx(mtile);
                                int tile_x = TileSheetX(mtile);
                                int tile_y = TileSheetY(mtile);
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

                                        canvas.DrawBitmap(TileMap[sheet_idx], source_rt, target_rt);

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
                                int mglyph = (int)game_ui_tile_types.CONDITION_MARKS + UITileOff;
                                int mtile = Glyph2Tile[mglyph];
                                int sheet_idx = TileSheetIdx(mtile);
                                int tile_x = TileSheetX(mtile);
                                int tile_y = TileSheetY(mtile);
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

                                        canvas.DrawBitmap(TileMap[sheet_idx], source_rt, target_rt);

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
                                            int mglyph = (propidx - 1) / GHConstants.BUFFS_PER_TILE + BuffTileOff;
                                            int mtile = Glyph2Tile[mglyph];
                                            int sheet_idx = TileSheetIdx(mtile);
                                            int tile_x = TileSheetX(mtile);
                                            int tile_y = TileSheetY(mtile);

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

                                            canvas.DrawBitmap(TileMap[sheet_idx], source_rt, target_rt);

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
                                    SKColor dotcolor = ClientUtils.NHColor2SKColorCore(i, 0, true);
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
                            if (valtext.Substring(0, 3) == "DL:" && valtext.Length > 3)
                                printtext = valtext.Substring(3);
                            else
                                printtext = valtext;

                            target_width = target_scale * _statusDungeonLevelBitmap.Width;
                            target_height = target_scale * _statusDungeonLevelBitmap.Height;
                            float print_width = textPaint.MeasureText(printtext);
                            curx = canvaswidth - hmargin - print_width - innerspacing - target_width;
                            statusDest = new SKRect(curx, cury, curx + target_width, cury + target_height);
                            canvas.DrawBitmap(_statusDungeonLevelBitmap, statusDest, textPaint);
                            curx += target_width;
                            curx += innerspacing;
                            canvas.DrawText(printtext, curx, cury - textPaint.FontMetrics.Ascent, textPaint);
                            curx += print_width;
                        }

                        /* Pets */
                        if (ShowPets)
                        {
                            lock (_petDataLock)
                            {
                                textPaint.Color = SKColors.White;
                                textPaint.Typeface = App.LatoRegular;
                                textPaint.TextSize = 36;
                                float pet_target_height = inverse_canvas_scale * (float)(ESCButton.Height + lAbilitiesButton.Width) / 2;
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
                                float pet_target_width = pet_target_height; // inverse_canvas_scale * (float)ESCButton.Width;

                                SKRect menubuttonrect = GetViewScreenRect(GameMenuButton);
                                float menu_button_left = menubuttonrect.Left;
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
                                            gis.DrawOnCanvas(canvas);
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
                                        canvas.DrawRect(petHPFill, textPaint);
                                        SKRect petHPNonFill = new SKRect(tx + pet_target_width * petpct, curpety, tx + pet_target_width, curpety + petHPHeight);
                                        textPaint.Color = SKColors.Gray.WithAlpha(144);
                                        canvas.DrawRect(petHPNonFill, textPaint);
                                        textPaint.Color = SKColors.Black.WithAlpha(144);
                                        textPaint.Style = SKPaintStyle.Stroke;
                                        textPaint.StrokeWidth = 2;
                                        canvas.DrawRect(petHPRect, textPaint);

                                        curpety += barpadding - textPaint.FontMetrics.Ascent;
                                        textPaint.Style = SKPaintStyle.Fill;
                                        textPaint.Color = SKColors.White;
                                        canvas.DrawText(mi.mhp + "(" + mi.mhpmax + ")", tx + pet_target_width / 2, curpety, textPaint);

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
                                                int mglyph = (int)game_ui_tile_types.STATUS_MARKS + UITileOff;
                                                int mtile = Glyph2Tile[mglyph];
                                                int sheet_idx = TileSheetIdx(mtile);
                                                int tile_x = TileSheetX(mtile);
                                                int tile_y = TileSheetY(mtile);
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

                                                        canvas.DrawBitmap(TileMap[sheet_idx], source_rt, target_rt);

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
                                                int mglyph = (int)game_ui_tile_types.CONDITION_MARKS + UITileOff;
                                                int mtile = Glyph2Tile[mglyph];
                                                int sheet_idx = TileSheetIdx(mtile);
                                                int tile_x = TileSheetX(mtile);
                                                int tile_y = TileSheetY(mtile);
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

                                                        canvas.DrawBitmap(TileMap[sheet_idx], source_rt, target_rt);

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
                                                            int mglyph = (propidx - 1) / GHConstants.BUFFS_PER_TILE + BuffTileOff;
                                                            int mtile = Glyph2Tile[mglyph];
                                                            int sheet_idx = TileSheetIdx(mtile);
                                                            int tile_x = TileSheetX(mtile);
                                                            int tile_y = TileSheetY(mtile);

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

                                                            canvas.DrawBitmap(TileMap[sheet_idx], source_rt, target_rt);

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

                    lock (_rectLock)
                    {
                        HealthRect = new SKRect();
                        ManaRect = new SKRect();

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
                            DrawOrb(canvas, textPaint, orbBorderDest, SKColors.Blue, valtext, maxtext, orbfillpercentage, ShowMaxManaInOrb);
                            lastdrawnrecty = orbBorderDest.Bottom;
                        }

                        if (skillbuttonok)
                        {
                            SKRect skillDest = new SKRect(tx, lastdrawnrecty + 15.0f, tx + orbbordersize, lastdrawnrecty + 15.0f + orbbordersize);
                            SkillRect = skillDest;
                            textPaint.Color = SKColors.White;
                            textPaint.Typeface = App.LatoRegular;
                            textPaint.TextSize = 9.5f * skillDest.Width / 50.0f;
                            textPaint.TextAlign = SKTextAlign.Center;
                            canvas.DrawBitmap(_skillBitmap, skillDest, textPaint);
                            float text_x = (skillDest.Left + skillDest.Right) / 2;
                            float text_y = skillDest.Bottom - textPaint.FontMetrics.Ascent;
                            canvas.DrawText("Skills", text_x, text_y, textPaint);
                            textPaint.TextAlign = SKTextAlign.Left;
                        }
                    }
                }

                /* Number Pad and Direction Arrows */
                _canvasButtonRect.Right = canvaswidth * (float)(0.8);
                _canvasButtonRect.Left = canvaswidth * (float)(0.2);

                if (_showDirections || (MapWalkMode && WalkArrows))
                {
                    SKRect targetrect;
                    float buttonsize = _showDirections ? GHConstants.ArrowButtonSize : GHConstants.MoveArrowButtonSize;
                    SKColor oldcolor = textPaint.Color;
                    textPaint.Color = _showDirections ? textPaint.Color.WithAlpha(170) : textPaint.Color.WithAlpha(85);

                    for (int i = 0; i < 9; i++)
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
                        canvas.DrawBitmap(_arrowBitmap[i], targetrect, textPaint);
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
                        textPaint.Typeface = App.DiabloTypeface;
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
                        for (int i = 0; i <= 9; i++)
                        {
                            switch (i)
                            {
                                case 0:
                                    str = "4";
                                    tx = _canvasButtonRect.Left + _canvasButtonRect.Width * (buttonsize / 2) - textPaint.FontMetrics.AverageCharacterWidth / 2;
                                    ty = _canvasButtonRect.Top + _canvasButtonRect.Height / 2 + textPaint.FontMetrics.Descent;
                                    break;
                                case 1:
                                    str = "8";
                                    tx = _canvasButtonRect.Left + _canvasButtonRect.Width / 2 - textPaint.FontMetrics.AverageCharacterWidth / 2;
                                    ty = _canvasButtonRect.Top + _canvasButtonRect.Height * (buttonsize / 2) + textPaint.FontMetrics.Descent;
                                    break;
                                case 2:
                                    str = "6";
                                    tx = _canvasButtonRect.Left + _canvasButtonRect.Width * (1.0f - buttonsize / 2) - textPaint.FontMetrics.AverageCharacterWidth / 2;
                                    ty = _canvasButtonRect.Top + _canvasButtonRect.Height / 2 + textPaint.FontMetrics.Descent;
                                    break;
                                case 3:
                                    str = "2";
                                    tx = _canvasButtonRect.Left + _canvasButtonRect.Width / 2 - textPaint.FontMetrics.AverageCharacterWidth / 2;
                                    ty = _canvasButtonRect.Top + _canvasButtonRect.Height * (1.0f - buttonsize / 2) + textPaint.FontMetrics.Descent;
                                    break;
                                case 4:
                                    str = "7";
                                    tx = _canvasButtonRect.Left + _canvasButtonRect.Width * (buttonsize / 2) - textPaint.FontMetrics.AverageCharacterWidth / 2;
                                    ty = _canvasButtonRect.Top + _canvasButtonRect.Height * (buttonsize / 2) + textPaint.FontMetrics.Descent;
                                    break;
                                case 5:
                                    str = "5";
                                    tx = _canvasButtonRect.Left + _canvasButtonRect.Width / 2 - textPaint.FontMetrics.AverageCharacterWidth / 2;
                                    ty = _canvasButtonRect.Top + _canvasButtonRect.Height / 2 + textPaint.FontMetrics.Descent;
                                    break;
                                case 6:
                                    str = "9";
                                    tx = _canvasButtonRect.Left + _canvasButtonRect.Width * (1.0f - buttonsize / 2) - textPaint.FontMetrics.AverageCharacterWidth / 2;
                                    ty = _canvasButtonRect.Top + _canvasButtonRect.Height * (buttonsize / 2) + textPaint.FontMetrics.Descent;
                                    break;
                                case 7:
                                    str = "3";
                                    tx = _canvasButtonRect.Left + _canvasButtonRect.Width * (1.0f - buttonsize / 2) - textPaint.FontMetrics.AverageCharacterWidth / 2;
                                    ty = _canvasButtonRect.Top + _canvasButtonRect.Height * (1.0f - buttonsize / 2) + textPaint.FontMetrics.Descent;
                                    break;
                                case 8:
                                    str = "1";
                                    tx = _canvasButtonRect.Left + _canvasButtonRect.Width * (buttonsize / 2) - textPaint.FontMetrics.AverageCharacterWidth / 2;
                                    ty = _canvasButtonRect.Top + _canvasButtonRect.Height * (1.0f - buttonsize / 2) + textPaint.FontMetrics.Descent;
                                    break;
                                case 9:
                                    str = "0";
                                    tx = 0 + _canvasButtonRect.Left / 2 - textPaint.FontMetrics.AverageCharacterWidth / 2;
                                    //ty = _canvasButtonRect.Top + _canvasButtonRect.Height * (buttonsize / 2) + textPaint.FontMetrics.Descent;
                                    ty = _canvasButtonRect.Top + _canvasButtonRect.Height * (1.0f - buttonsize / 2) + textPaint.FontMetrics.Descent;
                                    textPaint.TextSize = Math.Max(10.0f, textPaint.TextSize * Math.Min(1.0f, _canvasButtonRect.Left / (_canvasButtonRect.Width * buttonsize)));
                                    break;
                            }
                            canvas.DrawText(str, tx + offset, ty + offset, textPaint);
                        }
                        textPaint.Color = oldcolor;
                        textPaint.MaskFilter = oldfilter;
                    }
                    textPaint.Style = SKPaintStyle.Fill;
                }


                /* Status Screen */
                if (ShowExtendedStatusBar)
                {
                    textPaint.Style = SKPaintStyle.Fill;
                    textPaint.Color = SKColors.Black.WithAlpha(200);
                    canvas.DrawRect(0, 0, canvaswidth, canvasheight, textPaint);
                    textPaint.Color = SKColors.White;

                    float box_left = canvaswidth < canvasheight ? 1.25f * inverse_canvas_scale * (float)ESCButton.Width :
                        2.25f * inverse_canvas_scale * (float)ESCButton.Width;
                    float box_right = canvaswidth - box_left;
                    if (box_right < box_left)
                        box_right = box_left;
                    float box_top = canvaswidth < canvasheight ? GetStatusBarSkiaHeight() + 1.25f * inverse_canvas_scale * (float)ESCButton.Height :
                        GetStatusBarSkiaHeight() + 0.25f * inverse_canvas_scale * (float)ESCButton.Height;
                    float box_bottom = canvasheight - 1.25f * inverse_canvas_scale * (float)ButtonRowStack.Height;
                    if (box_bottom < box_top)
                        box_bottom = box_top;

                    /* Window Background */
                    textPaint.Color = SKColors.Black;
                    SKRect bkgrect = new SKRect(box_left, box_top, box_right, box_bottom);
                    canvas.DrawBitmap(App.ScrollBitmap, bkgrect, textPaint);

                    float youmargin = Math.Min((box_right - box_left), (box_bottom - box_top)) / 14;
                    float yousize = Math.Min((box_right - box_left), (box_bottom - box_top)) / 8;
                    float youtouchsize = Math.Min((box_right - box_left), (box_bottom - box_top)) / 6;
                    float youtouchmargin = Math.Max(0, (youtouchsize - yousize) / 2);
                    SKRect urect = new SKRect(box_right - youmargin - yousize, box_top + youmargin, box_right - youmargin, box_top + youmargin + yousize);
                    SKRect utouchrect = new SKRect(urect.Left - youtouchmargin, urect.Top - youtouchmargin, urect.Right + youtouchmargin, urect.Bottom + youtouchmargin);
                    canvas.DrawBitmap(App.YouBitmap, urect, textPaint);
                    YouRect = utouchrect;

                    textPaint.Style = SKPaintStyle.Fill;
                    textPaint.Typeface = App.UnderwoodTypeface;
                    textPaint.Color = SKColors.Black;
                    textPaint.TextSize = 36;

                    float twidth = textPaint.MeasureText("Strength");
                    float theight = textPaint.FontSpacing;
                    float tscale =  Math.Max(0.1f, Math.Min((bkgrect.Width * (1 - 2f / 12.6f) / 4) / twidth, (bkgrect.Height * (1 - 2f / 8.5f) / 18) / theight));
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
                        textPaint.Typeface = App.ImmortalTypeface;
                        textPaint.TextSize = basefontsize * 1.1f;
                        tx = (bkgrect.Left + bkgrect.Right) / 2;
                        textPaint.TextAlign = SKTextAlign.Center;
                        canvas.DrawText(valtext, tx, ty, textPaint);
                        textPaint.TextAlign = SKTextAlign.Left;
                        textPaint.Typeface = App.UnderwoodTypeface;
                        textPaint.TextSize = basefontsize;
                        tx = bkgrect.Left + bkgrect.Width / 12.6f;
                        ty += textPaint.FontSpacing;
                        ty += textPaint.FontSpacing * 0.5f;
                    }

                    using(new SKAutoCanvasRestore(canvas, true))
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
                                textPaint.Color = ClientUtils.NHColor2SKColorCore(valcolor, 0, true);
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
                            icon_width = icon_height * (float)_statusXPLevelBitmap.Width / (float)_statusXPLevelBitmap.Height;
                            icon_tx = icon_base_left + (icon_max_width - icon_width) / 2f;
                            icon_ty = ty + textPaint.FontMetrics.Ascent - textPaint.FontMetrics.Descent / 2 + (textPaint.FontSpacing - icon_height) / 2;
                            icon_rect = new SKRect(icon_tx, icon_ty, icon_tx + icon_width, icon_ty + icon_height);
                            canvas.DrawBitmap(_statusXPLevelBitmap, icon_rect);
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
                            icon_width = icon_height * (float)_statusHDBitmap.Width / (float)_statusHDBitmap.Height;
                            icon_tx = icon_base_left + (icon_max_width - icon_width) / 2f;
                            icon_ty = ty + textPaint.FontMetrics.Ascent - textPaint.FontMetrics.Descent / 2 + (textPaint.FontSpacing - icon_height) / 2;
                            icon_rect = new SKRect(icon_tx, icon_ty, icon_tx + icon_width, icon_ty + icon_height);
                            canvas.DrawBitmap(_statusHDBitmap, icon_rect);
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
                            icon_width = icon_height * (float)_statusACBitmap.Width / (float)_statusACBitmap.Height;
                            icon_tx = icon_base_left + (icon_max_width - icon_width) / 2f;
                            icon_ty = ty + textPaint.FontMetrics.Ascent - textPaint.FontMetrics.Descent / 2 + (textPaint.FontSpacing - icon_height) / 2;
                            icon_rect = new SKRect(icon_tx, icon_ty, icon_tx + icon_width, icon_ty + icon_height);
                            canvas.DrawBitmap(_statusACBitmap, icon_rect);
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
                            icon_width = icon_height * (float)_statusMCBitmap.Width / (float)_statusMCBitmap.Height;
                            icon_tx = icon_base_left + (icon_max_width - icon_width) / 2f;
                            icon_ty = ty + textPaint.FontMetrics.Ascent - textPaint.FontMetrics.Descent / 2 + (textPaint.FontSpacing - icon_height) / 2;
                            icon_rect = new SKRect(icon_tx, icon_ty, icon_tx + icon_width, icon_ty + icon_height);
                            canvas.DrawBitmap(_statusMCBitmap, icon_rect);
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
                            icon_width = icon_height * (float)_statusMoveBitmap.Width / (float)_statusMoveBitmap.Height;
                            icon_tx = icon_base_left + (icon_max_width - icon_width) / 2f;
                            icon_ty = ty + textPaint.FontMetrics.Ascent - textPaint.FontMetrics.Descent / 2 + (textPaint.FontSpacing - icon_height) / 2;
                            icon_rect = new SKRect(icon_tx, icon_ty, icon_tx + icon_width, icon_ty + icon_height);
                            canvas.DrawBitmap(_statusMoveBitmap, icon_rect);
                            ty += textPaint.FontSpacing;
                        }

                        valtext = "";
                        valtext2 = "";
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
                        }
                        if (valtext != "")
                        {
                            lock (_mapOffsetLock)
                            {
                                _statusLargestBottom = ty + textPaint.FontMetrics.Descent;
                            }
                            canvas.DrawText("Weapon style:", tx, ty, textPaint);
                            string printtext = valtext2 != "" ? valtext + "/" + valtext2 : valtext;
                            canvas.DrawText(printtext, tx + indentation, ty, textPaint);
                            icon_width = icon_height * (float)_statusWeaponStyleBitmap.Width / (float)_statusWeaponStyleBitmap.Height;
                            icon_tx = icon_base_left + (icon_max_width - icon_width) / 2f;
                            icon_ty = ty + textPaint.FontMetrics.Ascent - textPaint.FontMetrics.Descent / 2 + (textPaint.FontSpacing - icon_height) / 2;
                            icon_rect = new SKRect(icon_tx, icon_ty, icon_tx + icon_width, icon_ty + icon_height);
                            canvas.DrawBitmap(_statusWeaponStyleBitmap, icon_rect);
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
                            if (valtext.Substring(0, 1) == "\\" && valtext.Length > 11)
                                printtext = valtext.Substring(11);
                            else
                                printtext = valtext;

                            canvas.DrawText("Gold:", tx, ty, textPaint);
                            canvas.DrawText(printtext, tx + indentation, ty, textPaint);
                            icon_width = icon_height * (float)_statusGoldBitmap.Width / (float)_statusGoldBitmap.Height;
                            icon_tx = icon_base_left + (icon_max_width - icon_width) / 2f;
                            icon_ty = ty + textPaint.FontMetrics.Ascent - textPaint.FontMetrics.Descent / 2 + (textPaint.FontSpacing - icon_height) / 2;
                            icon_rect = new SKRect(icon_tx, icon_ty, icon_tx + icon_width, icon_ty + icon_height);
                            canvas.DrawBitmap(_statusGoldBitmap, icon_rect);
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
                            lock(_mapOffsetLock)
                            {
                                _statusLargestBottom = ty + textPaint.FontMetrics.Descent;
                            }
                            canvas.DrawText("Turns:", tx, ty, textPaint);
                            canvas.DrawText(valtext, tx + indentation, ty, textPaint);
                            icon_width = icon_height * (float)_statusTurnsBitmap.Width / (float)_statusTurnsBitmap.Height;
                            icon_tx = icon_base_left + (icon_max_width - icon_width) / 2f;
                            icon_ty = ty + textPaint.FontMetrics.Ascent - textPaint.FontMetrics.Descent / 2 + (textPaint.FontSpacing - icon_height) / 2;
                            icon_rect = new SKRect(icon_tx, icon_ty, icon_tx + icon_width, icon_ty + icon_height);
                            canvas.DrawBitmap(_statusTurnsBitmap, icon_rect);
                            ty += textPaint.FontSpacing;
                        }

                        ty += textPaint.FontSpacing * 0.5f;

                        /* Condition, status and buff marks */
                        if (bkgrect.Width - bkgrect.Width * 2f / 12.6f >= indentation * 2.5f)
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
                            int mglyph = (int)game_ui_tile_types.STATUS_MARKS + UITileOff;
                            int mtile = Glyph2Tile[mglyph];
                            int sheet_idx = TileSheetIdx(mtile);
                            int tile_x = TileSheetX(mtile);
                            int tile_y = TileSheetY(mtile);
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
                            int mglyph = (int)game_ui_tile_types.CONDITION_MARKS + UITileOff;
                            int mtile = Glyph2Tile[mglyph];
                            int sheet_idx = TileSheetIdx(mtile);
                            int tile_x = TileSheetX(mtile);
                            int tile_y = TileSheetY(mtile);
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
                                        string propname = App.GnollHackService.GetPropertyName(propidx);
                                        if (propname != null && propname.Length > 0)
                                            propname = propname[0].ToString().ToUpper() + (propname.Length == 1 ? "" : propname.Substring(1));

                                        int mglyph = (propidx - 1) / GHConstants.BUFFS_PER_TILE + BuffTileOff;
                                        int mtile = Glyph2Tile[mglyph];
                                        int sheet_idx = TileSheetIdx(mtile);
                                        int tile_x = TileSheetX(mtile);
                                        int tile_y = TileSheetY(mtile);

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

                }

                if (ShowWaitIcon)
                {
                    SKRect targetrect;
                    float size = canvaswidth / 5.0f;
                    targetrect = new SKRect(canvaswidth / 2 - size / 2, canvasheight / 2 - size / 2, canvaswidth / 2 + size / 2, canvasheight / 2 + size / 2);
                    canvas.DrawBitmap(_logoBitmap, targetrect);
                }
            }

        }

        bool IsNoWallEndAutoDraw(int x, int y)
        {
            if (!GHUtils.isok(x, y))
                return true;

            if (_mapData[x, y].Layers.layer_gui_glyphs[(int)layer_types.LAYER_FLOOR] == UnexploredGlyph
                || _mapData[x, y].Layers.layer_gui_glyphs[(int)layer_types.LAYER_FLOOR] == NoGlyph)
                return true;

            if ((_mapData[x, y].Layers.layer_flags & (ulong)LayerFlags.LFLAGS_NO_WALL_END_AUTODRAW) != 0)
                return true;

            return false;
        }

        public void DrawAutoDraw(int autodraw, SKCanvas canvas, SKPaint paint, ObjectDataItem otmp_round,
            int layer_idx, int mapx, int mapy,
            bool tileflag_halfsize, bool tileflag_normalobjmissile, bool tileflag_fullsizeditem,
            float tx, float ty, float width, float height,
            float scale, float targetscale, float scaled_x_padding, float scaled_y_padding, float scaled_tile_height,
            bool is_inventory)
        {
            /******************/
            /* AUTODRAW START */
            /******************/
            if (_autodraws != null)
            {
                float opaqueness = 1;
                int sheet_idx = 0;

                if (false && _autodraws[autodraw].draw_type == (int)autodraw_drawing_types.AUTODRAW_DRAW_REPLACE_WALL_ENDS)
                { /* Deactivated for the time being */
                    for (byte dir = 0; dir < 4; dir++)
                    {
                        byte dir_bit = (byte)(1 << dir);
                        if ((_autodraws[autodraw].flags & dir_bit) != 0)
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
                                    int source_glyph = _autodraws[autodraw].source_glyph;
                                    int atile = Glyph2Tile[source_glyph];
                                    int a_sheet_idx = TileSheetIdx(atile);
                                    int at_x = TileSheetX(atile);
                                    int at_y = TileSheetY(atile);

                                    SKRect source_rt = new SKRect();
                                    switch (dir)
                                    {
                                        case 0: /* left */
                                            if (IsNoWallEndAutoDraw(corner_x[corner], corner_y[corner])) // NO_WALL_END_AUTODRAW(corner_x[corner], corner_y[corner]))
                                            {
                                                source_glyph = _autodraws[autodraw].source_glyph2; /* S_vwall */
                                                atile = Glyph2Tile[source_glyph];
                                                a_sheet_idx = TileSheetIdx(atile);
                                                at_x = TileSheetX(atile);
                                                at_y = TileSheetY(atile);
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
                                                source_glyph = _autodraws[autodraw].source_glyph2; /* S_vwall */
                                                atile = Glyph2Tile[source_glyph];
                                                a_sheet_idx = TileSheetIdx(atile);
                                                at_x = TileSheetX(atile);
                                                at_y = TileSheetY(atile);
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
                                                source_glyph = _autodraws[autodraw].source_glyph3; /* S_hwall */
                                                atile = Glyph2Tile[source_glyph];
                                                a_sheet_idx = TileSheetIdx(atile);
                                                at_x = TileSheetX(atile);
                                                at_y = TileSheetY(atile);
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
                                                source_glyph = _autodraws[autodraw].source_glyph3; /* S_hwall */
                                                atile = Glyph2Tile[source_glyph];
                                                a_sheet_idx = TileSheetIdx(atile);
                                                at_x = TileSheetX(atile);
                                                at_y = TileSheetY(atile);
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
                                    canvas.DrawBitmap(TileMap[a_sheet_idx], source_rt, target_rt, paint);
                                }
                            }
                        }
                    }
                }
                else if (_autodraws[autodraw].draw_type == (int)autodraw_drawing_types.AUTODRAW_DRAW_LONG_WORM)
                {
                    /* Long worm here */

                    int source_glyph_seg_end = _autodraws[autodraw].source_glyph;
                    int source_glyph_seg_dir_out = _autodraws[autodraw].source_glyph2;
                    int source_glyph_seg_dir_in = _autodraws[autodraw].source_glyph2 + 4;
                    int source_glyph_seg_layer = _autodraws[autodraw].source_glyph3;
                    int drawing_tail = _autodraws[autodraw].flags;
                    int wdir_out = _mapData[mapx, mapy].Layers.wsegdir;
                    int wdir_in = _mapData[mapx, mapy].Layers.reverse_prev_wsegdir;
                    bool is_head = (_mapData[mapx, mapy].Layers.monster_flags & (ulong)LayerMonsterFlags.LMFLAGS_WORM_HEAD) != 0;
                    bool is_tailend = (_mapData[mapx, mapy].Layers.monster_flags & (ulong)LayerMonsterFlags.LMFLAGS_WORM_TAILEND) != 0;
                    for (int wlayer = 0; wlayer < 5; wlayer++)
                    {
                        int source_glyph = NoGlyph;
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
                                source_glyph = is_tailend ? NoGlyph : is_head ? source_glyph_seg_end : source_glyph_seg_dir_in;
                                break;
                            case 3:
                                source_glyph = is_tailend ? source_glyph_seg_end : is_head ? NoGlyph : source_glyph_seg_dir_out;
                                break;
                            default:
                                break;
                        }

                        if (source_glyph != NoGlyph)
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

                            int atile = Glyph2Tile[source_glyph];
                            int a_sheet_idx = TileSheetIdx(atile);
                            int at_x = TileSheetX(atile);
                            int at_y = TileSheetY(atile);

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
                else if (_autodraws[autodraw].draw_type == (int)autodraw_drawing_types.AUTODRAW_DRAW_BOOKSHELF_CONTENTS && otmp_round != null && otmp_round.ContainedObjs != null)
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

                            int source_glyph = _autodraws[autodraw].source_glyph;
                            int atile = Glyph2Tile[source_glyph];
                            int a_sheet_idx = TileSheetIdx(atile);
                            int at_x = TileSheetX(atile);
                            int at_y = TileSheetY(atile);

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
                else if (_autodraws[autodraw].draw_type == (int)autodraw_drawing_types.AUTODRAW_DRAW_WEAPON_RACK_CONTENTS && otmp_round != null && otmp_round.ContainedObjs != null)
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
                        if (source_glyph <= 0 || source_glyph == NoGlyph)
                            continue;
                        bool has_floor_tile = (GlyphTileFlags[source_glyph] & (byte)glyph_tile_flags.GLYPH_TILE_FLAG_HAS_FLOOR_TILE) != 0; // artidx > 0 ? has_artifact_floor_tile(artidx) : has_obj_floor_tile(contained_obj);
                        bool fullsizeditem = (GlyphTileFlags[source_glyph] & (byte)glyph_tile_flags.GLYPH_TILE_FLAG_FULL_SIZED_ITEM) != 0;
                        int cobj_height = contained_obj.OtypData.tile_height; // artidx ? artilist[artidx].tile_floor_height : OBJ_TILE_HEIGHT(contained_obj->otyp);
                        int artidx = contained_obj.ObjData.oartifact;
                        float dest_x = 0, dest_y = 0;
                        int src_x = 0, src_y = fullsizeditem || has_floor_tile ? 0 : GHConstants.TileHeight / 2;
                        int item_width = has_floor_tile ? GHConstants.TileHeight / 2 : cobj_height > 0 ? cobj_height : GHConstants.TileHeight / 2;
                        int item_height = has_floor_tile ? GHConstants.TileWidth : (item_width * GHConstants.TileWidth) / (GHConstants.TileHeight / 2);
                        int padding = (GHConstants.TileHeight / 2 - item_width) / 2;
                        int vertical_padding = (GHConstants.TileWidth - item_height) / 2;
                        if (contained_obj.ObjData.oclass != (int)obj_class_types.WEAPON_CLASS)
                            continue;

                        int item_xpos = cnt / 2 * rack_item_spacing;
                        if (item_xpos >= rack_width / 2)
                            break;

                        dest_y = (y_to_rack_top + vertical_padding) * scale * targetscale;
                        dest_x = (cnt % 2 == 0 ? rack_start + item_xpos + padding : GHConstants.TileWidth - item_width - rack_start - item_xpos - padding) * scale * targetscale;

                        int atile = Glyph2Tile[source_glyph];
                        int a_sheet_idx = TileSheetIdx(atile);
                        int at_x = TileSheetX(atile);
                        int at_y = TileSheetY(atile);

                        SKRect source_rt = new SKRect();
                        source_rt.Left = at_x + src_x;
                        source_rt.Right = source_rt.Left + GHConstants.TileWidth;
                        source_rt.Top = at_y + src_y;
                        source_rt.Bottom = source_rt.Top + (fullsizeditem ? GHConstants.TileHeight : GHConstants.TileHeight / 2);

                        float original_width = source_rt.Right - source_rt.Left;
                        float original_height = source_rt.Bottom - source_rt.Top;
                        float rotated_width = original_height;
                        float rotated_height = original_width;

                        float content_scale = fullsizeditem || has_floor_tile ? 1.0f : item_width / 48.0f;

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
                else if (_autodraws[autodraw].draw_type == (int)autodraw_drawing_types.AUTODRAW_DRAW_CANDELABRUM_CANDLES && otmp_round != null)
                {
                    float y_start = scaled_y_padding;
                    float x_start = scaled_x_padding;
                    int x_padding = 13;
                    int item_width = 6;
                    int item_height = 13;
                    int src_unlit_x = 0;
                    int src_unlit_y = 10;
                    int src_lit_x = 6 * (1 + (int)_autodraws[autodraw].flags);
                    int src_lit_y = 10;
                    int cnt = 0;

                    for (int cidx = 0; cidx < Math.Min((short)7, otmp_round.ObjData.special_quality); cidx++)
                    {
                        int src_x = 0, src_y = 0;
                        float dest_x = 0, dest_y = 0;
                        if (otmp_round.LampLit)
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

                        int source_glyph = _autodraws[autodraw].source_glyph;
                        int atile = Glyph2Tile[source_glyph];
                        int a_sheet_idx = TileSheetIdx(atile);
                        int at_x = TileSheetX(atile);
                        int at_y = TileSheetY(atile);

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
                else if (_autodraws[autodraw].draw_type == (int)autodraw_drawing_types.AUTODRAW_DRAW_LARGE_FIVE_BRANCHED_CANDELABRUM_CANDLES && otmp_round != null)
                {
                    float y_start = scaled_y_padding;
                    float x_start = scaled_x_padding;
                    int item_width = 9;
                    int item_height = 31;
                    int src_unlit_x = 0;
                    int src_unlit_y = 0;
                    int src_lit_x = 9 * (1 + (int)_autodraws[autodraw].flags);
                    int src_lit_y = 0;
                    int cnt = 0;

                    for (int cidx = 0; cidx < Math.Min((short)otmp_round.OtypData.special_quality, otmp_round.ObjData.special_quality); cidx++)
                    {
                        int src_x = 0, src_y = 0;
                        float dest_x = 0, dest_y = 0;
                        if (otmp_round.LampLit)
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

                        int source_glyph = _autodraws[autodraw].source_glyph;
                        int atile = Glyph2Tile[source_glyph];
                        int a_sheet_idx = TileSheetIdx(atile);
                        int at_x = TileSheetX(atile);
                        int at_y = TileSheetY(atile);

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

                /*
                 * AUTODRAW END
                 */

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

                        int source_glyph = (int)game_ui_tile_types.ITEM_PROPERTY_MARKS + UITileOff;
                        int atile = Glyph2Tile[source_glyph];
                        int a_sheet_idx = TileSheetIdx(atile);
                        int at_x = TileSheetX(atile);
                        int at_y = TileSheetY(atile);

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

                        canvas.DrawBitmap(TileMap[a_sheet_idx], source_rt, target_rt);

                        cnt++;
                    }
                }
            }
        }


        public double CurrentPageWidth { get { return _currentPageWidth; } }
        public double CurrentPageHeight { get { return _currentPageHeight; } }

        private double _currentPageWidth = 0;
        private double _currentPageHeight = 0;

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            if (width != _currentPageWidth || height != _currentPageHeight)
            {
                _currentPageWidth = width;
                _currentPageHeight = height;

                if (TipView.IsVisible)
                    TipView.InvalidateSurface();

                GameMenuButton.SetSideSize(width, height);
                ESCButton.SetSideSize(width, height);
                ToggleAutoCenterModeButton.SetSideSize(width, height);
                LookModeButton.SetSideSize(width, height);
                ToggleTravelModeButton.SetSideSize(width, height);
                ToggleZoomMiniButton.SetSideSize(width, height);
                ToggleZoomAlternateButton.SetSideSize(width, height);

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

                LabeledImageButton firstchild = (LabeledImageButton)UpperCmdGrid.Children[0];
                UpperCmdGrid.HeightRequest = firstchild.GridHeight;
                LowerCmdGrid.HeightRequest = firstchild.GridHeight;

                MenuHeaderLabel.Margin = ClientUtils.GetHeaderMarginWithBorder(MenuBackground.BorderStyle, width, height);
                MenuCloseGrid.Margin = ClientUtils.GetFooterMarginWithBorder(MenuBackground.BorderStyle, width, height);
                Thickness smallthick = ClientUtils.GetSmallBorderThickness(width, height, 1.5);
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
                    _messageOffsetY = 0;
                    _statusOffsetY = 0;
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
                }

                IsSizeAllocatedProcessed = true;
            }
        }

        public float GetStatusBarSkiaHeight()
        {
            float statusbarheight;
            using (SKPaint textPaint = new SKPaint())
            {
                textPaint.Typeface = App.LatoRegular;
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

        protected void ConnectToServer()
        {
            if (App.AuthenticationCookie == null)
            {
                throw new Exception("AuthenticationCookie is null");
            }

            if (App.SelectedServer == null)
            {
                throw new Exception("SelectedServer is null");
            }

            _connection = new HubConnectionBuilder().WithUrl(App.SelectedServer.Url + "gnollhack", options =>
            {
                options.Cookies.Add(App.AuthenticationCookie);
            }).Build();

            if (_connection != null)
            {
                _connection_status = "Connection attempted";
            }
            else
            {
                _connection_status = "Connection attempt failed";
            }

            _connection.Closed += async (error) =>
            {
                _connection_status = "Connection closed";
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await _connection.StartAsync();
            };

            _connection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                _message = message;
            });

            _connection.On<int>("CalcResult", (result) =>
            {
                _result = result;
            });

            _connection.On<string, string>("LoginMessage", (user, message) =>
            {
                _message2 = message;
            });

            _connection.On<int>("AddNewGameResult", (result) =>
            {
                _message3 = "New Game Added: " + result;
                _clientGame = new ClientGame(this);
            });

            _connection.On<bool>("GameAliveResult", (result) =>
            {
                _message4 = "Game Alive: " + result.ToString();
            });
            _connection.On<int, int>("Client_ExitHack", (hash, status) =>
            {
                _message5 = "ExitHack: Hash: " + hash + ", Status: " + status;
            });
            _connection.On<int>("Client_PlayerSelection", (hash) =>
            {
                _message5 = "PlayerSelection: Hash: " + hash;
            });
            _connection.On<GHCommandFromServer>("CommandFromServer", (command) =>
            {
                _message5 = "CommandFromServer: " + command.CommandName + ", GUID: " + command.Id.ToString();
            });
            _connection.On<Guid, int>("ResponseFromClientResult", (guid, result) =>
            {
                _message5 = "ResponseFromClientResult: " + result + ", GUID: " + guid;
            });
        }

        protected async void LoginToServer()
        {
            try
            {
                await _connection.StartAsync();

                await _connection.InvokeAsync("SendMessage",
                    "user", "My message");

                await _connection.InvokeAsync("DoCalc");

                await _connection.InvokeAsync("AddNewServerGame");
            }
            catch
            {
                //Error
            }
        }

        private Dictionary<long, TouchEntry> TouchDictionary = new Dictionary<long, TouchEntry>();
        private object _mapOffsetLock = new object();
        public float _mapOffsetX = 0;
        public float _mapOffsetY = 0;
        public float _mapMiniOffsetX = 0;
        public float _mapMiniOffsetY = 0;
        public float _messageOffsetY = 0;
        public float _messageSmallestTop = 0;
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

        private void canvasView_Touch(object sender, SKTouchEventArgs e)
        {
            canvas_page_types page = 0;
            lock (_canvasPageLock)
            {
                page = _canvasPage;
            }

            switch (page)
            {
                case canvas_page_types.MainGamePage:
                    canvasView_Touch_MainPage(sender, e);
                    break;
                case canvas_page_types.MenuPage:
                    //MenuCanvas_Touch(sender, e);
                    break;
            }
        }

        private void canvasView_Touch_MainPage(object sender, SKTouchEventArgs e)
        {
            if (_clientGame != null)
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
                                            lock (_mapOffsetLock)
                                            {
                                                if (ShowExtendedStatusBar)
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
                                                else if (ForceAllMessages)
                                                {
                                                    if(diffY > 0)
                                                    {
                                                        if (_messageSmallestTop < 0)
                                                        {
                                                            _messageOffsetY += Math.Min(-_messageSmallestTop, diffY);
                                                        }
                                                    }
                                                    else
                                                        _messageOffsetY += diffY;

                                                    if (_messageOffsetY < 0)
                                                    {
                                                        _messageOffsetY = 0;
                                                    }
                                                }
                                                else
                                                {
                                                    if(ZoomMiniMode)
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

                                            if (ZoomMiniMode)
                                                MapFontMiniRelativeSize = newfontsize;
                                            else if (ZoomAlternateMode)
                                                MapFontAlternateSize = newfontsize;
                                            else
                                                MapFontSize = newfontsize;
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

                            if (_touchWithinSkillButton)
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
                                if (ClientGame.ResponseDictionary.TryGetValue(_clientGame, out queue))
                                {
                                    queue.Enqueue(new GHResponse(_clientGame, GHRequestType.SetPetMID, _touchWithinPet));
                                    queue.Enqueue(new GHResponse(_clientGame, GHRequestType.GetChar, (int)'{'));
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
                                        else if (ForceAllMessages)
                                            ToggleMessageNumberButton_Clicked(sender, e);
                                        else
                                            IssueNHCommandViaTouch(sender, e);
                                    }
                                }
                            }
                            if (TouchDictionary.ContainsKey(e.Id))
                                TouchDictionary.Remove(e.Id);
                            if (TouchDictionary.Count == 0)
                                _touchMoved = false;
                            e.Handled = true;
                        }
                        break;
                    case SKTouchAction.Cancelled:
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
                        offsetX -= (ClipX - (GHConstants.MapCols - 1) / 2) * _tileWidth;
                        offsetY -= (ClipY - GHConstants.MapRows / 2) * _tileHeight;
                    }
                }
            }
            lock (_mapOffsetLock)
            {
                offsetX += _mapOffsetX;
                offsetY += _mapOffsetY + _mapFontAscent;
            }

            if (_tileWidth > 0)
                x = (int)((e.Location.X - offsetX) / _tileWidth);
            if (_tileHeight > 0)
                y = (int)((e.Location.Y - offsetY) / _tileHeight);

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
                    if (ClientGame.ResponseDictionary.TryGetValue(_clientGame, out queue))
                    {
                        queue.Enqueue(new GHResponse(_clientGame, GHRequestType.Location, x, y, mod));
                    }
                }
            }
            else
            {
                float buttonsize = ShowNumberPad ? GHConstants.NumberButtonSize : _showDirections ? GHConstants.ArrowButtonSize : GHConstants.MoveArrowButtonSize;
                if (e.Location.X >= _canvasButtonRect.Left && e.Location.X <= _canvasButtonRect.Right && e.Location.Y >= _canvasButtonRect.Top && e.Location.Y <= _canvasButtonRect.Bottom)
                {
                    int resp = 0;
                    SKPoint RectLoc = new SKPoint(e.Location.X - _canvasButtonRect.Left, e.Location.Y - _canvasButtonRect.Top);

                    if (RectLoc.Y < _canvasButtonRect.Height * buttonsize && RectLoc.X < _canvasButtonRect.Width * buttonsize)
                        resp = -7;
                    else if (RectLoc.Y < _canvasButtonRect.Height * buttonsize && RectLoc.X > _canvasButtonRect.Width * (1.0f - buttonsize))
                        resp = -9;
                    else if (RectLoc.Y > _canvasButtonRect.Height * (1.0f - buttonsize) && RectLoc.X < _canvasButtonRect.Width * buttonsize)
                        resp = -1;
                    else if (RectLoc.Y > _canvasButtonRect.Height * (1.0f - buttonsize) && RectLoc.X > _canvasButtonRect.Width * (1.0f - buttonsize))
                        resp = -3;
                    else if (RectLoc.Y < _canvasButtonRect.Height * buttonsize)
                        resp = -8; //ch = "k";
                    else if (RectLoc.Y > _canvasButtonRect.Height * (1.0f - buttonsize))
                        resp = -2; // ch = "j";
                    else if (RectLoc.X < _canvasButtonRect.Width * buttonsize)
                        resp = -4; // ch = "h";
                    else if (RectLoc.X > _canvasButtonRect.Width * (1.0f - buttonsize))
                        resp = -6; // ch = "l";
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
                                    resp = -1 * (5 + dx - 3 * dy);
                                    if (resp == 5)
                                        resp = 46; /* '.', or self */
                                }
                                else
                                    return;
                            }
                            else
                                resp = ShowNumberPad ? -5 : 46; /* '.', or self */
                        }
                }

                    if (ShowNumberPad)
                        resp -= 10;

                    ConcurrentQueue<GHResponse> queue;
                    if (ClientGame.ResponseDictionary.TryGetValue(_clientGame, out queue))
                    {
                        queue.Enqueue(new GHResponse(_clientGame, GHRequestType.GetChar, resp));
                    }
                }
                else if (ShowNumberPad && e.Location.X < _canvasButtonRect.Left
                    && e.Location.Y >= _canvasButtonRect.Top + _canvasButtonRect.Height * (1.0f - buttonsize)
                    && e.Location.Y <= _canvasButtonRect.Top + _canvasButtonRect.Height)
                {
                    int resp = -10;
                    ConcurrentQueue<GHResponse> queue;
                    if (ClientGame.ResponseDictionary.TryGetValue(_clientGame, out queue))
                    {
                        queue.Enqueue(new GHResponse(_clientGame, GHRequestType.GetChar, resp));
                    }
                }
            }
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
                    /* Update counter value only if the monster just moved here, not, e.g. if it changes action in the same square,
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
                _mapData[x, y].Glyph = glyph;
                _mapData[x, y].BkGlyph = bkglyph;
                _mapData[x, y].Symbol = Char.ConvertFromUtf32(c);
                _mapData[x, y].Color = ClientUtils.NHColor2SKColor(color, (special & 0x00002000UL) != 0 ? (int)MenuItemAttributes.AltColors : 0);
                _mapData[x, y].Special = special;
                _mapData[x, y].Layers = layers;

                _mapData[x, y].NeedsUpdate = true;
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
                        _mapData[x, y].Glyph = UnexploredGlyph;
                        _mapData[x, y].BkGlyph = NoGlyph;
                        _mapData[x, y].Symbol = "";
                        _mapData[x, y].Color = SKColors.Black;// default(MapData);
                        _mapData[x, y].Special = 0;
                        _mapData[x, y].NeedsUpdate = true;
                        _mapData[x, y].GlyphPrintAnimationCounterValue = 0;
                        _mapData[x, y].GlyphPrintMainCounterValue = 0;
                        _mapData[x, y].GlyphObjectPrintAnimationCounterValue = 0;
                        _mapData[x, y].GlyphObjectPrintMainCounterValue = 0;

                        _mapData[x, y].Layers = new LayerInfo();
                        _mapData[x, y].Layers.layer_glyphs = new int[(int)layer_types.MAX_LAYERS];
                        _mapData[x, y].Layers.layer_gui_glyphs = new int[(int)layer_types.MAX_LAYERS];
                        _mapData[x, y].Layers.leash_mon_x = new sbyte[GHConstants.MaxLeashed + 1];
                        _mapData[x, y].Layers.leash_mon_y = new sbyte[GHConstants.MaxLeashed + 1];

                        _mapData[x, y].Layers.layer_glyphs[0] = UnexploredGlyph;
                        _mapData[x, y].Layers.layer_gui_glyphs[0] = UnexploredGlyph;
                        for (int i = 1; i < (int)layer_types.MAX_LAYERS; i++)
                        {
                            _mapData[x, y].Layers.layer_glyphs[i] = NoGlyph;
                            _mapData[x, y].Layers.layer_gui_glyphs[i] = NoGlyph;
                        }

                        _mapData[x, y].Layers.glyph = UnexploredGlyph;
                        _mapData[x, y].Layers.bkglyph = NoGlyph;
                    }
                }
            }
        }


        public void SetTargetClip(int x, int y, bool immediate_pan)
        {
            long curtimervalue = 0;
            long pantime = Math.Max(2, (long)Math.Ceiling((double)ClientUtils.GetMainCanvasAnimationFrequency(MapRefreshRate) / 8.0));
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
                        _originMapOffsetWithNewClipX = _mapOffsetX + (float)(x - ClipX) * _tileWidth;
                        _originMapOffsetWithNewClipY = _mapOffsetY + (float)(y - ClipY) * _tileHeight;
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
            lock (_objectDataLock)
            {
                if (_objectData[x, y] != null)
                {
                    bool is_memoryobj = (where == (int)obj_where_types.OBJ_HEROMEMORY);
                    bool is_drawn_in_front = (oflags & (ulong)objdata_flags.OBJDATA_FLAGS_DRAWN_IN_FRONT) != 0UL;
                    bool hallucinated = (oflags & (ulong)objdata_flags.OBJDATA_FLAGS_HALLUCINATION) != 0UL;
                    List<ObjectDataItem> ObjectList = is_memoryobj ? (is_drawn_in_front ? _objectData[x, y].CoverMemoryObjectList : _objectData[x, y].MemoryObjectList) : (is_drawn_in_front ? _objectData[x, y].CoverFloorObjectList : _objectData[x, y].FloorObjectList);

                    switch (cmdtype)
                    {
                        case 1: /* Clear */
                            if (ObjectList != null)
                                ObjectList.Clear();
                            break;
                        case 2: /* Add item */
                            if (ObjectList == null)
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

                                ObjectList = is_memoryobj ? (is_drawn_in_front ? _objectData[x, y].CoverMemoryObjectList : _objectData[x, y].MemoryObjectList) : (is_drawn_in_front ? _objectData[x, y].CoverFloorObjectList : _objectData[x, y].FloorObjectList);
                            }
                            ObjectList.Add(new ObjectDataItem(otmp, otypdata, hallucinated));
                            break;
                        case 3: /* Add container item to previous item */
                            if (ObjectList == null || ObjectList.Count == 0)
                                break;
                            if (ObjectList[ObjectList.Count - 1].ContainedObjs == null)
                                ObjectList[ObjectList.Count - 1].ContainedObjs = new List<ObjectDataItem>();
                            ObjectList[ObjectList.Count - 1].ContainedObjs.Add(new ObjectDataItem(otmp, otypdata, hallucinated));
                            break;
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

            if (_clientGame != null)
            {
                ConcurrentQueue<GHResponse> queue;
                if (ClientGame.ResponseDictionary.TryGetValue(_clientGame, out queue))
                {
                    queue.Enqueue(new GHResponse(_clientGame, GHRequestType.GetChar, resp));
                }
            }
        }

        private void InventoryButton_Clicked(object sender, EventArgs e)
        {
            App.DebugWriteRestart("Inventory");
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
            StartCommandCanvasAnimation();
        }

        private void YnButton_Clicked(object sender, EventArgs e)
        {
            LabeledImageButton ghb = (LabeledImageButton)sender;
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
            App.PlayButtonClickedSound();
            GenericButton_Clicked(sender, e, 27);
        }
        public void ToggleAutoCenterMode()
        {
            ToggleAutoCenterModeButton_Clicked(ToggleAutoCenterModeButton, new EventArgs());
        }

        private void ToggleAutoCenterModeButton_Clicked(object sender, EventArgs e)
        {
            App.PlayMenuSelectSound();
            MapNoClipMode = !MapNoClipMode;
            if (MapNoClipMode)
            {
                ToggleAutoCenterModeButton.ImgSourcePath = "resource://GnollHackClient.Assets.UI.stone-autocenter-off.png";
            }
            else
            {
                ToggleAutoCenterModeButton.ImgSourcePath = "resource://GnollHackClient.Assets.UI.stone-autocenter-on.png";
                if (sender != null && GHUtils.isok(_ux, _uy))
                {
                    SetTargetClip(_ux, _uy, false);
                }
            }
        }
        private EmbeddedResourceImageSource travelmode_on_source = new EmbeddedResourceImageSource(new Uri("resource://GnollHackClient.Assets.UI.stone-travel-on.png"));
        private EmbeddedResourceImageSource travelmode_off_source = new EmbeddedResourceImageSource(new Uri("resource://GnollHackClient.Assets.UI.stone-travel-off.png"));
        private void ToggleTravelModeButton_Clicked(object sender, EventArgs e)
        {
            App.PlayMenuSelectSound();
            MapTravelMode = !MapTravelMode;
            if (MapTravelMode)
            {
                ToggleTravelModeButton.ImgSourcePath = "resource://GnollHackClient.Assets.UI.stone-travel-on.png";
                //ToggleModeImg.Source = travelmode_on_source;
            }
            else
            {
                ToggleTravelModeButton.ImgSourcePath = "resource://GnollHackClient.Assets.UI.stone-travel-off.png";
                //ToggleModeImg.Source = travelmode_off_source;
            }
        }
        private EmbeddedResourceImageSource lookmode_on_source = new EmbeddedResourceImageSource(new Uri("resource://GnollHackClient.Assets.UI.stone-look-on.png"));
        private EmbeddedResourceImageSource lookmode_off_source = new EmbeddedResourceImageSource(new Uri("resource://GnollHackClient.Assets.UI.stone-look-off.png"));
        private void LookModeButton_Clicked(object sender, EventArgs e)
        {
            App.PlayMenuSelectSound();
            MapLookMode = !MapLookMode;
            if (MapLookMode)
            {
                LookModeButton.ImgSourcePath = "resource://GnollHackClient.Assets.UI.stone-look-on.png";
                //LookModeImg.Source = lookmode_on_source;
            }
            else
            {
                LookModeButton.ImgSourcePath = "resource://GnollHackClient.Assets.UI.stone-look-off.png";
                //LookModeImg.Source = lookmode_off_source;
            }
        }

        private EmbeddedResourceImageSource minimap_on_source = new EmbeddedResourceImageSource(new Uri("resource://GnollHackClient.Assets.UI.stone-minimap-on.png"));
        private EmbeddedResourceImageSource minimap_off_source = new EmbeddedResourceImageSource(new Uri("resource://GnollHackClient.Assets.UI.stone-minimap-off.png"));
        private void ToggleZoomMiniButton_Clicked(object sender, EventArgs e)
        {
            App.PlayMenuSelectSound();
            ZoomMiniMode = !ZoomMiniMode;
            if (ZoomMiniMode)
            {
                ToggleZoomMiniButton.ImgSourcePath = "resource://GnollHackClient.Assets.UI.stone-minimap-on.png";
                //ToggleZoomMiniImg.Source = minimap_on_source;
                //ToggleZoomMiniButton.BackgroundColor = Color.Green;
            }
            else
            {
                ToggleZoomMiniButton.ImgSourcePath = "resource://GnollHackClient.Assets.UI.stone-minimap-off.png";
                //ToggleZoomMiniImg.Source = minimap_off_source;
                //ToggleZoomMiniButton.BackgroundColor = Color.DarkBlue;
            }
        }

        private EmbeddedResourceImageSource altmap_on_source = new EmbeddedResourceImageSource(new Uri("resource://GnollHackClient.Assets.UI.stone-altmap-on.png"));
        private EmbeddedResourceImageSource altmap_off_source = new EmbeddedResourceImageSource(new Uri("resource://GnollHackClient.Assets.UI.stone-altmap-off.png"));
        private void ToggleZoomAlternateButton_Clicked(object sender, EventArgs e)
        {
            App.PlayMenuSelectSound();
            ZoomAlternateMode = !ZoomAlternateMode;
            if (ZoomAlternateMode)
            {
                ToggleZoomAlternateButton.ImgSourcePath = "resource://GnollHackClient.Assets.UI.stone-altmap-on.png";
                //ToggleZoomAlternateImg.Source = altmap_on_source;
                //ToggleZoomAlternateButton.BackgroundColor = Color.Green;
                lock(_mapOffsetLock)
                {
                    if(MapFontSize > 0)
                    {
                        _mapOffsetX = _mapOffsetX * MapFontAlternateSize / MapFontSize;
                        _mapOffsetY = _mapOffsetY * MapFontAlternateSize / MapFontSize;
                    }
                }

                //if (ZoomChangeCenterMode && !MapAlternateNoClipMode && MapNoClipMode && GHUtils.isok(_ux, _uy))
                //{
                //    SetTargetClip(_ux, _uy, true);
                //}
            }
            else
            {
                ToggleZoomAlternateButton.ImgSourcePath = "resource://GnollHackClient.Assets.UI.stone-altmap-off.png";
                //ToggleZoomAlternateImg.Source = altmap_off_source;
                //ToggleZoomAlternateButton.BackgroundColor = Color.DarkBlue;
                lock (_mapOffsetLock)
                {
                    if (MapFontAlternateSize > 0)
                    {
                        _mapOffsetX = _mapOffsetX * MapFontSize / MapFontAlternateSize;
                        _mapOffsetY = _mapOffsetY * MapFontSize / MapFontAlternateSize;
                    }
                }

                //if (ZoomChangeCenterMode && !MapNoClipMode && MapAlternateNoClipMode && GHUtils.isok(_ux, _uy))
                //{
                //    SetTargetClip(_ux, _uy, true);
                //}
            }

        }

        private void GameMenuButton_Clicked(object sender, EventArgs e)
        {
            GameMenuButton.IsEnabled = false;
            App.PlayButtonClickedSound();
            ShowGameMenu(sender, e);
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
            App.DebugWriteRestart("GHButton_Clicked");
            LabeledImageButton ghbutton = (LabeledImageButton)sender;
            GenericButton_Clicked(sender, e, (int)ghbutton.GHCommand);

        }


        private SKColor _suffixTextColor = new SKColor(220, 220, 220);
        private SKColor _suffixTextColorReverted = new SKColor(35, 35, 35);
        private SKColor _menuHighlightColor = new SKColor(0xFF, 0x88, 0x00, 0x88);
        private int _firstDrawnMenuItemIdx = -1;
        private int _lastDrawnMenuItemIdx = -1;
        private float _totalMenuHeight = 0;
        private void MenuCanvas_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            //DebugWriteProfilingStopwatchTime("Draw Menu Canvas Start");

            SKImageInfo info = e.Info;
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;
            SwitchableCanvasView referenceCanvasView = MenuCanvas;
            float canvaswidth = referenceCanvasView.CanvasSize.Width;
            float canvasheight = referenceCanvasView.CanvasSize.Height;
            float x = 0, y = 0;
            string str;
            SKRect textBounds = new SKRect();
            float scale = canvaswidth / (float)referenceCanvasView.Width;

            canvas.Clear();
            lock (_menuDrawOnlyLock)
            {
                if (_menuDrawOnlyClear)
                    return;
            }

            lock (MenuCanvas.MenuItemLock)
            {
                if (referenceCanvasView.MenuItems == null)
                    return;
            }

            using (SKPaint textPaint = new SKPaint())
            {
                textPaint.Typeface = App.UnderwoodTypeface;
                textPaint.TextSize = GHConstants.MenuDefaultRowHeight * scale;
                float picturewidth = 64.0f * textPaint.FontSpacing / 48.0f;
                float picturepadding = 9 * scale;
                float leftinnerpadding = 5;
                float curmenuoffset = 0;
                lock (MenuScrollLock)
                {
                    curmenuoffset = _menuScrollOffset;
                }
                y += curmenuoffset;
                double menumarginx = 15.0;
                double menuwidth = Math.Max(1, Math.Min(MenuCanvas.Width - menumarginx * 2, ClientUtils.MenuViewWidthRequest(referenceCanvasView.MenuStyle)));
                float menuwidthoncanvas = (float)(menuwidth * scale);
                float leftmenupadding = Math.Max(0, (canvaswidth - menuwidthoncanvas) / 2);
                float rightmenupadding = leftmenupadding;
                float accel_fixed_width = 10;
                bool first = true;
                int idx = -1;
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

                    foreach (GHMenuItem mi in referenceCanvasView.MenuItems)
                    {
                        if ((mi.Attributes & (int)MenuItemAttributes.HalfSize) != 0)
                            textPaint.TextSize = (GHConstants.MenuDefaultRowHeight / 2) * scale;
                        else
                            textPaint.TextSize = GHConstants.MenuDefaultRowHeight * scale;
                        float minrowheight = textPaint.FontSpacing;

                        idx++;
                        x = leftmenupadding;
                        mi.DrawBounds.Left = x;
                        float mainfontsize = (float)mi.FontSize * scale;
                        float relsuffixsize = (float)mi.RelativeSuffixFontSize;
                        float suffixfontsize = relsuffixsize * mainfontsize;
                        textPaint.Typeface = App.GetTypefaceByName(mi.FontFamily);
                        textPaint.TextSize = mainfontsize;
                        textPaint.TextAlign = SKTextAlign.Left;

                        /* Bottom Padding */
                        if (((ulong)mi.MenuFlags & (ulong)MenuFlags.MENU_FLAGS_IS_HEADING) != 0)
                        {
                            bottomPadding = 3 * scale;
                        }
                        else
                        {
                            if (mi.IsSuffixTextVisible && mi.IsSuffix2TextVisible)
                                bottomPadding = 6 * scale;
                            else
                                bottomPadding = 3 * scale;
                        }


                        /* Top Padding */
                        if (((ulong)mi.MenuFlags & (ulong)MenuFlags.MENU_FLAGS_IS_HEADING) != 0)
                        {
                            topPadding = (float)mi.HeadingTopMargin * scale;
                        }
                        else
                        {
                            if (mi.IsSuffixTextVisible && mi.IsSuffix2TextVisible)
                                topPadding = 6 * scale;
                            else
                                topPadding = 3 * scale;
                        }


                        mi.DrawBounds.Top = y;
                        //if (mi.DrawBounds.Top >= canvasheight)
                        //    break;

                        if (first)
                        {
                            accel_fixed_width = textPaint.FontMetrics.AverageCharacterWidth; // + 3 * textPaint.MeasureText(" ");
                            _firstDrawnMenuItemIdx = idx;
                            maintext_x_start = leftmenupadding + leftinnerpadding + (has_identifiers ? accel_fixed_width : 0) + (has_pictures ? picturepadding + picturewidth + picturepadding : textPaint.FontMetrics.AverageCharacterWidth);
                            first = false;
                        }

                        string trimmed_maintext = mi.MainText.Trim();
                        string[] maintextsplit = trimmed_maintext.Split(' ');
                        int maintextrows = CountTextSplitRows(maintextsplit, maintext_x_start, canvaswidth, rightmenupadding, textPaint, mi.UseSpecialSymbols);

                        textPaint.TextSize = suffixfontsize;
                        string trimmed_suffixtext = mi.SuffixText.Trim();
                        string[] suffixtextsplit = trimmed_suffixtext.Split(' ');
                        int suffixtextrows = CountTextSplitRows(suffixtextsplit, maintext_x_start, canvaswidth, rightmenupadding, textPaint, mi.UseSpecialSymbols);

                        string trimmed_suffix2text = mi.Suffix2Text.Trim();
                        string[] suffix2textsplit = trimmed_suffix2text.Split(' ');
                        int suffix2textrows = CountTextSplitRows(suffix2textsplit, maintext_x_start, canvaswidth, rightmenupadding, textPaint, mi.UseSpecialSymbols);

                        textPaint.TextSize = mainfontsize;

                        fontspacingpadding = (textPaint.FontSpacing - (textPaint.FontMetrics.Descent - textPaint.FontMetrics.Ascent)) / 2;
                        float generallinepadding = Math.Max(0.0f, (minrowheight - (textPaint.FontSpacing) * ((float)maintextrows + suffixtextrows * (mi.IsSuffixTextVisible ? relsuffixsize : 0.0f) + (mi.IsSuffix2TextVisible ? relsuffixsize : 0.0f))) / 2);

                        bool isselected = referenceCanvasView.SelectionHow == SelectionMode.Multiple ? mi.Selected :
                            referenceCanvasView.SelectionHow == SelectionMode.Single ? idx == referenceCanvasView.SelectionIndex : false;

                        float totalRowHeight = topPadding + bottomPadding + ((float)maintextrows + suffixtextrows * (mi.IsSuffixTextVisible ? relsuffixsize : 0.0f) + (mi.IsSuffix2TextVisible ? relsuffixsize : 0.0f)) * (textPaint.FontSpacing) + 2 * generallinepadding;
                        float totalRowWidth = canvaswidth - leftmenupadding - rightmenupadding;

                        /* Selection rectangle */
                        SKRect selectionrect = new SKRect(x, y, x + totalRowWidth, y + totalRowHeight);
                        if (isselected && !(y + totalRowHeight <= 0 || y >= canvasheight))
                        {
                            textPaint.Color = _menuHighlightColor;
                            textPaint.Style = SKPaintStyle.Fill;
                            canvas.DrawRect(selectionrect, textPaint);
                        }

                        float singlelinepadding = Math.Max(0.0f, ((float)(maintextrows - 1) * (textPaint.FontSpacing)) / 2);
                        y += topPadding;
                        y += generallinepadding;
                        y += fontspacingpadding;
                        y -= textPaint.FontMetrics.Ascent;
                        x += leftinnerpadding;

                        if (has_identifiers)
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
                        else
                        {
                            x += textPaint.FontMetrics.AverageCharacterWidth;
                        }

                        /* Main text */
                        SKColor maincolor = ClientUtils.NHColor2SKColorCore(mi.NHColor, mi.Attributes, MenuCanvas.RevertBlackAndWhite);
                        textPaint.Color = maincolor;
                        //int split_idx_on_row = -1;
                        float start_x = x;
                        float indent_start_x = start_x;
                        string indentstr = GHUtils.GetIndentationString(trimmed_maintext, mi.Attributes);
                        if (indentstr != "")
                        {
                            indent_start_x += textPaint.MeasureText(indentstr);
                        }
                        DrawTextSplit(canvas, maintextsplit, ref x, ref y, start_x, indent_start_x, canvaswidth, canvasheight, rightmenupadding, textPaint, mi.UseSpecialSymbols);
                        y += textPaint.FontMetrics.Descent + fontspacingpadding;

                        /* Suffix text */
                        if (mi.IsSuffixTextVisible)
                        {
                            textPaint.Color = mi.UseColorForSuffixes ? maincolor : MenuCanvas.RevertBlackAndWhite ? _suffixTextColorReverted : _suffixTextColor;
                            textPaint.TextSize = suffixfontsize;
                            y += fontspacingpadding;
                            y -= textPaint.FontMetrics.Ascent;
                            DrawTextSplit(canvas, suffixtextsplit, ref x, ref y, start_x, indent_start_x, canvaswidth, canvasheight, rightmenupadding, textPaint, mi.UseSpecialSymbols);
                            y += textPaint.FontMetrics.Descent + fontspacingpadding;
                        }

                        /* Suffix 2 text */
                        if (mi.IsSuffix2TextVisible)
                        {
                            textPaint.Color = mi.UseColorForSuffixes ? maincolor : MenuCanvas.RevertBlackAndWhite ? _suffixTextColorReverted : _suffixTextColor;
                            textPaint.TextSize = suffixfontsize;
                            fontspacingpadding = (textPaint.FontSpacing - (textPaint.FontMetrics.Descent - textPaint.FontMetrics.Ascent)) / 2;
                            y += fontspacingpadding;
                            y -= textPaint.FontMetrics.Ascent;
                            DrawTextSplit(canvas, suffix2textsplit, ref x, ref y, start_x, indent_start_x, canvaswidth, canvasheight, rightmenupadding, textPaint, mi.UseSpecialSymbols);
                            y += textPaint.FontMetrics.Descent + fontspacingpadding;
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
                    }
                    _totalMenuHeight = y - curmenuoffset;
                }
            }

            //DebugWriteProfilingStopwatchTime("Draw Menu Canvas End");
        }

        private int CountTextSplitRows(string[] textsplit, float x_start, float canvaswidth, float rightmenupadding, SKPaint textPaint, bool usespecialsymbols)
        {
            int rows = 1;
            float calc_x_start = x_start;
            int rowidx = -1;
            float spacelength = textPaint.MeasureText(" ");

            foreach (string s in textsplit)
            {
                bool nowrap = false;
                if (string.IsNullOrWhiteSpace(s))
                    nowrap = true;
                rowidx++;
                float endposition = calc_x_start;
                SKBitmap symbolbitmap = null;
                float printlength = 0;
                float marginlength = 0;
                if (usespecialsymbols && (symbolbitmap = GetSpecialSymbol(s)) != null)
                {
                    float bmpheight = textPaint.FontMetrics.Descent / 2 - textPaint.FontMetrics.Ascent;
                    float bmpwidth = bmpheight * (float)symbolbitmap.Width / (float)Math.Max(1, symbolbitmap.Height);
                    float bmpmargin = bmpheight / 8;
                    printlength = bmpwidth;
                    marginlength = bmpmargin;
                }
                else
                {
                    printlength = textPaint.MeasureText(s);
                    marginlength = spacelength;
                }
                endposition = calc_x_start + printlength;
                bool pastend = endposition > canvaswidth - rightmenupadding;
                if (pastend && rowidx > 0 & !nowrap)
                {
                    rows++;
                    calc_x_start = x_start + printlength + marginlength;
                    rowidx = 0;
                }
                else
                {
                    calc_x_start = endposition + marginlength;
                }
            }
            return rows;
        }

        private void DrawTextSplit(SKCanvas canvas, string[] textsplit, ref float x, ref float y, float start_x, float indent_start_x, float canvaswidth, float canvasheight, float rightmenupadding, SKPaint textPaint, bool usespecialsymbols)
        {
            int split_idx_on_row = -1;
            x = start_x;
            float spacelength = textPaint.MeasureText(" ");
            foreach (string split_str in textsplit)
            {
                bool nowrap = false;
                if (string.IsNullOrWhiteSpace(split_str))
                    nowrap = true;
                split_idx_on_row++;
                float endposition = x;
                SKBitmap symbolbitmap = null;
                if(usespecialsymbols && (symbolbitmap = GetSpecialSymbol(split_str)) != null)
                {
                    float bmpheight = textPaint.FontMetrics.Descent / 2 - textPaint.FontMetrics.Ascent;
                    float bmpwidth = bmpheight * (float)symbolbitmap.Width / (float)Math.Max(1, symbolbitmap.Height);
                    float bmpmargin = bmpheight / 8;
                    endposition = x + bmpwidth + bmpmargin;
                    bool pastend = x + bmpwidth > canvaswidth - rightmenupadding;
                    if (pastend && split_idx_on_row > 0 && !nowrap)
                    {
                        x = indent_start_x;
                        y += textPaint.FontSpacing;
                        split_idx_on_row = 0;
                        endposition = x + bmpwidth + bmpmargin;
                    }
                    if (!(y + textPaint.FontSpacing + textPaint.FontMetrics.Ascent <= 0 || y + textPaint.FontMetrics.Ascent >= canvasheight))
                    {
                        float bmpx = x;
                        float bmpy = y + textPaint.FontMetrics.Ascent;
                        SKRect bmptargetrect = new SKRect(bmpx, bmpy, bmpx + bmpwidth, bmpy + bmpheight);
                        canvas.DrawBitmap(symbolbitmap, bmptargetrect, textPaint);
                    }
                }
                else
                {
                    float printlength = textPaint.MeasureText(split_str);
                    endposition = x + printlength + spacelength;
                    bool pastend = x + printlength > canvaswidth - rightmenupadding;
                    if (pastend && split_idx_on_row > 0 && !nowrap)
                    {
                        x = indent_start_x;
                        y += textPaint.FontSpacing;
                        split_idx_on_row = 0;
                        endposition = x + printlength + spacelength;
                    }

                    if (!(y + textPaint.FontSpacing + textPaint.FontMetrics.Ascent <= 0 || y + textPaint.FontMetrics.Ascent >= canvasheight))
                        canvas.DrawText(split_str, x, y, textPaint);
                }

                x = endposition;
            }
            x = start_x;
        }

        private SKBitmap GetSpecialSymbol(string str)
        {
            string trimmed_str = str.Trim();
            if(trimmed_str == "&success;")
            {
                return _successBitmap;
            }
            else if (trimmed_str == "&mana;")
            {
                return _manaBitmap;
            }
            else if (trimmed_str == "&cool;")
            {
                return _cooldownBitmap;
            }
            else if (trimmed_str == "&casts;")
            {
                return _castsBitmap;
            }
            else if (trimmed_str == "&adds;")
            {
                return _addsBitmap;
            }
            else if (trimmed_str == "&food;")
            {
                return _foodBitmap;
            }
            else if (trimmed_str == "&spabj;")
            {
                return _spellAbjurationBitmap;
            }
            else if (trimmed_str == "&sparc;")
            {
                return _spellArcaneBitmap;
            }
            else if (trimmed_str == "&spcel;")
            {
                return _spellCelestialBitmap;
            }
            else if (trimmed_str == "&spcle;")
            {
                return _spellClericalBitmap;
            }
            else if (trimmed_str == "&spcon;")
            {
                return _spellConjurationBitmap;
            }
            else if (trimmed_str == "&spdiv;")
            {
                return _spellDivinationBitmap;
            }
            else if (trimmed_str == "&spenc;")
            {
                return _spellEnchantmentBitmap;
            }
            else if (trimmed_str == "&sphea;")
            {
                return _spellHealingBitmap;
            }
            else if (trimmed_str == "&spmov;")
            {
                return _spellMovementBitmap;
            }
            else if (trimmed_str == "&spnat;")
            {
                return _spellNatureBitmap;
            }
            else if (trimmed_str == "&spnec;")
            {
                return _spellNecromancyBitmap;
            }
            else if (trimmed_str == "&sptra;")
            {
                return _spellTransmutationBitmap;
            }
            return null;
        }

        private object MenuScrollLock = new object();
        private float _menuScrollOffset = 0;

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

                    if (MenuTouchDictionary.Count > 1)
                        _menuTouchMoved = true;
                    else
                    {
                        _savedMenuSender = sender;
                        _savedMenuEventArgs = e;

                        Device.StartTimer(TimeSpan.FromSeconds(GHConstants.LongMenuTapThreshold), () =>
                        {
                            if (_savedMenuSender == null || _savedMenuEventArgs == null)
                                return false;

                            if (_savedMenuSender == null || _savedMenuEventArgs == null)
                                return false;
                            DateTime curtime = DateTime.Now;
                            if (curtime - _savedMenuTimeStamp < TimeSpan.FromSeconds(GHConstants.LongMenuTapThreshold * 0.8))
                                return false; /* Changed touch position */

                            MenuCanvas_LongTap(_savedMenuSender, _savedMenuEventArgs);
                            return false;
                        });
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
                                    /* Do not scroll within button press time threshold, unless large move */
                                    if (dist > GHConstants.MoveDistanceThreshold || (DateTime.Now.Ticks - entry.PressTime.Ticks) / TimeSpan.TicksPerMillisecond > GHConstants.MoveOrPressTimeThreshold)
                                    {
                                        lock (MenuScrollLock)
                                        {
                                            _menuScrollOffset += diffY;
                                            if (_menuScrollOffset > 0)
                                                _menuScrollOffset = 0;
                                            else if (_menuScrollOffset < MenuCanvas.CanvasSize.Height - _totalMenuHeight)
                                                _menuScrollOffset = Math.Min(0, MenuCanvas.CanvasSize.Height - _totalMenuHeight);
                                        }
                                        MenuTouchDictionary[e.Id].Location = e.Location;
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
                            if (MenuTouchDictionary.Count == 0)
                                _menuTouchMoved = false;
                        }
                        e.Handled = true;
                    }
                    break;
                case SKTouchAction.Cancelled:
                    break;
                case SKTouchAction.Exited:
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

            if (MenuCanvas.MenuItems[selectedidx].MaxCount <= 1)
                return;

            _countMenuItem = MenuCanvas.MenuItems[selectedidx];

            _menuTouchMoved = true; /* No further action upon release */
            if ((MenuCanvas.SelectionHow == SelectionMode.Multiple && !MenuCanvas.MenuItems[selectedidx].Selected)
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
                for (int i = 0; i <= (int)MenuCanvas.MenuItems[selectedidx].MaxCount; i++)
                {
                    _countPickList.Add(new GHNumberPickItem(i));
                    if (_countMenuItem.Count == i)
                        countselindex = i + 1;
                }
                CountPicker.ItemsSource = _countPickList;
                CountPicker.ItemDisplayBinding = new Binding("Name");
                CountPicker.SelectedIndex = countselindex;
            }

            MenuCountCaption.Text = (MenuCountEntry.IsVisible ? "Type" : "Select") + " Count for " + MenuCanvas.MenuItems[selectedidx].MainText;
            MenuCountBackgroundGrid.IsVisible = true;
        }

        private void MenuCanvas_NormalClickRelease(object sender, SKTouchEventArgs e)
        {
            lock (MenuCanvas.MenuItemLock)
            {
                if (MenuCanvas.MenuItems == null)
                    return;

                for (int idx = _firstDrawnMenuItemIdx; idx >= 0 && idx <= _lastDrawnMenuItemIdx; idx++)
                {
                    if (idx >= MenuCanvas.MenuItems.Count)
                        break;
                    if (e.Location.Y >= MenuCanvas.MenuItems[idx].DrawBounds.Top && e.Location.Y <= MenuCanvas.MenuItems[idx].DrawBounds.Bottom)
                    {
                        GHMenuItem mi = MenuCanvas.MenuItems[idx];
                        if (mi.Identifier == 0)
                        {
                            if (MenuCanvas.SelectionHow == SelectionMode.Multiple && (mi.MenuFlags & (ulong)MenuFlags.MENU_FLAGS_IS_GROUP_HEADING) != 0)
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
                                    MenuCanvas.MenuItems[idx].Count = -1;
                                else
                                    MenuCanvas.MenuItems[idx].Count = 0;
                            }
                            else
                            {
                                if (idx != MenuCanvas.SelectionIndex && MenuCanvas.SelectionIndex >= 0 && MenuCanvas.SelectionIndex < MenuCanvas.MenuItems.Count)
                                    MenuCanvas.MenuItems[MenuCanvas.SelectionIndex].Count = 0;

                                MenuCanvas.SelectionIndex = idx;
                                if (MenuCanvas.MenuItems[idx].Count == 0)
                                    MenuCanvas.MenuItems[idx].Count = -1;

                                /* Else keep the current selection number */
                            }
                        }
                        break;
                    }
                }
            }
        }

        private object _menuHideCancelledLock = new object();
        private bool _menuHideCancelled = false;
        private bool _menuHideOn = false;
        private void MenuOKButton_Clicked(object sender, EventArgs e)
        {
            lock (_menuDrawOnlyLock)
            {
                _menuRefresh = false;
                _menuDrawOnlyClear = true;
            }

            _menuScrollOffset = 0;

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

            if (ClientGame.ResponseDictionary.TryGetValue(_clientGame, out queue))
            {
                queue.Enqueue(new GHResponse(_clientGame, GHRequestType.ShowMenuPage, MenuCanvas.GHWindow, resultlist));
            }

            DelayedMenuHide();
        }

        private void MenuCancelButton_Clicked(object sender, EventArgs e)
        {
            lock (_menuDrawOnlyLock)
            {
                _menuRefresh = false;
                _menuDrawOnlyClear = true;
            }

            _menuScrollOffset = 0;
            _menuHideCancelled = false;

            ConcurrentQueue<GHResponse> queue;
            if (ClientGame.ResponseDictionary.TryGetValue(_clientGame, out queue))
            {
                queue.Enqueue(new GHResponse(_clientGame, GHRequestType.ShowMenuPage, MenuCanvas.GHWindow, new List<GHMenuItem>()));
            }

            DelayedMenuHide();
        }

        private void DelayedMenuHide()
        {
            lock(_menuHideCancelledLock)
            {
                _menuHideCancelled = false;
                _menuHideOn = true;
            }
            Device.StartTimer(TimeSpan.FromSeconds(GHConstants.WindowHideIntervals / GHConstants.MainCanvasAnimationFrequency), () =>
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

                lock (_canvasPageLock)
                {
                    _canvasPage = canvas_page_types.MainGamePage;
                }
                lock (RefreshScreenLock)
                {
                    RefreshScreen = true;
                }
                StartMainCanvasAnimation();

                return false;
            });
        }

        private object _delayedTextHideLock = new object();
        private bool _delayedTextHideOn = false;
        private bool _delayedTextHideCancelled = false;
        private void DelayedTextHide()
        {
            lock (_delayedTextHideLock)
            {
                _delayedTextHideOn = true;
                _delayedTextHideCancelled = false;
            }
            Device.StartTimer(TimeSpan.FromSeconds(GHConstants.WindowHideIntervals / GHConstants.MainCanvasAnimationFrequency), () =>
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
                        MenuCountEntry.TextColor = Color.Red;
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
                MenuCountEntry.TextColor = Color.White;
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
                    MenuCountEntry.TextColor = Color.Green;
                }
                else
                {
                    MenuCountEntry.TextColor = Color.Red;
                }
            }
        }


        private GlyphImageSource _textGlyphImageSource = new GlyphImageSource();

        public ImageSource TextGlyphImage
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
                return (Math.Abs(_textGlyphImageSource.Glyph) > 0 && _textGlyphImageSource.Glyph != NoGlyph);
            }
        }


        private float _totalTextHeight = 0;

        private object TextScrollLock = new object();
        private float _textScrollOffset = 0;

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

            lock (TextCanvas.MenuItemLock)
            {
                if (TextCanvas.PutStrItems == null || TextCanvas.PutStrItems.Count == 0)
                    return;
            }

            using (SKPaint textPaint = new SKPaint())
            {
                textPaint.Typeface = App.UnderwoodTypeface;
                textPaint.TextSize = 30 * scale;
                textPaint.Style = SKPaintStyle.Fill;
                float minrowheight = textPaint.FontSpacing;
                float leftinnerpadding = 5;
                float curmenuoffset = 0;
                lock (TextScrollLock)
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
                float glyphpadding = scale * (float)Math.Max(0.0, TextCanvas.X + TextCanvas.Width - TextWindowGlyphImage.X);
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
                        textPaint.Typeface = App.GetTypefaceByName(putstritem.TextWindowFontFamily);
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
                        float start_x = x;
                        string indentstr = putstritem.GetIndentationString();
                        if(indentstr != "")
                        {
                            start_x += textPaint.MeasureText(indentstr);
                        }

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

                            textPaint.Color = ClientUtils.NHColor2SKColorCore(
                                instr.Color < (int)nhcolor.CLR_MAX ? instr.Color : TextCanvas.RevertBlackAndWhite ? (int)nhcolor.CLR_BLACK : (int)nhcolor.CLR_WHITE, 
                                instr.Attributes,
                                TextCanvas.RevertBlackAndWhite);

                            string[] split = str.Split(' ');
                            int idx = 0;
                            foreach (string split_str in split)
                            {
                                bool nowrap = false;
                                if (string.IsNullOrWhiteSpace(split_str))
                                    nowrap = true;
                                string added_str = (idx == split.Length - 1 ? "" : " ");
                                string added_split_str = split_str + added_str;
                                float printlength = textPaint.MeasureText(split_str);
                                float spaceprintlength = (added_str != "") ? textPaint.MeasureText(added_str) : 0.0f;
                                float endposition = x + printlength;
                                float endposition_with_space = x + printlength + spaceprintlength;
                                float usedglyphpadding = 0.0f;

                                if (TextWindowGlyphImage.IsVisible
                                    && (putstritem.InstructionList[0].Attributes & (int)MenuItemAttributes.Title) != 0
                                    && y - curmenuoffset + textPaint.FontMetrics.Ascent <= glyphyend 
                                    && y - curmenuoffset + textPaint.FontMetrics.Descent >= glyphystart)
                                    usedglyphpadding = glyphpadding;

                                bool pastend = endposition > canvaswidth - usedglyphpadding - rightmenupadding - (float)putstritem.RightPaddingWidth * scale;
                                if (pastend && x > start_x && !nowrap)
                                {
                                    x = start_x;
                                    y += textPaint.FontSpacing;
                                    endposition_with_space = x + printlength + spaceprintlength;
                                }

                                if (!(y + textPaint.FontSpacing + textPaint.FontMetrics.Ascent <= 0 || y + textPaint.FontMetrics.Ascent >= canvasheight))
                                {
                                    if(TextCanvas.UseTextOutline)
                                    {
                                        textPaint.Style = SKPaintStyle.Stroke;
                                        textPaint.StrokeWidth = textPaint.TextSize / 10;
                                        SKColor oldcolor = textPaint.Color;
                                        textPaint.Color = TextCanvas.RevertBlackAndWhite ? SKColors.White : SKColors.Black;
                                        canvas.DrawText(added_split_str, x, y, textPaint);
                                        textPaint.Style = SKPaintStyle.Fill;
                                        textPaint.Color = oldcolor;
                                    }
                                    canvas.DrawText(added_split_str, x, y, textPaint);
                                }

                                x = endposition_with_space;
                                idx++;
                            }
                        }
                        j++;
                        y += textPaint.FontMetrics.Descent + fontspacingpadding;
                    }
                    _totalTextHeight = y - curmenuoffset;
                }
            }
        }

        private void TextCanvas_Touch(object sender, SKTouchEventArgs e)
        {
            lock (TextCanvas.TextItemLock)
            {
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
                                            lock (TextScrollLock)
                                            {
                                                _textScrollOffset += diffY;
                                                if (_textScrollOffset > 0)
                                                    _textScrollOffset = 0;
                                                else if (_textScrollOffset < TextCanvas.CanvasSize.Height - _totalTextHeight)
                                                    _textScrollOffset = Math.Min(0, TextCanvas.CanvasSize.Height - _totalTextHeight);
                                            }
                                            TextTouchDictionary[e.Id].Location = e.Location;
                                            _textTouchMoved = true;
                                            _savedTextTimeStamp = DateTime.Now;
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
                                if (TextTouchDictionary.Count == 0)
                                    _textTouchMoved = false;
                            }
                            e.Handled = true;
                        }
                        break;
                    case SKTouchAction.Cancelled:
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

        private void InitializeArrowButtons(Assembly assembly)
        {
            using (Stream stream = assembly.GetManifestResourceStream("GnollHackClient.Assets.UI.arrow_left.png"))
            {
                _arrowBitmap[0] = SKBitmap.Decode(stream);
            }
            using (Stream stream = assembly.GetManifestResourceStream("GnollHackClient.Assets.UI.arrow_up.png"))
            {
                _arrowBitmap[1] = SKBitmap.Decode(stream);
            }
            using (Stream stream = assembly.GetManifestResourceStream("GnollHackClient.Assets.UI.arrow_right.png"))
            {
                _arrowBitmap[2] = SKBitmap.Decode(stream);
            }
            using (Stream stream = assembly.GetManifestResourceStream("GnollHackClient.Assets.UI.arrow_down.png"))
            {
                _arrowBitmap[3] = SKBitmap.Decode(stream);
            }
            using (Stream stream = assembly.GetManifestResourceStream("GnollHackClient.Assets.UI.arrow_upleft.png"))
            {
                _arrowBitmap[4] = SKBitmap.Decode(stream);
            }
            using (Stream stream = assembly.GetManifestResourceStream("GnollHackClient.Assets.UI.no.png"))
            {
                _arrowBitmap[5] = SKBitmap.Decode(stream);
            }
            using (Stream stream = assembly.GetManifestResourceStream("GnollHackClient.Assets.UI.arrow_upright.png"))
            {
                _arrowBitmap[6] = SKBitmap.Decode(stream);
            }
            using (Stream stream = assembly.GetManifestResourceStream("GnollHackClient.Assets.UI.arrow_downright.png"))
            {
                _arrowBitmap[7] = SKBitmap.Decode(stream);
            }
            using (Stream stream = assembly.GetManifestResourceStream("GnollHackClient.Assets.UI.arrow_downleft.png"))
            {
                _arrowBitmap[8] = SKBitmap.Decode(stream);
            }
        }

        private void InitializeUIBitmaps(Assembly assembly)
        {
            using (Stream stream = assembly.GetManifestResourceStream("GnollHackClient.Assets.UI.orb_border.png"))
            {
                _orbBorderBitmap = SKBitmap.Decode(stream);
            }
            using (Stream stream = assembly.GetManifestResourceStream("GnollHackClient.Assets.UI.orb_fill.png"))
            {
                _orbFillBitmap = SKBitmap.Decode(stream);
            }

            using (SKPaint bmpPaint = new SKPaint())
            {
                bmpPaint.Color = SKColors.White.WithAlpha(204);

                var redbitmap = new SKBitmap(_orbFillBitmap.Width, _orbFillBitmap.Height, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
                var redcanvas = new SKCanvas(redbitmap);
                redcanvas.Clear(SKColors.Transparent);
                bmpPaint.ColorFilter = SKColorFilter.CreateColorMatrix(new float[]
                    {
                    -1.0f, 0,     0,    0, 255f,
                    0,     1.0f,  0,    0, 0,
                    0,     0,     1.0f, 0, 0,
                    0,     0,     0,    1, 0
                    });
                redcanvas.DrawBitmap(_orbFillBitmap, 0, 0, bmpPaint);
                _orbFillBitmapRed = redbitmap;

                var bluebitmap = new SKBitmap(_orbFillBitmap.Width, _orbFillBitmap.Height, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
                var bluecanvas = new SKCanvas(bluebitmap);
                bluecanvas.Clear(SKColors.Transparent);
                bmpPaint.ColorFilter = SKColorFilter.CreateColorMatrix(new float[]
                    {
                    1.0f,  0,      0,    0,   0,
                    0,     1.0f,   0,    0,   0,
                    0,     0,     -1.0f, 0,   255f,
                    0,     0,     0,     1,   0
                    });

                bluecanvas.DrawBitmap(_orbFillBitmap, 0, 0, bmpPaint);
                _orbFillBitmapBlue = bluebitmap;
            }

            using (Stream stream = assembly.GetManifestResourceStream("GnollHackClient.Assets.UI.orb_glass.png"))
            {
                _orbGlassBitmap = SKBitmap.Decode(stream);
            }

            using (Stream stream = assembly.GetManifestResourceStream("GnollHackClient.Assets.UI.status-wizard-mode.png"))
            {
                _statusWizardBitmap = SKBitmap.Decode(stream);
            }
            using (Stream stream = assembly.GetManifestResourceStream("GnollHackClient.Assets.UI.status-casual-mode.png"))
            {
                _statusCasualBitmap = SKBitmap.Decode(stream);
            }
            using (Stream stream = assembly.GetManifestResourceStream("GnollHackClient.Assets.UI.status-casual-classic-mode.png"))
            {
                _statusCasualClassicBitmap = SKBitmap.Decode(stream);
            }
            using (Stream stream = assembly.GetManifestResourceStream("GnollHackClient.Assets.UI.status-modern-mode.png"))
            {
                _statusModernBitmap = SKBitmap.Decode(stream);
            }
            using (Stream stream = assembly.GetManifestResourceStream("GnollHackClient.Assets.UI.status-difficulty.png"))
            {
                _statusDifficultyBitmap = SKBitmap.Decode(stream);
            }
            using (Stream stream = assembly.GetManifestResourceStream("GnollHackClient.Assets.UI.status-difficulty-very-easy.png"))
            {
                _statusDifficultyVeryEasyBitmap = SKBitmap.Decode(stream);
            }
            using (Stream stream = assembly.GetManifestResourceStream("GnollHackClient.Assets.UI.status-difficulty-easy.png"))
            {
                _statusDifficultyEasyBitmap = SKBitmap.Decode(stream);
            }
            using (Stream stream = assembly.GetManifestResourceStream("GnollHackClient.Assets.UI.status-difficulty-average.png"))
            {
                _statusDifficultyAverageBitmap = SKBitmap.Decode(stream);
            }
            using (Stream stream = assembly.GetManifestResourceStream("GnollHackClient.Assets.UI.status-difficulty-hard.png"))
            {
                _statusDifficultyHardBitmap = SKBitmap.Decode(stream);
            }
            using (Stream stream = assembly.GetManifestResourceStream("GnollHackClient.Assets.UI.status-difficulty-expert.png"))
            {
                _statusDifficultyExpertBitmap = SKBitmap.Decode(stream);
            }
            using (Stream stream = assembly.GetManifestResourceStream("GnollHackClient.Assets.UI.status-difficulty-master.png"))
            {
                _statusDifficultyMasterBitmap = SKBitmap.Decode(stream);
            }
            using (Stream stream = assembly.GetManifestResourceStream("GnollHackClient.Assets.UI.status-difficulty-grand-master.png"))
            {
                _statusDifficultyGrandMasterBitmap = SKBitmap.Decode(stream);
            }

            using (Stream stream = assembly.GetManifestResourceStream("GnollHackClient.Assets.UI.status-xp-level.png"))
            {
                _statusXPLevelBitmap = SKBitmap.Decode(stream);
            }
            using (Stream stream = assembly.GetManifestResourceStream("GnollHackClient.Assets.UI.status-hd.png"))
            {
                _statusHDBitmap = SKBitmap.Decode(stream);
            }
            using (Stream stream = assembly.GetManifestResourceStream("GnollHackClient.Assets.UI.status-ac.png"))
            {
                _statusACBitmap = SKBitmap.Decode(stream);
            }
            using (Stream stream = assembly.GetManifestResourceStream("GnollHackClient.Assets.UI.status-mc.png"))
            {
                _statusMCBitmap = SKBitmap.Decode(stream);
            }
            using (Stream stream = assembly.GetManifestResourceStream("GnollHackClient.Assets.UI.status-move.png"))
            {
                _statusMoveBitmap = SKBitmap.Decode(stream);
            }
            using (Stream stream = assembly.GetManifestResourceStream("GnollHackClient.Assets.UI.status-weapon-style.png"))
            {
                _statusWeaponStyleBitmap = SKBitmap.Decode(stream);
            }

            using (Stream stream = assembly.GetManifestResourceStream("GnollHackClient.Assets.UI.status-gold.png"))
            {
                _statusGoldBitmap = SKBitmap.Decode(stream);
            }
            using (Stream stream = assembly.GetManifestResourceStream("GnollHackClient.Assets.UI.status-turns.png"))
            {
                _statusTurnsBitmap = SKBitmap.Decode(stream);
            }
            using (Stream stream = assembly.GetManifestResourceStream("GnollHackClient.Assets.UI.status-dungeon-level.png"))
            {
                _statusDungeonLevelBitmap = SKBitmap.Decode(stream);
            }

            using (Stream stream = assembly.GetManifestResourceStream("GnollHackClient.Assets.UI.search.png"))
            {
                _searchBitmap = SKBitmap.Decode(stream);
            }
            using (Stream stream = assembly.GetManifestResourceStream("GnollHackClient.Assets.UI.wait.png"))
            {
                _waitBitmap = SKBitmap.Decode(stream);
            }

            /* Replaceable menu symbols */
            using (Stream stream = assembly.GetManifestResourceStream("GnollHackClient.Assets.UI.symbol-success.png"))
            {
                _successBitmap = SKBitmap.Decode(stream);
            }
            using (Stream stream = assembly.GetManifestResourceStream("GnollHackClient.Assets.UI.symbol-mana.png"))
            {
                _manaBitmap = SKBitmap.Decode(stream);
            }
            using (Stream stream = assembly.GetManifestResourceStream("GnollHackClient.Assets.UI.symbol-cooldown.png"))
            {
                _cooldownBitmap = SKBitmap.Decode(stream);
            }
            using (Stream stream = assembly.GetManifestResourceStream("GnollHackClient.Assets.UI.symbol-casts.png"))
            {
                _castsBitmap = SKBitmap.Decode(stream);
            }
            using (Stream stream = assembly.GetManifestResourceStream("GnollHackClient.Assets.UI.symbol-adds.png"))
            {
                _addsBitmap = SKBitmap.Decode(stream);
            }
            using (Stream stream = assembly.GetManifestResourceStream("GnollHackClient.Assets.UI.symbol-food.png"))
            {
                _foodBitmap = SKBitmap.Decode(stream);
            }

            using (Stream stream = assembly.GetManifestResourceStream("GnollHackClient.Assets.UI.symbol-spell-abjuration.png"))
            {
                _spellAbjurationBitmap = SKBitmap.Decode(stream);
            }
            using (Stream stream = assembly.GetManifestResourceStream("GnollHackClient.Assets.UI.symbol-spell-arcane.png"))
            {
                _spellArcaneBitmap = SKBitmap.Decode(stream);
            }
            using (Stream stream = assembly.GetManifestResourceStream("GnollHackClient.Assets.UI.symbol-spell-celestial.png"))
            {
                _spellCelestialBitmap = SKBitmap.Decode(stream);
            }
            using (Stream stream = assembly.GetManifestResourceStream("GnollHackClient.Assets.UI.symbol-spell-clerical.png"))
            {
                _spellClericalBitmap = SKBitmap.Decode(stream);
            }
            using (Stream stream = assembly.GetManifestResourceStream("GnollHackClient.Assets.UI.symbol-spell-conjuration.png"))
            {
                _spellConjurationBitmap = SKBitmap.Decode(stream);
            }
            using (Stream stream = assembly.GetManifestResourceStream("GnollHackClient.Assets.UI.symbol-spell-divination.png"))
            {
                _spellDivinationBitmap = SKBitmap.Decode(stream);
            }
            using (Stream stream = assembly.GetManifestResourceStream("GnollHackClient.Assets.UI.symbol-spell-enchantment.png"))
            {
                _spellEnchantmentBitmap = SKBitmap.Decode(stream);
            }
            using (Stream stream = assembly.GetManifestResourceStream("GnollHackClient.Assets.UI.symbol-spell-healing.png"))
            {
                _spellHealingBitmap = SKBitmap.Decode(stream);
            }
            using (Stream stream = assembly.GetManifestResourceStream("GnollHackClient.Assets.UI.symbol-spell-movement.png"))
            {
                _spellMovementBitmap = SKBitmap.Decode(stream);
            }
            using (Stream stream = assembly.GetManifestResourceStream("GnollHackClient.Assets.UI.symbol-spell-nature.png"))
            {
                _spellNatureBitmap = SKBitmap.Decode(stream);
            }
            using (Stream stream = assembly.GetManifestResourceStream("GnollHackClient.Assets.UI.symbol-spell-necromancy.png"))
            {
                _spellNecromancyBitmap = SKBitmap.Decode(stream);
            }
            using (Stream stream = assembly.GetManifestResourceStream("GnollHackClient.Assets.UI.symbol-spell-transmutation.png"))
            {
                _spellTransmutationBitmap = SKBitmap.Decode(stream);
            }

        }

        public GHCommandButtonItem[,,] _moreBtnMatrix = new GHCommandButtonItem[GHConstants.MoreButtonPages, GHConstants.MoreButtonsPerRow, GHConstants.MoreButtonsPerColumn];
        public SKBitmap[,,] _moreBtnBitmaps = new SKBitmap[GHConstants.MoreButtonPages, GHConstants.MoreButtonsPerRow, GHConstants.MoreButtonsPerColumn];
        public string[] _moreButtonPageTitle = new string[GHConstants.MoreButtonPages] { "Wizard Mode Commands", "Common Commands", "Additional Commands", "Context and More Commands" };

        private object _moreCmdLock = new object();
        private int _moreCmdPage = 1;
        private float _moreCmdOffsetX = 0.0f;
        private float _moreCmdOffsetY = 0.0f;
        public int MoreCmdPage { get { lock (_moreCmdLock) { return _moreCmdPage; } } set { lock (_moreCmdLock) { _moreCmdPage = value; } } }
        public float MoreCmdOffsetX { get { lock (_moreCmdLock) { return _moreCmdOffsetX; } } set { lock (_moreCmdLock) { _moreCmdOffsetX = value; } } }
        public float MoreCmdOffsetY { get { lock (_moreCmdLock) { return _moreCmdOffsetY; } } set { lock (_moreCmdLock) { _moreCmdOffsetY = value; } } }
        private float _moreCmdOffsetAutoSpeed = 5.0f; /* Screen widths per second */


        private void InitializeMoreCommandButtons()
        {
            _moreBtnMatrix[0, 0, 0] = new GHCommandButtonItem("Wish", "GnollHackClient.Assets.UI.wish.png", GHUtils.Ctrl((int)'w'));
            _moreBtnMatrix[0, 1, 0] = new GHCommandButtonItem("Reveal", "GnollHackClient.Assets.UI.reveal.png", GHUtils.Ctrl((int)'f'));
            _moreBtnMatrix[0, 2, 0] = new GHCommandButtonItem("Genesis", "GnollHackClient.Assets.UI.genesis.png", GHUtils.Meta((int)'m'));
            _moreBtnMatrix[0, 3, 0] = new GHCommandButtonItem("Levelport", "GnollHackClient.Assets.UI.levelport.png", GHUtils.Ctrl((int)'v'));

            _moreBtnMatrix[0, 0, 1] = new GHCommandButtonItem("Identify", "GnollHackClient.Assets.UI.identify.png", GHUtils.Ctrl((int)'i'));
            _moreBtnMatrix[0, 1, 1] = new GHCommandButtonItem("Teleport", "GnollHackClient.Assets.UI.teleport.png", GHUtils.Ctrl((int)'t'));
            _moreBtnMatrix[0, 2, 1] = new GHCommandButtonItem("Level Change", "GnollHackClient.Assets.UI.levelchange.png", GHUtils.Meta(0));
            _moreBtnMatrix[0, 3, 1] = new GHCommandButtonItem("Polymorph Self", "GnollHackClient.Assets.UI.polymorph.png", GHUtils.Meta(1));

            _moreBtnMatrix[0, 2, 5] = new GHCommandButtonItem("Extended", "GnollHackClient.Assets.UI.extended.png", (int)'#');
            _moreBtnMatrix[0, 3, 5] = new GHCommandButtonItem("Back to Game", "GnollHackClient.Assets.UI.more.png", -1);

            _moreBtnMatrix[1, 0, 0] = new GHCommandButtonItem("You", "GnollHackClient.Assets.UI.you.png", (int)'}');
            _moreBtnMatrix[1, 1, 0] = new GHCommandButtonItem("Skills", "GnollHackClient.Assets.UI.skill.png", (int)'S');
            _moreBtnMatrix[1, 2, 0] = new GHCommandButtonItem("Overview", "GnollHackClient.Assets.UI.overview.png", GHUtils.Ctrl((int)'o'));
            _moreBtnMatrix[1, 3, 0] = new GHCommandButtonItem("Discoveries", "GnollHackClient.Assets.UI.discoveries.png", (int)'\\');

            _moreBtnMatrix[1, 0, 1] = new GHCommandButtonItem("View Spell", "GnollHackClient.Assets.UI.viewspell.png", GHUtils.Meta((int)'z'));
            _moreBtnMatrix[1, 1, 1] = new GHCommandButtonItem("Mix", "GnollHackClient.Assets.UI.mix.png", (int)'X');
            _moreBtnMatrix[1, 2, 1] = new GHCommandButtonItem("Travel", "GnollHackClient.Assets.UI.travel.png", (int)'_');
            _moreBtnMatrix[1, 3, 1] = new GHCommandButtonItem("2-Weapon", "GnollHackClient.Assets.UI.twoweap.png", GHUtils.Ctrl((int)'x'));

            _moreBtnMatrix[1, 0, 2] = new GHCommandButtonItem("Examine", "GnollHackClient.Assets.UI.examine.png", GHUtils.Meta((int)'x'));
            _moreBtnMatrix[1, 1, 2] = new GHCommandButtonItem("Engrave", "GnollHackClient.Assets.UI.engrave.png", (int)'E');
            _moreBtnMatrix[1, 2, 2] = new GHCommandButtonItem("Ride", "GnollHackClient.Assets.UI.ride.png", GHUtils.Meta((int)'R'));
            _moreBtnMatrix[1, 3, 2] = new GHCommandButtonItem("Wield", "GnollHackClient.Assets.UI.wield.png", (int)'w');

            _moreBtnMatrix[1, 0, 3] = new GHCommandButtonItem("Eat", "GnollHackClient.Assets.UI.eat.png", (int)'e');
            _moreBtnMatrix[1, 1, 3] = new GHCommandButtonItem("Drink", "GnollHackClient.Assets.UI.quaff.png", (int)'q');
            _moreBtnMatrix[1, 2, 3] = new GHCommandButtonItem("Read", "GnollHackClient.Assets.UI.read.png", (int)'r');
            _moreBtnMatrix[1, 3, 3] = new GHCommandButtonItem("Drop Types", "GnollHackClient.Assets.UI.dropmany.png", (int)'D');

            _moreBtnMatrix[1, 0, 4] = new GHCommandButtonItem("Untrap", "GnollHackClient.Assets.UI.untrap.png", GHUtils.Meta((int)'u'));
            _moreBtnMatrix[1, 1, 4] = new GHCommandButtonItem("Handedness", "GnollHackClient.Assets.UI.handedness.png", GHUtils.Meta((int)'h'));
            _moreBtnMatrix[1, 2, 4] = new GHCommandButtonItem("Yell", "GnollHackClient.Assets.UI.yell.png", GHUtils.Ctrl((int)'y'));
            _moreBtnMatrix[1, 3, 4] = new GHCommandButtonItem("Pray", "GnollHackClient.Assets.UI.pray.png", GHUtils.Meta((int)'p'));

            _moreBtnMatrix[1, 0, 5] = new GHCommandButtonItem("Count", "GnollHackClient.Assets.UI.count.png", -5);
            _moreBtnMatrix[1, 1, 5] = new GHCommandButtonItem("Search 20", "GnollHackClient.Assets.UI.search20.png", -2);
            _moreBtnMatrix[1, 2, 5] = new GHCommandButtonItem("Search 200", "GnollHackClient.Assets.UI.search200.png", -3);
            _moreBtnMatrix[1, 3, 5] = new GHCommandButtonItem("Back to Game", "GnollHackClient.Assets.UI.more.png", -1);

            _moreBtnMatrix[2, 0, 0] = new GHCommandButtonItem("Attributes", "GnollHackClient.Assets.UI.attributes.png", GHUtils.Meta(2));
            _moreBtnMatrix[2, 1, 0] = new GHCommandButtonItem("Conduct", "GnollHackClient.Assets.UI.conduct.png", GHUtils.Meta(3));
            _moreBtnMatrix[2, 2, 0] = new GHCommandButtonItem("Killed", "GnollHackClient.Assets.UI.killed.png", GHUtils.Meta((int)'k'));
            _moreBtnMatrix[2, 3, 0] = new GHCommandButtonItem("Genocided", "GnollHackClient.Assets.UI.genocided.png", GHUtils.Meta((int)'g'));

            _moreBtnMatrix[2, 0, 1] = new GHCommandButtonItem("Wear", "GnollHackClient.Assets.UI.wear.png", (int)'W');
            _moreBtnMatrix[2, 1, 1] = new GHCommandButtonItem("Put On", "GnollHackClient.Assets.UI.puton.png", (int)'P');
            _moreBtnMatrix[2, 2, 1] = new GHCommandButtonItem("Quiver", "GnollHackClient.Assets.UI.quiver.png", (int)'Q');
            _moreBtnMatrix[2, 3, 1] = new GHCommandButtonItem("Fight", "GnollHackClient.Assets.UI.fight.png", (int)'F');

            _moreBtnMatrix[2, 0, 2] = new GHCommandButtonItem("Take Off", "GnollHackClient.Assets.UI.takeoff.png", (int)'T');
            _moreBtnMatrix[2, 1, 2] = new GHCommandButtonItem("Remove", "GnollHackClient.Assets.UI.remove.png", (int)'R');
            _moreBtnMatrix[2, 2, 2] = new GHCommandButtonItem("Take Off Many", "GnollHackClient.Assets.UI.takeoffmany.png", GHUtils.Meta((int)'t'));
            _moreBtnMatrix[2, 3, 2] = new GHCommandButtonItem("Jump", "GnollHackClient.Assets.UI.jump.png", (int)'j');

            _moreBtnMatrix[2, 0, 3] = new GHCommandButtonItem("Dig", "GnollHackClient.Assets.UI.dig.png", GHUtils.Ctrl((int)'g'));
            _moreBtnMatrix[2, 1, 3] = new GHCommandButtonItem("Light", "GnollHackClient.Assets.UI.light.png", GHUtils.Ctrl((int)'l'));
            _moreBtnMatrix[2, 2, 3] = new GHCommandButtonItem("Break", "GnollHackClient.Assets.UI.break.png", GHUtils.Ctrl((int)'b'));
            _moreBtnMatrix[2, 3, 3] = new GHCommandButtonItem("Force Lock", "GnollHackClient.Assets.UI.forcelock.png", GHUtils.Meta((int)'f'));

            _moreBtnMatrix[2, 0, 4] = new GHCommandButtonItem("Tip", "GnollHackClient.Assets.UI.tip.png", GHUtils.Meta((int)'T'));
            _moreBtnMatrix[2, 1, 4] = new GHCommandButtonItem("Invoke", "GnollHackClient.Assets.UI.invoke.png", GHUtils.Meta((int)'i'));
            _moreBtnMatrix[2, 2, 4] = new GHCommandButtonItem("Rub", "GnollHackClient.Assets.UI.rub.png", GHUtils.Meta((int)'r'));
            _moreBtnMatrix[2, 3, 4] = new GHCommandButtonItem("Wipe", "GnollHackClient.Assets.UI.wipe.png", GHUtils.Meta((int)'w'));

            _moreBtnMatrix[2, 0, 5] = new GHCommandButtonItem("Name", "GnollHackClient.Assets.UI.name.png", (int)'N');
            _moreBtnMatrix[2, 1, 5] = new GHCommandButtonItem("What Is", "GnollHackClient.Assets.UI.whatis.png", (int)'/');
            _moreBtnMatrix[2, 2, 5] = new GHCommandButtonItem("Look Far", "GnollHackClient.Assets.UI.lookfar.png", (int)';');
            _moreBtnMatrix[2, 3, 5] = new GHCommandButtonItem("Back to Game", "GnollHackClient.Assets.UI.more.png", -1);

            _moreBtnMatrix[3, 0, 0] = new GHCommandButtonItem("Look Here", "GnollHackClient.Assets.UI.lookhere.png", (int)':');
            _moreBtnMatrix[3, 1, 0] = new GHCommandButtonItem("Pick Up", "GnollHackClient.Assets.UI.pickup.png", (int)',');
            _moreBtnMatrix[3, 2, 0] = new GHCommandButtonItem("Sit", "GnollHackClient.Assets.UI.sit.png", GHUtils.Ctrl((int)'s'));
            _moreBtnMatrix[3, 3, 0] = new GHCommandButtonItem("Pay", "GnollHackClient.Assets.UI.pay.png", (int)'p');

            _moreBtnMatrix[3, 0, 1] = new GHCommandButtonItem("Loot", "GnollHackClient.Assets.UI.loot.png", (int)'l');
            _moreBtnMatrix[3, 1, 1] = new GHCommandButtonItem("Dip", "GnollHackClient.Assets.UI.dip.png", GHUtils.Meta((int)'d'));
            _moreBtnMatrix[3, 2, 1] = new GHCommandButtonItem("Offer", "GnollHackClient.Assets.UI.offer.png", GHUtils.Meta((int)'o'));
            _moreBtnMatrix[3, 3, 1] = new GHCommandButtonItem("Spells", "GnollHackClient.Assets.UI.spells.png", (int)'+');

            _moreBtnMatrix[3, 0, 2] = new GHCommandButtonItem("Go Down", "GnollHackClient.Assets.UI.stairs-down.png", (int)'>');
            _moreBtnMatrix[3, 1, 2] = new GHCommandButtonItem("Go Up", "GnollHackClient.Assets.UI.stairs-up.png", (int)'<');
            _moreBtnMatrix[3, 2, 2] = new GHCommandButtonItem("Open", "GnollHackClient.Assets.UI.open.png", (int)'o');
            _moreBtnMatrix[3, 3, 2] = new GHCommandButtonItem("Close", "GnollHackClient.Assets.UI.close.png", (int)'c');

            _moreBtnMatrix[3, 0, 3] = new GHCommandButtonItem("Teleport", "GnollHackClient.Assets.UI.teleport.png", GHUtils.Ctrl((int)'t'));
            _moreBtnMatrix[3, 1, 3] = new GHCommandButtonItem("Monster", "GnollHackClient.Assets.UI.monster.png", GHUtils.Meta(4));

            _moreBtnMatrix[3, 0, 5] = new GHCommandButtonItem("Help", "GnollHackClient.Assets.UI.help.png", (int)'?'); // "Menu", "GnollHackClient.Assets.Icons.missing_icon.png", -4);
            _moreBtnMatrix[3, 1, 5] = new GHCommandButtonItem("Commands", "GnollHackClient.Assets.UI.commands.png", GHUtils.Meta((int)'c'));
            _moreBtnMatrix[3, 2, 5] = new GHCommandButtonItem("Extended", "GnollHackClient.Assets.UI.extended.png", (int)'#');
            _moreBtnMatrix[3, 3, 5] = new GHCommandButtonItem("Back to Game", "GnollHackClient.Assets.UI.more.png", -1);


            Assembly assembly = GetType().GetTypeInfo().Assembly;
            for (int k = 0; k < GHConstants.MoreButtonPages; k++)
            {
                for (int i = 0; i < GHConstants.MoreButtonsPerRow; i++)
                {
                    for (int j = 0; j < GHConstants.MoreButtonsPerColumn; j++)
                    {
                        if (_moreBtnMatrix[k, i, j] != null && !string.IsNullOrEmpty(_moreBtnMatrix[k, i, j].ImageSourcePath))
                        {
                            using (Stream stream = assembly.GetManifestResourceStream(_moreBtnMatrix[k, i, j].ImageSourcePath))
                            {
                                if (stream != null)
                                    _moreBtnBitmaps[k, i, j] = SKBitmap.Decode(stream);
                            }
                        }
                    }
                }
            }
        }


        public object CommandButtonLock = new object();
        private Dictionary<long, TouchEntry> CommandTouchDictionary = new Dictionary<long, TouchEntry>();
        private object _savedCommandSender = null;
        private SKTouchEventArgs _savedCommandEventArgs = null;
        private DateTime _savedCommandTimeStamp;
        private bool _commandTouchMoved = false;
        private bool _commandChangedPage = false;

        private object _cmdBtnMatrixRectLock = new object();
        private SKRect _cmdBtnMatrixRect = new SKRect();
        public SKRect CmdBtnMatrixRect { get { SKRect val; lock (_cmdBtnMatrixRectLock) { val = _cmdBtnMatrixRect; } return val; } set { lock (_cmdBtnMatrixRectLock) { _cmdBtnMatrixRect = value; } } }

        private object _mainCounterLock = new object();
        private long _mainCounterValue = 0;
        public long MainCounterValue { get { lock (_mainCounterLock) { return _mainCounterValue; } } }

        private object _mainFPSCounterLock = new object();
        private long _mainFPSCounterValue = 0;

        private object _commandFPSCounterLock = new object();
        private long _commandFPSCounterValue = 0;

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
            CmdBtnMatrixRect = new SKRect();

            using (SKPaint textPaint = new SKPaint())
            {
                float cmdOffsetX = MoreCmdOffsetX;
                int curpage = MoreCmdPage;
                int pagemin = cmdOffsetX > 0 ? Math.Max(EnableWizardMode ? 0 : 1, curpage - 1) : curpage;
                int pagemax = cmdOffsetX < 0 ? Math.Min(GHConstants.MoreButtonPages - 1, curpage + 1) : curpage;
                SKRect screenRect = new SKRect(0, 0, canvaswidth, canvasheight);

                float smalldotheight = Math.Min(canvaswidth, canvasheight) / 120 * scale;
                float largedotheight = smalldotheight * 2;
                float dotmargin = smalldotheight;

                textPaint.Style = SKPaintStyle.Fill;
                for (int i = (EnableWizardMode ? 0 : 1); i < GHConstants.MoreButtonPages; i++)
                {
                    int numdots = (EnableWizardMode ? 4 : 3);
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
                    textPaint.Typeface = App.ImmortalTypeface;
                    textPaint.TextSize = titlesize;
                    textPaint.TextAlign = SKTextAlign.Center;

                    string titlestr = _moreButtonPageTitle[page];
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
                    textPaint.Typeface = App.LatoRegular;
                    textPaint.TextSize = 12.0f * 3.0f * btnImgRawWidth / 256.0f;
                    textPaint.TextAlign = SKTextAlign.Center;

                    float btnImgRawHeight = Math.Min(256, Math.Min(btnAreaHeight * 0.925f - textPaint.FontSpacing, 128 * scale));

                    float btnImgWidth = Math.Min(btnImgRawWidth, btnImgRawHeight);
                    float btnImgHeight = btnImgWidth;

                    for (int i = 0; i < GHConstants.MoreButtonsPerRow; i++)
                    {
                        int pos_j = 0;
                        for (int j = 0; j < GHConstants.MoreButtonsPerColumn; j++)
                        {
                            if (_moreBtnMatrix[page, i, j] != null && _moreBtnBitmaps[page, i, j] != null)
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

                                canvas.DrawBitmap(_moreBtnBitmaps[page, i, j], targetrect);
                                canvas.DrawText(_moreBtnMatrix[page, i, j].Text, text_x, text_y, textPaint);
                            }
                            pos_j++;
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
                    textPaint.Typeface = App.LatoBold;
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
                                            int maxpage = GHConstants.MoreButtonPages - 1;
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

                                        GHCommandButtonItem cbi = _moreBtnMatrix[MoreCmdPage, i, j];
                                        if (cbi != null)
                                        {
                                            if (cbi.Command >= 0)
                                                GenericButton_Clicked(CommandCanvas, e, cbi.Command);
                                            else
                                            {
                                                switch (cbi.Command)
                                                {
                                                    case -2:
                                                        GenericButton_Clicked(sender, e, 'n');
                                                        GenericButton_Clicked(sender, e, -12);
                                                        GenericButton_Clicked(sender, e, -10);
                                                        GenericButton_Clicked(sender, e, 's');
                                                        break;
                                                    case -3:
                                                        GenericButton_Clicked(sender, e, 'n');
                                                        GenericButton_Clicked(sender, e, -12);
                                                        GenericButton_Clicked(sender, e, -10);
                                                        GenericButton_Clicked(sender, e, -10);
                                                        GenericButton_Clicked(sender, e, 's');
                                                        break;
                                                    case -4:
                                                        GameMenuButton_Clicked(sender, e);
                                                        break;
                                                    case -5:
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
                                        if (cmdPage < GHConstants.MoreButtonPages - 1)
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
                                        if (cmdPage < GHConstants.MoreButtonPages - 1)
                                        {
                                            MoreCmdPage = cmdPage + 1;
                                            MoreCmdOffsetX = cmdOffset + btnMatrixWidth;
                                        }

                                        _commandChangedPage = true;
                                    }
                                }

                                if (CommandTouchDictionary.ContainsKey(e.Id))
                                    CommandTouchDictionary.Remove(e.Id);
                                if (CommandTouchDictionary.Count == 0)
                                    _commandTouchMoved = false;
                            }
                            e.Handled = true;
                        }
                        break;
                    case SKTouchAction.Cancelled:
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
            lock(_mapOffsetLock)
            {
                _messageOffsetY = 0.0f;
            }
            ForceAllMessages = !ForceAllMessages;
        }

        private object _tipLock = new object();
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
                textPaint.Typeface = App.LatoRegular;
                textPaint.TextSize = 36;
                textPaint.MeasureText(str, ref bounds);
                float fontsize = Math.Min(48, 36 * 0.18f / Math.Max(0.01f, (bounds.Width / Math.Max(1, Math.Min(canvaswidth, canvasheight)))));

                str = "This is an explanation.";
                textPaint.Typeface = App.UnderwoodTypeface;
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
                        textPaint.Typeface = App.ARChristyTypeface;
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
                        textPaint.Typeface = App.UnderwoodTypeface;
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
                        PaintTipButton(canvas, textPaint, GameMenuButton, "This opens the main menu.", "Main Menu", 1.5f, centerfontsize, fontsize, false, -0.15f, 0);
                        break;
                    case 2:
                        PaintTipButton(canvas, textPaint, ESCButton, "This cancels any command.", "Escape Button", 1.5f, centerfontsize, fontsize, false, -1.5f, 0);
                        break;
                    case 3:
                        PaintTipButton(canvas, textPaint, ToggleAutoCenterModeButton, "This toggles auto-center on player.", "Map Auto-Center", 1.5f, centerfontsize, fontsize, false, -1.5f, 0);
                        break;
                    case 4:
                        PaintTipButton(canvas, textPaint, ToggleZoomMiniButton, "This zoom shows the entire level.", "Minimap", 1.5f, centerfontsize, fontsize, false, landscape ? -0.15f : -0.5f, landscape ? 0 : 1.5f);
                        break;
                    case 5:
                        PaintTipButton(canvas, textPaint, ToggleZoomAlternateButton, "This is the secondary zoom.", "Alternative Zoom", 1.5f, centerfontsize, fontsize, false, landscape ? -1.5f : -0.15f, 0);
                        break;
                    case 6:
                        PaintTipButton(canvas, textPaint, LookModeButton, "This allows you to inspect the map.", "Look Mode", 1.5f, centerfontsize, fontsize, false, -0.15f, landscape ? -0.5f : 0);
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
                        lock(_rectLock)
                        {
                            PaintTipButtonByRect(canvas, textPaint, HealthRect, "This orb shows your hit points.", "Health Orb", 1.1f, centerfontsize, fontsize, true, 0.15f, 0.0f);
                        }
                        break;
                    case 13:
                        lock (_rectLock)
                        {
                            PaintTipButtonByRect(canvas, textPaint, ManaRect, "And this one your mana.", "Mana Orb", 1.1f, centerfontsize, fontsize, true, 0.15f, 0.0f);
                        }
                        break;
                    case 14:
                        textPaint.TextSize = 36;
                        textPaint.Typeface = App.ARChristyTypeface;
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
                        textPaint.Typeface = App.UnderwoodTypeface;
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
                    if(ShownTip == 12 && HealthRect.Width == 0)
                        ShownTip++;
                    if (ShownTip == 13 && ManaRect.Width == 0)
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
                            if (ClientGame.ResponseDictionary.TryGetValue(_clientGame, out queue))
                            {
                                queue.Enqueue(new GHResponse(_clientGame, GHRequestType.ShowGUITips));
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

        public SKRect GetViewScreenRect(VisualElement view)
        {
            float canvaswidth = canvasView.CanvasSize.Width;
            float canvasheight = canvasView.CanvasSize.Height;

            double screenCoordinateX = view.X;
            double screenCoordinateY = view.Y;
            // Get the view's parent (if it has one...)
            if (view.Parent.GetType() != typeof(App))
            {
                VisualElement parent = (VisualElement)view.Parent;

                // Loop through all parents
                while (parent != null)
                {
                    // Add in the coordinates of the parent with respect to ITS parent
                    screenCoordinateX += parent.X;
                    screenCoordinateY += parent.Y;

                    // If the parent of this parent isn't the app itself, get the parent's parent.
                    if (parent.Parent.GetType() == typeof(App))
                        parent = null;
                    else
                        parent = (VisualElement)parent.Parent;
                }
            }
            float relX = (float)(screenCoordinateX / canvasView.Width) * canvaswidth;
            float relY = (float)(screenCoordinateY / canvasView.Height) * canvasheight;
            float relWidth = (float)(GameMenuButton.Width / canvasView.Width) * canvaswidth;
            float relHeight = (float)(GameMenuButton.Height / canvasView.Height) * canvasheight;

            SKRect res = new SKRect(relX, relY, relX + relWidth, relY + relHeight);
            return res;
        }

        public void PaintTipButton(SKCanvas canvas, SKPaint textPaint, VisualElement view, string centertext, string boxtext, float radius_mult, float centertextfontsize, float boxfontsize, bool linefromright, float lineoffsetx, float lineoffsety)
        {
            SKRect viewrect = GetViewScreenRect(view);
            PaintTipButtonByRect(canvas, textPaint, viewrect, centertext, boxtext, radius_mult, centertextfontsize, boxfontsize, linefromright, lineoffsetx, lineoffsety);
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
            textPaint.Typeface = App.UnderwoodTypeface;
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

            textPaint.Typeface = App.LatoRegular;
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
            canvas.DrawBitmap(_orbBorderBitmap, orbBorderDest, textPaint);
            if (orbfillpercentage < 0)
                orbfillpercentage = 0;
            if (orbfillpercentage > 1)
                orbfillpercentage = 1;
            SKBitmap fillBitmap = fillcolor == SKColors.Red ? _orbFillBitmapRed : fillcolor == SKColors.Blue ? _orbFillBitmapBlue : _orbFillBitmap;
            SKRect orbFillSrc = new SKRect(0.0f, (float)fillBitmap.Height * (1.0f - orbfillpercentage), (float)fillBitmap.Width, (float)fillBitmap.Height);
            SKRect orbFillDest = new SKRect(orbDest.Left, orbDest.Top + orbDest.Height * (1.0f - orbfillpercentage), orbDest.Right, orbDest.Bottom);
            canvas.DrawBitmap(fillBitmap, orbFillSrc, orbFillDest, textPaint);
            canvas.DrawBitmap(_orbGlassBitmap, orbDest, textPaint);
            if(val != null && val != "")
            {
                textPaint.TextSize = 36;
                textPaint.Typeface = App.LatoBold;
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
                canvas.DrawText(val, tx, ty, textPaint);
                textPaint.Style = SKPaintStyle.Fill;
                textPaint.Color = SKColors.White;
                canvas.DrawText(val, tx, ty, textPaint);
            }

            if (showmax && maxval != null && maxval != "")
            {
                textPaint.TextSize = 36;
                textPaint.Typeface = App.LatoBold;
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
                canvas.DrawText(maxval, tx, ty, textPaint);
                textPaint.Style = SKPaintStyle.Fill;
                textPaint.Color = SKColors.White;
                canvas.DrawText(maxval, tx, ty, textPaint);
            }
            textPaint.TextAlign = SKTextAlign.Left;
        }

        public enum UIShieldStyles
        {
            AC = 0,
            MC,
            Move,
            Weapon,
            DungeonLevel,
            XPLevel
        }

        public async void ReportPanic(string text)
        {
            await DisplayAlert("Panic", text != null ? text : "GnollHack has panicked. See the Panic Log.", "OK");

            ConcurrentQueue<GHResponse> queue;
            if (ClientGame.ResponseDictionary.TryGetValue(_clientGame, out queue))
            {
                queue.Enqueue(new GHResponse(_clientGame, GHRequestType.Panic));
            }
        }

        public async void ShowMessage(string text)
        {
            await DisplayAlert("Message", text != null ? text : "No message.", "OK");

            ConcurrentQueue<GHResponse> queue;
            if (ClientGame.ResponseDictionary.TryGetValue(_clientGame, out queue))
            {
                queue.Enqueue(new GHResponse(_clientGame, GHRequestType.Message));
            }
        }

        public async void ReportCrashDetected()
        {
            bool answer = await DisplayAlert("Crash Detected", "A crashed game has been detected. GnollHack will attempt to restore this game. Also, do you want to create a crash report? This will create a zip archive of the files in your game directory and ask it to be shared further.", "Yes", "No");
            if (answer)
            {
                await CheckAndRequestWritePermission();
                await CheckAndRequestReadPermission();
                string archive_file = "";
                try
                {
                    archive_file = App.CreateGameZipArchive();
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Archive Creation Failure", "GnollHack failed to create a crash report archive: " + ex.Message, "OK");
                    goto finished;
                }
                try
                {
                    if (archive_file != "")
                        ShareFile(archive_file, "GnollHack Crash Report");

                }
                catch (Exception ex)
                {
                    await DisplayAlert("Share File Failure", "GnollHack failed to share a crash report archive: " + ex.Message, "OK");
                    goto finished;
                }
            }

        finished:
            ConcurrentQueue<GHResponse> queue;
            if (ClientGame.ResponseDictionary.TryGetValue(_clientGame, out queue))
            {
                queue.Enqueue(new GHResponse(_clientGame, GHRequestType.CrashReport));
            }
        }

        private async void ShareFile(string filename, string title)
        {
            if (!File.Exists(filename))
            {
                await DisplayAlert("File Sharing Failure", "GnollHack cannot find file \'" + filename + "\'", "OK");
                return;
            }
            await Share.RequestAsync(new ShareFileRequest
            {
                Title = title,
                File = new ShareFile(filename)
            });
        }
        public async Task<PermissionStatus> CheckAndRequestWritePermission()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();

            if (status == PermissionStatus.Granted)
                return status;

            if (status == PermissionStatus.Denied && DeviceInfo.Platform == DevicePlatform.iOS)
            {
                // Prompt the user to turn on in settings
                // On iOS once a permission has been denied it may not be requested again from the application
                await DisplayAlert("Permission Needed", "GnollHack needs the file write permission to create a zip file. Please turn it on in Settings.", "OK");
                return status;
            }

            if (Permissions.ShouldShowRationale<Permissions.StorageWrite>())
            {
                await DisplayAlert("Permission Needed", "GnollHack needs the file write permission to create a zip file.", "OK");
            }

            status = await Permissions.RequestAsync<Permissions.StorageWrite>();

            return status;
        }
        public async Task<PermissionStatus> CheckAndRequestReadPermission()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.StorageRead>();

            if (status == PermissionStatus.Granted)
                return status;

            if (status == PermissionStatus.Denied && DeviceInfo.Platform == DevicePlatform.iOS)
            {
                // Prompt the user to turn on in settings
                // On iOS once a permission has been denied it may not be requested again from the application
                await DisplayAlert("Permission Needed", "GnollHack needs the file read permission to work with a zip file. Please turn it on in Settings.", "OK");
                return status;
            }

            if (Permissions.ShouldShowRationale<Permissions.StorageRead>())
            {
                await DisplayAlert("Permission Needed", "GnollHack needs the file read permission to work with a zip file.", "OK");
            }

            status = await Permissions.RequestAsync<Permissions.StorageRead>();

            return status;
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

                for (int i = 0; i < ExtendedCommands.Count; i++)
                {
                    var command = ExtendedCommands[i];
                    if (command == null)
                        break;
                    if (command.StartsWith(GetLineEntryText.Text))
                    {
                        GetLineAutoComplete.Text = command;
                        break;
                    }
                }
            }
        }
    }

    public class ColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if ((bool)value == true)
            {
                return Color.White;
            }

            return Color.DarkGray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value == true)
            {
                return Color.White;
            }

            return Color.DarkGray;
        }
    }

    public class TouchEntry 
    {
        public SKPoint Location;
        public DateTime PressTime;
        public SKPoint OriginalLocation;

        public TouchEntry()
        {

        }
        public TouchEntry(SKPoint loc, DateTime time)
        {
            Location = loc;
            PressTime = time;
            OriginalLocation = loc;
        }
    }

}