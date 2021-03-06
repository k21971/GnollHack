/* GnollHack File Change Notice: This file has been changed from the original. Date of last change: 2021-09-14 */

/* GnollHack 4.0	gnaskstr.h	$NHDT-Date: 1432512806 2015/05/25 00:13:26 $  $NHDT-Branch: master $:$NHDT-Revision: 1.8 $ */
/* Copyright (C) 1998 by Erik Andersen <andersee@debian.org> */
/* GnollHack may be freely redistributed.  See license for details. */

#ifndef GnomeHackAskStringDialog_h
#define GnomeHackAskStringDialog_h

int ghack_ask_string_dialog(const char *szMessageStr,
                            const char *szDefaultStr, const char *szTitleStr,
                            char *buffer);

#endif /* GnomeHackAskStringDialog_h */
