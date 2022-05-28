/* GnollHack File Change Notice: This file has been changed from the original. Date of last change: 2022-04-16 */

/* GnollHack 4.0    mkobj.c    $NHDT-Date: 1548978605 2019/01/31 23:50:05 $  $NHDT-Branch: GnollHack-3.6.2-beta01 $:$NHDT-Revision: 1.142 $ */
/* Copyright (c) Stichting Mathematisch Centrum, Amsterdam, 1985. */
/*-Copyright (c) Derek S. Ray, 2015. */
/* GnollHack may be freely redistributed.  See license for details. */

#include "hack.h"

STATIC_DCL void FDECL(mkbox_cnts, (struct obj *));
STATIC_DCL unsigned FDECL(nextoid, (struct obj *, struct obj *));
STATIC_DCL void FDECL(maybe_adjust_light, (struct obj *, int));
STATIC_DCL void FDECL(maybe_adjust_sound_volume, (struct obj*, double));
STATIC_DCL void FDECL(obj_timer_checks, (struct obj *,
                                         XCHAR_P, XCHAR_P, int));
STATIC_DCL void FDECL(container_weight, (struct obj *));
STATIC_DCL struct obj *FDECL(save_mtraits, (struct obj *, struct monst *));
STATIC_DCL void FDECL(objlist_sanity, (struct obj *, int, const char *));
STATIC_DCL void FDECL(mon_obj_sanity, (struct monst *, const char *));
STATIC_DCL const char *FDECL(where_name, (struct obj *));
STATIC_DCL void FDECL(insane_object, (struct obj *, const char *,
                                      const char *, struct monst *));
STATIC_DCL void FDECL(check_contained, (struct obj *, const char *));
STATIC_DCL void FDECL(sanity_check_worn, (struct obj *));

struct icp {
    int iprob;   /* probability of an item type */
    char iclass; /* item class */
};

static const struct icp mkobjprobs[] = { { 12, WEAPON_CLASS },
                                         { 12, ARMOR_CLASS },
                                         { 11, FOOD_CLASS },
                                         { 5, TOOL_CLASS },
                                         { 5, GEM_CLASS },
                                         { 12, POTION_CLASS },
                                         { 12, SCROLL_CLASS },
                                         { 11, SPBOOK_CLASS },
                                         { 2, WAND_CLASS },
                                         { 5, RING_CLASS },
                                         { 6, REAGENT_CLASS },
                                         { 5, MISCELLANEOUS_CLASS },
                                         { 2, AMULET_CLASS } };

static const struct icp boxiprobs[] = { { 12, GEM_CLASS },
                                        { 12, FOOD_CLASS },
                                        { 10, POTION_CLASS },
                                        { 12, SCROLL_CLASS },
                                        { 14, SPBOOK_CLASS },
                                        { 10, COIN_CLASS },
                                        { 12, WAND_CLASS },
                                        { 6, RING_CLASS },
                                        { 4, REAGENT_CLASS },
                                           { 6, MISCELLANEOUS_CLASS },
                                        { 2, AMULET_CLASS } };

static const struct icp rogueprobs[] = { { 12, WEAPON_CLASS },
                                         { 12, ARMOR_CLASS },
                                         { 22, FOOD_CLASS },
                                         { 22, POTION_CLASS },
                                         { 22, SCROLL_CLASS },
                                         { 5, WAND_CLASS },
                                         { 5, RING_CLASS } };

static const struct icp hellprobs[] = { { 15, WEAPON_CLASS },
                                        { 15, ARMOR_CLASS },
                                        { 12, FOOD_CLASS },
                                        { 12, TOOL_CLASS },
                                        { 8, GEM_CLASS },
                                        { 1, POTION_CLASS },
                                        { 1, SCROLL_CLASS },
                                        { 8, REAGENT_CLASS },
                                        { 8, WAND_CLASS },
                                        { 8, RING_CLASS },
                                        { 8, MISCELLANEOUS_CLASS },
                                        { 4, AMULET_CLASS } };

struct oextra *
newoextra()
{
    struct oextra *oextra;

    oextra = (struct oextra *) alloc(sizeof (struct oextra));
    oextra->oname = 0;
    oextra->uoname = 0;
    oextra->omonst = 0;
    oextra->omid = 0;
    oextra->olong = 0;
    oextra->omailcmd = 0;
    return oextra;
}

void
dealloc_oextra(o)
struct obj *o;
{
    struct oextra *x = o->oextra;

    if (x) {
        if (x->oname)
            free((genericptr_t) x->oname);
        if (x->uoname)
            free((genericptr_t)x->uoname);
        if (x->omonst)
            free_omonst(o);     /* 'o' rather than 'x' */
        if (x->omid)
            free((genericptr_t) x->omid);
        if (x->olong)
            free((genericptr_t) x->olong);
        if (x->omailcmd)
            free((genericptr_t) x->omailcmd);

        free((genericptr_t) x);
        o->oextra = (struct oextra *) 0;
    }
}

void
newomonst(otmp)
struct obj *otmp;
{
    if (!otmp->oextra)
        otmp->oextra = newoextra();

    if (!OMONST(otmp)) {
        struct monst *m = newmonst();

        *m = zeromonst;
        OMONST(otmp) = m;
    }
}

void
free_omonst(otmp)
struct obj *otmp;
{
    if (otmp->oextra) {
        struct monst *m = OMONST(otmp);

        if (m) {
            if (m->mextra)
                dealloc_mextra(m);
            free((genericptr_t) m);
            OMONST(otmp) = (struct monst *) 0;
        }
    }
}

void
newomid(otmp)
struct obj *otmp;
{
    if (!otmp->oextra)
        otmp->oextra = newoextra();
    if (!OMID(otmp)) {
        OMID(otmp) = (unsigned *) alloc(sizeof (unsigned));
        (void) memset((genericptr_t) OMID(otmp), 0, sizeof (unsigned));
    }
}

void
free_omid(otmp)
struct obj *otmp;
{
    if (otmp->oextra && OMID(otmp)) {
        free((genericptr_t) OMID(otmp));
        OMID(otmp) = (unsigned *) 0;
    }
}

void
newolong(otmp)
struct obj *otmp;
{
    if (!otmp->oextra)
        otmp->oextra = newoextra();
    if (!OLONG(otmp)) {
        OLONG(otmp) = (long *) alloc(sizeof (long));
        (void) memset((genericptr_t) OLONG(otmp), 0, sizeof (long));
    }
}

void
free_olong(otmp)
struct obj *otmp;
{
    if (otmp->oextra && OLONG(otmp)) {
        free((genericptr_t) OLONG(otmp));
        OLONG(otmp) = (long *) 0;
    }
}

void
new_omailcmd(otmp, response_cmd)
struct obj *otmp;
const char *response_cmd;
{
    if (!otmp->oextra)
        otmp->oextra = newoextra();
    if (OMAILCMD(otmp))
        free_omailcmd(otmp);
    OMAILCMD(otmp) = dupstr(response_cmd);
}

void
free_omailcmd(otmp)
struct obj *otmp;
{
    if (otmp->oextra && OMAILCMD(otmp)) {
        free((genericptr_t) OMAILCMD(otmp));
        OMAILCMD(otmp) = (char *) 0;
    }
}

struct obj *
mkobj_at_with_flags(let, x, y, artif, mkflags)
char let;
int x, y;
boolean artif;
unsigned long mkflags;
{
    if (!isok(x, y))
        return (struct obj*)0;

    struct obj *otmp;

    otmp = mkobj_with_flags(let, artif, FALSE, mkflags);
    if (otmp)
    {
        place_object(otmp, x, y);
        if ((objects[otmp->otyp].oc_flags5 & O5_TILE_IS_TILESET_DEPENDENT) != 0)
        {
            otmp->has_special_tileset = 1;
            otmp->special_tileset = levl[x][y].use_special_tileset ? levl[x][y].special_tileset : get_current_cmap_type_index();
        }
    }
    return otmp;
}

struct obj *
mkobj_at(let, x, y, artif)
char let;
int x, y;
boolean artif;
{
    return mkobj_at_with_flags(let, x, y, artif, 0UL);
}

struct obj *
mksobj_at(otyp, x, y, init, artif)
int otyp, x, y;
boolean init, artif;
{
    return mksobj_at_with_flags(otyp, x, y, init, artif, 0, 0, 0UL);
}

struct obj*
mksobj_at_with_flags(otyp, x, y, init, artif, mkobj_type, param, mkflags)
int otyp, x, y, mkobj_type, param;
boolean init, artif;
unsigned long mkflags;
{
    if (!isok(x, y))
        return (struct obj*)0;

    struct obj* otmp;

    otmp = mksobj_with_flags(otyp, init, artif, mkobj_type, param, mkflags);
    if (otmp)
    {
        place_object(otmp, x, y);
        if ((objects[otmp->otyp].oc_flags5 & O5_TILE_IS_TILESET_DEPENDENT) != 0)
        {
            otmp->has_special_tileset = 1;
            otmp->special_tileset = levl[x][y].use_special_tileset ? levl[x][y].special_tileset : get_current_cmap_type_index();
        }
    }
    return otmp;
}

struct obj *
mksobj_migr_to_species(otyp, mflags2, init, artif)
int otyp;
unsigned mflags2;
boolean init, artif;
{
    struct obj *otmp;

    otmp = mksobj(otyp, init, artif, FALSE);
    if (otmp) {
        add_to_migration(otmp);
        otmp->owornmask = (long) MIGR_TO_SPECIES;
        otmp->corpsenm = mflags2;
    }
    return otmp;
}


/* mkobj(): select a type of item from a class, use mksobj() to create it */
/* mkobj_type = 0 making contents on the floor, = 1 making box contents, = 2 wishing, = 3 forging */
struct obj*
mkobj(oclass, artif, mkobj_type)
char oclass;
boolean artif;
int mkobj_type;
{
    return mkobj_with_flags(oclass, artif, mkobj_type, 0UL);
}

struct obj *
mkobj_with_flags(oclass, artif, mkobj_type, mkflags)
char oclass;
boolean artif;
int mkobj_type;
unsigned long mkflags;
{
    int tprob;
    int i = 0;
    unsigned long rndflags = 0UL;
    if (mkflags & MKOBJ_FLAGS_ALSO_RARE)
        rndflags |= RNDITEM_FLAGS_ALSO_RARE;

    for (int try_ct = 0; try_ct < 20; try_ct++)
    {
        if (oclass == RANDOM_CLASS) {
            const struct icp* iprobs = Is_really_rogue_level(&u.uz)
                ? (const struct icp*)rogueprobs
                : Inhell ? (const struct icp*)hellprobs
                : (const struct icp*)mkobjprobs;

            for (tprob = rnd(100); (tprob -= iprobs->iprob) > 0; iprobs++)
                ;
            oclass = iprobs->iclass;
        }

        i = random_objectid_from_class(oclass, rndflags);

        if (mkobj_type == 1 && (objects[i].oc_flags2 & O2_CONTAINER)) /* No containers in containers */
            continue;
        else if ((mkflags & MKOBJ_FLAGS_SHOP_ITEM) && (objects[i].oc_flags4 & O4_NO_CLASS_GENERATION_IN_SHOP) && try_ct < 15) /* No inappropriate items for shops */
            continue;
        else
            break;
    }
    return mksobj_with_flags(i, TRUE, artif, mkobj_type, 0, mkflags);
}

int
random_objectid_from_class(oclass, rndflags)
char oclass;
unsigned long rndflags;
{
    int i = 0;
    boolean also_rare = !!(rndflags & RNDITEM_FLAGS_ALSO_RARE);
    xchar leveldif = level_difficulty();

    int randomizationclass = rn2(4);

    for (int tryct = 0; tryct < 50; tryct++)
    {
        boolean unacceptable = FALSE;
        boolean breakforloop = TRUE;

        int prob = rnd(1000);
        i = bases[(int)oclass];
        while ((prob -= objects[i].oc_prob) > 0)
            i++;

        if (objects[i].oc_class != oclass || !OBJ_NAME(objects[i]))
        {
            panic("probtype error, oclass=%d i=%d", (int)oclass, i);
            return STRANGE_OBJECT;
        }

        if (objects[i].oc_flags3 & O3_NO_GENERATION)
        {
            i = 0;
            continue; /* new try */
        }

        if (!also_rare && (objects[i].oc_flags4 & O4_RARE) && leveldif < 6 && tryct < 25)
        {
            i = 0;
            continue; /* new try */
        }

        if (!also_rare && (objects[i].oc_flags4 & O4_VERY_RARE) && leveldif < 12 && tryct < 25)
        {
            i = 0;
            continue; /* new try */
        }

        /* Special code generating more relevant spellbooks */
        if (oclass == SPBOOK_CLASS && randomizationclass < 3)
        {
            switch (randomizationclass)
            {
            case 0: /* Disregard spell books of too low and high level, stat, and school */
                if (P_SKILL_LEVEL(objects[i].oc_skill) < P_BASIC)
                    unacceptable = TRUE;
                /* FALLTHRU */
            case 1: /* Disregard spell books of too low and high level and stat */
                if ((Role_if(PM_WIZARD) && !(objects[i].oc_spell_attribute == A_INT
                    || objects[i].oc_spell_attribute == A_MAX_INT_WIS
                    || objects[i].oc_spell_attribute == A_MAX_INT_CHA
                    || objects[i].oc_spell_attribute == A_MAX_INT_WIS_CHA
                    || objects[i].oc_spell_attribute == A_AVG_INT_WIS
                    || objects[i].oc_spell_attribute == A_AVG_INT_CHA
                    || objects[i].oc_spell_attribute == A_AVG_INT_WIS_CHA
                    ))
                    || (Role_if(PM_PRIEST) && !(objects[i].oc_spell_attribute == A_WIS
                        || objects[i].oc_spell_attribute == A_MAX_INT_WIS
                        || objects[i].oc_spell_attribute == A_MAX_WIS_CHA
                        || objects[i].oc_spell_attribute == A_MAX_INT_WIS_CHA
                        || objects[i].oc_spell_attribute == A_AVG_INT_WIS
                        || objects[i].oc_spell_attribute == A_AVG_WIS_CHA
                        || objects[i].oc_spell_attribute == A_AVG_INT_WIS_CHA
                        ))
                    )
                    unacceptable = TRUE;
                /* FALLTHRU */
            case 2: /* Disregard spell books of too low and high level */
                if (objects[i].oc_spell_level > max(3, (u.ulevel + 1) / 2 + 2)        /* Level 1 ->  3, Level 5 ->  5, Level 11 ->  8, Level 19 -> 12*/
                    || objects[i].oc_spell_level < min(6, (u.ulevel + 1) / 2 - 4) /* Level 1 -> -3, Level 5 -> -1, Level 11 ->  2, Level 19 -> 6 */
                    )
                    unacceptable = TRUE;
                break;
            default:
                break;
            }


            if (!unacceptable)
            {
                boolean alreadyknown = FALSE;

                for (int j = 0; i < MAXSPELL && spellid(j) != NO_SPELL; j++)
                {
                    if (spellid(j) == i)
                    {
                        alreadyknown = TRUE;
                        break;
                    }
                }

                if (alreadyknown)
                {
                    switch (rn2(3))
                    {
                    case 0: /* pick another item from the same randomization class */
                        breakforloop = FALSE;
                        break;
                    case 1: /* pick another item from new randomized randomization class */
                        randomizationclass = rn2(4);
                        breakforloop = FALSE;
                        break;
                    case 2: /* we make a spellbook for a spell that the player already knows */
                        breakforloop = TRUE;
                        break;
                    }
                }
            }
        }
        if (breakforloop)
            break; /* stop the for loop and make the item */
    }

    return i;
}

STATIC_OVL void
mkbox_cnts(box)
struct obj *box;
{
    register int n;
    register struct obj *otmp;

    //box->cobj = (struct obj *) 0; /* Box may have previous contents, such as a coffin corpse */

    switch (box->otyp) 
    {
    case ICE_BOX:
        n = 20;
        break;
    case BOOKSHELF:
        n = !rn2(100) ? 10 : (level_difficulty() >= 13) ? 5 : (level_difficulty() >= 10) ? 4 : 3;
        break;
    case MINE_CART:
        n = !rn2(3) ? 0 : 8; /* At least one third of the mine carts are empty */
        break;
    case WEAPON_RACK:
        n = 4;
        break;
    case CHEST:
        n = box->olocked ? 7 : 5;
        break;
    case LARGE_BOX:
        n = box->olocked ? 5 : 3;
        break;
    case COFFIN:
        n = 3;
        break;
    case SARCOPHAGUS:
        n = 7;
        break;
    case SACK:
    case OILSKIN_SACK:
    case BACKPACK:
    case LEATHER_BAG:
    case ORIENTAL_SILK_SACK:
    case EXPENSIVE_HANDBAG:
    case BAG_OF_WIZARDRY:
    case BAG_OF_TREASURE_HAULING:
    case BAG_OF_THE_GLUTTON:
        /* initial inventory: sack starts out empty */
        if (moves <= 1 && !in_mklev) {
            n = 0;
            break;
        }
        /*FALLTHRU*/
    case QUIVER_OF_INFINITE_ARROWS:
    case POUCH_OF_ENDLESS_BOLTS:
    case BAG_OF_INFINITE_SLING_BULLETS:
    case BAG_OF_HOLDING:
        n = 1;
        break;
    default:
        n = 0;
        break;
    }

    for (n = rn2(n + 1); n > 0; n--) 
    {
        if (box->otyp == ICE_BOX) 
        {
            if (!(otmp = mksobj(CORPSE, TRUE, TRUE, TRUE)))
                continue;
            /* Note: setting age to 0 is correct.  Age has a different
             * from usual meaning for objects stored in ice boxes. -KAA
             */
            otmp->age = 0L;
            if (otmp->timed) 
            {
                (void)stop_timer(ROT_CORPSE, obj_to_any(otmp));
                (void)stop_timer(REVIVE_MON, obj_to_any(otmp));
            }
        }
        else if (box->otyp == BOOKSHELF) 
        {
            if (rn2(3))
                otmp = mkobj(SCROLL_CLASS, FALSE, TRUE);
            else
                otmp = mkobj(SPBOOK_CLASS, FALSE, TRUE);
        } 
        else if (box->otyp == MINE_CART)
        {
            if (!rn2(2))
            {
                otmp = mksobj(ROCK, TRUE, FALSE, 1); /* Half the contents are just rocks */
            }
            else if (!rn2(2))
            {
                otmp = mksobj(rnd_class(NUGGET_OF_IRON_ORE, NUGGET_OF_MITHRIL_ORE), TRUE, FALSE, 1); /* Lots of ores */
            }
            else
            {
                otmp = mkobj(GEM_CLASS, FALSE, TRUE); /* And some GEM_CLASS stuff in general */
            }
        }
        else if (box->otyp == WEAPON_RACK)
        {
            otmp = mkobj(WEAPON_CLASS, TRUE, TRUE);
        }
        else if (box->otyp == SARCOPHAGUS || box->otyp == COFFIN)
        {
            char item_classes[5] = { COIN_CLASS, GEM_CLASS, MISCELLANEOUS_CLASS, AMULET_CLASS, RING_CLASS };
            otmp = mkobj(item_classes[rn2(5)], TRUE, TRUE);
            if (otmp->oclass == COIN_CLASS)
            {
                /* 2.5 x level's usual amount; weight adjusted below */
                otmp->quan = (long)(rnd(level_difficulty() + 2) * rnd(75));
                otmp->owt = weight(otmp);
            }
        }
        else
        {
            register int tprob;
            const struct icp *iprobs = boxiprobs;

            for (tprob = rnd(100); (tprob -= iprobs->iprob) > 0; iprobs++)
                ;
            if (!(otmp = mkobj(iprobs->iclass, TRUE, TRUE)))
                continue;

            /* handle a couple of special cases */
            if (otmp->oclass == COIN_CLASS)
            {
                /* 2.5 x level's usual amount; weight adjusted below */
                otmp->quan = (long) (rnd(level_difficulty() + 2) * rnd(75));
                otmp->owt = weight(otmp);
            }
            else
                while (is_rock(otmp))
                {
                    otmp->otyp = rnd_class(DILITHIUM_CRYSTAL, JINXSTONE);
                    if (otmp->quan > 2L)
                        otmp->quan = 1L;
                    otmp->owt = weight(otmp);
                }

            if (Is_weight_changing_bag(box)) 
            {
                if (Is_mbag(otmp)) 
                {
                    otmp->otyp = SACK;
                    otmp->enchantment = 0;
                    otmp->special_quality = 0;
                    otmp->charges = 0;
                    otmp->speflags = 0;
                    otmp->owt = weight(otmp);
                } 
                else
                    while (otmp->otyp == WAN_CANCELLATION || otmp->otyp == WAN_DISJUNCTION)
                        otmp->otyp = rnd_class(WAN_LIGHT, WAN_LIGHTNING);
            }
        }
        (void) add_to_container(box, otmp);
    }
}

/* select a random, common monster type, usually for a corpse */
int
rndmonnum()
{
    register struct permonst *ptr;
    register int i;
    unsigned short excludeflags;

    /* Plan A: get a level-appropriate common monster */
    ptr = rndmonst();
    if (ptr)
        return monsndx(ptr);

    /* Plan B: get any common monster */
    excludeflags = G_UNIQ | G_NOGEN | (Inhell ? G_NOHELL : G_HELL) | (In_mines(&u.uz) ? G_NOMINES : 0);
    do {
        i = rn1(SPECIAL_PM - LOW_PM, LOW_PM);
        ptr = &mons[i];
    } while ((ptr->geno & excludeflags &&
        !((In_modron_level(&u.uz) && (ptr->geno & G_MODRON)) || (In_bovine_level(&u.uz)  && (ptr->geno & G_YACC)))
        ) != 0);

    return i;
}

void
copy_oextra(obj2, obj1)
struct obj *obj2, *obj1;
{
    if (!obj2 || !obj1 || !obj1->oextra)
        return;

    if (!obj2->oextra)
        obj2->oextra = newoextra();
    if (has_oname(obj1))
        oname(obj2, ONAME(obj1));
    if (has_uoname(obj1))
        uoname(obj2, UONAME(obj1));
    if (has_omonst(obj1))
    {
        if (!OMONST(obj2))
            newomonst(obj2);
        if (OMONST(obj2))
        {
            (void)memcpy((genericptr_t)OMONST(obj2),
                (genericptr_t)OMONST(obj1), sizeof(struct monst));
            OMONST(obj2)->mextra = (struct mextra*) 0;
            OMONST(obj2)->nmon = (struct monst*) 0;
#if 0 /* Let's keep the original m_id */
            OMONST(obj2)->m_id = context.ident++;
            if (OMONST(obj2)->m_id) /* ident overflowed */
                OMONST(obj2)->m_id = context.ident++;
#endif
        }
        if (OMONST(obj1)->mextra)
            copy_mextra(OMONST(obj2), OMONST(obj1));
    }
    if (has_omid(obj1)) 
    {
        if (!OMID(obj2))
            newomid(obj2);
        if(OMID(obj2))
            (void) memcpy((genericptr_t) OMID(obj2), (genericptr_t) OMID(obj1),
                      sizeof (unsigned));
    }
    if (has_olong(obj1)) {
        if (!OLONG(obj2))
            newolong(obj2);
        if(OLONG(obj2))
            (void) memcpy((genericptr_t) OLONG(obj2), (genericptr_t) OLONG(obj1),
                      sizeof (long));
    }
    if (has_omailcmd(obj1)) {
        new_omailcmd(obj2, OMAILCMD(obj1));
    }
}

/*
 * Split obj so that it gets size gets reduced by num. The quantity num is
 * put in the object structure delivered by this call.  The returned object
 * has its wornmask cleared and is positioned just following the original
 * in the nobj chain (and nexthere chain when on the floor).
 */
struct obj *
splitobj(obj, num)
struct obj *obj;
long num;
{
    struct obj *otmp;

    if (obj->cobj || num <= 0L || obj->quan <= num)
    {
        panic("splitobj"); /* can't split containers */
        return (struct obj*)0;
    }
    otmp = newobj();
    *otmp = *obj; /* copies whole structure */
    otmp->oextra = (struct oextra *) 0;
    otmp->o_id = nextoid(obj, otmp);
    otmp->timed = 0;                  /* not timed, yet */
    otmp->lamplit = 0;                /* ditto */
    otmp->makingsound = 0;                /* ditto */
    otmp->owornmask = 0L;             /* new object isn't worn */
    obj->quan -= num;
    obj->owt = weight(obj);
    otmp->quan = num;
    otmp->owt = weight(otmp); /* -= obj->owt ? */

    context.objsplit.parent_oid = obj->o_id;
    context.objsplit.child_oid = otmp->o_id;
    obj->nobj = otmp;
    /* Only set nexthere when on the floor, nexthere is also used */
    /* as a back pointer to the container object when contained. */
    if (obj->where == OBJ_FLOOR)
        obj->nexthere = otmp;
    copy_oextra(otmp, obj);
    if (has_omid(otmp))
        free_omid(otmp); /* only one association with m_id*/
    if (obj->unpaid)
        splitbill(obj, otmp);
    if (obj->timed)
        obj_split_timers(obj, otmp);
    if (obj_sheds_light(obj))
        obj_split_light_source(obj, otmp);
    if (obj_has_sound_source(obj))
        obj_split_sound_source(obj, otmp);
    return otmp;
}

/* when splitting a stack that has o_id-based shop prices, pick an
   o_id value for the new stack that will maintain the same price */
STATIC_OVL unsigned
nextoid(oldobj, newobj)
struct obj *oldobj, *newobj;
{
    int olddif, newdif, trylimit = 256; /* limit of 4 suffices at present */
    unsigned oid = context.ident - 1; /* loop increment will reverse -1 */

    olddif = oid_price_adjustment(oldobj, oldobj->o_id);
    do {
        ++oid;
        if (!oid) /* avoid using 0 (in case value wrapped) */
            ++oid;
        newdif = oid_price_adjustment(newobj, oid);
    } while (newdif != olddif && --trylimit >= 0);
    context.ident = oid + 1; /* ready for next new object */
    return oid;
}

/* try to find the stack obj was split from, then merge them back together;
   returns the combined object if unsplit is successful, null otherwise */
struct obj *
unsplitobj(obj)
struct obj *obj;
{
    unsigned target_oid = 0;
    struct obj *oparent = 0, *ochild = 0, *list = 0;

    /*
     * We don't operate on floor objects (we're following o->nobj rather
     * than o->nexthere), on free objects (don't know which list to use when
     * looking for obj's parent or child), on bill objects (too complicated,
     * not needed), or on buried or migrating objects (not needed).
     * [This could be improved, but at present additional generality isn't
     * necessary.]
     */
    switch (obj->where) {
    case OBJ_FREE:
    case OBJ_FLOOR:
    case OBJ_ONBILL:
    case OBJ_HEROMEMORY:
    case OBJ_MIGRATING:
    case OBJ_BURIED:
    default:
        return (struct obj *) 0;
    case OBJ_INVENT:
        list = invent;
        break;
    case OBJ_MINVENT:
        list = obj->ocarry->minvent;
        break;
    case OBJ_CONTAINED:
        list = obj->ocontainer->cobj;
        break;
    }

    /* first try the expected case; obj is split from another stack */
    if (obj->o_id == context.objsplit.child_oid) {
        /* parent probably precedes child and will require list traversal */
        ochild = obj;
        target_oid = context.objsplit.parent_oid;
        if (obj->nobj && obj->nobj->o_id == target_oid)
            oparent = obj->nobj;
    } else if (obj->o_id == context.objsplit.parent_oid) {
        /* alternate scenario: another stack was split from obj;
           child probably follows parent and will be found here */
        oparent = obj;
        target_oid = context.objsplit.child_oid;
        if (obj->nobj && obj->nobj->o_id == target_oid)
            ochild = obj->nobj;
    }
    /* if we have only half the split, scan obj's list to find other half */
    if (ochild && !oparent) {
        /* expected case */
        for (obj = list; obj; obj = obj->nobj)
            if (obj->o_id == target_oid) {
                oparent = obj;
                break;
            }
    } else if (oparent && !ochild) {
        /* alternate scenario */
        for (obj = list; obj; obj = obj->nobj)
            if (obj->o_id == target_oid) {
                ochild = obj;
                break;
            }
    }
    /* if we have both parent and child, try to merge them;
       if successful, return the combined stack, otherwise return null */
    return (oparent && ochild && merged(&oparent, &ochild)) ? oparent : 0;
}

/* reset splitobj()/unsplitobj() context */
void
clear_splitobjs()
{
    context.objsplit.parent_oid = context.objsplit.child_oid = 0;
}

/*
 * Insert otmp right after obj in whatever chain(s) it is on.  Then extract
 * obj from the chain(s).  This function does a literal swap.  It is up to
 * the caller to provide a valid context for the swap.  When done, obj will
 * still exist, but not on any chain.
 *
 * Note:  Don't use use obj_extract_self() -- we are doing an in-place swap,
 * not actually moving something.
 */
void
replace_object(obj, otmp)
struct obj *obj;
struct obj *otmp;
{
    otmp->where = obj->where;
    switch (obj->where) {
    case OBJ_FREE:
        /* do nothing */
        break;
    case OBJ_INVENT:
        otmp->nobj = obj->nobj;
        obj->nobj = otmp;
        extract_nobj(obj, &invent);
        break;
    case OBJ_CONTAINED:
        otmp->nobj = obj->nobj;
        otmp->ocontainer = obj->ocontainer;
        obj->nobj = otmp;
        extract_nobj(obj, &obj->ocontainer->cobj);
        break;
    case OBJ_MINVENT:
        otmp->nobj = obj->nobj;
        otmp->ocarry = obj->ocarry;
        obj->nobj = otmp;
        extract_nobj(obj, &obj->ocarry->minvent);
        break;
    case OBJ_FLOOR:
        otmp->nobj = obj->nobj;
        otmp->nexthere = obj->nexthere;
        otmp->ox = obj->ox;
        otmp->oy = obj->oy;
        obj->nobj = otmp;
        obj->nexthere = otmp;
        extract_nobj(obj, &fobj);
        extract_nexthere(obj, &level.objects[obj->ox][obj->oy]);
        break;
    case OBJ_HEROMEMORY:
        otmp->nobj = obj->nobj;
        otmp->nexthere = obj->nexthere;
        otmp->ox = obj->ox;
        otmp->oy = obj->oy;
        obj->nobj = otmp;
        obj->nexthere = otmp;
        extract_nobj(obj, &memoryobjs);
        extract_nexthere(obj, &level.locations[obj->ox][obj->oy].hero_memory_layers.memory_objchn);
        obj->lamplit = 0;
        obj->makingsound = 0;
        break;
    default:
        panic("replace_object: obj position");
        break;
    }
}

/* is 'obj' inside a container whose contents aren't known?
   if so, return the outermost container meeting that criterium */
struct obj *
unknwn_contnr_contents(obj)
struct obj *obj;
{
    struct obj *result = 0, *parent;

    while (obj->where == OBJ_CONTAINED) {
        parent = obj->ocontainer;
        if (!parent->cknown)
            result = parent;
        obj = parent;
    }
    return result;
}

/*
 * Create a dummy duplicate to put on shop bill.  The duplicate exists
 * only in the billobjs chain.  This function is used when a shop object
 * is being altered, and a copy of the original is needed for billing
 * purposes.  For example, when eating, where an interruption will yield
 * an object which is different from what it started out as; the "I x"
 * command needs to display the original object.
 *
 * The caller is responsible for checking otmp->unpaid and
 * costly_spot(u.ux, u.uy).  This function will make otmp no charge.
 *
 * Note that check_unpaid_usage() should be used instead for partial
 * usage of an object.
 */
void
bill_dummy_object(otmp)
register struct obj *otmp;
{
    register struct obj *dummy;
    long cost = 0L;

    if (otmp->unpaid) {
        cost = unpaid_cost(otmp, FALSE);
        subfrombill(otmp, shop_keeper(*u.ushops));
    }
    dummy = newobj();
    *dummy = *otmp;
    dummy->oextra = (struct oextra *) 0;
    dummy->where = OBJ_FREE;
    dummy->o_id = context.ident++;
    if (!dummy->o_id)
        dummy->o_id = context.ident++; /* ident overflowed */
    dummy->timed = 0;
    copy_oextra(dummy, otmp);
    if (has_omid(dummy))
        free_omid(dummy); /* only one association with m_id*/
    if (is_candle(dummy))
        dummy->lamplit = 0;
    dummy->makingsound = 0;
    dummy->owornmask = 0L; /* dummy object is not worn */
    addtobill(dummy, FALSE, TRUE, TRUE);
    if (cost)
        alter_cost(dummy, -cost);
    /* no_charge is only valid for some locations */
    otmp->no_charge =
        (otmp->where == OBJ_FLOOR || otmp->where == OBJ_CONTAINED) ? 1 : 0;
    otmp->unpaid = 0;
    return;
}

struct obj*
memory_dummy_object(otmp)
register struct obj* otmp;
{
    if (!otmp)
        return (struct obj*)0;

    register struct obj* dummy;
    int x = otmp->ox, y = otmp->oy;

    if (!isok(x, y))
        return (struct obj*)0;

    dummy = newobj();
    *dummy = *otmp;
    dummy->nobj = (struct obj*)0; /* set nobj to zero; this is just a copy */
    dummy->cobj = (struct obj*)0;
    add_memory_object_contents(dummy, otmp);
    dummy->nexthere = (struct obj*)0;
    dummy->oextra = (struct oextra*)0;
    dummy->where = OBJ_FREE;
    dummy->o_id_memory = otmp->o_id;
    dummy->o_id = context.ident++;
    if (!dummy->o_id)
        dummy->o_id = context.ident++; /* ident overflowed */
    dummy->timed = 0;
    copy_oextra(dummy, otmp);
    if (has_omid(dummy))
        free_omid(dummy); /* only one association with m_id*/
    dummy->owornmask = 0L; /* dummy object is not worn */

    /* Add to memoryobjs chain */
    add_to_memoryobjs(dummy);

    /* Add to nexthere memory */
    if (!level.locations[x][y].hero_memory_layers.memory_objchn)
    {
        level.locations[x][y].hero_memory_layers.memory_objchn = dummy;
    }
    else
    {
        /* Do for to preserve order */
        // dummy->nexthere = level.locations[x][y].hero_memory_layers.memory_objchn;
        // level.locations[x][y].hero_memory_layers.memory_objchn = dummy;
        for (struct obj* last_obj = level.locations[x][y].hero_memory_layers.memory_objchn; last_obj; last_obj = last_obj->nexthere)
        {
            if (last_obj->nexthere == (struct obj*)0)
            {
                last_obj->nexthere = dummy;
                break;
            }
        }
    }

    return dummy;
}

void
add_memory_object_contents(memory_obj, orig_obj)
struct obj* memory_obj, *orig_obj;
{
    if (!memory_obj || !orig_obj || !orig_obj->cobj)
        return;

    struct obj* dummy;

    for (struct obj* otmp = orig_obj->cobj; otmp; otmp = otmp->nobj)
    {
        dummy = newobj();
        *dummy = *otmp;
        dummy->nobj = (struct obj*)0;
        dummy->cobj = (struct obj*)0; /* no contains of containers in memory */
        dummy->nexthere = (struct obj*)0;
        dummy->oextra = (struct oextra*)0;
        dummy->where = OBJ_CONTAINED;
        dummy->ocontainer = memory_obj;
        dummy->o_id_memory = otmp->o_id;
        dummy->o_id = context.ident++;
        if (!dummy->o_id)
            dummy->o_id = context.ident++; /* ident overflowed */
        dummy->timed = 0;
        copy_oextra(dummy, otmp);
        if (has_omid(dummy))
            free_omid(dummy); /* only one association with m_id*/
        dummy->owornmask = 0L; /* dummy object is not worn */

        /* Add to cobj memory */
        if (!memory_obj->cobj)
        {
            memory_obj->cobj = dummy;
        }
        else
        {
            /* Do for to preserve order */
            // dummy->nexthere = level.locations[x][y].hero_memory_layers.memory_objchn;
            // level.locations[x][y].hero_memory_layers.memory_objchn = dummy;
            for (struct obj* last_obj = memory_obj->cobj; last_obj; last_obj = last_obj->nobj)
            {
                if (last_obj->nobj == (struct obj*)0)
                {
                    last_obj->nobj = dummy;
                    break;
                }
            }
        }
    }
}

void
add_to_memoryobjs(obj)
struct obj* obj;
{
    if (obj->where != OBJ_FREE)
    {
        panic("add_to_memoryobjs: obj not free");
        return;
    }
    if (obj->timed)
        obj_stop_timers(obj);

    obj->nobj = memoryobjs;
    memoryobjs = obj;
    obj->where = OBJ_HEROMEMORY;
}

void
clear_memoryobjs()
{
    struct obj* obj, *contained_obj;
    while ((obj = memoryobjs) != 0) {
        obj_extract_self(obj);
        while ((contained_obj = obj->cobj) != 0) {
            obj_extract_self(contained_obj);
            dealloc_obj(contained_obj);
        }
        dealloc_obj(obj);
    }

}

void
clear_hero_object_memory_at(x, y)
int x, y;
{
    if (isok(x, y))
    {
        /* Just in case */
        level.locations[x][y].hero_memory_layers.layer_glyphs[LAYER_OBJECT] = NO_GLYPH;
        level.locations[x][y].hero_memory_layers.layer_gui_glyphs[LAYER_OBJECT] = NO_GLYPH;
        level.locations[x][y].hero_memory_layers.layer_glyphs[LAYER_COVER_OBJECT] = NO_GLYPH;
        level.locations[x][y].hero_memory_layers.layer_gui_glyphs[LAYER_COVER_OBJECT] = NO_GLYPH;
        level.locations[x][y].hero_memory_layers.o_id = 0;

        /* Clear actual memory objects */
        struct obj* obj, * contained_obj;
        while ((obj = level.locations[x][y].hero_memory_layers.memory_objchn) != 0)
        {
            obj_extract_self(obj);
            while ((contained_obj = obj->cobj) != 0) {
                obj_extract_self(contained_obj);
                dealloc_obj(contained_obj);
            }
            dealloc_obj(obj);
        }
    }
}

/* alteration types; must match COST_xxx macros in hack.h */
static const char *const alteration_verbs[] = {
    "cancel", "drain", "uncharge", "unbless", "uncurse", "disenchant",
    "degrade", "dilute", "erase", "burn", "neutralize", "destroy", "splatter",
    "bite", "open", "break the lock on", "rust", "rot", "tarnish"
};

/* possibly bill for an object which the player has just modified */
void
costly_alteration(obj, alter_type)
struct obj *obj;
int alter_type;
{
    xchar ox, oy;
    char objroom;
    boolean set_bknown;
    const char *those, *them;
    struct monst *shkp = 0;

    if (alter_type < 0 || alter_type >= SIZE(alteration_verbs))
    {
        impossible("invalid alteration type (%d)", alter_type);
        alter_type = 0;
    }

    ox = oy = 0;    /* lint suppression */
    objroom = '\0'; /* ditto */
    if (carried(obj) || obj->where == OBJ_FREE)
    {
        /* OBJ_FREE catches obj_no_longer_held()'s transformation
           of crysknife back into worm tooth; the object has been
           removed from inventory but not necessarily placed at
           its new location yet--the unpaid flag will still be set
           if this item is owned by a shop */
        if (!obj->unpaid)
            return;
    }
    else 
    {
        /* this get_obj_location shouldn't fail, but if it does,
           use hero's location */
        if (!get_obj_location(obj, &ox, &oy, CONTAINED_TOO))
            ox = u.ux, oy = u.uy;
        if (!costly_spot(ox, oy))
            return;
        objroom = *in_rooms(ox, oy, SHOPBASE);
        /* if no shop cares about it, we're done */
        if (!billable(&shkp, obj, objroom, FALSE))
            return;
    }

    if (obj->quan == 1L)
        those = "that", them = "it";
    else
        those = "those", them = "them";

    /* when shopkeeper describes the object as being uncursed or unblessed
       hero will know that it is now uncursed; will also make the feedback
       from `I x' after bill_dummy_object() be more specific for this item */
    set_bknown = (alter_type == COST_UNCURS || alter_type == COST_UNBLSS);

    switch (obj->where) 
    {
    case OBJ_FREE: /* obj_no_longer_held() */
    case OBJ_INVENT:
        if (set_bknown)
            obj->bknown = 1;
        boolean didtalk = FALSE;
        if (iflags.using_gui_sounds)
        {
            char* o_shop = in_rooms(u.ux, u.uy, SHOPBASE);
            shkp = shop_keeper(*o_shop);
            if (obj->unpaid && shkp && inhishop(shkp) && is_obj_on_shk_bill(obj, shkp) && costly_spot(u.ux, u.uy))
            {
                //play_voice_shopkeeper_simple_line(shkp, obj->quan == 1L ? SHOPKEEPER_LINE_YOU_ALTER_THAT_YOU_PAY_FOR_IT : SHOPKEEPER_LINE_YOU_ALTER_THOSE_YOU_PAY_FOR_THEM);
                play_voice_shopkeeper_costly_alteration(shkp, obj, alter_type);
                verbalize("You %s %s, you pay for %s!", alteration_verbs[alter_type], those, them);
                didtalk = TRUE;
            }
        }

        if(!didtalk)
            verbalize("You %s %s %s, you pay for %s!",
                      alteration_verbs[alter_type], those, simpleonames(obj),
                      them);

        obj->speflags |= SPEFLAGS_ADDED_TO_YOUR_BILL;
        bill_dummy_object(obj);
        break;
    case OBJ_FLOOR:
        if (set_bknown)
            obj->bknown = 1;
        if (costly_spot(u.ux, u.uy) && objroom == *u.ushops) 
        {
            objroom = *in_rooms(ox, oy, SHOPBASE);
            if (billable(&shkp, obj, objroom, FALSE) && shkp)
            {
                //play_voice_shopkeeper_simple_line(shkp, obj->quan == 1L ? SHOPKEEPER_LINE_YOU_ALTER_THAT_YOU_PAY_FOR_IT : SHOPKEEPER_LINE_YOU_ALTER_THOSE_YOU_PAY_FOR_THEM);
                play_voice_shopkeeper_costly_alteration(shkp, obj, alter_type);
                verbalize("You %s %s, you pay for %s!",
                    alteration_verbs[alter_type], those, them);
                obj->speflags |= SPEFLAGS_ADDED_TO_YOUR_BILL;
            }
            bill_dummy_object(obj);
        } 
        else 
        {
            (void) stolen_value(obj, ox, oy, FALSE, FALSE);
        }
        break;
    }
}

/* These are the classes where dknown is zero by default! */
static const char dknowns[] = { WAND_CLASS,   RING_CLASS, POTION_CLASS, ARMOR_CLASS, MISCELLANEOUS_CLASS, REAGENT_CLASS,
                                SCROLL_CLASS, GEM_CLASS,  SPBOOK_CLASS,
                                WEAPON_CLASS, TOOL_CLASS, FOOD_CLASS, 0 };

/* mksobj(): create a specific type of object */
/* mkobj_type = 0 making contents on the floor, = 1 making box contents, = 2 wishing, = 3 forging */
struct obj*
mksobj(otyp, init, artif, mkobj_type)
int otyp;
boolean init;
boolean artif;
int mkobj_type;
{
    return mksobj_with_flags(otyp, init, artif, mkobj_type, 0, 0UL);
}

struct obj *
mksobj_with_flags(otyp, init, artif, mkobj_type, param, mkflags)
int otyp;
boolean init;
boolean artif;
int mkobj_type; /* 0 = floor, 1 = box, 2 = wishing, -1 = special level preset item */
int param;
unsigned long mkflags;
{
    int mndx, tryct;
    struct obj *otmp;
    char let = objects[otyp].oc_class;

    otmp = newobj();
    *otmp = zeroobj;
    otmp->age = monstermoves;
    otmp->o_id = context.ident++;
    if (!otmp->o_id)
        otmp->o_id = context.ident++; /* ident overflowed */
    otmp->quan = 1L;
    otmp->oclass = let;
    otmp->otyp = otyp;
    otmp->where = OBJ_FREE;
    otmp->glyph = NO_GLYPH;
    otmp->gui_glyph = NO_GLYPH;
    otmp->dknown = index(dknowns, let) ? 0 : 1;
    if ((otmp->otyp >= ELVEN_SHIELD && otmp->otyp <= ORCISH_SHIELD)
        || otmp->otyp == SHIELD_OF_REFLECTION
        || objects[otmp->otyp].oc_merge)
        otmp->dknown = 0;
    if (!objects[otmp->otyp].oc_uses_known)
        otmp->known = 1;
    otmp->lknown = 0;
    otmp->tknown = 0;
    otmp->cknown = (objects[otmp->otyp].oc_flags4 & O4_CONTAINER_CONTENTS_VISIBLE) ? 1 : 0;
    otmp->corpsenm = NON_PM;
    otmp->elemental_enchantment = 0;
    otmp->exceptionality = 0;
    otmp->mythic_prefix = 0;
    otmp->mythic_suffix = 0;
    otmp->cooldownleft = 0;
    otmp->repowerleft = 0;
    otmp->blessed = 0;
    otmp->lamplit = 0;
    otmp->makingsound = 0;
    otmp->special_quality = 0;
    otmp->has_special_tileset = 0;
    otmp->special_tileset = 0;
    otmp->cobj = (struct obj*)0;

    if ((objects[otmp->otyp].oc_flags4 & O4_CONTAINER_HAS_LID) && (mkflags & MKOBJ_FLAGS_OPEN_COFFIN))
        otmp->speflags |= SPEFLAGS_LID_OPENED;

    /* Change type before init if need be*/
    if (mkobj_type == 0 && (In_mines(&u.uz) || level_difficulty() < 10))
    {
        if (otyp == FROST_HORN || otyp == FIRE_HORN)
            otmp->otyp = !rn2(3) ? HORN_OF_CHAOS : !rn2(2) ? BRASS_HORN : TOOLED_HORN;
        else if (otmp->otyp == WAN_COLD || otmp->otyp == WAN_FIRE || otmp->otyp == WAN_LIGHTNING)
            otmp->otyp = !rn2(3) ? WAN_STRIKING : !rn2(2) ? WAN_DIGGING : WAN_SPEED_MONSTER;
    }
    
    if (mkobj_type == 0 && !Inhell) /* No instadeath wands on floor ever, except in Gehennom */
    {
        if (otmp->otyp == WAN_DEATH || otmp->otyp == WAN_DISINTEGRATION || otmp->otyp == WAN_PETRIFICATION)
            otmp->otyp = !rn2(2) ? WAN_LIGHTNING : WAN_FIRE;
    }

    if (mkobj_type >= 0 && mkobj_type < 2 && (depth(&u.uz) == 1 || depth(&u.uz) == 2 || level_difficulty() < 5))
    {
        if (otmp->otyp == WAN_WISHING)
            otmp->otyp = WAN_POLYMORPH;
    }

    /* Initialize */
    if (init) 
    {
        /* quantity */
        if(objects[otmp->otyp].oc_merge && !is_obj_unique(otmp))
            otmp->quan = get_multigen_quan(objects[otmp->otyp].oc_multigen_type);

        /* charges if any */
        if (objects[otmp->otyp].oc_charged)
            otmp->charges = get_obj_init_charge(otmp);

        /* alternative appearance */
        if ((objects[otmp->otyp].oc_flags5 & O5_HAS_ALTERNATIVE_APPEARANCE) != 0)
        {
            if (rn2(2))
                otmp->speflags |= SPEFLAGS_ALTERNATIVE_APPEARANCE;
        }

        switch (let) {
        case WEAPON_CLASS:
            otmp->quan = get_multigen_quan(objects[otmp->otyp].oc_multigen_type);// is_multigen(otmp) ? (long) rn1(6, 6) : 1L;
            if (!rn2(11) && !is_cursed_magic_item(otmp))
            {
                otmp->enchantment = rne(3);
                otmp->blessed = rn2(2);
            } 
            else if (!rn2(10) || is_cursed_magic_item(otmp)) 
            {
                curse(otmp);
                otmp->enchantment = -rne(3);
            } 
            else
                blessorcurse(otmp, 10);

            if (mkobj_type < 2 && is_poisonable(otmp) && !rn2(100) && !(objects[otmp->otyp].oc_flags2 & O2_GENERATED_DEATH_OR_COLD_ENCHANTED))
                otmp->opoisoned = 1;
            else if (is_elemental_enchantable(otmp) && ((objects[otmp->otyp].oc_flags2 & O2_GENERATED_DEATH_OR_COLD_ENCHANTED) || (mkobj_type < 2 && (is_multigen(otmp) ? !rn2(40) : !rn2(160)))))
            {
                if (is_death_enchantable(otmp) && ((objects[otmp->otyp].oc_flags2 & O2_GENERATED_DEATH_OR_COLD_ENCHANTED) || (mkobj_type < 2 && !rn2(10))))
                {
                    otmp->elemental_enchantment = DEATH_ENCHANTMENT;
                    if (is_multigen(otmp))
                    {
                        otmp->quan = rnd(2);
                    }
                }
                else
                {
                    otmp->elemental_enchantment = (objects[otmp->otyp].oc_flags2 & O2_GENERATED_DEATH_OR_COLD_ENCHANTED) ? COLD_ENCHANTMENT :
                        level_difficulty() > 11 ? (!rn2(7) ? COLD_ENCHANTMENT : !rn2(3) ? LIGHTNING_ENCHANTMENT  : FIRE_ENCHANTMENT) :
                        (!rn2(3) ? LIGHTNING_ENCHANTMENT : FIRE_ENCHANTMENT);

                    if (is_multigen(otmp))
                    {
                        otmp->quan = (otmp->quan + 1) / 2;
                    }
                }
            }

            if (artif && !rn2(20))
                otmp = mk_artifact(otmp, (aligntyp) A_NONE, MKARTIFACT_FLAGS_NONE);

            break;
        case FOOD_CLASS:
            otmp->oeaten = 0;
            switch (otmp->otyp) 
            {
            case CORPSE:
                /* possibly overridden by mkcorpstat() */
                tryct = 50;
                do
                    otmp->corpsenm = undead_to_corpse(rndmonnum());
                while ((mvitals[otmp->corpsenm].mvflags & MV_NOCORPSE)
                       && (--tryct > 0));
                if (tryct == 0)
                {
                    /* perhaps rndmonnum() only wants to make G_NOCORPSE
                       monsters on this level; create an adventurer's
                       corpse instead, then */
                    otmp->corpsenm = PM_HUMAN;
                }
                /* timer set below */
                break;
            case EGG:
                otmp->corpsenm = NON_PM; /* generic egg */
                if (!rn2(3))
                    for (tryct = 200; tryct > 0; --tryct)
                    {
                        mndx = can_be_hatched(rndmonnum());
                        if (mndx != NON_PM && !dead_species(mndx, TRUE))
                        {
                            otmp->corpsenm = mndx; /* typed egg */
                            break;
                        }
                    }
                /* timer set below */
                break;
            case TIN:
                otmp->corpsenm = NON_PM; /* empty (so far) */
                if (!rn2(6))
                    set_tin_variety(otmp, SPINACH_TIN);
                else
                    for (tryct = 200; tryct > 0; --tryct) 
                    {
                        mndx = undead_to_corpse(rndmonnum());
                        if (mons[mndx].cnutrit
                            && !(mvitals[mndx].mvflags & MV_NOCORPSE))
                        {
                            otmp->corpsenm = mndx;
                            set_tin_variety(otmp, RANDOM_TIN);
                            break;
                        }
                    }
                blessorcurse(otmp, 10);
                break;
            case SLIME_MOLD:
                otmp->special_quality = context.current_fruit;
                flags.made_fruit = TRUE;
                break;
            case KELP_FROND:
                otmp->quan = (long) rnd(2);
                break;
            }
            if (Is_pudding(otmp)) 
            {
                otmp->globby = 1;
                otmp->known = otmp->dknown = 1;
                otmp->corpsenm = PM_GRAY_OOZE
                                 + (otmp->otyp - GLOB_OF_GRAY_OOZE);
            } 
            else 
            {
                if (otmp->otyp != CORPSE && otmp->otyp != MEAT_RING
                    && otmp->otyp != KELP_FROND && !rn2(6)) 
                {
                    otmp->quan = 2L;
                }
            }
            break;
        case GEM_CLASS:
            otmp->corpsenm = 0; /* LOADSTONE hack */
            if (is_rock(otmp))
                otmp->quan = (long) rn1(6, 6);
            else if (otmp->otyp == FLINT)
                otmp->quan = (long)rnd(30);
            else if (is_ore(otmp) && Inhell)
                otmp->quan = (long)rnd(6);
            else if (otmp->otyp != LUCKSTONE && !rn2(6))
                otmp->quan = 2L;
            else
                otmp->quan = 1L;
            break;
        case TOOL_CLASS:
            /* Primary initialization */
            switch (otmp->otyp)
            {
            case TALLOW_CANDLE:
            case WAX_CANDLE:
                otmp->special_quality = 1;
                otmp->age = 30L * /* 600 or 300 */
                            objects[otmp->otyp].oc_cost;
                otmp->lamplit = 0;
                otmp->quan = 1L + (long) (rn2(2) ? rn2(7) : 0);
                blessorcurse(otmp, 5);
                break;
            case LARGE_FIVE_BRANCHED_CANDELABRUM:
                otmp->special_quality = !rn2(3) ? 0 : !rn2(2) ? objects[otmp->otyp].oc_special_quality : rnd(objects[otmp->otyp].oc_special_quality);
                otmp->lamplit = 0;
                if (otmp->special_quality > 0)
                {
                    otmp->age = CANDELABRUM_STARTING_AGE;
                }
                else
                {
                    otmp->age = 0;
                }
                blessorcurse(otmp, 5);
                break;
            case BRASS_LANTERN:
            case OIL_LAMP:
                otmp->special_quality = 1;
                otmp->age = (long) rn1(500, 1000);
                otmp->lamplit = 0;
                blessorcurse(otmp, 5);
                break;
            case MAGIC_CANDLE:
                otmp->special_quality = 2;
                otmp->lamplit = 0;
                blessorcurse(otmp, 2);
                break;
            case MAGIC_LAMP:
                otmp->special_quality = 1;
                otmp->lamplit = 0;
                blessorcurse(otmp, 2);
                break;
            case CHEST:
            case LARGE_BOX:
                otmp->olocked = !!(rn2(5));
                otmp->otrapped = !(rn2(10));
                break;
            case SARCOPHAGUS:
            case COFFIN:
            {

                if (!(mkflags & MKOBJ_FLAGS_OPEN_COFFIN) && !(mkflags & MKOBJ_FLAGS_MONSTER_SPECIFIED))
                {
                    int cnm = NON_PM;
                    struct permonst* ptr = 0;

                    if (otmp->otyp == SARCOPHAGUS)
                    {
                        if (!rn2(2))
                            cnm = NON_PM;
                        else
                        {
                            boolean low_level = ((level_difficulty() + u.ulevel) / 2 < mons[PM_LICH].difficulty);
                            int classmonster = NON_PM;
                            ptr = mkclass(rn2(3) || low_level ? S_GREATER_UNDEAD : S_LICH, 0);
                            if (ptr)
                                classmonster = monsndx(ptr);

                            int tmp = ((low_level || rn2(3)) && classmonster > NON_PM ? classmonster :
                                ((level_difficulty() + u.ulevel) / 2 < mons[PM_SKELETON_WARRIOR].difficulty - 2 ? PM_SKELETON :
                                    !rn2(2) || (level_difficulty() + u.ulevel) / 2 < mons[PM_SKELETON_LORD].difficulty - 3 ? PM_SKELETON_WARRIOR :
                                    !rn2(2) || (level_difficulty() + u.ulevel) / 2 < mons[PM_SKELETON_KING].difficulty - 4 ? PM_SKELETON_LORD : PM_SKELETON_KING));

                            if (tmp >= LOW_PM && !(mvitals[tmp].mvflags & MV_GONE))
                                cnm = tmp;
                        }

                        set_corpsenm(otmp, cnm);
                    }
                    else
                    {
                        /* Coffin */
                        int classmonster = NON_PM;
                        if (In_V_tower(&u.uz))
                        {
                            ptr = mkclass(S_VAMPIRE, 0);
                            if(ptr)
                                classmonster = monsndx(ptr);
                            if (classmonster >= LOW_PM)
                                cnm = classmonster;
                            else if (!(mvitals[PM_VAMPIRE].mvflags & MV_GONE))
                                cnm = PM_VAMPIRE;
                        }
                        else
                        {
                            if (!rn2(3))
                                cnm = NON_PM;
                            else
                            {
                                boolean low_level = ((level_difficulty() + u.ulevel) / 2 < mons[PM_VAMPIRE].difficulty);
                                boolean very_low_level = ((level_difficulty() + u.ulevel) / 2 < mons[PM_BARROW_WIGHT].difficulty - 1);
                                ptr = mkclass(rn2(3) && !low_level ? S_VAMPIRE : S_LESSER_UNDEAD, 0);
                                if (ptr)
                                    classmonster = monsndx(ptr);
                                int tmp = (rn2(9) && classmonster > NON_PM ? classmonster : very_low_level ? PM_HUMAN_ZOMBIE : low_level ? PM_BARROW_WIGHT : PM_SKELETON_WARRIOR);
                                if (tmp >= LOW_PM && !(mvitals[tmp].mvflags & MV_GONE))
                                    cnm = tmp;
                            }
                        }
                        set_corpsenm(otmp, cnm);
                    }

                    if (otmp->corpsenm == NON_PM && !rn2(2))
                    {
                        if (!rn2(2))
                        {
                            struct obj* otmp2 = mksobj(BONE, TRUE, TRUE, 1);
                            if (otmp2)
                            {
                                otmp2->quan = rnd(4);
                                otmp2->owt = weight(otmp2);
                                (void)add_to_container(otmp, otmp2);
                            }
                            otmp2 = mksobj(HUMAN_SKULL, TRUE, TRUE, 1);
                            if (otmp2)
                                (void)add_to_container(otmp, otmp2);
                        }
                        else
                        {
                            struct obj* corpse = mksobj(CORPSE, TRUE, TRUE, 1);
                            if (corpse)
                            {
                                if(!rn2(5))
                                {
                                    struct obj* corpse2 = tt_oname(corpse);
                                    if(corpse2)
                                        corpse = corpse2;
                                }
                                else
                                    set_corpsenm(corpse, !rn2(4) ? PM_HUMAN : rn2(PM_WIZARD - PM_ARCHAEOLOGIST + 1) + PM_ARCHAEOLOGIST);

                                (void)add_to_container(otmp, corpse);
                            }
                        }
                    }

                    if (otmp->otyp == SARCOPHAGUS && otmp->corpsenm == NON_PM && !rn2(3))
                    {
                        struct obj* otmp2 = mksobj(MUMMY_WRAPPING, TRUE, TRUE, 1);
                        if (otmp2)
                            (void)add_to_container(otmp, otmp2);
                    }

                    otmp->owt = weight(otmp);
                }
                break;
            }
            case EXPENSIVE_CAMERA:
            case TINNING_KIT:
            case MAGIC_MARKER:
            case HORN_OF_PLENTY:
            case BAG_OF_TRICKS:
            case BELL_OF_OPENING:
            case MAGIC_FLUTE:
            case MAGIC_HARP:
            case FROST_HORN:
            case FIRE_HORN:
            case HORN_OF_CHAOS:
            case UNICORN_HORN:
            case DRUM_OF_EARTHQUAKE:
                break;
            case CAN_OF_GREASE:
                otmp->charges = get_obj_init_charge(otmp);
                blessorcurse(otmp, 10);
                break;
            case CRYSTAL_BALL:
                otmp->charges = get_obj_init_charge(otmp);
                blessorcurse(otmp, 2);
                break;
            case FIGURINE:
                tryct = 0;
                do
                    otmp->corpsenm = rndmonnum();
                while (is_human(&mons[otmp->corpsenm]) && tryct++ < 30);
                blessorcurse(otmp, 4);
                break;
            }

            /* Make container contents */
            if (Is_proper_container(otmp))
                mkbox_cnts(otmp);

            break;
        case AMULET_CLASS:
            if (otmp->otyp == AMULET_OF_YENDOR)
                context.made_amulet = TRUE;
            if (rn2(10) && (is_cursed_magic_item(otmp))) 
            {
                curse(otmp);
            }
            else
                blessorcurse(otmp, 10);

            break;
        case REAGENT_CLASS:
        case VENOM_CLASS:
        case CHAIN_CLASS:
        case BALL_CLASS:
            break;
        case POTION_CLASS: /* note: potions get some additional init below */
        case SCROLL_CLASS:
#ifdef MAIL
            if (otmp->otyp != SCR_MAIL)
#endif
                blessorcurse(otmp, 4);
            break;
        case SPBOOK_CLASS:
            otmp->spestudied = 0;
            blessorcurse(otmp, 17);
            break;
        case ARMOR_CLASS:
            if (rn2(10)
                && (is_cursed_magic_item(otmp) || !rn2(11))) 
            {
                curse(otmp);
                otmp->enchantment = -rne(3);
            } 
            else if (!rn2(10)) 
            {
                otmp->blessed = (objects[otmp->otyp].oc_flags2 & O2_GENERATED_BLESSED) ? 1 : rn2(2);
                otmp->enchantment = rne(3);
            } 
            else
                blessorcurse(otmp, 10);

            if (artif && !rn2(40))
                otmp = mk_artifact(otmp, (aligntyp) A_NONE, MKARTIFACT_FLAGS_NONE);

            /* simulate lacquered armor for samurai */
            if (Role_if(PM_SAMURAI) && otmp->otyp == SPLINT_MAIL
                && (moves <= 1 || In_quest(&u.uz))) {
#ifdef UNIXPC
                /* optimizer bitfield bug */
                otmp->oerodeproof = 1;
                otmp->rknown = 1;
#else
                otmp->oerodeproof = otmp->rknown = 1;
#endif
            }
            break;
        case WAND_CLASS:
            otmp->charges = get_obj_init_charge(otmp);
            blessorcurse(otmp, 17);
            otmp->recharged = 0; /* used to control recharging */
            break;
        case MISCELLANEOUS_CLASS:
        case RING_CLASS:
            if (objects[otmp->otyp].oc_enchantable == ENCHTYPE_RING_NORMAL || objects[otmp->otyp].oc_enchantable == ENCHTYPE_RING_1_7
                || objects[otmp->otyp].oc_enchantable == ENCHTYPE_RING_DOUBLE || objects[otmp->otyp].oc_enchantable == ENCHTYPE_RING_POWER
                || objects[otmp->otyp].oc_enchantable == ENCHTYPE_MISCELLANEOUS_NORMAL
                )
            {
                int addition = 0;
                if (objects[otmp->otyp].oc_enchantable == ENCHTYPE_RING_NORMAL || objects[otmp->otyp].oc_enchantable == ENCHTYPE_MISCELLANEOUS_NORMAL)
                    addition += rnd(2) + (!rn2(2) ? 0 : !rn2(2) ? 1 : !rn2(2) ? 2 : 3);
                else if (objects[otmp->otyp].oc_enchantable == ENCHTYPE_RING_1_7)
                    addition += rnd(3) + (!rn2(3) ? 0 : !rn2(3) ? 1 : !rn2(3) ? 2 : !rn2(3) ? 3 : 4);
                else if (objects[otmp->otyp].oc_enchantable == ENCHTYPE_RING_DOUBLE)
                    addition += rnd(4) + (!rn2(4) ? 0 : !rn2(4) ? 1 : !rn2(4) ? 2 : !rn2(4) ? 3 : !rn2(4) ? 4 : !rn2(4) ? 5 : 6);
                else
                    addition += rnd(2);

                if ((is_cursed_magic_item(otmp) || !rn2(11)))
                    curse(otmp);
                else
                    blessorcurse(otmp, 3);

                if (rn2(10)) 
                {
                    if (rn2(10) && bcsign(otmp))
                        otmp->enchantment = bcsign(otmp) * addition; // rne(3);
                    else
                        otmp->enchantment = rn2(2) ? addition : -addition; //rne(3) : -rne(3);
                }
                /* make useless +0 rings much less common */
                if (otmp->enchantment == 0)
                    otmp->enchantment = ((objects[otmp->otyp].oc_enchantable == ENCHTYPE_RING_POWER) ? rnd(3) - 2 : rn2(4) - rn2(3));
                /* negative rings are usually cursed */
                if (otmp->enchantment < 0 && rn2(5))
                    curse(otmp);
            }
            else if (objects[otmp->otyp].oc_enchantable)
            {
                int addition = get_obj_init_enchantment(otmp);
                blessorcurse(otmp, 3);
                otmp->enchantment = bcsign(otmp) * addition;
                /* negative rings are usually cursed */
                if (otmp->enchantment < 0 && rn2(5))
                    curse(otmp);
            }
            else if (rn2(10) && (is_cursed_magic_item(otmp) || !rn2(9))) {
                curse(otmp);
            }
            break;
        case ROCK_CLASS:
            switch (otmp->otyp) {
            case STATUE:
                /* possibly overridden by mkcorpstat() */
                otmp->corpsenm = rndmonnum();

                if(!rn2(2))
                    otmp->speflags |= SPEFLAGS_FACING_RIGHT;
                
                if (randomize_monster_gender(&mons[otmp->corpsenm]))
                {
                    otmp->speflags |= SPEFLAGS_FEMALE;
                }
                else if (!is_neuter(&mons[otmp->corpsenm]))
                {
                    otmp->speflags |= SPEFLAGS_MALE;
                }

                if (!verysmall(&mons[otmp->corpsenm])
                    && rn2(level_difficulty() / 2 + 10) > 10)
                    (void) add_to_container(otmp, mkobj(SPBOOK_CLASS, FALSE, TRUE));
            }
            break;
        case COIN_CLASS:
            break; /* do nothing */
        default:
            impossible("impossible mkobj %d, sym '%c'.", otmp->otyp,
                       objects[otmp->otyp].oc_class);
            return (struct obj *) 0;
        }
    }

    /* Default to current tile set, and override later, if necessary */
    if ((objects[otmp->otyp].oc_flags5 & O5_TILE_IS_TILESET_DEPENDENT) != 0)
    {
        otmp->has_special_tileset = 1;
        otmp->special_tileset = get_current_cmap_type_index();
    }

    /* Blessed or cursed */
    if (is_obj_generated_blessed(otmp))
    {
        otmp->cursed = 0;
        otmp->enchantment = abs(otmp->enchantment);
        otmp->blessed = 1;
    }
    else if (is_obj_generated_cursed(otmp))
        curse(otmp);

    /* Exceptionality */
    if (can_have_exceptionality(otmp) && !(objects[otmp->otyp].oc_flags4 & O4_NEVER_GENERATED_WITH_EXCEPTIONALITY) && mkobj_type < 2 && otmp->oartifact == 0)
    {
        if (objects[otmp->otyp].oc_flags4 & O4_GENERATED_CELESTIAL)
        {
            otmp->exceptionality = EXCEPTIONALITY_CELESTIAL;
        }
        else if (objects[otmp->otyp].oc_flags4 & O4_GENERATED_PRIMORDIAL)
        {
            otmp->exceptionality = EXCEPTIONALITY_PRIMORDIAL;
        }
        else if (objects[otmp->otyp].oc_flags4 & O4_GENERATED_INFERNAL)
        {
            otmp->exceptionality = EXCEPTIONALITY_INFERNAL;
        }
        else if (objects[otmp->otyp].oc_flags4 & O4_GENERATED_ELITE)
        {
            otmp->exceptionality = EXCEPTIONALITY_ELITE;
        }
        else if (objects[otmp->otyp].oc_flags4 & O4_GENERATED_EXCEPTIONAL)
        {
            otmp->exceptionality = EXCEPTIONALITY_EXCEPTIONAL;
        }
        else
        {
            if (In_endgame(&u.uz))
            {
                if (!rn2(4))
                    otmp->exceptionality = (!rn2(3) && objects[otmp->otyp].oc_material != MAT_SILVER ? EXCEPTIONALITY_INFERNAL : !rn2(2) ? EXCEPTIONALITY_PRIMORDIAL : EXCEPTIONALITY_CELESTIAL);
                else if (!rn2(3))
                    otmp->exceptionality = EXCEPTIONALITY_ELITE;
                else if (!rn2(2))
                    otmp->exceptionality = EXCEPTIONALITY_EXCEPTIONAL;
            }
            else if (Inhell)
            {
                if (!rn2(10) && objects[otmp->otyp].oc_material != MAT_SILVER)
                    otmp->exceptionality = EXCEPTIONALITY_INFERNAL;
                else if (!rn2(4))
                    otmp->exceptionality = EXCEPTIONALITY_ELITE;
                else if (!rn2(2))
                    otmp->exceptionality = EXCEPTIONALITY_EXCEPTIONAL;
            }
            else if (depth(&u.uz) >= 20)
            {
                if (!rn2(Is_stronghold(&u.uz) ? 20 : 40))
                    otmp->exceptionality = (!rn2(3) && objects[otmp->otyp].oc_material != MAT_SILVER ? EXCEPTIONALITY_INFERNAL : !rn2(2) ? EXCEPTIONALITY_PRIMORDIAL : EXCEPTIONALITY_CELESTIAL);
                else if (!rn2(6))
                    otmp->exceptionality = EXCEPTIONALITY_ELITE;
                else if (!rn2(3))
                    otmp->exceptionality = EXCEPTIONALITY_EXCEPTIONAL;
            }
            else if (depth(&u.uz) >= 10)
            {
                if (!rn2(20))
                    otmp->exceptionality = EXCEPTIONALITY_ELITE;
                else if (!rn2(4))
                    otmp->exceptionality = EXCEPTIONALITY_EXCEPTIONAL;
            }
            else
            {
                if (!rn2(100))
                    otmp->exceptionality = EXCEPTIONALITY_ELITE;
                else if (!rn2(20))
                    otmp->exceptionality = EXCEPTIONALITY_EXCEPTIONAL;
            }
        }
    }

    /* Mythic quality */
    if (can_obj_have_mythic(otmp) && level_difficulty() >= 3 && mkobj_type < 2 && otmp->oartifact == 0)
    {
        boolean doublechance = !!(objects[otmp->otyp].oc_flags4 & O4_DOUBLE_MYTHIC_CHANCE);
        boolean makemythic = FALSE;
        if (In_endgame(&u.uz))
            makemythic = otmp->exceptionality || otmp->oclass == ARMOR_CLASS ? !rn2(doublechance ? 2 : 4) : !rn2(doublechance ? 4 : 8);
        else if (Inhell)
            makemythic = otmp->exceptionality || otmp->oclass == ARMOR_CLASS ? !rn2(doublechance ? 3 : 5) : !rn2(doublechance ? 5 : 10);
        else if (level_difficulty() >= 20)
            makemythic = otmp->exceptionality || otmp->oclass == ARMOR_CLASS ? !rn2(doublechance ? 3 : 6) : !rn2(doublechance ? 6 : 12);
        else if (level_difficulty() >= 10)
            makemythic = otmp->exceptionality ? !rn2(doublechance ? 4 : 8) : !rn2(doublechance ? 8 : 16);
        else
            makemythic = otmp->exceptionality ? !rn2(doublechance ? 6 : 12) : !rn2(doublechance ? 12 : 24);

        if (makemythic)
        {
            randomize_mythic_quality(otmp, FALSE, &otmp->mythic_prefix, &otmp->mythic_suffix);
        }
    }

    if (has_obj_mythic_uncurseable(otmp))
        otmp->cursed = 0;

    /* some things must get done (corpsenm, timers) even if init = 0 */
    switch ((otmp->oclass == POTION_CLASS && otmp->otyp != POT_OIL)
            ? POT_WATER
            : otmp->otyp) {
    case CORPSE:
        if (otmp->corpsenm == NON_PM) {
            otmp->corpsenm = undead_to_corpse(rndmonnum());
            if (mvitals[otmp->corpsenm].mvflags & (MV_NOCORPSE | MV_GONE))
                otmp->corpsenm = urole.monsternum;
        }
        /*FALLTHRU*/
    case STATUE:
    case FIGURINE:
        if (otmp->corpsenm == NON_PM)
            otmp->corpsenm = rndmonnum();
        /*FALLTHRU*/
    case EGG:
    /* case TIN: */
        set_corpsenm(otmp, otmp->corpsenm);
        break;
    case POT_OIL:
        otmp->age = MAX_OIL_IN_FLASK; /* amount of oil */
        /*FALLTHRU*/
    case POT_WATER: /* POTION_CLASS */
        otmp->speflags &= ~(SPEFLAGS_FROM_SINK); /* overloads corpsenm, which was set to NON_PM */
        break;
    case LEASH:
        otmp->leashmon = 0; /* overloads corpsenm, which was set to NON_PM */
        break;
    case SPE_NOVEL:
        if(mkflags & MKOBJ_FLAGS_PARAM_IS_TITLE)
            otmp->novelidx = (short)param;
        else
            otmp->novelidx = -1; /* "none of the above"; will be changed */
        otmp = oname(otmp, noveltitle(&otmp->novelidx));
        otmp->nknown = TRUE;
        break;
    case SPE_MANUAL:
        if (mkflags & MKOBJ_FLAGS_PARAM_IS_TITLE)
            otmp->manualidx = (short)param;
        else
            otmp->manualidx = -1; /* "none of the above"; will be changed */
        otmp = oname(otmp, manualtitle(&otmp->manualidx));
        otmp->nknown = TRUE;
        otmp->cursed = otmp->blessed = 0; /* Never blessed or cursed */
        break;
    }

    /* unique objects may have an associated artifact entry */
    if (is_otyp_unique(otyp) && !otmp->oartifact)
        otmp = mk_artifact(otmp, (aligntyp) A_NONE, MKARTIFACT_FLAGS_NONE);
    otmp->owt = weight(otmp);

    return otmp;
}


int
get_obj_init_charge(otmp)
struct obj* otmp;
{
    if (!otmp)
        return 0;

    int init_charge = get_init_charge(objects[otmp->otyp].oc_charged);

    /* Possible extra modifications here */

    return init_charge;
}

int
get_obj_max_charge(otmp)
struct obj* otmp;
{
    if (!otmp)
        return 0;

    int init_charge = get_max_charge(objects[otmp->otyp].oc_charged);

    /* Possible extra modifications here */

    return init_charge;
}

int
get_init_charge(charge_init_index)
int charge_init_index;
{
    int charge = 1;

    switch (charge_init_index)
    {
    case CHARGED_NOT_CHARGED:
        charge = 0;
        break;
    case CHARGED_GENERAL:
        charge = rne(3);
        break;
    case CHARGED_ALWAYS_1:
        charge = 1;
        break;
    case CHARGED_ALWAYS_2:
        charge = 2;
        break;
    case CHARGED_ALWAYS_3:
        charge = 3;
        break;
    case CHARGED_ALWAYS_4:
        charge = 4;
        break;
    case CHARGED_ALWAYS_5:
        charge = 5;
        break;
    case CHARGED_ALWAYS_6:
        charge = 6;
        break;
    case CHARGED_ALWAYS_7:
        charge = 7;
        break;
    case CHARGED_ALWAYS_8:
        charge = 8;
        break;
    case CHARGED_ALWAYS_9:
        charge = 9;
        break;
    case CHARGED_1D8_1:
        charge = rnd(8) + 1;
        break;
    case CHARGED_WAND_NORMAL_NODIR:
        charge = rn1(5, 11);
        break;
    case CHARGED_WAND_NORMAL_DIR:
        charge = rn1(5, 4);
        break;
    case CHARGED_WAND_WISHING:
        charge = rnd(3);
        break;
    case CHARGED_HORN_NORMAL:
        charge = rn1(5, 4);
        break;
    case CHARGED_UNICORN_HORN:
        charge = 6 + rnd(3);
        break;
    case CHARGED_BAG_OF_TRICKS:
        charge = rnd(20);
        break;
    case CHARGED_CRYSTAL_BALL:
        charge = 2 + rnd(5);
        break;
    case CHARGED_CAN_OF_GREASE:
        charge = rnd(25);
        break;
    case CHARGED_MAGIC_MARKER:
        charge = rn1(70, 30);
        break;
    case CHARGED_LUCK_BLADE:
        charge = 2;
        break;
    case CHARGED_1D6_6:
        charge = rnd(6) + 6;
        break;
    case CHARGED_2D6_8:
        charge = d(2, 6) + 8;
        break;
    case CHARGED_1D15_15:
        charge = rnd(15) + 15;
        break;
    case CHARGED_1D30_30:
        charge = rnd(30) + 30;
        break;
    case CHARGED_1D45_45:
        charge = rnd(45) + 45;
        break;
    case CHARGED_1D75_75:
        charge = rnd(75) + 75;
        break;
    case CHARGED_1D6_3:
        charge = rnd(6) + 3;
        break;
    case CHARGED_1D4_4:
        charge = rnd(4) + 3;
        break;
    case CHARGED_1D20_20:
        charge = rnd(20) + 20;
        break;
    case CHARGED_HOLY_GRAIL:
        charge = 0;
        break;
    case CHARGED_1D4_1:
        charge = rnd(4) + 1;
        break;
    }

    return charge;
}


int
get_max_charge(charge_init_index)
int charge_init_index;
{
    int charge = 1;

    switch (charge_init_index)
    {
    case CHARGED_NOT_CHARGED:
        charge = 0;
        break;
    case CHARGED_GENERAL:
        charge = 5;
        break;
    case CHARGED_ALWAYS_1:
        charge = 1;
        break;
    case CHARGED_ALWAYS_2:
        charge = 2;
        break;
    case CHARGED_ALWAYS_3:
        charge = 3;
        break;
    case CHARGED_ALWAYS_4:
        charge = 4;
        break;
    case CHARGED_ALWAYS_5:
        charge = 5;
        break;
    case CHARGED_ALWAYS_6:
        charge = 6;
        break;
    case CHARGED_ALWAYS_7:
        charge = 7;
        break;
    case CHARGED_ALWAYS_8:
        charge = 8;
        break;
    case CHARGED_ALWAYS_9:
        charge = 9;
        break;
    case CHARGED_1D8_1:
        charge = 9;
        break;
    case CHARGED_WAND_NORMAL_NODIR:
        charge = 15;
        break;
    case CHARGED_WAND_NORMAL_DIR:
        charge = 8;
        break;
    case CHARGED_WAND_WISHING:
        charge = 3;
        break;
    case CHARGED_HORN_NORMAL:
        charge = 10;
        break;
    case CHARGED_UNICORN_HORN:
        charge = 9;
        break;
    case CHARGED_BAG_OF_TRICKS:
        charge = 20;
        break;
    case CHARGED_CRYSTAL_BALL:
        charge = 7;
        break;
    case CHARGED_CAN_OF_GREASE:
        charge = 25;
        break;
    case CHARGED_MAGIC_MARKER:
        charge = 100;
        break;
    case CHARGED_LUCK_BLADE:
        charge = 2;
        break;
    case CHARGED_1D6_6:
        charge = 12;
        break;
    case CHARGED_2D6_8:
        charge = 20;
        break;
    case CHARGED_1D15_15:
        charge = 30;
        break;
    case CHARGED_1D30_30:
        charge = 60;
        break;
    case CHARGED_1D45_45:
        charge = 90;
        break;
    case CHARGED_1D75_75:
        charge = 150;
        break;
    case CHARGED_1D6_3:
        charge = 9;
        break;
    case CHARGED_1D4_4:
        charge = 8;
        break;
    case CHARGED_1D20_20:
        charge = 40;
        break;
    case CHARGED_HOLY_GRAIL:
        charge = 2;
        break;
    case CHARGED_1D4_1:
        charge = 5;
        break;
    }

    return charge;
}

const char* recharge_texts[MAX_RECHARGING_TYPES] =
{
    "not rechargeable",
    "rechargeable normally",
    "rechargeable normally as wand",
    "rechargeable once as wand",
    "rechargeable as normal tool",
    "rechargeable as special tool",
    "rechargeable as nonmagical tool",
    "rechargeable as musical instrument",
    "rechargeable as can of grease",
    "rechargeable as unicorn horn",
    "rechargeable as holy symbol",
    "rechargeable as crystal ball",
    "rechargeable as special magical tool",
    "rechargeable as camera",
    "rechargeable as an artifact bell",
    "rechargeable up to five times",
    "not rechargeable",
    "not rechargeable",
    "rechargeable up to seven times",
    "rechargeable up to seven times",
    "rechargeable up to five times",
    "rechargeable to fill up the jar",
    "rechargeable as a magic cube",
};

const char *
get_recharge_text(recharge_index)
enum recharging_types recharge_index;
{
    return recharge_texts[recharge_index];
}


int
get_obj_init_enchantment(otmp)
struct obj* otmp;
{
    if (!otmp)
        return 0;

    int init_spe = get_init_enchantment(objects[otmp->otyp].oc_enchantable);

    /* Possible extra modifications here */

    return init_spe;
}

int
get_obj_max_enchantment(otmp)
struct obj* otmp;
{
    if (!otmp)
        return 0;

    int max_spe = get_max_enchantment(objects[otmp->otyp].oc_enchantable);

    /* Extra modifications */
    if (has_obj_mythic_added_enchantability(otmp))
        max_spe = (max_spe * 3) / 2;

    return max_spe;
}

int
get_init_enchantment(spe_type_index)
int spe_type_index;
{
    int initspe = 1;

    switch (spe_type_index)
    {
    case ENCHTYPE_NO_ENCHANTMENT:
        initspe = 0;
        break;
    case ENCHTYPE_GENERAL_WEAPON:
        initspe = Inhell ? rngh(2, 6) : rngh(3, 3);
        break;
    case ENCHTYPE_GENERAL_ARMOR:
        initspe = Inhell ? rngh(3, 4) : rngh(3, 3);
        break;
    case ENCHTYPE_GENERAL_WEAPON_ALWAYS_START_0:
    case ENCHTYPE_GENERAL_ARMOR_ALWAYS_START_0:
    case ENCHTYPE_PROJECTILE_ALWAYS_START_0:
        initspe = 0;
        break;
    case ENCHTYPE_ELVEN_ARMOR:
        initspe = Inhell ? rngh(2, 5) : rngh(3, 4);
        break;
    case ENCHTYPE_LEATHER_NONBODY_ARMOR:
        initspe = Inhell ? rngh(3, 4) : rngh(3, 3);
        break;
    case ENCHTYPE_LEATHER_BODY_ARMOR:
        initspe = Inhell ? rngh(2, 5) : rngh(3, 4);
        break;
    case ENCHTYPE_ALWAYS_1:
        initspe = 1;
        break;
    case ENCHTYPE_ALWAYS_2:
        initspe = 2;
        break;
    case ENCHTYPE_ALWAYS_3:
        initspe = 3;
        break;
    case ENCHTYPE_ALWAYS_4:
        initspe = 4;
        break;
    case ENCHTYPE_ALWAYS_5:
        initspe = 5;
        break;
    case ENCHTYPE_RING_NORMAL:
        initspe = rnd(5);
        break;
    case ENCHTYPE_RING_1_7:
        initspe = rnd(7);
        break;
    case ENCHTYPE_RING_DOUBLE:
        initspe = rnd(10);
        break;
    case ENCHTYPE_MISCELLANEOUS_NORMAL:
        initspe = rnd(5);
        break;
    case ENCHTYPE_RING_POWER:
        initspe = rnd(3);
        break;
    case ENCHTYPE_PROJECTILE:
        initspe = Inhell ? rngh(3, 4) : rngh(3, 3);
        break;
    case ENCHTYPE_THROWN_WEAPON:
        initspe = Inhell ? rngh(2, 7) : rngh(3, 4);
        break;
    case ENCHTYPE_SINGLE_HANDED_MELEE_WEAPON:
        initspe = Inhell ? rngh(2, 7) : rngh(3, 4);
        break;
    case ENCHTYPE_TWO_HANDED_MELEE_WEAPON:
        initspe = Inhell ? rngh(2, 12) : rngh(3, 7);
        break;
    case ENCHTYPE_SINGLE_HANDED_LAUNCHER:
        initspe = Inhell ? rngh(3, 4) : rngh(3, 3);
        break;
    case ENCHTYPE_TWO_HANDED_LAUNCHER:
        initspe = Inhell ? rngh(2, 5) : rngh(3, 3);
        break;
    case ENCHTYPE_TWO_HANDED_ELVEN_LAUNCHER:
        initspe = Inhell ? rngh(2, 7) : rngh(3, 4);
        break;
    case ENCHTYPE_SINGLE_HANDED_ELVEN_MELEE_WEAPON:
        initspe = Inhell ? rngh(2, 10) : rngh(3, 6);
        break;
    }

    return initspe;
}


int
get_max_enchantment(spe_type_index)
int spe_type_index;
{
    int maxspe = 1;

    switch (spe_type_index)
    {
    case ENCHTYPE_NO_ENCHANTMENT:
        maxspe = 0;
        break;
    case ENCHTYPE_GENERAL_WEAPON:
    case ENCHTYPE_GENERAL_WEAPON_ALWAYS_START_0:
        maxspe = 10;
        break;
    case ENCHTYPE_GENERAL_ARMOR:
    case ENCHTYPE_GENERAL_ARMOR_ALWAYS_START_0:
        maxspe = 3;
        break;
    case ENCHTYPE_ELVEN_ARMOR:
        maxspe = 5;
        break;
    case ENCHTYPE_LEATHER_NONBODY_ARMOR:
        maxspe = 4;
        break;
    case ENCHTYPE_LEATHER_BODY_ARMOR:
        maxspe = 5;
        break;
    case ENCHTYPE_ALWAYS_1:
        maxspe = 1;
        break;
    case ENCHTYPE_ALWAYS_2:
        maxspe = 2;
        break;
    case ENCHTYPE_ALWAYS_3:
        maxspe = 3;
        break;
    case ENCHTYPE_ALWAYS_4:
        maxspe = 4;
        break;
    case ENCHTYPE_ALWAYS_5:
        maxspe = 5;
        break;
    case ENCHTYPE_RING_NORMAL:
        maxspe = 6;
        break;
    case ENCHTYPE_RING_1_7:
        maxspe = 9;
        break;
    case ENCHTYPE_RING_DOUBLE:
        maxspe = 12;
        break;
    case ENCHTYPE_MISCELLANEOUS_NORMAL:
        maxspe = 7;
        break;
    case ENCHTYPE_PROJECTILE:
        maxspe = 5;
        break;
    case ENCHTYPE_THROWN_WEAPON:
        maxspe = 10;
        break;
    case ENCHTYPE_SINGLE_HANDED_MELEE_WEAPON:
        maxspe = 10;
        break;
    case ENCHTYPE_TWO_HANDED_MELEE_WEAPON:
        maxspe = 15;
        break;
    case ENCHTYPE_SINGLE_HANDED_LAUNCHER:
        maxspe = 5;
        break;
    case ENCHTYPE_TWO_HANDED_LAUNCHER:
        maxspe = 7;
        break;
    case ENCHTYPE_TWO_HANDED_ELVEN_LAUNCHER:
        maxspe = 10;
        break;
    case ENCHTYPE_SINGLE_HANDED_ELVEN_MELEE_WEAPON:
        maxspe = 15;
        break;
    case ENCHTYPE_PROJECTILE_ALWAYS_START_0:
        maxspe = 5;
        break;
    }

    return maxspe;
}

int 
get_multigen_quan(multigen_index)
int multigen_index;
{
    int quan = 1;

    switch (multigen_index)
    {
    case MULTIGEN_SINGLE:
        quan = 1;
        break;
    case MULTIGEN_1D2:
        quan = rnd(2);
        break;
    case MULTIGEN_1D3:
        quan = rnd(3);
        break;
    case MULTIGEN_1D4:
        quan = rnd(4);
        break;
    case MULTIGEN_1D6:
        quan = rnd(6);
        break;
    case MULTIGEN_1D8:
        quan = rnd(8);
        break;
    case MULTIGEN_1D10:
        quan = rnd(10);
        break;
    case MULTIGEN_2D6:
        quan = d(2, 6);
        break;
    case MULTIGEN_3D6:
        quan = d(3, 6);
        break;
    case MULTIGEN_4D6:
        quan = d(4, 6);
        break;
    case MULTIGEN_5D6:
        quan = d(5, 6);
        break;
    case MULTIGEN_6D6:
        quan = d(6, 6);
        break;
    case MULTIGEN_1D6_1:
        quan = rnd(6) + 1;
        break;
    case MULTIGEN_1D6_2:
        quan = rnd(6) + 2;
        break;
    case MULTIGEN_1D6_3:
        quan = rnd(6) + 3;
        break;
    case MULTIGEN_1D6_4:
        quan = rnd(6) + 4;
        break;
    case MULTIGEN_1D6_5:
        quan = rnd(6) + 5;
        break;
    case MULTIGEN_1D6_6:
        quan = rnd(6) + 6;
        break;
    case MULTIGEN_2D6_1:
        quan = d(2, 6) + 1;
        break;
    case MULTIGEN_2D6_2:
        quan = d(2, 6) + 2;
        break;
    case MULTIGEN_2D6_3:
        quan = d(2, 6) + 3;
        break;
    case MULTIGEN_2D6_4:
        quan = d(2, 6) + 4;
        break;
    case MULTIGEN_2D6_5:
        quan = d(2, 6) + 5;
        break;
    case MULTIGEN_2D6_6:
        quan = d(2, 6) + 6;
        break;
    case MULTIGEN_1D2_1:
        quan = rnd(2) + 1;
        break;
    case MULTIGEN_1D3_1:
        quan = rnd(3) + 1;
        break;
    case MULTIGEN_1D4_1:
        quan = rnd(4) + 1;
        break;
    case MULTIGEN_1D2_2:
        quan = rnd(2) + 2;
        break;
    case MULTIGEN_1D3_2:
        quan = rnd(3) + 2;
        break;
    case MULTIGEN_1D3_3:
        quan = rnd(3) + 3;
        break;
    case MULTIGEN_1D4_2:
        quan = rnd(4) + 2;
        break;
    case MULTIGEN_1D4_3:
        quan = rnd(4) + 3;
        break;
    case MULTIGEN_1D4_4:
        quan = rnd(4) + 4;
        break;
    default:
        break;
    }
    return quan;
}
/*
 * Several areas of the code made direct reassignments
 * to obj->corpsenm. Because some special handling is
 * required in certain cases, place that handling here
 * and call this routine in place of the direct assignment.
 *
 * If the object was a lizard or lichen corpse:
 *     - ensure that you don't end up with some
 *       other corpse type which has no rot-away timer.
 *
 * If the object was a troll corpse:
 *     - ensure that you don't end up with some other
 *       corpse type which resurrects from the dead.
 *
 * Re-calculates the weight of figurines and corpses to suit the
 * new species.
 *
 * Existing timeout value for egg hatch is preserved.
 *
 */
void
set_corpsenm(obj, id)
struct obj *obj;
int id;
{
    long when = 0L;

    if (obj->timed) 
    {
        if (obj->otyp == EGG)
            when = stop_timer(HATCH_EGG, obj_to_any(obj));
        else 
        {
            when = 0L;
            obj_stop_timers(obj); /* corpse or figurine */
        }
    }
    obj->corpsenm = id;

    switch (obj->otyp) 
    {
    case CORPSE:
        start_corpse_timeout(obj);
        obj->owt = weight(obj);
        break;
    case FIGURINE:
        if (obj->corpsenm != NON_PM && !dead_species(obj->corpsenm, TRUE)
            && (carried(obj) || mcarried(obj)))
            attach_fig_transform_timeout(obj);
        obj->owt = weight(obj);
        break;
    case EGG:
        if (obj->corpsenm != NON_PM && !dead_species(obj->corpsenm, TRUE))
            attach_egg_hatch_timeout(obj, when);
        break;
    default: /* tin, etc. */
        obj->owt = weight(obj);
        break;
    }
}

/*
 * Start a corpse decay or revive timer.
 * This takes the age of the corpse into consideration as of 3.4.0.
 */
void
start_corpse_timeout(body)
struct obj *body;
{
    long when = 0;       /* rot away when this old */
    long corpse_age = 0; /* age of corpse          */
    int rot_adjust= 0;
    short action = -1;

#define TAINT_AGE (50L)        /* age when corpses go bad */
#define TROLL_REVIVE_CHANCE 37 /* 1/37 chance for 50 turns ~ 75% chance */
#define PHOENIX_REVIVE_CHANCE 33
#define GENERAL_REVIVE_CHANCE 40 /* 1/40 chance for 50 turns ~ 70% chance */
#define UNIQUE_REVIVE_CHANCE 32 /* 1/32 chance for 50 turns ~ 65% chance */
#define ROT_AGE (250L)         /* age when corpses rot away */

    /* lizards and lichen don't rot */
    if (!nonrotting_corpse(body->corpsenm))
    { 
        action = ROT_CORPSE;             /* default action: rot away */
        rot_adjust = in_mklev ? 25 : 10; /* give some variation */
        corpse_age = monstermoves - body->age;
        if (corpse_age > ROT_AGE)
            when = rot_adjust;
        else
            when = ROT_AGE - corpse_age;
        when += (long)(rnz(rot_adjust) - rot_adjust);
    }

    if (is_reviver(&mons[body->corpsenm]))
    {
        if (is_rider(&mons[body->corpsenm]))
        {
            /*
             * Riders always revive.  They have a 1/3 chance per turn
             * of reviving after 12 turns.  Always revive by 500.
             */
            action = REVIVE_MON;
            for (when = 12L; when < 500L; when++)
                if (!rn2(3))
                    break;

        }
        else if (mons[body->corpsenm].geno & G_UNIQ)
        {
            /*
             * Tarrasque and other unique monsters always revive.
             */
            action = REVIVE_MON;
            for (when = 24L; when < 500L; when++)
                if (!rn2(UNIQUE_REVIVE_CHANCE))
                    break;

        }
        else if (body->corpsenm == PM_PHOENIX && !body->norevive)
        {
            /*
             * Phoenixes always revive.  They have a 1/4 chance per turn
             * of reviving after 20 turns.  Always revive by 500.
             */
            action = REVIVE_MON;
            for (when = 25L; when < 500L; when++)
                if (!rn2(PHOENIX_REVIVE_CHANCE))
                    break;

        }
        else if (mons[body->corpsenm].mlet == S_TROLL && !body->norevive) 
        {
            long age;
            for (age = 2; age <= TAINT_AGE; age++)
                if (!rn2(TROLL_REVIVE_CHANCE)) 
                { /* troll revives */
                    action = REVIVE_MON;
                    when = age;
                    break;
                }
        }
        else if(!body->norevive)
        {
            /* Base case here for all other monsters */
            long age;
            for (age = 2; age <= TAINT_AGE; age++)
                if (!rn2(GENERAL_REVIVE_CHANCE))
                { /* monster revives */
                    action = REVIVE_MON;
                    when = age;
                    break;
                }
        }
    }

    if (body->norevive)
        body->norevive = 0;

    if(action > -1 && when > 0)
        (void) start_timer(when, TIMER_OBJECT, action, obj_to_any(body));

}

STATIC_OVL void
maybe_adjust_light(obj, old_range)
struct obj *obj;
int old_range;
{
    char buf[BUFSZ];
    xchar ox, oy;
    int new_range = arti_light_radius(obj), delta = new_range - old_range;

    /* radius of light emitting artifact varies by curse/bless state
       so will change after blessing or cursing */
    if (delta) {
        obj_adjust_light_radius(obj, new_range);
        /* simplifying assumptions:  hero is wielding this object;
           artifacts have to be in use to emit light and monsters'
           gear won't change bless or curse state */
        if (!Blind && get_obj_location(obj, &ox, &oy, 0)) {
            *buf = '\0';
            if (iflags.last_msg == PLNMSG_OBJ_GLOWS)
                /* we just saw "The <obj> glows <color>." from dipping */
                Strcpy(buf, (obj->quan == 1L) ? "It" : "They");
            else if (carried(obj) || cansee(ox, oy))
                Strcpy(buf, Yname2(obj));
            if (*buf) {
                /* initial activation says "dimly" if cursed,
                   "brightly" if uncursed, and "brilliantly" if blessed;
                   when changing intensity, using "less brightly" is
                   straightforward for dimming, but we need "brighter"
                   rather than "more brightly" for brightening; ugh */
                pline_ex(ATR_NONE, CLR_MSG_ATTENTION, "%s %s %s%s.", buf, otense(obj, "shine"),
                      (abs(delta) > 1) ? "much " : "",
                      (delta > 0) ? "brighter" : "less brightly");
            }
        }
    }
}

STATIC_OVL void
maybe_adjust_sound_volume(obj, old_volume)
struct obj* obj;
double old_volume;
{
    char buf[BUFSZ];
    xchar ox, oy;
    double new_volume = obj_ambient_sound_volume(obj), delta = new_volume - old_volume;

    /* radius of light emitting artifact varies by curse/bless state
       so will change after blessing or cursing */
    if (delta) 
    {
        obj_adjust_sound_volume(obj, new_volume);
        /* simplifying assumptions:  hero is wielding this object;
           artifacts have to be in use to emit light and monsters'
           gear won't change bless or curse state */
        if (!Blind && get_obj_location(obj, &ox, &oy, 0)) 
        {
            *buf = '\0';
            Strcpy(buf, (obj->quan == 1L) ? "An object" : "Objects");
            if (carried(obj) || cansee(ox, oy))
                Strcpy(buf, Yname2(obj));

            if (*buf)
            {
                pline("%s %s sound %s%s.", buf, otense(obj, "make"),
                    (absd(delta) > 1.0) ? "much " : "",
                    (delta > 0.0) ? "louder" : "more quietly");
            }
        }
    }
}

/*
 *      bless(), curse(), unbless(), uncurse() -- any relevant message
 *      about glowing amber/black/&c should be delivered prior to calling
 *      these routines to make the actual curse/bless state change.
 */

void
bless(otmp)
register struct obj *otmp;
{
    if (!otmp)
        return;

    int old_light = 0;
    double old_volume = 0;

    if (otmp->oclass == COIN_CLASS)
        return;
    if (otmp->lamplit)
        old_light = arti_light_radius(otmp);
    if (otmp->makingsound)
        old_volume = obj_ambient_sound_volume(otmp);
    otmp->cursed = 0;
    otmp->blessed = 1;
    if (carried(otmp) && (confers_luck(otmp) || confers_unluck(otmp)))
        updateabon();
    else if (Is_weight_changing_bag(otmp))
        otmp->owt = weight(otmp);
    else if (otmp->otyp == FIGURINE && otmp->timed)
        (void) stop_timer(FIG_TRANSFORM, obj_to_any(otmp));
    if (otmp->lamplit)
        maybe_adjust_light(otmp, old_light);
    if (otmp->makingsound)
        maybe_adjust_sound_volume(otmp, old_volume);
    return;
}

void
unbless(otmp)
register struct obj *otmp;
{
    if (!otmp)
        return;

    int old_light = 0;
    double old_volume = 0;

    if (otmp->lamplit)
        old_light = arti_light_radius(otmp);
    if (otmp->makingsound)
        old_volume = obj_ambient_sound_volume(otmp);
    otmp->blessed = 0;
    if (carried(otmp) && (confers_luck(otmp) || confers_unluck(otmp)))
        updateabon();
    else if (Is_weight_changing_bag(otmp))
        otmp->owt = weight(otmp);
    if (otmp->lamplit)
        maybe_adjust_light(otmp, old_light);
    if (otmp->makingsound)
        maybe_adjust_sound_volume(otmp, old_volume);
}

void
curse(otmp)
register struct obj *otmp;
{
    if (!otmp)
        return;

    unsigned already_cursed;
    int old_light = 0;
    double old_volume = 0;

    if (otmp->oclass == COIN_CLASS)
        return;
    if (otmp->lamplit)
        old_light = arti_light_radius(otmp);
    if (otmp->makingsound)
        old_volume = obj_ambient_sound_volume(otmp);
    already_cursed = otmp->cursed;
    otmp->blessed = 0;
    if((objects[otmp->otyp].oc_flags & O1_NOT_CURSEABLE) || has_obj_mythic_uncurseable(otmp))
        otmp->cursed = 0;
    else
        otmp->cursed = 1;
    /* welded two-handed weapon interferes with some armor removal */
    if (otmp->cursed)
    {
        if (otmp == uwep && bimanual(uwep))
            reset_remarm();
        /* some cursed items need immediate updating */
        if (carried(otmp) && (confers_luck(otmp) || confers_unluck(otmp))) {
            updateabon();
        }
        else if (Is_weight_changing_bag(otmp)) {
            otmp->owt = weight(otmp);
        }
        else if (otmp->otyp == FIGURINE) {
            if (otmp->corpsenm != NON_PM && !dead_species(otmp->corpsenm, TRUE)
                && (carried(otmp) || mcarried(otmp)))
                attach_fig_transform_timeout(otmp);
        }
        else if (otmp->oclass == SPBOOK_CLASS) {
            /* if book hero is reading becomes cursed, interrupt */
            if (!already_cursed)
                book_cursed(otmp);
        }
        if (otmp->lamplit)
            maybe_adjust_light(otmp, old_light);
        if (otmp->makingsound)
            maybe_adjust_sound_volume(otmp, old_volume);
    }
    return;
}

void
uncurse(otmp)
register struct obj *otmp;
{
    if (!otmp)
        return;

    int old_light = 0;
    double old_volume = 0;

    if (otmp->lamplit)
        old_light = arti_light_radius(otmp);
    if (otmp->makingsound)
        old_volume = obj_ambient_sound_volume(otmp);
    otmp->cursed = 0;
    if (carried(otmp) && (confers_luck(otmp) || confers_unluck(otmp)))
        updateabon();
    else if (Is_weight_changing_bag(otmp))
        otmp->owt = weight(otmp);
    else if (otmp->otyp == FIGURINE && otmp->timed)
        (void) stop_timer(FIG_TRANSFORM, obj_to_any(otmp));
    if (otmp->lamplit)
        maybe_adjust_light(otmp, old_light);
    if (otmp->makingsound)
        maybe_adjust_sound_volume(otmp, old_volume);
    return;
}

void
blessorcurse(otmp, chance)
register struct obj *otmp;
register int chance;
{
    if (!otmp)
        return;

    if (otmp->blessed || otmp->cursed)
        return;

    if (!rn2(chance)) {
        if (!rn2(2)) {
            curse(otmp);
        } else {
            bless(otmp);
        }
    }
    return;
}

int
bcsign(otmp)
register struct obj *otmp;
{
    return (!!otmp->blessed - !!otmp->cursed);
}

/*
 *  Calculate the weight of the given object.  This will recursively follow
 *  and calculate the weight of any containers.
 *
 *  Note:  It is possible to end up with an incorrect weight if some part
 *         of the code messes with a contained object and doesn't update the
 *         container's weight.
 */
int
weight(obj)
register struct obj *obj;
{
    int wt = (int) objects[obj->otyp].oc_weight;

    if (has_obj_mythic_lightness(obj))
        wt /= 3;

    /* glob absorpsion means that merging globs accumulates weight while
       quantity stays 1, so update 'wt' to reflect that, unless owt is 0,
       when we assume this is a brand new glob so use objects[].oc_weight */
    if (obj->globby && obj->owt > 0)
        wt = obj->owt;
    if (Is_container(obj) || obj->otyp == STATUE) {
        struct obj *contents;
        register int cwt = 0;

        if (obj->otyp == STATUE && obj->corpsenm >= LOW_PM)
            wt = (int) obj->quan * ((int) mons[obj->corpsenm].cwt * 3 / 2);

        if ((objects[obj->otyp].oc_flags4 & O4_CONTAINER_MAY_CONTAIN_MONSTER))
        {
            struct monst* mtmp = (struct monst*)0;
            if (has_omonst(obj))
            {
                mtmp = get_mtraits(obj, FALSE);
                if (mtmp && mtmp->data)
                {
                    cwt += (int)obj->quan * ((int)mtmp->data->cwt);
                }
            }
            
            if(!mtmp && obj->corpsenm >= LOW_PM)
                cwt += (int)obj->quan * ((int)mons[obj->corpsenm].cwt);
        }

        for (contents = obj->cobj; contents; contents = contents->nobj)
        {
            if (obj->otyp == BAG_OF_WIZARDRY
                && (contents->oclass == REAGENT_CLASS || contents->oclass == SPBOOK_CLASS
                    || contents->oclass == WAND_CLASS || contents->oclass == SCROLL_CLASS
                    || objects[contents->otyp].oc_flags & O1_NONE
                    ))
                cwt += obj->cursed ? (weight(contents) * 2) : obj->blessed ? ((weight(contents) + 15) / 16)
                : ((weight(contents) + 7) / 8);
            else if (obj->otyp == BAG_OF_TREASURE_HAULING
                && (contents->oclass == COIN_CLASS || contents->oclass == GEM_CLASS
                    || contents->oclass == RING_CLASS || contents->oclass == AMULET_CLASS
                    || contents->oclass == MISCELLANEOUS_CLASS
                    || objects[contents->otyp].oc_material == MAT_SILVER
                    || objects[contents->otyp].oc_material == MAT_GOLD
                    || objects[contents->otyp].oc_material == MAT_PLATINUM
                    || objects[contents->otyp].oc_material == MAT_MITHRIL
                    || objects[contents->otyp].oc_material == MAT_ADAMANTIUM
                    || objects[contents->otyp].oc_material == MAT_GEMSTONE
                    ))
                cwt += obj->cursed ? (weight(contents) * 2) : obj->blessed ? ((weight(contents) + 63) / 64)
                : ((weight(contents) + 31) / 32);
            else if (obj->otyp == BAG_OF_THE_GLUTTON
                && (contents->oclass == POTION_CLASS || is_obj_normally_edible(contents)
                    ))
                cwt += obj->cursed ? (weight(contents) * 2) : obj->blessed ? ((weight(contents) + 19) / 20)
                : ((weight(contents) + 9) / 10);
            else
                cwt += weight(contents);
        }
        /*
         *  The weight of bags of holding is calculated as the weight
         *  of the bag plus the weight of the bag's contents modified
         *  as follows:
         *
         *      Bag status      Weight of contents
         *      ----------      ------------------
         *      cursed                  2x
         *      blessed                 x/4 [rounded up: (x+3)/4]
         *      otherwise               x/2 [rounded up: (x+1)/2]
         *
         *  The macro DELTA_CWT in pickup.c also implements these
         *  weight equations.
         */
        if (obj->otyp == BAG_OF_HOLDING || obj->otyp == GOLDEN_CHEST)
            cwt = obj->cursed ? (cwt * 2) : obj->blessed ? ((cwt + 3) / 4)
                                                         : ((cwt + 1) / 2);

        return wt + cwt;
    }
    if (obj->otyp == CORPSE && obj->corpsenm >= LOW_PM) {
        long long_wt = obj->quan * (long) mons[obj->corpsenm].cwt;

        wt = (long_wt > LARGEST_INT) ? LARGEST_INT : (int) long_wt;
        if (obj->oeaten)
            wt = eaten_stat(wt, obj);
        return wt;
    } else if (obj->oclass == FOOD_CLASS && obj->oeaten) {
        return eaten_stat((int) obj->quan * wt, obj);
    } else if (obj->oclass == COIN_CLASS) {
        return (int) ((obj->quan) / 10L) + 1;
    } else if (obj->otyp == HEAVY_IRON_BALL && obj->owt != 0) {
        return (int) obj->owt; /* kludge for "very" heavy iron ball */
    } else if (is_obj_candelabrum(obj) && obj->special_quality > 0) {
        return wt + obj->special_quality * (int) objects[TALLOW_CANDLE].oc_weight;
    }
    return (wt ? wt * (int) obj->quan : ((int) obj->quan + 1) >> 1);
}

static int treefruits[] = { APPLE, ORANGE, PEAR, BANANA, POMEGRANATE, EUCALYPTUS_LEAF, FIG, DRAGON_FRUIT };

struct obj *
rnd_treefruit_at(x, y)
int x, y;
{
    return mksobj_at(treefruits[rn2(SIZE(treefruits))], x, y, TRUE, FALSE);
}

struct obj *
mkgold(amount, x, y)
long amount;
int x, y;
{
    register struct obj *gold = g_at(x, y);

    if (amount <= 0L) {
        long mul = rnd(30 / max(12-depth(&u.uz), 2));
        amount = (long) (1 + rnd(level_difficulty() + 2) * mul);
    }
    if (gold) {
        gold->quan += amount;
    } else {
        gold = mksobj_at(GOLD_PIECE, x, y, TRUE, FALSE);
        gold->quan = amount;
    }
    gold->owt = weight(gold);
    return gold;
}

/* return TRUE if the corpse has special timing */
#define special_corpse(mnum)                                                 \
    (nonrotting_corpse(mnum) || is_reviver_corpse(mnum))

/*
 * OEXTRA note: Passing mtmp causes mtraits to be saved
 * even if ptr passed as well, but ptr is always used for
 * the corpse type (corpsenm). That allows the corpse type
 * to be different from the original monster,
 *      i.e.  vampire -> human corpse
 * yet still allow restoration of the original monster upon
 * resurrection.
 */
struct obj *
mkcorpstat(objtype, mtmp, ptr, x, y, corpstatflags)
int objtype; /* CORPSE or STATUE */
struct monst *mtmp;
struct permonst *ptr;
int x, y;
unsigned corpstatflags;
{
    register struct obj *otmp;
    boolean init = ((corpstatflags & CORPSTAT_INIT) != 0);
    
    if (objtype != CORPSE && objtype != STATUE)
        impossible("making corpstat type %d", objtype);

    if (x == 0 && y == 0) 
    { /* special case - random placement */
        otmp = mksobj(objtype, init, FALSE, FALSE);
        if (otmp)
            (void) rloco(otmp);
    }
    else
        otmp = mksobj_at(objtype, x, y, init, FALSE);

    if (otmp) 
    {
        if (mtmp) 
        {
            struct obj *otmp2;

            if (!ptr)
                ptr = mtmp->data;
            /* save_mtraits frees original data pointed to by otmp */
            otmp2 = save_mtraits(otmp, mtmp);
            if (otmp2)
                otmp = otmp2;
        }
        /* use the corpse or statue produced by mksobj() as-is
           unless `ptr' is non-null */
        if (ptr) 
        {
            int old_corpsenm = otmp->corpsenm;

            otmp->corpsenm = monsndx(ptr);
            otmp->owt = weight(otmp);
            if (otmp->otyp == CORPSE && (special_corpse(old_corpsenm)
                                         || special_corpse(otmp->corpsenm))) 
            {
                obj_stop_timers(otmp);
                start_corpse_timeout(otmp);
            }
        }
    }
    return otmp;
}

/*
 * Return the type of monster that this corpse will
 * revive as, even if it has a monster structure
 * attached to it. In that case, you can't just
 * use obj->corpsenm, because the stored monster
 * type can, and often is, different.
 * The return value is an index into mons[].
 */
int
corpse_revive_type(obj)
struct obj *obj;
{
    int revivetype;
    struct monst *mtmp;
    if (has_omonst(obj)
        && ((mtmp = get_mtraits(obj, FALSE)) != (struct monst *) 0)) {
        /* mtmp is a temporary pointer to a monster's stored
        attributes, not a real monster */
        revivetype = mtmp->mnum;
    } else
        revivetype = obj->corpsenm;
    return revivetype;
}

/*
 * Attach a monster id to an object, to provide
 * a lasting association between the two.
 */
struct obj *
obj_attach_mid(obj, mid)
struct obj *obj;
unsigned mid;
{
    if (!mid || !obj)
        return (struct obj *) 0;
    newomid(obj);
    *OMID(obj) = mid;
    return obj;
}

static struct obj *
save_mtraits(obj, mtmp)
struct obj *obj;
struct monst *mtmp;
{
    if (mtmp->ispriest)
        forget_temple_entry(mtmp); /* EPRI() */
    if (mtmp->issmith)
        forget_smithy_entry(mtmp); /* ESMI() */
    if (mtmp->isnpc)
        forget_npc_entry(mtmp); /* ENPC() */
    if (!has_omonst(obj))
        newomonst(obj);
    if (has_omonst(obj)) 
    {
        struct monst *mtmp2 = OMONST(obj);

        *mtmp2 = *mtmp;
        mtmp2->mextra = (struct mextra *) 0;

#if 0 /* All monsters should have data and mnum */
        if (mtmp->data)
            mtmp2->mnum = monsndx(mtmp->data);
#endif
        /* invalidate pointers */
        /* m_id is needed to know if this is a revived quest leader */
        /* but m_id must be cleared when loading bones */
        mtmp2->nmon = (struct monst *) 0;
        //mtmp2->data = (struct permonst *) 0; /* This sounds very dangerous to set to zero */
        mtmp2->minvent = (struct obj *) 0;
        mtmp2->facing_right = mtmp->facing_right;
        if (mtmp->mextra)
            copy_mextra(mtmp2, mtmp);
    }
    return obj;
}

/* returns a pointer to a new monst structure based on
 * the one contained within the obj.
 */
struct monst *
get_mtraits(obj, copyof)
struct obj *obj;
boolean copyof;
{
    struct monst *mtmp = (struct monst *) 0;
    struct monst *mnew = (struct monst *) 0;

    if (has_omonst(obj))
        mtmp = OMONST(obj);
    if (mtmp) {
        if (copyof) {
            mnew = newmonst();
            *mnew = *mtmp;
            mnew->mextra = (struct mextra *) 0;
            if (mtmp->mextra)
                copy_mextra(mnew, mtmp);
        } else {
            /* Never insert this returned pointer into mon chains! */
            mnew = mtmp;
        }

        if (mnew && !mnew->data)
            mnew->data = &mons[mnew->mnum];
    }

    return mnew;
}

/* make an object named after someone listed in the scoreboard file */
struct obj *
mk_tt_object(objtype, x, y)
int objtype; /* CORPSE or STATUE */
register int x, y;
{
    register struct obj *otmp, *otmp2;
    boolean initialize_it;

    /* player statues never contain books */
    initialize_it = (objtype != STATUE);
    if ((otmp = mksobj_at(objtype, x, y, initialize_it, FALSE)) != 0) {
        /* tt_oname will return null if the scoreboard is empty */
        if ((objtype == STATUE || objtype == CORPSE) && (otmp2 = tt_oname(otmp)) != 0)
            otmp = otmp2;
        else
            set_corpsenm(otmp, rn2(PM_WIZARD - PM_ARCHAEOLOGIST + 1) + PM_ARCHAEOLOGIST);
    }
    return otmp;
}

/* make a new corpse or statue, uninitialized if a statue (i.e. no books) */
struct obj *
mk_named_object(objtype, ptr, x, y, nm)
int objtype; /* CORPSE or STATUE */
struct permonst *ptr;
int x, y;
const char *nm;
{
    struct obj *otmp;
    unsigned corpstatflags =
        (objtype != STATUE) ? CORPSTAT_INIT : CORPSTAT_NONE;

    otmp = mkcorpstat(objtype, (struct monst *) 0, ptr, x, y, corpstatflags);
    if (nm)
        otmp = oname(otmp, nm);
    return otmp;
}

boolean
is_flammable(otmp)
register struct obj *otmp;
{
    int otyp = otmp->otyp;
    int omat = objects[otyp].oc_material;

    /* Candles can be burned, but they're not flammable in the sense that
     * they can't get fire damage and it makes no sense for them to be
     * fireproofed.
     */
    if (is_candle(otmp))
        return FALSE;

    if ((objects[otyp].oc_flags & O1_FIRE_RESISTANT) != 0)
        return FALSE;

    return (boolean)material_definitions[omat].flammable; // ((omat <= MAT_WOOD && omat != MAT_LIQUID) || omat == MAT_PLASTIC);
}

boolean
is_rottable(otmp)
register struct obj *otmp;
{
    int otyp = otmp->otyp;

    if (objects[otyp].oc_flags & O1_ROT_RESISTANT)
        return FALSE;

    return (boolean)material_definitions[objects[otyp].oc_material].rottable;// <= MAT_WOOD && objects[otyp].oc_material != MAT_LIQUID);
}

/*
 * These routines maintain the single-linked lists headed in level.objects[][]
 * and threaded through the nexthere fields in the object-instance structure.
 */

/* put the object at the given location */
void
place_object(otmp, x, y)
register struct obj *otmp;
int x, y;
{
    register struct obj *otmp2 = level.objects[x][y];

    if (otmp->where != OBJ_FREE)
    {
        panic("place_object: obj not free");
        return;
    }

    obj_no_longer_held(otmp);
    /* (could bypass this vision update if there is already a boulder here) */
    if (otmp->otyp == BOULDER)
        block_vision_and_hearing_at_point(x, y); /* vision */

    /* obj goes under boulders */
    if (otmp2 && (otmp2->otyp == BOULDER))
    {
        otmp->nexthere = otmp2->nexthere;
        otmp2->nexthere = otmp;
    } 
    else 
    {
        otmp->nexthere = otmp2;
        level.objects[x][y] = otmp;
    }

    /* set the new object's location */
    otmp->ox = otmp->ox0 = x;
    otmp->oy = otmp->oy0 = y;

    otmp->where = OBJ_FLOOR;

    /* add to floor chain */
    otmp->nobj = fobj;
    fobj = otmp;
    if (otmp->timed)
        obj_timer_checks(otmp, x, y, 0);
}

/* put the memory object at the given location */
void
place_memory_object(otmp, x, y)
register struct obj* otmp;
int x, y;
{
    register struct obj* otmp2 = level.locations[x][y].hero_memory_layers.memory_objchn;

    if (otmp->where != OBJ_FREE)
    {
        panic("place_memory_object: obj not free");
        return;
    }

    /* obj goes under boulders */
    if (otmp2 && (otmp2->otyp == BOULDER))
    {
        otmp->nexthere = otmp2->nexthere;
        otmp2->nexthere = otmp;
    }
    else
    {
        otmp->nexthere = otmp2;
        level.locations[x][y].hero_memory_layers.memory_objchn = otmp;
    }

    /* set the new object's location */
    otmp->ox = x;
    otmp->oy = y;

    otmp->where = OBJ_HEROMEMORY;

    /* add to floor chain */
    otmp->nobj = memoryobjs;
    memoryobjs = otmp;

    /* If there is a memory object, then it must be flagged as shown */
    if (!level.locations[x][y].hero_memory_layers.memory_objchn)
        level.locations[x][y].hero_memory_layers.layer_flags |= LFLAGS_SHOWING_MEMORY;
}

void
remove_memory_object(otmp)
register struct obj* otmp;
{
    xchar x = otmp->ox;
    xchar y = otmp->oy;

    if (otmp->where != OBJ_HEROMEMORY)
    {
        panic("remove_memory_object: obj not in hero memory");
        return;
    }
    extract_nexthere(otmp, &level.locations[x][y].hero_memory_layers.memory_objchn);
    extract_nobj(otmp, &memoryobjs);
    otmp->lamplit = 0;
    otmp->makingsound = 0;
}


#define ROT_ICE_ADJUSTMENT 2 /* rotting on ice takes 2 times as long */

/* If ice was affecting any objects correct that now
 * Also used for starting ice effects too. [zap.c]
 */
void
obj_ice_effects(x, y, do_buried)
int x, y;
boolean do_buried;
{
    struct obj *otmp;

    for (otmp = level.objects[x][y]; otmp; otmp = otmp->nexthere) {
        if (otmp->timed)
            obj_timer_checks(otmp, x, y, 0);
    }
    if (do_buried) {
        for (otmp = level.buriedobjlist; otmp; otmp = otmp->nobj) {
            if (otmp->ox == x && otmp->oy == y) {
                if (otmp->timed)
                    obj_timer_checks(otmp, x, y, 0);
            }
        }
    }
}

/*
 * Returns an obj->age for a corpse object on ice, that would be the
 * actual obj->age if the corpse had just been lifted from the ice.
 * This is useful when just using obj->age in a check or calculation because
 * rot timers pertaining to the object don't have to be stopped and
 * restarted etc.
 */
long
peek_at_iced_corpse_age(otmp)
struct obj *otmp;
{
    long age, retval = otmp->age;

    if (otmp->otyp == CORPSE && (otmp->speflags & SPEFLAGS_CORPSE_ON_ICE)) {
        /* Adjust the age; must be same as obj_timer_checks() for off ice*/
        age = monstermoves - otmp->age;
        retval += age * (ROT_ICE_ADJUSTMENT - 1) / ROT_ICE_ADJUSTMENT;
        debugpline3(
          "The %s age has ice modifications: otmp->age = %ld, returning %ld.",
                    s_suffix(doname(otmp)), otmp->age, retval);
        debugpline1("Effective age of corpse: %ld.", monstermoves - retval);
    }
    return retval;
}

STATIC_OVL void
obj_timer_checks(otmp, x, y, force)
struct obj *otmp;
xchar x, y;
int force; /* 0 = no force so do checks, <0 = force off, >0 force on */
{
    long tleft = 0L;
    short action = ROT_CORPSE;
    boolean restart_timer = FALSE;
    boolean on_floor = (otmp->where == OBJ_FLOOR);
    boolean buried = (otmp->where == OBJ_BURIED);

    /* Check for corpses just placed on or in ice */
    if (otmp->otyp == CORPSE && (on_floor || buried) && is_ice(x, y)) {
        tleft = stop_timer(action, obj_to_any(otmp));
        if (tleft == 0L) {
            action = REVIVE_MON;
            tleft = stop_timer(action, obj_to_any(otmp));
        }
        if (tleft != 0L) {
            long age;

            /* mark the corpse as being on ice */
            otmp->speflags |= SPEFLAGS_CORPSE_ON_ICE;
            debugpline3("%s is now on ice at <%d,%d>.", The(xname(otmp)), x,
                        y);
            /* Adjust the time remaining */
            tleft *= ROT_ICE_ADJUSTMENT;
            restart_timer = TRUE;
            /* Adjust the age; time spent off ice needs to be multiplied
               by the ice adjustment and subtracted from the age so that
               later calculations behave as if it had been on ice during
               that time (longwinded way of saying this is the inverse
               of removing it from the ice and of peeking at its age). */
            age = monstermoves - otmp->age;
            otmp->age = monstermoves - (age * ROT_ICE_ADJUSTMENT);
        }

    /* Check for corpses coming off ice */
    } else if (force < 0 || (otmp->otyp == CORPSE && (otmp->speflags & SPEFLAGS_CORPSE_ON_ICE)
                             && !((on_floor || buried) && is_ice(x, y)))) {
        tleft = stop_timer(action, obj_to_any(otmp));
        if (tleft == 0L) {
            action = REVIVE_MON;
            tleft = stop_timer(action, obj_to_any(otmp));
        }
        if (tleft != 0L) {
            long age;

            otmp->speflags &= ~SPEFLAGS_CORPSE_ON_ICE;
            debugpline3("%s is no longer on ice at <%d,%d>.",
                        The(xname(otmp)), x, y);
            /* Adjust the remaining time */
            tleft /= ROT_ICE_ADJUSTMENT;
            restart_timer = TRUE;
            /* Adjust the age */
            age = monstermoves - otmp->age;
            otmp->age += age * (ROT_ICE_ADJUSTMENT - 1) / ROT_ICE_ADJUSTMENT;
        }
    }

    /* now re-start the timer with the appropriate modifications */
    if (restart_timer)
        (void) start_timer(tleft, TIMER_OBJECT, action, obj_to_any(otmp));
}

#undef ROT_ICE_ADJUSTMENT

void
remove_object(otmp)
register struct obj *otmp;
{
    xchar x = otmp->ox;
    xchar y = otmp->oy;

    if (otmp->where != OBJ_FLOOR)
    {
        panic("remove_object: obj not on floor");
        return;
    }
    extract_nexthere(otmp, &level.objects[x][y]);
    extract_nobj(otmp, &fobj);
    /* update vision iff this was the only boulder at its spot */
    if (otmp->otyp == BOULDER && !sobj_at(BOULDER, x, y))
        unblock_vision_and_hearing_at_point(x, y); /* vision */
    if (otmp->timed)
        obj_timer_checks(otmp, x, y, 0);
}

/* throw away all of a monster's inventory */
void
discard_minvent(mtmp)
struct monst *mtmp;
{
    struct obj *otmp, *mwep = MON_WEP(mtmp);
    boolean keeping_mon = (!DEADMONSTER(mtmp));

    while ((otmp = mtmp->minvent) != 0) {
        /* this has now become very similar to m_useupall()... */
        obj_extract_self(otmp);
        if (otmp->owornmask) 
        {
            if (keeping_mon) 
            {
                if (otmp == mwep)
                    mwepgone(mtmp), mwep = 0;
                mtmp->worn_item_flags &= ~otmp->owornmask;
                otmp->owornmask = 0L;
                update_all_mon_statistics(mtmp, TRUE);
                if (mtmp == u.usteed && otmp->otyp == SADDLE)
                    dismount_steed(DISMOUNT_FELL);
            }
            otmp->owornmask = 0L; /* obfree() expects this */
        }
        obfree(otmp, (struct obj *) 0); /* dealloc_obj() isn't sufficient */
    }
}

/*
 * Free obj from whatever list it is on in preparation for deleting it
 * or moving it elsewhere; obj->where will end up set to OBJ_FREE.
 * Doesn't handle unwearing of objects in hero's or monsters' inventories.
 *
 * Object positions:
 *      OBJ_FREE        not on any list
 *      OBJ_FLOOR       fobj, level.locations[][] chains (use remove_object)
 *      OBJ_CONTAINED   cobj chain of container object
 *      OBJ_INVENT      hero's invent chain (use freeinv)
 *      OBJ_MINVENT     monster's invent chain
 *      OBJ_MIGRATING   migrating chain
 *      OBJ_BURIED      level.buriedobjs chain
 *      OBJ_ONBILL      on billobjs chain
 *      OBJ_HEROMEMORY  on memoryobjs chain
 */
void
obj_extract_self(obj)
struct obj *obj;
{
    switch (obj->where) {
    case OBJ_FREE:
        break;
    case OBJ_FLOOR:
        remove_object(obj);
        break;
    case OBJ_CONTAINED:
        extract_nobj(obj, &obj->ocontainer->cobj);
        container_weight(obj->ocontainer);
        obj->ocontainer = (struct obj *) 0; /* clear stale back-link */
        break;
    case OBJ_INVENT:
        freeinv(obj);
        break;
    case OBJ_MINVENT:
        extract_nobj(obj, &obj->ocarry->minvent);
        obj->ocarry = (struct monst *) 0; /* clear stale back-link */
        break;
    case OBJ_MIGRATING:
        extract_nobj(obj, &migrating_objs);
        break;
    case OBJ_BURIED:
        extract_nobj(obj, &level.buriedobjlist);
        break;
    case OBJ_ONBILL:
        extract_nobj(obj, &billobjs);
        break;
    case OBJ_HEROMEMORY:
        remove_memory_object(obj);
        break;
    default:
        panic("obj_extract_self");
        break;
    }
}

/* Extract the given object from the chain, following nobj chain. */
void
extract_nobj(obj, head_ptr)
struct obj *obj, **head_ptr;
{
    struct obj *curr, *prev;

    curr = *head_ptr;
    for (prev = (struct obj *) 0; curr; prev = curr, curr = curr->nobj) {
        if (curr == obj) {
            if (prev)
                prev->nobj = curr->nobj;
            else
                *head_ptr = curr->nobj;
            break;
        }
    }
    if (!curr)
    {
        panic("extract_nobj: object lost");
        return;
    }
    obj->where = OBJ_FREE;
    obj->nobj = (struct obj *) 0;
}

/*
 * Extract the given object from the chain, following nexthere chain.
 *
 * This does not set obj->where, this function is expected to be called
 * in tandem with extract_nobj, which does set it.
 */
void
extract_nexthere(obj, head_ptr)
struct obj *obj, **head_ptr;
{
    struct obj *curr, *prev;

    curr = *head_ptr;
    for (prev = (struct obj *) 0; curr; prev = curr, curr = curr->nexthere) {
        if (curr == obj) {
            if (prev)
                prev->nexthere = curr->nexthere;
            else
                *head_ptr = curr->nexthere;
            break;
        }
    }
    if (!curr)
    {
        panic("extract_nexthere: object lost");
        return;
    }
    obj->nexthere = (struct obj *) 0;
}

/*
 * Add obj to mon's inventory.  If obj is able to merge with something already
 * in the inventory, then the passed obj is deleted and 1 is returned.
 * Otherwise 0 is returned.
 */
int
add_to_minv(mon, obj)
struct monst *mon;
struct obj *obj;
{
    struct obj *otmp;

    if (obj->where != OBJ_FREE)
    {
        panic("add_to_minv: obj not free");
        return 0;
    }
    /* merge if possible */
    for (otmp = mon->minvent; otmp; otmp = otmp->nobj)
        if (merged(&otmp, &obj))
            return 1; /* obj merged and then free'd */
    /* else insert; don't bother forcing it to end of chain */
    obj->where = OBJ_MINVENT;
    obj->ocarry = mon;
    obj->nobj = mon->minvent;
    mon->minvent = obj;
    return 0; /* obj on mon's inventory chain */
}

/*
 * Add obj to container, make sure obj is "free".  Returns (merged) obj.
 * The input obj may be deleted in the process.
 */
struct obj *
add_to_container(container, obj)
struct obj *container, *obj;
{
    struct obj *otmp;

    if (obj->where != OBJ_FREE)
    {
        panic("add_to_container: obj not free");
        return (struct obj*)0;
    }
    if (container->where != OBJ_INVENT && container->where != OBJ_MINVENT)
        obj_no_longer_held(obj);

    /* merge if possible */
    for (otmp = container->cobj; otmp; otmp = otmp->nobj)
        if (merged(&otmp, &obj))
            return otmp;

    obj->where = OBJ_CONTAINED;
    obj->ocontainer = container;
    obj->nobj = container->cobj;
    container->cobj = obj;
    return obj;
}

void
add_to_migration(obj)
struct obj *obj;
{
    if (obj->where != OBJ_FREE)
    {
        panic("add_to_migration: obj not free");
        return;
    }

    /* lock picking context becomes stale if it's for this object */
    if (Is_container(obj))
        maybe_reset_pick(obj);

    obj->where = OBJ_MIGRATING;
    obj->nobj = migrating_objs;
    migrating_objs = obj;
}

void
add_to_buried(obj)
struct obj *obj;
{
    if (obj->where != OBJ_FREE)
    {
        panic("add_to_buried: obj not free");
        return;
    }

    obj->where = OBJ_BURIED;
    obj->nobj = level.buriedobjlist;
    level.buriedobjlist = obj;
}

/* Recalculate the weight of this container and all of _its_ containers. */
STATIC_OVL void
container_weight(container)
struct obj *container;
{
    container->owt = weight(container);
    if (container->where == OBJ_CONTAINED)
        container_weight(container->ocontainer);
    /*
        else if (container->where == OBJ_INVENT)
        recalculate load delay here ???
    */
}

/*
 * Deallocate the object.  _All_ objects should be run through here for
 * them to be deallocated.
 */
void
dealloc_obj(obj)
struct obj *obj;
{
    if (obj->where != OBJ_FREE)
        panic("dealloc_obj: obj not free");
    if (obj->nobj)
        panic("dealloc_obj with nobj");
    if (obj->cobj)
        panic("dealloc_obj with cobj");

    /* free up any timers attached to the object */
    if (obj->timed)
        obj_stop_timers(obj);

    /*
     * Free up any light sources attached to the object.
     *
     * We may want to just call del_light_source() without any
     * checks (requires a code change there).  Otherwise this
     * list must track all objects that can have a light source
     * attached to it (and also requires lamplit to be set).
     */
    if (obj_sheds_light(obj))
        del_light_source(LS_OBJECT, obj_to_any(obj));

    if (obj_has_sound_source(obj))
        del_sound_source(SOUNDSOURCE_OBJECT, obj_to_any(obj));

    if (obj == thrownobj)
        thrownobj = 0;
    if (obj == kickedobj)
        kickedobj = 0;

    if (obj->oextra)
        dealloc_oextra(obj);
    free((genericptr_t) obj);
}

/* create an object from a horn of plenty; mirrors bagotricks(makemon.c) */
int
hornoplenty(horn, tipping)
struct obj *horn;
boolean tipping; /* caller emptying entire contents; affects shop handling */
{
    int objcount = 0;

    if (!horn || horn->otyp != HORN_OF_PLENTY)
    {
        impossible("bad horn o' plenty");
    } 
    else if (horn->charges < 1)
    {
        play_sfx_sound(SFX_GENERAL_OUT_OF_CHARGES);
        pline1(nothing_happens);
    }
    else 
    {
        struct obj *obj;
        const char *what;

        consume_obj_charge(horn, !tipping);
        if (!rn2(13)) 
        {
            obj = mkobj(POTION_CLASS, FALSE, FALSE);
            if (objects[obj->otyp].oc_magic)
                do 
                {
                    obj->otyp = rnd_class(POT_ELVEN_HERBAL_BREW, POT_WATER);
                } while (obj->otyp == POT_SICKNESS || obj->otyp == POT_POISON || obj->otyp == POT_URINE);
            what = (obj->quan > 1L) ? "Some potions" : "A potion";
        }
        else 
        {
            obj = mkobj(FOOD_CLASS, FALSE, FALSE);
            if (obj->otyp == FOOD_RATION && !rn2(7))
                obj->otyp = LUMP_OF_ROYAL_JELLY;
            what = "Some food";
        }
        ++objcount;
        pline("%s %s out.", what, vtense(what, "spill"));
        obj->blessed = horn->blessed;
        obj->cursed = horn->cursed;
        obj->owt = weight(obj);
        /* using a shop's horn of plenty entails a usage fee and also
           confers ownership of the created item to the shopkeeper */
        if (horn->unpaid)
            addtobill(obj, FALSE, FALSE, tipping);
        /* if it ended up on bill, we don't want "(unpaid, N zorkids)"
           being included in its formatted name during next message */
        iflags.suppress_price++;
        if (!tipping) 
        {
            obj = hold_another_object(obj,
                                      u.uswallow
                                        ? "Oops!  %s out of your reach!"
                                        : (Is_airlevel(&u.uz)
                                           || Is_waterlevel(&u.uz)
                                           || levl[u.ux][u.uy].typ < IRONBARS
                                           || levl[u.ux][u.uy].typ >= ICE)
                                          ? "Oops!  %s away from you!"
                                          : "Oops!  %s to the floor!",
                                      The(aobjnam(obj, "slip")), (char *) 0);
            nhUse(obj);
        } 
        else
        {
            /* assumes this is taking place at hero's location */
            if (!can_reach_floor(TRUE))
            {
                hitfloor(obj, TRUE); /* does altar check, message, drop */
            }
            else 
            {
                if (IS_ALTAR(levl[u.ux][u.uy].typ))
                    doaltarobj(obj); /* does its own drop message */
                else
                    pline("%s %s to the %s.", Doname2(obj),
                          otense(obj, "drop"), surface(u.ux, u.uy));
                dropy(obj);
            }
        }
        iflags.suppress_price--;
        if (horn->dknown)
            makeknown(HORN_OF_PLENTY);
    }
    return objcount;
}

/* support for wizard-mode's `sanity_check' option */

static const char NEARDATA /* pline formats for insane_object() */
    ofmt0[] = "%s obj %s %s: %s",
    ofmt3[] = "%s [not null] %s %s: %s",
    /* " held by mon %p (%s)" will be appended, filled by M,mon_nam(M) */
    mfmt1[] = "%s obj %s %s (%s)",
    mfmt2[] = "%s obj %s %s (%s) *not*";

/* Check all object lists for consistency. */
void
obj_sanity_check()
{
    int x, y;
    struct obj *obj;

    /*
     * TODO:
     *  Should check whether the obj->bypass and/or obj->nomerge bits
     *  are set.  Those are both used for temporary purposes and should
     *  be clear between moves.
     */

    objlist_sanity(fobj, OBJ_FLOOR, "floor sanity");

    /* check that the map's record of floor objects is consistent;
       those objects should have already been sanity checked via
       the floor list so container contents are skipped here */
    for (x = 0; x < COLNO; x++)
        for (y = 0; y < ROWNO; y++)
            for (obj = level.objects[x][y]; obj; obj = obj->nexthere) {
                /* <ox,oy> should match <x,y>; <0,*> should always be empty */
                if (obj->where != OBJ_FLOOR || x == 0
                    || obj->ox != x || obj->oy != y) {
                    char at_fmt[BUFSZ];

                    Sprintf(at_fmt, "%%s obj@<%d,%d> %%s %%s: %%s@<%d,%d>",
                            x, y, obj->ox, obj->oy);
                    insane_object(obj, at_fmt, "location sanity",
                                  (struct monst *) 0);
                }
            }

    objlist_sanity(invent, OBJ_INVENT, "invent sanity");
    objlist_sanity(migrating_objs, OBJ_MIGRATING, "migrating sanity");
    objlist_sanity(level.buriedobjlist, OBJ_BURIED, "buried sanity");
    objlist_sanity(billobjs, OBJ_ONBILL, "bill sanity");
    objlist_sanity(memoryobjs, OBJ_HEROMEMORY, "memoryobjs sanity");

    mon_obj_sanity(fmon, "minvent sanity");
    mon_obj_sanity(migrating_mons, "migrating minvent sanity");
    /* monsters temporarily in transit;
       they should have arrived with hero by the time we get called */
    if (mydogs) {
        impossible("mydogs sanity [not empty]");
        mon_obj_sanity(mydogs, "mydogs minvent sanity");
    }

    /* objects temporarily freed from invent/floor lists;
       they should have arrived somewhere by the time we get called */
    if (thrownobj)
        insane_object(thrownobj, ofmt3, "thrownobj sanity",
                      (struct monst *) 0);
    if (kickedobj)
        insane_object(kickedobj, ofmt3, "kickedobj sanity",
                      (struct monst *) 0);
    /* current_wand isn't removed from invent while in use, but should
       be Null between moves when we're called */
    if (current_wand)
        insane_object(current_wand, ofmt3, "current_wand sanity",
                      (struct monst *) 0);
}

/* sanity check for objects on specified list (fobj, &c) */
STATIC_OVL void
objlist_sanity(objlist, wheretype, mesg)
struct obj *objlist;
int wheretype;
const char *mesg;
{
    struct obj *obj;

    for (obj = objlist; obj; obj = obj->nobj) {
        if (obj->where != wheretype)
            insane_object(obj, ofmt0, mesg, (struct monst *) 0);
        if (Has_contents(obj)) {
            if (wheretype == OBJ_ONBILL)
                /* containers on shop bill should always be empty */
                insane_object(obj, "%s obj contains something! %s %s: %s",
                              mesg, (struct monst *) 0);
            check_contained(obj, mesg);
        }
        if (obj->owornmask) {
            char maskbuf[40];
            boolean bc_ok = FALSE;

            switch (obj->where) {
            case OBJ_INVENT:
            case OBJ_MINVENT:
                sanity_check_worn(obj);
                break;
            case OBJ_MIGRATING:
                /* migrating objects overload the owornmask field
                   with a destination code; skip attempt to check it */
                break;
            case OBJ_FLOOR:
                /* note: ball and chain can also be OBJ_FREE, but not across
                   turns so this sanity check shouldn't encounter that */
                bc_ok = TRUE;
            /*FALLTHRU*/
            default:
                if ((obj != uchain && obj != uball) || !bc_ok) {
                    /* discovered an object not in inventory which
                       erroneously has worn mask set */
                    Sprintf(maskbuf, "worn mask 0x%08lx", obj->owornmask);
                    insane_object(obj, ofmt0, maskbuf, (struct monst *) 0);
                }
                break;
            }
        }
    }
}

/* sanity check for objects carried by all monsters in specified list */
STATIC_OVL void
mon_obj_sanity(monlist, mesg)
struct monst *monlist;
const char *mesg;
{
    struct monst *mon;
    struct obj *obj, *mwep;

    for (mon = monlist; mon; mon = mon->nmon) {
        if (DEADMONSTER(mon)) continue;
        mwep = MON_WEP(mon);
        if (mwep) {
            if (!mcarried(mwep))
                insane_object(mwep, mfmt1, mesg, mon);
            if (mwep->ocarry != mon)
                insane_object(mwep, mfmt2, mesg, mon);
        }
        for (obj = mon->minvent; obj; obj = obj->nobj) {
            if (obj->where != OBJ_MINVENT)
                insane_object(obj, mfmt1, mesg, mon);
            if (obj->ocarry != mon)
                insane_object(obj, mfmt2, mesg, mon);
            check_contained(obj, mesg);
        }
    }
}

/* This must stay consistent with the defines in obj.h. */
static const char *obj_state_names[NOBJ_STATES] = { "free",      "floor",
                                                    "contained", "invent",
                                                    "minvent",   "migrating",
                                                    "buried",    "onbill",
                                                    "heromemory" };

STATIC_OVL const char *
where_name(obj)
struct obj *obj;
{
    static char unknown[32]; /* big enough to handle rogue 64-bit int */
    int where;

    if (!obj)
        return "nowhere";
    where = obj->where;
    if (where < 0 || where >= NOBJ_STATES || !obj_state_names[where]) {
        Sprintf(unknown, "unknown[%d]", where);
        return unknown;
    }
    return obj_state_names[where];
}

STATIC_OVL void
insane_object(obj, fmt, mesg, mon)
struct obj *obj;
const char *fmt, *mesg;
struct monst *mon;
{
    const char *objnm, *monnm;
    char altfmt[BUFSZ];

    objnm = monnm = "null!";
    if (obj) {
        iflags.override_ID++;
        objnm = doname(obj);
        iflags.override_ID--;
    }
    if (mon || (strstri(mesg, "minvent") && !strstri(mesg, "contained"))) {
        Strcat(strcpy(altfmt, fmt), " held by mon %s (%s)");
        if (mon)
            monnm = x_monnam(mon, ARTICLE_A, (char *) 0, EXACT_NAME, TRUE);
        impossible(altfmt, mesg, fmt_ptr((genericptr_t) obj), where_name(obj),
              objnm, fmt_ptr((genericptr_t) mon), monnm);
    } else {
        impossible(fmt, mesg, fmt_ptr((genericptr_t) obj), where_name(obj), objnm);
    }
}

/* obj sanity check: check objects inside container */
STATIC_OVL void
check_contained(container, mesg)
struct obj *container;
const char *mesg;
{
    struct obj *obj;
    /* big enough to work with, not too big to blow out stack in recursion */
    char mesgbuf[40], nestedmesg[120];

    if (!Has_contents(container))
        return;
    /* change "invent sanity" to "contained invent sanity"
       but leave "nested contained invent sanity" as is */
    if (!strstri(mesg, "contained"))
        mesg = strcat(strcpy(mesgbuf, "contained "), mesg);

    for (obj = container->cobj; obj; obj = obj->nobj) {
        /* catch direct cycle to avoid unbounded recursion */
        if (obj == container)
        {
            panic("failed sanity check: container holds itself");
            return;
        }
        if (obj->where != OBJ_CONTAINED)
            insane_object(obj, "%s obj %s %s: %s", mesg, (struct monst *) 0);
        else if (obj->ocontainer != container)
            impossible("%s obj %s in container %s, not %s", mesg,
                  fmt_ptr((genericptr_t) obj),
                  fmt_ptr((genericptr_t) obj->ocontainer),
                  fmt_ptr((genericptr_t) container));

        if (Has_contents(obj)) {
            /* catch most likely indirect cycle; we won't notice if
               parent is present when something comes before it, or
               notice more deeply embedded cycles (grandparent, &c) */
            if (obj->cobj == container)
            {
                panic("failed sanity check: container holds its parent");
                return;
            }
            /* change "contained... sanity" to "nested contained... sanity"
               and "nested contained..." to "nested nested contained..." */
            Strcpy(nestedmesg, "nested ");
            copynchars(eos(nestedmesg), mesg, (int) sizeof nestedmesg
                                                  - (int) strlen(nestedmesg)
                                                  - 1);
            /* recursively check contents */
            check_contained(obj, nestedmesg);
        }
    }
}

/* check an object in hero's or monster's inventory which has worn mask set */
STATIC_OVL void
sanity_check_worn(obj)
struct obj *obj;
{
#if defined(BETA) || defined(DEBUG)
    static unsigned long wearbits[] = {
        W_ARM,    W_ARMC,   W_ARMH,    W_ARMS,     W_ARMG, W_ARMF,  W_ARMU,  W_ARMO,      W_ARMB,
        W_WEP,    W_QUIVER, W_SWAPWEP, W_SWAPWEP2, W_AMUL, W_RINGL, W_RINGR, W_BLINDFOLD,
        W_MISC,   W_MISC2,  W_MISC3,   W_MISC4,    W_MISC5,
        W_SADDLE, W_BALL,   W_CHAIN,   0
        /* [W_ARTIFACT_CARRIED,W_ARTIFACT_INVOKED are property bits for items which aren't worn] */
    };
    char maskbuf[60];
    const char *what;
    unsigned long owornmask, allmask = 0L;
    boolean embedded = FALSE;
    int i, n = 0;

    /* use owornmask for testing and bit twiddling, but use original
       obj->owornmask for printing */
    owornmask = obj->owornmask;
    /* figure out how many bits are set, and also which are viable */
    for (i = 0; wearbits[i]; ++i) {
        if ((owornmask & wearbits[i]) != 0L)
            ++n;
        allmask |= wearbits[i];
    }
    if (obj == uskin) {
        /* embedded dragon scales have an extra bit set;
           make sure it's set, then suppress it */
        embedded = TRUE;
        if ((owornmask & (W_ARM | I_SPECIAL)) == (W_ARM | I_SPECIAL))
            owornmask &= ~I_SPECIAL;
        else
            n = 0,  owornmask = ~0; /* force insane_object("bogus") below */
    }
    if (n == 2 && carried(obj)
        && obj == uball && (owornmask & W_BALL) != 0L
        && (owornmask & W_WEAPON) != 0L) {
        /* chained ball can be wielded/alt-wielded/quivered; if so,
          pretend it's not chained in order to check the weapon pointer
          (we've already verified the ball pointer by successfully passing
          the if-condition to get here...) */
        owornmask &= ~W_BALL;
        n = 1;
    }
    if (n > 1) {
        /* multiple bits set */
        Sprintf(maskbuf, "worn mask (multiple) 0x%08lx", obj->owornmask);
        insane_object(obj, ofmt0, maskbuf, (struct monst *) 0);
    }
    if ((owornmask & ~allmask) != 0L
        || (carried(obj) && (owornmask & W_SADDLE) != 0L)) {
        /* non-wearable bit(s) set */
        Sprintf(maskbuf, "worn mask (bogus)) 0x%08lx", obj->owornmask);
        insane_object(obj, ofmt0, maskbuf, (struct monst *) 0);
    }
    if (n == 1 && (carried(obj) || (owornmask & (W_BALL | W_CHAIN)) != 0L)) {
        what = 0;
        /* verify that obj in hero's invent (or ball/chain elsewhere)
           with owornmask of W_foo is the object pointed to by ufoo */
        switch (owornmask) {
        case W_ARM:
            if (obj != (embedded ? uskin : uarm))
                what = embedded ? "skin" : "suit";
            break;
        case W_ARMC:
            if (obj != uarmc)
                what = "cloak";
            break;
        case W_ARMO:
            if (obj != uarmo)
                what = "robe";
            break;
        case W_ARMH:
            if (obj != uarmh)
                what = "helm";
            break;
        case W_ARMS:
            if (obj != uarms)
            {
                if(is_shield(obj))
                    what = "shield";
                else
                    what = u.twoweap ? "left hand weapon" : "secondary weapon";
            }
            break;
        case W_ARMG:
            if (obj != uarmg)
                what = "gloves";
            break;
        case W_ARMB:
            if (obj != uarmb)
                what = "bracers";
            break;
        case W_MISC:
            if (obj != umisc)
                what = "miscellaneous item";
            break;
        case W_MISC2:
            if (obj != umisc2)
                what = "secondary miscellaneous item";
            break;
        case W_MISC3:
            if (obj != umisc3)
                what = "tertiary miscellaneous item";
            break;
        case W_MISC4:
            if (obj != umisc4)
                what = "quaternary miscellaneous item";
            break;
        case W_MISC5:
            if (obj != umisc5)
                what = "quinary miscellaneous item";
            break;
        case W_ARMF:
            if (obj != uarmf)
                what = "boots";
            break;
        case W_ARMU:
            if (obj != uarmu)
                what = "shirt";
            break;
        case W_WEP:
            if (obj != uwep)
                what = u.twoweap ? "right hand weapon" : "primary weapon";
            break;
        case W_QUIVER:
            if (obj != uquiver)
                what = "quiver";
            break;
        case W_SWAPWEP:
            if (obj != uswapwep)
                what = u.twoweap ? "right hand alternate weapon" : "alternate weapon";
            break;
        case W_SWAPWEP2:
            if (obj != uswapwep2)
                what = u.twoweap ? "left hand alternate weapon" : "another alternate weapon";
            break;
        case W_AMUL:
            if (obj != uamul)
                what = "amulet";
            break;
        case W_RINGL:
            if (obj != uleft)
                what = "left ring";
            break;
        case W_RINGR:
            if (obj != uright)
                what = "right ring";
            break;
        case W_BLINDFOLD:
            if (obj != ublindf)
                what = "blindfold";
            break;
        /* case W_SADDLE: */
        case W_BALL:
            if (obj != uball)
                what = "ball";
            break;
        case W_CHAIN:
            if (obj != uchain)
                what = "chain";
            break;
        default:
            break;
        }
        if (what) {
            Sprintf(maskbuf, "worn mask 0x%08lx != %s", obj->owornmask, what);
            insane_object(obj, ofmt0, maskbuf, (struct monst *) 0);
        }
    }
    if (n == 1 && (carried(obj) || (owornmask & (W_BALL | W_CHAIN)) != 0L
                   || mcarried(obj))) {
        /* check for items worn in invalid slots; practically anything can
           be wielded/alt-wielded/quivered, so tests on those are limited */
        what = 0;
        if (owornmask & (W_ARMOR & ~W_ARMS)) {
            if (obj->oclass != ARMOR_CLASS)
                what = "armor";
            /* 3.6: dragon scale mail reverts to dragon scales when
               becoming embedded in poly'd hero's skin */
            if (embedded && !is_dragon_scales(obj))
                what = "skin";
        } else if (owornmask & W_WEAPON) {
            /* monsters don't maintain alternate weapon or quiver */
            if (mcarried(obj) && (owornmask & (W_SWAP_WEAPON | W_QUIVER)) != 0L)
                what = (owornmask & W_SWAP_WEAPON) != 0L ? "monst alt weapon?"
                                                     : "monst quiver?";
            /* hero can quiver gold but not wield it (hence not alt-wield
               it either); also catches monster wielding gold */
            else if (obj->oclass == COIN_CLASS
                     && (owornmask & (W_WIELDED_WEAPON | W_SWAP_WEAPON)) != 0L)
                what = (owornmask & W_WIELDED_WEAPON) != 0L ? "weapon" : "alt weapon";
        } else if (owornmask & W_AMUL) {
            if (obj->oclass != AMULET_CLASS)
                what = "amulet";
        } else if (owornmask & W_MISCITEMS) {
            if (obj->oclass != MISCELLANEOUS_CLASS)
                what = "miscellaneous item";
        } else if (owornmask & W_RING) {
            if (obj->oclass != RING_CLASS && obj->otyp != MEAT_RING)
                what = "ring";
        } else if (owornmask & W_BLINDFOLD) {
            if (obj->otyp != BLINDFOLD && obj->otyp != TOWEL
                )
                what = "blindfold";
        } else if (owornmask & W_BALL) {
            if (obj->oclass != BALL_CLASS)
                what = "chained ball";
        } else if (owornmask & W_CHAIN) {
            if (obj->oclass != CHAIN_CLASS)
                what = "chain";
        } else if (owornmask & W_SADDLE) {
            if (obj->otyp != SADDLE)
                what = "saddle";
        }
        if (what) {
            char oclassname[30];
            struct monst *mon = mcarried(obj) ? obj->ocarry : 0;

            /* if we've found a potion worn in the amulet slot,
               this yields "worn (potion amulet)" */
            Strcpy(oclassname, def_oc_syms[(uchar) obj->oclass].name);
            Sprintf(maskbuf, "worn (%s %s)", makesingular(oclassname), what);
            insane_object(obj, ofmt0, maskbuf, mon);
        }
    }
#else /* not (BETA || DEBUG) */
    /* dummy use of obj to avoid "arg not used" complaint */
    if (!obj)
        insane_object(obj, ofmt0, "<null>", (struct monst *) 0);
#endif
}

/*
 * wrapper to make "near this object" convenient
 */
struct obj *
obj_nexto(otmp)
struct obj *otmp;
{
    if (!otmp) {
        impossible("obj_nexto: wasn't given an object to check");
        return (struct obj *) 0;
    }
    return obj_nexto_xy(otmp, otmp->ox, otmp->oy, TRUE);
}

/*
 * looks for objects of a particular type next to x, y
 * skips over oid if found (lets us avoid ourselves if
 * we're looking for a second type of an existing object)
 *
 * TODO: return a list of all objects near us so we can more
 * reliably predict which one we want to 'find' first
 */
struct obj *
obj_nexto_xy(obj, x, y, recurs)
struct obj *obj;
int x, y;
boolean recurs;
{
    struct obj *otmp;
    int fx, fy, ex, ey, otyp = obj->otyp;
    short dx, dy;

    /* check under our "feet" first */
    otmp = sobj_at(otyp, x, y);
    while (otmp) {
        /* don't be clever and find ourselves */
        if (otmp != obj && mergable(otmp, obj))
            return otmp;
        otmp = nxtobj(otmp, otyp, TRUE);
    }

    if (!recurs)
        return (struct obj *) 0;

    /* search in a random order */
    dx = (rn2(2) ? -1 : 1);
    dy = (rn2(2) ? -1 : 1);
    ex = x - dx;
    ey = y - dy;

    for (fx = ex; abs(fx - ex) < 3; fx += dx) {
        for (fy = ey; abs(fy - ey) < 3; fy += dy) {
            /* 0, 0 was checked above */
            if (isok(fx, fy) && (fx != x || fy != y)) {
                if ((otmp = obj_nexto_xy(obj, fx, fy, FALSE)) != 0)
                    return otmp;
            }
        }
    }
    return (struct obj *) 0;
}

/*
 * Causes one object to absorb another, increasing
 * weight accordingly. Frees obj2; obj1 remains and
 * is returned.
 */
struct obj *
obj_absorb(obj1, obj2)
struct obj **obj1, **obj2;
{
    struct obj *otmp1, *otmp2;
    int o1wt, o2wt;
    long agetmp;

    /* don't let people dumb it up */
    if (obj1 && obj2) {
        otmp1 = *obj1;
        otmp2 = *obj2;
        if (otmp1 && otmp2 && otmp1 != otmp2) {
            if (otmp1->bknown != otmp2->bknown)
                otmp1->bknown = otmp2->bknown = 0;
            if (otmp1->rknown != otmp2->rknown)
                otmp1->rknown = otmp2->rknown = 0;
            if (otmp1->greased != otmp2->greased)
                otmp1->greased = otmp2->greased = 0;
            if (otmp1->orotten || otmp2->orotten)
                otmp1->orotten = otmp2->orotten = 1;
            o1wt = otmp1->oeaten ? otmp1->oeaten : otmp1->owt;
            o2wt = otmp2->oeaten ? otmp2->oeaten : otmp2->owt;
            /* averaging the relative ages is less likely to overflow
               than averaging the absolute ages directly */
            agetmp = (((moves - otmp1->age) * o1wt
                       + (moves - otmp2->age) * o2wt)
                      / (o1wt + o2wt));
            otmp1->age = moves - agetmp; /* conv. relative back to absolute */
            otmp1->owt += o2wt;
            if (otmp1->oeaten)
                otmp1->oeaten += o2wt;
            otmp1->quan = 1L;
            obj_extract_self(otmp2);
            newsym(otmp2->ox, otmp2->oy); /* in case of floor */
            dealloc_obj(otmp2);
            *obj2 = (struct obj *) 0;
            return otmp1;
        }
    }

    impossible("obj_absorb: not called with two actual objects");
    return (struct obj *) 0;
}

/*
 * Causes the heavier object to absorb the lighter object;
 * wrapper for obj_absorb so that floor_effects works more
 * cleanly (since we don't know which we want to stay around)
 */
struct obj *
obj_meld(obj1, obj2)
struct obj **obj1, **obj2;
{
    struct obj *otmp1, *otmp2;

    if (obj1 && obj2) {
        otmp1 = *obj1;
        otmp2 = *obj2;
        if (otmp1 && otmp2 && otmp1 != otmp2) {
            if (otmp1->owt > otmp2->owt
                || (otmp1->owt == otmp2->owt && rn2(2))) {
                return obj_absorb(obj1, obj2);
            }
            return obj_absorb(obj2, obj1);
        }
    }

    impossible("obj_meld: not called with two actual objects");
    return (struct obj *) 0;
}

/* give a message if hero notices two globs merging [used to be in pline.c] */
void
pudding_merge_message(otmp, otmp2)
struct obj *otmp;
struct obj *otmp2;
{
    boolean visible = (cansee(otmp->ox, otmp->oy)
                       || cansee(otmp2->ox, otmp2->oy)),
            onfloor = (otmp->where == OBJ_FLOOR || otmp2->where == OBJ_FLOOR),
            inpack = (carried(otmp) || carried(otmp2));

    /* the player will know something happened inside his own inventory */
    play_simple_object_sound(otmp, OBJECT_SOUND_TYPE_GENERAL_EFFECT);
    if ((!Blind && visible) || inpack) 
    {
        if (Hallucination) 
        {
            if (onfloor)
            {
                You_see("parts of the floor melting!");
            } 
            else if (inpack) 
            {
                Your("pack reaches out and grabs something!");
            }
            /* even though we can see where they should be,
             * they'll be out of our view (minvent or container)
             * so don't actually show anything */
        } 
        else if (onfloor || inpack) 
        {
            pline("The %s coalesce%s.", makeplural(obj_typename(otmp->otyp)),
                  inpack ? " inside your pack" : "");
        }
    } 
    else 
    {
        You_hear("a faint sloshing sound.");
    }
}

/*mkobj.c*/
