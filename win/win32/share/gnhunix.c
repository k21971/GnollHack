/* 
 * droidunix.c
 * based on unixunix.c
 */

/* This file collects some Unix dependencies */

#include "hack.h"    /* mainly for index() which depends on BSD */

#include <errno.h>
#include <sys/stat.h>
#if defined(NO_FILE_LINKS) || defined(SUNOS4) || defined(POSIX_TYPES)
#include <fcntl.h>
#endif

void
regularize(s)    /* normalize file name - we don't like .'s, /'s, spaces */
char *s;
{
    register char *lp;

    while((lp=index(s, '.')) || (lp=index(s, '/')) || (lp=index(s,' ')) || (lp=index(s,':')))
        *lp = '_';
}

static int
eraseoldlocks()
{
    register int i;

    /* cannot use maxledgerno() here, because we need to find a lock name
     * before starting everything (including the dungeon initialization
     * that sets astral_level, needed for maxledgerno()) up
     */
    for(i = 1; i <= MAXDUNGEON*MAXLEVEL + 1; i++) {
        /* try to remove all */
        set_levelfile_name(lock, i);
        (void) unlink(fqname(lock, LEVELPREFIX, 0));
    }
    set_levelfile_name(lock, 0);
    if (unlink(fqname(lock, LEVELPREFIX, 0)))
        return(0);                /* cannot remove it */
    return(1);                    /* success! */
}

void
getlock()
{
    register int fd;
    const char *fq_lock;

    if (!lock_file(HLOCK, LOCKPREFIX, 10))
    {
        wait_synch();
        error("%s", "");
    }

    regularize(lock);
    set_levelfile_name(lock, 0);

    fq_lock = fqname(lock, LEVELPREFIX, 0);
    if((fd = open(fq_lock, 0)) == -1)
    {
        if(errno == ENOENT)
            goto gotlock;    /* no such file */
        perror(fq_lock);
        unlock_file(HLOCK);
        error("Cannot open %s", fq_lock);
    }
    (void) close(fd);

    if(!recover_savefile())
    {
        (void) eraseoldlocks();
        unlock_file(HLOCK);
        error("Couldn't recover old game.");
    }

gotlock:
    (void) eraseoldlocks();
    fd = creat(fq_lock, FCMASK);
    unlock_file(HLOCK);
    if(fd == -1)
    {
        error("cannot creat lock file (%s).", fq_lock);
    }
    else
    {
        //debuglog("created lock(%s)", fq_lock);

        if(write(fd, (genericptr_t) &hackpid, sizeof(hackpid)) != sizeof(hackpid))
        {
            error("cannot write lock (%s)", fq_lock);
        }
        if(close(fd) == -1)
        {
            error("cannot close lock (%s)", fq_lock);
        }
    }
}

int
dosh()
{
    return 0;
}

void
check_crash()
{
    if (!*recovery_plname)
        return;

    //list_files();

    char recover_lock[BUFSIZ] = "";
    if (!lock_file(HLOCK, LOCKPREFIX, 10))
    {
        wait_synch();
        error("%s", "");
    }

    if (!wizard)
        Sprintf(recover_lock, "%d%s", (int)getuid(), recovery_plname);

    register int fd;
    const char* fq_lock;

    regularize(recover_lock);
    set_levelfile_name(recover_lock, 0);

    fq_lock = fqname(recover_lock, LEVELPREFIX, 0);
    if ((fd = open(fq_lock, 0)) == -1)
    {
        if (errno == ENOENT)
            goto nofilefound;    /* no such file */
        perror(fq_lock);
        unlock_file(HLOCK);
        error("Cannot open %s", fq_lock);
    }
    (void)close(fd);

    struct special_view_info info = { 0 };
    info.viewtype = SPECIAL_VIEW_CRASH_DETECTED;
    (void)open_special_view(info);

    /* Use recover_plname instead */
    if(!wizard)
        strcpy(plname, recovery_plname);

nofilefound:
    unlock_file(HLOCK);
}

