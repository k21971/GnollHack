/* GnollHack File Change Notice: This file has been changed from the original. Date of last change: 2021-09-14 */

/* GnollHack 4.0	mhstatus.h	$NHDT-Date: 1432512800 2015/05/25 00:13:20 $  $NHDT-Branch: master $:$NHDT-Revision: 1.10 $ */
/* Copyright (C) 2001 by Alex Kompel 	 */
/* GnollHack may be freely redistributed.  See license for details. */

#ifndef MSWINStatusWindow_h
#define MSWINStatusWindow_h

#include "winMS.h"
#include "config.h"
#include "global.h"

HWND mswin_init_status_window();
void mswin_status_window_size(HWND hWnd, LPSIZE sz);

#endif /* MSWINStatusWindow_h */
