/* GnollHack File Change Notice: This file has been changed from the original. Date of last change: 2022-06-05 */

/* GnollHack 4.0    panic.c    $NHDT-Date: 1448210012 2015/11/22 16:33:32 $  $NHDT-Branch: master $:$NHDT-Revision: 1.10 $ */
/* Copyright (c) Stichting Mathematisch Centrum, Amsterdam, 1985. */
/*-Copyright (c) Robert Patrick Rankin, 2015. */
/* GnollHack may be freely redistributed.  See license for details. */

/*
 *      This code was adapted from the code in end.c to run in a standalone
 *      mode for the makedefs / drg code.
 */

#if defined(GNOLLHACK_MAIN_PROGRAM) && (defined(__BEOS__) || defined(MICRO) || defined(OS2) || defined(ANDROID) || defined(GNH_MOBILE) || defined(WIN32))
extern void FDECL(gnollhack_exit, (int));
#else
#ifndef gnollhack_exit
#define gnollhack_exit exit
#endif
#endif

#define NEED_VARARGS
#include "config.h"

#ifdef AZTEC
#define abort() exit()
#endif
#ifdef VMS
extern void NDECL(vms_abort);
#endif

/*VARARGS1*/
boolean panicking;
void VDECL(panic, (const char *, ...));

void panic
VA_DECL(const char *, str)
{
    VA_START(str);
    VA_INIT(str, char *);
    if (panicking++)
#ifdef SYSV
        (void)
#endif
            abort(); /* avoid loops - this should never happen*/

    (void) fputs(" ERROR:  ", stderr);
    Vfprintf(stderr, str, VA_ARGS);
    (void) fflush(stderr);
#if defined(UNIX) || defined(VMS)
#ifdef SYSV
    (void)
#endif
        abort(); /* generate core dump */
#endif
    VA_END();

    gnollhack_exit(EXIT_FAILURE);
}

#ifdef ALLOCA_HACK
/*
 * In case bison-generated foo_yacc.c tries to use alloca(); if we don't
 * have it then just use malloc() instead.  This may not work on some
 * systems, but they should either use yacc or get a real alloca routine.
 */
long *
alloca(cnt)
unsigned cnt;
{
    return cnt ? alloc((size_t)cnt) : (long *) 0;
}
#endif

/*panic.c*/
