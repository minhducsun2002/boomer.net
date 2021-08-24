using System;
using System.Collections.Generic;
using FgoExportedConstants;

namespace Pepper.Structures.External.FGO.Renderer
{
    public partial class InvocationRenderer
    {
        public static readonly Dictionary<FuncList.TYPE, string> FunctionNames = new()
        {
            {FuncList.TYPE.ABSORB_NPTURN, ""},
            {FuncList.TYPE.ADD_STATE, "Apply"},
            {FuncList.TYPE.ADD_STATE_SHORT, "Apply (short)"},
            {FuncList.TYPE.BREAK_GAUGE_DOWN, ""},
            {FuncList.TYPE.BREAK_GAUGE_UP, ""},
            {FuncList.TYPE.CALL_SERVANT, "Summon another servant"},
            {FuncList.TYPE.CARD_RESET, "Shuffle all cards"},
            {FuncList.TYPE.CHANGE_BG, ""},
            {FuncList.TYPE.CHANGE_BGM, ""},
            {FuncList.TYPE.CHANGE_SERVANT, ""},
            {FuncList.TYPE.CLASS_DROP_UP, "Increase drop up for class weak against this servant"},
            {FuncList.TYPE.DAMAGE, "Deal damage"},
            {FuncList.TYPE.DAMAGE_NP, "Deal NP damage"},
            {FuncList.TYPE.DAMAGE_NP_AND_CHECK_INDIVIDUALITY, ""},
            {FuncList.TYPE.DAMAGE_NP_COUNTER, ""},
            {FuncList.TYPE.DAMAGE_NP_HPRATIO_HIGH, ""},
            {FuncList.TYPE.DAMAGE_NP_HPRATIO_LOW, "Deal NP damage based on HP level"},
            {FuncList.TYPE.DAMAGE_NP_INDIVIDUAL, "Deal Special Attack NP damage"},
            {FuncList.TYPE.DAMAGE_NP_INDIVIDUAL_SUM, "Deal Special Attack NP damage (bonus per trait)"},
            {FuncList.TYPE.DAMAGE_NP_PIERCE, "Deal DEF-ignoring NP damage"},
            {FuncList.TYPE.DAMAGE_NP_RARE, "Deal Special Attack NP damage (bonus from rarity)"},
            {FuncList.TYPE.DAMAGE_NP_SAFE, ""},
            {FuncList.TYPE.DAMAGE_NP_STATE_INDIVIDUAL, ""},
            // deal Special Attack for those with a certain state
            {FuncList.TYPE.DAMAGE_NP_STATE_INDIVIDUAL_FIX, "Deal NP damage & Special Attack NP damage"},
            // deal a static value of damage?
            {FuncList.TYPE.DAMAGE_VALUE, "Deal damage"},
            {FuncList.TYPE.DAMAGE_VALUE_SAFE, "Deal damage without killing"},
            {FuncList.TYPE.DELAY_NPTURN, "Drain enemy charge"},
            {FuncList.TYPE.DISPLAY_BUFFSTRING, ""},
            {FuncList.TYPE.DROP_UP, ""},
            // https://apps.atlasacademy.io/db/#/NA/skill/990264
            {FuncList.TYPE.ENEMY_ENCOUNT_COPY_RATE_UP, "Increase rate of enemy copies' appearance"},
            // https://apps.atlasacademy.io/db/#/NA/skill/990317
            {FuncList.TYPE.ENEMY_ENCOUNT_RATE_UP, "Increase rate of enemies' appearance"},
            {FuncList.TYPE.ENEMY_PROB_DOWN, ""},
            {FuncList.TYPE.EVENT_DROP_RATE_UP, "Increase event drop rate"},
            {FuncList.TYPE.EVENT_DROP_UP, "Increase event drop"},
            {FuncList.TYPE.EVENT_POINT_RATE_UP, ""},
            {FuncList.TYPE.EVENT_POINT_UP, "Increase event point gained"},
            {FuncList.TYPE.EXP_UP, "Increase EXP gained"},
            {FuncList.TYPE.EXTEND_BUFFCOUNT, ""},
            {FuncList.TYPE.EXTEND_BUFFTURN, ""},
            {FuncList.TYPE.EXTEND_SKILL, "Increase skill cooldown"},
            {FuncList.TYPE.FIX_COMMANDCARD, "Lock card deck"},
            {FuncList.TYPE.FORCE_ALL_BUFF_NOACT, ""},
            {FuncList.TYPE.FORCE_INSTANT_DEATH, "Apply Instant Death"},
            {FuncList.TYPE.FRIEND_POINT_UP, "Increase FP gained"},
            {FuncList.TYPE.FRIEND_POINT_UP_DUPLICATE, "Increase FP gained (can duplicate)"},
            {FuncList.TYPE.GAIN_HP, "Gain HP"},
            {FuncList.TYPE.GAIN_HP_FROM_TARGETS, "Drain HP from target"},
            {FuncList.TYPE.GAIN_HP_PER, "Gain HP based on percentage"},
            {FuncList.TYPE.GAIN_NP, "Gain NP"},
            {FuncList.TYPE.GAIN_NP_BUFF_INDIVIDUAL_SUM, "Gain NP based on certain state count"},
            {FuncList.TYPE.GAIN_NP_FROM_TARGETS, "Gain NP from target"},
            {FuncList.TYPE.GAIN_STAR, "Gain Critical Stars"},
            // https://apps.atlasacademy.io/db/#/JP/skill/964246
            {FuncList.TYPE.GET_REWARD_GIFT, ""},
            {FuncList.TYPE.HASTEN_NPTURN, "Gain charge"},
            {FuncList.TYPE.INSTANT_DEATH, "Inflict Death"},
            {FuncList.TYPE.LOSS_HP, "Reduce HP"},
            {FuncList.TYPE.LOSS_HP_PER, ""},
            {FuncList.TYPE.LOSS_HP_PER_SAFE, ""},
            {FuncList.TYPE.LOSS_HP_SAFE, "Reduce HP without killing"},
            {FuncList.TYPE.LOSS_NP, "Decrease NP"},
            {FuncList.TYPE.LOSS_STAR, "Decrease Critical Stars"},
            // https://youtu.be/lrHzvSckdSY?t=87
            {FuncList.TYPE.MOVE_POSITION, "Move position of Zeus"},
            {FuncList.TYPE.MOVE_TO_LAST_SUBMEMBER, "Move to last position in backline"},
            {FuncList.TYPE.NONE, "No effect"},
            {FuncList.TYPE.OVERWRITE_DEAD_TYPE, ""},
            {FuncList.TYPE.PT_SHUFFLE, "Shuffle party"},
            {FuncList.TYPE.QP_DROP_UP, "Increase QP drop rate"},
            {FuncList.TYPE.QP_UP, "Increase QP gained"},
            {FuncList.TYPE.QUICK_CHANGE_BG, "Change field"},
            {FuncList.TYPE.RELEASE_STATE, ""},
            {FuncList.TYPE.REPLACE_MEMBER, "Replace active party member"},
            {FuncList.TYPE.REVIVAL, "Revive"},
            {FuncList.TYPE.SEND_SUPPORT_FRIEND_POINT, ""},
            {FuncList.TYPE.SERVANT_FRIENDSHIP_UP, "Increase Bond points gained"},
            {FuncList.TYPE.SHORTEN_SKILL, "Decrease skill cooldown"},
            {FuncList.TYPE.SUB_STATE, "Remove effects"},
            {FuncList.TYPE.TRANSFORM_SERVANT, "Swap servant"},
            {FuncList.TYPE.USER_EQUIP_EXP_UP, "Increase Mystic Code EXP Gain"}
        };
    }
}