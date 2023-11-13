using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ConstantLibrary
{
    #region POPUP CODES
    public const int POP_FUEL = 0;
    public const int POP_GOLD10 = 1;
    public const int POP_GOLD10_X2 = 2;
    public const int POP_GOLD30 = 3;
    public const int POP_GOLD30_X2 = 4;
    public const int POP_POINTS = 5;
    public const int POP_POINTS_X2 = 6;
    public const int POP_OVERDRIVE = 7;
    public const int POP_REPAIR = 8;
    public const int POP_HEALED = 9;
    #endregion

    #region tileType Constants
    public const int T_GROUND = 0;
    public const int T_WALL = 1;
    public const int T_ORE_GOLD = 2;
    public const int T_ORE_FUEL = 3;
    public const int T_ORE_AMATHYST = 4;
    public const int T_GEODE = 5;
    public const int T_GEODE_ORE = 6;
    public const int T_ORE_DIAMOND = 7;
    public const int T_HOLE = 9;
    public const int T_BRIDGE = 10;
    public const int T_RAIL = 11;
    public const int T_MINECART = 12;
    public const int T_WATER = 13;
    public const int T_ICE = 14;
    public const int T_LAVA = 15;
    public const int T_VINE = 16;
    public const int T_VINE_GROWING = 17;
    public const int T_LAVA_FLOW = 18;
    public const int T_LAVA_FLOW_BRIDGE = 19;
    public const int T_EXPLOSIVE = 20;
    #endregion

    //Tile Groups with similar properties
    public static readonly int[] typeCheck_vines = { T_VINE};
    public static readonly int[] typeCheck_lavaFlow = { T_LAVA_FLOW, T_LAVA_FLOW_BRIDGE, T_LAVA };
    public static readonly int[] typeCheck_nonWalls = { T_GROUND, T_HOLE, T_BRIDGE, T_RAIL, T_MINECART, T_WATER, T_ICE, T_LAVA, T_LAVA_FLOW, T_LAVA_FLOW_BRIDGE, T_VINE, T_VINE_GROWING };    //All tiles that should NOT be shaded like walls
    public static readonly int[] typeCheck_railSlots = { T_GROUND, T_WALL };  //All tiles that can be replaced by rails
    public static readonly int[] typeCheck_drillPathImmune = { T_HOLE, T_BRIDGE, T_WATER, T_RAIL, T_LAVA, T_LAVA_FLOW, T_LAVA_FLOW_BRIDGE };  //All tiles should not be destroyed by the drill
    public static readonly int[] typeCheck_shadowCasters = { T_WALL, T_ORE_GOLD, T_ORE_FUEL, T_ORE_AMATHYST, T_GEODE, T_GEODE_ORE, T_ORE_DIAMOND, T_EXPLOSIVE };

    //Tiles that can be hit but should not shake when hit
    public static readonly int[] tileNoShakeList = { T_VINE };

    //Cardinal Directions
    public const int NORTH = 0;
    public const int EAST = 1;
    public const int SOUTH = 2;
    public const int WEST = 3;

    //Damage Sources
    public const int DMGSRC_PHYSICAL = 0;
    public const int DMGSRC_ICE = 1;
    public const int DMGSRC_FIRE = 2;
    public const int DMGSRC_HEAL = 4;
    public const int DMGSRC_FALL = 5;

    //Item types
    public const int I_EMPTY = -1;
    public const int I_LOCKED = -2;
    public const int I_FUEL = 0;
    public const int I_GOLD = 1;
    public const int I_DIAMOND = 2;
    public const int I_AMATHYST = 3;
    public const int I_ASSORTMENT = 4;

    //Boss Codes
    public const int BOSS_TEST = -1;
    public const int BOSS_SPIDER = 0;
    public const int BOSS_BEE = 1;
    public const int BOSS_WORM = 2;
    public const int BOSS_SKEL = 3;
    public const int BOSS_FLYTRAP = 4;

    //World Generation Types
    public const int WORLDTYPE_NORMAL = 0;
    public const int WORLDTYPE_BOSS = 1;

    //Gameplay Constants
    public const float frozenMeter_updateFrequency = 0.4f;
    public const float frozenMeter_statusDecayTime = 2f;
    public const float frozen_damageRate = 1f;
    public const float onFire_damageRate = 0.5f;
    public const float onFire_decayTime = 2f;
    public const float vineRegrowthTime = 1.5f;
    public const float iceAccelerationRate = 1f;
    public const float defaultReloadTime = 1.5f;
    public const float defaultDodgeCooldownTime = 5f;


    #region Biome Codes
    public const int BIO_DEFAULT = 0;
    public const int BIO_GREEN_TRANS = 1;
    public const int BIO_GREEN = 2;
    public const int BIO_ICE_TRANS = 3;
    public const int BIO_ICE = 4;
    public const int BIO_LAVA_TRANS = 5;
    public const int BIO_LAVA = 6;
    #endregion

    public const int DRILLPATH_TOP = 10;
    public const int DRILLPATH_BOT = 14;

}
