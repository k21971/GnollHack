/* GnollHack File Change Notice: This file has been changed from the original. Date of last change: 2024-08-11 */

/* GnollHack 4.0	mactopl.c	$NHDT-Date: 1432512797 2015/05/25 00:13:17 $  $NHDT-Branch: master $:$NHDT-Revision: 1.9 $ */
/* Copyright (c) Stichting Mathematisch Centrum, Amsterdam, 1985. */
/*-Copyright (c) Kenneth Lorber, Kensington, Maryland, 2015. */
/* GnollHack may be freely redistributed.  See license for details. */

#include "hack.h"
#include "mactty.h"
#include "macwin.h"
#include "macpopup.h"

char
queued_resp(char *resp)
{
    char buf[30];
    if (try_key_queue(buf)) {
        if (!resp || strchr(resp, buf[0]))
            return buf[0];
        if (digit(buf[0]) && strchr(resp, '#')) {
            yn_number = atoi(buf);
            return '#';
        }
    }
    return '\0';
}

char
topl_yn_function_ex(int style, int attr, int color, int glyph, const char* title, const char *query, const char *resp, char def, const char* resp_desc, uint64_t ynflags)
{
    char buf[30];
    char c = queued_resp((char *) resp);
    if (!c) {
        enter_topl_mode(attr, color, (char *) query);
        topl_set_resp((char *) resp, def);

        do {
            c = readchar();
            if (c && resp && !strchr(resp, c)) {
                nhbell();
                c = '\0';
            }
        } while (!c);

        topl_set_resp("", '\0');
        leave_topl_mode(buf);
        if (c == '#')
            yn_number = atoi(buf);
    }
    return c;
}

char
mac_yn_function_ex(style, attr, color, glyph title, query, resp, def, resp_desc, introline, ynflags)
int style, attr, color, glyph;
const char *title, *query, *resp, *resp_desc, *introline;
char def;
uint64_t ynflags;
/*
 *   Generic yes/no function. 'def' is the default (returned by space or
 *   return; 'esc' returns 'q', or 'n', or the default, depending on
 *   what's in the string. The 'query' string is printed before the user
 *   is asked about the string.
 *   If resp is NULL, any single character is accepted and returned.
 */
{
    return topl_yn_function_ex(style, attr, color, glyph, title, query, resp, def, resp_desc, introline, ynflags);
}

/* mactopl.c */
