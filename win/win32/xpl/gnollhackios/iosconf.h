/* GnollHack File Change Notice: This file has been changed from the original. Date of last change: 2024-08-11 */

/*	SCCS Id: @(#)iosconf.h	3.4	2011/03/31	*/
/* Copyright (c) Kenneth Lorber, Bethesda, Maryland, 1990, 1991, 1992, 1993. */
/* GnollHack may be freely redistributed.  See license for details. */

#ifdef GNH_IOS
#ifndef IOSCONF_H
#define IOSCONF_H

#define error debuglog

#define NO_FILE_LINKS /* if no hard links */
#define LOCKDIR "." /* where to put locks */ 
//#define HOLD_LOCKFILE_OPEN	/* Keep an exclusive lock on the .0 file */
#define SELF_RECOVER		/* Allow the game itself to recover from an aborted game */ 

#ifdef getchar
#undef getchar
#endif
#define getchar nhgetch
#undef tgetch
#define tgetch nhgetch
#define getuid() 1

#undef SHELL				/* we do not support the '!' command */

#if defined (DUMPLOG) || defined (DUMPHTML)
#ifndef ALLOW_SNAPSHOT
#define ALLOW_SNAPSHOT
#endif
#ifndef WRITE_SNAPSHOT_JSON
#define WRITE_SNAPSHOT_JSON
#endif

#ifdef DUMPLOG_FILE
#undef DUMPLOG_FILE
#endif
#define DUMPLOG_FILE        "dumplog/gnollhack.%n.%d.txt" /* Note: Actually the one in sysconf is used, not this one */

#ifdef SNAPSHOT_FILE
#undef SNAPSHOT_FILE
#endif
#define SNAPSHOT_FILE        "snapshot/gnollhack.%n.%d.%D.txt" /* Note: Actually the one in sysconf is used, not this one */

#ifdef DUMPLOG_DIR
#undef DUMPLOG_DIR
#endif
#define DUMPLOG_DIR        "dumplog" /* Note: this is just used to create a directory, DUMPLOG_FILE still needs to have the full path */

#ifdef SNAPSHOT_DIR
#undef SNAPSHOT_DIR
#endif
#define SNAPSHOT_DIR        "snapshot" /* Note: this is just used to create a directory, SNAPSHOT_FILE still needs to have the full path */
#endif

//#undef MAIL
//#undef DEF_PAGER
#undef DEF_MAILREADER

#define ASCIIGRAPH

#define NO_SIGNAL

#define SELECTSAVED

#define USER_SOUNDS

#define CHANGE_COLOR

#define CONTINUE_PLAYING_AFTER_SAVING

#ifndef HAS_STDINT_H
#define HAS_STDINT_H    /* force include of stdint.h in integer.h */
#endif

#endif /* IOSCONF_H */
#endif /* GNH_IOS */
