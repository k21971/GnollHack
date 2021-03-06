/* GnollHack File Change Notice: This file has been changed from the original. Date of last change: 2021-09-14 */

/* GnollHack 4.0	micro.h	$NHDT-Date: 1524689515 2018/04/25 20:51:55 $  $NHDT-Branch: NetHack-3.6.0 $:$NHDT-Revision: 1.10 $ */
/*      Copyright (c) 2015 by Kenneth Lorber              */
/* GnollHack may be freely redistributed.  See license for details. */

/* micro.h - function declarations for various microcomputers */

#ifndef MICRO_H
#define MICRO_H

extern const char *alllevels, *allbones;
extern char levels[], bones[], permbones[], hackdir[];

extern int ramdisk;

#ifndef C
#define C(c) (0x40 & (c) ? 0x1f & (c) : (0x80 | (0x1f & (c))))
#endif
#ifndef M
#define M(c) (((char) 0x80) | (c))
#endif
#define ABORT C('a')

#endif /* MICRO_H */
