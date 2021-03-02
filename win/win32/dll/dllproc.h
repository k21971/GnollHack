/*
 * dllproc.h
 * Copyright (c) Janne Gustafsson, 2021
 */
 /* GnollHack may be freely redistributed.  See license for details. */

#ifndef DLLPROC_H
#define DLLPROC_H

#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <windowsx.h>
#include "hack.h"
#include "color.h"
#include "soundfx.h"
#include "lev.h"

/* Create an array to keep track of the various windows */

#define E extern

E struct window_procs dll_procs;

#undef E

/* Some prototypes */
void dll_init_nhwindows(int *argc, char **argv);
void dll_player_selection(void);
void dll_askname(void);
void dll_get_nh_event(void);
void dll_exit_nhwindows(const char *);
void dll_suspend_nhwindows(const char *);
void dll_resume_nhwindows(void);
winid dll_create_nhwindow(int type);
void dll_clear_nhwindow(winid wid);
void dll_display_nhwindow(winid wid, BOOLEAN_P block);
void dll_destroy_nhwindow(winid wid);
void dll_curs(winid wid, int x, int y);
void dll_putstr(winid wid, int attr, const char *text);
void dll_putstr_ex(winid wid, int attr, const char *text, int);
void dll_display_file(const char *filename, BOOLEAN_P must_exist);
void dll_start_menu(winid wid);
void dll_add_menu(winid wid, int glyph, const ANY_P *identifier,
                    CHAR_P accelerator, CHAR_P group_accel, int attr,
                    const char *str, BOOLEAN_P presel);
void dll_add_extended_menu(winid wid, int glyph, const ANY_P* identifier, struct extended_menu_info info,
    CHAR_P accelerator, CHAR_P group_accel, int attr,
    const char* str, BOOLEAN_P presel);
void dll_end_menu(winid wid, const char *prompt);
int dll_select_menu(winid wid, int how, MENU_ITEM_P **selected);
void dll_update_inventory(void);
void dll_mark_synch(void);
void dll_wait_synch(void);
void dll_cliparound(int x, int y);
void dll_print_glyph(winid wid, XCHAR_P x, XCHAR_P y, struct layer_info layers);
void dll_raw_print(const char *str);
void dll_raw_print_bold(const char *str);
void dll_raw_print_flush();
int dll_nhgetch(void);
int dll_nh_poskey(int *x, int *y, int *mod);
void dll_nhbell(void);
int dll_doprev_message(void);
char dll_yn_function(const char *question, const char *choices, CHAR_P def);
void dll_getlin(const char *question, char *input);
int dll_get_ext_cmd(void);
void dll_number_pad(int state);
void dll_delay_output(void);
void dll_delay_output_milliseconds(int interval);
void dll_delay_output_intervals(int intervals);
void dll_change_color(void);
char *dll_get_color_string(void);
void dll_start_screen(void);
void dll_end_screen(void);
void dll_outrip(winid wid, int how, time_t when);
void dll_preference_update(const char *pref);
char *dll_getmsghistory(BOOLEAN_P init);
void dll_putmsghistory(const char *msg, BOOLEAN_P);

void dll_status_init(void);
void dll_status_finish(void);
void dll_status_enablefield(int fieldidx, const char *nm, const char *fmt,
                              boolean enable);
void dll_status_update(int idx, genericptr_t ptr, int chg, int percent, int color, unsigned long *colormasks);
void dll_stretch_window(void);
void dll_set_animation_timer(unsigned int);
void dll_open_special_view(struct special_view_info info);
void dll_stop_all_sounds(struct stop_all_info info);
void dll_play_immediate_ghsound(struct ghsound_immediate_info info);
void dll_play_ghsound_occupation_ambient(struct ghsound_occupation_ambient_info info);
void dll_play_ghsound_effect_ambient(struct ghsound_effect_ambient_info info);
void dll_set_effect_ambient_volume(struct effect_ambient_volume_info info);
void dll_play_ghsound_music(struct ghsound_music_info info);
void dll_play_ghsound_level_ambient(struct ghsound_level_ambient_info info);
void dll_play_ghsound_environment_ambient(struct ghsound_environment_ambient_info info);
void dll_adjust_ghsound_general_volumes(VOID_ARGS);
void dll_add_ambient_ghsound(struct soundsource_t* soundsource);
void dll_delete_ambient_ghsound(struct soundsource_t* soundsource);
void dll_set_ambient_ghsound_volume(struct soundsource_t* soundsource);
void dll_exit_hack(int status);

/* helper function */
HWND dll_hwnd_from_winid(winid wid);
winid dll_winid_from_type(int type);
winid dll_winid_from_handle(HWND hWnd);
void dll_window_mark_dead(winid wid);
void dll_bail(const char *mesg);

void dll_popup_display(HWND popup, int *done_indicator);
void dll_popup_destroy(HWND popup);

void dll_read_reg(void);
void dll_destroy_reg(void);
void dll_write_reg(void);

void dll_get_window_placement(int type, LPRECT rt);
void dll_update_window_placement(int type, LPRECT rt);
void dll_apply_window_style(HWND hwnd);

void dll_init_platform(VOID_ARGS);
void dll_exit_platform(int);

int dll_NHMessageBox(HWND hWnd, LPCTSTR text, UINT type);


#endif /* DLLPROC_H */