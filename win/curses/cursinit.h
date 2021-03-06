/* GnollHack File Change Notice: This file has been changed from the original. Date of last change: 2021-09-14 */

/* vim:set cin ft=c sw=4 sts=4 ts=8 et ai cino=Ls\:0t0(0 : -*- mode:c;fill-column:80;tab-width:8;c-basic-offset:4;indent-tabs-mode:nil;c-file-style:"k&r" -*-*/
/* GnollHack 4.0 cursinit.h */
/* Copyright (c) Karl Garrison, 2010. */
/* GnollHack may be freely redistributed.  See license for details. */

#ifndef CURSINIT_H
# define CURSINIT_H

/* Global declarations */

void curses_create_main_windows(void);
void curses_init_nhcolors(void);
void curses_choose_character(void);
int curses_character_dialog(const char **choices, const char *prompt);
void curses_init_options(void);
void curses_display_splash_window(void);
void curses_cleanup(void);


#endif /* CURSINIT_H */
