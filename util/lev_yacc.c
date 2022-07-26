/* A Bison parser, made by GNU Bison 3.5.1.  */

/* Bison implementation for Yacc-like parsers in C

   Copyright (C) 1984, 1989-1990, 2000-2015, 2018-2020 Free Software Foundation,
   Inc.

   This program is free software: you can redistribute it and/or modify
   it under the terms of the GNU General Public License as published by
   the Free Software Foundation, either version 3 of the License, or
   (at your option) any later version.

   This program is distributed in the hope that it will be useful,
   but WITHOUT ANY WARRANTY; without even the implied warranty of
   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
   GNU General Public License for more details.

   You should have received a copy of the GNU General Public License
   along with this program.  If not, see <http://www.gnu.org/licenses/>.  */

/* As a special exception, you may create a larger work that contains
   part or all of the Bison parser skeleton and distribute that work
   under terms of your choice, so long as that work isn't itself a
   parser generator using the skeleton or a modified version thereof
   as a parser skeleton.  Alternatively, if you modify or redistribute
   the parser skeleton itself, you may (at your option) remove this
   special exception, which will cause the skeleton and the resulting
   Bison output files to be licensed under the GNU General Public
   License without this special exception.

   This special exception was added by the Free Software Foundation in
   version 2.2 of Bison.  */

/* C LALR(1) parser skeleton written by Richard Stallman, by
   simplifying the original so-called "semantic" parser.  */

/* All symbols defined below should begin with yy or YY, to avoid
   infringing on user name space.  This should be done even for local
   variables, as they might otherwise be expanded by user macros.
   There are some unavoidable exceptions within include files to
   define necessary library symbols; they are noted "INFRINGES ON
   USER NAME SPACE" below.  */

/* Undocumented macros, especially those whose name start with YY_,
   are private implementation details.  Do not rely on them.  */

/* Identify Bison output.  */
#define YYBISON 1

/* Bison version.  */
#define YYBISON_VERSION "3.5.1"

/* Skeleton name.  */
#define YYSKELETON_NAME "yacc.c"

/* Pure parsers.  */
#define YYPURE 0

/* Push parsers.  */
#define YYPUSH 0

/* Pull parsers.  */
#define YYPULL 1




/* First part of user prologue.  */
#line 1 "lev_comp.y"

/* GnollHack File Change Notice: This file has been changed from the original. Date of last change: 2022-04-16 */

/* GnollHack 4.0  lev_comp.y	$NHDT-Date: 1543371691 2018/11/28 02:21:31 $  $NHDT-Branch: NetHack-3.6.2-beta01 $:$NHDT-Revision: 1.22 $ */
/*      Copyright (c) 1989 by Jean-Christophe Collet */
/* GnollHack may be freely redistributed.  See license for details. */

/*
 * This file contains the Level Compiler code
 * It may handle special mazes & special room-levels
 */

/* In case we're using bison in AIX.  This definition must be
 * placed before any other C-language construct in the file
 * excluding comments and preprocessor directives (thanks IBM
 * for this wonderful feature...).
 *
 * Note: some cpps barf on this 'undefined control' (#pragma).
 * Addition of the leading space seems to prevent barfage for now,
 * and AIX will still see the directive.
 */
#ifdef _AIX
 #pragma alloca         /* keep leading space! */
#endif

#define SPEC_LEV    /* for USE_OLDARGS (sp_lev.h) */
#include "hack.h"
#include "sp_lev.h"

#define ERR             (-1)
/* many types of things are put in chars for transference to NetHack.
 * since some systems will use signed chars, limit everybody to the
 * same number for portability.
 */
#define MAX_OF_TYPE     128

#define MAX_NESTED_IFS   20
#define MAX_SWITCH_CASES 20

#define New(type) \
        (type *) memset((genericptr_t) alloc(sizeof (type)), 0, sizeof (type))
#define NewTab(type, size)      (type **) alloc(sizeof (type *) * size)
#define Free(ptr)               free((genericptr_t) ptr)

extern void VDECL(lc_error, (const char *, ...));
extern void VDECL(lc_warning, (const char *, ...));
extern void FDECL(yyerror, (const char *));
extern void FDECL(yywarning, (const char *));
extern int NDECL(yylex);
int NDECL(yyparse);

extern int FDECL(get_floor_type, (CHAR_P));
extern int FDECL(get_room_type, (char *));
extern int FDECL(get_trap_type, (char *));
extern int FDECL(get_monster_id, (char *,CHAR_P));
extern int FDECL(get_object_id, (char *,CHAR_P));
extern boolean FDECL(check_monster_char, (CHAR_P));
extern boolean FDECL(check_object_char, (CHAR_P));
extern char FDECL(what_map_char, (CHAR_P));
extern void FDECL(scan_map, (char *, sp_lev *));
extern void FDECL(add_opcode, (sp_lev *, int, genericptr_t));
extern genericptr_t FDECL(get_last_opcode_data1, (sp_lev *, int));
extern genericptr_t FDECL(get_last_opcode_data2, (sp_lev *, int, int));
extern boolean FDECL(check_subrooms, (sp_lev *));
extern boolean FDECL(write_level_file, (char *,sp_lev *));
extern struct opvar *FDECL(set_opvar_int, (struct opvar *, long));
extern void VDECL(add_opvars, (sp_lev *, const char *, ...));
extern void FDECL(start_level_def, (sp_lev * *, char *));

extern struct lc_funcdefs *FDECL(funcdef_new, (long,char *));
extern void FDECL(funcdef_free_all, (struct lc_funcdefs *));
extern struct lc_funcdefs *FDECL(funcdef_defined, (struct lc_funcdefs *,
                                                   char *, int));
extern char *FDECL(funcdef_paramtypes, (struct lc_funcdefs *));
extern char *FDECL(decode_parm_str, (char *));

extern struct lc_vardefs *FDECL(vardef_new, (long,char *));
extern void FDECL(vardef_free_all, (struct lc_vardefs *));
extern struct lc_vardefs *FDECL(vardef_defined, (struct lc_vardefs *,
                                                 char *, int));

extern void NDECL(break_stmt_start);
extern void FDECL(break_stmt_end, (sp_lev *));
extern void FDECL(break_stmt_new, (sp_lev *, long));

extern void FDECL(splev_add_from, (sp_lev *, sp_lev *));

extern void FDECL(check_vardef_type, (struct lc_vardefs *, char *, long));
extern void FDECL(vardef_used, (struct lc_vardefs *, char *));
extern struct lc_vardefs *FDECL(add_vardef_type, (struct lc_vardefs *,
                                                  char *, long));

extern int FDECL(reverse_jmp_opcode, (int));

struct coord {
    long x;
    long y;
};

struct forloopdef {
    char *varname;
    long jmp_point;
};
static struct forloopdef forloop_list[MAX_NESTED_IFS];
static short n_forloops = 0;


sp_lev *splev = NULL;

static struct opvar *if_list[MAX_NESTED_IFS];

static short n_if_list = 0;

unsigned int max_x_map, max_y_map;
int obj_containment = 0;

int in_container_obj = 0;

/* integer value is possibly an inconstant value (eg. dice notation
   or a variable) */
int is_inconstant_number = 0;

int in_switch_statement = 0;
static struct opvar *switch_check_jump = NULL;
static struct opvar *switch_default_case = NULL;
static struct opvar *switch_case_list[MAX_SWITCH_CASES];
static long switch_case_value[MAX_SWITCH_CASES];
int n_switch_case_list = 0;

int allow_break_statements = 0;
struct lc_breakdef *break_list = NULL;

extern struct lc_vardefs *vardefs; /* variable definitions */


struct lc_vardefs *function_tmp_var_defs = NULL;
extern struct lc_funcdefs *function_definitions;
struct lc_funcdefs *curr_function = NULL;
struct lc_funcdefs_parm * curr_function_param = NULL;
int in_function_definition = 0;
sp_lev *function_splev_backup = NULL;

extern int fatal_error;
extern int got_errors;
extern int line_number;
extern const char *fname;

extern char curr_token[512];


#line 221 "lev_yacc.c"

# ifndef YY_CAST
#  ifdef __cplusplus
#   define YY_CAST(Type, Val) static_cast<Type> (Val)
#   define YY_REINTERPRET_CAST(Type, Val) reinterpret_cast<Type> (Val)
#  else
#   define YY_CAST(Type, Val) ((Type) (Val))
#   define YY_REINTERPRET_CAST(Type, Val) ((Type) (Val))
#  endif
# endif
# ifndef YY_NULLPTR
#  if defined __cplusplus
#   if 201103L <= __cplusplus
#    define YY_NULLPTR nullptr
#   else
#    define YY_NULLPTR 0
#   endif
#  else
#   define YY_NULLPTR ((void*)0)
#  endif
# endif

/* Enabling verbose error messages.  */
#ifdef YYERROR_VERBOSE
# undef YYERROR_VERBOSE
# define YYERROR_VERBOSE 1
#else
# define YYERROR_VERBOSE 0
#endif

/* Use api.header.include to #include this header
   instead of duplicating it here.  */
#ifndef YY_YY_Y_TAB_H_INCLUDED
# define YY_YY_Y_TAB_H_INCLUDED
/* Debug traces.  */
#ifndef YYDEBUG
# define YYDEBUG 0
#endif
#if YYDEBUG
extern int yydebug;
#endif

/* Token type.  */
#ifndef YYTOKENTYPE
# define YYTOKENTYPE
  enum yytokentype
  {
    CHAR = 258,
    INTEGER = 259,
    BOOLEAN = 260,
    PERCENT = 261,
    SPERCENT = 262,
    MINUS_INTEGER = 263,
    PLUS_INTEGER = 264,
    MAZE_GRID_ID = 265,
    SOLID_FILL_ID = 266,
    MINES_ID = 267,
    ROGUELEV_ID = 268,
    MESSAGE_ID = 269,
    MAZE_ID = 270,
    LEVEL_ID = 271,
    LEV_INIT_ID = 272,
    TILESET_ID = 273,
    GEOMETRY_ID = 274,
    NOMAP_ID = 275,
    BOUNDARY_TYPE_ID = 276,
    SPECIAL_TILESET_ID = 277,
    OBJECT_ID = 278,
    COBJECT_ID = 279,
    MONSTER_ID = 280,
    TRAP_ID = 281,
    DOOR_ID = 282,
    DRAWBRIDGE_ID = 283,
    MONSTER_GENERATION_ID = 284,
    object_ID = 285,
    monster_ID = 286,
    terrain_ID = 287,
    MAZEWALK_ID = 288,
    WALLIFY_ID = 289,
    REGION_ID = 290,
    SPECIAL_REGION_ID = 291,
    SPECIAL_LEVREGION_ID = 292,
    SPECIAL_REGION_TYPE = 293,
    NAMING_ID = 294,
    NAMING_TYPE = 295,
    FILLING = 296,
    IRREGULAR = 297,
    JOINED = 298,
    ALTAR_ID = 299,
    ANVIL_ID = 300,
    NPC_ID = 301,
    LADDER_ID = 302,
    STAIR_ID = 303,
    NON_DIGGABLE_ID = 304,
    NON_PASSWALL_ID = 305,
    ROOM_ID = 306,
    ARTIFACT_NAME_ID = 307,
    PORTAL_ID = 308,
    TELEPRT_ID = 309,
    BRANCH_ID = 310,
    LEV = 311,
    MINERALIZE_ID = 312,
    AGE_ID = 313,
    CORRIDOR_ID = 314,
    GOLD_ID = 315,
    ENGRAVING_ID = 316,
    FOUNTAIN_ID = 317,
    THRONE_ID = 318,
    MODRON_PORTAL_ID = 319,
    LEVEL_TELEPORTER_ID = 320,
    LEVEL_TELEPORT_DIRECTION_TYPE = 321,
    LEVEL_TELEPORT_END_TYPE = 322,
    POOL_ID = 323,
    SINK_ID = 324,
    NONE = 325,
    RAND_CORRIDOR_ID = 326,
    DOOR_STATE = 327,
    LIGHT_STATE = 328,
    CURSE_TYPE = 329,
    MYTHIC_TYPE = 330,
    ENGRAVING_TYPE = 331,
    KEYTYPE_ID = 332,
    LEVER_ID = 333,
    NO_PICKUP_ID = 334,
    DIRECTION = 335,
    RANDOM_TYPE = 336,
    RANDOM_TYPE_BRACKET = 337,
    A_REGISTER = 338,
    ALIGNMENT = 339,
    LEFT_OR_RIGHT = 340,
    CENTER = 341,
    TOP_OR_BOT = 342,
    ALTAR_TYPE = 343,
    ALTAR_SUBTYPE = 344,
    UP_OR_DOWN = 345,
    ACTIVE_OR_INACTIVE = 346,
    MODRON_PORTAL_TYPE = 347,
    NPC_TYPE = 348,
    FOUNTAIN_TYPE = 349,
    SPECIAL_OBJECT_TYPE = 350,
    CMAP_TYPE = 351,
    FLOOR_SUBTYPE = 352,
    FLOOR_SUBTYPE_ID = 353,
    FLOOR_ID = 354,
    FLOOR_TYPE = 355,
    FLOOR_TYPE_ID = 356,
    ELEMENTAL_ENCHANTMENT_TYPE = 357,
    EXCEPTIONALITY_TYPE = 358,
    EXCEPTIONALITY_ID = 359,
    ELEMENTAL_ENCHANTMENT_ID = 360,
    ENCHANTMENT_ID = 361,
    SECRET_DOOR_ID = 362,
    USES_UP_KEY_ID = 363,
    MYTHIC_PREFIX_TYPE = 364,
    MYTHIC_SUFFIX_TYPE = 365,
    MYTHIC_PREFIX_ID = 366,
    MYTHIC_SUFFIX_ID = 367,
    CHARGES_ID = 368,
    SPECIAL_QUALITY_ID = 369,
    SPEFLAGS_ID = 370,
    SUBROOM_ID = 371,
    NAME_ID = 372,
    FLAGS_ID = 373,
    FLAG_TYPE = 374,
    MON_ATTITUDE = 375,
    MON_ALERTNESS = 376,
    SUBTYPE_ID = 377,
    NON_PASSDOOR_ID = 378,
    MON_APPEARANCE = 379,
    ROOMDOOR_ID = 380,
    IF_ID = 381,
    ELSE_ID = 382,
    TERRAIN_ID = 383,
    HORIZ_OR_VERT = 384,
    REPLACE_TERRAIN_ID = 385,
    LOCATION_SUBTYPE_ID = 386,
    DOOR_SUBTYPE = 387,
    BRAZIER_SUBTYPE = 388,
    SIGNPOST_SUBTYPE = 389,
    TREE_SUBTYPE = 390,
    FOREST_ID = 391,
    FOREST_TYPE = 392,
    INITIALIZE_TYPE = 393,
    EXIT_ID = 394,
    SHUFFLE_ID = 395,
    MANUAL_TYPE_ID = 396,
    MANUAL_TYPE = 397,
    QUANTITY_ID = 398,
    BURIED_ID = 399,
    LOOP_ID = 400,
    FOR_ID = 401,
    TO_ID = 402,
    SWITCH_ID = 403,
    CASE_ID = 404,
    BREAK_ID = 405,
    DEFAULT_ID = 406,
    ERODED_ID = 407,
    TRAPPED_STATE = 408,
    RECHARGED_ID = 409,
    INVIS_ID = 410,
    GREASED_ID = 411,
    INDESTRUCTIBLE_ID = 412,
    FEMALE_ID = 413,
    MALE_ID = 414,
    WAITFORU_ID = 415,
    PROTECTOR_ID = 416,
    CANCELLED_ID = 417,
    REVIVED_ID = 418,
    AVENGE_ID = 419,
    FLEEING_ID = 420,
    BLINDED_ID = 421,
    PARALYZED_ID = 422,
    STUNNED_ID = 423,
    CONFUSED_ID = 424,
    SEENTRAPS_ID = 425,
    ALL_ID = 426,
    MONTYPE_ID = 427,
    OBJTYPE_ID = 428,
    TERTYPE_ID = 429,
    TERTYPE2_ID = 430,
    LEVER_EFFECT_TYPE = 431,
    SWITCHABLE_ID = 432,
    CONTINUOUSLY_USABLE_ID = 433,
    TARGET_ID = 434,
    TRAPTYPE_ID = 435,
    EFFECT_FLAG_ID = 436,
    GRAVE_ID = 437,
    BRAZIER_ID = 438,
    SIGNPOST_ID = 439,
    TREE_ID = 440,
    ERODEPROOF_ID = 441,
    FUNCTION_ID = 442,
    MSG_OUTPUT_TYPE = 443,
    COMPARE_TYPE = 444,
    UNKNOWN_TYPE = 445,
    rect_ID = 446,
    fillrect_ID = 447,
    line_ID = 448,
    randline_ID = 449,
    grow_ID = 450,
    selection_ID = 451,
    flood_ID = 452,
    rndcoord_ID = 453,
    circle_ID = 454,
    ellipse_ID = 455,
    filter_ID = 456,
    complement_ID = 457,
    gradient_ID = 458,
    GRADIENT_TYPE = 459,
    LIMITED = 460,
    HUMIDITY_TYPE = 461,
    STRING = 462,
    MAP_ID = 463,
    NQSTRING = 464,
    VARSTRING = 465,
    CFUNC = 466,
    CFUNC_INT = 467,
    CFUNC_STR = 468,
    CFUNC_COORD = 469,
    CFUNC_REGION = 470,
    VARSTRING_INT = 471,
    VARSTRING_INT_ARRAY = 472,
    VARSTRING_STRING = 473,
    VARSTRING_STRING_ARRAY = 474,
    VARSTRING_VAR = 475,
    VARSTRING_VAR_ARRAY = 476,
    VARSTRING_COORD = 477,
    VARSTRING_COORD_ARRAY = 478,
    VARSTRING_REGION = 479,
    VARSTRING_REGION_ARRAY = 480,
    VARSTRING_MAPCHAR = 481,
    VARSTRING_MAPCHAR_ARRAY = 482,
    VARSTRING_MONST = 483,
    VARSTRING_MONST_ARRAY = 484,
    VARSTRING_OBJ = 485,
    VARSTRING_OBJ_ARRAY = 486,
    VARSTRING_SEL = 487,
    VARSTRING_SEL_ARRAY = 488,
    METHOD_INT = 489,
    METHOD_INT_ARRAY = 490,
    METHOD_STRING = 491,
    METHOD_STRING_ARRAY = 492,
    METHOD_VAR = 493,
    METHOD_VAR_ARRAY = 494,
    METHOD_COORD = 495,
    METHOD_COORD_ARRAY = 496,
    METHOD_REGION = 497,
    METHOD_REGION_ARRAY = 498,
    METHOD_MAPCHAR = 499,
    METHOD_MAPCHAR_ARRAY = 500,
    METHOD_MONST = 501,
    METHOD_MONST_ARRAY = 502,
    METHOD_OBJ = 503,
    METHOD_OBJ_ARRAY = 504,
    METHOD_SEL = 505,
    METHOD_SEL_ARRAY = 506,
    DICE = 507
  };
#endif
/* Tokens.  */
#define CHAR 258
#define INTEGER 259
#define BOOLEAN 260
#define PERCENT 261
#define SPERCENT 262
#define MINUS_INTEGER 263
#define PLUS_INTEGER 264
#define MAZE_GRID_ID 265
#define SOLID_FILL_ID 266
#define MINES_ID 267
#define ROGUELEV_ID 268
#define MESSAGE_ID 269
#define MAZE_ID 270
#define LEVEL_ID 271
#define LEV_INIT_ID 272
#define TILESET_ID 273
#define GEOMETRY_ID 274
#define NOMAP_ID 275
#define BOUNDARY_TYPE_ID 276
#define SPECIAL_TILESET_ID 277
#define OBJECT_ID 278
#define COBJECT_ID 279
#define MONSTER_ID 280
#define TRAP_ID 281
#define DOOR_ID 282
#define DRAWBRIDGE_ID 283
#define MONSTER_GENERATION_ID 284
#define object_ID 285
#define monster_ID 286
#define terrain_ID 287
#define MAZEWALK_ID 288
#define WALLIFY_ID 289
#define REGION_ID 290
#define SPECIAL_REGION_ID 291
#define SPECIAL_LEVREGION_ID 292
#define SPECIAL_REGION_TYPE 293
#define NAMING_ID 294
#define NAMING_TYPE 295
#define FILLING 296
#define IRREGULAR 297
#define JOINED 298
#define ALTAR_ID 299
#define ANVIL_ID 300
#define NPC_ID 301
#define LADDER_ID 302
#define STAIR_ID 303
#define NON_DIGGABLE_ID 304
#define NON_PASSWALL_ID 305
#define ROOM_ID 306
#define ARTIFACT_NAME_ID 307
#define PORTAL_ID 308
#define TELEPRT_ID 309
#define BRANCH_ID 310
#define LEV 311
#define MINERALIZE_ID 312
#define AGE_ID 313
#define CORRIDOR_ID 314
#define GOLD_ID 315
#define ENGRAVING_ID 316
#define FOUNTAIN_ID 317
#define THRONE_ID 318
#define MODRON_PORTAL_ID 319
#define LEVEL_TELEPORTER_ID 320
#define LEVEL_TELEPORT_DIRECTION_TYPE 321
#define LEVEL_TELEPORT_END_TYPE 322
#define POOL_ID 323
#define SINK_ID 324
#define NONE 325
#define RAND_CORRIDOR_ID 326
#define DOOR_STATE 327
#define LIGHT_STATE 328
#define CURSE_TYPE 329
#define MYTHIC_TYPE 330
#define ENGRAVING_TYPE 331
#define KEYTYPE_ID 332
#define LEVER_ID 333
#define NO_PICKUP_ID 334
#define DIRECTION 335
#define RANDOM_TYPE 336
#define RANDOM_TYPE_BRACKET 337
#define A_REGISTER 338
#define ALIGNMENT 339
#define LEFT_OR_RIGHT 340
#define CENTER 341
#define TOP_OR_BOT 342
#define ALTAR_TYPE 343
#define ALTAR_SUBTYPE 344
#define UP_OR_DOWN 345
#define ACTIVE_OR_INACTIVE 346
#define MODRON_PORTAL_TYPE 347
#define NPC_TYPE 348
#define FOUNTAIN_TYPE 349
#define SPECIAL_OBJECT_TYPE 350
#define CMAP_TYPE 351
#define FLOOR_SUBTYPE 352
#define FLOOR_SUBTYPE_ID 353
#define FLOOR_ID 354
#define FLOOR_TYPE 355
#define FLOOR_TYPE_ID 356
#define ELEMENTAL_ENCHANTMENT_TYPE 357
#define EXCEPTIONALITY_TYPE 358
#define EXCEPTIONALITY_ID 359
#define ELEMENTAL_ENCHANTMENT_ID 360
#define ENCHANTMENT_ID 361
#define SECRET_DOOR_ID 362
#define USES_UP_KEY_ID 363
#define MYTHIC_PREFIX_TYPE 364
#define MYTHIC_SUFFIX_TYPE 365
#define MYTHIC_PREFIX_ID 366
#define MYTHIC_SUFFIX_ID 367
#define CHARGES_ID 368
#define SPECIAL_QUALITY_ID 369
#define SPEFLAGS_ID 370
#define SUBROOM_ID 371
#define NAME_ID 372
#define FLAGS_ID 373
#define FLAG_TYPE 374
#define MON_ATTITUDE 375
#define MON_ALERTNESS 376
#define SUBTYPE_ID 377
#define NON_PASSDOOR_ID 378
#define MON_APPEARANCE 379
#define ROOMDOOR_ID 380
#define IF_ID 381
#define ELSE_ID 382
#define TERRAIN_ID 383
#define HORIZ_OR_VERT 384
#define REPLACE_TERRAIN_ID 385
#define LOCATION_SUBTYPE_ID 386
#define DOOR_SUBTYPE 387
#define BRAZIER_SUBTYPE 388
#define SIGNPOST_SUBTYPE 389
#define TREE_SUBTYPE 390
#define FOREST_ID 391
#define FOREST_TYPE 392
#define INITIALIZE_TYPE 393
#define EXIT_ID 394
#define SHUFFLE_ID 395
#define MANUAL_TYPE_ID 396
#define MANUAL_TYPE 397
#define QUANTITY_ID 398
#define BURIED_ID 399
#define LOOP_ID 400
#define FOR_ID 401
#define TO_ID 402
#define SWITCH_ID 403
#define CASE_ID 404
#define BREAK_ID 405
#define DEFAULT_ID 406
#define ERODED_ID 407
#define TRAPPED_STATE 408
#define RECHARGED_ID 409
#define INVIS_ID 410
#define GREASED_ID 411
#define INDESTRUCTIBLE_ID 412
#define FEMALE_ID 413
#define MALE_ID 414
#define WAITFORU_ID 415
#define PROTECTOR_ID 416
#define CANCELLED_ID 417
#define REVIVED_ID 418
#define AVENGE_ID 419
#define FLEEING_ID 420
#define BLINDED_ID 421
#define PARALYZED_ID 422
#define STUNNED_ID 423
#define CONFUSED_ID 424
#define SEENTRAPS_ID 425
#define ALL_ID 426
#define MONTYPE_ID 427
#define OBJTYPE_ID 428
#define TERTYPE_ID 429
#define TERTYPE2_ID 430
#define LEVER_EFFECT_TYPE 431
#define SWITCHABLE_ID 432
#define CONTINUOUSLY_USABLE_ID 433
#define TARGET_ID 434
#define TRAPTYPE_ID 435
#define EFFECT_FLAG_ID 436
#define GRAVE_ID 437
#define BRAZIER_ID 438
#define SIGNPOST_ID 439
#define TREE_ID 440
#define ERODEPROOF_ID 441
#define FUNCTION_ID 442
#define MSG_OUTPUT_TYPE 443
#define COMPARE_TYPE 444
#define UNKNOWN_TYPE 445
#define rect_ID 446
#define fillrect_ID 447
#define line_ID 448
#define randline_ID 449
#define grow_ID 450
#define selection_ID 451
#define flood_ID 452
#define rndcoord_ID 453
#define circle_ID 454
#define ellipse_ID 455
#define filter_ID 456
#define complement_ID 457
#define gradient_ID 458
#define GRADIENT_TYPE 459
#define LIMITED 460
#define HUMIDITY_TYPE 461
#define STRING 462
#define MAP_ID 463
#define NQSTRING 464
#define VARSTRING 465
#define CFUNC 466
#define CFUNC_INT 467
#define CFUNC_STR 468
#define CFUNC_COORD 469
#define CFUNC_REGION 470
#define VARSTRING_INT 471
#define VARSTRING_INT_ARRAY 472
#define VARSTRING_STRING 473
#define VARSTRING_STRING_ARRAY 474
#define VARSTRING_VAR 475
#define VARSTRING_VAR_ARRAY 476
#define VARSTRING_COORD 477
#define VARSTRING_COORD_ARRAY 478
#define VARSTRING_REGION 479
#define VARSTRING_REGION_ARRAY 480
#define VARSTRING_MAPCHAR 481
#define VARSTRING_MAPCHAR_ARRAY 482
#define VARSTRING_MONST 483
#define VARSTRING_MONST_ARRAY 484
#define VARSTRING_OBJ 485
#define VARSTRING_OBJ_ARRAY 486
#define VARSTRING_SEL 487
#define VARSTRING_SEL_ARRAY 488
#define METHOD_INT 489
#define METHOD_INT_ARRAY 490
#define METHOD_STRING 491
#define METHOD_STRING_ARRAY 492
#define METHOD_VAR 493
#define METHOD_VAR_ARRAY 494
#define METHOD_COORD 495
#define METHOD_COORD_ARRAY 496
#define METHOD_REGION 497
#define METHOD_REGION_ARRAY 498
#define METHOD_MAPCHAR 499
#define METHOD_MAPCHAR_ARRAY 500
#define METHOD_MONST 501
#define METHOD_MONST_ARRAY 502
#define METHOD_OBJ 503
#define METHOD_OBJ_ARRAY 504
#define METHOD_SEL 505
#define METHOD_SEL_ARRAY 506
#define DICE 507

/* Value type.  */
#if ! defined YYSTYPE && ! defined YYSTYPE_IS_DECLARED
union YYSTYPE
{
#line 153 "lev_comp.y"

    long    i;
    char    *map;
    struct {
        long room;
        long wall;
        long door;
    } corpos;
    struct {
        long area;
        long x1;
        long y1;
        long x2;
        long y2;
    } lregn;
    struct {
        long x;
        long y;
    } crd;
    struct {
        long ter;
        long lit;
    } terr;
    struct {
        long height;
        long width;
    } sze;
    struct {
        long die;
        long num;
    } dice;
    struct {
        long cfunc;
        char *varstr;
    } meth;

#line 814 "lev_yacc.c"

};
typedef union YYSTYPE YYSTYPE;
# define YYSTYPE_IS_TRIVIAL 1
# define YYSTYPE_IS_DECLARED 1
#endif


extern YYSTYPE yylval;

int yyparse (void);

#endif /* !YY_YY_Y_TAB_H_INCLUDED  */



#ifdef short
# undef short
#endif

/* On compilers that do not define __PTRDIFF_MAX__ etc., make sure
   <limits.h> and (if available) <stdint.h> are included
   so that the code can choose integer types of a good width.  */

#ifndef __PTRDIFF_MAX__
# include <limits.h> /* INFRINGES ON USER NAME SPACE */
# if defined __STDC_VERSION__ && 199901 <= __STDC_VERSION__
#  include <stdint.h> /* INFRINGES ON USER NAME SPACE */
#  define YY_STDINT_H
# endif
#endif

/* Narrow types that promote to a signed type and that can represent a
   signed or unsigned integer of at least N bits.  In tables they can
   save space and decrease cache pressure.  Promoting to a signed type
   helps avoid bugs in integer arithmetic.  */

#ifdef __INT_LEAST8_MAX__
typedef __INT_LEAST8_TYPE__ yytype_int8;
#elif defined YY_STDINT_H
typedef int_least8_t yytype_int8;
#else
typedef signed char yytype_int8;
#endif

#ifdef __INT_LEAST16_MAX__
typedef __INT_LEAST16_TYPE__ yytype_int16;
#elif defined YY_STDINT_H
typedef int_least16_t yytype_int16;
#else
typedef short yytype_int16;
#endif

#if defined __UINT_LEAST8_MAX__ && __UINT_LEAST8_MAX__ <= __INT_MAX__
typedef __UINT_LEAST8_TYPE__ yytype_uint8;
#elif (!defined __UINT_LEAST8_MAX__ && defined YY_STDINT_H \
       && UINT_LEAST8_MAX <= INT_MAX)
typedef uint_least8_t yytype_uint8;
#elif !defined __UINT_LEAST8_MAX__ && UCHAR_MAX <= INT_MAX
typedef unsigned char yytype_uint8;
#else
typedef short yytype_uint8;
#endif

#if defined __UINT_LEAST16_MAX__ && __UINT_LEAST16_MAX__ <= __INT_MAX__
typedef __UINT_LEAST16_TYPE__ yytype_uint16;
#elif (!defined __UINT_LEAST16_MAX__ && defined YY_STDINT_H \
       && UINT_LEAST16_MAX <= INT_MAX)
typedef uint_least16_t yytype_uint16;
#elif !defined __UINT_LEAST16_MAX__ && USHRT_MAX <= INT_MAX
typedef unsigned short yytype_uint16;
#else
typedef int yytype_uint16;
#endif

#ifndef YYPTRDIFF_T
# if defined __PTRDIFF_TYPE__ && defined __PTRDIFF_MAX__
#  define YYPTRDIFF_T __PTRDIFF_TYPE__
#  define YYPTRDIFF_MAXIMUM __PTRDIFF_MAX__
# elif defined PTRDIFF_MAX
#  ifndef ptrdiff_t
#   include <stddef.h> /* INFRINGES ON USER NAME SPACE */
#  endif
#  define YYPTRDIFF_T ptrdiff_t
#  define YYPTRDIFF_MAXIMUM PTRDIFF_MAX
# else
#  define YYPTRDIFF_T long
#  define YYPTRDIFF_MAXIMUM LONG_MAX
# endif
#endif

#ifndef YYSIZE_T
# ifdef __SIZE_TYPE__
#  define YYSIZE_T __SIZE_TYPE__
# elif defined size_t
#  define YYSIZE_T size_t
# elif defined __STDC_VERSION__ && 199901 <= __STDC_VERSION__
#  include <stddef.h> /* INFRINGES ON USER NAME SPACE */
#  define YYSIZE_T size_t
# else
#  define YYSIZE_T unsigned
# endif
#endif

#define YYSIZE_MAXIMUM                                  \
  YY_CAST (YYPTRDIFF_T,                                 \
           (YYPTRDIFF_MAXIMUM < YY_CAST (YYSIZE_T, -1)  \
            ? YYPTRDIFF_MAXIMUM                         \
            : YY_CAST (YYSIZE_T, -1)))

#define YYSIZEOF(X) YY_CAST (YYPTRDIFF_T, sizeof (X))

/* Stored state numbers (used for stacks). */
typedef yytype_int16 yy_state_t;

/* State numbers in computations.  */
typedef int yy_state_fast_t;

#ifndef YY_
# if defined YYENABLE_NLS && YYENABLE_NLS
#  if ENABLE_NLS
#   include <libintl.h> /* INFRINGES ON USER NAME SPACE */
#   define YY_(Msgid) dgettext ("bison-runtime", Msgid)
#  endif
# endif
# ifndef YY_
#  define YY_(Msgid) Msgid
# endif
#endif

#ifndef YY_ATTRIBUTE_PURE
# if defined __GNUC__ && 2 < __GNUC__ + (96 <= __GNUC_MINOR__)
#  define YY_ATTRIBUTE_PURE __attribute__ ((__pure__))
# else
#  define YY_ATTRIBUTE_PURE
# endif
#endif

#ifndef YY_ATTRIBUTE_UNUSED
# if defined __GNUC__ && 2 < __GNUC__ + (7 <= __GNUC_MINOR__)
#  define YY_ATTRIBUTE_UNUSED __attribute__ ((__unused__))
# else
#  define YY_ATTRIBUTE_UNUSED
# endif
#endif

/* Suppress unused-variable warnings by "using" E.  */
#if ! defined lint || defined __GNUC__
# define YYUSE(E) ((void) (E))
#else
# define YYUSE(E) /* empty */
#endif

#if defined __GNUC__ && ! defined __ICC && 407 <= __GNUC__ * 100 + __GNUC_MINOR__
/* Suppress an incorrect diagnostic about yylval being uninitialized.  */
# define YY_IGNORE_MAYBE_UNINITIALIZED_BEGIN                            \
    _Pragma ("GCC diagnostic push")                                     \
    _Pragma ("GCC diagnostic ignored \"-Wuninitialized\"")              \
    _Pragma ("GCC diagnostic ignored \"-Wmaybe-uninitialized\"")
# define YY_IGNORE_MAYBE_UNINITIALIZED_END      \
    _Pragma ("GCC diagnostic pop")
#else
# define YY_INITIAL_VALUE(Value) Value
#endif
#ifndef YY_IGNORE_MAYBE_UNINITIALIZED_BEGIN
# define YY_IGNORE_MAYBE_UNINITIALIZED_BEGIN
# define YY_IGNORE_MAYBE_UNINITIALIZED_END
#endif
#ifndef YY_INITIAL_VALUE
# define YY_INITIAL_VALUE(Value) /* Nothing. */
#endif

#if defined __cplusplus && defined __GNUC__ && ! defined __ICC && 6 <= __GNUC__
# define YY_IGNORE_USELESS_CAST_BEGIN                          \
    _Pragma ("GCC diagnostic push")                            \
    _Pragma ("GCC diagnostic ignored \"-Wuseless-cast\"")
# define YY_IGNORE_USELESS_CAST_END            \
    _Pragma ("GCC diagnostic pop")
#endif
#ifndef YY_IGNORE_USELESS_CAST_BEGIN
# define YY_IGNORE_USELESS_CAST_BEGIN
# define YY_IGNORE_USELESS_CAST_END
#endif


#define YY_ASSERT(E) ((void) (0 && (E)))

#if ! defined yyoverflow || YYERROR_VERBOSE

/* The parser invokes alloca or malloc; define the necessary symbols.  */

# ifdef YYSTACK_USE_ALLOCA
#  if YYSTACK_USE_ALLOCA
#   ifdef __GNUC__
#    define YYSTACK_ALLOC __builtin_alloca
#   elif defined __BUILTIN_VA_ARG_INCR
#    include <alloca.h> /* INFRINGES ON USER NAME SPACE */
#   elif defined _AIX
#    define YYSTACK_ALLOC __alloca
#   elif defined _MSC_VER
#    include <malloc.h> /* INFRINGES ON USER NAME SPACE */
#    define alloca _alloca
#   else
#    define YYSTACK_ALLOC alloca
#    if ! defined _ALLOCA_H && ! defined EXIT_SUCCESS
#     include <stdlib.h> /* INFRINGES ON USER NAME SPACE */
      /* Use EXIT_SUCCESS as a witness for stdlib.h.  */
#     ifndef EXIT_SUCCESS
#      define EXIT_SUCCESS 0
#     endif
#    endif
#   endif
#  endif
# endif

# ifdef YYSTACK_ALLOC
   /* Pacify GCC's 'empty if-body' warning.  */
#  define YYSTACK_FREE(Ptr) do { /* empty */; } while (0)
#  ifndef YYSTACK_ALLOC_MAXIMUM
    /* The OS might guarantee only one guard page at the bottom of the stack,
       and a page size can be as small as 4096 bytes.  So we cannot safely
       invoke alloca (N) if N exceeds 4096.  Use a slightly smaller number
       to allow for a few compiler-allocated temporary stack slots.  */
#   define YYSTACK_ALLOC_MAXIMUM 4032 /* reasonable circa 2006 */
#  endif
# else
#  define YYSTACK_ALLOC YYMALLOC
#  define YYSTACK_FREE YYFREE
#  ifndef YYSTACK_ALLOC_MAXIMUM
#   define YYSTACK_ALLOC_MAXIMUM YYSIZE_MAXIMUM
#  endif
#  if (defined __cplusplus && ! defined EXIT_SUCCESS \
       && ! ((defined YYMALLOC || defined malloc) \
             && (defined YYFREE || defined free)))
#   include <stdlib.h> /* INFRINGES ON USER NAME SPACE */
#   ifndef EXIT_SUCCESS
#    define EXIT_SUCCESS 0
#   endif
#  endif
#  ifndef YYMALLOC
#   define YYMALLOC malloc
#   if ! defined malloc && ! defined EXIT_SUCCESS
void *malloc (YYSIZE_T); /* INFRINGES ON USER NAME SPACE */
#   endif
#  endif
#  ifndef YYFREE
#   define YYFREE free
#   if ! defined free && ! defined EXIT_SUCCESS
void free (void *); /* INFRINGES ON USER NAME SPACE */
#   endif
#  endif
# endif
#endif /* ! defined yyoverflow || YYERROR_VERBOSE */


#if (! defined yyoverflow \
     && (! defined __cplusplus \
         || (defined YYSTYPE_IS_TRIVIAL && YYSTYPE_IS_TRIVIAL)))

/* A type that is properly aligned for any stack member.  */
union yyalloc
{
  yy_state_t yyss_alloc;
  YYSTYPE yyvs_alloc;
};

/* The size of the maximum gap between one aligned stack and the next.  */
# define YYSTACK_GAP_MAXIMUM (YYSIZEOF (union yyalloc) - 1)

/* The size of an array large to enough to hold all stacks, each with
   N elements.  */
# define YYSTACK_BYTES(N) \
     ((N) * (YYSIZEOF (yy_state_t) + YYSIZEOF (YYSTYPE)) \
      + YYSTACK_GAP_MAXIMUM)

# define YYCOPY_NEEDED 1

/* Relocate STACK from its old location to the new one.  The
   local variables YYSIZE and YYSTACKSIZE give the old and new number of
   elements in the stack, and YYPTR gives the new location of the
   stack.  Advance YYPTR to a properly aligned location for the next
   stack.  */
# define YYSTACK_RELOCATE(Stack_alloc, Stack)                           \
    do                                                                  \
      {                                                                 \
        YYPTRDIFF_T yynewbytes;                                         \
        YYCOPY (&yyptr->Stack_alloc, Stack, yysize);                    \
        Stack = &yyptr->Stack_alloc;                                    \
        yynewbytes = yystacksize * YYSIZEOF (*Stack) + YYSTACK_GAP_MAXIMUM; \
        yyptr += yynewbytes / YYSIZEOF (*yyptr);                        \
      }                                                                 \
    while (0)

#endif

#if defined YYCOPY_NEEDED && YYCOPY_NEEDED
/* Copy COUNT objects from SRC to DST.  The source and destination do
   not overlap.  */
# ifndef YYCOPY
#  if defined __GNUC__ && 1 < __GNUC__
#   define YYCOPY(Dst, Src, Count) \
      __builtin_memcpy (Dst, Src, YY_CAST (YYSIZE_T, (Count)) * sizeof (*(Src)))
#  else
#   define YYCOPY(Dst, Src, Count)              \
      do                                        \
        {                                       \
          YYPTRDIFF_T yyi;                      \
          for (yyi = 0; yyi < (Count); yyi++)   \
            (Dst)[yyi] = (Src)[yyi];            \
        }                                       \
      while (0)
#  endif
# endif
#endif /* !YYCOPY_NEEDED */

/* YYFINAL -- State number of the termination state.  */
#define YYFINAL  9
/* YYLAST -- Last index in YYTABLE.  */
#define YYLAST   1300

/* YYNTOKENS -- Number of terminals.  */
#define YYNTOKENS  270
/* YYNNTS -- Number of nonterminals.  */
#define YYNNTS  188
/* YYNRULES -- Number of rules.  */
#define YYNRULES  513
/* YYNSTATES -- Number of states.  */
#define YYNSTATES  1136

#define YYUNDEFTOK  2
#define YYMAXUTOK   507


/* YYTRANSLATE(TOKEN-NUM) -- Symbol number corresponding to TOKEN-NUM
   as returned by yylex, with out-of-bounds checking.  */
#define YYTRANSLATE(YYX)                                                \
  (0 <= (YYX) && (YYX) <= YYMAXUTOK ? yytranslate[YYX] : YYUNDEFTOK)

/* YYTRANSLATE[TOKEN-NUM] -- Symbol number corresponding to TOKEN-NUM
   as returned by yylex.  */
static const yytype_int16 yytranslate[] =
{
       0,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,   265,   269,     2,
     209,   210,   263,   261,   207,   262,   267,   264,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,   208,     2,
       2,   266,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,   211,     2,   212,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,   213,   268,   214,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     1,     2,     3,     4,
       5,     6,     7,     8,     9,    10,    11,    12,    13,    14,
      15,    16,    17,    18,    19,    20,    21,    22,    23,    24,
      25,    26,    27,    28,    29,    30,    31,    32,    33,    34,
      35,    36,    37,    38,    39,    40,    41,    42,    43,    44,
      45,    46,    47,    48,    49,    50,    51,    52,    53,    54,
      55,    56,    57,    58,    59,    60,    61,    62,    63,    64,
      65,    66,    67,    68,    69,    70,    71,    72,    73,    74,
      75,    76,    77,    78,    79,    80,    81,    82,    83,    84,
      85,    86,    87,    88,    89,    90,    91,    92,    93,    94,
      95,    96,    97,    98,    99,   100,   101,   102,   103,   104,
     105,   106,   107,   108,   109,   110,   111,   112,   113,   114,
     115,   116,   117,   118,   119,   120,   121,   122,   123,   124,
     125,   126,   127,   128,   129,   130,   131,   132,   133,   134,
     135,   136,   137,   138,   139,   140,   141,   142,   143,   144,
     145,   146,   147,   148,   149,   150,   151,   152,   153,   154,
     155,   156,   157,   158,   159,   160,   161,   162,   163,   164,
     165,   166,   167,   168,   169,   170,   171,   172,   173,   174,
     175,   176,   177,   178,   179,   180,   181,   182,   183,   184,
     185,   186,   187,   188,   189,   190,   191,   192,   193,   194,
     195,   196,   197,   198,   199,   200,   201,   202,   203,   204,
     205,   206,   215,   216,   217,   218,   219,   220,   221,   222,
     223,   224,   225,   226,   227,   228,   229,   230,   231,   232,
     233,   234,   235,   236,   237,   238,   239,   240,   241,   242,
     243,   244,   245,   246,   247,   248,   249,   250,   251,   252,
     253,   254,   255,   256,   257,   258,   259,   260
};

#if YYDEBUG
  /* YYRLINE[YYN] -- Source line where rule number YYN was defined.  */
static const yytype_int16 yyrline[] =
{
       0,   286,   286,   287,   290,   291,   294,   317,   322,   344,
     357,   369,   375,   404,   410,   414,   420,   426,   433,   436,
     443,   447,   454,   457,   464,   465,   469,   472,   479,   483,
     490,   493,   499,   505,   506,   507,   508,   509,   510,   511,
     512,   513,   514,   515,   516,   517,   518,   519,   520,   521,
     522,   523,   524,   525,   526,   527,   528,   529,   530,   531,
     532,   533,   534,   535,   536,   537,   538,   539,   540,   541,
     542,   543,   544,   545,   546,   547,   548,   549,   550,   551,
     552,   553,   554,   555,   556,   557,   558,   559,   560,   561,
     562,   563,   564,   565,   566,   569,   570,   571,   572,   573,
     574,   575,   576,   577,   580,   581,   582,   583,   584,   585,
     586,   587,   588,   591,   592,   593,   596,   597,   600,   616,
     622,   628,   634,   640,   646,   652,   658,   664,   674,   684,
     694,   704,   714,   724,   736,   741,   748,   753,   760,   765,
     772,   776,   782,   787,   794,   798,   804,   808,   815,   837,
     814,   851,   906,   913,   916,   922,   929,   933,   942,   946,
     941,  1009,  1010,  1014,  1013,  1027,  1026,  1041,  1051,  1052,
    1055,  1093,  1092,  1127,  1126,  1157,  1156,  1189,  1188,  1214,
    1225,  1224,  1252,  1258,  1263,  1268,  1275,  1282,  1291,  1299,
    1311,  1310,  1334,  1333,  1357,  1360,  1366,  1376,  1382,  1391,
    1397,  1402,  1408,  1413,  1419,  1430,  1436,  1437,  1440,  1441,
    1444,  1448,  1454,  1455,  1459,  1465,  1473,  1478,  1483,  1488,
    1493,  1498,  1503,  1511,  1518,  1526,  1534,  1535,  1538,  1539,
    1542,  1547,  1546,  1560,  1567,  1574,  1582,  1587,  1593,  1599,
    1605,  1611,  1616,  1621,  1626,  1631,  1636,  1641,  1646,  1651,
    1656,  1661,  1666,  1672,  1677,  1684,  1693,  1697,  1710,  1719,
    1718,  1736,  1746,  1752,  1760,  1766,  1771,  1776,  1781,  1786,
    1791,  1796,  1801,  1806,  1820,  1826,  1831,  1836,  1841,  1846,
    1851,  1856,  1861,  1866,  1871,  1876,  1881,  1886,  1891,  1896,
    1901,  1907,  1912,  1917,  1924,  1930,  1959,  1964,  1972,  1978,
    1982,  1990,  1997,  2004,  2014,  2024,  2040,  2051,  2054,  2060,
    2066,  2072,  2076,  2082,  2089,  2095,  2103,  2109,  2114,  2119,
    2124,  2129,  2135,  2141,  2146,  2151,  2156,  2161,  2166,  2173,
    2173,  2173,  2173,  2176,  2182,  2188,  2193,  2200,  2207,  2211,
    2217,  2223,  2229,  2234,  2241,  2247,  2257,  2264,  2263,  2295,
    2298,  2304,  2309,  2314,  2319,  2325,  2329,  2335,  2341,  2345,
    2351,  2355,  2361,  2365,  2370,  2377,  2381,  2388,  2392,  2397,
    2404,  2408,  2413,  2421,  2427,  2434,  2438,  2445,  2453,  2456,
    2466,  2470,  2473,  2479,  2483,  2490,  2494,  2498,  2505,  2508,
    2514,  2521,  2524,  2530,  2537,  2541,  2548,  2549,  2552,  2553,
    2556,  2557,  2558,  2564,  2565,  2566,  2572,  2573,  2576,  2585,
    2590,  2597,  2608,  2614,  2618,  2622,  2629,  2639,  2646,  2650,
    2656,  2660,  2668,  2672,  2679,  2689,  2702,  2706,  2713,  2723,
    2732,  2743,  2747,  2754,  2764,  2775,  2784,  2794,  2800,  2804,
    2811,  2821,  2832,  2841,  2851,  2855,  2862,  2863,  2869,  2873,
    2877,  2881,  2889,  2898,  2902,  2906,  2910,  2914,  2918,  2921,
    2928,  2937,  2970,  2971,  2974,  2975,  2978,  2982,  2989,  2996,
    3007,  3010,  3018,  3022,  3026,  3030,  3034,  3039,  3043,  3047,
    3052,  3057,  3062,  3066,  3071,  3076,  3080,  3084,  3089,  3093,
    3100,  3106,  3110,  3116,  3123,  3124,  3127,  3128,  3129,  3132,
    3136,  3140,  3144,  3150,  3151,  3154,  3155,  3158,  3159,  3162,
    3163,  3166,  3170,  3196
};
#endif

#if YYDEBUG || YYERROR_VERBOSE || 0
/* YYTNAME[SYMBOL-NUM] -- String name of the symbol SYMBOL-NUM.
   First, the terminals, then, starting at YYNTOKENS, nonterminals.  */
static const char *const yytname[] =
{
  "$end", "error", "$undefined", "CHAR", "INTEGER", "BOOLEAN", "PERCENT",
  "SPERCENT", "MINUS_INTEGER", "PLUS_INTEGER", "MAZE_GRID_ID",
  "SOLID_FILL_ID", "MINES_ID", "ROGUELEV_ID", "MESSAGE_ID", "MAZE_ID",
  "LEVEL_ID", "LEV_INIT_ID", "TILESET_ID", "GEOMETRY_ID", "NOMAP_ID",
  "BOUNDARY_TYPE_ID", "SPECIAL_TILESET_ID", "OBJECT_ID", "COBJECT_ID",
  "MONSTER_ID", "TRAP_ID", "DOOR_ID", "DRAWBRIDGE_ID",
  "MONSTER_GENERATION_ID", "object_ID", "monster_ID", "terrain_ID",
  "MAZEWALK_ID", "WALLIFY_ID", "REGION_ID", "SPECIAL_REGION_ID",
  "SPECIAL_LEVREGION_ID", "SPECIAL_REGION_TYPE", "NAMING_ID",
  "NAMING_TYPE", "FILLING", "IRREGULAR", "JOINED", "ALTAR_ID", "ANVIL_ID",
  "NPC_ID", "LADDER_ID", "STAIR_ID", "NON_DIGGABLE_ID", "NON_PASSWALL_ID",
  "ROOM_ID", "ARTIFACT_NAME_ID", "PORTAL_ID", "TELEPRT_ID", "BRANCH_ID",
  "LEV", "MINERALIZE_ID", "AGE_ID", "CORRIDOR_ID", "GOLD_ID",
  "ENGRAVING_ID", "FOUNTAIN_ID", "THRONE_ID", "MODRON_PORTAL_ID",
  "LEVEL_TELEPORTER_ID", "LEVEL_TELEPORT_DIRECTION_TYPE",
  "LEVEL_TELEPORT_END_TYPE", "POOL_ID", "SINK_ID", "NONE",
  "RAND_CORRIDOR_ID", "DOOR_STATE", "LIGHT_STATE", "CURSE_TYPE",
  "MYTHIC_TYPE", "ENGRAVING_TYPE", "KEYTYPE_ID", "LEVER_ID",
  "NO_PICKUP_ID", "DIRECTION", "RANDOM_TYPE", "RANDOM_TYPE_BRACKET",
  "A_REGISTER", "ALIGNMENT", "LEFT_OR_RIGHT", "CENTER", "TOP_OR_BOT",
  "ALTAR_TYPE", "ALTAR_SUBTYPE", "UP_OR_DOWN", "ACTIVE_OR_INACTIVE",
  "MODRON_PORTAL_TYPE", "NPC_TYPE", "FOUNTAIN_TYPE", "SPECIAL_OBJECT_TYPE",
  "CMAP_TYPE", "FLOOR_SUBTYPE", "FLOOR_SUBTYPE_ID", "FLOOR_ID",
  "FLOOR_TYPE", "FLOOR_TYPE_ID", "ELEMENTAL_ENCHANTMENT_TYPE",
  "EXCEPTIONALITY_TYPE", "EXCEPTIONALITY_ID", "ELEMENTAL_ENCHANTMENT_ID",
  "ENCHANTMENT_ID", "SECRET_DOOR_ID", "USES_UP_KEY_ID",
  "MYTHIC_PREFIX_TYPE", "MYTHIC_SUFFIX_TYPE", "MYTHIC_PREFIX_ID",
  "MYTHIC_SUFFIX_ID", "CHARGES_ID", "SPECIAL_QUALITY_ID", "SPEFLAGS_ID",
  "SUBROOM_ID", "NAME_ID", "FLAGS_ID", "FLAG_TYPE", "MON_ATTITUDE",
  "MON_ALERTNESS", "SUBTYPE_ID", "NON_PASSDOOR_ID", "MON_APPEARANCE",
  "ROOMDOOR_ID", "IF_ID", "ELSE_ID", "TERRAIN_ID", "HORIZ_OR_VERT",
  "REPLACE_TERRAIN_ID", "LOCATION_SUBTYPE_ID", "DOOR_SUBTYPE",
  "BRAZIER_SUBTYPE", "SIGNPOST_SUBTYPE", "TREE_SUBTYPE", "FOREST_ID",
  "FOREST_TYPE", "INITIALIZE_TYPE", "EXIT_ID", "SHUFFLE_ID",
  "MANUAL_TYPE_ID", "MANUAL_TYPE", "QUANTITY_ID", "BURIED_ID", "LOOP_ID",
  "FOR_ID", "TO_ID", "SWITCH_ID", "CASE_ID", "BREAK_ID", "DEFAULT_ID",
  "ERODED_ID", "TRAPPED_STATE", "RECHARGED_ID", "INVIS_ID", "GREASED_ID",
  "INDESTRUCTIBLE_ID", "FEMALE_ID", "MALE_ID", "WAITFORU_ID",
  "PROTECTOR_ID", "CANCELLED_ID", "REVIVED_ID", "AVENGE_ID", "FLEEING_ID",
  "BLINDED_ID", "PARALYZED_ID", "STUNNED_ID", "CONFUSED_ID",
  "SEENTRAPS_ID", "ALL_ID", "MONTYPE_ID", "OBJTYPE_ID", "TERTYPE_ID",
  "TERTYPE2_ID", "LEVER_EFFECT_TYPE", "SWITCHABLE_ID",
  "CONTINUOUSLY_USABLE_ID", "TARGET_ID", "TRAPTYPE_ID", "EFFECT_FLAG_ID",
  "GRAVE_ID", "BRAZIER_ID", "SIGNPOST_ID", "TREE_ID", "ERODEPROOF_ID",
  "FUNCTION_ID", "MSG_OUTPUT_TYPE", "COMPARE_TYPE", "UNKNOWN_TYPE",
  "rect_ID", "fillrect_ID", "line_ID", "randline_ID", "grow_ID",
  "selection_ID", "flood_ID", "rndcoord_ID", "circle_ID", "ellipse_ID",
  "filter_ID", "complement_ID", "gradient_ID", "GRADIENT_TYPE", "LIMITED",
  "HUMIDITY_TYPE", "','", "':'", "'('", "')'", "'['", "']'", "'{'", "'}'",
  "STRING", "MAP_ID", "NQSTRING", "VARSTRING", "CFUNC", "CFUNC_INT",
  "CFUNC_STR", "CFUNC_COORD", "CFUNC_REGION", "VARSTRING_INT",
  "VARSTRING_INT_ARRAY", "VARSTRING_STRING", "VARSTRING_STRING_ARRAY",
  "VARSTRING_VAR", "VARSTRING_VAR_ARRAY", "VARSTRING_COORD",
  "VARSTRING_COORD_ARRAY", "VARSTRING_REGION", "VARSTRING_REGION_ARRAY",
  "VARSTRING_MAPCHAR", "VARSTRING_MAPCHAR_ARRAY", "VARSTRING_MONST",
  "VARSTRING_MONST_ARRAY", "VARSTRING_OBJ", "VARSTRING_OBJ_ARRAY",
  "VARSTRING_SEL", "VARSTRING_SEL_ARRAY", "METHOD_INT", "METHOD_INT_ARRAY",
  "METHOD_STRING", "METHOD_STRING_ARRAY", "METHOD_VAR", "METHOD_VAR_ARRAY",
  "METHOD_COORD", "METHOD_COORD_ARRAY", "METHOD_REGION",
  "METHOD_REGION_ARRAY", "METHOD_MAPCHAR", "METHOD_MAPCHAR_ARRAY",
  "METHOD_MONST", "METHOD_MONST_ARRAY", "METHOD_OBJ", "METHOD_OBJ_ARRAY",
  "METHOD_SEL", "METHOD_SEL_ARRAY", "DICE", "'+'", "'-'", "'*'", "'/'",
  "'%'", "'='", "'.'", "'|'", "'&'", "$accept", "file", "levels", "level",
  "level_def", "lev_init", "tileset_detail", "forest_detail",
  "monster_generation_detail", "boundary_type_detail", "opt_limited",
  "opt_coord_or_var", "opt_fillchar", "walled", "flags", "flag_list",
  "levstatements", "stmt_block", "levstatement", "any_var_array",
  "any_var", "any_var_or_arr", "any_var_or_unk", "shuffle_detail",
  "variable_define", "encodeobj_list", "encodemonster_list",
  "mapchar_list", "encoderegion_list", "encodecoord_list", "integer_list",
  "string_list", "function_define", "$@1", "$@2", "function_call",
  "exitstatement", "opt_percent", "comparestmt", "switchstatement", "$@3",
  "$@4", "switchcases", "switchcase", "$@5", "$@6", "breakstatement",
  "for_to_span", "forstmt_start", "forstatement", "$@7", "loopstatement",
  "$@8", "chancestatement", "$@9", "ifstatement", "$@10", "if_ending",
  "$@11", "message", "random_corridors", "corridor", "corr_spec",
  "room_begin", "subroom_def", "$@12", "room_def", "$@13", "roomfill",
  "room_pos", "subroom_pos", "room_align", "room_size", "door_detail",
  "secret", "door_wall", "dir_list", "door_pos", "door_infos", "door_info",
  "map_definition", "h_justif", "v_justif", "monster_detail", "$@14",
  "monster_desc", "monster_infos", "monster_info", "seen_trap_mask",
  "object_detail", "$@15", "object_desc", "object_infos", "object_info",
  "trap_detail", "drawbridge_detail", "mazewalk_detail", "wallify_detail",
  "ladder_detail", "stair_detail", "stair_region", "portal_region",
  "teleprt_region", "branch_region", "teleprt_detail", "fountain_detail",
  "throne_detail", "modron_portal_detail", "lever_detail", "lever_infos",
  "lever_info", "valid_subtype", "sink_detail", "pool_detail",
  "terrain_type", "replace_terrain_detail", "terrain_detail",
  "diggable_detail", "passwall_detail", "naming_detail",
  "special_region_detail", "special_levregion_detail",
  "special_tileset_detail", "region_detail", "@16", "region_detail_end",
  "altar_detail", "anvil_detail", "floor_detail", "subtype_detail",
  "npc_detail", "grave_detail", "brazier_detail", "signpost_detail",
  "tree_detail", "gold_detail", "engraving_detail", "mineralize",
  "trap_name", "room_type", "optroomregionflags", "roomregionflags",
  "roomregionflag", "optfloortype", "floortype", "optfloorsubtype",
  "floorsubtype", "optmontype", "door_state", "light_state", "alignment",
  "alignment_prfx", "altar_type", "a_register", "string_or_var",
  "integer_or_var", "coord_or_var", "encodecoord", "humidity_flags",
  "region_or_var", "encoderegion", "mapchar_or_var", "mapchar",
  "monster_or_var", "encodemonster", "object_or_var", "encodeobj",
  "string_expr", "math_expr_var", "func_param_type", "func_param_part",
  "func_param_list", "func_params_list", "func_call_param_part",
  "func_call_param_list", "func_call_params_list", "ter_selection_x",
  "ter_selection", "dice", "tileset_number", "all_integers",
  "all_ints_push", "objectid", "monsterid", "terrainid", "engraving_type",
  "lev_region", "region", YY_NULLPTR
};
#endif

# ifdef YYPRINT
/* YYTOKNUM[NUM] -- (External) token number corresponding to the
   (internal) symbol number NUM (which must be that of a token).  */
static const yytype_int16 yytoknum[] =
{
       0,   256,   257,   258,   259,   260,   261,   262,   263,   264,
     265,   266,   267,   268,   269,   270,   271,   272,   273,   274,
     275,   276,   277,   278,   279,   280,   281,   282,   283,   284,
     285,   286,   287,   288,   289,   290,   291,   292,   293,   294,
     295,   296,   297,   298,   299,   300,   301,   302,   303,   304,
     305,   306,   307,   308,   309,   310,   311,   312,   313,   314,
     315,   316,   317,   318,   319,   320,   321,   322,   323,   324,
     325,   326,   327,   328,   329,   330,   331,   332,   333,   334,
     335,   336,   337,   338,   339,   340,   341,   342,   343,   344,
     345,   346,   347,   348,   349,   350,   351,   352,   353,   354,
     355,   356,   357,   358,   359,   360,   361,   362,   363,   364,
     365,   366,   367,   368,   369,   370,   371,   372,   373,   374,
     375,   376,   377,   378,   379,   380,   381,   382,   383,   384,
     385,   386,   387,   388,   389,   390,   391,   392,   393,   394,
     395,   396,   397,   398,   399,   400,   401,   402,   403,   404,
     405,   406,   407,   408,   409,   410,   411,   412,   413,   414,
     415,   416,   417,   418,   419,   420,   421,   422,   423,   424,
     425,   426,   427,   428,   429,   430,   431,   432,   433,   434,
     435,   436,   437,   438,   439,   440,   441,   442,   443,   444,
     445,   446,   447,   448,   449,   450,   451,   452,   453,   454,
     455,   456,   457,   458,   459,   460,   461,    44,    58,    40,
      41,    91,    93,   123,   125,   462,   463,   464,   465,   466,
     467,   468,   469,   470,   471,   472,   473,   474,   475,   476,
     477,   478,   479,   480,   481,   482,   483,   484,   485,   486,
     487,   488,   489,   490,   491,   492,   493,   494,   495,   496,
     497,   498,   499,   500,   501,   502,   503,   504,   505,   506,
     507,    43,    45,    42,    47,    37,    61,    46,   124,    38
};
# endif

#define YYPACT_NINF (-971)

#define yypact_value_is_default(Yyn) \
  ((Yyn) == YYPACT_NINF)

#define YYTABLE_NINF (-232)

#define yytable_value_is_error(Yyn) \
  0

  /* YYPACT[STATE-NUM] -- Index in YYTABLE of the portion describing
     STATE-NUM.  */
static const yytype_int16 yypact[] =
{
     230,   -38,   -29,   147,  -971,   230,    59,   -32,   -30,  -971,
    -971,   -18,   852,   -11,  -971,    81,  -971,     2,    11,    21,
      24,  -971,    60,    66,    79,    82,    87,    89,   101,   112,
     118,   121,   128,   140,   151,   169,   177,   179,   182,   184,
     194,   195,   203,   205,   207,   211,   212,   214,   216,   218,
     219,   220,   221,   225,   226,   228,   229,   234,   236,   237,
     243,   245,   256,    40,   258,   260,   261,  -971,   262,    -9,
     933,  -971,  -971,   265,   266,   267,   272,     4,   113,    26,
    -971,  -971,  -971,  -971,  -971,  -971,  -971,  -971,  -971,  -971,
    -971,  -971,  -971,  -971,  -971,  -971,  -971,  -971,  -971,  -971,
    -971,  -971,  -971,  -971,  -971,   852,  -971,  -971,   -27,  -971,
    -971,  -971,  -971,  -971,   275,  -971,  -971,  -971,  -971,  -971,
    -971,  -971,  -971,  -971,  -971,  -971,  -971,  -971,  -971,  -971,
    -971,  -971,  -971,  -971,  -971,  -971,  -971,  -971,  -971,  -971,
    -971,  -971,  -971,  -971,  -971,  -971,  -971,  -971,  -971,  -971,
    -971,  -971,  -971,  -971,  -971,  -971,  -971,  -971,  -971,  -971,
    -971,  -971,  -971,  -971,  -971,  -971,  -971,  -971,   269,    30,
    -971,  -111,   352,    76,   -23,   192,   947,    39,    39,   152,
     -34,    33,     0,   280,     0,   903,  -107,  -107,   -12,   186,
       0,     0,   313,     0,    13,  -107,  -107,   -31,   -12,   -12,
     -12,   113,   287,   113,     0,   947,   947,     0,   947,   947,
      88,     0,   947,   -31,   947,    84,  -971,   947,  -107,   301,
     820,   113,  -971,  -971,   233,   273,     0,     0,     0,     0,
     297,  -971,    25,  -971,   292,  -971,   193,  -971,   126,  -971,
     400,  -971,   298,  -971,    81,  -971,  -971,   299,  -971,   248,
     309,   311,   312,  -971,  -971,  -971,  -971,  -971,   316,  -971,
    -971,   317,   516,  -971,   318,   325,   327,  -971,  -971,  -107,
    -107,     0,     0,   326,     0,   328,   329,   330,   947,   332,
     615,  -971,  -971,   254,   336,  -971,  -971,  -971,   544,  -971,
    -971,   331,  -971,  -971,  -971,  -971,  -971,  -971,   551,  -971,
    -971,   344,   343,   351,  -971,  -971,  -971,   354,  -971,  -971,
     355,   356,   366,   368,  -971,  -971,   575,  -971,   370,   375,
    -971,   379,   390,   601,   401,  -971,   403,   404,   405,   407,
     409,   603,   410,   411,  -971,  -971,  -971,  -971,   416,   622,
     422,   428,   429,   432,   449,   636,   435,   -48,   436,   438,
    -971,   445,  -971,  -971,  -971,  -971,  -971,  -971,  -971,   446,
     447,   461,   486,  -971,  -971,   494,   298,   495,   498,   499,
    -971,   457,   113,   113,   500,   501,   508,   509,  -971,   490,
     493,   113,   113,  -971,   113,   113,   113,   113,   113,   248,
     449,  -971,   511,   510,  -971,  -971,  -971,  -971,  -971,  -971,
     513,    64,   106,  -971,  -971,   248,   449,   520,   521,   522,
     852,   852,  -971,  -971,   113,  -111,   728,    29,   735,   533,
     529,   947,   535,   113,   172,   740,   530,  -971,  -971,   541,
     542,   812,  -971,     0,     0,   464,  -971,   546,   549,   947,
     665,   564,   113,   565,   298,   566,   113,   298,     0,     0,
     947,   694,    38,   695,   570,   113,    50,   741,   774,   578,
     756,   755,   144,   624,     0,   707,   598,   731,   -12,   -20,
    -971,   616,   -12,   -12,   -12,   113,   618,    62,     0,    31,
     732,    20,   646,   727,   -16,    73,    33,   701,  -971,   160,
     160,   692,  -971,   198,   619,   -21,   699,   704,    -2,   889,
    -971,  -971,   385,   462,    53,    53,  -971,  -971,  -971,   126,
    -971,   947,   626,   -96,   -86,   -75,   -71,  -971,  -971,   248,
     449,   145,   110,   165,  -971,   633,   505,  -971,  -971,  -971,
     845,  -971,   642,   316,  -971,   640,   847,   539,  -971,  -971,
     327,  -971,  -971,     0,     0,   584,   647,   643,   649,   650,
    -971,   652,   589,  -971,   654,   660,  -971,   661,   676,  -971,
    -971,  -971,   669,   599,   335,  -971,   675,   670,  -971,  -971,
    -971,  -971,   710,   711,  -971,   712,   904,   683,  -971,  -971,
     715,  -971,   717,   921,  -971,   719,  -971,   716,  -971,   721,
    -971,   729,   722,  -971,   932,  -971,   733,  -971,   934,   734,
      50,   742,   743,  -971,   745,   859,  -971,  -971,  -971,  -971,
    -971,   746,  -971,   747,   748,  -971,   751,  -971,   938,   757,
    -971,   758,   759,   828,   964,   762,   763,  -971,   298,  -971,
     705,   113,  -971,  -971,   248,   766,   768,  -971,  -971,   771,
    -971,   769,   773,  -971,  -971,  -971,  -971,   977,   777,  -971,
      -8,  -971,   113,  -971,  -111,  -971,    27,  -971,    38,  -971,
      46,  -971,  -971,  -971,   780,   986,  -971,  -971,   783,  -971,
     779,  -971,   789,   910,   947,  -971,   113,   113,   947,   792,
     113,   947,   947,   799,   791,  -971,  -971,  -971,  -971,   808,
    -971,  -971,  -971,  -971,   809,  -971,   810,   811,   814,  -971,
     815,   816,   817,   818,   819,   822,   823,   824,  -971,   825,
    -971,   830,  -971,  -971,  -971,   832,  -971,  -971,  -971,  -971,
    -971,   831,  -971,   813,   837,    33,    38,  1041,   841,  -971,
     -31,  1046,   849,   882,  1054,    57,   152,   888,  -101,   972,
     857,    -6,  -971,   850,   976,  -971,   113,   860,  -111,  1002,
       0,   864,   975,   866,    -5,   978,   199,   298,  1011,   160,
    -971,  -971,   449,   886,    50,    -3,   155,   889,  -971,   -83,
    -971,  -971,   449,   248,   -63,  -971,   -61,   -58,  -971,    50,
     902,  -971,  -971,   113,  -971,   900,   288,   427,   901,    50,
     698,   922,   925,   113,  -971,   113,    39,  1028,  1034,   113,
    1043,  1027,   113,   113,   113,  -111,  1012,   113,   113,   113,
     152,  -971,   517,   417,  -971,  -971,   946,  1151,   951,   953,
    1158,   956,   954,  -971,  -971,   961,  -971,   962,  -971,  1168,
    -971,   294,   967,  -971,  -971,   968,    80,   248,   969,   973,
     662,  -971,  1175,  -971,  1177,   951,  -971,  -971,   979,  -971,
    -971,  -971,   981,     1,  -971,  -971,   248,  -971,  -971,  -971,
    -971,   298,    27,  -971,  -971,    46,  -971,   974,  1178,   449,
    -971,  1141,  -971,   113,  -971,   980,  -971,  -971,  -971,   472,
    -971,  -971,  -971,  -971,  -971,  -971,  -971,  -971,  -971,  -971,
     248,  -971,  -971,  -971,  -971,  -971,   -51,  -971,  -971,  -971,
    -111,  -971,  -971,  -971,  -971,  -971,  -971,  -971,  -971,   983,
     984,   985,  -971,  -971,   987,  -971,  -971,  -971,   248,   988,
    -971,  -971,   989,  -971,   990,  -971,  -971,  1182,  -971,   982,
     314,  1093,  1195,   993,   152,  -971,    -1,   152,   991,   995,
      -5,   113,  -971,  -971,   994,  1136,  1113,  -971,   998,   999,
    1000,  1001,  1003,  1004,  1005,  -971,  -971,  1006,  1007,  1008,
    -971,  1009,  1010,  1093,    80,  1203,   295,  1013,  1014,     1,
    -971,  -971,  -971,  -971,  1015,  1016,   307,  -971,   113,  1137,
     248,   113,   113,   113,   -78,    39,  1216,  1091,  -971,  1220,
    -971,  -971,  -971,  -971,  1018,  1019,  1131,  1023,  1227,  -971,
    1025,  1026,  -971,  -971,   172,   951,  -971,  -971,  1029,  1030,
    1138,  1229,    48,   152,    39,    29,    29,     0,   -34,  1234,
    -971,  1235,  1131,  -971,  -971,  1032,  -971,  -971,  -971,  1236,
    -971,  1201,  -971,   357,  -971,  -971,  -971,  -971,  -971,   992,
    -971,  -971,  -971,  -971,  1033,   314,  1144,  1037,  1074,  1243,
    1038,  1077,   152,  1040,  1093,  1159,  1161,  -971,  -971,  -971,
    -971,  -971,  -971,  -971,  -971,  -971,  -971,  -971,  -971,  -971,
    -971,  -971,  1044,  1074,   837,  -971,   852,  1048,  1047,  1051,
    1049,   -78,  -971,  -971,  -971,  -971,  1138,  1045,  -971,  1052,
    -971,  1050,  -971,  -971,  1131,  1056,  -971,  -971,  -971,   852,
    -971,    50,  -971,  -971,  1057,  -971,  -971,   152,   298,  -971,
     152,  1074,  1170,   298,  -971,  1058,     0,  -971,  -971,  -971,
    -971,  -971,  -971,  -971,    95,  1059,   298,  -971,  -971,   946,
       0,  1060,  -971,  -971,  -971,  -971
};

  /* YYDEFACT[STATE-NUM] -- Default reduction number in state STATE-NUM.
     Performed when YYTABLE does not specify something else to do.  Zero
     means the default is an error.  */
static const yytype_int16 yydefact[] =
{
       2,     0,     0,     0,     3,     4,    26,     0,     0,     1,
       5,     0,    30,     0,     7,     0,   155,     0,     0,     0,
       0,   223,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,   298,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,   376,     0,
       0,     0,     0,     0,     0,     0,     0,   183,     0,     0,
       0,     0,     0,     0,     0,     0,     0,   152,     0,     0,
       0,   158,   167,     0,     0,     0,     0,     0,     0,     0,
     115,   104,    95,   105,    96,   106,    97,   107,    98,   108,
      99,   109,   100,   110,   101,   111,   102,   112,   103,    34,
      35,    38,    40,    37,     6,    30,   113,   114,     0,    52,
      51,    69,    70,    67,     0,    62,    68,   171,    63,    64,
      66,    65,    33,    80,    50,    86,    85,    54,    73,    75,
      76,    93,    55,    74,    94,    71,    90,    91,    79,    92,
      49,    58,    59,    60,    72,    87,    78,    89,    88,    53,
      77,    81,    82,    83,    36,    84,    42,    43,    41,    39,
      44,    45,    46,    47,    48,    61,    56,    57,     0,    29,
      27,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,   177,     0,     0,     0,
       0,     0,   116,   117,     0,     0,     0,     0,     0,     0,
       0,   448,     0,   451,     0,   493,     0,   449,   470,    31,
       0,   175,     0,     8,     0,   409,   410,     0,   446,   182,
       0,     0,     0,    11,   495,   494,    13,   418,     0,   226,
     227,     0,     0,   415,     0,     0,   194,   413,    17,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,   489,   472,   491,     0,   442,   444,   445,     0,   441,
     439,     0,   258,   262,   438,   259,   435,   437,     0,   434,
     432,     0,   230,     0,   431,   378,   377,     0,   396,   397,
       0,     0,     0,     0,   300,   299,     0,   423,     0,     0,
     422,     0,     0,     0,     0,   511,     0,     0,   355,     0,
       0,     0,     0,     0,   340,   341,   380,   379,     0,   153,
       0,     0,     0,     0,   412,     0,     0,     0,     0,     0,
     310,     0,   334,   333,   498,   496,   497,   185,   184,     0,
       0,     0,     0,   206,   207,     0,     0,     0,     0,    15,
     118,     0,     0,     0,   364,   366,   369,   372,   148,     0,
       0,     0,     0,   157,     0,     0,     0,     0,     0,   467,
     466,   468,   471,     0,   504,   506,   503,   505,   507,   508,
       0,     0,     0,   125,   126,   121,   119,     0,     0,     0,
       0,    30,   172,    28,     0,     0,     0,     0,     0,   420,
       0,     0,     0,     0,     0,     0,     0,   473,   474,     0,
       0,     0,   482,     0,     0,     0,   488,     0,     0,     0,
       0,     0,     0,   261,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
     154,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,   179,   178,     0,
       0,     0,   173,     0,     0,     0,     0,     0,     0,   464,
     450,   458,     0,     0,   453,   454,   455,   456,   457,     0,
     151,     0,   448,     0,     0,     0,     0,   142,   140,   146,
     144,     0,     0,     0,   176,     0,     0,   447,    10,   335,
       0,     9,     0,     0,   419,     0,     0,     0,   229,   228,
     194,   195,   225,     0,     0,   210,     0,     0,     0,     0,
     429,     0,     0,   427,     0,     0,   426,     0,     0,   490,
     492,   346,     0,     0,     0,   260,     0,     0,   232,   234,
     294,   214,     0,    16,   136,   296,     0,     0,   398,   399,
       0,   344,     0,     0,   345,   342,   402,     0,   400,     0,
     401,     0,   360,   301,     0,   302,     0,   197,     0,     0,
       0,     0,   307,   306,     0,     0,   186,   187,   373,   509,
     510,     0,   309,     0,     0,   314,     0,   199,     0,     0,
     358,     0,     0,     0,     0,   338,     0,    14,     0,   169,
       0,     0,   159,   363,   362,     0,     0,   370,   371,     0,
     462,   465,     0,   452,   156,   469,   120,     0,     0,   129,
       0,   128,     0,   127,     0,   133,     0,   124,     0,   123,
       0,   122,    32,   411,     0,     0,   421,   414,     0,   416,
       0,   475,     0,     0,     0,   477,     0,     0,     0,     0,
       0,     0,     0,     0,     0,   440,   501,   499,   500,     0,
     273,   270,   264,   290,     0,   289,     0,     0,     0,   288,
       0,     0,     0,     0,     0,     0,     0,     0,   269,     0,
     274,     0,   276,   277,   287,     0,   272,   263,   278,   502,
     266,     0,   433,   233,   205,     0,     0,     0,     0,   424,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,   189,     0,     0,   305,     0,     0,     0,     0,
       0,   313,     0,     0,     0,     0,     0,     0,     0,     0,
     174,   168,   170,     0,     0,     0,     0,     0,   149,     0,
     141,   143,   145,   147,     0,   134,     0,     0,   138,     0,
       0,   417,   224,     0,   211,     0,     0,     0,     0,     0,
       0,     0,     0,     0,   443,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,   436,     0,     0,   295,   137,    22,     0,   381,     0,
       0,     0,     0,   407,   406,   351,   356,     0,   303,     0,
     201,     0,     0,   304,   308,     0,     0,   374,     0,     0,
       0,   357,     0,   203,     0,   381,   359,   209,     0,   208,
     181,   339,     0,   161,   365,   368,   367,   459,   460,   461,
     463,     0,     0,   132,   131,     0,   130,     0,     0,   476,
     478,     0,   483,     0,   479,     0,   428,   481,   480,     0,
     293,   286,   280,   279,   281,   291,   292,   282,   283,   285,
     267,   284,   268,   271,   275,   265,     0,   403,   237,   238,
       0,   243,   241,   242,   253,   254,   244,   245,   246,     0,
       0,     0,   250,   251,     0,   235,   239,   404,   236,     0,
     220,   221,     0,   222,     0,   219,   215,     0,   297,     0,
       0,   388,     0,     0,     0,   408,     0,     0,     0,     0,
       0,     0,   212,   213,     0,     0,     0,   316,     0,     0,
       0,     0,     0,     0,     0,   321,   322,     0,     0,     0,
     315,     0,     0,   388,     0,     0,     0,     0,     0,   161,
     150,   135,   139,   336,     0,     0,     0,   430,     0,     0,
     240,     0,     0,     0,     0,     0,     0,     0,    23,     0,
     385,   386,   387,   382,   383,     0,   391,     0,     0,   343,
     353,     0,   361,   196,     0,   381,   375,   188,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
     198,     0,   391,   214,   337,     0,   165,   160,   162,     0,
     484,     0,   485,   454,   405,   247,   248,   249,   256,   255,
     252,   217,   218,   216,     0,     0,     0,     0,   394,     0,
       0,     0,     0,     0,   388,     0,     0,   393,   326,   328,
     329,   330,   332,   331,   325,   317,   318,   319,   320,   323,
     324,   327,     0,   394,   204,   163,    30,     0,     0,     0,
       0,     0,   425,   384,   390,   389,     0,     0,   347,     0,
     513,     0,   352,   200,   391,     0,   311,   202,   190,    30,
     166,     0,   486,    19,     0,   257,   392,     0,   349,   512,
       0,   394,     0,     0,   164,     0,     0,   395,   350,   348,
     354,   192,   312,   191,     0,    20,     0,    24,    25,    22,
       0,     0,   193,    12,    21,   487
};

  /* YYPGOTO[NTERM-NUM].  */
static const yytype_int16 yypgoto[] =
{
    -971,  -971,  1262,  -971,  -971,  -971,  -971,  -971,  -971,  -971,
    -971,  -971,   139,  -971,  -971,  1031,  -105,  -361,   861,  1053,
    1199,  -485,  -971,  -971,  -971,  -971,   614,  -971,  -971,  -971,
    -971,  -971,  -971,  -971,  -971,  -971,  -971,  -971,  1211,  -971,
    -971,  -971,   308,  -971,  -971,  -971,  -971,  -971,  -971,  -971,
    -971,  -971,  -971,  -971,  -971,  -971,  -971,  -971,  -971,  -971,
    -971,  -971,   801,  1063,  -971,  -971,  -971,  -971,   739,  -971,
    -971,  -971,   340,  -971,  -971,  -971,  -582,   319,   259,  -971,
    -971,   450,   281,  -971,  -971,  -971,  -971,  -971,   206,  -971,
    -971,  1106,  -971,  -971,  -971,  -971,  -971,  -971,  -971,  -971,
    -971,  -971,  -971,  -971,  -971,  -971,  -971,  -971,  -971,  -971,
    -971,  -971,  -971,  -971,  -618,  -971,  -971,  -971,  -971,  -971,
    -971,  -971,  -971,  -971,  -971,  -971,  -971,  -971,  -971,  -971,
    -971,  -971,  -971,  -971,  -971,  -971,  -971,  -971,   268,   558,
    -810,   244,  -971,  -876,  -971,  -932,   204,  -970,  -461,  -597,
    -971,  -971,  -971,   479,   877,  -219,  -173,  -387,   760,   290,
    -383,  -466,  -596,  -516,  -440,  -513,  -605,  -161,   -74,  -971,
     527,  -971,  -971,   786,  -971,  -971,  1020,  -169,   736,  -971,
    -464,  -971,  -971,  -971,  -971,  -971,  -172,  -971
};

  /* YYDEFGOTO[NTERM-NUM].  */
static const yytype_int16 yydefgoto[] =
{
      -1,     3,     4,     5,     6,    99,   100,   101,   102,   103,
    1080,  1131,   928,  1129,    12,   170,   104,   412,   105,   106,
     107,   108,   224,   109,   110,   774,   573,   777,   513,   514,
     515,   516,   111,   499,   861,   112,   113,   471,   114,   115,
     225,   763,   968,   969,  1099,  1076,   116,   631,   117,   118,
     242,   119,   628,   120,   410,   121,   366,   488,   623,   122,
     123,   124,   346,   338,   125,  1113,   126,  1126,   426,   599,
     619,   832,   845,   127,   365,   848,   546,   944,   724,   926,
     128,   265,   540,   129,   447,   302,   723,   915,  1040,   130,
     444,   292,   443,   717,   131,   132,   133,   134,   135,   136,
     137,   138,   139,   140,   745,   141,   142,   143,   144,   751,
     960,  1064,   145,   146,   531,   147,   148,   149,   150,   151,
     152,   153,   154,   155,  1108,  1119,   156,   157,   158,   159,
     160,   161,   162,   163,   164,   165,   166,   167,   307,   339,
     931,   993,   994,   996,  1085,  1048,  1058,  1088,   310,   580,
     589,   916,   825,   590,   248,   343,   282,   267,   420,   319,
     320,   555,   556,   303,   304,   293,   294,   389,   344,   859,
     640,   641,   642,   391,   392,   393,   283,   438,   237,   256,
     358,   720,   407,   408,   409,   611,   324,   325
};

  /* YYTABLE[YYPACT[STATE-NUM]] -- What to do in state STATE-NUM.  If
     positive, shift that token.  If negative, reduce the rule whose
     number is the opposite.  If YYTABLE_NINF, syntax error.  */
static const yytype_int16 yytable[] =
{
     239,   266,   371,   742,   236,   487,   659,   284,   657,   311,
     249,   313,   574,   607,   639,   517,   315,   327,   328,   518,
     330,   332,   333,   625,   626,   622,   340,   341,   342,   231,
     285,   348,   529,   379,   351,   963,   349,   350,   359,   352,
     353,   296,   285,   360,   322,   362,    16,   305,   367,   550,
     336,   775,  1060,   374,   375,   376,   377,   661,   257,   258,
     633,   597,   259,   260,   778,   617,   354,   403,   512,   322,
     355,   356,   379,   257,   258,   830,   843,   620,   855,   405,
     254,   257,   258,   565,   942,   613,   568,  1022,  1000,   363,
    1073,   784,   354,  1038,   257,   258,   355,   356,   429,   430,
    1127,   432,   316,  1098,   245,   308,   820,   609,   286,   781,
     231,   648,   610,   296,   309,   246,   247,   231,   649,   297,
     286,   650,   287,   578,   817,   317,   318,   781,   651,   347,
     231,   579,   652,   637,   287,   638,   654,  1039,   823,   653,
    1061,  1121,  1062,   655,   862,   824,   726,     9,   285,   865,
     966,   863,   967,   864,   494,   296,   866,   979,   380,   478,
     734,   943,  1111,   550,   390,   364,   406,   854,   550,   357,
       7,  1001,   255,   621,   849,   261,  1128,    11,  1094,     8,
    1063,   306,   867,    13,   337,    14,   262,   257,   258,   598,
      15,   297,   875,   618,   245,  1054,   168,   323,   261,   614,
     169,   262,   221,   831,   844,   246,   247,   263,   264,   262,
     171,   261,   245,   384,   385,   386,   387,   388,   574,   172,
     826,   230,   331,   246,   247,   586,   286,   587,   588,   173,
     263,   264,   174,   297,   232,   238,   288,   244,   530,   240,
     287,   519,   289,   263,   264,     1,     2,   298,   288,   233,
     234,    78,   535,   299,   289,   624,   604,   971,   538,   539,
     548,   549,   547,   771,   814,   770,   557,   760,   175,   972,
     560,   345,   243,   232,   176,   569,   570,   290,   291,   545,
     847,   571,   639,   881,   312,   235,   815,   177,   233,   234,
     178,   592,   268,   852,   895,   179,   596,   180,   493,   354,
     601,   602,   603,   355,   356,   608,   525,   502,   503,   181,
     504,   505,   506,   507,   508,   401,   386,   387,   388,   298,
     182,   245,   232,   658,   235,   299,   183,   380,   520,   184,
     233,   234,   246,   247,   634,   232,   185,   233,   234,   686,
     526,   245,   646,   687,   688,   629,   300,   301,   186,   537,
     233,   234,   246,   247,   288,   990,   991,   992,   656,   187,
     289,   298,   250,   251,   252,   253,   235,   299,   563,   624,
     671,   672,   567,   235,   624,   857,   858,   188,   660,   259,
     260,   577,   382,   290,   291,   189,   235,   190,   300,   301,
     191,   718,   192,   689,   553,   554,   850,  1067,  1068,   553,
     554,   326,   193,   194,   231,   383,   329,   690,   691,   692,
     693,   195,   694,   196,   695,   197,   257,   258,   999,   198,
     199,  1002,   200,   394,   201,   395,   202,   203,   204,   205,
     396,   397,   398,   206,   207,   390,   208,   209,   369,   696,
     697,   698,   210,   699,   211,   212,   700,   701,   702,   703,
     704,   213,   705,   214,   384,   385,   386,   387,   388,   384,
     385,   386,   387,   388,   215,   630,   217,   550,   218,   219,
     220,   551,  1041,   226,   227,   228,   706,   321,   707,   708,
     229,   257,   258,   241,   373,   334,   335,   709,   710,   711,
     712,   713,   714,   773,   919,   871,   345,  1065,   872,   372,
     970,  1066,  1025,   381,  1115,   785,   378,   715,   368,   788,
     414,   411,   791,   792,  1031,   415,   416,  1032,   417,   418,
     422,   716,   419,   439,   920,   921,   421,   835,   399,   423,
     404,   922,   424,   261,   425,   431,  1092,   433,   434,   435,
     923,   437,   442,   440,   262,   257,   258,   441,   924,   384,
     385,   386,   387,   388,   445,   446,  -231,   762,   448,   427,
     428,   449,   450,   451,  1079,   263,   264,   -18,   384,   385,
     386,   387,   388,   452,   925,   453,   880,   839,   772,   454,
     884,   455,   456,   887,   888,   889,   457,   837,   892,   893,
     894,  1117,   679,   422,  1120,   235,   400,   643,   261,   458,
     896,   897,   786,   787,   856,   459,   790,   466,   460,   401,
     461,   462,   463,   402,   464,   245,   465,   467,   468,   422,
     386,   387,   388,   469,   233,   234,   246,   247,   470,   472,
     263,   264,   317,   318,   873,   473,   474,   898,   899,   475,
     476,   900,   477,   479,   890,   480,   384,   385,   386,   387,
     388,   918,   481,   482,   483,   269,   270,   271,   272,   273,
     235,   274,   261,   275,   276,   277,   278,   279,   484,   492,
     257,   258,   901,   552,   644,   902,   903,   904,   905,   906,
     907,   908,   909,   910,   911,   912,   913,   914,   384,   385,
     386,   387,   388,   485,   263,   264,   257,   258,   553,   554,
     500,   486,   489,   501,   281,   490,   491,   495,   496,   869,
     384,   385,   386,   387,   388,   497,   498,   663,   509,   879,
     510,   511,  1006,   384,   385,   386,   387,   388,   521,   522,
     523,   528,   245,   384,   978,   386,   387,   388,   532,   980,
     533,   534,   536,   246,   247,   541,   542,  1118,   543,   544,
     558,   669,  1123,   947,   384,   385,   386,   387,   388,   559,
     948,   561,  1035,  1036,  1037,  1132,   384,   385,   386,   387,
     388,   562,   564,   566,   572,   575,   949,   576,   582,   581,
     269,   270,   271,   272,   273,   583,   274,   261,   275,   276,
     277,   278,   279,   950,   584,   585,   591,   593,   280,   976,
     384,   385,   386,   387,   388,   594,   269,   270,   271,   272,
     273,   685,   274,   261,   275,   276,   277,   278,   279,   263,
     264,   595,   615,   600,   280,   605,   612,   616,  -180,   281,
     627,   632,   635,   647,   951,   952,   953,   954,   636,   955,
     956,   957,   958,   959,  1069,   263,   264,   662,   664,   665,
     667,   668,   673,   675,   674,   281,   676,   677,    16,   678,
     384,   385,   386,   387,   388,   680,    17,   681,   682,    18,
      19,    20,    21,    22,    23,    24,    25,    26,    27,    28,
      29,    30,   722,   683,   684,    31,    32,    33,    34,    35,
     721,    36,   545,   257,   258,   729,    37,    38,    39,    40,
      41,    42,    43,    44,  1033,    45,    46,    47,   728,    48,
     876,    49,    50,    51,    52,    53,    54,   725,   726,   727,
      55,    56,   730,    57,   731,   732,   733,   734,   735,   737,
      58,   384,   385,   386,   387,   388,   738,   736,   740,   747,
     739,   741,   753,  1125,   384,   385,   386,   387,   388,   743,
     744,    59,   746,   748,   749,   757,   750,  1134,   752,   384,
     385,   386,   387,   388,   754,   755,   756,   679,    60,   758,
     759,  1100,   761,   764,    61,   765,   767,    62,    63,   766,
      64,   769,    65,   768,   257,   258,   316,   779,    66,   780,
     545,    67,    68,   781,  1114,   782,   783,    69,    70,   789,
      71,   794,    72,   269,   270,   271,   272,   273,   793,   274,
     261,   275,   276,   277,   278,   279,   795,   796,   797,   798,
     812,   280,   799,   800,   801,   802,   803,   804,   257,   258,
     805,   806,   807,   808,    73,    74,    75,    76,   809,    77,
     810,   811,   263,   264,   813,    82,   816,    84,   817,    86,
     819,    88,   281,    90,   821,    92,   820,    94,   822,    96,
     827,    98,   828,    78,   829,   833,   834,   836,   838,    79,
      80,   840,   841,   842,   314,   846,    81,    82,    83,    84,
      85,    86,    87,    88,    89,    90,    91,    92,    93,    94,
      95,    96,    97,    98,   269,   270,   271,   272,   273,   853,
     274,   261,   275,   276,   277,   278,   279,    80,   851,   868,
     870,   874,   280,    81,    82,    83,    84,    85,    86,    87,
      88,    89,    90,    91,    92,    93,    94,    95,    96,    97,
      98,   882,   877,   263,   264,   878,   883,   886,   269,   270,
     271,   272,   273,   281,   274,   261,   275,   276,   277,   278,
     279,   222,   885,   927,   891,   929,   280,    81,   930,    83,
     932,    85,   933,    87,   934,    89,   935,    91,   936,    93,
     937,    95,   938,    97,   940,   941,   945,   263,   264,   961,
     946,   962,   975,   974,   973,   988,   964,   281,   965,   989,
     977,   981,   982,   983,   995,   984,   985,   986,   987,   997,
     998,  1003,  1004,  1008,  1007,  1009,  1010,  1011,  1012,  1013,
    1024,  1014,  1015,  1016,  1017,  1018,  1019,  1021,  1034,  1020,
    1042,  1026,  1029,  1043,  1044,  1045,  1030,  1046,  1027,  1047,
    1049,  1050,  1051,  1059,  1052,  1057,  1055,  1056,  1071,  1072,
    1075,  1077,  1078,  1082,  1084,  1086,  1087,  1089,  1090,  1091,
    1093,  1095,  1096,  1107,  1097,  1101,  1103,  1102,  1110,  1104,
    1081,  1122,  1109,  1112,  1116,  1124,  1130,    10,  1133,   223,
    1135,   524,   776,   370,   216,   413,   361,  1028,   606,   670,
    1005,   939,  1074,  1023,   295,  1053,  1070,  1105,   818,  1083,
    1106,   917,   527,   666,   860,   645,     0,     0,   436,     0,
     719
};

static const yytype_int16 yycheck[] =
{
     105,   174,   221,   600,    78,   366,   522,   176,   521,   182,
     171,   184,   452,   477,   499,   402,   185,   190,   191,   402,
     193,   194,   194,   489,   490,   486,   198,   199,   200,     4,
       3,   204,     3,     8,   207,   845,   205,   206,   211,   208,
     209,     3,     3,   212,    56,   214,     6,    81,   217,     3,
      81,   656,     4,   226,   227,   228,   229,   523,    81,    82,
      81,    81,    85,    86,   660,    81,     4,   240,     4,    56,
       8,     9,     8,    81,    82,    81,    81,     4,    81,   240,
       4,    81,    82,   444,     4,    65,   447,   963,    89,     5,
    1022,   673,     4,   171,    81,    82,     8,     9,   271,   272,
       5,   274,   209,  1073,   215,    72,   207,    76,    81,   210,
       4,   207,    81,     3,    81,   226,   227,     4,   214,    81,
      81,   207,    95,    73,   207,   232,   233,   210,   214,   203,
       4,    81,   207,   135,    95,   137,   207,   215,    81,   214,
      92,  1111,    94,   214,   207,    88,   207,     0,     3,   207,
     149,   214,   151,   214,   373,     3,   214,   208,   232,   207,
     211,    81,  1094,     3,   238,    81,   240,   764,     3,    81,
     208,   172,    96,   100,   756,   198,    81,   118,  1054,   208,
     132,   215,   779,   215,   215,   215,   209,    81,    82,   209,
     208,    81,   789,   209,   215,  1005,   207,   209,   198,   179,
     119,   209,   211,   209,   209,   226,   227,   230,   231,   209,
     208,   198,   215,   261,   262,   263,   264,   265,   658,   208,
     736,   217,   209,   226,   227,    81,    81,    83,    84,   208,
     230,   231,   208,    81,   209,   209,   209,   207,   209,   266,
      95,   402,   215,   230,   231,    15,    16,   209,   209,   224,
     225,   211,   421,   215,   215,   209,   475,   862,    86,    87,
     433,   434,   431,   650,   725,   648,   435,   628,   208,   865,
     439,   209,     3,   209,   208,   448,   449,   238,   239,    80,
      81,   450,   767,   796,     4,   260,   726,   208,   224,   225,
     208,   464,   100,   759,   810,   208,   468,   208,   372,     4,
     472,   473,   474,     8,     9,   478,   411,   381,   382,   208,
     384,   385,   386,   387,   388,   209,   263,   264,   265,   209,
     208,   215,   209,   213,   260,   215,   208,   401,   402,   208,
     224,   225,   226,   227,   495,   209,   208,   224,   225,     4,
     414,   215,   511,     8,     9,   147,   236,   237,   208,   423,
     224,   225,   226,   227,   209,    41,    42,    43,   213,   208,
     215,   209,    10,    11,    12,    13,   260,   215,   442,   209,
     543,   544,   446,   260,   209,   220,   221,   208,   213,    85,
      86,   455,   189,   238,   239,   208,   260,   208,   236,   237,
     208,   564,   208,    58,   234,   235,   757,  1015,  1016,   234,
     235,   215,   208,   208,     4,   212,    93,    72,    73,    74,
      75,   208,    77,   208,    79,   208,    81,    82,   934,   208,
     208,   937,   208,    23,   208,    25,   208,   208,   208,   208,
      30,    31,    32,   208,   208,   509,   208,   208,   137,   104,
     105,   106,   208,   108,   208,   208,   111,   112,   113,   114,
     115,   208,   117,   208,   261,   262,   263,   264,   265,   261,
     262,   263,   264,   265,   208,   267,   208,     3,   208,   208,
     208,     7,   985,   208,   208,   208,   141,   187,   143,   144,
     208,    81,    82,   208,   211,   195,   196,   152,   153,   154,
     155,   156,   157,   654,    77,   207,   209,  1013,   210,   266,
     861,  1014,   966,   211,  1101,   674,   209,   172,   218,   678,
     211,   213,   681,   682,   207,   267,   207,   210,   207,   207,
       4,   186,   206,   269,   107,   108,   209,   746,   128,   211,
     240,   114,   207,   198,   207,   209,  1052,   209,   209,   209,
     123,   209,   211,   207,   209,    81,    82,     3,   131,   261,
     262,   263,   264,   265,     3,   211,   213,   631,   207,   269,
     270,   207,   207,   207,   207,   230,   231,   210,   261,   262,
     263,   264,   265,   207,   157,   207,   795,   750,   652,     4,
     799,   211,   207,   802,   803,   804,   207,   748,   807,   808,
     809,  1107,     3,     4,  1110,   260,   196,   212,   198,   209,
      83,    84,   676,   677,   765,     4,   680,     4,   207,   209,
     207,   207,   207,   213,   207,   215,   207,   207,   207,     4,
     263,   264,   265,   207,   224,   225,   226,   227,     6,   207,
     230,   231,   232,   233,   207,   207,   207,   120,   121,   207,
       4,   124,   207,   207,   805,   207,   261,   262,   263,   264,
     265,   812,   207,   207,   207,   191,   192,   193,   194,   195,
     260,   197,   198,   199,   200,   201,   202,   203,   207,   212,
      81,    82,   155,   209,   212,   158,   159,   160,   161,   162,
     163,   164,   165,   166,   167,   168,   169,   170,   261,   262,
     263,   264,   265,   207,   230,   231,    81,    82,   234,   235,
     210,   207,   207,   210,   240,   207,   207,   207,   207,   783,
     261,   262,   263,   264,   265,   207,   207,   212,   207,   793,
     210,   208,   941,   261,   262,   263,   264,   265,   208,   208,
     208,     3,   215,   261,   262,   263,   264,   265,     3,   900,
     207,   212,   207,   226,   227,     5,   216,  1108,   207,   207,
     204,   212,  1113,    91,   261,   262,   263,   264,   265,   210,
      98,    96,   981,   982,   983,  1126,   261,   262,   263,   264,
     265,   207,   207,   207,    80,    80,   114,   207,     4,    38,
     191,   192,   193,   194,   195,   207,   197,   198,   199,   200,
     201,   202,   203,   131,    38,    40,   172,    90,   209,   873,
     261,   262,   263,   264,   265,   207,   191,   192,   193,   194,
     195,   212,   197,   198,   199,   200,   201,   202,   203,   230,
     231,    90,   176,   207,   209,   207,    94,   100,   127,   240,
     138,   212,   133,   207,   172,   173,   174,   175,   134,   177,
     178,   179,   180,   181,  1017,   230,   231,   214,     3,   207,
     210,     4,   268,   210,   207,   240,   207,   207,     6,   207,
     261,   262,   263,   264,   265,   211,    14,   207,   207,    17,
      18,    19,    20,    21,    22,    23,    24,    25,    26,    27,
      28,    29,   212,   207,   215,    33,    34,    35,    36,    37,
     215,    39,    80,    81,    82,   212,    44,    45,    46,    47,
      48,    49,    50,    51,   978,    53,    54,    55,     4,    57,
     212,    59,    60,    61,    62,    63,    64,   207,   207,   207,
      68,    69,   207,    71,   207,     4,   207,   211,   207,   207,
      78,   261,   262,   263,   264,   265,     4,   208,     4,    80,
     207,   207,     4,  1116,   261,   262,   263,   264,   265,   207,
     207,    99,   207,   207,   207,   127,   208,  1130,   207,   261,
     262,   263,   264,   265,   207,   207,   207,     3,   116,   207,
     207,  1076,   267,   207,   122,   207,   207,   125,   126,   208,
     128,     4,   130,   210,    81,    82,   209,   207,   136,     3,
      80,   139,   140,   210,  1099,   216,   207,   145,   146,   207,
     148,   210,   150,   191,   192,   193,   194,   195,   209,   197,
     198,   199,   200,   201,   202,   203,   208,   208,   208,   208,
     207,   209,   208,   208,   208,   208,   208,   208,    81,    82,
     208,   208,   208,   208,   182,   183,   184,   185,   208,   187,
     208,   210,   230,   231,   207,   225,     5,   227,   207,   229,
       4,   231,   240,   233,   172,   235,   207,   237,     4,   239,
     172,   241,    90,   211,   207,   215,    90,   207,    66,   217,
     218,   207,    97,   207,   171,    97,   224,   225,   226,   227,
     228,   229,   230,   231,   232,   233,   234,   235,   236,   237,
     238,   239,   240,   241,   191,   192,   193,   194,   195,   213,
     197,   198,   199,   200,   201,   202,   203,   218,    97,   207,
     210,   210,   209,   224,   225,   226,   227,   228,   229,   230,
     231,   232,   233,   234,   235,   236,   237,   238,   239,   240,
     241,   103,   210,   230,   231,   210,   102,   110,   191,   192,
     193,   194,   195,   240,   197,   198,   199,   200,   201,   202,
     203,   218,   109,   207,   142,     4,   209,   224,   207,   226,
     207,   228,     4,   230,   208,   232,   212,   234,   207,   236,
     208,   238,     4,   240,   207,   207,   207,   230,   231,     4,
     207,     4,    41,     5,   210,     3,   207,   240,   207,   207,
     210,   208,   208,   208,   101,   208,   208,   208,   208,     4,
     207,   210,   207,    67,   210,    92,   208,   208,   208,   208,
       7,   208,   208,   208,   208,   208,   208,   207,    81,   210,
       4,   208,   207,   132,     4,   207,   210,   208,   214,    98,
     207,     4,   207,     4,   208,    97,   207,   207,     4,     4,
     208,     5,    41,   210,   100,   208,   172,     4,   210,   172,
     210,    92,    91,   208,   210,   207,   205,   210,   208,   210,
     268,    91,   210,   207,   207,   207,   207,     5,  1129,    70,
     210,   410,   658,   220,    63,   244,   213,   969,   477,   540,
     940,   831,  1023,   964,   178,  1004,  1018,  1081,   730,  1045,
    1086,   812,   415,   533,   767,   509,    -1,    -1,   278,    -1,
     564
};

  /* YYSTOS[STATE-NUM] -- The (internal number of the) accessing
     symbol of state STATE-NUM.  */
static const yytype_int16 yystos[] =
{
       0,    15,    16,   271,   272,   273,   274,   208,   208,     0,
     272,   118,   284,   215,   215,   208,     6,    14,    17,    18,
      19,    20,    21,    22,    23,    24,    25,    26,    27,    28,
      29,    33,    34,    35,    36,    37,    39,    44,    45,    46,
      47,    48,    49,    50,    51,    53,    54,    55,    57,    59,
      60,    61,    62,    63,    64,    68,    69,    71,    78,    99,
     116,   122,   125,   126,   128,   130,   136,   139,   140,   145,
     146,   148,   150,   182,   183,   184,   185,   187,   211,   217,
     218,   224,   225,   226,   227,   228,   229,   230,   231,   232,
     233,   234,   235,   236,   237,   238,   239,   240,   241,   275,
     276,   277,   278,   279,   286,   288,   289,   290,   291,   293,
     294,   302,   305,   306,   308,   309,   316,   318,   319,   321,
     323,   325,   329,   330,   331,   334,   336,   343,   350,   353,
     359,   364,   365,   366,   367,   368,   369,   370,   371,   372,
     373,   375,   376,   377,   378,   382,   383,   385,   386,   387,
     388,   389,   390,   391,   392,   393,   396,   397,   398,   399,
     400,   401,   402,   403,   404,   405,   406,   407,   207,   119,
     285,   208,   208,   208,   208,   208,   208,   208,   208,   208,
     208,   208,   208,   208,   208,   208,   208,   208,   208,   208,
     208,   208,   208,   208,   208,   208,   208,   208,   208,   208,
     208,   208,   208,   208,   208,   208,   208,   208,   208,   208,
     208,   208,   208,   208,   208,   208,   308,   208,   208,   208,
     208,   211,   218,   290,   292,   310,   208,   208,   208,   208,
     217,     4,   209,   224,   225,   260,   438,   448,   209,   286,
     266,   208,   320,     3,   207,   215,   226,   227,   424,   437,
      10,    11,    12,    13,     4,    96,   449,    81,    82,    85,
      86,   198,   209,   230,   231,   351,   426,   427,   100,   191,
     192,   193,   194,   195,   197,   199,   200,   201,   202,   203,
     209,   240,   426,   446,   447,     3,    81,    95,   209,   215,
     238,   239,   361,   435,   436,   361,     3,    81,   209,   215,
     236,   237,   355,   433,   434,    81,   215,   408,    72,    81,
     418,   426,     4,   426,   171,   447,   209,   232,   233,   429,
     430,   429,    56,   209,   456,   457,   215,   426,   426,    93,
     426,   209,   426,   456,   429,   429,    81,   215,   333,   409,
     456,   456,   456,   425,   438,   209,   332,   438,   426,   447,
     447,   426,   447,   447,     4,     8,     9,    81,   450,   426,
     447,   333,   447,     5,    81,   344,   326,   447,   429,   137,
     289,   425,   266,   211,   426,   426,   426,   426,   209,     8,
     438,   211,   189,   212,   261,   262,   263,   264,   265,   437,
     438,   443,   444,   445,    23,    25,    30,    31,    32,   128,
     196,   209,   213,   426,   429,   437,   438,   452,   453,   454,
     324,   213,   287,   285,   211,   267,   207,   207,   207,   206,
     428,   209,     4,   211,   207,   207,   338,   429,   429,   426,
     426,   209,   426,   209,   209,   209,   446,   209,   447,   269,
     207,     3,   211,   362,   360,     3,   211,   354,   207,   207,
     207,   207,   207,   207,     4,   211,   207,   207,   209,     4,
     207,   207,   207,   207,   207,   207,     4,   207,   207,   207,
       6,   307,   207,   207,   207,   207,     4,   207,   207,   207,
     207,   207,   207,   207,   207,   207,   207,   287,   327,   207,
     207,   207,   212,   438,   425,   207,   207,   207,   207,   303,
     210,   210,   438,   438,   438,   438,   438,   438,   438,   207,
     210,   208,     4,   298,   299,   300,   301,   427,   430,   437,
     438,   208,   208,   208,   288,   286,   438,   424,     3,     3,
     209,   384,     3,   207,   212,   447,   207,   438,    86,    87,
     352,     5,   216,   207,   207,    80,   346,   447,   426,   426,
       3,     7,   209,   234,   235,   431,   432,   447,   204,   210,
     447,    96,   207,   438,   207,   287,   207,   438,   287,   426,
     426,   447,    80,   296,   434,    80,   207,   438,    73,    81,
     419,    38,     4,   207,    38,    40,    81,    83,    84,   420,
     423,   172,   426,    90,   207,    90,   456,    81,   209,   339,
     207,   456,   456,   456,   425,   207,   332,   450,   426,    76,
      81,   455,    94,    65,   179,   176,   100,    81,   209,   340,
       4,   100,   418,   328,   209,   431,   431,   138,   322,   147,
     267,   317,   212,    81,   437,   133,   134,   135,   137,   291,
     440,   441,   442,   212,   212,   443,   447,   207,   207,   214,
     207,   214,   207,   214,   207,   214,   213,   435,   213,   433,
     213,   431,   214,   212,     3,   207,   428,   210,     4,   212,
     338,   426,   426,   268,   207,   210,   207,   207,   207,     3,
     211,   207,   207,   207,   215,   212,     4,     8,     9,    58,
      72,    73,    74,    75,    77,    79,   104,   105,   106,   108,
     111,   112,   113,   114,   115,   117,   141,   143,   144,   152,
     153,   154,   155,   156,   157,   172,   186,   363,   426,   448,
     451,   215,   212,   356,   348,   207,   207,   207,     4,   212,
     207,   207,     4,   207,   211,   207,   208,   207,     4,   207,
       4,   207,   419,   207,   207,   374,   207,    80,   207,   207,
     208,   379,   207,     4,   207,   207,   207,   127,   207,   207,
     287,   267,   438,   311,   207,   207,   208,   207,   210,     4,
     430,   427,   438,   437,   295,   436,   296,   297,   432,   207,
       3,   210,   216,   207,   346,   447,   438,   438,   447,   207,
     438,   447,   447,   209,   210,   208,   208,   208,   208,   208,
     208,   208,   208,   208,   208,   208,   208,   208,   208,   208,
     208,   210,   207,   207,   418,   434,     5,   207,   409,     4,
     207,   172,     4,    81,    88,   422,   433,   172,    90,   207,
      81,   209,   341,   215,    90,   425,   207,   437,    66,   426,
     207,    97,   207,    81,   209,   342,    97,    81,   345,   346,
     287,    97,   431,   213,   419,    81,   437,   220,   221,   439,
     440,   304,   207,   214,   214,   207,   214,   419,   207,   438,
     210,   207,   210,   207,   210,   419,   212,   210,   210,   438,
     425,   435,   103,   102,   425,   109,   110,   425,   425,   425,
     437,   142,   425,   425,   425,   433,    83,    84,   120,   121,
     124,   155,   158,   159,   160,   161,   162,   163,   164,   165,
     166,   167,   168,   169,   170,   357,   421,   423,   437,    77,
     107,   108,   114,   123,   131,   157,   349,   207,   282,     4,
     207,   410,   207,     4,   208,   212,   207,   208,     4,   351,
     207,   207,     4,    81,   347,   207,   207,    91,    98,   114,
     131,   172,   173,   174,   175,   177,   178,   179,   180,   181,
     380,     4,     4,   410,   207,   207,   149,   151,   312,   313,
     287,   436,   432,   210,     5,    41,   438,   210,   262,   208,
     437,   208,   208,   208,   208,   208,   208,   208,     3,   207,
      41,    42,    43,   411,   412,   101,   413,     4,   207,   433,
      89,   172,   433,   210,   207,   342,   425,   210,    67,    92,
     208,   208,   208,   208,   208,   208,   208,   208,   208,   208,
     210,   207,   413,   347,     7,   450,   208,   214,   312,   207,
     210,   207,   210,   438,    81,   425,   425,   425,   171,   215,
     358,   435,     4,   132,     4,   207,   208,    98,   415,   207,
       4,   207,   208,   352,   410,   207,   207,    97,   416,     4,
       4,    92,    94,   132,   381,   433,   435,   384,   384,   426,
     408,     4,     4,   415,   348,   208,   315,     5,    41,   207,
     280,   268,   210,   411,   100,   414,   208,   172,   417,     4,
     210,   172,   433,   210,   413,    92,    91,   210,   417,   314,
     286,   207,   210,   205,   210,   358,   416,   208,   394,   210,
     208,   415,   207,   335,   286,   419,   207,   433,   287,   395,
     433,   417,    91,   287,   207,   426,   337,     5,    81,   283,
     207,   281,   287,   282,   426,   210
};

  /* YYR1[YYN] -- Symbol number of symbol that rule YYN derives.  */
static const yytype_int16 yyr1[] =
{
       0,   270,   271,   271,   272,   272,   273,   274,   274,   275,
     275,   275,   275,   276,   277,   277,   278,   279,   280,   280,
     281,   281,   282,   282,   283,   283,   284,   284,   285,   285,
     286,   286,   287,   288,   288,   288,   288,   288,   288,   288,
     288,   288,   288,   288,   288,   288,   288,   288,   288,   288,
     288,   288,   288,   288,   288,   288,   288,   288,   288,   288,
     288,   288,   288,   288,   288,   288,   288,   288,   288,   288,
     288,   288,   288,   288,   288,   288,   288,   288,   288,   288,
     288,   288,   288,   288,   288,   288,   288,   288,   288,   288,
     288,   288,   288,   288,   288,   289,   289,   289,   289,   289,
     289,   289,   289,   289,   290,   290,   290,   290,   290,   290,
     290,   290,   290,   291,   291,   291,   292,   292,   293,   294,
     294,   294,   294,   294,   294,   294,   294,   294,   294,   294,
     294,   294,   294,   294,   295,   295,   296,   296,   297,   297,
     298,   298,   299,   299,   300,   300,   301,   301,   303,   304,
     302,   305,   306,   307,   307,   308,   308,   308,   310,   311,
     309,   312,   312,   314,   313,   315,   313,   316,   317,   317,
     318,   320,   319,   322,   321,   324,   323,   326,   325,   327,
     328,   327,   329,   330,   330,   330,   331,   331,   332,   333,
     335,   334,   337,   336,   338,   338,   339,   339,   340,   340,
     341,   341,   342,   342,   343,   343,   344,   344,   345,   345,
     346,   346,   347,   347,   348,   348,   349,   349,   349,   349,
     349,   349,   349,   350,   350,   350,   351,   351,   352,   352,
     353,   354,   353,   355,   356,   356,   357,   357,   357,   357,
     357,   357,   357,   357,   357,   357,   357,   357,   357,   357,
     357,   357,   357,   357,   357,   358,   358,   358,   359,   360,
     359,   361,   362,   362,   363,   363,   363,   363,   363,   363,
     363,   363,   363,   363,   363,   363,   363,   363,   363,   363,
     363,   363,   363,   363,   363,   363,   363,   363,   363,   363,
     363,   363,   363,   363,   364,   365,   366,   366,   367,   367,
     367,   368,   369,   370,   371,   372,   373,   374,   374,   375,
     376,   377,   377,   378,   379,   379,   380,   380,   380,   380,
     380,   380,   380,   380,   380,   380,   380,   380,   380,   381,
     381,   381,   381,   382,   383,   384,   384,   385,   386,   386,
     387,   388,   389,   389,   390,   391,   392,   394,   393,   395,
     395,   396,   396,   396,   396,   397,   397,   398,   399,   399,
     400,   400,   401,   401,   401,   402,   402,   403,   403,   403,
     404,   404,   404,   405,   406,   407,   407,   408,   408,   409,
     409,   410,   410,   411,   411,   412,   412,   412,   413,   413,
     414,   415,   415,   416,   417,   417,   418,   418,   419,   419,
     420,   420,   420,   421,   421,   421,   422,   422,   423,   424,
     424,   424,   425,   426,   426,   426,   426,   427,   427,   427,
     428,   428,   429,   429,   429,   430,   431,   431,   431,   432,
     432,   433,   433,   433,   434,   434,   434,   434,   435,   435,
     435,   436,   436,   436,   436,   436,   437,   437,   438,   438,
     438,   438,   438,   438,   438,   438,   438,   438,   438,   439,
     439,   440,   441,   441,   442,   442,   443,   443,   444,   444,
     445,   445,   446,   446,   446,   446,   446,   446,   446,   446,
     446,   446,   446,   446,   446,   446,   446,   446,   446,   446,
     446,   447,   447,   448,   449,   449,   450,   450,   450,   451,
     451,   451,   451,   452,   452,   453,   453,   454,   454,   455,
     455,   456,   456,   457
};

  /* YYR2[YYN] -- Number of symbols on the right hand side of rule YYN.  */
static const yytype_int8 yyr2[] =
{
       0,     2,     0,     1,     1,     2,     3,     3,     5,     5,
       5,     3,    16,     3,     5,     3,     5,     3,     0,     2,
       0,     2,     0,     2,     1,     1,     0,     3,     3,     1,
       0,     2,     3,     1,     1,     1,     1,     1,     1,     1,
       1,     1,     1,     1,     1,     1,     1,     1,     1,     1,
       1,     1,     1,     1,     1,     1,     1,     1,     1,     1,
       1,     1,     1,     1,     1,     1,     1,     1,     1,     1,
       1,     1,     1,     1,     1,     1,     1,     1,     1,     1,
       1,     1,     1,     1,     1,     1,     1,     1,     1,     1,
       1,     1,     1,     1,     1,     1,     1,     1,     1,     1,
       1,     1,     1,     1,     1,     1,     1,     1,     1,     1,
       1,     1,     1,     1,     1,     1,     1,     1,     3,     3,
       5,     3,     5,     5,     5,     3,     3,     5,     5,     5,
       7,     7,     7,     5,     1,     3,     1,     3,     1,     3,
       1,     3,     1,     3,     1,     3,     1,     3,     0,     0,
       8,     4,     1,     0,     1,     1,     5,     3,     0,     0,
       9,     0,     2,     0,     5,     0,     4,     1,     2,     1,
       6,     0,     3,     0,     6,     0,     4,     0,     4,     1,
       0,     4,     3,     1,     3,     3,     5,     5,     7,     4,
       0,    13,     0,    15,     0,     2,     5,     1,     5,     1,
       5,     1,     5,     1,    10,     6,     1,     1,     1,     1,
       1,     3,     1,     1,     0,     3,     3,     3,     3,     1,
       1,     1,     1,     1,     7,     5,     1,     1,     1,     1,
       3,     0,     5,     4,     0,     3,     1,     1,     1,     1,
       2,     1,     1,     1,     1,     1,     1,     3,     3,     3,
       1,     1,     3,     1,     1,     1,     1,     3,     3,     0,
       5,     2,     0,     3,     1,     3,     1,     3,     3,     1,
       1,     3,     1,     1,     1,     3,     1,     1,     1,     3,
       3,     3,     3,     3,     3,     3,     3,     1,     1,     1,
       1,     3,     3,     3,     5,     7,     5,     8,     1,     3,
       3,     5,     5,     7,     7,     6,     5,     0,     2,     5,
       3,    11,    13,     6,     0,     3,     1,     3,     3,     3,
       3,     1,     1,     3,     3,     3,     3,     3,     3,     1,
       1,     1,     1,     3,     3,     1,     5,     9,     5,     7,
       3,     3,     5,     9,     5,     5,     5,     0,    13,     0,
       1,     7,    11,     9,    13,     3,     7,     7,     5,     7,
       5,     9,     5,     5,     3,     7,     3,     7,     7,     3,
       5,     5,     3,     5,     7,     9,     1,     1,     1,     1,
       1,     0,     2,     1,     3,     1,     1,     1,     0,     3,
       1,     0,     3,     1,     0,     3,     1,     1,     1,     1,
       1,     1,     1,     1,     1,     3,     1,     1,     4,     1,
       1,     4,     1,     1,     4,     1,     4,     5,     1,     3,
       1,     3,     1,     1,     4,     9,     1,     1,     4,     1,
       5,     1,     1,     4,     1,     1,     5,     1,     1,     1,
       4,     1,     1,     5,     1,     1,     1,     3,     1,     1,
       3,     1,     4,     3,     3,     3,     3,     3,     3,     1,
       1,     3,     1,     3,     0,     1,     1,     1,     1,     3,
       0,     1,     1,     2,     2,     4,     6,     4,     6,     6,
       6,     6,     2,     6,     8,     8,    10,    14,     2,     1,
       3,     1,     3,     1,     1,     1,     1,     1,     1,     1,
       1,     1,     1,     1,     1,     1,     1,     1,     1,     1,
       1,     1,    10,     9
};


#define yyerrok         (yyerrstatus = 0)
#define yyclearin       (yychar = YYEMPTY)
#define YYEMPTY         (-2)
#define YYEOF           0

#define YYACCEPT        goto yyacceptlab
#define YYABORT         goto yyabortlab
#define YYERROR         goto yyerrorlab


#define YYRECOVERING()  (!!yyerrstatus)

#define YYBACKUP(Token, Value)                                    \
  do                                                              \
    if (yychar == YYEMPTY)                                        \
      {                                                           \
        yychar = (Token);                                         \
        yylval = (Value);                                         \
        YYPOPSTACK (yylen);                                       \
        yystate = *yyssp;                                         \
        goto yybackup;                                            \
      }                                                           \
    else                                                          \
      {                                                           \
        yyerror (YY_("syntax error: cannot back up")); \
        YYERROR;                                                  \
      }                                                           \
  while (0)

/* Error token number */
#define YYTERROR        1
#define YYERRCODE       256



/* Enable debugging if requested.  */
#if YYDEBUG

# ifndef YYFPRINTF
#  include <stdio.h> /* INFRINGES ON USER NAME SPACE */
#  define YYFPRINTF fprintf
# endif

# define YYDPRINTF(Args)                        \
do {                                            \
  if (yydebug)                                  \
    YYFPRINTF Args;                             \
} while (0)

/* This macro is provided for backward compatibility. */
#ifndef YY_LOCATION_PRINT
# define YY_LOCATION_PRINT(File, Loc) ((void) 0)
#endif


# define YY_SYMBOL_PRINT(Title, Type, Value, Location)                    \
do {                                                                      \
  if (yydebug)                                                            \
    {                                                                     \
      YYFPRINTF (stderr, "%s ", Title);                                   \
      yy_symbol_print (stderr,                                            \
                  Type, Value); \
      YYFPRINTF (stderr, "\n");                                           \
    }                                                                     \
} while (0)


/*-----------------------------------.
| Print this symbol's value on YYO.  |
`-----------------------------------*/

static void
yy_symbol_value_print (FILE *yyo, int yytype, YYSTYPE const * const yyvaluep)
{
  FILE *yyoutput = yyo;
  YYUSE (yyoutput);
  if (!yyvaluep)
    return;
# ifdef YYPRINT
  if (yytype < YYNTOKENS)
    YYPRINT (yyo, yytoknum[yytype], *yyvaluep);
# endif
  YY_IGNORE_MAYBE_UNINITIALIZED_BEGIN
  YYUSE (yytype);
  YY_IGNORE_MAYBE_UNINITIALIZED_END
}


/*---------------------------.
| Print this symbol on YYO.  |
`---------------------------*/

static void
yy_symbol_print (FILE *yyo, int yytype, YYSTYPE const * const yyvaluep)
{
  YYFPRINTF (yyo, "%s %s (",
             yytype < YYNTOKENS ? "token" : "nterm", yytname[yytype]);

  yy_symbol_value_print (yyo, yytype, yyvaluep);
  YYFPRINTF (yyo, ")");
}

/*------------------------------------------------------------------.
| yy_stack_print -- Print the state stack from its BOTTOM up to its |
| TOP (included).                                                   |
`------------------------------------------------------------------*/

static void
yy_stack_print (yy_state_t *yybottom, yy_state_t *yytop)
{
  YYFPRINTF (stderr, "Stack now");
  for (; yybottom <= yytop; yybottom++)
    {
      int yybot = *yybottom;
      YYFPRINTF (stderr, " %d", yybot);
    }
  YYFPRINTF (stderr, "\n");
}

# define YY_STACK_PRINT(Bottom, Top)                            \
do {                                                            \
  if (yydebug)                                                  \
    yy_stack_print ((Bottom), (Top));                           \
} while (0)


/*------------------------------------------------.
| Report that the YYRULE is going to be reduced.  |
`------------------------------------------------*/

static void
yy_reduce_print (yy_state_t *yyssp, YYSTYPE *yyvsp, int yyrule)
{
  int yylno = yyrline[yyrule];
  int yynrhs = yyr2[yyrule];
  int yyi;
  YYFPRINTF (stderr, "Reducing stack by rule %d (line %d):\n",
             yyrule - 1, yylno);
  /* The symbols being reduced.  */
  for (yyi = 0; yyi < yynrhs; yyi++)
    {
      YYFPRINTF (stderr, "   $%d = ", yyi + 1);
      yy_symbol_print (stderr,
                       yystos[+yyssp[yyi + 1 - yynrhs]],
                       &yyvsp[(yyi + 1) - (yynrhs)]
                                              );
      YYFPRINTF (stderr, "\n");
    }
}

# define YY_REDUCE_PRINT(Rule)          \
do {                                    \
  if (yydebug)                          \
    yy_reduce_print (yyssp, yyvsp, Rule); \
} while (0)

/* Nonzero means print parse trace.  It is left uninitialized so that
   multiple parsers can coexist.  */
int yydebug;
#else /* !YYDEBUG */
# define YYDPRINTF(Args)
# define YY_SYMBOL_PRINT(Title, Type, Value, Location)
# define YY_STACK_PRINT(Bottom, Top)
# define YY_REDUCE_PRINT(Rule)
#endif /* !YYDEBUG */


/* YYINITDEPTH -- initial size of the parser's stacks.  */
#ifndef YYINITDEPTH
# define YYINITDEPTH 200
#endif

/* YYMAXDEPTH -- maximum size the stacks can grow to (effective only
   if the built-in stack extension method is used).

   Do not make this value too large; the results are undefined if
   YYSTACK_ALLOC_MAXIMUM < YYSTACK_BYTES (YYMAXDEPTH)
   evaluated with infinite-precision integer arithmetic.  */

#ifndef YYMAXDEPTH
# define YYMAXDEPTH 10000
#endif


#if YYERROR_VERBOSE

# ifndef yystrlen
#  if defined __GLIBC__ && defined _STRING_H
#   define yystrlen(S) (YY_CAST (YYPTRDIFF_T, strlen (S)))
#  else
/* Return the length of YYSTR.  */
static YYPTRDIFF_T
yystrlen (const char *yystr)
{
  YYPTRDIFF_T yylen;
  for (yylen = 0; yystr[yylen]; yylen++)
    continue;
  return yylen;
}
#  endif
# endif

# ifndef yystpcpy
#  if defined __GLIBC__ && defined _STRING_H && defined _GNU_SOURCE
#   define yystpcpy stpcpy
#  else
/* Copy YYSRC to YYDEST, returning the address of the terminating '\0' in
   YYDEST.  */
static char *
yystpcpy (char *yydest, const char *yysrc)
{
  char *yyd = yydest;
  const char *yys = yysrc;

  while ((*yyd++ = *yys++) != '\0')
    continue;

  return yyd - 1;
}
#  endif
# endif

# ifndef yytnamerr
/* Copy to YYRES the contents of YYSTR after stripping away unnecessary
   quotes and backslashes, so that it's suitable for yyerror.  The
   heuristic is that double-quoting is unnecessary unless the string
   contains an apostrophe, a comma, or backslash (other than
   backslash-backslash).  YYSTR is taken from yytname.  If YYRES is
   null, do not copy; instead, return the length of what the result
   would have been.  */
static YYPTRDIFF_T
yytnamerr (char *yyres, const char *yystr)
{
  if (*yystr == '"')
    {
      YYPTRDIFF_T yyn = 0;
      char const *yyp = yystr;

      for (;;)
        switch (*++yyp)
          {
          case '\'':
          case ',':
            goto do_not_strip_quotes;

          case '\\':
            if (*++yyp != '\\')
              goto do_not_strip_quotes;
            else
              goto append;

          append:
          default:
            if (yyres)
              yyres[yyn] = *yyp;
            yyn++;
            break;

          case '"':
            if (yyres)
              yyres[yyn] = '\0';
            return yyn;
          }
    do_not_strip_quotes: ;
    }

  if (yyres)
    return yystpcpy (yyres, yystr) - yyres;
  else
    return yystrlen (yystr);
}
# endif

/* Copy into *YYMSG, which is of size *YYMSG_ALLOC, an error message
   about the unexpected token YYTOKEN for the state stack whose top is
   YYSSP.

   Return 0 if *YYMSG was successfully written.  Return 1 if *YYMSG is
   not large enough to hold the message.  In that case, also set
   *YYMSG_ALLOC to the required number of bytes.  Return 2 if the
   required number of bytes is too large to store.  */
static int
yysyntax_error (YYPTRDIFF_T *yymsg_alloc, char **yymsg,
                yy_state_t *yyssp, int yytoken)
{
  enum { YYERROR_VERBOSE_ARGS_MAXIMUM = 5 };
  /* Internationalized format string. */
  const char *yyformat = YY_NULLPTR;
  /* Arguments of yyformat: reported tokens (one for the "unexpected",
     one per "expected"). */
  char const *yyarg[YYERROR_VERBOSE_ARGS_MAXIMUM];
  /* Actual size of YYARG. */
  int yycount = 0;
  /* Cumulated lengths of YYARG.  */
  YYPTRDIFF_T yysize = 0;

  /* There are many possibilities here to consider:
     - If this state is a consistent state with a default action, then
       the only way this function was invoked is if the default action
       is an error action.  In that case, don't check for expected
       tokens because there are none.
     - The only way there can be no lookahead present (in yychar) is if
       this state is a consistent state with a default action.  Thus,
       detecting the absence of a lookahead is sufficient to determine
       that there is no unexpected or expected token to report.  In that
       case, just report a simple "syntax error".
     - Don't assume there isn't a lookahead just because this state is a
       consistent state with a default action.  There might have been a
       previous inconsistent state, consistent state with a non-default
       action, or user semantic action that manipulated yychar.
     - Of course, the expected token list depends on states to have
       correct lookahead information, and it depends on the parser not
       to perform extra reductions after fetching a lookahead from the
       scanner and before detecting a syntax error.  Thus, state merging
       (from LALR or IELR) and default reductions corrupt the expected
       token list.  However, the list is correct for canonical LR with
       one exception: it will still contain any token that will not be
       accepted due to an error action in a later state.
  */
  if (yytoken != YYEMPTY)
    {
      int yyn = yypact[+*yyssp];
      YYPTRDIFF_T yysize0 = yytnamerr (YY_NULLPTR, yytname[yytoken]);
      yysize = yysize0;
      yyarg[yycount++] = yytname[yytoken];
      if (!yypact_value_is_default (yyn))
        {
          /* Start YYX at -YYN if negative to avoid negative indexes in
             YYCHECK.  In other words, skip the first -YYN actions for
             this state because they are default actions.  */
          int yyxbegin = yyn < 0 ? -yyn : 0;
          /* Stay within bounds of both yycheck and yytname.  */
          int yychecklim = YYLAST - yyn + 1;
          int yyxend = yychecklim < YYNTOKENS ? yychecklim : YYNTOKENS;
          int yyx;

          for (yyx = yyxbegin; yyx < yyxend; ++yyx)
            if (yycheck[yyx + yyn] == yyx && yyx != YYTERROR
                && !yytable_value_is_error (yytable[yyx + yyn]))
              {
                if (yycount == YYERROR_VERBOSE_ARGS_MAXIMUM)
                  {
                    yycount = 1;
                    yysize = yysize0;
                    break;
                  }
                yyarg[yycount++] = yytname[yyx];
                {
                  YYPTRDIFF_T yysize1
                    = yysize + yytnamerr (YY_NULLPTR, yytname[yyx]);
                  if (yysize <= yysize1 && yysize1 <= YYSTACK_ALLOC_MAXIMUM)
                    yysize = yysize1;
                  else
                    return 2;
                }
              }
        }
    }

  switch (yycount)
    {
# define YYCASE_(N, S)                      \
      case N:                               \
        yyformat = S;                       \
      break
    default: /* Avoid compiler warnings. */
      YYCASE_(0, YY_("syntax error"));
      YYCASE_(1, YY_("syntax error, unexpected %s"));
      YYCASE_(2, YY_("syntax error, unexpected %s, expecting %s"));
      YYCASE_(3, YY_("syntax error, unexpected %s, expecting %s or %s"));
      YYCASE_(4, YY_("syntax error, unexpected %s, expecting %s or %s or %s"));
      YYCASE_(5, YY_("syntax error, unexpected %s, expecting %s or %s or %s or %s"));
# undef YYCASE_
    }

  {
    /* Don't count the "%s"s in the final size, but reserve room for
       the terminator.  */
    YYPTRDIFF_T yysize1 = yysize + (yystrlen (yyformat) - 2 * yycount) + 1;
    if (yysize <= yysize1 && yysize1 <= YYSTACK_ALLOC_MAXIMUM)
      yysize = yysize1;
    else
      return 2;
  }

  if (*yymsg_alloc < yysize)
    {
      *yymsg_alloc = 2 * yysize;
      if (! (yysize <= *yymsg_alloc
             && *yymsg_alloc <= YYSTACK_ALLOC_MAXIMUM))
        *yymsg_alloc = YYSTACK_ALLOC_MAXIMUM;
      return 1;
    }

  /* Avoid sprintf, as that infringes on the user's name space.
     Don't have undefined behavior even if the translation
     produced a string with the wrong number of "%s"s.  */
  {
    char *yyp = *yymsg;
    int yyi = 0;
    while ((*yyp = *yyformat) != '\0')
      if (*yyp == '%' && yyformat[1] == 's' && yyi < yycount)
        {
          yyp += yytnamerr (yyp, yyarg[yyi++]);
          yyformat += 2;
        }
      else
        {
          ++yyp;
          ++yyformat;
        }
  }
  return 0;
}
#endif /* YYERROR_VERBOSE */

/*-----------------------------------------------.
| Release the memory associated to this symbol.  |
`-----------------------------------------------*/

static void
yydestruct (const char *yymsg, int yytype, YYSTYPE *yyvaluep)
{
  YYUSE (yyvaluep);
  if (!yymsg)
    yymsg = "Deleting";
  YY_SYMBOL_PRINT (yymsg, yytype, yyvaluep, yylocationp);

  YY_IGNORE_MAYBE_UNINITIALIZED_BEGIN
  YYUSE (yytype);
  YY_IGNORE_MAYBE_UNINITIALIZED_END
}




/* The lookahead symbol.  */
int yychar;

/* The semantic value of the lookahead symbol.  */
YYSTYPE yylval;
/* Number of syntax errors so far.  */
int yynerrs;


/*----------.
| yyparse.  |
`----------*/

int
yyparse (void)
{
    yy_state_fast_t yystate;
    /* Number of tokens to shift before error messages enabled.  */
    int yyerrstatus;

    /* The stacks and their tools:
       'yyss': related to states.
       'yyvs': related to semantic values.

       Refer to the stacks through separate pointers, to allow yyoverflow
       to reallocate them elsewhere.  */

    /* The state stack.  */
    yy_state_t yyssa[YYINITDEPTH];
    yy_state_t *yyss;
    yy_state_t *yyssp;

    /* The semantic value stack.  */
    YYSTYPE yyvsa[YYINITDEPTH];
    YYSTYPE *yyvs;
    YYSTYPE *yyvsp;

    YYPTRDIFF_T yystacksize;

  int yyn;
  int yyresult;
  /* Lookahead token as an internal (translated) token number.  */
  int yytoken = 0;
  /* The variables used to return semantic value and location from the
     action routines.  */
  YYSTYPE yyval;

#if YYERROR_VERBOSE
  /* Buffer for error messages, and its allocated size.  */
  char yymsgbuf[128];
  char *yymsg = yymsgbuf;
  YYPTRDIFF_T yymsg_alloc = sizeof yymsgbuf;
#endif

#define YYPOPSTACK(N)   (yyvsp -= (N), yyssp -= (N))

  /* The number of symbols on the RHS of the reduced rule.
     Keep to zero when no symbol should be popped.  */
  int yylen = 0;

  yyssp = yyss = yyssa;
  yyvsp = yyvs = yyvsa;
  yystacksize = YYINITDEPTH;

  YYDPRINTF ((stderr, "Starting parse\n"));

  yystate = 0;
  yyerrstatus = 0;
  yynerrs = 0;
  yychar = YYEMPTY; /* Cause a token to be read.  */
  goto yysetstate;


/*------------------------------------------------------------.
| yynewstate -- push a new state, which is found in yystate.  |
`------------------------------------------------------------*/
yynewstate:
  /* In all cases, when you get here, the value and location stacks
     have just been pushed.  So pushing a state here evens the stacks.  */
  yyssp++;


/*--------------------------------------------------------------------.
| yysetstate -- set current state (the top of the stack) to yystate.  |
`--------------------------------------------------------------------*/
yysetstate:
  YYDPRINTF ((stderr, "Entering state %d\n", yystate));
  YY_ASSERT (0 <= yystate && yystate < YYNSTATES);
  YY_IGNORE_USELESS_CAST_BEGIN
  *yyssp = YY_CAST (yy_state_t, yystate);
  YY_IGNORE_USELESS_CAST_END

  if (yyss + yystacksize - 1 <= yyssp)
#if !defined yyoverflow && !defined YYSTACK_RELOCATE
    goto yyexhaustedlab;
#else
    {
      /* Get the current used size of the three stacks, in elements.  */
      YYPTRDIFF_T yysize = yyssp - yyss + 1;

# if defined yyoverflow
      {
        /* Give user a chance to reallocate the stack.  Use copies of
           these so that the &'s don't force the real ones into
           memory.  */
        yy_state_t *yyss1 = yyss;
        YYSTYPE *yyvs1 = yyvs;

        /* Each stack pointer address is followed by the size of the
           data in use in that stack, in bytes.  This used to be a
           conditional around just the two extra args, but that might
           be undefined if yyoverflow is a macro.  */
        yyoverflow (YY_("memory exhausted"),
                    &yyss1, yysize * YYSIZEOF (*yyssp),
                    &yyvs1, yysize * YYSIZEOF (*yyvsp),
                    &yystacksize);
        yyss = yyss1;
        yyvs = yyvs1;
      }
# else /* defined YYSTACK_RELOCATE */
      /* Extend the stack our own way.  */
      if (YYMAXDEPTH <= yystacksize)
        goto yyexhaustedlab;
      yystacksize *= 2;
      if (YYMAXDEPTH < yystacksize)
        yystacksize = YYMAXDEPTH;

      {
        yy_state_t *yyss1 = yyss;
        union yyalloc *yyptr =
          YY_CAST (union yyalloc *,
                   YYSTACK_ALLOC (YY_CAST (YYSIZE_T, YYSTACK_BYTES (yystacksize))));
        if (! yyptr)
          goto yyexhaustedlab;
        YYSTACK_RELOCATE (yyss_alloc, yyss);
        YYSTACK_RELOCATE (yyvs_alloc, yyvs);
# undef YYSTACK_RELOCATE
        if (yyss1 != yyssa)
          YYSTACK_FREE (yyss1);
      }
# endif

      yyssp = yyss + yysize - 1;
      yyvsp = yyvs + yysize - 1;

      YY_IGNORE_USELESS_CAST_BEGIN
      YYDPRINTF ((stderr, "Stack size increased to %ld\n",
                  YY_CAST (long, yystacksize)));
      YY_IGNORE_USELESS_CAST_END

      if (yyss + yystacksize - 1 <= yyssp)
        YYABORT;
    }
#endif /* !defined yyoverflow && !defined YYSTACK_RELOCATE */

  if (yystate == YYFINAL)
    YYACCEPT;

  goto yybackup;


/*-----------.
| yybackup.  |
`-----------*/
yybackup:
  /* Do appropriate processing given the current state.  Read a
     lookahead token if we need one and don't already have one.  */

  /* First try to decide what to do without reference to lookahead token.  */
  yyn = yypact[yystate];
  if (yypact_value_is_default (yyn))
    goto yydefault;

  /* Not known => get a lookahead token if don't already have one.  */

  /* YYCHAR is either YYEMPTY or YYEOF or a valid lookahead symbol.  */
  if (yychar == YYEMPTY)
    {
      YYDPRINTF ((stderr, "Reading a token: "));
      yychar = yylex ();
    }

  if (yychar <= YYEOF)
    {
      yychar = yytoken = YYEOF;
      YYDPRINTF ((stderr, "Now at end of input.\n"));
    }
  else
    {
      yytoken = YYTRANSLATE (yychar);
      YY_SYMBOL_PRINT ("Next token is", yytoken, &yylval, &yylloc);
    }

  /* If the proper action on seeing token YYTOKEN is to reduce or to
     detect an error, take that action.  */
  yyn += yytoken;
  if (yyn < 0 || YYLAST < yyn || yycheck[yyn] != yytoken)
    goto yydefault;
  yyn = yytable[yyn];
  if (yyn <= 0)
    {
      if (yytable_value_is_error (yyn))
        goto yyerrlab;
      yyn = -yyn;
      goto yyreduce;
    }

  /* Count tokens shifted since error; after three, turn off error
     status.  */
  if (yyerrstatus)
    yyerrstatus--;

  /* Shift the lookahead token.  */
  YY_SYMBOL_PRINT ("Shifting", yytoken, &yylval, &yylloc);
  yystate = yyn;
  YY_IGNORE_MAYBE_UNINITIALIZED_BEGIN
  *++yyvsp = yylval;
  YY_IGNORE_MAYBE_UNINITIALIZED_END

  /* Discard the shifted token.  */
  yychar = YYEMPTY;
  goto yynewstate;


/*-----------------------------------------------------------.
| yydefault -- do the default action for the current state.  |
`-----------------------------------------------------------*/
yydefault:
  yyn = yydefact[yystate];
  if (yyn == 0)
    goto yyerrlab;
  goto yyreduce;


/*-----------------------------.
| yyreduce -- do a reduction.  |
`-----------------------------*/
yyreduce:
  /* yyn is the number of a rule to reduce with.  */
  yylen = yyr2[yyn];

  /* If YYLEN is nonzero, implement the default value of the action:
     '$$ = $1'.

     Otherwise, the following line sets YYVAL to garbage.
     This behavior is undocumented and Bison
     users should not rely upon it.  Assigning to YYVAL
     unconditionally makes the parser a bit smaller, and it avoids a
     GCC warning that YYVAL may be used uninitialized.  */
  yyval = yyvsp[1-yylen];


  YY_REDUCE_PRINT (yyn);
  switch (yyn)
    {
  case 6:
#line 295 "lev_comp.y"
                  {
			if (fatal_error > 0) {
				(void) fprintf(stderr,
              "%s: %d errors detected for level \"%s\". No output created!\n",
					       fname, fatal_error, (yyvsp[-2].map));
				fatal_error = 0;
				got_errors++;
			} else if (!got_errors) {
				if (!write_level_file((yyvsp[-2].map), splev)) {
                                    lc_error("Can't write output file for '%s'!",
                                             (yyvsp[-2].map));
				    exit(EXIT_FAILURE);
				}
			}
			Free((yyvsp[-2].map));
			Free(splev);
			splev = NULL;
			vardef_free_all(vardefs);
			vardefs = NULL;
		  }
#line 2939 "lev_yacc.c"
    break;

  case 7:
#line 318 "lev_comp.y"
                  {
		      start_level_def(&splev, (yyvsp[0].map));
		      (yyval.map) = (yyvsp[0].map);
		  }
#line 2948 "lev_yacc.c"
    break;

  case 8:
#line 323 "lev_comp.y"
                  {
		      start_level_def(&splev, (yyvsp[-2].map));
		      if ((yyvsp[0].i) == -1) {
			  add_opvars(splev, "iiiiiiiio",
				     VA_PASS9(LVLINIT_MAZEGRID, HWALL, 0,0,
					      0,0,0,0, SPO_INITLEVEL));
		      } else {
			  int bg = (int)what_map_char((char) (yyvsp[0].i));

			  add_opvars(splev, "iiiiiiiio",
				     VA_PASS9(LVLINIT_SOLIDFILL, bg, 0, 0,
					      0,0,0,0, SPO_INITLEVEL));
		      }
		      add_opvars(splev, "io",
				 VA_PASS2(MAZELEVEL, SPO_LEVEL_FLAGS));
		      max_x_map = COLNO-1;
		      max_y_map = ROWNO;
		      (yyval.map) = (yyvsp[-2].map);
		  }
#line 2972 "lev_yacc.c"
    break;

  case 9:
#line 345 "lev_comp.y"
                  {
		      int filling = (int) (yyvsp[0].terr).ter;

		      if (filling == INVALID_TYPE || filling >= MAX_TYPE)
			  lc_error("INIT_MAP: Invalid fill char type.");
		      add_opvars(splev, "iiiiiiiio",
				 VA_PASS9(LVLINIT_SOLIDFILL, filling,
                                          0, (int) (yyvsp[0].terr).lit,
                                          0,0,0,0, SPO_INITLEVEL));
		      max_x_map = COLNO-1;
		      max_y_map = ROWNO;
		  }
#line 2989 "lev_yacc.c"
    break;

  case 10:
#line 358 "lev_comp.y"
                  {
		      int filling = (int) what_map_char((char) (yyvsp[0].i));

		      if (filling == INVALID_TYPE || filling >= MAX_TYPE)
			  lc_error("INIT_MAP: Invalid fill char type.");
                      add_opvars(splev, "iiiiiiiio",
				 VA_PASS9(LVLINIT_MAZEGRID, filling, 0,0,
					  0,0,0,0, SPO_INITLEVEL));
		      max_x_map = COLNO-1;
		      max_y_map = ROWNO;
		  }
#line 3005 "lev_yacc.c"
    break;

  case 11:
#line 370 "lev_comp.y"
                  {
		      add_opvars(splev, "iiiiiiiio",
				 VA_PASS9(LVLINIT_ROGUE,0,0,0,
					  0,0,0,0, SPO_INITLEVEL));
		  }
#line 3015 "lev_yacc.c"
    break;

  case 12:
#line 376 "lev_comp.y"
                  {
                      int fg = (int) what_map_char((char) (yyvsp[-11].i)),
                          bg = (int) what_map_char((char) (yyvsp[-9].i));
                      int smoothed = (int) (yyvsp[-7].i),
                          joined = (int) (yyvsp[-5].i),
                          lit = (int) (yyvsp[-3].i),
                          walled = (int) (yyvsp[-1].i),
                          filling = (int) (yyvsp[0].i);

		      if (fg == INVALID_TYPE || fg >= MAX_TYPE)
			  lc_error("INIT_MAP: Invalid foreground type.");
		      if (bg == INVALID_TYPE || bg >= MAX_TYPE)
			  lc_error("INIT_MAP: Invalid background type.");
		      if (joined && fg != CORR && fg != ROOM && fg != GRASS && fg != GROUND && fg != AIR && fg != CLOUD)
			  lc_error("INIT_MAP: Invalid foreground type for joined map.");

		      if (filling == INVALID_TYPE)
			  lc_error("INIT_MAP: Invalid fill char type.");

		      add_opvars(splev, "iiiiiiiio",
				 VA_PASS9(LVLINIT_MINES, filling, walled, lit,
					  joined, smoothed, bg, fg,
					  SPO_INITLEVEL));
			max_x_map = COLNO-1;
			max_y_map = ROWNO;
		  }
#line 3046 "lev_yacc.c"
    break;

  case 13:
#line 405 "lev_comp.y"
                  {
		      add_opvars(splev, "io", VA_PASS2((int)(yyvsp[0].i), SPO_TILESET));
		  }
#line 3054 "lev_yacc.c"
    break;

  case 14:
#line 411 "lev_comp.y"
                  {
		      add_opvars(splev, "iio", VA_PASS3((int)(yyvsp[0].i), (int)(yyvsp[-2].i), SPO_FOREST));
		  }
#line 3062 "lev_yacc.c"
    break;

  case 15:
#line 415 "lev_comp.y"
                  {
		      add_opvars(splev, "iio", VA_PASS3(0, (int)(yyvsp[0].i), SPO_FOREST));
		  }
#line 3070 "lev_yacc.c"
    break;

  case 16:
#line 421 "lev_comp.y"
                  {
		      add_opvars(splev, "iio", VA_PASS3((int)(yyvsp[0].i), (int)(yyvsp[-2].i), SPO_MONSTER_GENERATION));
		  }
#line 3078 "lev_yacc.c"
    break;

  case 17:
#line 427 "lev_comp.y"
                  {
		      add_opvars(splev, "io", VA_PASS2((int)(yyvsp[0].i), SPO_BOUNDARY_TYPE));
		  }
#line 3086 "lev_yacc.c"
    break;

  case 18:
#line 433 "lev_comp.y"
                  {
		      (yyval.i) = 0;
		  }
#line 3094 "lev_yacc.c"
    break;

  case 19:
#line 437 "lev_comp.y"
                  {
		      (yyval.i) = (yyvsp[0].i);
		  }
#line 3102 "lev_yacc.c"
    break;

  case 20:
#line 443 "lev_comp.y"
                  {
		      add_opvars(splev, "o", VA_PASS1(SPO_COPY));
		      (yyval.i) = 0;
		  }
#line 3111 "lev_yacc.c"
    break;

  case 21:
#line 448 "lev_comp.y"
                  {
		      (yyval.i) = 1;
		  }
#line 3119 "lev_yacc.c"
    break;

  case 22:
#line 454 "lev_comp.y"
                  {
		      (yyval.i) = -1;
		  }
#line 3127 "lev_yacc.c"
    break;

  case 23:
#line 458 "lev_comp.y"
                  {
		      (yyval.i) = what_map_char((char) (yyvsp[0].i));
		  }
#line 3135 "lev_yacc.c"
    break;

  case 26:
#line 469 "lev_comp.y"
                  {
		      add_opvars(splev, "io", VA_PASS2(0, SPO_LEVEL_FLAGS));
		  }
#line 3143 "lev_yacc.c"
    break;

  case 27:
#line 473 "lev_comp.y"
                  {
		      add_opvars(splev, "io",
                                 VA_PASS2((int) (yyvsp[0].i), SPO_LEVEL_FLAGS));
		  }
#line 3152 "lev_yacc.c"
    break;

  case 28:
#line 480 "lev_comp.y"
                  {
		      (yyval.i) = ((yyvsp[-2].i) | (yyvsp[0].i));
		  }
#line 3160 "lev_yacc.c"
    break;

  case 29:
#line 484 "lev_comp.y"
                  {
		      (yyval.i) = (yyvsp[0].i);
		  }
#line 3168 "lev_yacc.c"
    break;

  case 30:
#line 490 "lev_comp.y"
                  {
		      (yyval.i) = 0;
		  }
#line 3176 "lev_yacc.c"
    break;

  case 31:
#line 494 "lev_comp.y"
                  {
		      (yyval.i) = 1 + (yyvsp[0].i);
		  }
#line 3184 "lev_yacc.c"
    break;

  case 32:
#line 500 "lev_comp.y"
                  {
		      (yyval.i) = (yyvsp[-1].i);
		  }
#line 3192 "lev_yacc.c"
    break;

  case 118:
#line 601 "lev_comp.y"
                  {
		      struct lc_vardefs *vd;

		      if ((vd = vardef_defined(vardefs, (yyvsp[0].map), 1))) {
			  if (!(vd->var_type & SPOVAR_ARRAY))
			      lc_error("Trying to shuffle non-array variable '%s'",
                                       (yyvsp[0].map));
		      } else
                          lc_error("Trying to shuffle undefined variable '%s'",
                                   (yyvsp[0].map));
		      add_opvars(splev, "so", VA_PASS2((yyvsp[0].map), SPO_SHUFFLE_ARRAY));
		      Free((yyvsp[0].map));
		  }
#line 3210 "lev_yacc.c"
    break;

  case 119:
#line 617 "lev_comp.y"
                  {
		      vardefs = add_vardef_type(vardefs, (yyvsp[-2].map), SPOVAR_INT);
		      add_opvars(splev, "iso", VA_PASS3(0, (yyvsp[-2].map), SPO_VAR_INIT));
		      Free((yyvsp[-2].map));
		  }
#line 3220 "lev_yacc.c"
    break;

  case 120:
#line 623 "lev_comp.y"
                  {
		      vardefs = add_vardef_type(vardefs, (yyvsp[-4].map), SPOVAR_SEL);
		      add_opvars(splev, "iso", VA_PASS3(0, (yyvsp[-4].map), SPO_VAR_INIT));
		      Free((yyvsp[-4].map));
		  }
#line 3230 "lev_yacc.c"
    break;

  case 121:
#line 629 "lev_comp.y"
                  {
		      vardefs = add_vardef_type(vardefs, (yyvsp[-2].map), SPOVAR_STRING);
		      add_opvars(splev, "iso", VA_PASS3(0, (yyvsp[-2].map), SPO_VAR_INIT));
		      Free((yyvsp[-2].map));
		  }
#line 3240 "lev_yacc.c"
    break;

  case 122:
#line 635 "lev_comp.y"
                  {
		      vardefs = add_vardef_type(vardefs, (yyvsp[-4].map), SPOVAR_MAPCHAR);
		      add_opvars(splev, "iso", VA_PASS3(0, (yyvsp[-4].map), SPO_VAR_INIT));
		      Free((yyvsp[-4].map));
		  }
#line 3250 "lev_yacc.c"
    break;

  case 123:
#line 641 "lev_comp.y"
                  {
		      vardefs = add_vardef_type(vardefs, (yyvsp[-4].map), SPOVAR_MONST);
		      add_opvars(splev, "iso", VA_PASS3(0, (yyvsp[-4].map), SPO_VAR_INIT));
		      Free((yyvsp[-4].map));
		  }
#line 3260 "lev_yacc.c"
    break;

  case 124:
#line 647 "lev_comp.y"
                  {
		      vardefs = add_vardef_type(vardefs, (yyvsp[-4].map), SPOVAR_OBJ);
		      add_opvars(splev, "iso", VA_PASS3(0, (yyvsp[-4].map), SPO_VAR_INIT));
		      Free((yyvsp[-4].map));
		  }
#line 3270 "lev_yacc.c"
    break;

  case 125:
#line 653 "lev_comp.y"
                  {
		      vardefs = add_vardef_type(vardefs, (yyvsp[-2].map), SPOVAR_COORD);
		      add_opvars(splev, "iso", VA_PASS3(0, (yyvsp[-2].map), SPO_VAR_INIT));
		      Free((yyvsp[-2].map));
		  }
#line 3280 "lev_yacc.c"
    break;

  case 126:
#line 659 "lev_comp.y"
                  {
		      vardefs = add_vardef_type(vardefs, (yyvsp[-2].map), SPOVAR_REGION);
		      add_opvars(splev, "iso", VA_PASS3(0, (yyvsp[-2].map), SPO_VAR_INIT));
		      Free((yyvsp[-2].map));
		  }
#line 3290 "lev_yacc.c"
    break;

  case 127:
#line 665 "lev_comp.y"
                  {
		      int n_items = (int) (yyvsp[-1].i);

		      vardefs = add_vardef_type(vardefs, (yyvsp[-4].map),
                                                SPOVAR_INT | SPOVAR_ARRAY);
		      add_opvars(splev, "iso",
				 VA_PASS3(n_items, (yyvsp[-4].map), SPO_VAR_INIT));
		      Free((yyvsp[-4].map));
		  }
#line 3304 "lev_yacc.c"
    break;

  case 128:
#line 675 "lev_comp.y"
                  {
		      int n_items = (int) (yyvsp[-1].i);

		      vardefs = add_vardef_type(vardefs, (yyvsp[-4].map),
                                                SPOVAR_COORD | SPOVAR_ARRAY);
		      add_opvars(splev, "iso",
				 VA_PASS3(n_items, (yyvsp[-4].map), SPO_VAR_INIT));
		      Free((yyvsp[-4].map));
		  }
#line 3318 "lev_yacc.c"
    break;

  case 129:
#line 685 "lev_comp.y"
                  {
                      int n_items = (int) (yyvsp[-1].i);

		      vardefs = add_vardef_type(vardefs, (yyvsp[-4].map),
                                                SPOVAR_REGION | SPOVAR_ARRAY);
		      add_opvars(splev, "iso",
				 VA_PASS3(n_items, (yyvsp[-4].map), SPO_VAR_INIT));
		      Free((yyvsp[-4].map));
		  }
#line 3332 "lev_yacc.c"
    break;

  case 130:
#line 695 "lev_comp.y"
                  {
                      int n_items = (int) (yyvsp[-1].i);

		      vardefs = add_vardef_type(vardefs, (yyvsp[-6].map),
                                                SPOVAR_MAPCHAR | SPOVAR_ARRAY);
		      add_opvars(splev, "iso",
				 VA_PASS3(n_items, (yyvsp[-6].map), SPO_VAR_INIT));
		      Free((yyvsp[-6].map));
		  }
#line 3346 "lev_yacc.c"
    break;

  case 131:
#line 705 "lev_comp.y"
                  {
		      int n_items = (int) (yyvsp[-1].i);

		      vardefs = add_vardef_type(vardefs, (yyvsp[-6].map),
                                                SPOVAR_MONST | SPOVAR_ARRAY);
		      add_opvars(splev, "iso",
				 VA_PASS3(n_items, (yyvsp[-6].map), SPO_VAR_INIT));
		      Free((yyvsp[-6].map));
		  }
#line 3360 "lev_yacc.c"
    break;

  case 132:
#line 715 "lev_comp.y"
                  {
                      int n_items = (int) (yyvsp[-1].i);

		      vardefs = add_vardef_type(vardefs, (yyvsp[-6].map),
                                                SPOVAR_OBJ | SPOVAR_ARRAY);
		      add_opvars(splev, "iso",
				 VA_PASS3(n_items, (yyvsp[-6].map), SPO_VAR_INIT));
		      Free((yyvsp[-6].map));
		  }
#line 3374 "lev_yacc.c"
    break;

  case 133:
#line 725 "lev_comp.y"
                  {
                      int n_items = (int) (yyvsp[-1].i);

		      vardefs = add_vardef_type(vardefs, (yyvsp[-4].map),
                                                SPOVAR_STRING | SPOVAR_ARRAY);
		      add_opvars(splev, "iso",
				 VA_PASS3(n_items, (yyvsp[-4].map), SPO_VAR_INIT));
		      Free((yyvsp[-4].map));
		  }
#line 3388 "lev_yacc.c"
    break;

  case 134:
#line 737 "lev_comp.y"
                  {
		      add_opvars(splev, "O", VA_PASS1((yyvsp[0].i)));
		      (yyval.i) = 1;
		  }
#line 3397 "lev_yacc.c"
    break;

  case 135:
#line 742 "lev_comp.y"
                  {
		      add_opvars(splev, "O", VA_PASS1((yyvsp[0].i)));
		      (yyval.i) = 1 + (yyvsp[-2].i);
		  }
#line 3406 "lev_yacc.c"
    break;

  case 136:
#line 749 "lev_comp.y"
                  {
		      add_opvars(splev, "M", VA_PASS1((yyvsp[0].i)));
		      (yyval.i) = 1;
		  }
#line 3415 "lev_yacc.c"
    break;

  case 137:
#line 754 "lev_comp.y"
                  {
		      add_opvars(splev, "M", VA_PASS1((yyvsp[0].i)));
		      (yyval.i) = 1 + (yyvsp[-2].i);
		  }
#line 3424 "lev_yacc.c"
    break;

  case 138:
#line 761 "lev_comp.y"
                  {
		      add_opvars(splev, "m", VA_PASS1((yyvsp[0].i)));
		      (yyval.i) = 1;
		  }
#line 3433 "lev_yacc.c"
    break;

  case 139:
#line 766 "lev_comp.y"
                  {
		      add_opvars(splev, "m", VA_PASS1((yyvsp[0].i)));
		      (yyval.i) = 1 + (yyvsp[-2].i);
		  }
#line 3442 "lev_yacc.c"
    break;

  case 140:
#line 773 "lev_comp.y"
                  {
		      (yyval.i) = 1;
		  }
#line 3450 "lev_yacc.c"
    break;

  case 141:
#line 777 "lev_comp.y"
                  {
		      (yyval.i) = 1 + (yyvsp[-2].i);
		  }
#line 3458 "lev_yacc.c"
    break;

  case 142:
#line 783 "lev_comp.y"
                  {
		      add_opvars(splev, "c", VA_PASS1((yyvsp[0].i)));
		      (yyval.i) = 1;
		  }
#line 3467 "lev_yacc.c"
    break;

  case 143:
#line 788 "lev_comp.y"
                  {
		      add_opvars(splev, "c", VA_PASS1((yyvsp[0].i)));
		      (yyval.i) = 1 + (yyvsp[-2].i);
		  }
#line 3476 "lev_yacc.c"
    break;

  case 144:
#line 795 "lev_comp.y"
                  {
		      (yyval.i) = 1;
		  }
#line 3484 "lev_yacc.c"
    break;

  case 145:
#line 799 "lev_comp.y"
                  {
		      (yyval.i) = 1 + (yyvsp[-2].i);
		  }
#line 3492 "lev_yacc.c"
    break;

  case 146:
#line 805 "lev_comp.y"
                  {
		      (yyval.i) = 1;
		  }
#line 3500 "lev_yacc.c"
    break;

  case 147:
#line 809 "lev_comp.y"
                  {
		      (yyval.i) = 1 + (yyvsp[-2].i);
		  }
#line 3508 "lev_yacc.c"
    break;

  case 148:
#line 815 "lev_comp.y"
                  {
		      struct lc_funcdefs *funcdef;

		      if (in_function_definition)
			  lc_error("Recursively defined functions not allowed (function %s).", (yyvsp[-1].map));

		      in_function_definition++;

		      if (funcdef_defined(function_definitions, (yyvsp[-1].map), 1))
			  lc_error("Function '%s' already defined once.", (yyvsp[-1].map));

		      funcdef = funcdef_new(-1, (yyvsp[-1].map));
		      funcdef->next = function_definitions;
		      function_definitions = funcdef;
		      function_splev_backup = splev;
		      splev = &(funcdef->code);
		      Free((yyvsp[-1].map));
		      curr_function = funcdef;
		      function_tmp_var_defs = vardefs;
		      vardefs = NULL;
		  }
#line 3534 "lev_yacc.c"
    break;

  case 149:
#line 837 "lev_comp.y"
                  {
		      /* nothing */
		  }
#line 3542 "lev_yacc.c"
    break;

  case 150:
#line 841 "lev_comp.y"
                  {
		      add_opvars(splev, "io", VA_PASS2(0, SPO_RETURN));
		      splev = function_splev_backup;
		      in_function_definition--;
		      curr_function = NULL;
		      vardef_free_all(vardefs);
		      vardefs = function_tmp_var_defs;
		  }
#line 3555 "lev_yacc.c"
    break;

  case 151:
#line 852 "lev_comp.y"
                  {
		      struct lc_funcdefs *tmpfunc;

		      tmpfunc = funcdef_defined(function_definitions, (yyvsp[-3].map), 1);
		      if (tmpfunc) {
			  int l;
			  int nparams = (int) strlen((yyvsp[-1].map));
			  char *fparamstr = funcdef_paramtypes(tmpfunc);

			  if (strcmp((yyvsp[-1].map), fparamstr)) {
			      char *tmps = strdup(decode_parm_str(fparamstr));

			      lc_error("Function '%s' requires params '%s', got '%s' instead.",
                                       (yyvsp[-3].map), tmps, decode_parm_str((yyvsp[-1].map)));
			      Free(tmps);
			  }
			  Free(fparamstr);
			  Free((yyvsp[-1].map));
			  if (!(tmpfunc->n_called)) {
			      /* we haven't called the function yet, so insert it in the code */
			      struct opvar *jmp = New(struct opvar);

			      set_opvar_int(jmp, splev->n_opcodes+1);
			      add_opcode(splev, SPO_PUSH, jmp);
                              /* we must jump past it first, then CALL it, due to RETURN. */
			      add_opcode(splev, SPO_JMP, NULL);

			      tmpfunc->addr = splev->n_opcodes;

			      { /* init function parameter variables */
				  struct lc_funcdefs_parm *tfp = tmpfunc->params;
				  while (tfp) {
				      add_opvars(splev, "iso",
						 VA_PASS3(0, tfp->name,
							  SPO_VAR_INIT));
				      tfp = tfp->next;
				  }
			      }

			      splev_add_from(splev, &(tmpfunc->code));
			      set_opvar_int(jmp,
                                            splev->n_opcodes - jmp->vardata.l);
			  }
			  l = (int) (tmpfunc->addr - splev->n_opcodes - 2);
			  add_opvars(splev, "iio",
				     VA_PASS3(nparams, l, SPO_CALL));
			  tmpfunc->n_called++;
		      } else {
			  lc_error("Function '%s' not defined.", (yyvsp[-3].map));
		      }
		      Free((yyvsp[-3].map));
		  }
#line 3612 "lev_yacc.c"
    break;

  case 152:
#line 907 "lev_comp.y"
                  {
		      add_opcode(splev, SPO_EXIT, NULL);
		  }
#line 3620 "lev_yacc.c"
    break;

  case 153:
#line 913 "lev_comp.y"
                  {
		      (yyval.i) = 100;
		  }
#line 3628 "lev_yacc.c"
    break;

  case 154:
#line 917 "lev_comp.y"
                  {
		      (yyval.i) = (yyvsp[0].i);
		  }
#line 3636 "lev_yacc.c"
    break;

  case 155:
#line 923 "lev_comp.y"
                  {
		      /* val > rn2(100) */
		      add_opvars(splev, "iio",
				 VA_PASS3((int) (yyvsp[0].i), 100, SPO_RN2));
		      (yyval.i) = SPO_JG;
                  }
#line 3647 "lev_yacc.c"
    break;

  case 156:
#line 930 "lev_comp.y"
                  {
		      (yyval.i) = (yyvsp[-2].i);
                  }
#line 3655 "lev_yacc.c"
    break;

  case 157:
#line 934 "lev_comp.y"
                  {
		      /* boolean, explicit foo != 0 */
		      add_opvars(splev, "i", VA_PASS1(0));
		      (yyval.i) = SPO_JNE;
                  }
#line 3665 "lev_yacc.c"
    break;

  case 158:
#line 942 "lev_comp.y"
                  {
		      is_inconstant_number = 0;
		  }
#line 3673 "lev_yacc.c"
    break;

  case 159:
#line 946 "lev_comp.y"
                  {
		      struct opvar *chkjmp;

		      if (in_switch_statement > 0)
			  lc_error("Cannot nest switch-statements.");

		      in_switch_statement++;

		      n_switch_case_list = 0;
		      switch_default_case = NULL;

		      if (!is_inconstant_number)
			  add_opvars(splev, "o", VA_PASS1(SPO_RN2));
		      is_inconstant_number = 0;

		      chkjmp = New(struct opvar);
		      set_opvar_int(chkjmp, splev->n_opcodes+1);
		      switch_check_jump = chkjmp;
		      add_opcode(splev, SPO_PUSH, chkjmp);
		      add_opcode(splev, SPO_JMP, NULL);
		      break_stmt_start();
		  }
#line 3700 "lev_yacc.c"
    break;

  case 160:
#line 969 "lev_comp.y"
                  {
		      struct opvar *endjump = New(struct opvar);
		      int i;

		      set_opvar_int(endjump, splev->n_opcodes+1);

		      add_opcode(splev, SPO_PUSH, endjump);
		      add_opcode(splev, SPO_JMP, NULL);

		      set_opvar_int(switch_check_jump,
			     splev->n_opcodes - switch_check_jump->vardata.l);

		      for (i = 0; i < n_switch_case_list; i++) {
			  add_opvars(splev, "oio",
				     VA_PASS3(SPO_COPY,
					      switch_case_value[i], SPO_CMP));
			  set_opvar_int(switch_case_list[i],
			 switch_case_list[i]->vardata.l - splev->n_opcodes-1);
			  add_opcode(splev, SPO_PUSH, switch_case_list[i]);
			  add_opcode(splev, SPO_JE, NULL);
		      }

		      if (switch_default_case) {
			  set_opvar_int(switch_default_case,
			 switch_default_case->vardata.l - splev->n_opcodes-1);
			  add_opcode(splev, SPO_PUSH, switch_default_case);
			  add_opcode(splev, SPO_JMP, NULL);
		      }

		      set_opvar_int(endjump, splev->n_opcodes - endjump->vardata.l);

		      break_stmt_end(splev);

		      add_opcode(splev, SPO_POP, NULL); /* get rid of the value in stack */
		      in_switch_statement--;


		  }
#line 3743 "lev_yacc.c"
    break;

  case 163:
#line 1014 "lev_comp.y"
                  {
		      if (n_switch_case_list < MAX_SWITCH_CASES) {
			  struct opvar *tmppush = New(struct opvar);

			  set_opvar_int(tmppush, splev->n_opcodes);
			  switch_case_value[n_switch_case_list] = (yyvsp[-1].i);
			  switch_case_list[n_switch_case_list++] = tmppush;
		      } else lc_error("Too many cases in a switch.");
		  }
#line 3757 "lev_yacc.c"
    break;

  case 164:
#line 1024 "lev_comp.y"
                  {
		  }
#line 3764 "lev_yacc.c"
    break;

  case 165:
#line 1027 "lev_comp.y"
                  {
		      struct opvar *tmppush = New(struct opvar);

		      if (switch_default_case)
			  lc_error("Switch default case already used.");

		      set_opvar_int(tmppush, splev->n_opcodes);
		      switch_default_case = tmppush;
		  }
#line 3778 "lev_yacc.c"
    break;

  case 166:
#line 1037 "lev_comp.y"
                  {
		  }
#line 3785 "lev_yacc.c"
    break;

  case 167:
#line 1042 "lev_comp.y"
                  {
		      if (!allow_break_statements)
			  lc_error("Cannot use BREAK outside a statement block.");
		      else {
			  break_stmt_new(splev, splev->n_opcodes);
		      }
		  }
#line 3797 "lev_yacc.c"
    break;

  case 170:
#line 1056 "lev_comp.y"
                  {
		      char buf[256], buf2[256];

		      if (n_forloops >= MAX_NESTED_IFS) {
			  lc_error("FOR: Too deeply nested loops.");
			  n_forloops = MAX_NESTED_IFS - 1;
		      }

		      /* first, define a variable for the for-loop end value */
		      Sprintf(buf, "%s end", (yyvsp[-4].map));
		      /* the value of which is already in stack (the 2nd math_expr) */
		      add_opvars(splev, "iso", VA_PASS3(0, buf, SPO_VAR_INIT));

		      vardefs = add_vardef_type(vardefs, (yyvsp[-4].map), SPOVAR_INT);
		      /* define the for-loop variable. value is in stack (1st math_expr) */
		      add_opvars(splev, "iso", VA_PASS3(0, (yyvsp[-4].map), SPO_VAR_INIT));

		      /* calculate value for the loop "step" variable */
		      Sprintf(buf2, "%s step", (yyvsp[-4].map));
		      /* end - start */
		      add_opvars(splev, "vvo",
				 VA_PASS3(buf, (yyvsp[-4].map), SPO_MATH_SUB));
		      /* sign of that */
		      add_opvars(splev, "o", VA_PASS1(SPO_MATH_SIGN));
		      /* save the sign into the step var */
		      add_opvars(splev, "iso",
				 VA_PASS3(0, buf2, SPO_VAR_INIT));

		      forloop_list[n_forloops].varname = strdup((yyvsp[-4].map));
		      forloop_list[n_forloops].jmp_point = splev->n_opcodes;

		      n_forloops++;
		      Free((yyvsp[-4].map));
		  }
#line 3836 "lev_yacc.c"
    break;

  case 171:
#line 1093 "lev_comp.y"
                  {
		      /* nothing */
		      break_stmt_start();
		  }
#line 3845 "lev_yacc.c"
    break;

  case 172:
#line 1098 "lev_comp.y"
                  {
                      int l;
		      char buf[256], buf2[256];

		      n_forloops--;
		      Sprintf(buf, "%s step", forloop_list[n_forloops].varname);
		      Sprintf(buf2, "%s end", forloop_list[n_forloops].varname);
		      /* compare for-loop var to end value */
		      add_opvars(splev, "vvo",
				 VA_PASS3(forloop_list[n_forloops].varname,
					  buf2, SPO_CMP));
		      /* var + step */
		      add_opvars(splev, "vvo",
				VA_PASS3(buf, forloop_list[n_forloops].varname,
					 SPO_MATH_ADD));
		      /* for-loop var = (for-loop var + step) */
		      add_opvars(splev, "iso",
				 VA_PASS3(0, forloop_list[n_forloops].varname,
					  SPO_VAR_INIT));
		      /* jump back if compared values were not equal */
                      l = (int) (forloop_list[n_forloops].jmp_point
                                 - splev->n_opcodes - 1);
		      add_opvars(splev, "io", VA_PASS2(l, SPO_JNE));
		      Free(forloop_list[n_forloops].varname);
		      break_stmt_end(splev);
		  }
#line 3876 "lev_yacc.c"
    break;

  case 173:
#line 1127 "lev_comp.y"
                  {
		      struct opvar *tmppush = New(struct opvar);

		      if (n_if_list >= MAX_NESTED_IFS) {
			  lc_error("LOOP: Too deeply nested conditionals.");
			  n_if_list = MAX_NESTED_IFS - 1;
		      }
		      set_opvar_int(tmppush, splev->n_opcodes);
		      if_list[n_if_list++] = tmppush;

		      add_opvars(splev, "o", VA_PASS1(SPO_DEC));
		      break_stmt_start();
		  }
#line 3894 "lev_yacc.c"
    break;

  case 174:
#line 1141 "lev_comp.y"
                  {
		      struct opvar *tmppush;

		      add_opvars(splev, "oio", VA_PASS3(SPO_COPY, 0, SPO_CMP));

		      tmppush = (struct opvar *) if_list[--n_if_list];
		      set_opvar_int(tmppush,
                                    tmppush->vardata.l - splev->n_opcodes-1);
		      add_opcode(splev, SPO_PUSH, tmppush);
		      add_opcode(splev, SPO_JG, NULL);
		      add_opcode(splev, SPO_POP, NULL); /* discard count */
		      break_stmt_end(splev);
		  }
#line 3912 "lev_yacc.c"
    break;

  case 175:
#line 1157 "lev_comp.y"
                  {
		      struct opvar *tmppush2 = New(struct opvar);

		      if (n_if_list >= MAX_NESTED_IFS) {
			  lc_error("IF: Too deeply nested conditionals.");
			  n_if_list = MAX_NESTED_IFS - 1;
		      }

		      add_opcode(splev, SPO_CMP, NULL);

		      set_opvar_int(tmppush2, splev->n_opcodes+1);

		      if_list[n_if_list++] = tmppush2;

		      add_opcode(splev, SPO_PUSH, tmppush2);

		      add_opcode(splev, reverse_jmp_opcode( (yyvsp[-1].i) ), NULL);

		  }
#line 3936 "lev_yacc.c"
    break;

  case 176:
#line 1177 "lev_comp.y"
                  {
		      if (n_if_list > 0) {
			  struct opvar *tmppush;

			  tmppush = (struct opvar *) if_list[--n_if_list];
			  set_opvar_int(tmppush,
                                        splev->n_opcodes - tmppush->vardata.l);
		      } else lc_error("IF: Huh?!  No start address?");
		  }
#line 3950 "lev_yacc.c"
    break;

  case 177:
#line 1189 "lev_comp.y"
                  {
		      struct opvar *tmppush2 = New(struct opvar);

		      if (n_if_list >= MAX_NESTED_IFS) {
			  lc_error("IF: Too deeply nested conditionals.");
			  n_if_list = MAX_NESTED_IFS - 1;
		      }

		      add_opcode(splev, SPO_CMP, NULL);

		      set_opvar_int(tmppush2, splev->n_opcodes+1);

		      if_list[n_if_list++] = tmppush2;

		      add_opcode(splev, SPO_PUSH, tmppush2);

		      add_opcode(splev, reverse_jmp_opcode( (yyvsp[0].i) ), NULL);

		  }
#line 3974 "lev_yacc.c"
    break;

  case 178:
#line 1209 "lev_comp.y"
                  {
		     /* do nothing */
		  }
#line 3982 "lev_yacc.c"
    break;

  case 179:
#line 1215 "lev_comp.y"
                  {
		      if (n_if_list > 0) {
			  struct opvar *tmppush;

			  tmppush = (struct opvar *) if_list[--n_if_list];
			  set_opvar_int(tmppush,
                                        splev->n_opcodes - tmppush->vardata.l);
		      } else lc_error("IF: Huh?!  No start address?");
		  }
#line 3996 "lev_yacc.c"
    break;

  case 180:
#line 1225 "lev_comp.y"
                  {
		      if (n_if_list > 0) {
			  struct opvar *tmppush = New(struct opvar);
			  struct opvar *tmppush2;

			  set_opvar_int(tmppush, splev->n_opcodes+1);
			  add_opcode(splev, SPO_PUSH, tmppush);

			  add_opcode(splev, SPO_JMP, NULL);

			  tmppush2 = (struct opvar *) if_list[--n_if_list];

			  set_opvar_int(tmppush2,
                                      splev->n_opcodes - tmppush2->vardata.l);
			  if_list[n_if_list++] = tmppush;
		      } else lc_error("IF: Huh?!  No else-part address?");
		  }
#line 4018 "lev_yacc.c"
    break;

  case 181:
#line 1243 "lev_comp.y"
                  {
		      if (n_if_list > 0) {
			  struct opvar *tmppush;
			  tmppush = (struct opvar *) if_list[--n_if_list];
			  set_opvar_int(tmppush, splev->n_opcodes - tmppush->vardata.l);
		      } else lc_error("IF: Huh?! No end address?");
		  }
#line 4030 "lev_yacc.c"
    break;

  case 182:
#line 1253 "lev_comp.y"
                  {
		      add_opvars(splev, "o", VA_PASS1(SPO_MESSAGE));
		  }
#line 4038 "lev_yacc.c"
    break;

  case 183:
#line 1259 "lev_comp.y"
                  {
		      add_opvars(splev, "iiiiiio",
			      VA_PASS7(-1,  0, -1, -1, -1, -1, SPO_CORRIDOR));
		  }
#line 4047 "lev_yacc.c"
    break;

  case 184:
#line 1264 "lev_comp.y"
                  {
		      add_opvars(splev, "iiiiiio",
			      VA_PASS7(-1, (yyvsp[0].i), -1, -1, -1, -1, SPO_CORRIDOR));
		  }
#line 4056 "lev_yacc.c"
    break;

  case 185:
#line 1269 "lev_comp.y"
                  {
		      add_opvars(splev, "iiiiiio",
			      VA_PASS7(-1, -1, -1, -1, -1, -1, SPO_CORRIDOR));
		  }
#line 4065 "lev_yacc.c"
    break;

  case 186:
#line 1276 "lev_comp.y"
                  {
		      add_opvars(splev, "iiiiiio",
				 VA_PASS7((yyvsp[-2].corpos).room, (yyvsp[-2].corpos).door, (yyvsp[-2].corpos).wall,
					  (yyvsp[0].corpos).room, (yyvsp[0].corpos).door, (yyvsp[0].corpos).wall,
					  SPO_CORRIDOR));
		  }
#line 4076 "lev_yacc.c"
    break;

  case 187:
#line 1283 "lev_comp.y"
                  {
		      add_opvars(splev, "iiiiiio",
				 VA_PASS7((yyvsp[-2].corpos).room, (yyvsp[-2].corpos).door, (yyvsp[-2].corpos).wall,
					  -1, -1, (long)(yyvsp[0].i),
					  SPO_CORRIDOR));
		  }
#line 4087 "lev_yacc.c"
    break;

  case 188:
#line 1292 "lev_comp.y"
                  {
			(yyval.corpos).room = (yyvsp[-5].i);
			(yyval.corpos).wall = (yyvsp[-3].i);
			(yyval.corpos).door = (yyvsp[-1].i);
		  }
#line 4097 "lev_yacc.c"
    break;

  case 189:
#line 1300 "lev_comp.y"
                  {
		      if (((yyvsp[-2].i) < 100) && ((yyvsp[-3].i) == OROOM))
			  lc_error("Only typed rooms can have a chance.");
		      else {
			  add_opvars(splev, "iii",
				     VA_PASS3((long)(yyvsp[-3].i), (long)(yyvsp[-2].i), (long)(yyvsp[0].i)));
		      }
                  }
#line 4110 "lev_yacc.c"
    break;

  case 190:
#line 1311 "lev_comp.y"
                  {
		      long rflags = (yyvsp[-3].i);
		      long flmt = (long)(yyvsp[-2].i);
		      long flt = (long)(yyvsp[-1].i);

		      if (rflags == -1) rflags = (1 << 0);
		      //if (flmt == -1) flmt = ROOM;
		      //if (flt == -1) flt = 0;

		      add_opvars(splev, "iiiiiiiiio",
				 VA_PASS10(flt, flmt, rflags, ERR, ERR,
					  (yyvsp[-6].crd).x, (yyvsp[-6].crd).y, (yyvsp[-4].sze).width, (yyvsp[-4].sze).height,
					  SPO_SUBROOM));
		      break_stmt_start();
		  }
#line 4130 "lev_yacc.c"
    break;

  case 191:
#line 1327 "lev_comp.y"
                  {
		      break_stmt_end(splev);
		      add_opcode(splev, SPO_ENDROOM, NULL);
		  }
#line 4139 "lev_yacc.c"
    break;

  case 192:
#line 1334 "lev_comp.y"
                  {
		      long rflags = (yyvsp[-3].i);
		      long flmt = (long)(yyvsp[-2].i);
		      long flt = (long)(yyvsp[-1].i);

		      if (rflags == -1) rflags = (1 << 0);
		      //if (flmt == -1) flmt = ROOM;
		      //if (flt == -1) flt = 0;

		      add_opvars(splev, "iiiiiiiiio",
				 VA_PASS10(flt, flmt, rflags,
					  (yyvsp[-6].crd).x, (yyvsp[-6].crd).y, (yyvsp[-8].crd).x, (yyvsp[-8].crd).y,
					  (yyvsp[-4].sze).width, (yyvsp[-4].sze).height, SPO_ROOM));
		      break_stmt_start();
		  }
#line 4159 "lev_yacc.c"
    break;

  case 193:
#line 1350 "lev_comp.y"
                  {
		      break_stmt_end(splev);
		      add_opcode(splev, SPO_ENDROOM, NULL);
		  }
#line 4168 "lev_yacc.c"
    break;

  case 194:
#line 1357 "lev_comp.y"
                  {
			(yyval.i) = 1;
		  }
#line 4176 "lev_yacc.c"
    break;

  case 195:
#line 1361 "lev_comp.y"
                  {
			(yyval.i) = (yyvsp[0].i);
		  }
#line 4184 "lev_yacc.c"
    break;

  case 196:
#line 1367 "lev_comp.y"
                  {
			if ( (yyvsp[-3].i) < 1 || (yyvsp[-3].i) > 5 ||
			    (yyvsp[-1].i) < 1 || (yyvsp[-1].i) > 5 ) {
			    lc_error("Room positions should be between 1-5: (%li,%li)!", (yyvsp[-3].i), (yyvsp[-1].i));
			} else {
			    (yyval.crd).x = (yyvsp[-3].i);
			    (yyval.crd).y = (yyvsp[-1].i);
			}
		  }
#line 4198 "lev_yacc.c"
    break;

  case 197:
#line 1377 "lev_comp.y"
                  {
			(yyval.crd).x = (yyval.crd).y = ERR;
		  }
#line 4206 "lev_yacc.c"
    break;

  case 198:
#line 1383 "lev_comp.y"
                  {
			if ( (yyvsp[-3].i) < 0 || (yyvsp[-1].i) < 0) {
			    lc_error("Invalid subroom position (%li,%li)!", (yyvsp[-3].i), (yyvsp[-1].i));
			} else {
			    (yyval.crd).x = (yyvsp[-3].i);
			    (yyval.crd).y = (yyvsp[-1].i);
			}
		  }
#line 4219 "lev_yacc.c"
    break;

  case 199:
#line 1392 "lev_comp.y"
                  {
			(yyval.crd).x = (yyval.crd).y = ERR;
		  }
#line 4227 "lev_yacc.c"
    break;

  case 200:
#line 1398 "lev_comp.y"
                  {
		      (yyval.crd).x = (yyvsp[-3].i);
		      (yyval.crd).y = (yyvsp[-1].i);
		  }
#line 4236 "lev_yacc.c"
    break;

  case 201:
#line 1403 "lev_comp.y"
                  {
		      (yyval.crd).x = (yyval.crd).y = ERR;
		  }
#line 4244 "lev_yacc.c"
    break;

  case 202:
#line 1409 "lev_comp.y"
                  {
			(yyval.sze).width = (yyvsp[-3].i);
			(yyval.sze).height = (yyvsp[-1].i);
		  }
#line 4253 "lev_yacc.c"
    break;

  case 203:
#line 1414 "lev_comp.y"
                  {
			(yyval.sze).height = (yyval.sze).width = ERR;
		  }
#line 4261 "lev_yacc.c"
    break;

  case 204:
#line 1420 "lev_comp.y"
                  {
			/* ERR means random here */
			if ((yyvsp[-3].i) == ERR && (yyvsp[-1].i) != ERR) {
			    lc_error("If the door wall is random, so must be its pos!");
			} else {
			    add_opvars(splev, "iiiio",
				       VA_PASS5((long)(yyvsp[-1].i), (long)(yyvsp[-5].i), (long)(yyvsp[-7].i),
						(long)(yyvsp[-3].i), SPO_ROOM_DOOR));
			}
		  }
#line 4276 "lev_yacc.c"
    break;

  case 205:
#line 1431 "lev_comp.y"
                  {
		      add_opvars(splev, "io", VA_PASS2((long)(yyvsp[-3].i), SPO_DOOR));
		  }
#line 4284 "lev_yacc.c"
    break;

  case 210:
#line 1445 "lev_comp.y"
                  {
		      (yyval.i) = (yyvsp[0].i);
		  }
#line 4292 "lev_yacc.c"
    break;

  case 211:
#line 1449 "lev_comp.y"
                  {
		      (yyval.i) = ((yyvsp[-2].i) | (yyvsp[0].i));
		  }
#line 4300 "lev_yacc.c"
    break;

  case 214:
#line 1459 "lev_comp.y"
                  {
		      struct opvar *stopit = New(struct opvar);
		      set_opvar_int(stopit, SP_D_V_END);
		      add_opcode(splev, SPO_PUSH, stopit);
		      (yyval.i) = 0x00;
		  }
#line 4311 "lev_yacc.c"
    break;

  case 215:
#line 1466 "lev_comp.y"
                  {
		      if (( (yyvsp[-2].i) & (yyvsp[0].i) ))
			  lc_error("DOOR extra info '%s' defined twice.", curr_token);
		      (yyval.i) = ( (yyvsp[-2].i) | (yyvsp[0].i) );
		  }
#line 4321 "lev_yacc.c"
    break;

  case 216:
#line 1474 "lev_comp.y"
                  {	
		      add_opvars(splev, "ii", VA_PASS2((yyvsp[0].i), SP_D_V_SUBTYPE));
		      (yyval.i) = 0x0001;
		  }
#line 4330 "lev_yacc.c"
    break;

  case 217:
#line 1479 "lev_comp.y"
                  {
		      add_opvars(splev, "i", VA_PASS1(SP_D_V_KEY_TYPE));
		      (yyval.i) = 0x0002;
		  }
#line 4339 "lev_yacc.c"
    break;

  case 218:
#line 1484 "lev_comp.y"
                  {
		      add_opvars(splev, "ii", VA_PASS2((yyvsp[0].i), SP_D_V_SPECIAL_QUALITY));
		      (yyval.i) = 0x0004;
		  }
#line 4348 "lev_yacc.c"
    break;

  case 219:
#line 1489 "lev_comp.y"
                  {
		      add_opvars(splev, "ii", VA_PASS2(1, SP_D_V_INDESTRUCTIBLE));
		      (yyval.i) = 0x0008;
		  }
#line 4357 "lev_yacc.c"
    break;

  case 220:
#line 1494 "lev_comp.y"
                  {
		      add_opvars(splev, "ii", VA_PASS2(1, SP_D_V_SECRET_DOOR));
		      (yyval.i) = 0x0010;
		  }
#line 4366 "lev_yacc.c"
    break;

  case 221:
#line 1499 "lev_comp.y"
                  {
		      add_opvars(splev, "ii", VA_PASS2(1, SP_D_V_USES_UP_KEY));
		      (yyval.i) = 0x0020;
		  }
#line 4375 "lev_yacc.c"
    break;

  case 222:
#line 1504 "lev_comp.y"
                  {
		      add_opvars(splev, "ii", VA_PASS2(1, SP_D_V_NON_PASSDOOR));
		      (yyval.i) = 0x0040;
		  }
#line 4384 "lev_yacc.c"
    break;

  case 223:
#line 1512 "lev_comp.y"
                  {
		      add_opvars(splev, "ciisiio",
				 VA_PASS7(0, 0, 1, (char *) 0, 0, 0, SPO_MAP));
		      max_x_map = COLNO-1;
		      max_y_map = ROWNO;
		  }
#line 4395 "lev_yacc.c"
    break;

  case 224:
#line 1519 "lev_comp.y"
                  {
		      add_opvars(splev, "cii",
				 VA_PASS3(SP_COORD_PACK(((yyvsp[-4].i)), ((yyvsp[-2].i))),
					  1, (int) (yyvsp[-1].i)));
		      scan_map((yyvsp[0].map), splev);
		      Free((yyvsp[0].map));
		  }
#line 4407 "lev_yacc.c"
    break;

  case 225:
#line 1527 "lev_comp.y"
                  {
		      add_opvars(splev, "ii", VA_PASS2(2, (int) (yyvsp[-1].i)));
		      scan_map((yyvsp[0].map), splev);
		      Free((yyvsp[0].map));
		  }
#line 4417 "lev_yacc.c"
    break;

  case 230:
#line 1543 "lev_comp.y"
                  {
		      add_opvars(splev, "io", VA_PASS2(0, SPO_MONSTER));
		  }
#line 4425 "lev_yacc.c"
    break;

  case 231:
#line 1547 "lev_comp.y"
                  {
		      add_opvars(splev, "io", VA_PASS2(1, SPO_MONSTER));
		      in_container_obj++;
		      break_stmt_start();
		  }
#line 4435 "lev_yacc.c"
    break;

  case 232:
#line 1553 "lev_comp.y"
                 {
		     break_stmt_end(splev);
		     in_container_obj--;
		     add_opvars(splev, "o", VA_PASS1(SPO_END_MONINVENT));
		 }
#line 4445 "lev_yacc.c"
    break;

  case 233:
#line 1561 "lev_comp.y"
                  {
		      /* nothing */
		  }
#line 4453 "lev_yacc.c"
    break;

  case 234:
#line 1567 "lev_comp.y"
                  {
		      struct opvar *stopit = New(struct opvar);

		      set_opvar_int(stopit, SP_M_V_END);
		      add_opcode(splev, SPO_PUSH, stopit);
		      (yyval.i) = 0x00000000;
		  }
#line 4465 "lev_yacc.c"
    break;

  case 235:
#line 1575 "lev_comp.y"
                  {
		      if (( (yyvsp[-2].i) & (yyvsp[0].i) ))
			  lc_error("MONSTER extra info defined twice.");
		      (yyval.i) = ( (yyvsp[-2].i) | (yyvsp[0].i) );
		  }
#line 4475 "lev_yacc.c"
    break;

  case 236:
#line 1583 "lev_comp.y"
                  {
		      add_opvars(splev, "i", VA_PASS1(SP_M_V_NAME));
		      (yyval.i) = 0x00000001;
		  }
#line 4484 "lev_yacc.c"
    break;

  case 237:
#line 1588 "lev_comp.y"
                  {
		      add_opvars(splev, "ii",
				 VA_PASS2((int) (yyvsp[0].i), SP_M_V_PEACEFUL));
		      (yyval.i) = 0x00000002;
		  }
#line 4494 "lev_yacc.c"
    break;

  case 238:
#line 1594 "lev_comp.y"
                  {
		      add_opvars(splev, "ii",
				 VA_PASS2((int) (yyvsp[0].i), SP_M_V_ASLEEP));
		      (yyval.i) = 0x00000004;
		  }
#line 4504 "lev_yacc.c"
    break;

  case 239:
#line 1600 "lev_comp.y"
                  {
		      add_opvars(splev, "ii",
				 VA_PASS2((int) (yyvsp[0].i), SP_M_V_ALIGN));
		      (yyval.i) = 0x00000008;
		  }
#line 4514 "lev_yacc.c"
    break;

  case 240:
#line 1606 "lev_comp.y"
                  {
		      add_opvars(splev, "ii",
				 VA_PASS2((int) (yyvsp[-1].i), SP_M_V_APPEAR));
		      (yyval.i) = 0x00000010;
		  }
#line 4524 "lev_yacc.c"
    break;

  case 241:
#line 1612 "lev_comp.y"
                  {
		      add_opvars(splev, "ii", VA_PASS2(1, SP_M_V_FEMALE));
		      (yyval.i) = 0x00000020;
		  }
#line 4533 "lev_yacc.c"
    break;

  case 242:
#line 1617 "lev_comp.y"
                  {
		      add_opvars(splev, "ii", VA_PASS2(0, SP_M_V_FEMALE));
		      (yyval.i) = 0x00000020;
		  }
#line 4542 "lev_yacc.c"
    break;

  case 243:
#line 1622 "lev_comp.y"
                  {
		      add_opvars(splev, "ii", VA_PASS2(1, SP_M_V_INVIS));
		      (yyval.i) = 0x00000040;
		  }
#line 4551 "lev_yacc.c"
    break;

  case 244:
#line 1627 "lev_comp.y"
                  {
		      add_opvars(splev, "ii", VA_PASS2(1, SP_M_V_CANCELLED));
		      (yyval.i) = 0x00000080;
		  }
#line 4560 "lev_yacc.c"
    break;

  case 245:
#line 1632 "lev_comp.y"
                  {
		      add_opvars(splev, "ii", VA_PASS2(1, SP_M_V_REVIVED));
		      (yyval.i) = 0x00000100;
		  }
#line 4569 "lev_yacc.c"
    break;

  case 246:
#line 1637 "lev_comp.y"
                  {
		      add_opvars(splev, "ii", VA_PASS2(1, SP_M_V_AVENGE));
		      (yyval.i) = 0x00000200;
		  }
#line 4578 "lev_yacc.c"
    break;

  case 247:
#line 1642 "lev_comp.y"
                  {
		      add_opvars(splev, "i", VA_PASS1(SP_M_V_FLEEING));
		      (yyval.i) = 0x00000400;
		  }
#line 4587 "lev_yacc.c"
    break;

  case 248:
#line 1647 "lev_comp.y"
                  {
		      add_opvars(splev, "i", VA_PASS1(SP_M_V_BLINDED));
		      (yyval.i) = 0x00000800;
		  }
#line 4596 "lev_yacc.c"
    break;

  case 249:
#line 1652 "lev_comp.y"
                  {
		      add_opvars(splev, "i", VA_PASS1(SP_M_V_PARALYZED));
		      (yyval.i) = 0x00001000;
		  }
#line 4605 "lev_yacc.c"
    break;

  case 250:
#line 1657 "lev_comp.y"
                  {
		      add_opvars(splev, "ii", VA_PASS2(1, SP_M_V_STUNNED));
		      (yyval.i) = 0x00002000;
		  }
#line 4614 "lev_yacc.c"
    break;

  case 251:
#line 1662 "lev_comp.y"
                  {
		      add_opvars(splev, "ii", VA_PASS2(1, SP_M_V_CONFUSED));
		      (yyval.i) = 0x00004000;
		  }
#line 4623 "lev_yacc.c"
    break;

  case 252:
#line 1667 "lev_comp.y"
                  {
		      add_opvars(splev, "ii",
				 VA_PASS2((int) (yyvsp[0].i), SP_M_V_SEENTRAPS));
		      (yyval.i) = 0x00008000;
		  }
#line 4633 "lev_yacc.c"
    break;

  case 253:
#line 1673 "lev_comp.y"
                  {
		      add_opvars(splev, "ii", VA_PASS2(1, SP_M_V_WAITFORU));
		      (yyval.i) = 0x00010000;
		  }
#line 4642 "lev_yacc.c"
    break;

  case 254:
#line 1678 "lev_comp.y"
                  {
		      add_opvars(splev, "ii", VA_PASS2(1, SP_M_V_PROTECTOR));
		      (yyval.i) = 0x00020000;
		  }
#line 4651 "lev_yacc.c"
    break;

  case 255:
#line 1685 "lev_comp.y"
                  {
		      int token = get_trap_type((yyvsp[0].map));

		      if (token == ERR || token == 0)
			  lc_error("Unknown trap type '%s'!", (yyvsp[0].map));
                      Free((yyvsp[0].map));
		      (yyval.i) = (1L << (token - 1));
		  }
#line 4664 "lev_yacc.c"
    break;

  case 256:
#line 1694 "lev_comp.y"
                  {
		      (yyval.i) = (long) ~0;
		  }
#line 4672 "lev_yacc.c"
    break;

  case 257:
#line 1698 "lev_comp.y"
                  {
		      int token = get_trap_type((yyvsp[-2].map));
		      if (token == ERR || token == 0)
			  lc_error("Unknown trap type '%s'!", (yyvsp[-2].map));

		      if ((1L << (token - 1)) & (yyvsp[0].i))
			  lc_error("Monster seen_traps, trap '%s' listed twice.", (yyvsp[-2].map));
                      Free((yyvsp[-2].map));
		      (yyval.i) = ((1L << (token - 1)) | (yyvsp[0].i));
		  }
#line 4687 "lev_yacc.c"
    break;

  case 258:
#line 1711 "lev_comp.y"
                  {
		      int cnt = 0;

		      if (in_container_obj)
                          cnt |= SP_OBJ_CONTENT;
		      add_opvars(splev, "io", VA_PASS2(cnt, SPO_OBJECT));
		  }
#line 4699 "lev_yacc.c"
    break;

  case 259:
#line 1719 "lev_comp.y"
                  {
		      int cnt = SP_OBJ_CONTAINER;

		      if (in_container_obj)
                          cnt |= SP_OBJ_CONTENT;
		      add_opvars(splev, "io", VA_PASS2(cnt, SPO_OBJECT));
		      in_container_obj++;
		      break_stmt_start();
		  }
#line 4713 "lev_yacc.c"
    break;

  case 260:
#line 1729 "lev_comp.y"
                 {
		     break_stmt_end(splev);
		     in_container_obj--;
		     add_opcode(splev, SPO_POP_CONTAINER, NULL);
		 }
#line 4723 "lev_yacc.c"
    break;

  case 261:
#line 1737 "lev_comp.y"
                  {
		      if (( (yyvsp[0].i) & 0x4000) && in_container_obj)
                          lc_error("Object cannot have a coord when contained.");
		      else if (!( (yyvsp[0].i) & 0x4000) && !in_container_obj)
                          lc_error("Object needs a coord when not contained.");
		  }
#line 4734 "lev_yacc.c"
    break;

  case 262:
#line 1746 "lev_comp.y"
                  {
		      struct opvar *stopit = New(struct opvar);
		      set_opvar_int(stopit, SP_O_V_END);
		      add_opcode(splev, SPO_PUSH, stopit);
		      (yyval.i) = 0x00;
		  }
#line 4745 "lev_yacc.c"
    break;

  case 263:
#line 1753 "lev_comp.y"
                  {
		      if (( (yyvsp[-2].i) & (yyvsp[0].i) ))
			  lc_error("OBJECT extra info '%s' defined twice.", curr_token);
		      (yyval.i) = ( (yyvsp[-2].i) | (yyvsp[0].i) );
		  }
#line 4755 "lev_yacc.c"
    break;

  case 264:
#line 1761 "lev_comp.y"
                  {
		      add_opvars(splev, "ii",
				 VA_PASS2((int) (yyvsp[0].i), SP_O_V_CURSE));
		      (yyval.i) = 0x0001;
		  }
#line 4765 "lev_yacc.c"
    break;

  case 265:
#line 1767 "lev_comp.y"
                  {
		      add_opvars(splev, "i", VA_PASS1(SP_O_V_CORPSENM));
		      (yyval.i) = 0x0002;
		  }
#line 4774 "lev_yacc.c"
    break;

  case 266:
#line 1772 "lev_comp.y"
                  {
		      add_opvars(splev, "i", VA_PASS1(SP_O_V_SPE));
		      (yyval.i) = 0x0004;
		  }
#line 4783 "lev_yacc.c"
    break;

  case 267:
#line 1777 "lev_comp.y"
                  {
		      add_opvars(splev, "i", VA_PASS1(SP_O_V_NAME));
		      (yyval.i) = 0x0008;
		  }
#line 4792 "lev_yacc.c"
    break;

  case 268:
#line 1782 "lev_comp.y"
                  {
		      add_opvars(splev, "i", VA_PASS1(SP_O_V_QUAN));
		      (yyval.i) = 0x0010;
		  }
#line 4801 "lev_yacc.c"
    break;

  case 269:
#line 1787 "lev_comp.y"
                  {
		      add_opvars(splev, "ii", VA_PASS2(1, SP_O_V_BURIED));
		      (yyval.i) = 0x0020;
		  }
#line 4810 "lev_yacc.c"
    break;

  case 270:
#line 1792 "lev_comp.y"
                  {
		      add_opvars(splev, "ii", VA_PASS2((int) (yyvsp[0].i), SP_O_V_LIT));
		      (yyval.i) = 0x0040;
		  }
#line 4819 "lev_yacc.c"
    break;

  case 271:
#line 1797 "lev_comp.y"
                  {
		      add_opvars(splev, "i", VA_PASS1(SP_O_V_ERODED));
		      (yyval.i) = 0x0080;
		  }
#line 4828 "lev_yacc.c"
    break;

  case 272:
#line 1802 "lev_comp.y"
                  {
		      add_opvars(splev, "ii", VA_PASS2(-1, SP_O_V_ERODED));
		      (yyval.i) = 0x0080;
		  }
#line 4837 "lev_yacc.c"
    break;

  case 273:
#line 1807 "lev_comp.y"
                  {
		      if ((yyvsp[0].i) == D_LOCKED) {
			  add_opvars(splev, "ii", VA_PASS2(1, SP_O_V_LOCKED));
			  (yyval.i) = 0x0100;
		      } else if ((yyvsp[0].i) == D_BROKEN) {
			  add_opvars(splev, "ii", VA_PASS2(1, SP_O_V_BROKEN));
			  (yyval.i) = 0x0200;
		      } else if ((yyvsp[0].i) == D_ISOPEN) {
			  add_opvars(splev, "ii", VA_PASS2(1, SP_O_V_OPEN));
			  (yyval.i) = 0x2000000;
		      } else
			  lc_error("DOOR state can only be locked or broken.");
		  }
#line 4855 "lev_yacc.c"
    break;

  case 274:
#line 1821 "lev_comp.y"
                  {
		      add_opvars(splev, "ii",
                                 VA_PASS2((int) (yyvsp[0].i), SP_O_V_TRAPPED));
		      (yyval.i) = 0x0400;
		  }
#line 4865 "lev_yacc.c"
    break;

  case 275:
#line 1827 "lev_comp.y"
                  {
		      add_opvars(splev, "i", VA_PASS1(SP_O_V_RECHARGED));
		      (yyval.i) = 0x0800;
		  }
#line 4874 "lev_yacc.c"
    break;

  case 276:
#line 1832 "lev_comp.y"
                  {
		      add_opvars(splev, "ii", VA_PASS2(1, SP_O_V_INVIS));
		      (yyval.i) = 0x1000;
		  }
#line 4883 "lev_yacc.c"
    break;

  case 277:
#line 1837 "lev_comp.y"
                  {
		      add_opvars(splev, "ii", VA_PASS2(1, SP_O_V_GREASED));
		      (yyval.i) = 0x2000;
		  }
#line 4892 "lev_yacc.c"
    break;

  case 278:
#line 1842 "lev_comp.y"
                  {
		      add_opvars(splev, "i", VA_PASS1(SP_O_V_COORD));
		      (yyval.i) = 0x4000;
		  }
#line 4901 "lev_yacc.c"
    break;

  case 279:
#line 1847 "lev_comp.y"
                  {
		      add_opvars(splev, "ii", VA_PASS2((int)(yyvsp[0].i), SP_O_V_ELEMENTAL_ENCHANTMENT));
		      (yyval.i) = 0x8000;
		  }
#line 4910 "lev_yacc.c"
    break;

  case 280:
#line 1852 "lev_comp.y"
                  {
		      add_opvars(splev, "ii", VA_PASS2((int)(yyvsp[0].i), SP_O_V_EXCEPTIONALITY));
		      (yyval.i) = 0x10000;
		  }
#line 4919 "lev_yacc.c"
    break;

  case 281:
#line 1857 "lev_comp.y"
                  {
		      add_opvars(splev, "i", VA_PASS1(SP_O_V_ENCHANTMENT));
		      (yyval.i) = 0x20000;
		  }
#line 4928 "lev_yacc.c"
    break;

  case 282:
#line 1862 "lev_comp.y"
                  {
		      add_opvars(splev, "i", VA_PASS1(SP_O_V_CHARGES));
		      (yyval.i) = 0x40000;
		  }
#line 4937 "lev_yacc.c"
    break;

  case 283:
#line 1867 "lev_comp.y"
                  {
		      add_opvars(splev, "i", VA_PASS1(SP_O_V_SPECIAL_QUALITY));
		      (yyval.i) = 0x80000;
		  }
#line 4946 "lev_yacc.c"
    break;

  case 284:
#line 1872 "lev_comp.y"
                  {
		      add_opvars(splev, "ii", VA_PASS2((int)(yyvsp[0].i), SP_O_V_SPECIAL_QUALITY));
		      (yyval.i) = 0x80000;
		  }
#line 4955 "lev_yacc.c"
    break;

  case 285:
#line 1877 "lev_comp.y"
                  {
		      add_opvars(splev, "i", VA_PASS1(SP_O_V_SPEFLAGS));
		      (yyval.i) = 0x100000;
		  }
#line 4964 "lev_yacc.c"
    break;

  case 286:
#line 1882 "lev_comp.y"
                  {
		      add_opvars(splev, "i", VA_PASS1(SP_O_V_KEY_TYPE));
		      (yyval.i) = 0x200000;
		  }
#line 4973 "lev_yacc.c"
    break;

  case 287:
#line 1887 "lev_comp.y"
                  {
		      add_opvars(splev, "ii", VA_PASS2(1, SP_O_V_INDESTRUCTIBLE));
		      (yyval.i) = 0x400000;
		  }
#line 4982 "lev_yacc.c"
    break;

  case 288:
#line 1892 "lev_comp.y"
                  {
		      add_opvars(splev, "ii", VA_PASS2(1, SP_O_V_USES_UP_KEY));
		      (yyval.i) = 0x800000;
		  }
#line 4991 "lev_yacc.c"
    break;

  case 289:
#line 1897 "lev_comp.y"
                  {
		      add_opvars(splev, "ii", VA_PASS2(1, SP_O_V_NO_PICKUP));
		      (yyval.i) = 0x1000000;
		  }
#line 5000 "lev_yacc.c"
    break;

  case 290:
#line 1902 "lev_comp.y"
                  {
		      add_opvars(splev, "ii",
				 VA_PASS2((int) (yyvsp[0].i), SP_O_V_MYTHIC_TYPE));
		      (yyval.i) = 0x2000000;
		  }
#line 5010 "lev_yacc.c"
    break;

  case 291:
#line 1908 "lev_comp.y"
                  {
		      add_opvars(splev, "ii", VA_PASS2((int)(yyvsp[0].i), SP_O_V_MYTHIC_PREFIX));
		      (yyval.i) = 0x4000000;
		  }
#line 5019 "lev_yacc.c"
    break;

  case 292:
#line 1913 "lev_comp.y"
                  {
		      add_opvars(splev, "ii", VA_PASS2((int)(yyvsp[0].i), SP_O_V_MYTHIC_SUFFIX));
		      (yyval.i) = 0x8000000;
		  }
#line 5028 "lev_yacc.c"
    break;

  case 293:
#line 1918 "lev_comp.y"
                  {
		      add_opvars(splev, "i", VA_PASS1(SP_O_V_AGE));
		      (yyval.i) = 0x10000000;
		  }
#line 5037 "lev_yacc.c"
    break;

  case 294:
#line 1925 "lev_comp.y"
                  {
		      add_opvars(splev, "io", VA_PASS2((int) (yyvsp[-2].i), SPO_TRAP));
		  }
#line 5045 "lev_yacc.c"
    break;

  case 295:
#line 1931 "lev_comp.y"
                   {
		       long dir, state = 0;

		       /* convert dir from a DIRECTION to a DB_DIR */
		       dir = (yyvsp[-2].i);
		       switch (dir) {
		       case W_NORTH: dir = DB_NORTH; break;
		       case W_SOUTH: dir = DB_SOUTH; break;
		       case W_EAST:  dir = DB_EAST;  break;
		       case W_WEST:  dir = DB_WEST;  break;
		       default:
			   lc_error("Invalid drawbridge direction.");
			   break;
		       }

		       if ( (yyvsp[0].i) == D_ISOPEN )
			   state = 1;
		       else if ( (yyvsp[0].i) == D_CLOSED )
			   state = 0;
		       else if ( (yyvsp[0].i) == -1 )
			   state = -1;
		       else
			   lc_error("A drawbridge can only be open, closed or random!");
		       add_opvars(splev, "iio",
				  VA_PASS3(state, dir, SPO_DRAWBRIDGE));
		   }
#line 5076 "lev_yacc.c"
    break;

  case 296:
#line 1960 "lev_comp.y"
                  {
		      add_opvars(splev, "iiio",
				 VA_PASS4((int) (yyvsp[0].i), 1, 0, SPO_MAZEWALK));
		  }
#line 5085 "lev_yacc.c"
    break;

  case 297:
#line 1965 "lev_comp.y"
                  {
		      add_opvars(splev, "iiio",
				 VA_PASS4((int) (yyvsp[-3].i), (int) (yyvsp[-1].i),
					  (int) (yyvsp[0].i), SPO_MAZEWALK));
		  }
#line 5095 "lev_yacc.c"
    break;

  case 298:
#line 1973 "lev_comp.y"
                  {
		      add_opvars(splev, "rio",
				 VA_PASS3(SP_REGION_PACK(-1,-1,-1,-1),
					  0, SPO_WALLIFY));
		  }
#line 5105 "lev_yacc.c"
    break;

  case 299:
#line 1979 "lev_comp.y"
                  {
		      add_opvars(splev, "io", VA_PASS2(1, SPO_WALLIFY));
		  }
#line 5113 "lev_yacc.c"
    break;

  case 300:
#line 1983 "lev_comp.y"
                  {
		      add_opvars(splev, "rio",
				 VA_PASS3(SP_REGION_PACK(-1,-1,-1,-1),
					  2, SPO_WALLIFY));
		  }
#line 5123 "lev_yacc.c"
    break;

  case 301:
#line 1991 "lev_comp.y"
                  {
		      add_opvars(splev, "io",
				 VA_PASS2((int) (yyvsp[0].i), SPO_LADDER));
		  }
#line 5132 "lev_yacc.c"
    break;

  case 302:
#line 1998 "lev_comp.y"
                  {
		      add_opvars(splev, "io",
				 VA_PASS2((int) (yyvsp[0].i), SPO_STAIR));
		  }
#line 5141 "lev_yacc.c"
    break;

  case 303:
#line 2005 "lev_comp.y"
                  {
		      add_opvars(splev, "iiiii iiiii iiso",
				 VA_PASS14((yyvsp[-4].lregn).x1, (yyvsp[-4].lregn).y1, (yyvsp[-4].lregn).x2, (yyvsp[-4].lregn).y2, (yyvsp[-4].lregn).area,
					   (yyvsp[-2].lregn).x1, (yyvsp[-2].lregn).y1, (yyvsp[-2].lregn).x2, (yyvsp[-2].lregn).y2, (yyvsp[-2].lregn).area,
				     (long) (((yyvsp[0].i)) ? LR_UPSTAIR : LR_DOWNSTAIR),
					   0, (char *) 0, SPO_LEVREGION));
		  }
#line 5153 "lev_yacc.c"
    break;

  case 304:
#line 2015 "lev_comp.y"
                  {
		      add_opvars(splev, "iiiii iiiii iiso",
				 VA_PASS14((yyvsp[-4].lregn).x1, (yyvsp[-4].lregn).y1, (yyvsp[-4].lregn).x2, (yyvsp[-4].lregn).y2, (yyvsp[-4].lregn).area,
					   (yyvsp[-2].lregn).x1, (yyvsp[-2].lregn).y1, (yyvsp[-2].lregn).x2, (yyvsp[-2].lregn).y2, (yyvsp[-2].lregn).area,
					   LR_PORTAL, 0, (yyvsp[0].map), SPO_LEVREGION));
		      Free((yyvsp[0].map));
		  }
#line 5165 "lev_yacc.c"
    break;

  case 305:
#line 2025 "lev_comp.y"
                  {
		      long rtyp = 0;
		      switch((yyvsp[0].i)) {
		      case -1: rtyp = LR_TELE; break;
		      case  0: rtyp = LR_DOWNTELE; break;
		      case  1: rtyp = LR_UPTELE; break;
		      case  2: rtyp = LR_NOTELE; break;
		      }
		      add_opvars(splev, "iiiii iiiii iiso",
				 VA_PASS14((yyvsp[-3].lregn).x1, (yyvsp[-3].lregn).y1, (yyvsp[-3].lregn).x2, (yyvsp[-3].lregn).y2, (yyvsp[-3].lregn).area,
					   (yyvsp[-1].lregn).x1, (yyvsp[-1].lregn).y1, (yyvsp[-1].lregn).x2, (yyvsp[-1].lregn).y2, (yyvsp[-1].lregn).area,
					   rtyp, 0, (char *)0, SPO_LEVREGION));
		  }
#line 5183 "lev_yacc.c"
    break;

  case 306:
#line 2041 "lev_comp.y"
                  {
		      add_opvars(splev, "iiiii iiiii iiso",
				 VA_PASS14((yyvsp[-2].lregn).x1, (yyvsp[-2].lregn).y1, (yyvsp[-2].lregn).x2, (yyvsp[-2].lregn).y2, (yyvsp[-2].lregn).area,
					   (yyvsp[0].lregn).x1, (yyvsp[0].lregn).y1, (yyvsp[0].lregn).x2, (yyvsp[0].lregn).y2, (yyvsp[0].lregn).area,
					   (long) LR_BRANCH, 0,
					   (char *) 0, SPO_LEVREGION));
		  }
#line 5195 "lev_yacc.c"
    break;

  case 307:
#line 2051 "lev_comp.y"
                  {
			(yyval.i) = -1;
		  }
#line 5203 "lev_yacc.c"
    break;

  case 308:
#line 2055 "lev_comp.y"
                  {
			(yyval.i) = (yyvsp[0].i);
		  }
#line 5211 "lev_yacc.c"
    break;

  case 309:
#line 2061 "lev_comp.y"
                  {
		      add_opvars(splev, "io", VA_PASS2((int)(yyvsp[0].i), SPO_FOUNTAIN));
		  }
#line 5219 "lev_yacc.c"
    break;

  case 310:
#line 2067 "lev_comp.y"
                  {
		      add_opvars(splev, "o", VA_PASS1(SPO_THRONE));
		  }
#line 5227 "lev_yacc.c"
    break;

  case 311:
#line 2073 "lev_comp.y"
                  {
		      add_opvars(splev, "iio", VA_PASS3((int) (yyvsp[-2].i), (int) (yyvsp[0].i), SPO_MODRON_PORTAL));
		  }
#line 5235 "lev_yacc.c"
    break;

  case 312:
#line 2077 "lev_comp.y"
                  {
		      add_opvars(splev, "iiiio", VA_PASS5((int) (yyvsp[-6].i), (int) (yyvsp[-4].i), (int) (yyvsp[-2].i), (int) (yyvsp[0].i), SPO_MODRON_LEVEL_TELEPORTER));
		  }
#line 5243 "lev_yacc.c"
    break;

  case 313:
#line 2083 "lev_comp.y"
                  {
		      add_opvars(splev, "io", VA_PASS2((int) (yyvsp[-1].i), SPO_LEVER));
		  }
#line 5251 "lev_yacc.c"
    break;

  case 314:
#line 2089 "lev_comp.y"
                  {
		      struct opvar *stopit = New(struct opvar);
		      set_opvar_int(stopit, SP_L_V_END);
		      add_opcode(splev, SPO_PUSH, stopit);
		      (yyval.i) = 0x00;
		  }
#line 5262 "lev_yacc.c"
    break;

  case 315:
#line 2096 "lev_comp.y"
                  {
		      if (( (yyvsp[-2].i) & (yyvsp[0].i) ))
			  lc_error("LEVER extra info '%s' defined twice.", curr_token);
		      (yyval.i) = ( (yyvsp[-2].i) | (yyvsp[0].i) );
		  }
#line 5272 "lev_yacc.c"
    break;

  case 316:
#line 2104 "lev_comp.y"
                  {	
		      add_opvars(splev, "ii",
				 VA_PASS2((int) (yyvsp[0].i), SP_L_V_ACTIVE));
		      (yyval.i) = 0x0001;
		  }
#line 5282 "lev_yacc.c"
    break;

  case 317:
#line 2110 "lev_comp.y"
                  {
		      add_opvars(splev, "i", VA_PASS1(SP_L_V_MONSTER));
		      (yyval.i) = 0x0002;
		  }
#line 5291 "lev_yacc.c"
    break;

  case 318:
#line 2115 "lev_comp.y"
                  {
		      add_opvars(splev, "i", VA_PASS1(SP_L_V_OBJECT));
		      (yyval.i) = 0x0004;
		  }
#line 5300 "lev_yacc.c"
    break;

  case 319:
#line 2120 "lev_comp.y"
                  {
		      add_opvars(splev, "ii", VA_PASS2((int)(yyvsp[0].terr).ter, SP_L_V_TERRAIN));
		      (yyval.i) = 0x0008;
		  }
#line 5309 "lev_yacc.c"
    break;

  case 320:
#line 2125 "lev_comp.y"
                  {
		      add_opvars(splev, "ii", VA_PASS2((int)(yyvsp[0].terr).ter, SP_L_V_TERRAIN2));
		      (yyval.i) = 0x0010;
		  }
#line 5318 "lev_yacc.c"
    break;

  case 321:
#line 2130 "lev_comp.y"
                  {	
		      add_opvars(splev, "ii",
				 VA_PASS2(1, SP_L_V_SWITCHABLE));
		      (yyval.i) = 0x0020;
		  }
#line 5328 "lev_yacc.c"
    break;

  case 322:
#line 2136 "lev_comp.y"
                  {	
		      add_opvars(splev, "ii",
				 VA_PASS2(1, SP_L_V_CONTINUOUS));
		      (yyval.i) = 0x0040;
		  }
#line 5338 "lev_yacc.c"
    break;

  case 323:
#line 2142 "lev_comp.y"
                  {
		      add_opvars(splev, "i", VA_PASS1(SP_L_V_COORD));
		      (yyval.i) = 0x0080;
		  }
#line 5347 "lev_yacc.c"
    break;

  case 324:
#line 2147 "lev_comp.y"
                  {
		      add_opvars(splev, "ii", VA_PASS2((yyvsp[0].i), SP_L_V_TRAP));
		      (yyval.i) = 0x0100;
		  }
#line 5356 "lev_yacc.c"
    break;

  case 325:
#line 2152 "lev_comp.y"
                  {
		      add_opvars(splev, "ii", VA_PASS2((yyvsp[0].i), SP_L_V_SUBTYPE));
		      (yyval.i) = 0x0200;
		  }
#line 5365 "lev_yacc.c"
    break;

  case 326:
#line 2157 "lev_comp.y"
                  {
		      add_opvars(splev, "ii", VA_PASS2((yyvsp[0].i), SP_L_V_FLOOR_SUBTYPE));
		      (yyval.i) = 0x0400;
		  }
#line 5374 "lev_yacc.c"
    break;

  case 327:
#line 2162 "lev_comp.y"
                  {
		      add_opvars(splev, "ii", VA_PASS2((yyvsp[0].i), SP_L_V_EFFECT_FLAG));
		      (yyval.i) = 0x0800;
		  }
#line 5383 "lev_yacc.c"
    break;

  case 328:
#line 2167 "lev_comp.y"
                  {
		      add_opvars(splev, "ii", VA_PASS2((yyvsp[0].i), SP_L_V_SPECIAL_QUALITY));
		      (yyval.i) = 0x1000;
		  }
#line 5392 "lev_yacc.c"
    break;

  case 333:
#line 2177 "lev_comp.y"
                  {
		      add_opvars(splev, "o", VA_PASS1(SPO_SINK));
		  }
#line 5400 "lev_yacc.c"
    break;

  case 334:
#line 2183 "lev_comp.y"
                  {
		      add_opvars(splev, "o", VA_PASS1(SPO_POOL));
		  }
#line 5408 "lev_yacc.c"
    break;

  case 335:
#line 2189 "lev_comp.y"
                  {
		      (yyval.terr).lit = -2;
		      (yyval.terr).ter = what_map_char((char) (yyvsp[0].i));
		  }
#line 5417 "lev_yacc.c"
    break;

  case 336:
#line 2194 "lev_comp.y"
                  {
		      (yyval.terr).lit = (yyvsp[-1].i);
		      (yyval.terr).ter = what_map_char((char) (yyvsp[-3].i));
		  }
#line 5426 "lev_yacc.c"
    break;

  case 337:
#line 2201 "lev_comp.y"
                  {
		      add_opvars(splev, "io",
				 VA_PASS2((yyvsp[0].i), SPO_REPLACETERRAIN));
		  }
#line 5435 "lev_yacc.c"
    break;

  case 338:
#line 2208 "lev_comp.y"
                 {
		     add_opvars(splev, "io", VA_PASS2(-1, SPO_TERRAIN));
		 }
#line 5443 "lev_yacc.c"
    break;

  case 339:
#line 2212 "lev_comp.y"
                 {
		     add_opvars(splev, "io", VA_PASS2((yyvsp[0].i), SPO_TERRAIN));
		 }
#line 5451 "lev_yacc.c"
    break;

  case 340:
#line 2218 "lev_comp.y"
                  {
		      add_opvars(splev, "o", VA_PASS1(SPO_NON_DIGGABLE));
		  }
#line 5459 "lev_yacc.c"
    break;

  case 341:
#line 2224 "lev_comp.y"
                  {
		      add_opvars(splev, "o", VA_PASS1(SPO_NON_PASSWALL));
		  }
#line 5467 "lev_yacc.c"
    break;

  case 342:
#line 2230 "lev_comp.y"
                  {
		      add_opvars(splev, "Miso", VA_PASS4(-1, (yyvsp[0].i), (yyvsp[-2].map), SPO_NAMING));
		      Free((yyvsp[-2].map));
		  }
#line 5476 "lev_yacc.c"
    break;

  case 343:
#line 2235 "lev_comp.y"
                  {
		      add_opvars(splev, "iso", VA_PASS3((yyvsp[-4].i), (yyvsp[-6].map), SPO_NAMING));
		      Free((yyvsp[-6].map));
		  }
#line 5485 "lev_yacc.c"
    break;

  case 344:
#line 2242 "lev_comp.y"
                  {
		      add_opvars(splev, "io", VA_PASS2((yyvsp[0].i), SPO_SPECIAL_REGION));
		  }
#line 5493 "lev_yacc.c"
    break;

  case 345:
#line 2248 "lev_comp.y"
                  {
		      add_opvars(splev, "iiiii iiiii iiso",
				 VA_PASS14((yyvsp[-2].lregn).x1, (yyvsp[-2].lregn).y1, (yyvsp[-2].lregn).x2, (yyvsp[-2].lregn).y2, (yyvsp[-2].lregn).area,
					   0, 0, 0, 0, 1,
					   (yyvsp[0].i) == REGION_SPECIAL_LEVEL_SEEN ? (long) LR_SPECIAL_MAP_SEEN : (long) LR_SPECIAL_MAP_NAME_REVEALED, 0,
					   (char *) 0, SPO_LEVREGION));
		  }
#line 5505 "lev_yacc.c"
    break;

  case 346:
#line 2258 "lev_comp.y"
                  {
		      add_opvars(splev, "io", VA_PASS2((yyvsp[0].i), SPO_SPECIAL_TILESET));
		  }
#line 5513 "lev_yacc.c"
    break;

  case 347:
#line 2264 "lev_comp.y"
                  {
		      long irr;
		      long rt = (yyvsp[-4].i);
		      long rflags = (yyvsp[-3].i);
		      long flmt = (long)(yyvsp[-2].i);
		      long flt = (long)(yyvsp[-1].i);

		      if (rflags == -1) rflags = (1 << 0);
		      //if (flmt == -1) flmt = 0;
		      //if (flt == -1) flt = 0;

		      if (!(rflags & 1)) rt += MAXRTYPE+1;
		      irr = ((rflags & 2) != 0);
		      add_opvars(splev, "iiiiio",
				 VA_PASS6((long)(yyvsp[-6].i), rt, rflags, flmt, flt, SPO_REGION));
		      (yyval.i) = (irr || (rflags & 1) || rt != OROOM);
		      break_stmt_start();
		  }
#line 5536 "lev_yacc.c"
    break;

  case 348:
#line 2283 "lev_comp.y"
                  {
		      break_stmt_end(splev);
			  add_opcode(splev, SPO_ENDROOM, NULL);
		      /*if ( $<i>9 ||  $<i>10 ||  $<i>11 ) {
			  	add_opcode(splev, SPO_ENDROOM, NULL);
		       } else if ( $<i>12 )
			  	lc_error("Cannot use lev statements in non-permanent REGION");
		      */
		  }
#line 5550 "lev_yacc.c"
    break;

  case 349:
#line 2295 "lev_comp.y"
                  {
		      (yyval.i) = 0;
		  }
#line 5558 "lev_yacc.c"
    break;

  case 350:
#line 2299 "lev_comp.y"
                  {
		      (yyval.i) = (yyvsp[0].i);
		  }
#line 5566 "lev_yacc.c"
    break;

  case 351:
#line 2305 "lev_comp.y"
                  {
		      add_opvars(splev, "Miiio",
				 VA_PASS5(-1, 0, (long)(yyvsp[0].i), (long)(yyvsp[-2].i), SPO_ALTAR));
		  }
#line 5575 "lev_yacc.c"
    break;

  case 352:
#line 2310 "lev_comp.y"
                  {
		      add_opvars(splev, "iiio",
				 VA_PASS4(0, (long)(yyvsp[-4].i), (long)(yyvsp[-6].i), SPO_ALTAR));
		  }
#line 5584 "lev_yacc.c"
    break;

  case 353:
#line 2315 "lev_comp.y"
                  {
		      add_opvars(splev, "Miiio",
				 VA_PASS5(-1, (long)(yyvsp[0].i), (long)(yyvsp[-2].i), (long)(yyvsp[-4].i), SPO_ALTAR));
		  }
#line 5593 "lev_yacc.c"
    break;

  case 354:
#line 2320 "lev_comp.y"
                  {
		      add_opvars(splev, "iiio",
				 VA_PASS4((long)(yyvsp[-4].i), (long)(yyvsp[-6].i), (long)(yyvsp[-8].i), SPO_ALTAR));
		  }
#line 5602 "lev_yacc.c"
    break;

  case 355:
#line 2326 "lev_comp.y"
                  {
		      add_opvars(splev, "Mo", VA_PASS2(-1, SPO_ANVIL));
		  }
#line 5610 "lev_yacc.c"
    break;

  case 356:
#line 2330 "lev_comp.y"
                  {
		      add_opvars(splev, "o", VA_PASS1(SPO_ANVIL));
		  }
#line 5618 "lev_yacc.c"
    break;

  case 357:
#line 2336 "lev_comp.y"
                  {
		      add_opvars(splev, "iio", VA_PASS3((int)(yyvsp[0].i), (int)(yyvsp[-2].i), SPO_FLOOR));
		  }
#line 5626 "lev_yacc.c"
    break;

  case 358:
#line 2342 "lev_comp.y"
                  {
		      add_opvars(splev, "iio", VA_PASS3((int)(yyvsp[0].i), -1, SPO_SUBTYPE));
		  }
#line 5634 "lev_yacc.c"
    break;

  case 359:
#line 2346 "lev_comp.y"
                  {
		      add_opvars(splev, "iio", VA_PASS3((int)(yyvsp[0].i), (int)(yyvsp[-2].i), SPO_SUBTYPE));
		  }
#line 5642 "lev_yacc.c"
    break;

  case 360:
#line 2352 "lev_comp.y"
                  {
		      add_opvars(splev, "Mio", VA_PASS3(-1, (int)(yyvsp[-2].i), SPO_NPC));
		  }
#line 5650 "lev_yacc.c"
    break;

  case 361:
#line 2356 "lev_comp.y"
                  {
		      add_opvars(splev, "io", VA_PASS2((int)(yyvsp[-6].i), SPO_NPC));
		  }
#line 5658 "lev_yacc.c"
    break;

  case 362:
#line 2362 "lev_comp.y"
                  {
		      add_opvars(splev, "io", VA_PASS2(2, SPO_GRAVE));
		  }
#line 5666 "lev_yacc.c"
    break;

  case 363:
#line 2366 "lev_comp.y"
                  {
		      add_opvars(splev, "sio",
				 VA_PASS3((char *)0, 1, SPO_GRAVE));
		  }
#line 5675 "lev_yacc.c"
    break;

  case 364:
#line 2371 "lev_comp.y"
                  {
		      add_opvars(splev, "sio",
				 VA_PASS3((char *)0, 0, SPO_GRAVE));
		  }
#line 5684 "lev_yacc.c"
    break;

  case 365:
#line 2378 "lev_comp.y"
                  {
		      add_opvars(splev, "iio", VA_PASS3((int)(yyvsp[0].i), (int)(yyvsp[-2].i), SPO_BRAZIER));
		  }
#line 5692 "lev_yacc.c"
    break;

  case 366:
#line 2382 "lev_comp.y"
                  {
		      add_opvars(splev, "iio",
				 VA_PASS3(1, 0, SPO_BRAZIER));
		  }
#line 5701 "lev_yacc.c"
    break;

  case 367:
#line 2389 "lev_comp.y"
                  {
		      add_opvars(splev, "iio", VA_PASS3(2, (int)(yyvsp[-2].i), SPO_SIGNPOST));
		  }
#line 5709 "lev_yacc.c"
    break;

  case 368:
#line 2393 "lev_comp.y"
                  {
		      add_opvars(splev, "siio",
				 VA_PASS4((char *)0, 1, (int)(yyvsp[-2].i), SPO_SIGNPOST));
		  }
#line 5718 "lev_yacc.c"
    break;

  case 369:
#line 2398 "lev_comp.y"
                  {
		      add_opvars(splev, "sio",
				 VA_PASS4((char *)0, 0, 0, SPO_SIGNPOST));
		  }
#line 5727 "lev_yacc.c"
    break;

  case 370:
#line 2405 "lev_comp.y"
                  {
		      add_opvars(splev, "iio", VA_PASS3(-1, (int)(yyvsp[0].i), SPO_TREE));
		  }
#line 5735 "lev_yacc.c"
    break;

  case 371:
#line 2409 "lev_comp.y"
                  {
		      add_opvars(splev, "iio",
				 VA_PASS3((int)(yyvsp[0].i), -1, SPO_TREE));
		  }
#line 5744 "lev_yacc.c"
    break;

  case 372:
#line 2414 "lev_comp.y"
                  {
		      add_opvars(splev, "iio",
				 VA_PASS3(-1, -1, SPO_TREE));
		  }
#line 5753 "lev_yacc.c"
    break;

  case 373:
#line 2422 "lev_comp.y"
                  {
		      add_opvars(splev, "o", VA_PASS1(SPO_GOLD));
		  }
#line 5761 "lev_yacc.c"
    break;

  case 374:
#line 2428 "lev_comp.y"
                  {
		      add_opvars(splev, "io",
				 VA_PASS2((long)(yyvsp[-2].i), SPO_ENGRAVING));
		  }
#line 5770 "lev_yacc.c"
    break;

  case 375:
#line 2435 "lev_comp.y"
                  {
		      add_opvars(splev, "o", VA_PASS1(SPO_MINERALIZE));
		  }
#line 5778 "lev_yacc.c"
    break;

  case 376:
#line 2439 "lev_comp.y"
                  {
		      add_opvars(splev, "iiiio",
				 VA_PASS5(-1L, -1L, -1L, -1L, SPO_MINERALIZE));
		  }
#line 5787 "lev_yacc.c"
    break;

  case 377:
#line 2446 "lev_comp.y"
                  {
			int token = get_trap_type((yyvsp[0].map));
			if (token == ERR)
			    lc_error("Unknown trap type '%s'!", (yyvsp[0].map));
			(yyval.i) = token;
			Free((yyvsp[0].map));
		  }
#line 5799 "lev_yacc.c"
    break;

  case 379:
#line 2457 "lev_comp.y"
                  {
			int token = get_room_type((yyvsp[0].map));
			if (token == ERR) {
			    lc_warning("Unknown room type \"%s\"!  Making ordinary room...", (yyvsp[0].map));
				(yyval.i) = OROOM;
			} else
				(yyval.i) = token;
			Free((yyvsp[0].map));
		  }
#line 5813 "lev_yacc.c"
    break;

  case 381:
#line 2470 "lev_comp.y"
                  {
			(yyval.i) = -1;
		  }
#line 5821 "lev_yacc.c"
    break;

  case 382:
#line 2474 "lev_comp.y"
                  {
			(yyval.i) = (yyvsp[0].i);
		  }
#line 5829 "lev_yacc.c"
    break;

  case 383:
#line 2480 "lev_comp.y"
                  {
			(yyval.i) = (yyvsp[0].i);
		  }
#line 5837 "lev_yacc.c"
    break;

  case 384:
#line 2484 "lev_comp.y"
                  {
			(yyval.i) = (yyvsp[-2].i) | (yyvsp[0].i);
		  }
#line 5845 "lev_yacc.c"
    break;

  case 385:
#line 2491 "lev_comp.y"
                  {
		      (yyval.i) = ((yyvsp[0].i) << 0);
		  }
#line 5853 "lev_yacc.c"
    break;

  case 386:
#line 2495 "lev_comp.y"
                  {
		      (yyval.i) = ((yyvsp[0].i) << 1);
		  }
#line 5861 "lev_yacc.c"
    break;

  case 387:
#line 2499 "lev_comp.y"
                  {
		      (yyval.i) = ((yyvsp[0].i) << 2);
		  }
#line 5869 "lev_yacc.c"
    break;

  case 388:
#line 2505 "lev_comp.y"
                  {
			(yyval.i) = -1;
		  }
#line 5877 "lev_yacc.c"
    break;

  case 389:
#line 2509 "lev_comp.y"
                  {
			(yyval.i) = (yyvsp[0].i);
		  }
#line 5885 "lev_yacc.c"
    break;

  case 390:
#line 2515 "lev_comp.y"
                  {
		      (yyval.i) = (yyvsp[0].i);
		  }
#line 5893 "lev_yacc.c"
    break;

  case 391:
#line 2521 "lev_comp.y"
                  {
			(yyval.i) = -1;
		  }
#line 5901 "lev_yacc.c"
    break;

  case 392:
#line 2525 "lev_comp.y"
                  {
			(yyval.i) = (yyvsp[0].i);
		  }
#line 5909 "lev_yacc.c"
    break;

  case 393:
#line 2531 "lev_comp.y"
                  {
		      (yyval.i) = (yyvsp[0].i);
		  }
#line 5917 "lev_yacc.c"
    break;

  case 394:
#line 2537 "lev_comp.y"
                  {
			add_opvars(splev, "M", VA_PASS1(-1));
			(yyval.i) = -1;
		  }
#line 5926 "lev_yacc.c"
    break;

  case 395:
#line 2542 "lev_comp.y"
                  {
			(yyval.i) = (yyvsp[0].i);
		  }
#line 5934 "lev_yacc.c"
    break;

  case 402:
#line 2559 "lev_comp.y"
                  {
			(yyval.i) = - MAX_REGISTERS - 1;
		  }
#line 5942 "lev_yacc.c"
    break;

  case 405:
#line 2567 "lev_comp.y"
                  {
			(yyval.i) = - MAX_REGISTERS - 1;
		  }
#line 5950 "lev_yacc.c"
    break;

  case 408:
#line 2577 "lev_comp.y"
                  {
			if ( (yyvsp[-1].i) >= 3 )
				lc_error("Register Index overflow!");
			else
				(yyval.i) = - (yyvsp[-1].i) - 1;
		  }
#line 5961 "lev_yacc.c"
    break;

  case 409:
#line 2586 "lev_comp.y"
                  {
		      add_opvars(splev, "s", VA_PASS1((yyvsp[0].map)));
		      Free((yyvsp[0].map));
		  }
#line 5970 "lev_yacc.c"
    break;

  case 410:
#line 2591 "lev_comp.y"
                  {
		      check_vardef_type(vardefs, (yyvsp[0].map), SPOVAR_STRING);
		      vardef_used(vardefs, (yyvsp[0].map));
		      add_opvars(splev, "v", VA_PASS1((yyvsp[0].map)));
		      Free((yyvsp[0].map));
		  }
#line 5981 "lev_yacc.c"
    break;

  case 411:
#line 2598 "lev_comp.y"
                  {
		      check_vardef_type(vardefs, (yyvsp[-3].map),
                                        SPOVAR_STRING | SPOVAR_ARRAY);
		      vardef_used(vardefs, (yyvsp[-3].map));
		      add_opvars(splev, "v", VA_PASS1((yyvsp[-3].map)));
		      Free((yyvsp[-3].map));
		  }
#line 5993 "lev_yacc.c"
    break;

  case 412:
#line 2609 "lev_comp.y"
                  {
		      /* nothing */
		  }
#line 6001 "lev_yacc.c"
    break;

  case 413:
#line 2615 "lev_comp.y"
                  {
		      add_opvars(splev, "c", VA_PASS1((yyvsp[0].i)));
		  }
#line 6009 "lev_yacc.c"
    break;

  case 414:
#line 2619 "lev_comp.y"
                  {
		      add_opvars(splev, "o", VA_PASS1(SPO_SEL_RNDCOORD));
		  }
#line 6017 "lev_yacc.c"
    break;

  case 415:
#line 2623 "lev_comp.y"
                  {
		      check_vardef_type(vardefs, (yyvsp[0].map), SPOVAR_COORD);
		      vardef_used(vardefs, (yyvsp[0].map));
		      add_opvars(splev, "v", VA_PASS1((yyvsp[0].map)));
		      Free((yyvsp[0].map));
		  }
#line 6028 "lev_yacc.c"
    break;

  case 416:
#line 2630 "lev_comp.y"
                  {
		      check_vardef_type(vardefs, (yyvsp[-3].map),
                                        SPOVAR_COORD | SPOVAR_ARRAY);
		      vardef_used(vardefs, (yyvsp[-3].map));
		      add_opvars(splev, "v", VA_PASS1((yyvsp[-3].map)));
		      Free((yyvsp[-3].map));
		  }
#line 6040 "lev_yacc.c"
    break;

  case 417:
#line 2640 "lev_comp.y"
                  {
		      if ((yyvsp[-3].i) < 0 || (yyvsp[-1].i) < 0 || (yyvsp[-3].i) >= COLNO || (yyvsp[-1].i) >= ROWNO)
                          lc_error("Coordinates (%li,%li) out of map range!",
                                   (yyvsp[-3].i), (yyvsp[-1].i));
		      (yyval.i) = SP_COORD_PACK((yyvsp[-3].i), (yyvsp[-1].i));
		  }
#line 6051 "lev_yacc.c"
    break;

  case 418:
#line 2647 "lev_comp.y"
                  {
		      (yyval.i) = SP_COORD_PACK_RANDOM(0);
		  }
#line 6059 "lev_yacc.c"
    break;

  case 419:
#line 2651 "lev_comp.y"
                  {
		      (yyval.i) = SP_COORD_PACK_RANDOM((yyvsp[-1].i));
		  }
#line 6067 "lev_yacc.c"
    break;

  case 420:
#line 2657 "lev_comp.y"
                  {
		      (yyval.i) = (yyvsp[0].i);
		  }
#line 6075 "lev_yacc.c"
    break;

  case 421:
#line 2661 "lev_comp.y"
                  {
		      if (((yyvsp[-2].i) & (yyvsp[0].i)))
			  lc_warning("Humidity flag used twice.");
		      (yyval.i) = ((yyvsp[-2].i) | (yyvsp[0].i));
		  }
#line 6085 "lev_yacc.c"
    break;

  case 422:
#line 2669 "lev_comp.y"
                  {
		      /* nothing */
		  }
#line 6093 "lev_yacc.c"
    break;

  case 423:
#line 2673 "lev_comp.y"
                  {
		      check_vardef_type(vardefs, (yyvsp[0].map), SPOVAR_REGION);
		      vardef_used(vardefs, (yyvsp[0].map));
		      add_opvars(splev, "v", VA_PASS1((yyvsp[0].map)));
		      Free((yyvsp[0].map));
		  }
#line 6104 "lev_yacc.c"
    break;

  case 424:
#line 2680 "lev_comp.y"
                  {
		      check_vardef_type(vardefs, (yyvsp[-3].map),
                                        SPOVAR_REGION | SPOVAR_ARRAY);
		      vardef_used(vardefs, (yyvsp[-3].map));
		      add_opvars(splev, "v", VA_PASS1((yyvsp[-3].map)));
		      Free((yyvsp[-3].map));
		  }
#line 6116 "lev_yacc.c"
    break;

  case 425:
#line 2690 "lev_comp.y"
                  {
		      long r = SP_REGION_PACK((yyvsp[-7].i), (yyvsp[-5].i), (yyvsp[-3].i), (yyvsp[-1].i));

		      if ((yyvsp[-7].i) > (yyvsp[-3].i) || (yyvsp[-5].i) > (yyvsp[-1].i))
			  lc_error("Region start > end: (%ld,%ld,%ld,%ld)!",
                                   (yyvsp[-7].i), (yyvsp[-5].i), (yyvsp[-3].i), (yyvsp[-1].i));

		      add_opvars(splev, "r", VA_PASS1(r));
		      (yyval.i) = r;
		  }
#line 6131 "lev_yacc.c"
    break;

  case 426:
#line 2703 "lev_comp.y"
                  {
		      add_opvars(splev, "m", VA_PASS1((yyvsp[0].i)));
		  }
#line 6139 "lev_yacc.c"
    break;

  case 427:
#line 2707 "lev_comp.y"
                  {
		      check_vardef_type(vardefs, (yyvsp[0].map), SPOVAR_MAPCHAR);
		      vardef_used(vardefs, (yyvsp[0].map));
		      add_opvars(splev, "v", VA_PASS1((yyvsp[0].map)));
		      Free((yyvsp[0].map));
		  }
#line 6150 "lev_yacc.c"
    break;

  case 428:
#line 2714 "lev_comp.y"
                  {
		      check_vardef_type(vardefs, (yyvsp[-3].map),
                                        SPOVAR_MAPCHAR | SPOVAR_ARRAY);
		      vardef_used(vardefs, (yyvsp[-3].map));
		      add_opvars(splev, "v", VA_PASS1((yyvsp[-3].map)));
		      Free((yyvsp[-3].map));
		  }
#line 6162 "lev_yacc.c"
    break;

  case 429:
#line 2724 "lev_comp.y"
                  {
		      if (what_map_char((char) (yyvsp[0].i)) != INVALID_TYPE)
			  (yyval.i) = SP_MAPCHAR_PACK(what_map_char((char) (yyvsp[0].i)), -2);
		      else {
			  lc_error("Unknown map char type '%c'!", (yyvsp[0].i));
			  (yyval.i) = SP_MAPCHAR_PACK(STONE, -2);
		      }
		  }
#line 6175 "lev_yacc.c"
    break;

  case 430:
#line 2733 "lev_comp.y"
                  {
		      if (what_map_char((char) (yyvsp[-3].i)) != INVALID_TYPE)
			  (yyval.i) = SP_MAPCHAR_PACK(what_map_char((char) (yyvsp[-3].i)), (yyvsp[-1].i));
		      else {
			  lc_error("Unknown map char type '%c'!", (yyvsp[-3].i));
			  (yyval.i) = SP_MAPCHAR_PACK(STONE, (yyvsp[-1].i));
		      }
		  }
#line 6188 "lev_yacc.c"
    break;

  case 431:
#line 2744 "lev_comp.y"
                  {
		      add_opvars(splev, "M", VA_PASS1((yyvsp[0].i)));
		  }
#line 6196 "lev_yacc.c"
    break;

  case 432:
#line 2748 "lev_comp.y"
                  {
		      check_vardef_type(vardefs, (yyvsp[0].map), SPOVAR_MONST);
		      vardef_used(vardefs, (yyvsp[0].map));
		      add_opvars(splev, "v", VA_PASS1((yyvsp[0].map)));
		      Free((yyvsp[0].map));
		  }
#line 6207 "lev_yacc.c"
    break;

  case 433:
#line 2755 "lev_comp.y"
                  {
		      check_vardef_type(vardefs, (yyvsp[-3].map),
                                        SPOVAR_MONST | SPOVAR_ARRAY);
		      vardef_used(vardefs, (yyvsp[-3].map));
		      add_opvars(splev, "v", VA_PASS1((yyvsp[-3].map)));
		      Free((yyvsp[-3].map));
		  }
#line 6219 "lev_yacc.c"
    break;

  case 434:
#line 2765 "lev_comp.y"
                  {
                      long m = get_monster_id((yyvsp[0].map), (char)0);
                      if (m == ERR) {
                          lc_error("Unknown monster \"%s\"!", (yyvsp[0].map));
                          (yyval.i) = -1;
                      } else
                          (yyval.i) = SP_MONST_PACK(m,
                                         def_monsyms[(int) mons[m].mlet].sym);
                      Free((yyvsp[0].map));
                  }
#line 6234 "lev_yacc.c"
    break;

  case 435:
#line 2776 "lev_comp.y"
                  {
                        if (check_monster_char((char) (yyvsp[0].i)))
                            (yyval.i) = SP_MONST_PACK(-1, (yyvsp[0].i));
                        else {
                            lc_error("Unknown monster class '%c'!", (yyvsp[0].i));
                            (yyval.i) = -1;
                        }
                  }
#line 6247 "lev_yacc.c"
    break;

  case 436:
#line 2785 "lev_comp.y"
                  {
                      long m = get_monster_id((yyvsp[-1].map), (char) (yyvsp[-3].i));
                      if (m == ERR) {
                          lc_error("Unknown monster ('%c', \"%s\")!", (yyvsp[-3].i), (yyvsp[-1].map));
                          (yyval.i) = -1;
                      } else
                          (yyval.i) = SP_MONST_PACK(m, (yyvsp[-3].i));
                      Free((yyvsp[-1].map));
                  }
#line 6261 "lev_yacc.c"
    break;

  case 437:
#line 2795 "lev_comp.y"
                  {
                      (yyval.i) = -1;
                  }
#line 6269 "lev_yacc.c"
    break;

  case 438:
#line 2801 "lev_comp.y"
                  {
		      add_opvars(splev, "O", VA_PASS1((yyvsp[0].i)));
		  }
#line 6277 "lev_yacc.c"
    break;

  case 439:
#line 2805 "lev_comp.y"
                  {
		      check_vardef_type(vardefs, (yyvsp[0].map), SPOVAR_OBJ);
		      vardef_used(vardefs, (yyvsp[0].map));
		      add_opvars(splev, "v", VA_PASS1((yyvsp[0].map)));
		      Free((yyvsp[0].map));
		  }
#line 6288 "lev_yacc.c"
    break;

  case 440:
#line 2812 "lev_comp.y"
                  {
		      check_vardef_type(vardefs, (yyvsp[-3].map),
                                        SPOVAR_OBJ | SPOVAR_ARRAY);
		      vardef_used(vardefs, (yyvsp[-3].map));
		      add_opvars(splev, "v", VA_PASS1((yyvsp[-3].map)));
		      Free((yyvsp[-3].map));
		  }
#line 6300 "lev_yacc.c"
    break;

  case 441:
#line 2822 "lev_comp.y"
                  {
		      long m = get_object_id((yyvsp[0].map), (char)0);
		      if (m == ERR) {
			  lc_error("Unknown object \"%s\"!", (yyvsp[0].map));
			  (yyval.i) = -1;
		      } else
                          /* obj class != 0 to force generation of a specific item */
                          (yyval.i) = SP_OBJ_PACK(m, 1);
                      Free((yyvsp[0].map));
		  }
#line 6315 "lev_yacc.c"
    break;

  case 442:
#line 2833 "lev_comp.y"
                  {
			if (check_object_char((char) (yyvsp[0].i)))
			    (yyval.i) = SP_OBJ_PACK(-1, (yyvsp[0].i));
			else {
			    lc_error("Unknown object class '%c'!", (yyvsp[0].i));
			    (yyval.i) = -1;
			}
		  }
#line 6328 "lev_yacc.c"
    break;

  case 443:
#line 2842 "lev_comp.y"
                  {
		      long m = get_object_id((yyvsp[-1].map), (char) (yyvsp[-3].i));
		      if (m == ERR) {
			  lc_error("Unknown object ('%c', \"%s\")!", (yyvsp[-3].i), (yyvsp[-1].map));
			  (yyval.i) = -1;
		      } else
			  (yyval.i) = SP_OBJ_PACK(m, (yyvsp[-3].i));
                      Free((yyvsp[-1].map));
		  }
#line 6342 "lev_yacc.c"
    break;

  case 444:
#line 2852 "lev_comp.y"
                  {
		      (yyval.i) = OBJECT_SPECIAL_CREATE_TYPE_RANDOM;
		  }
#line 6350 "lev_yacc.c"
    break;

  case 445:
#line 2856 "lev_comp.y"
                  {
  			  (yyval.i) = (yyvsp[0].i);
		  }
#line 6358 "lev_yacc.c"
    break;

  case 446:
#line 2862 "lev_comp.y"
                                                { }
#line 6364 "lev_yacc.c"
    break;

  case 447:
#line 2864 "lev_comp.y"
                  {
		      add_opvars(splev, "o", VA_PASS1(SPO_MATH_ADD));
		  }
#line 6372 "lev_yacc.c"
    break;

  case 448:
#line 2870 "lev_comp.y"
                  {
		      add_opvars(splev, "i", VA_PASS1((yyvsp[0].i)));
		  }
#line 6380 "lev_yacc.c"
    break;

  case 449:
#line 2874 "lev_comp.y"
                  {
		      is_inconstant_number = 1;
		  }
#line 6388 "lev_yacc.c"
    break;

  case 450:
#line 2878 "lev_comp.y"
                  {
		      add_opvars(splev, "i", VA_PASS1((yyvsp[-1].i)));
		  }
#line 6396 "lev_yacc.c"
    break;

  case 451:
#line 2882 "lev_comp.y"
                  {
		      check_vardef_type(vardefs, (yyvsp[0].map), SPOVAR_INT);
		      vardef_used(vardefs, (yyvsp[0].map));
		      add_opvars(splev, "v", VA_PASS1((yyvsp[0].map)));
		      Free((yyvsp[0].map));
		      is_inconstant_number = 1;
		  }
#line 6408 "lev_yacc.c"
    break;

  case 452:
#line 2890 "lev_comp.y"
                  {
		      check_vardef_type(vardefs, (yyvsp[-3].map),
					SPOVAR_INT | SPOVAR_ARRAY);
		      vardef_used(vardefs, (yyvsp[-3].map));
		      add_opvars(splev, "v", VA_PASS1((yyvsp[-3].map)));
		      Free((yyvsp[-3].map));
		      is_inconstant_number = 1;
		  }
#line 6421 "lev_yacc.c"
    break;

  case 453:
#line 2899 "lev_comp.y"
                  {
		      add_opvars(splev, "o", VA_PASS1(SPO_MATH_ADD));
		  }
#line 6429 "lev_yacc.c"
    break;

  case 454:
#line 2903 "lev_comp.y"
                  {
		      add_opvars(splev, "o", VA_PASS1(SPO_MATH_SUB));
		  }
#line 6437 "lev_yacc.c"
    break;

  case 455:
#line 2907 "lev_comp.y"
                  {
		      add_opvars(splev, "o", VA_PASS1(SPO_MATH_MUL));
		  }
#line 6445 "lev_yacc.c"
    break;

  case 456:
#line 2911 "lev_comp.y"
                  {
		      add_opvars(splev, "o", VA_PASS1(SPO_MATH_DIV));
		  }
#line 6453 "lev_yacc.c"
    break;

  case 457:
#line 2915 "lev_comp.y"
                  {
		      add_opvars(splev, "o", VA_PASS1(SPO_MATH_MOD));
		  }
#line 6461 "lev_yacc.c"
    break;

  case 458:
#line 2918 "lev_comp.y"
                                                    { }
#line 6467 "lev_yacc.c"
    break;

  case 459:
#line 2922 "lev_comp.y"
                  {
		      if (!strcmp("int", (yyvsp[0].map)) || !strcmp("integer", (yyvsp[0].map))) {
			  (yyval.i) = (int)'i';
		      } else
			  lc_error("Unknown function parameter type '%s'", (yyvsp[0].map));
		  }
#line 6478 "lev_yacc.c"
    break;

  case 460:
#line 2929 "lev_comp.y"
                  {
		      if (!strcmp("str", (yyvsp[0].map)) || !strcmp("string", (yyvsp[0].map))) {
			  (yyval.i) = (int)'s';
		      } else
			  lc_error("Unknown function parameter type '%s'", (yyvsp[0].map));
		  }
#line 6489 "lev_yacc.c"
    break;

  case 461:
#line 2938 "lev_comp.y"
                  {
		      struct lc_funcdefs_parm *tmp = New(struct lc_funcdefs_parm);

		      if (!curr_function) {
			  lc_error("Function parameters outside function definition.");
		      } else if (!tmp) {
			  lc_error("Could not alloc function params.");
		      } else {
			  long vt = SPOVAR_NULL;

			  tmp->name = strdup((yyvsp[-2].map));
			  tmp->parmtype = (char) (yyvsp[0].i);
			  tmp->next = curr_function->params;
			  curr_function->params = tmp;
			  curr_function->n_params++;
			  switch (tmp->parmtype) {
			  case 'i':
                              vt = SPOVAR_INT;
                              break;
			  case 's':
                              vt = SPOVAR_STRING;
                              break;
			  default:
                              lc_error("Unknown func param conversion.");
                              break;
			  }
			  vardefs = add_vardef_type( vardefs, (yyvsp[-2].map), vt);
		      }
		      Free((yyvsp[-2].map));
		  }
#line 6524 "lev_yacc.c"
    break;

  case 466:
#line 2979 "lev_comp.y"
                          {
			      (yyval.i) = (int)'i';
			  }
#line 6532 "lev_yacc.c"
    break;

  case 467:
#line 2983 "lev_comp.y"
                          {
			      (yyval.i) = (int)'s';
			  }
#line 6540 "lev_yacc.c"
    break;

  case 468:
#line 2990 "lev_comp.y"
                          {
			      char tmpbuf[2];
			      tmpbuf[0] = (char) (yyvsp[0].i);
			      tmpbuf[1] = '\0';
			      (yyval.map) = strdup(tmpbuf);
			  }
#line 6551 "lev_yacc.c"
    break;

  case 469:
#line 2997 "lev_comp.y"
                          {
			      long len = strlen( (yyvsp[-2].map) );
			      char *tmp = (char *) alloc(len + 2);
			      sprintf(tmp, "%c%s", (char) (yyvsp[0].i), (yyvsp[-2].map) );
			      Free( (yyvsp[-2].map) );
			      (yyval.map) = tmp;
			  }
#line 6563 "lev_yacc.c"
    break;

  case 470:
#line 3007 "lev_comp.y"
                          {
			      (yyval.map) = strdup("");
			  }
#line 6571 "lev_yacc.c"
    break;

  case 471:
#line 3011 "lev_comp.y"
                          {
			      char *tmp = strdup( (yyvsp[0].map) );
			      Free( (yyvsp[0].map) );
			      (yyval.map) = tmp;
			  }
#line 6581 "lev_yacc.c"
    break;

  case 472:
#line 3019 "lev_comp.y"
                  {
		      add_opvars(splev, "o", VA_PASS1(SPO_SEL_POINT));
		  }
#line 6589 "lev_yacc.c"
    break;

  case 473:
#line 3023 "lev_comp.y"
                  {
		      add_opvars(splev, "o", VA_PASS1(SPO_SEL_RECT));
		  }
#line 6597 "lev_yacc.c"
    break;

  case 474:
#line 3027 "lev_comp.y"
                  {
		      add_opvars(splev, "o", VA_PASS1(SPO_SEL_FILLRECT));
		  }
#line 6605 "lev_yacc.c"
    break;

  case 475:
#line 3031 "lev_comp.y"
                  {
		      add_opvars(splev, "o", VA_PASS1(SPO_SEL_LINE));
		  }
#line 6613 "lev_yacc.c"
    break;

  case 476:
#line 3035 "lev_comp.y"
                  {
		      /* randline (x1,y1),(x2,y2), roughness */
		      add_opvars(splev, "o", VA_PASS1(SPO_SEL_RNDLINE));
		  }
#line 6622 "lev_yacc.c"
    break;

  case 477:
#line 3040 "lev_comp.y"
                  {
		      add_opvars(splev, "io", VA_PASS2(W_ANY, SPO_SEL_GROW));
		  }
#line 6630 "lev_yacc.c"
    break;

  case 478:
#line 3044 "lev_comp.y"
                  {
		      add_opvars(splev, "io", VA_PASS2((yyvsp[-3].i), SPO_SEL_GROW));
		  }
#line 6638 "lev_yacc.c"
    break;

  case 479:
#line 3048 "lev_comp.y"
                  {
		      add_opvars(splev, "iio",
			     VA_PASS3((yyvsp[-3].i), SPOFILTER_PERCENT, SPO_SEL_FILTER));
		  }
#line 6647 "lev_yacc.c"
    break;

  case 480:
#line 3053 "lev_comp.y"
                  {
		      add_opvars(splev, "io",
			       VA_PASS2(SPOFILTER_SELECTION, SPO_SEL_FILTER));
		  }
#line 6656 "lev_yacc.c"
    break;

  case 481:
#line 3058 "lev_comp.y"
                  {
		      add_opvars(splev, "io",
				 VA_PASS2(SPOFILTER_MAPCHAR, SPO_SEL_FILTER));
		  }
#line 6665 "lev_yacc.c"
    break;

  case 482:
#line 3063 "lev_comp.y"
                  {
		      add_opvars(splev, "o", VA_PASS1(SPO_SEL_FLOOD));
		  }
#line 6673 "lev_yacc.c"
    break;

  case 483:
#line 3067 "lev_comp.y"
                  {
		      add_opvars(splev, "oio",
				 VA_PASS3(SPO_COPY, 1, SPO_SEL_ELLIPSE));
		  }
#line 6682 "lev_yacc.c"
    break;

  case 484:
#line 3072 "lev_comp.y"
                  {
		      add_opvars(splev, "oio",
				 VA_PASS3(SPO_COPY, (yyvsp[-1].i), SPO_SEL_ELLIPSE));
		  }
#line 6691 "lev_yacc.c"
    break;

  case 485:
#line 3077 "lev_comp.y"
                  {
		      add_opvars(splev, "io", VA_PASS2(1, SPO_SEL_ELLIPSE));
		  }
#line 6699 "lev_yacc.c"
    break;

  case 486:
#line 3081 "lev_comp.y"
                  {
		      add_opvars(splev, "io", VA_PASS2((yyvsp[-1].i), SPO_SEL_ELLIPSE));
		  }
#line 6707 "lev_yacc.c"
    break;

  case 487:
#line 3085 "lev_comp.y"
                  {
		      add_opvars(splev, "iio",
				 VA_PASS3((yyvsp[-5].i), (yyvsp[-11].i), SPO_SEL_GRADIENT));
		  }
#line 6716 "lev_yacc.c"
    break;

  case 488:
#line 3090 "lev_comp.y"
                  {
		      add_opvars(splev, "o", VA_PASS1(SPO_SEL_COMPLEMENT));
		  }
#line 6724 "lev_yacc.c"
    break;

  case 489:
#line 3094 "lev_comp.y"
                  {
		      check_vardef_type(vardefs, (yyvsp[0].map), SPOVAR_SEL);
		      vardef_used(vardefs, (yyvsp[0].map));
		      add_opvars(splev, "v", VA_PASS1((yyvsp[0].map)));
		      Free((yyvsp[0].map));
		  }
#line 6735 "lev_yacc.c"
    break;

  case 490:
#line 3101 "lev_comp.y"
                  {
		      /* nothing */
		  }
#line 6743 "lev_yacc.c"
    break;

  case 491:
#line 3107 "lev_comp.y"
                  {
		      /* nothing */
		  }
#line 6751 "lev_yacc.c"
    break;

  case 492:
#line 3111 "lev_comp.y"
                  {
		      add_opvars(splev, "o", VA_PASS1(SPO_SEL_ADD));
		  }
#line 6759 "lev_yacc.c"
    break;

  case 493:
#line 3117 "lev_comp.y"
                  {
		      add_opvars(splev, "iio",
				 VA_PASS3((yyvsp[0].dice).num, (yyvsp[0].dice).die, SPO_DICE));
		  }
#line 6768 "lev_yacc.c"
    break;

  case 499:
#line 3133 "lev_comp.y"
                  {
		      add_opvars(splev, "i", VA_PASS1((yyvsp[0].i)));
		  }
#line 6776 "lev_yacc.c"
    break;

  case 500:
#line 3137 "lev_comp.y"
                  {
		      add_opvars(splev, "i", VA_PASS1((yyvsp[0].i)));
		  }
#line 6784 "lev_yacc.c"
    break;

  case 501:
#line 3141 "lev_comp.y"
                  {
		      add_opvars(splev, "i", VA_PASS1((yyvsp[0].i)));
		  }
#line 6792 "lev_yacc.c"
    break;

  case 502:
#line 3145 "lev_comp.y"
                  {
		      /* nothing */
		  }
#line 6800 "lev_yacc.c"
    break;

  case 511:
#line 3167 "lev_comp.y"
                  {
			(yyval.lregn) = (yyvsp[0].lregn);
		  }
#line 6808 "lev_yacc.c"
    break;

  case 512:
#line 3171 "lev_comp.y"
                  {
			if ((yyvsp[-7].i) <= 0 || (yyvsp[-7].i) >= COLNO)
			    lc_error(
                          "Region (%ld,%ld,%ld,%ld) out of level range (x1)!",
                                     (yyvsp[-7].i), (yyvsp[-5].i), (yyvsp[-3].i), (yyvsp[-1].i));
			else if ((yyvsp[-5].i) < 0 || (yyvsp[-5].i) >= ROWNO)
			    lc_error(
                          "Region (%ld,%ld,%ld,%ld) out of level range (y1)!",
                                     (yyvsp[-7].i), (yyvsp[-5].i), (yyvsp[-3].i), (yyvsp[-1].i));
			else if ((yyvsp[-3].i) <= 0 || (yyvsp[-3].i) >= COLNO)
			    lc_error(
                          "Region (%ld,%ld,%ld,%ld) out of level range (x2)!",
                                     (yyvsp[-7].i), (yyvsp[-5].i), (yyvsp[-3].i), (yyvsp[-1].i));
			else if ((yyvsp[-1].i) < 0 || (yyvsp[-1].i) >= ROWNO)
			    lc_error(
                          "Region (%ld,%ld,%ld,%ld) out of level range (y2)!",
                                     (yyvsp[-7].i), (yyvsp[-5].i), (yyvsp[-3].i), (yyvsp[-1].i));
			(yyval.lregn).x1 = (yyvsp[-7].i);
			(yyval.lregn).y1 = (yyvsp[-5].i);
			(yyval.lregn).x2 = (yyvsp[-3].i);
			(yyval.lregn).y2 = (yyvsp[-1].i);
			(yyval.lregn).area = 1;
		  }
#line 6836 "lev_yacc.c"
    break;

  case 513:
#line 3197 "lev_comp.y"
                  {
/* This series of if statements is a hack for MSC 5.1.  It seems that its
   tiny little brain cannot compile if these are all one big if statement. */
			if ((yyvsp[-7].i) < 0 || (yyvsp[-7].i) > (int) max_x_map)
			    lc_error(
                            "Region (%ld,%ld,%ld,%ld) out of map range (x1)!",
                                     (yyvsp[-7].i), (yyvsp[-5].i), (yyvsp[-3].i), (yyvsp[-1].i));
			else if ((yyvsp[-5].i) < 0 || (yyvsp[-5].i) > (int) max_y_map)
			    lc_error(
                            "Region (%ld,%ld,%ld,%ld) out of map range (y1)!",
                                     (yyvsp[-7].i), (yyvsp[-5].i), (yyvsp[-3].i), (yyvsp[-1].i));
			else if ((yyvsp[-3].i) < 0 || (yyvsp[-3].i) > (int) max_x_map)
			    lc_error(
                            "Region (%ld,%ld,%ld,%ld) out of map range (x2)!",
                                     (yyvsp[-7].i), (yyvsp[-5].i), (yyvsp[-3].i), (yyvsp[-1].i));
			else if ((yyvsp[-1].i) < 0 || (yyvsp[-1].i) > (int) max_y_map)
			    lc_error(
                            "Region (%ld,%ld,%ld,%ld) out of map range (y2)!",
                                     (yyvsp[-7].i), (yyvsp[-5].i), (yyvsp[-3].i), (yyvsp[-1].i));
			(yyval.lregn).area = 0;
			(yyval.lregn).x1 = (yyvsp[-7].i);
			(yyval.lregn).y1 = (yyvsp[-5].i);
			(yyval.lregn).x2 = (yyvsp[-3].i);
			(yyval.lregn).y2 = (yyvsp[-1].i);
		  }
#line 6866 "lev_yacc.c"
    break;


#line 6870 "lev_yacc.c"

      default: break;
    }
  /* User semantic actions sometimes alter yychar, and that requires
     that yytoken be updated with the new translation.  We take the
     approach of translating immediately before every use of yytoken.
     One alternative is translating here after every semantic action,
     but that translation would be missed if the semantic action invokes
     YYABORT, YYACCEPT, or YYERROR immediately after altering yychar or
     if it invokes YYBACKUP.  In the case of YYABORT or YYACCEPT, an
     incorrect destructor might then be invoked immediately.  In the
     case of YYERROR or YYBACKUP, subsequent parser actions might lead
     to an incorrect destructor call or verbose syntax error message
     before the lookahead is translated.  */
  YY_SYMBOL_PRINT ("-> $$ =", yyr1[yyn], &yyval, &yyloc);

  YYPOPSTACK (yylen);
  yylen = 0;
  YY_STACK_PRINT (yyss, yyssp);

  *++yyvsp = yyval;

  /* Now 'shift' the result of the reduction.  Determine what state
     that goes to, based on the state we popped back to and the rule
     number reduced by.  */
  {
    const int yylhs = yyr1[yyn] - YYNTOKENS;
    const int yyi = yypgoto[yylhs] + *yyssp;
    yystate = (0 <= yyi && yyi <= YYLAST && yycheck[yyi] == *yyssp
               ? yytable[yyi]
               : yydefgoto[yylhs]);
  }

  goto yynewstate;


/*--------------------------------------.
| yyerrlab -- here on detecting error.  |
`--------------------------------------*/
yyerrlab:
  /* Make sure we have latest lookahead translation.  See comments at
     user semantic actions for why this is necessary.  */
  yytoken = yychar == YYEMPTY ? YYEMPTY : YYTRANSLATE (yychar);

  /* If not already recovering from an error, report this error.  */
  if (!yyerrstatus)
    {
      ++yynerrs;
#if ! YYERROR_VERBOSE
      yyerror (YY_("syntax error"));
#else
# define YYSYNTAX_ERROR yysyntax_error (&yymsg_alloc, &yymsg, \
                                        yyssp, yytoken)
      {
        char const *yymsgp = YY_("syntax error");
        int yysyntax_error_status;
        yysyntax_error_status = YYSYNTAX_ERROR;
        if (yysyntax_error_status == 0)
          yymsgp = yymsg;
        else if (yysyntax_error_status == 1)
          {
            if (yymsg != yymsgbuf)
              YYSTACK_FREE (yymsg);
            yymsg = YY_CAST (char *, YYSTACK_ALLOC (YY_CAST (YYSIZE_T, yymsg_alloc)));
            if (!yymsg)
              {
                yymsg = yymsgbuf;
                yymsg_alloc = sizeof yymsgbuf;
                yysyntax_error_status = 2;
              }
            else
              {
                yysyntax_error_status = YYSYNTAX_ERROR;
                yymsgp = yymsg;
              }
          }
        yyerror (yymsgp);
        if (yysyntax_error_status == 2)
          goto yyexhaustedlab;
      }
# undef YYSYNTAX_ERROR
#endif
    }



  if (yyerrstatus == 3)
    {
      /* If just tried and failed to reuse lookahead token after an
         error, discard it.  */

      if (yychar <= YYEOF)
        {
          /* Return failure if at end of input.  */
          if (yychar == YYEOF)
            YYABORT;
        }
      else
        {
          yydestruct ("Error: discarding",
                      yytoken, &yylval);
          yychar = YYEMPTY;
        }
    }

  /* Else will try to reuse lookahead token after shifting the error
     token.  */
  goto yyerrlab1;


/*---------------------------------------------------.
| yyerrorlab -- error raised explicitly by YYERROR.  |
`---------------------------------------------------*/
yyerrorlab:
  /* Pacify compilers when the user code never invokes YYERROR and the
     label yyerrorlab therefore never appears in user code.  */
  if (0)
    YYERROR;

  /* Do not reclaim the symbols of the rule whose action triggered
     this YYERROR.  */
  YYPOPSTACK (yylen);
  yylen = 0;
  YY_STACK_PRINT (yyss, yyssp);
  yystate = *yyssp;
  goto yyerrlab1;


/*-------------------------------------------------------------.
| yyerrlab1 -- common code for both syntax error and YYERROR.  |
`-------------------------------------------------------------*/
yyerrlab1:
  yyerrstatus = 3;      /* Each real token shifted decrements this.  */

  for (;;)
    {
      yyn = yypact[yystate];
      if (!yypact_value_is_default (yyn))
        {
          yyn += YYTERROR;
          if (0 <= yyn && yyn <= YYLAST && yycheck[yyn] == YYTERROR)
            {
              yyn = yytable[yyn];
              if (0 < yyn)
                break;
            }
        }

      /* Pop the current state because it cannot handle the error token.  */
      if (yyssp == yyss)
        YYABORT;


      yydestruct ("Error: popping",
                  yystos[yystate], yyvsp);
      YYPOPSTACK (1);
      yystate = *yyssp;
      YY_STACK_PRINT (yyss, yyssp);
    }

  YY_IGNORE_MAYBE_UNINITIALIZED_BEGIN
  *++yyvsp = yylval;
  YY_IGNORE_MAYBE_UNINITIALIZED_END


  /* Shift the error token.  */
  YY_SYMBOL_PRINT ("Shifting", yystos[yyn], yyvsp, yylsp);

  yystate = yyn;
  goto yynewstate;


/*-------------------------------------.
| yyacceptlab -- YYACCEPT comes here.  |
`-------------------------------------*/
yyacceptlab:
  yyresult = 0;
  goto yyreturn;


/*-----------------------------------.
| yyabortlab -- YYABORT comes here.  |
`-----------------------------------*/
yyabortlab:
  yyresult = 1;
  goto yyreturn;


#if !defined yyoverflow || YYERROR_VERBOSE
/*-------------------------------------------------.
| yyexhaustedlab -- memory exhaustion comes here.  |
`-------------------------------------------------*/
yyexhaustedlab:
  yyerror (YY_("memory exhausted"));
  yyresult = 2;
  /* Fall through.  */
#endif


/*-----------------------------------------------------.
| yyreturn -- parsing is finished, return the result.  |
`-----------------------------------------------------*/
yyreturn:
  if (yychar != YYEMPTY)
    {
      /* Make sure we have latest lookahead translation.  See comments at
         user semantic actions for why this is necessary.  */
      yytoken = YYTRANSLATE (yychar);
      yydestruct ("Cleanup: discarding lookahead",
                  yytoken, &yylval);
    }
  /* Do not reclaim the symbols of the rule whose action triggered
     this YYABORT or YYACCEPT.  */
  YYPOPSTACK (yylen);
  YY_STACK_PRINT (yyss, yyssp);
  while (yyssp != yyss)
    {
      yydestruct ("Cleanup: popping",
                  yystos[+*yyssp], yyvsp);
      YYPOPSTACK (1);
    }
#ifndef yyoverflow
  if (yyss != yyssa)
    YYSTACK_FREE (yyss);
#endif
#if YYERROR_VERBOSE
  if (yymsg != yymsgbuf)
    YYSTACK_FREE (yymsg);
#endif
  return yyresult;
}
#line 3225 "lev_comp.y"


/*lev_comp.y*/
