/* GnollHack 4.0	mswproc.c	$NHDT-Date: 1545705822 2018/12/25 02:43:42 $  $NHDT-Branch: GnollHack-3.6.2-beta01 $:$NHDT-Revision: 1.130 $ */
/* Copyright (C) 2001 by Alex Kompel 	 */
/* GnollHack may be freely redistributed.  See license for details. */

/*
 * This file implements the interface between the window port specific
 * code in the mswin port and the rest of the GnollHack game engine.
*/

#include "hack.h"
#include "color.h"
#include "dlb.h"
#include "func_tab.h" /* for extended commands */
#include "winMS.h"
#include "sdlproc.h"
#include "sdlnuklear.h"
#include <assert.h>
#include <mmsystem.h>
#include "mhmap.h"
#include "mhstatus.h"
#include "mhtext.h"
#include "mhmsgwnd.h"
#include "mhmenu.h"
#include "mhsplash.h"
#include "mhmsg.h"
#include "mhinput.h"
#include "mhaskyn.h"
#include "mhdlg.h"
#include "mhrip.h"
#include "mhmain.h"
#include "mhfont.h"
#include "resource.h"

extern void mswin_main_loop();

#define LLEN 128

#define NHTRACE_LOG "nhtrace.log"

#ifdef DEBUG
# ifdef _DEBUG
static FILE* _s_debugfp = NULL;
extern void logDebug(const char *fmt, ...);
# endif
#endif

static void sdl_main_loop(void);
static void sdl_wait_loop(int milliseconds);
static BOOL sdl_initMapTiles(void);
static void sdl_color_from_string(char *colorstring, HBRUSH *brushptr,
                                    COLORREF *colorptr);
static void prompt_for_player_selection(void);

boolean sdl_disallow_keyboard_commands_in_wait_loop = FALSE;

#define TOTAL_BRUSHES 10
HBRUSH sdl_brush_table[TOTAL_BRUSHES];
int sdl_max_brush = 0;

HBRUSH sdl_menu_bg_brush = NULL;
HBRUSH sdl_menu_fg_brush = NULL;
HBRUSH sdl_text_bg_brush = NULL;
HBRUSH sdl_text_fg_brush = NULL;
HBRUSH sdl_status_bg_brush = NULL;
HBRUSH sdl_status_fg_brush = NULL;
HBRUSH sdl_message_bg_brush = NULL;
HBRUSH sdl_message_fg_brush = NULL;

COLORREF sdl_menu_bg_color = RGB(0, 0, 0);
COLORREF sdl_menu_fg_color = RGB(0xFF, 0xFF, 0xFF);
COLORREF sdl_text_bg_color = RGB(0, 0, 0);
COLORREF sdl_text_fg_color = RGB(0xFF, 0xFF, 0xFF);
COLORREF sdl_status_bg_color = RGB(0, 0, 0);
COLORREF sdl_status_fg_color = RGB(0xFF, 0xFF, 0xFF);
COLORREF sdl_message_bg_color = RGB(0, 0, 0);
COLORREF sdl_message_fg_color = RGB(0xFF, 0xFF, 0xFF);

strbuf_t sdl_raw_print_strbuf = { 0 };

/* Interface definition, for windows.c */
struct window_procs sdl_procs = {
    "SDL",
    WC_COLOR | WC_HILITE_PET | WC_ALIGN_MESSAGE | WC_ALIGN_STATUS | WC_INVERSE
        | WC_SCROLL_AMOUNT | WC_SCROLL_MARGIN | WC_MAP_MODE | WC_FONT_MESSAGE
        | WC_FONT_STATUS | WC_FONT_MENU | WC_FONT_TEXT | WC_FONT_MAP
        | WC_FONTSIZ_MESSAGE | WC_FONTSIZ_STATUS | WC_FONTSIZ_MENU
        | WC_FONTSIZ_TEXT | WC_TILE_WIDTH | WC_TILE_HEIGHT | WC_TILE_FILE
        | WC_VARY_MSGCOUNT | WC_WINDOWCOLORS | WC_PLAYER_SELECTION
        | WC_SPLASH_SCREEN | WC_POPUP_DIALOG | WC_MOUSE_SUPPORT,
#ifdef STATUS_HILITES
    WC2_HITPOINTBAR | WC2_FLUSH_STATUS | WC2_RESET_STATUS | WC2_HILITE_STATUS |
#endif
    WC2_PREFERRED_SCREEN_SCALE, sdl_init_nhwindows, sdl_player_selection, sdl_askname,
    sdl_get_nh_event, sdl_exit_nhwindows, sdl_suspend_nhwindows,
    sdl_resume_nhwindows, sdl_create_nhwindow, sdl_clear_nhwindow,
    sdl_display_nhwindow, sdl_destroy_nhwindow, sdl_curs, sdl_putstr,
    genl_putmixed, sdl_display_file, sdl_start_menu, sdl_add_menu, sdl_add_extended_menu,
    sdl_end_menu, sdl_select_menu,
    genl_message_menu, /* no need for X-specific handling */
    sdl_update_inventory, sdl_mark_synch, sdl_wait_synch,
#ifdef CLIPPING
    sdl_cliparound,
#endif
#ifdef POSITIONBAR
    donull,
#endif
    sdl_print_glyph, sdl_raw_print, sdl_raw_print_bold, sdl_nhgetch,
    sdl_nh_poskey, sdl_nhbell, sdl_doprev_message, sdl_yn_function,
    sdl_getlin, sdl_get_ext_cmd, sdl_number_pad, sdl_delay_output, sdl_delay_output_milliseconds,
#ifdef CHANGE_COLOR /* only a Mac option currently */
    mswin, sdl_change_background,
#endif
    /* other defs that really should go away (they're tty specific) */
    sdl_start_screen, sdl_end_screen, sdl_outrip,
    sdl_preference_update, sdl_getmsghistory, sdl_putmsghistory,
    sdl_status_init, sdl_status_finish, sdl_status_enablefield,
    sdl_status_update,
    genl_can_suspend_yes,
    sdl_stretch_window,
    sdl_set_animation_timer,
    sdl_open_special_view,
    sdl_stop_all_sounds,
    sdl_play_immediate_ghsound,
    sdl_play_ghsound_occupation_ambient,
    sdl_play_ghsound_effect_ambient,
    sdl_set_effect_ambient_volume,
    sdl_play_ghsound_music,
    sdl_play_ghsound_level_ambient,
    sdl_play_ghsound_environment_ambient,
    sdl_adjust_ghsound_general_volumes,
    sdl_add_ambient_ghsound,
    sdl_delete_ambient_ghsound,
    sdl_set_ambient_ghsound_volume,
};

/*
init_nhwindows(int* argcp, char** argv)
                -- Initialize the windows used by GnollHack.  This can also
                   create the standard windows listed at the top, but does
                   not display them.
                -- Any commandline arguments relevant to the windowport
                   should be interpreted, and *argcp and *argv should
                   be changed to remove those arguments.
                -- When the message window is created, the variable
                   iflags.window_inited needs to be set to TRUE.  Otherwise
                   all plines() will be done via raw_print().
                ** Why not have init_nhwindows() create all of the "standard"
                ** windows?  Or at least all but WIN_INFO?      -dean
*/
void
sdl_init_nhwindows(int *argc, char **argv)
{
    UNREFERENCED_PARAMETER(argc);
    UNREFERENCED_PARAMETER(argv);

#ifdef DEBUG
# ifdef _DEBUG
    if (showdebug(NHTRACE_LOG) && !_s_debugfp) {
        /* truncate trace file */
        _s_debugfp = fopen(NHTRACE_LOG, "w");
    }
# endif
#endif
    logDebug("sdl_init_nhwindows()\n");

    /* SDL setup */
#define WINDOW_WIDTH 1200
#define WINDOW_HEIGHT 800
    SDL_SetHint(SDL_HINT_VIDEO_HIGHDPI_DISABLED, "0");
    SDL_Init(SDL_INIT_VIDEO);

    PGHSdlApp sdlapp = GetGHSdlApp();
    PNHWinApp ghapp = GetNHApp();
        
    //SDL_Init(SDL_INIT_VIDEO);
    SDL_GL_SetAttribute(SDL_GL_DOUBLEBUFFER, 1);
    SDL_GL_SetAttribute(SDL_GL_DEPTH_SIZE, 24);
    SDL_GL_SetAttribute(SDL_GL_STENCIL_SIZE, 8);
    SDL_GL_SetAttribute(SDL_GL_CONTEXT_MAJOR_VERSION, 2);
    SDL_GL_SetAttribute(SDL_GL_CONTEXT_MINOR_VERSION, 2);
    sdlapp->win = SDL_CreateWindow("Demo",
        SDL_WINDOWPOS_CENTERED, SDL_WINDOWPOS_CENTERED,
        WINDOW_WIDTH, WINDOW_HEIGHT, SDL_WINDOW_OPENGL | SDL_WINDOW_SHOWN | SDL_WINDOW_ALLOW_HIGHDPI | SDL_WINDOW_FULLSCREEN_DESKTOP);
    sdlapp->glContext = SDL_GL_CreateContext(sdlapp->win);
    SDL_GetWindowSize(sdlapp->win, &sdlapp->win_width, &sdlapp->win_height);

    windowprocs.win_raw_print = sdl_raw_print;
    windowprocs.win_raw_print_bold = sdl_raw_print_bold;
    windowprocs.win_wait_synch = sdl_wait_synch;

    init_nuklear(ghapp->hApp, sdlapp);
    //Other initializations
    init_resource_fonts();
    mswin_nh_input_init();

    /* Do main menu here */
    enum nuklear_main_menu_command ncmd = nuklear_main_menu(sdlapp);

    if (ncmd != MAIN_MENU_START_GAME)
        sdl_bail((char*)0);

    /* set it to WIN_ERR so we can detect attempts to
       use this ID before it is inialized */
    WIN_MAP = WIN_ERR;

    /* Read Windows settings from the reqistry */
    /* First set safe defaults */
    GetNHApp()->regMainMinX = CW_USEDEFAULT;
    sdl_read_reg();
    /* Create the main window */
    GetNHApp()->hMainWnd = mswin_init_main_window();
    if (!GetNHApp()->hMainWnd) {
        panic("Cannot create main window");
		return;
    }

    /* Set menu check mark for interface mode */
    mswin_menu_check_intf_mode();

    /* check default values */
    if (iflags.wc_fontsiz_status < NHFONT_SIZE_MIN
        || iflags.wc_fontsiz_status > NHFONT_SIZE_MAX)
        iflags.wc_fontsiz_status = NHFONT_DEFAULT_SIZE;

    if (iflags.wc_fontsiz_message < NHFONT_SIZE_MIN
        || iflags.wc_fontsiz_message > NHFONT_SIZE_MAX)
        iflags.wc_fontsiz_message = NHFONT_DEFAULT_SIZE;

    if (iflags.wc_fontsiz_text < NHFONT_SIZE_MIN
        || iflags.wc_fontsiz_text > NHFONT_SIZE_MAX)
        iflags.wc_fontsiz_text = NHFONT_DEFAULT_SIZE;

    if (iflags.wc_fontsiz_menu < NHFONT_SIZE_MIN
        || iflags.wc_fontsiz_menu > NHFONT_SIZE_MAX)
        iflags.wc_fontsiz_menu = NHFONT_DEFAULT_SIZE;

    if (iflags.wc_align_message == 0)
        iflags.wc_align_message = ALIGN_TOP;
    if (iflags.wc_align_status == 0)
        iflags.wc_align_status = ALIGN_BOTTOM;
    if (iflags.wc_scroll_margin == 0)
        iflags.wc_scroll_margin = DEF_CLIPAROUND_MARGIN;
    if (iflags.wc_scroll_amount == 0)
        iflags.wc_scroll_amount = DEF_CLIPAROUND_AMOUNT;
    if (iflags.wc_tile_width == 0)
        iflags.wc_tile_width = TILE_X;
    if (iflags.wc_tile_height == 0)
        iflags.wc_tile_height = TILE_Y;

    if (iflags.wc_vary_msgcount == 0)
        iflags.wc_vary_msgcount = 4;

    /* force tabs in menus */
    iflags.menu_tab_sep = 1;

    /* force toptenwin to be true.  toptenwin is the option that decides
     * whether to
     * write output to a window or stdout.  stdout doesn't make sense on
     * Windows
     * non-console applications
     */
    iflags.toptenwin = 1;
    set_option_mod_status("toptenwin", SET_IN_FILE);
    //set_option_mod_status("perm_invent", SET_IN_FILE);
    set_option_mod_status("mouse_support", SET_IN_GAME);

    /* initialize map tiles bitmap */
    sdl_initMapTiles();

    /* set tile-related options to readonly */
    set_wc_option_mod_status(WC_TILE_WIDTH | WC_TILE_HEIGHT | WC_TILE_FILE,
                             DISP_IN_GAME);

    /* set font-related options to change in the game */
    set_wc_option_mod_status(
        WC_HILITE_PET | WC_ALIGN_MESSAGE | WC_ALIGN_STATUS | WC_SCROLL_AMOUNT
            | WC_SCROLL_MARGIN | WC_MAP_MODE | WC_FONT_MESSAGE
            | WC_FONT_STATUS | WC_FONT_MENU | WC_FONT_TEXT
            | WC_FONTSIZ_MESSAGE | WC_FONTSIZ_STATUS | WC_FONTSIZ_MENU
            | WC_FONTSIZ_TEXT | WC_VARY_MSGCOUNT,
        SET_IN_GAME);

    sdl_color_from_string(iflags.wc_foregrnd_menu, &menu_fg_brush,
                            &menu_fg_color);
    sdl_color_from_string(iflags.wc_foregrnd_message, &message_fg_brush,
                            &message_fg_color);
    sdl_color_from_string(iflags.wc_foregrnd_status, &status_fg_brush,
                            &status_fg_color);
    sdl_color_from_string(iflags.wc_foregrnd_text, &text_fg_brush,
                            &text_fg_color);
    sdl_color_from_string(iflags.wc_backgrnd_menu, &menu_bg_brush,
                            &menu_bg_color);
    sdl_color_from_string(iflags.wc_backgrnd_message, &message_bg_brush,
                            &message_bg_color);
    sdl_color_from_string(iflags.wc_backgrnd_status, &status_bg_brush,
                            &status_bg_color);
    sdl_color_from_string(iflags.wc_backgrnd_text, &text_bg_brush,
                            &text_bg_color);

    HINSTANCE hInstance = (HINSTANCE)GetModuleHandle(NULL);
    int rid[2] = { IDR_RCDATA_MASTER, IDR_RCDATA_STRINGS };
    char* bfilename[2] = { 0, 0 };
    bfilename[0] = iflags.wc2_master_bank_file;
    bfilename[1] = iflags.wc2_master_strings_bank_file;
    for (int i = 0; i < 2; i++)
    {
        if (bfilename[i])
        {
            if (!load_fmod_bank_from_file(hInstance, bfilename[i]))
            {
                impossible("cannot load FMOD sound bank %d from file", i);
                /* Continue to loading from resource */
            }
            else
                continue;
        }

        if (!load_fmod_bank_from_resource(hInstance, rid[i]))
        {
            panic("cannot load FMOD sound bank %d from resource", i);
            return;
        }
    }

    if (iflags.wc_splash_screen)
        mswin_display_splash_window(FALSE);

    iflags.window_inited = TRUE;
}

/* Do a window-port specific player type selection. If player_selection()
   offers a Quit option, it is its responsibility to clean up and terminate
   the process. You need to fill in pl_character[0].
*/
void
sdl_player_selection(void)
{
    logDebug("sdl_player_selection()\n");

    if (iflags.wc_player_selection == VIA_DIALOG) {
        /* pick player type randomly (use pre-selected
         * role/race/gender/alignment) */
        if (flags.randomall) {
            if (flags.initrole < 0) {
                flags.initrole = pick_role(flags.initrace, flags.initgend,
                                           flags.initalign, PICK_RANDOM);
                if (flags.initrole < 0) {
                    raw_print("Incompatible role!");
                    flags.initrole = randrole(FALSE);
                }
            }

            if (flags.initrace < 0
                || !validrace(flags.initrole, flags.initrace)) {
                flags.initrace = pick_race(flags.initrole, flags.initgend,
                                           flags.initalign, PICK_RANDOM);
                if (flags.initrace < 0) {
                    raw_print("Incompatible race!");
                    flags.initrace = randrace(flags.initrole);
                }
            }

            if (flags.initgend < 0
                || !validgend(flags.initrole, flags.initrace,
                              flags.initgend)) {
                flags.initgend = pick_gend(flags.initrole, flags.initrace,
                                           flags.initalign, PICK_RANDOM);
                if (flags.initgend < 0) {
                    raw_print("Incompatible gender!");
                    flags.initgend = randgend(flags.initrole, flags.initrace);
                }
            }

            if (flags.initalign < 0
                || !validalign(flags.initrole, flags.initrace,
                               flags.initalign)) {
                flags.initalign = pick_align(flags.initrole, flags.initrace,
                                             flags.initgend, PICK_RANDOM);
                if (flags.initalign < 0) {
                    raw_print("Incompatible alignment!");
                    flags.initalign =
                        randalign(flags.initrole, flags.initrace);
                }
            }
        } else {
            /* select a role */
            if (!nuklear_player_selection(GetGHSdlApp())) {
                sdl_bail(0);
            }
        }
    } else { /* iflags.wc_player_selection == VIA_PROMPTS */
        prompt_for_player_selection();
    }
}

void
prompt_for_player_selection(void)
{
    int i, k, n;
    char pick4u = 'n', thisch, lastch = 0;
    char pbuf[QBUFSZ], plbuf[QBUFSZ];
    winid win;
    anything any;
    menu_item *selected = 0;
    DWORD box_result;

    logDebug("prompt_for_player_selection()\n");

    /* prevent an unnecessary prompt */
    rigid_role_checks();

    /* Should we randomly pick for the player? */
    if (!flags.randomall
        && (flags.initrole == ROLE_NONE || flags.initrace == ROLE_NONE
            || flags.initgend == ROLE_NONE || flags.initalign == ROLE_NONE)) {
        /* int echoline; */
        char *prompt = build_plselection_prompt(
            pbuf, QBUFSZ, flags.initrole, flags.initrace, flags.initgend,
            flags.initalign);

        /* tty_putstr(BASE_WINDOW, 0, ""); */
        /* echoline = wins[BASE_WINDOW]->cury; */
        box_result = SDL_NHMessageBox(NULL, prompt, MB_YESNOCANCEL | MB_DEFBUTTON1
                                                    | MB_ICONQUESTION);
        pick4u =
            (box_result == IDYES) ? 'y' : (box_result == IDNO) ? 'n' : '\033';
        /* tty_putstr(BASE_WINDOW, 0, prompt); */
        do {
            /* pick4u = lowc(readchar()); */
            if (index(quitchars, pick4u))
                pick4u = 'y';
        } while (!index(ynqchars, pick4u));
        if ((int) strlen(prompt) + 1 < CO) {
            /* Echo choice and move back down line */
            /* tty_putsym(BASE_WINDOW, (int)strlen(prompt)+1, echoline,
             * pick4u); */
            /* tty_putstr(BASE_WINDOW, 0, ""); */
        } else
            /* Otherwise it's hard to tell where to echo, and things are
             * wrapping a bit messily anyway, so (try to) make sure the next
             * question shows up well and doesn't get wrapped at the
             * bottom of the window.
             */
            /* tty_clear_nhwindow(BASE_WINDOW) */;

        if (pick4u != 'y' && pick4u != 'n') {
        give_up: /* Quit */
            if (selected)
                free((genericptr_t) selected);
            sdl_bail((char *) 0);
            /*NOTREACHED*/
            return;
        }
    }

    (void) root_plselection_prompt(plbuf, QBUFSZ - 1, flags.initrole,
                                   flags.initrace, flags.initgend,
                                   flags.initalign);

    /* Select a role, if necessary */
    /* we'll try to be compatible with pre-selected race/gender/alignment,
     * but may not succeed */
    if (flags.initrole < 0) {
        char rolenamebuf[QBUFSZ];
        /* Process the choice */
        if (pick4u == 'y' || flags.initrole == ROLE_RANDOM
            || flags.randomall) {
            /* Pick a random role */
            flags.initrole = pick_role(flags.initrace, flags.initgend,
                                       flags.initalign, PICK_RANDOM);
            if (flags.initrole < 0) {
                /* tty_putstr(BASE_WINDOW, 0, "Incompatible role!"); */
                flags.initrole = randrole(FALSE);
            }
        } else {
            /* tty_clear_nhwindow(BASE_WINDOW); */
            /* tty_putstr(BASE_WINDOW, 0, "Choosing Character's Role"); */
            /* Prompt for a role */
            win = create_nhwindow(NHW_MENU);
            start_menu(win);
            any = zeroany; /* zero out all bits */
            for (i = 0; roles[i].name.m; i++) {
                if (ok_role(i, flags.initrace, flags.initgend,
                            flags.initalign)) {
                    any.a_int = i + 1; /* must be non-zero */
                    thisch = lowc(roles[i].name.m[0]);
                    if (thisch == lastch)
                        thisch = highc(thisch);
                    if (flags.initgend != ROLE_NONE
                        && flags.initgend != ROLE_RANDOM) {
                        if (flags.initgend == 1 && roles[i].name.f)
                            Strcpy(rolenamebuf, roles[i].name.f);
                        else
                            Strcpy(rolenamebuf, roles[i].name.m);
                    } else {
                        if (roles[i].name.f) {
                            Strcpy(rolenamebuf, roles[i].name.m);
                            Strcat(rolenamebuf, "/");
                            Strcat(rolenamebuf, roles[i].name.f);
                        } else
                            Strcpy(rolenamebuf, roles[i].name.m);
                    }
                    add_menu(win, NO_GLYPH, &any, thisch, 0, ATR_NONE,
                             an(rolenamebuf), MENU_UNSELECTED);
                    lastch = thisch;
                }
            }
            any.a_int = pick_role(flags.initrace, flags.initgend,
                                  flags.initalign, PICK_RANDOM) + 1;
            if (any.a_int == 0) /* must be non-zero */
                any.a_int = randrole(FALSE) + 1;
            add_menu(win, NO_GLYPH, &any, '*', 0, ATR_NONE, "Random",
                     MENU_UNSELECTED);
            any.a_int = i + 1; /* must be non-zero */
            add_menu(win, NO_GLYPH, &any, 'q', 0, ATR_NONE, "Quit",
                     MENU_UNSELECTED);
            Sprintf(pbuf, "Pick a role for your %s", plbuf);
            end_menu(win, pbuf);
            n = select_menu(win, PICK_ONE, &selected);
            destroy_nhwindow(win);

            /* Process the choice */
            if (n != 1 || selected[0].item.a_int == any.a_int)
                goto give_up; /* Selected quit */

            flags.initrole = selected[0].item.a_int - 1;
            free((genericptr_t) selected), selected = 0;
        }
        (void) root_plselection_prompt(plbuf, QBUFSZ - 1, flags.initrole,
                                       flags.initrace, flags.initgend,
                                       flags.initalign);
    }

    /* Select a race, if necessary */
    /* force compatibility with role, try for compatibility with
     * pre-selected gender/alignment */
    if (flags.initrace < 0 || !validrace(flags.initrole, flags.initrace)) {
        /* pre-selected race not valid */
        if (pick4u == 'y' || flags.initrace == ROLE_RANDOM
            || flags.randomall) {
            flags.initrace = pick_race(flags.initrole, flags.initgend,
                                       flags.initalign, PICK_RANDOM);
            if (flags.initrace < 0) {
                /* tty_putstr(BASE_WINDOW, 0, "Incompatible race!"); */
                flags.initrace = randrace(flags.initrole);
            }
        } else { /* pick4u == 'n' */
            /* Count the number of valid races */
            n = 0; /* number valid */
            k = 0; /* valid race */
            for (i = 0; races[i].noun; i++) {
                if (ok_race(flags.initrole, i, flags.initgend,
                            flags.initalign)) {
                    n++;
                    k = i;
                }
            }
            if (n == 0) {
                for (i = 0; races[i].noun; i++) {
                    if (validrace(flags.initrole, i)) {
                        n++;
                        k = i;
                    }
                }
            }

            /* Permit the user to pick, if there is more than one */
            if (n > 1) {
                /* tty_clear_nhwindow(BASE_WINDOW); */
                /* tty_putstr(BASE_WINDOW, 0, "Choosing Race"); */
                win = create_nhwindow(NHW_MENU);
                start_menu(win);
                any = zeroany; /* zero out all bits */
                for (i = 0; races[i].noun; i++)
                    if (ok_race(flags.initrole, i, flags.initgend,
                                flags.initalign)) {
                        any.a_int = i + 1; /* must be non-zero */
                        add_menu(win, NO_GLYPH, &any, races[i].noun[0], 0,
                                 ATR_NONE, races[i].noun, MENU_UNSELECTED);
                    }
                any.a_int = pick_race(flags.initrole, flags.initgend,
                                      flags.initalign, PICK_RANDOM) + 1;
                if (any.a_int == 0) /* must be non-zero */
                    any.a_int = randrace(flags.initrole) + 1;
                add_menu(win, NO_GLYPH, &any, '*', 0, ATR_NONE, "Random",
                         MENU_UNSELECTED);
                any.a_int = i + 1; /* must be non-zero */
                add_menu(win, NO_GLYPH, &any, 'q', 0, ATR_NONE, "Quit",
                         MENU_UNSELECTED);
                Sprintf(pbuf, "Pick the race of your %s", plbuf);
                end_menu(win, pbuf);
                n = select_menu(win, PICK_ONE, &selected);
                destroy_nhwindow(win);
                if (n != 1 || selected[0].item.a_int == any.a_int)
                    goto give_up; /* Selected quit */

                k = selected[0].item.a_int - 1;
                free((genericptr_t) selected), selected = 0;
            }
            flags.initrace = k;
        }
        (void) root_plselection_prompt(plbuf, QBUFSZ - 1, flags.initrole,
                                       flags.initrace, flags.initgend,
                                       flags.initalign);
    }

    /* Select a gender, if necessary */
    /* force compatibility with role/race, try for compatibility with
     * pre-selected alignment */
    if (flags.initgend < 0
        || !validgend(flags.initrole, flags.initrace, flags.initgend)) {
        /* pre-selected gender not valid */
        if (pick4u == 'y' || flags.initgend == ROLE_RANDOM
            || flags.randomall) {
            flags.initgend = pick_gend(flags.initrole, flags.initrace,
                                       flags.initalign, PICK_RANDOM);
            if (flags.initgend < 0) {
                /* tty_putstr(BASE_WINDOW, 0, "Incompatible gender!"); */
                flags.initgend = randgend(flags.initrole, flags.initrace);
            }
        } else { /* pick4u == 'n' */
            /* Count the number of valid genders */
            n = 0; /* number valid */
            k = 0; /* valid gender */
            for (i = 0; i < ROLE_GENDERS; i++) {
                if (ok_gend(flags.initrole, flags.initrace, i,
                            flags.initalign)) {
                    n++;
                    k = i;
                }
            }
            if (n == 0) {
                for (i = 0; i < ROLE_GENDERS; i++) {
                    if (validgend(flags.initrole, flags.initrace, i)) {
                        n++;
                        k = i;
                    }
                }
            }

            /* Permit the user to pick, if there is more than one */
            if (n > 1) {
                /* tty_clear_nhwindow(BASE_WINDOW); */
                /* tty_putstr(BASE_WINDOW, 0, "Choosing Gender"); */
                win = create_nhwindow(NHW_MENU);
                start_menu(win);
                any = zeroany; /* zero out all bits */
                for (i = 0; i < ROLE_GENDERS; i++)
                    if (ok_gend(flags.initrole, flags.initrace, i,
                                flags.initalign)) {
                        any.a_int = i + 1;
                        add_menu(win, NO_GLYPH, &any, genders[i].adj[0], 0,
                                 ATR_NONE, genders[i].adj, MENU_UNSELECTED);
                    }
                any.a_int = pick_gend(flags.initrole, flags.initrace,
                                      flags.initalign, PICK_RANDOM) + 1;
                if (any.a_int == 0) /* must be non-zero */
                    any.a_int = randgend(flags.initrole, flags.initrace) + 1;
                add_menu(win, NO_GLYPH, &any, '*', 0, ATR_NONE, "Random",
                         MENU_UNSELECTED);
                any.a_int = i + 1; /* must be non-zero */
                add_menu(win, NO_GLYPH, &any, 'q', 0, ATR_NONE, "Quit",
                         MENU_UNSELECTED);
                Sprintf(pbuf, "Pick the gender of your %s", plbuf);
                end_menu(win, pbuf);
                n = select_menu(win, PICK_ONE, &selected);
                destroy_nhwindow(win);
                if (n != 1 || selected[0].item.a_int == any.a_int)
                    goto give_up; /* Selected quit */

                k = selected[0].item.a_int - 1;
                free((genericptr_t) selected), selected = 0;
            }
            flags.initgend = k;
        }
        (void) root_plselection_prompt(plbuf, QBUFSZ - 1, flags.initrole,
                                       flags.initrace, flags.initgend,
                                       flags.initalign);
    }

    /* Select an alignment, if necessary */
    /* force compatibility with role/race/gender */
    if (flags.initalign < 0
        || !validalign(flags.initrole, flags.initrace, flags.initalign)) {
        /* pre-selected alignment not valid */
        if (pick4u == 'y' || flags.initalign == ROLE_RANDOM
            || flags.randomall) {
            flags.initalign = pick_align(flags.initrole, flags.initrace,
                                         flags.initgend, PICK_RANDOM);
            if (flags.initalign < 0) {
                /* tty_putstr(BASE_WINDOW, 0, "Incompatible alignment!"); */
                flags.initalign = randalign(flags.initrole, flags.initrace);
            }
        } else { /* pick4u == 'n' */
            /* Count the number of valid alignments */
            n = 0; /* number valid */
            k = 0; /* valid alignment */
            for (i = 0; i < ROLE_ALIGNS; i++) {
                if (ok_align(flags.initrole, flags.initrace, flags.initgend,
                             i)) {
                    n++;
                    k = i;
                }
            }
            if (n == 0) {
                for (i = 0; i < ROLE_ALIGNS; i++) {
                    if (validalign(flags.initrole, flags.initrace, i)) {
                        n++;
                        k = i;
                    }
                }
            }

            /* Permit the user to pick, if there is more than one */
            if (n > 1) {
                /* tty_clear_nhwindow(BASE_WINDOW); */
                /* tty_putstr(BASE_WINDOW, 0, "Choosing Alignment"); */
                win = create_nhwindow(NHW_MENU);
                start_menu(win);
                any = zeroany; /* zero out all bits */
                for (i = 0; i < ROLE_ALIGNS; i++)
                    if (ok_align(flags.initrole, flags.initrace,
                                 flags.initgend, i)) {
                        any.a_int = i + 1;
                        add_menu(win, NO_GLYPH, &any, aligns[i].adj[0], 0,
                                 ATR_NONE, aligns[i].adj, MENU_UNSELECTED);
                    }
                any.a_int = pick_align(flags.initrole, flags.initrace,
                                       flags.initgend, PICK_RANDOM) + 1;
                if (any.a_int == 0) /* must be non-zero */
                    any.a_int = randalign(flags.initrole, flags.initrace) + 1;
                add_menu(win, NO_GLYPH, &any, '*', 0, ATR_NONE, "Random",
                         MENU_UNSELECTED);
                any.a_int = i + 1; /* must be non-zero */
                add_menu(win, NO_GLYPH, &any, 'q', 0, ATR_NONE, "Quit",
                         MENU_UNSELECTED);
                Sprintf(pbuf, "Pick the alignment of your %s", plbuf);
                end_menu(win, pbuf);
                n = select_menu(win, PICK_ONE, &selected);
                destroy_nhwindow(win);
                if (n != 1 || selected[0].item.a_int == any.a_int)
                    goto give_up; /* Selected quit */

                k = selected[0].item.a_int - 1;
                free((genericptr_t) selected), selected = 0;
            }
            flags.initalign = k;
        }
    }
    /* Success! */
    /* tty_display_nhwindow(BASE_WINDOW, FALSE); */
}

/* Ask the user for a player name. */
void
sdl_askname(void)
{
    logDebug("sdl_askname()\n");

    if (mswin_getlin_window("Who are you?", plname, PL_NSIZ) == IDCANCEL) {
        sdl_bail("bye-bye");
        /* not reached */
    }
}

/* Does window event processing (e.g. exposure events).
   A noop for the tty and X window-ports.
*/
void
sdl_get_nh_event(void)
{
    MSG msg;

    logDebug("sdl_get_nh_event()\n");

    while (PeekMessage(&msg, NULL, 0, 0, PM_REMOVE) != 0) {
        if (!TranslateAccelerator(msg.hwnd, GetNHApp()->hAccelTable, &msg)) {
            TranslateMessage(&msg);
            DispatchMessage(&msg);
        }
    }
    return;
}

/* Exits the window system.  This should dismiss all windows,
   except the "window" used for raw_print().  str is printed if possible.
*/
void
sdl_exit_nhwindows(const char *str)
{
    logDebug("sdl_exit_nhwindows(%s)\n", str);

    /* Write Window settings to the registry */
    sdl_write_reg();

    /* set things back to failsafes */
    windowprocs = *get_safe_procs(0);

    /* and make sure there is still a way to communicate something */
    windowprocs.win_raw_print = mswin_raw_print;
    windowprocs.win_raw_print_bold = mswin_raw_print_bold;
    windowprocs.win_wait_synch = mswin_wait_synch;
}

/* Prepare the window to be suspended. */
void
sdl_suspend_nhwindows(const char *str)
{
    logDebug("sdl_suspend_nhwindows(%s)\n", str);

    return;
}

/* Restore the windows after being suspended. */
void
sdl_resume_nhwindows()
{
    logDebug("sdl_resume_nhwindows()\n");

    return;
}

/*  Create a window of type "type" which can be
        NHW_MESSAGE     (top line)
        NHW_STATUS      (bottom lines)
        NHW_MAP         (main dungeon)
        NHW_MENU        (inventory or other "corner" windows)
        NHW_TEXT        (help/text, full screen paged window)
*/
winid
sdl_create_nhwindow(int type)
{
    winid i = 0;
    MSNHMsgAddWnd data;

    logDebug("sdl_create_nhwindow(%d)\n", type);

    /* Return the next available winid
     */

    for (i = 1; i < MAXWINDOWS; i++)
        if (GetNHApp()->windowlist[i].win == NULL
            && !GetNHApp()->windowlist[i].dead)
            break;
	if (i == MAXWINDOWS)
	{
		panic("ERROR:  No windows available...\n");
		return 0;
	}

    switch (type) {
    case NHW_MAP: {
        GetNHApp()->windowlist[i].win = mswin_init_map_window();
        GetNHApp()->windowlist[i].type = type;
        GetNHApp()->windowlist[i].dead = 0;
        break;
    }
    case NHW_MESSAGE: {
        GetNHApp()->windowlist[i].win = mswin_init_message_window();
        GetNHApp()->windowlist[i].type = type;
        GetNHApp()->windowlist[i].dead = 0;
        break;
    }
    case NHW_STATUS: {
        GetNHApp()->windowlist[i].win = mswin_init_status_window();
        GetNHApp()->windowlist[i].type = type;
        GetNHApp()->windowlist[i].dead = 0;
        break;
    }
    case NHW_MENU: {
        GetNHApp()->windowlist[i].win = NULL; // will create later
        GetNHApp()->windowlist[i].type = type;
        GetNHApp()->windowlist[i].dead = 1;
        break;
    }
    case NHW_TEXT: {
        GetNHApp()->windowlist[i].win = mswin_init_text_window();
        GetNHApp()->windowlist[i].type = type;
        GetNHApp()->windowlist[i].dead = 0;
        break;
    }
    }

    ZeroMemory(&data, sizeof(data));
    data.wid = i;
    SendMessage(GetNHApp()->hMainWnd, WM_MSNH_COMMAND,
                (WPARAM) MSNH_MSG_ADDWND, (LPARAM) &data);
    return i;
}

/* Clear the given window, when asked to. */
void
sdl_clear_nhwindow(winid wid)
{
    logDebug("sdl_clear_nhwindow(%d)\n", wid);

    if ((wid >= 0) && (wid < MAXWINDOWS)
        && (GetNHApp()->windowlist[wid].win != NULL)) {
        if (GetNHApp()->windowlist[wid].type == NHW_MAP) {
            if (Is_rogue_level(&u.uz))
                if (iflags.wc_map_mode == MAP_MODE_ASCII_FIT_TO_SCREEN ||
                    iflags.wc_map_mode == MAP_MODE_TILES_FIT_TO_SCREEN)

                    mswin_map_mode(sdl_hwnd_from_winid(WIN_MAP),
                                   ROGUE_LEVEL_MAP_MODE_FIT_TO_SCREEN);
                else
                    mswin_map_mode(sdl_hwnd_from_winid(WIN_MAP),
                                   ROGUE_LEVEL_MAP_MODE);
            else
                mswin_map_mode(sdl_hwnd_from_winid(WIN_MAP),
                               iflags.wc_map_mode);
        }

        SendMessage(GetNHApp()->windowlist[wid].win, WM_MSNH_COMMAND,
                    (WPARAM) MSNH_MSG_CLEAR_WINDOW, (LPARAM) NULL);
    }
}

/* -- Display the window on the screen.  If there is data
                   pending for output in that window, it should be sent.
                   If blocking is TRUE, display_nhwindow() will not
                   return until the data has been displayed on the screen,
                   and acknowledged by the user where appropriate.
                -- All calls are blocking in the tty window-port.
                -- Calling display_nhwindow(WIN_MESSAGE,???) will do a
                   --more--, if necessary, in the tty window-port.
*/
void
sdl_display_nhwindow(winid wid, BOOLEAN_P block)
{
    logDebug("sdl_display_nhwindow(%d, %d)\n", wid, block);
    if (GetNHApp()->windowlist[wid].win != NULL) {
        ShowWindow(GetNHApp()->windowlist[wid].win, SW_SHOW);
        mswin_layout_main_window(GetNHApp()->windowlist[wid].win);
        if (GetNHApp()->windowlist[wid].type == NHW_MENU) {
            MENU_ITEM_P *p;
            mswin_menu_window_select_menu(GetNHApp()->windowlist[wid].win,
                                          PICK_NONE, &p, TRUE);
        }
        if (GetNHApp()->windowlist[wid].type == NHW_TEXT) {
            mswin_display_text_window(GetNHApp()->windowlist[wid].win);
        }
        if (GetNHApp()->windowlist[wid].type == NHW_RIP) {
            mswin_display_RIP_window(GetNHApp()->windowlist[wid].win);
        } else {
            if (!block) {
                UpdateWindow(GetNHApp()->windowlist[wid].win);
            } else {
                if (GetNHApp()->windowlist[wid].type == NHW_MAP) {
                    (void) sdl_nhgetch();
                }
            }
        }
        SetFocus(GetNHApp()->hMainWnd);
    }
}

HWND
sdl_hwnd_from_winid(winid wid)
{
    if (wid >= 0 && wid < MAXWINDOWS) {
        return GetNHApp()->windowlist[wid].win;
    } else {
        return NULL;
    }
}

winid
sdl_winid_from_handle(HWND hWnd)
{
    winid i = 0;

    for (i = 1; i < MAXWINDOWS; i++)
        if (GetNHApp()->windowlist[i].win == hWnd)
            return i;
    return -1;
}

winid
sdl_winid_from_type(int type)
{
    winid i = 0;

    for (i = 1; i < MAXWINDOWS; i++)
        if (GetNHApp()->windowlist[i].type == type)
            return i;
    return -1;
}

void
sdl_window_mark_dead(winid wid)
{
    if (wid >= 0 && wid < MAXWINDOWS) {
        GetNHApp()->windowlist[wid].win = NULL;
        GetNHApp()->windowlist[wid].dead = 1;
    }
}

/* Destroy will dismiss the window if the window has not
 * already been dismissed.
*/
void
sdl_destroy_nhwindow(winid wid)
{
    logDebug("sdl_destroy_nhwindow(%d)\n", wid);

    if ((GetNHApp()->windowlist[wid].type == NHW_MAP)
        || (GetNHApp()->windowlist[wid].type == NHW_MESSAGE)
        || (GetNHApp()->windowlist[wid].type == NHW_STATUS)) {
        /* main windows is going to take care of those */
        return;
    }

    if (wid != -1) {
        if (!GetNHApp()->windowlist[wid].dead
            && GetNHApp()->windowlist[wid].win != NULL)
            DestroyWindow(GetNHApp()->windowlist[wid].win);
        GetNHApp()->windowlist[wid].win = NULL;
        GetNHApp()->windowlist[wid].type = 0;
        GetNHApp()->windowlist[wid].dead = 0;
    }
}

/* Next output to window will start at (x,y), also moves
 displayable cursor to (x,y).  For backward compatibility,
 1 <= x < cols, 0 <= y < rows, where cols and rows are
 the size of window.
*/
void
sdl_curs(winid wid, int x, int y)
{
    logDebug("sdl_curs(%d, %d, %d)\n", wid, x, y);

    if ((wid >= 0) && (wid < MAXWINDOWS)
        && (GetNHApp()->windowlist[wid].win != NULL)) {
        MSNHMsgCursor data;
        data.x = x;
        data.y = y;
        SendMessage(GetNHApp()->windowlist[wid].win, WM_MSNH_COMMAND,
                    (WPARAM) MSNH_MSG_CURSOR, (LPARAM) &data);
    }
}

/*
putstr(window, attr, str)
                -- Print str on the window with the given attribute.  Only
                   printable ASCII characters (040-0126) must be supported.
                   Multiple putstr()s are output on separate lines.
Attributes
                   can be one of
                        ATR_NONE (or 0)
                        ATR_ULINE
                        ATR_BOLD
                        ATR_BLINK
                        ATR_INVERSE
                   If a window-port does not support all of these, it may map
                   unsupported attributes to a supported one (e.g. map them
                   all to ATR_INVERSE).  putstr() may compress spaces out of
                   str, break str, or truncate str, if necessary for the
                   display.  Where putstr() breaks a line, it has to clear
                   to end-of-line.
                -- putstr should be implemented such that if two putstr()s
                   are done consecutively the user will see the first and
                   then the second.  In the tty port, pline() achieves this
                   by calling more() or displaying both on the same line.
*/
void
sdl_putstr(winid wid, int attr, const char *text)
{
    logDebug("sdl_putstr(%d, %d, %s)\n", wid, attr, text);

    sdl_putstr_ex(wid, attr, text, 0);
}

void
sdl_putstr_ex(winid wid, int attr, const char *text, int app)
{
    if ((wid >= 0) && (wid < MAXWINDOWS)) {
        if (GetNHApp()->windowlist[wid].win == NULL
            && GetNHApp()->windowlist[wid].type == NHW_MENU) {
            GetNHApp()->windowlist[wid].win =
                mswin_init_menu_window(MENU_TYPE_TEXT);
            GetNHApp()->windowlist[wid].dead = 0;
        }

        if (GetNHApp()->windowlist[wid].win != NULL) {
            MSNHMsgPutstr data;
            ZeroMemory(&data, sizeof(data));
            data.attr = attr;
            data.text = text;
            data.append = app;
            SendMessage(GetNHApp()->windowlist[wid].win, WM_MSNH_COMMAND,
                        (WPARAM) MSNH_MSG_PUTSTR, (LPARAM) &data);
        }
        /* yield a bit so it gets done immediately */
        sdl_get_nh_event();
    } else {
        // build text to display later in message box
        GetNHApp()->saved_text =
            realloc(GetNHApp()->saved_text,
                    strlen(text) + strlen(GetNHApp()->saved_text) + 1);
        strcat(GetNHApp()->saved_text, text);
    }
}

/* Display the file named str.  Complain about missing files
                   iff complain is TRUE.
*/
void
sdl_display_file(const char *filename, BOOLEAN_P must_exist)
{
    dlb *f;
    TCHAR wbuf[BUFSZ];

    logDebug("sdl_display_file(%s, %d)\n", filename, must_exist);

    f = dlb_fopen(filename, RDTMODE);
    if (!f) {
        if (must_exist) {
            TCHAR message[90];
            _stprintf(message, TEXT("Warning! Could not find file: %s\n"),
                      NH_A2W(filename, wbuf, sizeof(wbuf)));
            SDL_NHMessageBox(GetNHApp()->hMainWnd, message,
                         MB_OK | MB_ICONEXCLAMATION);
        }
    } else {
        winid text;
        char line[LLEN];

        text = sdl_create_nhwindow(NHW_TEXT);

        while (dlb_fgets(line, LLEN, f)) {
            size_t len;
            len = strlen(line);
            if (line[len - 1] == '\n')
                line[len - 1] = '\x0';
            sdl_putstr(text, ATR_NONE, line);
        }
        (void) dlb_fclose(f);

        sdl_display_nhwindow(text, 1);
        sdl_destroy_nhwindow(text);
    }
}

/* Start using window as a menu.  You must call start_menu()
   before add_menu().  After calling start_menu() you may not
   putstr() to the window.  Only windows of type NHW_MENU may
   be used for menus.
*/
void
sdl_start_menu(winid wid)
{
    logDebug("sdl_start_menu(%d)\n", wid);
    if ((wid >= 0) && (wid < MAXWINDOWS)) {
        if (GetNHApp()->windowlist[wid].win == NULL
            && GetNHApp()->windowlist[wid].type == NHW_MENU) {
            GetNHApp()->windowlist[wid].win =
                mswin_init_menu_window(MENU_TYPE_MENU);
            GetNHApp()->windowlist[wid].dead = 0;
        }

        if (GetNHApp()->windowlist[wid].win != NULL) {
            SendMessage(GetNHApp()->windowlist[wid].win, WM_MSNH_COMMAND,
                        (WPARAM) MSNH_MSG_STARTMENU, (LPARAM) NULL);
        }
    }
}

/*
add_menu(windid window, int glyph, const anything identifier,
                                char accelerator, char groupacc,
                                int attr, char *str, boolean preselected)
                -- Add a text line str to the given menu window.  If
identifier
                   is 0, then the line cannot be selected (e.g. a title).
                   Otherwise, identifier is the value returned if the line is
                   selected.  Accelerator is a keyboard key that can be used
                   to select the line.  If the accelerator of a selectable
                   item is 0, the window system is free to select its own
                   accelerator.  It is up to the window-port to make the
                   accelerator visible to the user (e.g. put "a - " in front
                   of str).  The value attr is the same as in putstr().
                   Glyph is an optional glyph to accompany the line.  If
                   window port cannot or does not want to display it, this
                   is OK.  If there is no glyph applicable, then this
                   value will be NO_GLYPH.
                -- All accelerators should be in the range [A-Za-z].
                -- It is expected that callers do not mix accelerator
                   choices.  Either all selectable items have an accelerator
                   or let the window system pick them.  Don't do both.
                -- Groupacc is a group accelerator.  It may be any character
                   outside of the standard accelerator (see above) or a
                   number.  If 0, the item is unaffected by any group
                   accelerator.  If this accelerator conflicts with
                   the menu command (or their user defined aliases), it loses.
                   The menu commands and aliases take care not to interfere
                   with the default object class symbols.
                -- If you want this choice to be preselected when the
                   menu is displayed, set preselected to TRUE.
*/
void
sdl_add_extended_menu(winid wid, int glyph, const ANY_P *identifier, struct extended_menu_info info,
               CHAR_P accelerator, CHAR_P group_accel, int attr,
               const char *str, BOOLEAN_P presel)
{
    logDebug("sdl_add_menu(%d, %d, %p, %c, %c, %d, %s, %d)\n", wid, glyph,
             identifier, (char) accelerator, (char) group_accel, attr, str,
             presel);

    struct obj* otmp = info.object;

    if ((wid >= 0) && (wid < MAXWINDOWS)
        && (GetNHApp()->windowlist[wid].win != NULL)) {
        MSNHMsgAddMenu data;
        ZeroMemory(&data, sizeof(data));
        data.glyph = glyph;
        data.identifier = identifier;
        data.object = otmp;
        data.accelerator = accelerator;
        data.group_accel = group_accel;
        data.attr = attr;
        data.str = str;
        data.presel = presel;

        SendMessage(GetNHApp()->windowlist[wid].win, WM_MSNH_COMMAND,
                    (WPARAM) MSNH_MSG_ADDMENU, (LPARAM) &data);
    }
}

void
sdl_add_menu(winid wid, int glyph, const ANY_P* identifier,
    CHAR_P accelerator, CHAR_P group_accel, int attr,
    const char* str, BOOLEAN_P presel)
{
    sdl_add_extended_menu(wid, glyph, identifier, zeroextendedmenuinfo,
        accelerator, group_accel, attr,
        str, presel);
}

/*
end_menu(window, prompt)
                -- Stop adding entries to the menu and flushes the window
                   to the screen (brings to front?).  Prompt is a prompt
                   to give the user.  If prompt is NULL, no prompt will
                   be printed.
                ** This probably shouldn't flush the window any more (if
                ** it ever did).  That should be select_menu's job.  -dean
*/
void
sdl_end_menu(winid wid, const char *prompt)
{
    logDebug("sdl_end_menu(%d, %s)\n", wid, prompt);
    if ((wid >= 0) && (wid < MAXWINDOWS)
        && (GetNHApp()->windowlist[wid].win != NULL)) {
        MSNHMsgEndMenu data;
        ZeroMemory(&data, sizeof(data));
        data.text = prompt;

        SendMessage(GetNHApp()->windowlist[wid].win, WM_MSNH_COMMAND,
                    (WPARAM) MSNH_MSG_ENDMENU, (LPARAM) &data);
    }
}

/*
int select_menu(windid window, int how, menu_item **selected)
                -- Return the number of items selected; 0 if none were chosen,
                   -1 when explicitly cancelled.  If items were selected, then
                   selected is filled in with an allocated array of menu_item
                   structures, one for each selected line.  The caller must
                   free this array when done with it.  The "count" field
                   of selected is a user supplied count.  If the user did
                   not supply a count, then the count field is filled with
                   -1 (meaning all).  A count of zero is equivalent to not
                   being selected and should not be in the list.  If no items
                   were selected, then selected is NULL'ed out.  How is the
                   mode of the menu.  Three valid values are PICK_NONE,
                   PICK_ONE, and PICK_N, meaning: nothing is selectable,
                   only one thing is selectable, and any number valid items
                   may selected.  If how is PICK_NONE, this function should
                   never return anything but 0 or -1.
                -- You may call select_menu() on a window multiple times --
                   the menu is saved until start_menu() or destroy_nhwindow()
                   is called on the window.
                -- Note that NHW_MENU windows need not have select_menu()
                   called for them. There is no way of knowing whether
                   select_menu() will be called for the window at
                   create_nhwindow() time.
*/
int
sdl_select_menu(winid wid, int how, MENU_ITEM_P **selected)
{
    int nReturned = -1;

    logDebug("sdl_select_menu(%d, %d)\n", wid, how);

    if ((wid >= 0) && (wid < MAXWINDOWS)
        && (GetNHApp()->windowlist[wid].win != NULL)) {
        ShowWindow(GetNHApp()->windowlist[wid].win, SW_SHOW);
        nReturned = mswin_menu_window_select_menu(
            GetNHApp()->windowlist[wid].win, how, selected,
            !(iflags.perm_invent && wid == WIN_INVEN
              && how == PICK_NONE) /* don't activate inventory window if
                                      perm_invent is on */
            );
    }
    return nReturned;
}

/*
    -- Indicate to the window port that the inventory has been changed.
    -- Merely calls display_inventory() for window-ports that leave the
        window up, otherwise empty.
*/
void
sdl_update_inventory()
{
    logDebug("sdl_update_inventory()\n");
    if (iflags.perm_invent && program_state.something_worth_saving
        && iflags.window_inited && WIN_INVEN != WIN_ERR)
        display_inventory(NULL, FALSE, 0);
}

/*
mark_synch()    -- Don't go beyond this point in I/O on any channel until
                   all channels are caught up to here.  Can be an empty call
                   for the moment
*/
void
sdl_mark_synch()
{
    logDebug("sdl_mark_synch()\n");
}

/*
wait_synch()    -- Wait until all pending output is complete (*flush*() for
                   streams goes here).
                -- May also deal with exposure events etc. so that the
                   display is OK when return from wait_synch().
*/
void
sdl_wait_synch()
{
    logDebug("sdl_wait_synch()\n");
    sdl_raw_print_flush();
}

/*
cliparound(x, y)-- Make sure that the user is more-or-less centered on the
                   screen if the playing area is larger than the screen.
                -- This function is only defined if CLIPPING is defined.
*/
void
sdl_cliparound(int x, int y)
{
    winid wid = WIN_MAP;

    logDebug("sdl_cliparound(%d, %d)\n", x, y);

    if ((wid >= 0) && (wid < MAXWINDOWS)
        && (GetNHApp()->windowlist[wid].win != NULL)) {
        MSNHMsgClipAround data;
        data.x = x;
        data.y = y;
        SendMessage(GetNHApp()->windowlist[wid].win, WM_MSNH_COMMAND,
                    (WPARAM) MSNH_MSG_CLIPAROUND, (LPARAM) &data);
    }
}

/*
print_glyph(window, x, y, layers)
                -- Print the glyph at (x,y) on the given window.  Glyphs are
                   integers at the interface, mapped to whatever the window-
                   port wants (symbol, font, color, attributes, ...there's
                   a 1-1 map between glyphs and distinct things on the map).
		-- bkglyph is a background glyph for potential use by some
		   graphical or tiled environments to allow the depiction
		   to fall against a background consistent with the grid 
		   around x,y.
                   
*/
void
sdl_print_glyph(winid wid, XCHAR_P x, XCHAR_P y, struct layer_info layers)
{
    int glyph = layers.glyph;
    int bkglyph = layers.bkglyph;

    logDebug("sdl_print_glyph(%d, %d, %d, %d, %d)\n", wid, x, y, glyph, bkglyph);

    if ((wid >= 0) && (wid < MAXWINDOWS)
        && (GetNHApp()->windowlist[wid].win != NULL)) {
        MSNHMsgPrintGlyph data;

        ZeroMemory(&data, sizeof(data));
        data.x = x;
        data.y = y;
        data.layers = layers;
        SendMessage(GetNHApp()->windowlist[wid].win, WM_MSNH_COMMAND,
                    (WPARAM) MSNH_MSG_PRINT_GLYPH, (LPARAM) &data);
    }
}

/*
 * sdl_raw_print_accumulate() accumulate the given text into
 *   raw_print_strbuf.
 */
void
sdl_raw_print_accumulate(const char * str, boolean bold)
{
    bold; // ignored for now

    if (sdl_raw_print_strbuf.str != NULL) strbuf_append(&sdl_raw_print_strbuf, "\n");
    strbuf_append(&sdl_raw_print_strbuf, str);
}

/*
 * sdl_raw_print_flush() - display any text found in raw_print_strbuf in a
 *   dialog box and clear raw_print_strbuf.
 */
void
sdl_raw_print_flush()
{
    if (sdl_raw_print_strbuf.str != NULL) {
        size_t wlen = strlen(sdl_raw_print_strbuf.str) + 1;
        TCHAR * wbuf = (TCHAR *) alloc(wlen * sizeof(TCHAR));
        if (wbuf != NULL) {
            SDL_NHMessageBox(GetNHApp()->hMainWnd,
                            NH_A2W(sdl_raw_print_strbuf.str, wbuf, wlen),
                            MB_ICONINFORMATION | MB_OK);
            free(wbuf);
        }
        strbuf_empty(&sdl_raw_print_strbuf);
    }
}


/*
raw_print(str)  -- Print directly to a screen, or otherwise guarantee that
                   the user sees str.  raw_print() appends a newline to str.
                   It need not recognize ASCII control characters.  This is
                   used during startup (before windowing system initialization
                   -- maybe this means only error startup messages are raw),
                   for error messages, and maybe other "msg" uses.  E.g.
                   updating status for micros (i.e, "saving").
*/
void
sdl_raw_print(const char *str)
{
    logDebug("sdl_raw_print(%s)\n", str);

    if (str && *str) {
        extern int redirect_stdout;
        if (!redirect_stdout)
            sdl_raw_print_accumulate(str, FALSE);
        else
            fprintf(stdout, "%s", str);
    }
}

/*
raw_print_bold(str)
                -- Like raw_print(), but prints in bold/standout (if
possible).
*/
void
sdl_raw_print_bold(const char *str)
{
    logDebug("sdl_raw_print_bold(%s)\n", str);
    if (str && *str) {
        extern int redirect_stdout;
        if (!redirect_stdout)
            sdl_raw_print_accumulate(str, TRUE);
        else
            fprintf(stdout, "%s", str);
    }
}

/*
int nhgetch()   -- Returns a single character input from the user.
                -- In the tty window-port, nhgetch() assumes that tgetch()
                   will be the routine the OS provides to read a character.
                   Returned character _must_ be non-zero.
*/
int
sdl_nhgetch()
{
    PMSNHEvent event;
    int key = 0;

    logDebug("sdl_nhgetch()\n");
    PGHSdlApp sdlapp = GetGHSdlApp();
    if (sdlapp->running)
    {
        /* Empty normal windows command queue */
        while ((event = mswin_input_pop()) != NULL)
            ;

        key = nuklear_main_loop(sdlapp);
        if (key == -1)
        {
            //sdl_bail((char*)0);
            /* Empty normal windows command queue */
            while ((event = mswin_input_pop()) != NULL)
                ;
            
            key = 0;
        }
    }
    else
    {
        while ((event = mswin_input_pop()) == NULL || event->type != NHEVENT_CHAR)
            mswin_main_loop();
        key = event->kbd.ch;
    }

    return (key);
}

/*
int nh_poskey(int *x, int *y, int *mod)
                -- Returns a single character input from the user or a
                   a positioning event (perhaps from a mouse).  If the
                   return value is non-zero, a character was typed, else,
                   a position in the MAP window is returned in x, y and mod.
                   mod may be one of

                        CLICK_1         -- mouse click type 1
                        CLICK_2         -- mouse click type 2

                   The different click types can map to whatever the
                   hardware supports.  If no mouse is supported, this
                   routine always returns a non-zero character.
*/
int
sdl_nh_poskey(int *x, int *y, int *mod)
{
    PMSNHEvent event;
    int key = 0;

    logDebug("sdl_nh_poskey()\n");

    PGHSdlApp sdlapp = GetGHSdlApp();
    if (sdlapp->running)
    {
        /* Empty normal windows command queue */
        while ((event = mswin_input_pop()) != NULL)
            ;

        key = nuklear_main_loop(sdlapp);
        if (key == -1)
        {
            //sdl_bail((char*)0);

            /* Empty normal windows command queue */
            while ((event = mswin_input_pop()) != NULL)
                ;

            key = 0;
        }
    }
    else
    {
        while ((event = mswin_input_pop()) == NULL)
            mswin_main_loop();

        if (event->type == NHEVENT_MOUSE) {
            if (iflags.wc_mouse_support) {
                *mod = event->ms.mod;
                *x = event->ms.x;
                *y = event->ms.y;
            }
            key = 0;
        }
        else {
            key = event->kbd.ch;
        }
    }
    return (key);
}

/*
nhbell()        -- Beep at user.  [This will exist at least until sounds are
                   redone, since sounds aren't attributable to windows
anyway.]
*/
void
sdl_nhbell()
{
    logDebug("sdl_nhbell()\n");
}

/*
doprev_message()
                -- Display previous messages.  Used by the ^P command.
                -- On the tty-port this scrolls WIN_MESSAGE back one line.
*/
int
sdl_doprev_message()
{
    logDebug("sdl_doprev_message()\n");
    SendMessage(sdl_hwnd_from_winid(WIN_MESSAGE), WM_VSCROLL,
                MAKEWPARAM(SB_LINEUP, 0), (LPARAM) NULL);
    return 0;
}

/*
char yn_function(const char *ques, const char *choices, char default)
                -- Print a prompt made up of ques, choices and default.
                   Read a single character response that is contained in
                   choices or default.  If choices is NULL, all possible
                   inputs are accepted and returned.  This overrides
                   everything else.  The choices are expected to be in
                   lower case.  Entering ESC always maps to 'q', or 'n',
                   in that order, if present in choices, otherwise it maps
                   to default.  Entering any other quit character (SPACE,
                   RETURN, NEWLINE) maps to default.
                -- If the choices string contains ESC, then anything after
                   it is an acceptable response, but the ESC and whatever
                   follows is not included in the prompt.
                -- If the choices string contains a '#' then accept a count.
                   Place this value in the global "yn_number" and return '#'.
                -- This uses the top line in the tty window-port, other
                   ports might use a popup.
*/
char
sdl_yn_function(const char *question, const char *choices, CHAR_P def)
{
    char ch;
    char yn_esc_map = '\033';
    char message[BUFSZ];
    char res_ch[2];
    int createcaret;
    boolean digit_ok, allow_num;

    logDebug("sdl_yn_function(%s, %s, %d)\n", question, choices, def);

    if (WIN_MESSAGE == WIN_ERR && choices == ynchars) {
        char *text =
            realloc(strdup(GetNHApp()->saved_text),
                    strlen(question) + strlen(GetNHApp()->saved_text) + 1);
        DWORD box_result;
        strcat(text, question);
        box_result =
            SDL_NHMessageBox(NULL, NH_W2A(text, message, sizeof(message)),
                         MB_ICONQUESTION | MB_YESNOCANCEL
                             | ((def == 'y') ? MB_DEFBUTTON1
                                             : (def == 'n') ? MB_DEFBUTTON2
                                                            : MB_DEFBUTTON3));
        free(text);
        GetNHApp()->saved_text = strdup("");
        return box_result == IDYES ? 'y' : box_result == IDNO ? 'n' : '\033';
    }

    if (choices) {
        char *cb, choicebuf[QBUFSZ];

        allow_num = (index(choices, '#') != 0);

        Strcpy(choicebuf, choices);
        if ((cb = index(choicebuf, '\033')) != 0) {
            /* anything beyond <esc> is hidden */
            *cb = '\0';
        }
        (void) strncpy(message, question, QBUFSZ - 1);
        message[QBUFSZ - 1] = '\0';
        sprintf(eos(message), " [%s]", choicebuf);
        if (def)
            sprintf(eos(message), " (%c)", def);
        Strcat(message, " ");
        /* escape maps to 'q' or 'n' or default, in that order */
        yn_esc_map =
            (index(choices, 'q') ? 'q' : (index(choices, 'n') ? 'n' : def));
    } else {
        Strcpy(message, question);
        Strcat(message, " ");
    }

    createcaret = 1;
    SendMessage(sdl_hwnd_from_winid(WIN_MESSAGE), WM_MSNH_COMMAND,
                (WPARAM) MSNH_MSG_CARET, (LPARAM) &createcaret);

    sdl_clear_nhwindow(WIN_MESSAGE);
    sdl_putstr(WIN_MESSAGE, ATR_BOLD, message);

    /* Only here if main window is not present */
    ch = 0;
    do {
        ShowCaret(sdl_hwnd_from_winid(WIN_MESSAGE));
        ch = sdl_nhgetch();
        HideCaret(sdl_hwnd_from_winid(WIN_MESSAGE));
        if (choices)
            ch = lowc(ch);
        else
            break; /* If choices is NULL, all possible inputs are accepted and
                      returned. */

        digit_ok = allow_num && digit(ch);
        if (ch == '\033') {
            if (index(choices, 'q'))
                ch = 'q';
            else if (index(choices, 'n'))
                ch = 'n';
            else
                ch = def;
            break;
        } else if (index(quitchars, ch)) {
            ch = def;
            break;
        } else if (!index(choices, ch) && !digit_ok) {
            sdl_nhbell();
            ch = (char) 0;
            /* and try again... */
        } else if (ch == '#' || digit_ok) {
            char z, digit_string[2];
            int n_len = 0;
            long value = 0;
            sdl_putstr_ex(WIN_MESSAGE, ATR_BOLD, ("#"), 1);
            n_len++;
            digit_string[1] = '\0';
            if (ch != '#') {
                digit_string[0] = ch;
                sdl_putstr_ex(WIN_MESSAGE, ATR_BOLD, digit_string, 1);
                n_len++;
                value = ch - '0';
                ch = '#';
            }
            do { /* loop until we get a non-digit */
                z = lowc(readchar());
                if (digit(z)) {
                    value = (10 * value) + (z - '0');
                    if (value < 0)
                        break; /* overflow: try again */
                    digit_string[0] = z;
                    sdl_putstr_ex(WIN_MESSAGE, ATR_BOLD, digit_string, 1);
                    n_len++;
                } else if (z == 'y' || index(quitchars, z)) {
                    if (z == '\033')
                        value = -1; /* abort */
                    z = '\n';       /* break */
                } else if (z == '\b') {
                    if (n_len <= 1) {
                        value = -1;
                        break;
                    } else {
                        value /= 10;
                        sdl_putstr_ex(WIN_MESSAGE, ATR_BOLD, digit_string,
                                        -1);
                        n_len--;
                    }
                } else {
                    value = -1; /* abort */
                    sdl_nhbell();
                    break;
                }
            } while (z != '\n');
            if (value > 0)
                yn_number = value;
            else if (value == 0)
                ch = 'n'; /* 0 => "no" */
            else {        /* remove number from top line, then try again */
                sdl_putstr_ex(WIN_MESSAGE, ATR_BOLD, digit_string, -n_len);
                n_len = 0;
                ch = (char) 0;
            }
        }
    } while (!ch);

    createcaret = 0;
    SendMessage(sdl_hwnd_from_winid(WIN_MESSAGE), WM_MSNH_COMMAND,
                (WPARAM) MSNH_MSG_CARET, (LPARAM) &createcaret);

    /* display selection in the message window */
    if (isprint((uchar) ch) && ch != '#') {
        res_ch[0] = ch;
        res_ch[1] = '\x0';
        sdl_putstr_ex(WIN_MESSAGE, ATR_BOLD, res_ch, 1);
    }

    return ch;
}

/*
getlin(const char *ques, char *input)
            -- Prints ques as a prompt and reads a single line of text,
               up to a newline.  The string entered is returned without the
               newline.  ESC is used to cancel, in which case the string
               "\033\000" is returned.
            -- getlin() must call flush_screen(1) before doing anything.
            -- This uses the top line in the tty window-port, other
               ports might use a popup.
*/
void
sdl_getlin(const char *question, char *input)
{
    logDebug("sdl_getlin(%s, %p)\n", question, input);

    if (!iflags.wc_popup_dialog) {
        char c;
        int len;
        int done;
        int createcaret;

        createcaret = 1;
        SendMessage(sdl_hwnd_from_winid(WIN_MESSAGE), WM_MSNH_COMMAND,
                    (WPARAM) MSNH_MSG_CARET, (LPARAM) &createcaret);

        /* sdl_clear_nhwindow(WIN_MESSAGE); */
        sdl_putstr_ex(WIN_MESSAGE, ATR_BOLD, question, 0);
        sdl_putstr_ex(WIN_MESSAGE, ATR_BOLD, " ", 1);
#ifdef EDIT_GETLIN
        sdl_putstr_ex(WIN_MESSAGE, ATR_BOLD, input, 0);
        len = strlen(input);
#else
        input[0] = '\0';
        len = 0;
#endif
        ShowCaret(sdl_hwnd_from_winid(WIN_MESSAGE));
        done = FALSE;
        while (!done) {
            c = sdl_nhgetch();
            switch (c) {
            case VK_ESCAPE:
                strcpy(input, "\033");
                done = TRUE;
                break;
            case '\n':
            case '\r':
            case -115:
                done = TRUE;
                break;
            default:
                if (input[0])
                    sdl_putstr_ex(WIN_MESSAGE, ATR_NONE, input, -len);
                if (c == VK_BACK) {
                    if (len > 0)
                        len--;
                    input[len] = '\0';
                } else if (len>=(BUFSZ-1)) {
                    PlaySound((LPCSTR)SND_ALIAS_SYSTEMEXCLAMATION, NULL, SND_ALIAS_ID|SND_ASYNC);
                } else {
                    input[len++] = c;
                    input[len] = '\0';
                }
                sdl_putstr_ex(WIN_MESSAGE, ATR_NONE, input, 1);
                break;
            }
        }
        HideCaret(sdl_hwnd_from_winid(WIN_MESSAGE));
        createcaret = 0;
        SendMessage(sdl_hwnd_from_winid(WIN_MESSAGE), WM_MSNH_COMMAND,
                    (WPARAM) MSNH_MSG_CARET, (LPARAM) &createcaret);
    } else {
        if (mswin_getlin_window(question, input, BUFSZ) == IDCANCEL) {
            strcpy(input, "\033");
        }
    }
}

/*
int get_ext_cmd(void)
            -- Get an extended command in a window-port specific way.
               An index into extcmdlist[] is returned on a successful
               selection, -1 otherwise.
*/
int
sdl_get_ext_cmd()
{
    int ret;
    logDebug("sdl_get_ext_cmd()\n");

    if (!iflags.wc_popup_dialog) {
        char c;
        char cmd[BUFSZ];
        int i, len;
        int createcaret;

        createcaret = 1;
        SendMessage(sdl_hwnd_from_winid(WIN_MESSAGE), WM_MSNH_COMMAND,
                    (WPARAM) MSNH_MSG_CARET, (LPARAM) &createcaret);

        cmd[0] = '\0';
        i = -2;
        sdl_clear_nhwindow(WIN_MESSAGE);
        sdl_putstr_ex(WIN_MESSAGE, ATR_BOLD, "#", 0);
        len = 0;
        ShowCaret(sdl_hwnd_from_winid(WIN_MESSAGE));
        while (i == -2) {
            int oindex, com_index;
            c = sdl_nhgetch();
            switch (c) {
            case VK_ESCAPE:
                i = -1;
                break;
            case '\n':
            case '\r':
            case -115:
                for (i = 0; extcmdlist[i].ef_txt != (char *) 0; i++)
                    if (!strcmpi(cmd, extcmdlist[i].ef_txt))
                        break;

                if (extcmdlist[i].ef_txt == (char *) 0) {
                    pline("%s: unknown extended command.", cmd);
                    i = -1;
                }
                break;
            default:
                if (cmd[0])
                    sdl_putstr_ex(WIN_MESSAGE, ATR_BOLD, cmd,
                                    -(int) strlen(cmd));
                if (c == VK_BACK) {
                    if (len > 0)
                        len--;
                    cmd[len] = '\0';
                } else {
                    cmd[len++] = c;
                    cmd[len] = '\0';
                    /* Find a command with this prefix in extcmdlist */
                    com_index = -1;
                    for (oindex = 0; extcmdlist[oindex].ef_txt != (char *) 0;
                         oindex++) {
                        if ((extcmdlist[oindex].flags & AUTOCOMPLETE)
                            && !(!wizard && (extcmdlist[oindex].flags & WIZMODECMD))
                            && !strncmpi(cmd, extcmdlist[oindex].ef_txt, len)) {
                            if (com_index == -1) /* no matches yet */
                                com_index = oindex;
                            else
                                com_index =
                                    -2; /* two matches, don't complete */
                        }
                    }
                    if (com_index >= 0) {
                        Strcpy(cmd, extcmdlist[com_index].ef_txt);
                    }
                }
                sdl_putstr_ex(WIN_MESSAGE, ATR_BOLD, cmd, 1);
                break;
            }
        }
        HideCaret(sdl_hwnd_from_winid(WIN_MESSAGE));
        createcaret = 0;
        SendMessage(sdl_hwnd_from_winid(WIN_MESSAGE), WM_MSNH_COMMAND,
                    (WPARAM) MSNH_MSG_CARET, (LPARAM) &createcaret);
        return i;
    } else {
        if (mswin_ext_cmd_window(&ret) == IDCANCEL)
            return -1;
        else
            return ret;
    }
}

/*
number_pad(state)
            -- Initialize the number pad to the given state.
*/
void
sdl_number_pad(int state)
{
    /* Do Nothing */
    logDebug("sdl_number_pad(%d)\n", state);
}

/*
delay_output()  -- Causes a visible delay of 50ms in the output.
               Conceptually, this is similar to wait_synch() followed
               by a nap(50ms), but allows asynchronous operation.
*/
void
sdl_delay_output()
{
    logDebug("sdl_delay_output()\n");
    //Sleep(50);
    sdl_wait_loop((flags.animation_frame_interval_in_milliseconds > 0 ? flags.animation_frame_interval_in_milliseconds : ANIMATION_FRAME_INTERVAL) * DELAY_OUTPUT_INTERVAL_IN_FRAMES);
}

void
sdl_delay_output_milliseconds(int interval)
{
    logDebug("sdl_delay_output_milliseconds()\n");
    //Sleep(interval);
    sdl_wait_loop(interval);
}

void
sdl_change_color()
{
    logDebug("sdl_change_color()\n");
}

char *
sdl_get_color_string()
{
    logDebug("sdl_get_color_string()\n");
    return ("");
}

/*
start_screen()  -- Only used on Unix tty ports, but must be declared for
               completeness.  Sets up the tty to work in full-screen
               graphics mode.  Look at win/tty/termcap.c for an
               example.  If your window-port does not need this function
               just declare an empty function.
*/
void
sdl_start_screen()
{
    /* Do Nothing */
    logDebug("sdl_start_screen()\n");
}

/*
end_screen()    -- Only used on Unix tty ports, but must be declared for
               completeness.  The complement of start_screen().
*/
void
sdl_end_screen()
{
    /* Do Nothing */
    logDebug("sdl_end_screen()\n");
}

/*
outrip(winid, int, when)
            -- The tombstone code.  If you want the traditional code use
               genl_outrip for the value and check the #if in rip.c.
*/
#define STONE_LINE_LEN 16
void
sdl_outrip(winid wid, int how, time_t when)
{
    char buf[BUFSZ];
    long year;

    logDebug("sdl_outrip(%d, %d, %ld)\n", wid, how, (long) when);
    if ((wid >= 0) && (wid < MAXWINDOWS)) {
        DestroyWindow(GetNHApp()->windowlist[wid].win);
        GetNHApp()->windowlist[wid].win = mswin_init_RIP_window();
        GetNHApp()->windowlist[wid].type = NHW_RIP;
        GetNHApp()->windowlist[wid].dead = 0;
    }

    /* Put name on stone */
    Sprintf(buf, "%s", plname);
    buf[STONE_LINE_LEN] = 0;
    putstr(wid, 0, buf);

    /* Put $ on stone */
    Sprintf(buf, "%ld Au", done_money);
    buf[STONE_LINE_LEN] = 0; /* It could be a *lot* of gold :-) */
    putstr(wid, 0, buf);

    /* Put together death description */
    formatkiller(buf, sizeof buf, how, FALSE);

    /* Put death type on stone */
    putstr(wid, 0, buf);

    /* Put year on stone */
    year = yyyymmdd(when) / 10000L;
    Sprintf(buf, "%4ld", year);
    putstr(wid, 0, buf);
    mswin_finish_rip_text(wid);
}

/* handle options updates here */
void
sdl_preference_update(const char *pref)
{
    if (stricmp(pref, "font_menu") == 0
        || stricmp(pref, "font_size_menu") == 0) 
    {
        if (iflags.wc_fontsiz_menu == 0)
            iflags.wc_fontsiz_menu = NHFONT_DEFAULT_SIZE;
        else if (iflags.wc_fontsiz_menu < NHFONT_SIZE_MIN)
            iflags.wc_fontsiz_menu = NHFONT_SIZE_MIN;
        else if (iflags.wc_fontsiz_menu > NHFONT_SIZE_MAX)
            iflags.wc_fontsiz_menu = NHFONT_SIZE_MAX;

        HDC hdc = GetDC(GetNHApp()->hMainWnd);
        mswin_get_font(NHW_MENU, ATR_NONE, hdc, TRUE);
        mswin_get_font(NHW_MENU, ATR_BOLD, hdc, TRUE);
        mswin_get_font(NHW_MENU, ATR_DIM, hdc, TRUE);
        mswin_get_font(NHW_MENU, ATR_ULINE, hdc, TRUE);
        mswin_get_font(NHW_MENU, ATR_BLINK, hdc, TRUE);
        mswin_get_font(NHW_MENU, ATR_INVERSE, hdc, TRUE);
        ReleaseDC(GetNHApp()->hMainWnd, hdc);

        mswin_layout_main_window(NULL);
        return;
    }

    if (stricmp(pref, "font_status") == 0
        || stricmp(pref, "font_size_status") == 0) 
    {
        if (iflags.wc_fontsiz_status == 0)
            iflags.wc_fontsiz_status = NHFONT_DEFAULT_SIZE;
        else if (iflags.wc_fontsiz_status < NHFONT_SIZE_MIN)
            iflags.wc_fontsiz_status = NHFONT_SIZE_MIN;
        else if (iflags.wc_fontsiz_menu > NHFONT_SIZE_MAX)
            iflags.wc_fontsiz_status = NHFONT_SIZE_MAX;

        HDC hdc = GetDC(GetNHApp()->hMainWnd);
        mswin_get_font(NHW_STATUS, ATR_NONE, hdc, TRUE);
        mswin_get_font(NHW_STATUS, ATR_BOLD, hdc, TRUE);
        mswin_get_font(NHW_STATUS, ATR_DIM, hdc, TRUE);
        mswin_get_font(NHW_STATUS, ATR_ULINE, hdc, TRUE);
        mswin_get_font(NHW_STATUS, ATR_BLINK, hdc, TRUE);
        mswin_get_font(NHW_STATUS, ATR_INVERSE, hdc, TRUE);
        ReleaseDC(GetNHApp()->hMainWnd, hdc);

        for (int i = 1; i < MAXWINDOWS; i++) {
            if (GetNHApp()->windowlist[i].type == NHW_STATUS
                && GetNHApp()->windowlist[i].win != NULL) {
                InvalidateRect(GetNHApp()->windowlist[i].win, NULL, TRUE);
            }
        }
        mswin_layout_main_window(NULL);
        return;
    }

    if (stricmp(pref, "font_message") == 0
        || stricmp(pref, "font_size_message") == 0)
    {
        if (iflags.wc_fontsiz_message == 0)
            iflags.wc_fontsiz_message = NHFONT_DEFAULT_SIZE;
        else if (iflags.wc_fontsiz_message < NHFONT_SIZE_MIN)
            iflags.wc_fontsiz_message = NHFONT_SIZE_MIN;
        else if (iflags.wc_fontsiz_message > NHFONT_SIZE_MAX)
            iflags.wc_fontsiz_message = NHFONT_SIZE_MAX;

        HDC hdc = GetDC(GetNHApp()->hMainWnd);
        mswin_get_font(NHW_MESSAGE, ATR_NONE, hdc, TRUE);
        mswin_get_font(NHW_MESSAGE, ATR_BOLD, hdc, TRUE);
        mswin_get_font(NHW_MESSAGE, ATR_DIM, hdc, TRUE);
        mswin_get_font(NHW_MESSAGE, ATR_ULINE, hdc, TRUE);
        mswin_get_font(NHW_MESSAGE, ATR_BLINK, hdc, TRUE);
        mswin_get_font(NHW_MESSAGE, ATR_INVERSE, hdc, TRUE);
        ReleaseDC(GetNHApp()->hMainWnd, hdc);

        InvalidateRect(sdl_hwnd_from_winid(WIN_MESSAGE), NULL, TRUE);
        mswin_layout_main_window(NULL);
        return;
    }

    if (stricmp(pref, "font_text") == 0
        || stricmp(pref, "font_size_text") == 0) 
    {
        if (iflags.wc_fontsiz_text == 0)
            iflags.wc_fontsiz_text = NHFONT_DEFAULT_SIZE;
        else if (iflags.wc_fontsiz_text < NHFONT_SIZE_MIN)
            iflags.wc_fontsiz_text = NHFONT_SIZE_MIN;
        else if (iflags.wc_fontsiz_text > NHFONT_SIZE_MAX)
            iflags.wc_fontsiz_text = NHFONT_SIZE_MAX;

        HDC hdc = GetDC(GetNHApp()->hMainWnd);
        mswin_get_font(NHW_TEXT, ATR_NONE, hdc, TRUE);
        mswin_get_font(NHW_TEXT, ATR_BOLD, hdc, TRUE);
        mswin_get_font(NHW_TEXT, ATR_DIM, hdc, TRUE);
        mswin_get_font(NHW_TEXT, ATR_ULINE, hdc, TRUE);
        mswin_get_font(NHW_TEXT, ATR_BLINK, hdc, TRUE);
        mswin_get_font(NHW_TEXT, ATR_INVERSE, hdc, TRUE);
        ReleaseDC(GetNHApp()->hMainWnd, hdc);

        mswin_layout_main_window(NULL);
        return;
    }

    if (stricmp(pref, "scroll_amount") == 0) {
        sdl_cliparound(u.ux, u.uy);
        return;
    }

    if (stricmp(pref, "scroll_margin") == 0) {
        sdl_cliparound(u.ux, u.uy);
        return;
    }

    if (stricmp(pref, "map_mode") == 0) {
        mswin_select_map_mode(iflags.wc_map_mode);
        return;
    }

    if (stricmp(pref, "hilite_pet") == 0) {
        InvalidateRect(sdl_hwnd_from_winid(WIN_MAP), NULL, TRUE);
        return;
    }

    if (stricmp(pref, "align_message") == 0
        || stricmp(pref, "align_status") == 0) {
        mswin_layout_main_window(NULL);
        return;
    }

    if (stricmp(pref, "vary_msgcount") == 0) {
        InvalidateRect(sdl_hwnd_from_winid(WIN_MESSAGE), NULL, TRUE);
        mswin_layout_main_window(NULL);
        return;
    }

    if (stricmp(pref, "perm_invent") == 0) {
        sdl_update_inventory();
        return;
    }

    if (stricmp(pref, "preferred_screen_scale") == 0) {
        dozoomnormal();
        sdl_stretch_window();
        return;
    }

}

#define TEXT_BUFFER_SIZE 4096
char *
sdl_getmsghistory(BOOLEAN_P init)
{
    static PMSNHMsgGetText text = 0;
    static char *next_message = 0;

    if (init) {
        text = (PMSNHMsgGetText) malloc(sizeof(MSNHMsgGetText)
                                        + TEXT_BUFFER_SIZE);

		if (!text)
			return (char*)0;

        text->max_size =
            TEXT_BUFFER_SIZE
            - 1; /* make sure we always have 0 at the end of the buffer */

        ZeroMemory(text->buffer, TEXT_BUFFER_SIZE);
        SendMessage(sdl_hwnd_from_winid(WIN_MESSAGE), WM_MSNH_COMMAND,
                    (WPARAM) MSNH_MSG_GETTEXT, (LPARAM) text);

        next_message = text->buffer;
    }

    if (!(next_message && next_message[0])) {
        free(text);
        next_message = 0;
        return (char *) 0;
    } else {
        char *retval = next_message;
        char *p;
        next_message = p = strchr(next_message, '\n');
        if (next_message)
            next_message++;
        if (p)
            while (p >= retval && isspace((uchar) *p))
                *p-- = (char) 0; /* delete trailing whitespace */
        return retval;
    }
}

void
sdl_putmsghistory(const char *msg, BOOLEAN_P restoring)
{
    BOOL save_sound_opt;

    UNREFERENCED_PARAMETER(restoring);

    if (!msg)
        return; /* end of message history restore */
    save_sound_opt = GetNHApp()->bNoSounds;
    GetNHApp()->bNoSounds =
        TRUE; /* disable sounds while restoring message history */
    sdl_putstr_ex(WIN_MESSAGE, ATR_NONE, msg, 0);
    clear_nhwindow(WIN_MESSAGE); /* it is in fact end-of-turn indication so
                                    each message will print on the new line */
    GetNHApp()->bNoSounds = save_sound_opt; /* restore sounds option */
}

void
sdl_main_loop()
{
    //OBSOLETE

    MSG msg;

    while (!mswin_have_input()) {
        if (!iflags.debug_fuzzer || PeekMessage(&msg, NULL, 0, 0, FALSE)) {
            if(GetMessage(&msg, NULL, 0, 0) != 0) {
                if (GetNHApp()->regGnollHackMode
                    || !TranslateAccelerator(msg.hwnd, GetNHApp()->hAccelTable,
                                                &msg)) {
                    TranslateMessage(&msg);
                    DispatchMessage(&msg);
                }
            } else {
                /* WM_QUIT */
                break;
            }
        } else {
            nhassert(iflags.debug_fuzzer);
            PostMessage(GetNHApp()->hMainWnd, WM_MSNH_COMMAND,
                        MSNH_MSG_RANDOM_INPUT, 0);
        }
    }
}

void
sdl_wait_loop(int milliseconds)
{
    if (milliseconds <= 0)
        return;

    MSG msg;
    SYSTEMTIME start_systime = { 0 };
    FILETIME start_filetime = { 0 };
    ULARGE_INTEGER start_largeint = { 0 };
    GetSystemTime(&start_systime);
    SystemTimeToFileTime(&start_systime, &start_filetime);
    start_largeint.LowPart = start_filetime.dwLowDateTime;
    start_largeint.HighPart = start_filetime.dwHighDateTime;

    SYSTEMTIME current_systime = { 0 };
    FILETIME current_filetime = { 0 };
    ULARGE_INTEGER current_largeint = { 0 };
    ULONGLONG timepassed = 0;
    ULONGLONG threshold = (ULONGLONG)milliseconds * 10000ULL;
    if (threshold > 50000000ULL)
        threshold = 50000000ULL;

    disallow_keyboard_commands_in_wait_loop = TRUE;

    do 
    {
        if (GetMessage(&msg, NULL, 0, 0) != 0) 
        {
            if (GetNHApp()->regGnollHackMode || !TranslateAccelerator(msg.hwnd, GetNHApp()->hAccelTable, &msg)) 
            {
                TranslateMessage(&msg);
                DispatchMessage(&msg);
            }
        }
        else 
        {
            /* WM_QUIT */
            break;
        }

        GetSystemTime(&current_systime);
        SystemTimeToFileTime(&current_systime, &current_filetime);
        current_largeint.LowPart = current_filetime.dwLowDateTime;
        current_largeint.HighPart = current_filetime.dwHighDateTime;

        timepassed = current_largeint.QuadPart - start_largeint.QuadPart;
    } while (timepassed < threshold);

    disallow_keyboard_commands_in_wait_loop = FALSE;

    reduce_counters(milliseconds);
}

/* clean up and quit */
void
sdl_bail(const char *mesg)
{
    clearlocks();
    sdl_exit_nhwindows(mesg);
    nh_terminate(EXIT_SUCCESS);
    /*NOTREACHED*/
}

BOOL
sdl_initMapTiles(void)
{
    HBITMAP hBmp;
    BITMAP bm;
    DWORD errcode;
    int tl_num;
    SIZE map_size;
    int total_tiles_used = process_tiledata(2, (const char*)0, (short*)0, (uchar*)0);

    /* no file - no tile */
    if (!(iflags.wc_tile_file && *iflags.wc_tile_file))
        return TRUE;

    /* load bitmap */
    hBmp = LoadPNGFromFile(iflags.wc_tile_file, TILE_BK_COLOR);

    if (hBmp == NULL) {
        char errmsg[BUFSZ];

        errcode = GetLastError();
        Sprintf(errmsg, "%s (0x%x).",
            "Cannot load tiles from the file. Reverting back to default",
            errcode);
        raw_print(errmsg);
        return FALSE;
    }

    /* calculate tile dimensions */
    GetObject(hBmp, sizeof(BITMAP), (LPVOID) &bm);
    if (bm.bmWidth % iflags.wc_tile_width
        || bm.bmHeight % iflags.wc_tile_height) {
        DeleteObject(hBmp);
        raw_print("Tiles bitmap does not match tile_width and tile_height "
                  "options. Reverting back to default.");
        return FALSE;
    }

    tl_num = (bm.bmWidth / iflags.wc_tile_width)
             * (bm.bmHeight / iflags.wc_tile_height);
    if (tl_num < total_tiles_used) {
        DeleteObject(hBmp);
        raw_print("Number of tiles in the bitmap is less than required by "
                  "the game. Reverting back to default.");
        return FALSE;
    }

    /* set the tile information */
    if (GetNHApp()->bmpMapTiles != GetNHApp()->bmpTiles) {
        DeleteObject(GetNHApp()->bmpMapTiles);
    }

    GetNHApp()->bmpMapTiles = hBmp;
    GetNHApp()->mapTile_X = iflags.wc_tile_width;
    GetNHApp()->mapTile_Y = iflags.wc_tile_height;
    GetNHApp()->mapTilesPerLine = bm.bmWidth / iflags.wc_tile_width;

    map_size.cx = GetNHApp()->mapTile_X * COLNO;
    map_size.cy = GetNHApp()->mapTile_Y * ROWNO;
    mswin_map_stretch(sdl_hwnd_from_winid(WIN_MAP), &map_size, TRUE);
    return TRUE;
}

void
sdl_popup_display(HWND hWnd, int *done_indicator)
{
    MSG msg;
    HWND hChild;
    HMENU hMenu;
    int mi_count;
    int i;

    /* activate the menu window */
    GetNHApp()->hPopupWnd = hWnd;

    mswin_layout_main_window(hWnd);

    /* disable game windows */
    for (hChild = GetWindow(GetNHApp()->hMainWnd, GW_CHILD); hChild;
         hChild = GetWindow(hChild, GW_HWNDNEXT)) {
        if (hChild != hWnd)
            EnableWindow(hChild, FALSE);
    }

    /* disable menu */
    hMenu = GetMenu(GetNHApp()->hMainWnd);
    mi_count = GetMenuItemCount(hMenu);
    for (i = 0; i < mi_count; i++) {
        EnableMenuItem(hMenu, i, MF_BYPOSITION | MF_GRAYED);
    }
    DrawMenuBar(GetNHApp()->hMainWnd);

    /* bring menu window on top */
    SetWindowPos(hWnd, HWND_TOP, 0, 0, 0, 0,
                 SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
    SetFocus(hWnd);

    /* go into message loop */
    while (IsWindow(hWnd) && (done_indicator == NULL || !*done_indicator)) {
        if (!iflags.debug_fuzzer || PeekMessage(&msg, NULL, 0, 0, FALSE)) {
            if(GetMessage(&msg, NULL, 0, 0) != 0) {
                if (msg.message == WM_MSNH_COMMAND ||
                    !IsDialogMessage(hWnd, &msg)) {
                    if (!TranslateAccelerator(msg.hwnd,
                                              GetNHApp()->hAccelTable, &msg)) {
                        TranslateMessage(&msg);
                        DispatchMessage(&msg);
                    }
                }
            } else {
                /* WM_QUIT */
                break;
            }
        } else {
            nhassert(iflags.debug_fuzzer);
            PostMessage(hWnd, WM_MSNH_COMMAND, MSNH_MSG_RANDOM_INPUT, 0);
        }
    }
}

void
sdl_popup_destroy(HWND hWnd)
{
    HWND hChild;
    HMENU hMenu;
    int mi_count;
    int i;

    /* enable game windows */
    for (hChild = GetWindow(GetNHApp()->hMainWnd, GW_CHILD); hChild;
         hChild = GetWindow(hChild, GW_HWNDNEXT)) {
        if (hChild != hWnd) {
            EnableWindow(hChild, TRUE);
        }
    }

    /* enable menu */
    hMenu = GetMenu(GetNHApp()->hMainWnd);
    mi_count = GetMenuItemCount(hMenu);
    for (i = 0; i < mi_count; i++) {
        EnableMenuItem(hMenu, i, MF_BYPOSITION | MF_ENABLED);
    }
    DrawMenuBar(GetNHApp()->hMainWnd);

    /* Don't hide the permanent inventory window ... leave it showing */
    if (!iflags.perm_invent || sdl_winid_from_handle(hWnd) != WIN_INVEN)
        ShowWindow(hWnd, SW_HIDE);

    GetNHApp()->hPopupWnd = NULL;

    mswin_layout_main_window(hWnd);

    SetFocus(GetNHApp()->hMainWnd);
}

/* Reading and writing settings from the registry. */
#define CATEGORYKEY "Software"
#define COMPANYKEY "GnollHack"
#define PRODUCTKEY "GnollHack 3.6.2"
#define SETTINGSKEY "Settings"
#define MAINSHOWSTATEKEY "MainShowState"
#define MAINMINXKEY "MainMinX"
#define MAINMINYKEY "MainMinY"
#define MAINMAXXKEY "MainMaxX"
#define MAINMAXYKEY "MainMaxY"
#define MAINLEFTKEY "MainLeft"
#define MAINRIGHTKEY "MainRight"
#define MAINTOPKEY "MainTop"
#define MAINBOTTOMKEY "MainBottom"
#define MAINAUTOLAYOUT "AutoLayout"
#define MAPLEFT "MapLeft"
#define MAPRIGHT "MapRight"
#define MAPTOP "MapTop"
#define MAPBOTTOM "MapBottom"
#define MSGLEFT "MsgLeft"
#define MSGRIGHT "MsgRight"
#define MSGTOP "MsgTop"
#define MSGBOTTOM "MsgBottom"
#define STATUSLEFT "StatusLeft"
#define STATUSRIGHT "StatusRight"
#define STATUSTOP "StatusTop"
#define STATUSBOTTOM "StatusBottom"
#define MENULEFT "MenuLeft"
#define MENURIGHT "MenuRight"
#define MENUTOP "MenuTop"
#define MENUBOTTOM "MenuBottom"
#define TEXTLEFT "TextLeft"
#define TEXTRIGHT "TextRight"
#define TEXTTOP "TextTop"
#define TEXTBOTTOM "TextBottom"
#define INVENTLEFT "InventLeft"
#define INVENTRIGHT "InventRight"
#define INVENTTOP "InventTop"
#define INVENTBOTTOM "InventBottom"

/* #define all the subkeys here */
#define INTFKEY "Interface"

void
sdl_read_reg()
{
    HKEY key;
    DWORD size;
    DWORD safe_buf;
    char keystring[MAX_PATH];
    int i;
    COLORREF default_mapcolors[CLR_MAX] = {
	RGB(0x55, 0x55, 0x55), /* CLR_BLACK */
	RGB(0xFF, 0x00, 0x00), /* CLR_RED */
	RGB(0x00, 0x80, 0x00), /* CLR_GREEN */
	RGB(0xA5, 0x2A, 0x2A), /* CLR_BROWN */
	RGB(0x00, 0x00, 0xFF), /* CLR_BLUE */
	RGB(0xFF, 0x00, 0xFF), /* CLR_MAGENTA */
	RGB(0x00, 0xFF, 0xFF), /* CLR_CYAN */
	RGB(0xC0, 0xC0, 0xC0), /* CLR_GRAY */
	RGB(0xFF, 0xFF, 0xFF), /* NO_COLOR */
	RGB(0xFF, 0xA5, 0x00), /* CLR_ORANGE */
	RGB(0x00, 0xFF, 0x00), /* CLR_BRIGHT_GREEN */
	RGB(0xFF, 0xFF, 0x00), /* CLR_YELLOW */
	RGB(0x00, 0xC0, 0xFF), /* CLR_BRIGHT_BLUE */
	RGB(0xFF, 0x80, 0xFF), /* CLR_BRIGHT_MAGENTA */
	RGB(0x80, 0xFF, 0xFF), /* CLR_BRIGHT_CYAN */
	RGB(0xFF, 0xFF, 0xFF)  /* CLR_WHITE */
    };

    sprintf(keystring, "%s\\%s\\%s\\%s", CATEGORYKEY, COMPANYKEY, PRODUCTKEY,
            SETTINGSKEY);

    /* Set the defaults here. The very first time the app is started, nothing
       is
       read from the registry, so these defaults apply. */
    GetNHApp()->saveRegistrySettings = 1; /* Normally, we always save */
    GetNHApp()->regGnollHackMode = TRUE;

    for (i = 0; i < CLR_MAX; i++)
        GetNHApp()->regMapColors[i] = default_mapcolors[i];

    if (RegOpenKeyEx(HKEY_CURRENT_USER, keystring, 0, KEY_READ, &key)
        != ERROR_SUCCESS)
        return;

    size = sizeof(DWORD);

#define NHGETREG_DWORD(name, val)                                       \
    RegQueryValueEx(key, (name), 0, NULL, (unsigned char *)(&safe_buf), \
                    &size);                                             \
    (val) = safe_buf;

    /* read the keys here */
    NHGETREG_DWORD(INTFKEY, GetNHApp()->regGnollHackMode);

    /* read window placement */
    NHGETREG_DWORD(MAINSHOWSTATEKEY, GetNHApp()->regMainShowState);
    NHGETREG_DWORD(MAINMINXKEY, GetNHApp()->regMainMinX);
    NHGETREG_DWORD(MAINMINYKEY, GetNHApp()->regMainMinY);
    NHGETREG_DWORD(MAINMAXXKEY, GetNHApp()->regMainMaxX);
    NHGETREG_DWORD(MAINMAXYKEY, GetNHApp()->regMainMaxY);
    NHGETREG_DWORD(MAINLEFTKEY, GetNHApp()->regMainLeft);
    NHGETREG_DWORD(MAINRIGHTKEY, GetNHApp()->regMainRight);
    NHGETREG_DWORD(MAINTOPKEY, GetNHApp()->regMainTop);
    NHGETREG_DWORD(MAINBOTTOMKEY, GetNHApp()->regMainBottom);

    NHGETREG_DWORD(MAINAUTOLAYOUT, GetNHApp()->bAutoLayout);
    NHGETREG_DWORD(MAPLEFT, GetNHApp()->rtMapWindow.left);
    NHGETREG_DWORD(MAPRIGHT, GetNHApp()->rtMapWindow.right);
    NHGETREG_DWORD(MAPTOP, GetNHApp()->rtMapWindow.top);
    NHGETREG_DWORD(MAPBOTTOM, GetNHApp()->rtMapWindow.bottom);
    NHGETREG_DWORD(MSGLEFT, GetNHApp()->rtMsgWindow.left);
    NHGETREG_DWORD(MSGRIGHT, GetNHApp()->rtMsgWindow.right);
    NHGETREG_DWORD(MSGTOP, GetNHApp()->rtMsgWindow.top);
    NHGETREG_DWORD(MSGBOTTOM, GetNHApp()->rtMsgWindow.bottom);
    NHGETREG_DWORD(STATUSLEFT, GetNHApp()->rtStatusWindow.left);
    NHGETREG_DWORD(STATUSRIGHT, GetNHApp()->rtStatusWindow.right);
    NHGETREG_DWORD(STATUSTOP, GetNHApp()->rtStatusWindow.top);
    NHGETREG_DWORD(STATUSBOTTOM, GetNHApp()->rtStatusWindow.bottom);
    NHGETREG_DWORD(MENULEFT, GetNHApp()->rtMenuWindow.left);
    NHGETREG_DWORD(MENURIGHT, GetNHApp()->rtMenuWindow.right);
    NHGETREG_DWORD(MENUTOP, GetNHApp()->rtMenuWindow.top);
    NHGETREG_DWORD(MENUBOTTOM, GetNHApp()->rtMenuWindow.bottom);
    NHGETREG_DWORD(TEXTLEFT, GetNHApp()->rtTextWindow.left);
    NHGETREG_DWORD(TEXTRIGHT, GetNHApp()->rtTextWindow.right);
    NHGETREG_DWORD(TEXTTOP, GetNHApp()->rtTextWindow.top);
    NHGETREG_DWORD(TEXTBOTTOM, GetNHApp()->rtTextWindow.bottom);
    NHGETREG_DWORD(INVENTLEFT, GetNHApp()->rtInvenWindow.left);
    NHGETREG_DWORD(INVENTRIGHT, GetNHApp()->rtInvenWindow.right);
    NHGETREG_DWORD(INVENTTOP, GetNHApp()->rtInvenWindow.top);
    NHGETREG_DWORD(INVENTBOTTOM, GetNHApp()->rtInvenWindow.bottom);
#undef NHGETREG_DWORD

    for (i = 0; i < CLR_MAX; i++) {
        COLORREF cl;
        char mapcolorkey[64];
        sprintf(mapcolorkey, "MapColor%02d", i);
        if (RegQueryValueEx(key, mapcolorkey, NULL, NULL, (BYTE *)&cl, &size) == ERROR_SUCCESS)
            GetNHApp()->regMapColors[i] = cl;
    }

    RegCloseKey(key);

    /* check the data for validity */
    if (IsRectEmpty(&GetNHApp()->rtMapWindow)
        || IsRectEmpty(&GetNHApp()->rtMsgWindow)
        || IsRectEmpty(&GetNHApp()->rtStatusWindow)
        || IsRectEmpty(&GetNHApp()->rtMenuWindow)
        || IsRectEmpty(&GetNHApp()->rtTextWindow)
        || IsRectEmpty(&GetNHApp()->rtInvenWindow)) {
        GetNHApp()->bAutoLayout = TRUE;
    }
}

void
sdl_write_reg()
{
    HKEY key;
    DWORD disposition;
    int i;

    if (GetNHApp()->saveRegistrySettings) {
        char keystring[MAX_PATH];
        DWORD safe_buf;

        sprintf(keystring, "%s\\%s\\%s\\%s", CATEGORYKEY, COMPANYKEY,
                PRODUCTKEY, SETTINGSKEY);

        if (RegOpenKeyEx(HKEY_CURRENT_USER, keystring, 0, KEY_WRITE, &key)
            != ERROR_SUCCESS) {
            RegCreateKeyEx(HKEY_CURRENT_USER, keystring, 0, "",
                           REG_OPTION_NON_VOLATILE, KEY_ALL_ACCESS, NULL,
                           &key, &disposition);
        }

#define NHSETREG_DWORD(name, val)                                   \
    RegSetValueEx(key, (name), 0, REG_DWORD,                        \
                  (unsigned char *)((safe_buf = (val)), &safe_buf), \
                  sizeof(DWORD));

        /* Write the keys here */
        NHSETREG_DWORD(INTFKEY, GetNHApp()->regGnollHackMode);

        /* Main window placement */
        NHSETREG_DWORD(MAINSHOWSTATEKEY, GetNHApp()->regMainShowState);
        NHSETREG_DWORD(MAINMINXKEY, GetNHApp()->regMainMinX);
        NHSETREG_DWORD(MAINMINYKEY, GetNHApp()->regMainMinY);
        NHSETREG_DWORD(MAINMAXXKEY, GetNHApp()->regMainMaxX);
        NHSETREG_DWORD(MAINMAXYKEY, GetNHApp()->regMainMaxY);
        NHSETREG_DWORD(MAINLEFTKEY, GetNHApp()->regMainLeft);
        NHSETREG_DWORD(MAINRIGHTKEY, GetNHApp()->regMainRight);
        NHSETREG_DWORD(MAINTOPKEY, GetNHApp()->regMainTop);
        NHSETREG_DWORD(MAINBOTTOMKEY, GetNHApp()->regMainBottom);

        NHSETREG_DWORD(MAINAUTOLAYOUT, GetNHApp()->bAutoLayout);
        NHSETREG_DWORD(MAPLEFT, GetNHApp()->rtMapWindow.left);
        NHSETREG_DWORD(MAPRIGHT, GetNHApp()->rtMapWindow.right);
        NHSETREG_DWORD(MAPTOP, GetNHApp()->rtMapWindow.top);
        NHSETREG_DWORD(MAPBOTTOM, GetNHApp()->rtMapWindow.bottom);
        NHSETREG_DWORD(MSGLEFT, GetNHApp()->rtMsgWindow.left);
        NHSETREG_DWORD(MSGRIGHT, GetNHApp()->rtMsgWindow.right);
        NHSETREG_DWORD(MSGTOP, GetNHApp()->rtMsgWindow.top);
        NHSETREG_DWORD(MSGBOTTOM, GetNHApp()->rtMsgWindow.bottom);
        NHSETREG_DWORD(STATUSLEFT, GetNHApp()->rtStatusWindow.left);
        NHSETREG_DWORD(STATUSRIGHT, GetNHApp()->rtStatusWindow.right);
        NHSETREG_DWORD(STATUSTOP, GetNHApp()->rtStatusWindow.top);
        NHSETREG_DWORD(STATUSBOTTOM, GetNHApp()->rtStatusWindow.bottom);
        NHSETREG_DWORD(MENULEFT, GetNHApp()->rtMenuWindow.left);
        NHSETREG_DWORD(MENURIGHT, GetNHApp()->rtMenuWindow.right);
        NHSETREG_DWORD(MENUTOP, GetNHApp()->rtMenuWindow.top);
        NHSETREG_DWORD(MENUBOTTOM, GetNHApp()->rtMenuWindow.bottom);
        NHSETREG_DWORD(TEXTLEFT, GetNHApp()->rtTextWindow.left);
        NHSETREG_DWORD(TEXTRIGHT, GetNHApp()->rtTextWindow.right);
        NHSETREG_DWORD(TEXTTOP, GetNHApp()->rtTextWindow.top);
        NHSETREG_DWORD(TEXTBOTTOM, GetNHApp()->rtTextWindow.bottom);
        NHSETREG_DWORD(INVENTLEFT, GetNHApp()->rtInvenWindow.left);
        NHSETREG_DWORD(INVENTRIGHT, GetNHApp()->rtInvenWindow.right);
        NHSETREG_DWORD(INVENTTOP, GetNHApp()->rtInvenWindow.top);
        NHSETREG_DWORD(INVENTBOTTOM, GetNHApp()->rtInvenWindow.bottom);
#undef NHSETREG_DWORD

        for (i = 0; i < CLR_MAX; i++) {
            COLORREF cl = GetNHApp()->regMapColors[i];
            char mapcolorkey[64];
            sprintf(mapcolorkey, "MapColor%02d", i);
            RegSetValueEx(key, mapcolorkey, 0, REG_DWORD, (BYTE *)&cl, sizeof(DWORD));
        }

        RegCloseKey(key);
    }
}

void
sdl_destroy_reg()
{
    char keystring[MAX_PATH];
    HKEY key;
    DWORD nrsubkeys;

    /* Delete keys one by one, as NT does not delete trees */
    sprintf(keystring, "%s\\%s\\%s\\%s", CATEGORYKEY, COMPANYKEY, PRODUCTKEY,
            SETTINGSKEY);
    RegDeleteKey(HKEY_CURRENT_USER, keystring);
    sprintf(keystring, "%s\\%s\\%s", CATEGORYKEY, COMPANYKEY, PRODUCTKEY);
    RegDeleteKey(HKEY_CURRENT_USER, keystring);
    /* The company key will also contain information about newer versions
       of GnollHack (e.g. a subkey called GnollHack 4.0), so only delete that
       if it's empty now. */
    sprintf(keystring, "%s\\%s", CATEGORYKEY, COMPANYKEY);
    /* If we cannot open it, we probably cannot delete it either... Just
       go on and see what happens. */
    RegOpenKeyEx(HKEY_CURRENT_USER, keystring, 0, KEY_READ, &key);
    nrsubkeys = 0;
    RegQueryInfoKey(key, NULL, NULL, NULL, &nrsubkeys, NULL, NULL, NULL, NULL,
                    NULL, NULL, NULL);
    RegCloseKey(key);
    if (nrsubkeys == 0)
        RegDeleteKey(HKEY_CURRENT_USER, keystring);

    /* Prevent saving on exit */
    GetNHApp()->saveRegistrySettings = 0;
}

typedef struct ctv {
    const char *colorstring;
    COLORREF colorvalue;
} color_table_value;

/*
 * The color list here is a combination of:
 * GnollHack colors.  (See mhmap.c)
 * HTML colors. (See http://www.w3.org/TR/REC-html40/types.html#h-6.5 )
 */

static color_table_value color_table[] = {
    /* GnollHack colors */
    { "black", RGB(0x55, 0x55, 0x55) },
    { "red", RGB(0xFF, 0x00, 0x00) },
    { "green", RGB(0x00, 0x80, 0x00) },
    { "brown", RGB(0xA5, 0x2A, 0x2A) },
    { "blue", RGB(0x00, 0x00, 0xFF) },
    { "magenta", RGB(0xFF, 0x00, 0xFF) },
    { "cyan", RGB(0x00, 0xFF, 0xFF) },
    { "orange", RGB(0xFF, 0xA5, 0x00) },
    { "brightgreen", RGB(0x00, 0xFF, 0x00) },
    { "yellow", RGB(0xFF, 0xFF, 0x00) },
    { "brightblue", RGB(0x00, 0xC0, 0xFF) },
    { "brightmagenta", RGB(0xFF, 0x80, 0xFF) },
    { "brightcyan", RGB(0x80, 0xFF, 0xFF) },
    { "white", RGB(0xFF, 0xFF, 0xFF) },
    /* Remaining HTML colors */
    { "trueblack", RGB(0x00, 0x00, 0x00) },
    { "gray", RGB(0x80, 0x80, 0x80) },
    { "grey", RGB(0x80, 0x80, 0x80) },
    { "purple", RGB(0x80, 0x00, 0x80) },
    { "silver", RGB(0xC0, 0xC0, 0xC0) },
    { "maroon", RGB(0x80, 0x00, 0x00) },
    { "fuchsia", RGB(0xFF, 0x00, 0xFF) }, /* = GnollHack magenta */
    { "lime", RGB(0x00, 0xFF, 0x00) },    /* = GnollHack bright green */
    { "olive", RGB(0x80, 0x80, 0x00) },
    { "navy", RGB(0x00, 0x00, 0x80) },
    { "teal", RGB(0x00, 0x80, 0x80) },
    { "aqua", RGB(0x00, 0xFF, 0xFF) }, /* = GnollHack cyan */
    { "", RGB(0x00, 0x00, 0x00) },
};

typedef struct ctbv {
    char *colorstring;
    int syscolorvalue;
} color_table_brush_value;

static color_table_brush_value color_table_brush[] = {
    { "activeborder", COLOR_ACTIVEBORDER },
    { "activecaption", COLOR_ACTIVECAPTION },
    { "appworkspace", COLOR_APPWORKSPACE },
    { "background", COLOR_BACKGROUND },
    { "btnface", COLOR_BTNFACE },
    { "btnshadow", COLOR_BTNSHADOW },
    { "btntext", COLOR_BTNTEXT },
    { "captiontext", COLOR_CAPTIONTEXT },
    { "graytext", COLOR_GRAYTEXT },
    { "greytext", COLOR_GRAYTEXT },
    { "highlight", COLOR_HIGHLIGHT },
    { "highlighttext", COLOR_HIGHLIGHTTEXT },
    { "inactiveborder", COLOR_INACTIVEBORDER },
    { "inactivecaption", COLOR_INACTIVECAPTION },
    { "menu", COLOR_MENU },
    { "menutext", COLOR_MENUTEXT },
    { "scrollbar", COLOR_SCROLLBAR },
    { "window", COLOR_WINDOW },
    { "windowframe", COLOR_WINDOWFRAME },
    { "windowtext", COLOR_WINDOWTEXT },
    { "", -1 },
};

static void
sdl_color_from_string(char *colorstring, HBRUSH *brushptr,
                        COLORREF *colorptr)
{
    color_table_value *ctv_ptr = color_table;
    color_table_brush_value *ctbv_ptr = color_table_brush;
    int red_value, blue_value, green_value;
    static char *hexadecimals = "0123456789abcdef";

    if (colorstring == NULL)
        return;
    if (*colorstring == '#') {
        if (strlen(++colorstring) != 6)
            return;

        red_value = (int) (index(hexadecimals, tolower((uchar) *colorstring))
                           - hexadecimals);
        ++colorstring;
        red_value *= 16;
        red_value += (int) (index(hexadecimals, tolower((uchar) *colorstring))
                            - hexadecimals);
        ++colorstring;

        green_value = (int) (index(hexadecimals,
                                   tolower((uchar) *colorstring))
                             - hexadecimals);
        ++colorstring;
        green_value *= 16;
        green_value += (int) (index(hexadecimals,
                                    tolower((uchar) *colorstring))
                              - hexadecimals);
        ++colorstring;

        blue_value = (int) (index(hexadecimals, tolower((uchar) *colorstring))
                            - hexadecimals);
        ++colorstring;
        blue_value *= 16;
        blue_value += (int) (index(hexadecimals,
                                   tolower((uchar) *colorstring))
                             - hexadecimals);
        ++colorstring;

        *colorptr = RGB(red_value, green_value, blue_value);
    } else {
        while (*ctv_ptr->colorstring
               && stricmp(ctv_ptr->colorstring, colorstring))
            ++ctv_ptr;
        if (*ctv_ptr->colorstring) {
            *colorptr = ctv_ptr->colorvalue;
        } else {
            while (*ctbv_ptr->colorstring
                   && stricmp(ctbv_ptr->colorstring, colorstring))
                ++ctbv_ptr;
            if (*ctbv_ptr->colorstring) {
                *brushptr = SYSCLR_TO_BRUSH(ctbv_ptr->syscolorvalue);
                *colorptr = GetSysColor(ctbv_ptr->syscolorvalue);
            }
        }
    }
	if (sdl_max_brush >= TOTAL_BRUSHES)
	{
		panic("Too many colors!");
		return;
	}
    *brushptr = CreateSolidBrush(*colorptr);
    sdl_brush_table[sdl_max_brush] = *brushptr;
    sdl_max_brush++;
}

void
sdl_get_window_placement(int type, LPRECT rt)
{
    switch (type) {
    case NHW_MAP:
        *rt = GetNHApp()->rtMapWindow;
        break;

    case NHW_MESSAGE:
        *rt = GetNHApp()->rtMsgWindow;
        break;

    case NHW_STATUS:
        *rt = GetNHApp()->rtStatusWindow;
        break;

    case NHW_MENU:
        *rt = GetNHApp()->rtMenuWindow;
        break;

    case NHW_TEXT:
        *rt = GetNHApp()->rtTextWindow;
        break;

    case NHW_INVEN:
        *rt = GetNHApp()->rtInvenWindow;
        break;

    default:
        SetRect(rt, 0, 0, 0, 0);
        break;
    }
}

void
sdl_update_window_placement(int type, LPRECT rt)
{
    LPRECT rt_conf = NULL;

    switch (type) {
    case NHW_MAP:
        rt_conf = &GetNHApp()->rtMapWindow;
        break;

    case NHW_MESSAGE:
        rt_conf = &GetNHApp()->rtMsgWindow;
        break;

    case NHW_STATUS:
        rt_conf = &GetNHApp()->rtStatusWindow;
        break;

    case NHW_MENU:
        rt_conf = &GetNHApp()->rtMenuWindow;
        break;

    case NHW_TEXT:
        rt_conf = &GetNHApp()->rtTextWindow;
        break;

    case NHW_INVEN:
        rt_conf = &GetNHApp()->rtInvenWindow;
        break;
    }

    if (rt_conf && !IsRectEmpty(rt) && !EqualRect(rt_conf, rt)) {
        *rt_conf = *rt;
    }
}

int
SDL_NHMessageBox(HWND hWnd, LPCTSTR text, UINT type)
{
    TCHAR title[MAX_LOADSTRING];

    LoadString(GetNHApp()->hApp, IDS_APP_TITLE_SHORT, title, MAX_LOADSTRING);

    return MessageBox(hWnd, text, title, type);
}

static mswin_status_lines _status_lines;
static mswin_status_string _status_strings[MAXBLSTATS];
static mswin_status_string _condition_strings[BL_MASK_BITS];
static mswin_status_field _status_fields[MAXBLSTATS];

static mswin_condition_field _condition_fields[BL_MASK_BITS] = {
    { BL_MASK_STONE, "Stone" },
    { BL_MASK_SLIME, "Slime" },
    { BL_MASK_STRNGL, "Strngl" },
	{ BL_MASK_SUFFOC, "Suffoc" },
	{ BL_MASK_FOODPOIS, "FoodPois" },
    { BL_MASK_TERMILL, "TermIll" },
    { BL_MASK_BLIND, "Blind" },
    { BL_MASK_DEAF, "Deaf" },
    { BL_MASK_STUN, "Stun" },
    { BL_MASK_CONF, "Conf" },
    { BL_MASK_HALLU, "Hallu" },
    { BL_MASK_LEV, "Lev" },
    { BL_MASK_FLY, "Fly" },
    { BL_MASK_RIDE, "Ride" },
	{ BL_MASK_SLOWED, "Slow" },
	{ BL_MASK_PARALYZED, "Paral" },
	{ BL_MASK_FEARFUL, "Fear" },
	{ BL_MASK_SLEEPING, "Sleep" },
	{ BL_MASK_CANCELLED, "Cancl" },
	{ BL_MASK_SILENCED, "Silent" },
    { BL_MASK_GRAB, "Grab" },
    { BL_MASK_ROT, "Rot" },
    { BL_MASK_LYCANTHROPY, "Lyca" },
};


extern winid WIN_STATUS;

#ifdef STATUS_HILITES
typedef struct hilite_data_struct {
    int thresholdtype;
    anything value;
    int behavior;
    int under;
    int over;
} hilite_data_t;
static hilite_data_t _status_hilites[MAXBLSTATS];
#endif /* STATUS_HILITES */
/*
status_init()   -- core calls this to notify the window port that a status
                   display is required. The window port should perform
                   the necessary initialization in here, allocate memory, etc.
*/
void
sdl_status_init(void)
{
    logDebug("sdl_status_init()\n");

    for (int i = 0; i < SIZE(_status_fields); i++) {
        mswin_status_field * status_field = &_status_fields[i];
        status_field->field_index = i;
        status_field->enabled = FALSE;
    }

    for (int i = 0; i < SIZE(_condition_fields); i++) {
        mswin_condition_field * condition_field = &_condition_fields[i];
        nhassert(condition_field->mask == (1 << i));
        condition_field->bit_position = i;
    }

    for (int i = 0; i < SIZE(_status_strings); i++) {
        mswin_status_string * status_string = &_status_strings[i];
        status_string->str = NULL;
    }

    for (int i = 0; i < SIZE(_condition_strings); i++) {
        mswin_status_string * status_string = &_condition_strings[i];
        status_string->str = NULL;
    }

    for (int lineIndex = 0; lineIndex < iflags.wc2_statuslines /* SIZE(_status_lines.lines)*/; lineIndex++) {
        mswin_status_line * line = &_status_lines.lines[lineIndex];

        mswin_status_fields * status_fields = &line->status_fields;
        status_fields->count = 0;

        mswin_status_strings * status_strings = &line->status_strings;
        status_strings->count = 0;

        for (int i = 0; i < fieldcounts[lineIndex]; i++) {
            int field_index = iflags.wc2_statuslines == 2 ? fieldorders_2statuslines[lineIndex][i] : fieldorders[lineIndex][i];
            nhassert(field_index <= SIZE(_status_fields));

            nhassert(status_fields->count <= SIZE(status_fields->status_fields));
            status_fields->status_fields[status_fields->count++] = &_status_fields[field_index];

            nhassert(status_strings->count <= SIZE(status_strings->status_strings));
            status_strings->status_strings[status_strings->count++] =
                &_status_strings[field_index];

            if (field_index == BL_CONDITION) {
                for (int j = 0; j < BL_MASK_BITS; j++) {
                    nhassert(status_strings->count <= SIZE(status_strings->status_strings));
                    status_strings->status_strings[status_strings->count++] =
                        &_condition_strings[j];
                }
            }
        }
    }


    for (int i = 0; i < MAXBLSTATS; ++i) {
#ifdef STATUS_HILITES
        _status_hilites[i].thresholdtype = 0;
        _status_hilites[i].behavior = BL_TH_NONE;
        _status_hilites[i].under = BL_HILITE_NONE;
        _status_hilites[i].over = BL_HILITE_NONE;
#endif /* STATUS_HILITES */
    }
    /* Use a window for the genl version; backward port compatibility */
    WIN_STATUS = create_nhwindow(NHW_STATUS);
    display_nhwindow(WIN_STATUS, FALSE);
}

/*
status_finish() -- called when it is time for the window port to tear down
                   the status display and free allocated memory, etc.
*/
void
sdl_status_finish(void)
{
    logDebug("sdl_status_finish()\n");
}

/*
status_enablefield(int fldindex, char fldname, char fieldfmt, boolean enable)
                -- notifies the window port which fields it is authorized to
                   display.
                -- This may be called at any time, and is used
                   to disable as well as enable fields, depending on the
                   value of the final argument (TRUE = enable).
                -- fldindex could be one of the following from botl.h:
                   BL_TITLE, BL_STR, BL_DX, BL_CO, BL_IN, BL_WI, BL_CH,
                   BL_ALIGN, BL_SCORE, BL_CAP, BL_GOLD, BL_ENE, BL_ENEMAX,
                   BL_XP, BL_AC, BL_HD, BL_TIME, BL_HUNGER, BL_HP, BL_HPMAX,
                   BL_LEVELDESC, BL_EXP, BL_CONDITION
                -- There are MAXBLSTATS status fields (from botl.h)
*/
void
sdl_status_enablefield(int fieldidx, const char *nm, const char *fmt,
                         boolean enable)
{
    logDebug("sdl_status_enablefield(%d, %s, %s, %d)\n", fieldidx, nm, fmt,
             (int) enable);

    nhassert(fieldidx <= SIZE(_status_fields));
    mswin_status_field * field = &_status_fields[fieldidx];

    nhassert(fieldidx <= SIZE(_status_strings));
    mswin_status_string * string = &_status_strings[fieldidx];

    if (field != NULL) {
        field->format = fmt;
        field->space_in_front = (fmt[0] == ' ');
        if (field->space_in_front) field->format++;
        field->name = nm;
        field->enabled = enable;

        string->str = (field->enabled ? field->string : NULL);
        string->space_in_front = field->space_in_front;

        if (field->field_index == BL_CONDITION)
            string->str = NULL;

        string->draw_bar = (field->enabled && field->field_index == BL_TITLE);
    }
}

/* TODO: turn this into a commmon helper; multiple identical implementations */
static int
sdl_condcolor(bm, bmarray)
long bm;
unsigned long *bmarray;
{
    int i;

    if (bm && bmarray)
        for (i = 0; i < CLR_MAX; ++i) {
            if ((bm & bmarray[i]) != 0)
                return i;
        }
    return NO_COLOR;
}

static int
sdl_condattr(bm, bmarray)
long bm;
unsigned long *bmarray;
{
    if (bm && bmarray) {
        if (bm & bmarray[HL_ATTCLR_DIM]) return HL_DIM;
        if (bm & bmarray[HL_ATTCLR_BLINK]) return HL_BLINK;
        if (bm & bmarray[HL_ATTCLR_ULINE]) return HL_ULINE;
        if (bm & bmarray[HL_ATTCLR_INVERSE]) return HL_INVERSE;
        if (bm & bmarray[HL_ATTCLR_BOLD]) return HL_BOLD;
    }

    return HL_NONE;
}

/*

status_update(int fldindex, genericptr_t ptr, int chg, int percent, int color, unsigned long *colormasks)
                -- update the value of a status field.
                -- the fldindex identifies which field is changing and
                   is an integer index value from botl.h
                -- fldindex could be any one of the following from botl.h:
                   BL_TITLE, BL_STR, BL_DX, BL_CO, BL_IN, BL_WI, BL_CH,
                   BL_ALIGN, BL_SCORE, BL_CAP, BL_GOLD, BL_ENE, BL_ENEMAX,
                   BL_XP, BL_AC, BL_HD, BL_TIME, BL_HUNGER, BL_HP, BL_HPMAX,
                   BL_LEVELDESC, BL_EXP, BL_CONDITION
		-- fldindex could also be BL_FLUSH, which is not really
		   a field index, but is a special trigger to tell the 
		   windowport that it should output all changes received
                   to this point. It marks the end of a bot() cycle.
		-- fldindex could also be BL_RESET, which is not really
		   a field index, but is a special advisory to to tell the 
		   windowport that it should redisplay all its status fields,
		   even if no changes have been presented to it.
                -- ptr is usually a "char *", unless fldindex is BL_CONDITION.
                   If fldindex is BL_CONDITION, then ptr is a long value with
                   any or none of the following bits set (from botl.h):
                        BL_MASK_STONE           0x00000001L
                        BL_MASK_SLIME           0x00000002L
                        BL_MASK_STRNGL          0x00000004L
                        BL_MASK_FOODPOIS        0x00000008L
                        BL_MASK_TERMILL         0x00000010L
                        BL_MASK_BLIND           0x00000020L
                        BL_MASK_DEAF            0x00000040L
                        BL_MASK_STUN            0x00000080L
                        BL_MASK_CONF            0x00000100L
                        BL_MASK_HALLU           0x00000200L
                        BL_MASK_LEV             0x00000400L
                        BL_MASK_FLY             0x00000800L
                        BL_MASK_RIDE            0x00001000L
                -- The value passed for BL_GOLD includes an encoded leading
                   symbol for MAT_GOLD "\GXXXXNNNN:nnn". If window port needs
                   textual gold amount without the leading "$:" the port will
                   have to skip past ':' in passed "ptr" for the BL_GOLD case.
                -- color is the color that the GnollHack core is telling you to
                   use to display the text.
                -- condmasks is a pointer to a set of BL_ATTCLR_MAX unsigned
                   longs telling which conditions should be displayed in each
                   color and attriubte.
*/
void
sdl_status_update(int idx, genericptr_t ptr, int chg, int percent, int color, unsigned long *condmasks)
{
    long cond, *condptr = (long *) ptr;
    char *text = (char *) ptr;
    MSNHMsgUpdateStatus update_cmd_data;
    int ocolor, ochar;
    unsigned long ospecial;

    logDebug("sdl_status_update(%d, %p, %d, %d, %x, %p)\n", idx, ptr, chg, percent, color, condmasks);

    if (idx >= 0) {

        nhassert(idx <= SIZE(_status_fields));
        mswin_status_field * status_field = &_status_fields[idx];
        nhassert(status_field->field_index == idx);

        nhassert(idx <= SIZE(_status_strings));
        mswin_status_string * status_string = &_status_strings[idx];

        if (!status_field->enabled) {
            nhassert(status_string->str == NULL);
            return;
        }

        status_field->color = status_string->color = color & 0xff;
        status_field->attribute = status_string->attribute = (color >> 8) & 0xff;

        switch (idx) {
        case BL_CONDITION: {
            mswin_condition_field * condition_field = _condition_fields;

            nhassert(status_string->str == NULL);

            cond = *condptr;

            for (int i = 0; i < BL_MASK_BITS; i++, condition_field++) {
                status_string = &_condition_strings[i];

                if (condition_field->mask & cond) {
                    status_string->str = condition_field->name;
                    status_string->space_in_front = TRUE;
                    status_string->color = sdl_condcolor(condition_field->mask, condmasks);
                    status_string->attribute = sdl_condattr(condition_field->mask, condmasks);
                }
                else
                    status_string->str = NULL;
            }
        } break;
			
        case BL_GOLD: {
            char buf[BUFSZ];
            char *p;

            ZeroMemory(buf, sizeof(buf));
            if (iflags.invis_goldsym)
                ochar = GOLD_SYM;
            else
            {
                struct layer_info layers = nul_layerinfo;
                layers.glyph = objnum_to_glyph(GOLD_PIECE);
                mapglyph(layers,
                    &ochar, &ocolor, &ospecial, 0, 0);

            }
            buf[0] = ochar;
            p = strchr(text, ':');
            if (p) {
                strncpy(buf + 1, p, sizeof(buf) - 2);
            } else {
                buf[1] = ':';
                strncpy(buf + 2, text, sizeof(buf) - 3);
            }
            buf[sizeof buf - 1] = '\0';
            Sprintf(status_field->string,
                    status_field->format ? status_field->format : "%s", buf);
            nhassert(status_string->str == status_field->string);
        } break;
        default: {
            Sprintf(status_field->string,
                    status_field->format ? status_field->format : "%s", text);
            nhassert(status_string->str == status_field->string);
        } break;
        }

        /* if we received an update for the hp field, we must update the
         * bar percent and bar color for the title string */
        if (idx == BL_HP || idx == BL_HPMAX)
        {
            mswin_status_string * title_string = &_status_strings[BL_TITLE];

            if (idx == BL_HP)
            {
                title_string->bar_color = color & 0xff;
                title_string->bar_attribute = (color >> 8) & 0xff;
            }
            title_string->bar_percent = percent;

        }

    }

    if (idx == BL_FLUSH || idx == BL_RESET) {
        /* send command to status window to update */
        ZeroMemory(&update_cmd_data, sizeof(update_cmd_data));
        update_cmd_data.status_lines = &_status_lines;
        SendMessage(sdl_hwnd_from_winid(WIN_STATUS), WM_MSNH_COMMAND,
            (WPARAM)MSNH_MSG_UPDATE_STATUS, (LPARAM)&update_cmd_data);
    }
}

void
sdl_stretch_window(void)
{
    if (GetNHApp()->windowlist[WIN_MAP].win != NULL) {
        MSNHMsgClipAround data;

        ZeroMemory(&data, sizeof(data));
        data.x = GetNHApp()->mapTile_X;
        data.y = GetNHApp()->mapTile_Y;
        SendMessage(GetNHApp()->windowlist[WIN_MAP].win, WM_MSNH_COMMAND,
            (WPARAM)MSNH_MSG_STRETCH_MAP, (LPARAM)&data);
    }
}

void
sdl_set_animation_timer(unsigned int interval)
{
    if (GetNHApp()->windowlist[WIN_MAP].win != NULL) {
        UINT data;

        ZeroMemory(&data, sizeof(data));
        data = interval;
        SendMessage(GetNHApp()->windowlist[WIN_MAP].win, WM_MSNH_COMMAND,
            (WPARAM)MSNH_MSG_SET_ANIMATION_TIMER, (LPARAM)&data);
    }
}


void
sdl_open_special_view(struct special_view_info info)
{
    return;
}

void
sdl_stop_all_sounds(struct stop_all_info info)
{
    if (!fmod_stop_all_sounds(info))
    {
        impossible("Cannot stop all sounds!");
    }
}

void
sdl_play_immediate_ghsound(struct ghsound_immediate_info info)
{
    if (!fmod_play_immediate_sound(info))
    {
        impossible("Cannot play immediate sound!");
    }
}

void
sdl_play_ghsound_occupation_ambient(struct ghsound_occupation_ambient_info info)
{
    if (!fmod_play_occupation_ambient_sound(info))
    {
        impossible("Cannot play occupation ambient sound!");
    }
}

void
sdl_play_ghsound_effect_ambient(struct ghsound_effect_ambient_info info)
{
    if (!fmod_play_effect_ambient_sound(info))
    {
        impossible("Cannot play effect ambient sound!");
    }
}

void
sdl_set_effect_ambient_volume(struct effect_ambient_volume_info info)
{
    if (!fmod_set_effect_ambient_volume(info))
    {
        impossible("Cannot play set effect ambient volume!");
    }
}

void
sdl_play_ghsound_music(struct ghsound_music_info info)
{
    if (!fmod_play_music(info))
    {
        impossible("Cannot play music!");
    }
}

void
sdl_play_ghsound_level_ambient(struct ghsound_level_ambient_info info)
{
    if (!fmod_play_level_ambient_sound(info))
    {
        impossible("Cannot play level ambient sound!");
    }
}

void
sdl_play_ghsound_environment_ambient(struct ghsound_environment_ambient_info info)
{
    if (!fmod_play_environment_ambient_sound(info))
    {
        impossible("Cannot play environment ambient sound!");
    }
}

void
sdl_adjust_ghsound_general_volumes(VOID_ARGS)
{
    float new_general_volume = ((float)flags.sound_volume_general) / 100.0f;
    float new_music_volume = ((float)flags.sound_volume_music) / 100.0f;
    float new_ambient_volume = ((float)flags.sound_volume_ambient) / 100.0f;
    float new_effects_volume = ((float)flags.sound_volume_effects) / 100.0f;
    float new_ui_volume = ((float)flags.sound_volume_ui) / 100.0f;

    if (!fmod_adjust_ghsound_general_volumes(new_general_volume, new_music_volume, new_ambient_volume, new_effects_volume, new_ui_volume))
    {
        impossible("Cannot adjust volume!");
    }
}

void
sdl_add_ambient_ghsound(struct soundsource_t* soundsource)
{
    if (!fmod_add_ambient_ghsound(soundsource->ghsound, soundsource->heard_volume, &soundsource->ambient_ghsound_ptr))
    {
        impossible("Cannot add ambient sound!");
    }
}

void
sdl_delete_ambient_ghsound(struct soundsource_t* soundsource)
{
    if (!fmod_delete_ambient_ghsound(soundsource->ambient_ghsound_ptr))
    {
        impossible("Cannot delete ambient sound!");
    }
}

void
sdl_set_ambient_ghsound_volume(struct soundsource_t* soundsource)
{
    if (!fmod_set_ambient_ghsound_volume(soundsource->ambient_ghsound_ptr, soundsource->heard_volume))
    {
        impossible("Cannot set ambient sound volume!");
    }
}