/* GnollHack File Change Notice: This file has been changed from the original. Date of last change: 2021-09-14 */

/* GnollHack 4.0    win10.c    $NHDT-Date: 1432512810 2015/05/25 00:13:30 $  $NHDT-Branch: master $:$NHDT-Revision: 1.15 $ */
/* Copyright (C) 2018 by Bart House      */
/* GnollHack may be freely redistributed.  See license for details. */

#ifdef _MSC_VER
#include "win10.h"
#include <process.h>
#include <VersionHelpers.h>

#include "hack.h"

Win10 gWin10 = { 0 };

void win10_init()
{
    if (IsWindows10OrGreater())
    {
        HINSTANCE hUser32 = LoadLibraryA("user32.dll");

        if (hUser32 == NULL)
        {
            panic("Unable to load user32.dll");
            return;
        }

        gWin10.GetThreadDpiAwarenessContext = (GetThreadDpiAwarenessContextProc) GetProcAddress(hUser32, "GetThreadDpiAwarenessContext");
        if (gWin10.GetThreadDpiAwarenessContext == NULL)
        {
            panic("Unable to get address of GetThreadDpiAwarenessContext()");
            return;
        }
        gWin10.AreDpiAwarenessContextsEqual = (AreDpiAwarenessContextsEqualProc) GetProcAddress(hUser32, "AreDpiAwarenessContextsEqual");
        if (gWin10.AreDpiAwarenessContextsEqual == NULL)
        {
            panic("Unable to get address of AreDpiAwarenessContextsEqual");
            return;
        }

        gWin10.GetDpiForWindow = (GetDpiForWindowProc) GetProcAddress(hUser32, "GetDpiForWindow");
        if (gWin10.GetDpiForWindow == NULL)
        {
            panic("Unable to get address of GetDpiForWindow");
            return;
        }

        FreeLibrary(hUser32);

        gWin10.Valid = TRUE;
    }
#ifndef DLL_GRAPHICS
    if (gWin10.Valid) {
        if (!gWin10.AreDpiAwarenessContextsEqual(
            gWin10.GetThreadDpiAwarenessContext(),
            DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2))
        {
            panic("Unexpected DpiAwareness state");
            return;
        }
    }
#endif
}

int win10_monitor_dpi(HWND hWnd)
{
    UINT monitorDpi = 96;

    if (gWin10.Valid) {
        monitorDpi = gWin10.GetDpiForWindow(hWnd);
        if (monitorDpi == 0)
            monitorDpi = 96;
    }

    monitorDpi = max(96, monitorDpi);

    return monitorDpi;
}

double win10_monitor_scale(HWND hWnd)
{
    return (double) win10_monitor_dpi(hWnd) / 96.0;
}

void win10_monitor_info(HWND hWnd, MonitorInfo * monitorInfo)
{
    monitorInfo->scale = win10_monitor_scale(hWnd);

    HMONITOR monitor = MonitorFromWindow(hWnd, MONITOR_DEFAULTTONEAREST);
    MONITORINFO info;
    info.cbSize = sizeof(MONITORINFO);
    BOOL success = GetMonitorInfo(monitor, &info);
    nhassert(success);
    monitorInfo->width = info.rcMonitor.right - info.rcMonitor.left;
    monitorInfo->height = info.rcMonitor.bottom - info.rcMonitor.top;
    monitorInfo->left = info.rcMonitor.left;
    monitorInfo->top = info.rcMonitor.top;
}
#endif /* _MSC_VER */
