/* GnollHack File Change Notice: This file has been changed from the original. Date of last change: 2021-09-14 */

/* GnollHack 4.0  pmatchregex.c	$NHDT-Date: 1544482890 2018/12/10 23:01:30 $  $NHDT-Branch: NetHack-3.6.2-beta01 $:$NHDT-Revision: 1.2 $ */
/* Copyright (c) Sean Hunt  2015.                                 */
/* GnollHack may be freely redistributed.  See license for details. */

#include "hack.h"

/* Implementation of the regex engine using pmatch().
 * [Switched to pmatchi() so as to ignore case.]
 *
 * This is a fallback ONLY and should be avoided where possible, as it results
 * in regexes not behaving as POSIX extended regular expressions. As a result,
 * configuration files for NetHacks built with this engine will not be
 * portable to ones built with an alternate regex engine.
 */

const char regex_id[] = "pmatchregex";

struct nhregex {
    const char *pat;
};

struct nhregex *
regex_init()
{
    struct nhregex *re;

    re = (struct nhregex *) alloc(sizeof (struct nhregex));
    re->pat = (const char *) 0;
    return re;
}

boolean
regex_compile(s, re)
const char *s;
struct nhregex *re;
{
    if (!re)
        return FALSE;
    if (re->pat)
        free((genericptr_t) re->pat);

    re->pat = dupstr(s);
    return TRUE;
}

const char *
regex_error_desc(re)
struct nhregex *re UNUSED;
{
    return "pattern match compilation error";
}

boolean
regex_match(s, re)
const char *s;
struct nhregex *re;
{
    if (!re || !re->pat || !s)
        return FALSE;

    return pmatchi(re->pat, s);
}

void
regex_free(re)
struct nhregex *re;
{
    if (re) {
        if (re->pat)
            free((genericptr_t) re->pat);
        free((genericptr_t) re);
    }
}
