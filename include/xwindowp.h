/* GnollHack File Change Notice: This file has been changed from the original. Date of last change: 2021-09-14 */

/* GnollHack 4.0	xwindowp.h	$NHDT-Date: 1432512779 2015/05/25 00:12:59 $  $NHDT-Branch: master $:$NHDT-Revision: 1.8 $ */
/* Copyright (c) Dean Luick, 1992				  */
/* GnollHack may be freely redistributed.  See license for details. */

#ifndef _xwindowp_h
#define _xwindowp_h

#include "xwindow.h"

#ifndef SYSV
#define PRESERVE_NO_SYSV /* X11 include files may define SYSV */
#endif

/* include superclass private header file */
#include <X11/CoreP.h>

#ifdef PRESERVE_NO_SYSV
#ifdef SYSV
#undef SYSV
#endif
#undef PRESERVE_NO_SYSV
#endif

/* define unique representation types not found in <X11/StringDefs.h> */

#define XtRWindowResource "WindowResource"

typedef struct {
    int empty;
} WindowClassPart;

typedef struct _WindowClassRec {
    CoreClassPart core_class;
    WindowClassPart window_class;
} WindowClassRec;

extern WindowClassRec windowClassRec;

typedef struct {
    /* resources */
    Dimension rows;
    Dimension columns;
    Pixel foreground;
    Pixel black;
    Pixel red;
    Pixel green;
    Pixel brown;
    Pixel blue;
    Pixel magenta;
    Pixel cyan;
    Pixel gray;
    Pixel orange;
    Pixel bright_green;
    Pixel yellow;
    Pixel bright_blue;
    Pixel bright_magenta;
    Pixel bright_cyan;
    Pixel white;
    XFontStruct *font;
    XtCallbackList expose_callback;
    XtCallbackList input_callback;
    XtCallbackList resize_callback;
    /* private state */
    /* (none) */
} WindowPart;

typedef struct _WindowRec {
    CorePart core;
    WindowPart window;
} WindowRec;

#endif /* _xwindowp_h */
