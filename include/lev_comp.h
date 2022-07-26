/* A Bison parser, made by GNU Bison 3.5.1.  */

/* Bison interface for Yacc-like parsers in C

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

/* Undocumented macros, especially those whose name start with YY_,
   are private implementation details.  Do not rely on them.  */

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

#line 598 "lev_comp.h"

};
typedef union YYSTYPE YYSTYPE;
# define YYSTYPE_IS_TRIVIAL 1
# define YYSTYPE_IS_DECLARED 1
#endif


extern YYSTYPE yylval;

int yyparse (void);

#endif /* !YY_YY_Y_TAB_H_INCLUDED  */
