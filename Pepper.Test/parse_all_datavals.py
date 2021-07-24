import argparse
import re
import json
import time
from enum import IntEnum
from pathlib import Path
from typing import Union
from os import path


class FuncType(IntEnum):
    NONE = 0
    ADD_STATE = 1
    SUB_STATE = 2
    DAMAGE = 3
    DAMAGE_NP = 4
    GAIN_STAR = 5
    GAIN_HP = 6
    GAIN_NP = 7
    LOSS_NP = 8
    SHORTEN_SKILL = 9
    EXTEND_SKILL = 10
    RELEASE_STATE = 11
    LOSS_HP = 12
    INSTANT_DEATH = 13
    DAMAGE_NP_PIERCE = 14
    DAMAGE_NP_INDIVIDUAL = 15
    ADD_STATE_SHORT = 16
    GAIN_HP_PER = 17
    DAMAGE_NP_STATE_INDIVIDUAL = 18
    HASTEN_NPTURN = 19
    DELAY_NPTURN = 20
    DAMAGE_NP_HPRATIO_HIGH = 21
    DAMAGE_NP_HPRATIO_LOW = 22
    CARD_RESET = 23
    REPLACE_MEMBER = 24
    LOSS_HP_SAFE = 25
    DAMAGE_NP_COUNTER = 26
    DAMAGE_NP_STATE_INDIVIDUAL_FIX = 27
    DAMAGE_NP_SAFE = 28
    CALL_SERVANT = 29
    PT_SHUFFLE = 30
    LOSS_STAR = 31
    CHANGE_SERVANT = 32
    CHANGE_BG = 33
    DAMAGE_VALUE = 34
    WITHDRAW = 35
    FIX_COMMANDCARD = 36
    SHORTEN_BUFFTURN = 37
    EXTEND_BUFFTURN = 38
    SHORTEN_BUFFCOUNT = 39
    EXTEND_BUFFCOUNT = 40
    CHANGE_BGM = 41
    DISPLAY_BUFFSTRING = 42
    RESURRECTION = 43
    GAIN_NP_BUFF_INDIVIDUAL_SUM = 44
    SET_SYSTEM_ALIVE_FLAG = 45
    FORCE_INSTANT_DEATH = 46
    DAMAGE_NP_RARE = 47
    GAIN_NP_FROM_TARGETS = 48
    GAIN_HP_FROM_TARGETS = 49
    LOSS_HP_PER = 50
    LOSS_HP_PER_SAFE = 51
    SHORTEN_USER_EQUIP_SKILL = 52
    QUICK_CHANGE_BG = 53
    SHIFT_SERVANT = 54
    DAMAGE_NP_AND_CHECK_INDIVIDUALITY = 55
    ABSORB_NPTURN = 56
    OVERWRITE_DEAD_TYPE = 57
    FORCE_ALL_BUFF_NOACT = 58
    BREAK_GAUGE_UP = 59
    BREAK_GAUGE_DOWN = 60
    MOVE_TO_LAST_SUBMEMBER = 61
    EXP_UP = 101
    QP_UP = 102
    DROP_UP = 103
    FRIEND_POINT_UP = 104
    EVENT_DROP_UP = 105
    EVENT_DROP_RATE_UP = 106
    EVENT_POINT_UP = 107
    EVENT_POINT_RATE_UP = 108
    TRANSFORM_SERVANT = 109
    QP_DROP_UP = 110
    SERVANT_FRIENDSHIP_UP = 111
    USER_EQUIP_EXP_UP = 112
    CLASS_DROP_UP = 113
    ENEMY_ENCOUNT_COPY_RATE_UP = 114
    ENEMY_ENCOUNT_RATE_UP = 115
    ENEMY_PROB_DOWN = 116
    GET_REWARD_GIFT = 117
    SEND_SUPPORT_FRIEND_POINT = 118
    MOVE_POSITION = 119
    REVIVAL = 120
    DAMAGE_NP_INDIVIDUAL_SUM = 121
    DAMAGE_VALUE_SAFE = 122
    FRIEND_POINT_UP_DUPLICATE = 123
    MOVE_STATE = 124
    CHANGE_BGM_COSTUME = 125
    FUNC_126 = 126
    FUNC_127 = 127


def remove_brackets(val_string: str) -> str:
    return val_string.removeprefix("[").removesuffix("]")


EVENT_DROP_FUNCTIONS = {
    FuncType.EVENT_POINT_UP,
    FuncType.EVENT_POINT_RATE_UP,
    FuncType.EVENT_DROP_UP,
    FuncType.EVENT_DROP_RATE_UP,
}
EVENT_FUNCTIONS = EVENT_DROP_FUNCTIONS | {
    FuncType.ENEMY_ENCOUNT_COPY_RATE_UP,
    FuncType.ENEMY_ENCOUNT_RATE_UP,
}
FRIEND_SUPPORT_FUNCTIONS = {
    FuncType.SERVANT_FRIENDSHIP_UP,
    FuncType.USER_EQUIP_EXP_UP,
    FuncType.EXP_UP,
    FuncType.QP_DROP_UP,
    FuncType.QP_UP,
}
LIST_DATAVALS = {
    # "TargetList",
    # "TargetRarityList",
    # "AndCheckIndividualityList",
    # "ParamAddSelfIndividuality",
    # "ParamAddOpIndividuality",
    # "ParamAddFieldIndividuality",
}


def parse_dataVals(
    datavals: str, functype: int, mstFuncId: dict[int, dict[str, int]]
) -> dict[str, Union[int, str, list[int]]]:
    error_message = f"Can't parse datavals: {datavals}"
    initial_value = -100000
    # Prefix to be used for temporary keys that need further parsing.
    # Some functions' datavals can't be parsed by themselves and need the first
    # or second datavals to determine whether it's a rate % or an absolute value.
    # See the "Further parsing" section.
    # The prefix should be something unlikely to be a dataval key.
    prefix = "aa"

    output: dict[str, Union[int, str, list[int]]] = {}
    if datavals != "[]":
        datavals = remove_brackets(datavals)
        array = re.split(r",\s*(?![^\[\]]*])", datavals)
        for i, arrayi in enumerate(array):
            text = ""
            value = initial_value
            try:
                value = int(arrayi)
                if functype in {
                    FuncType.DAMAGE_NP_INDIVIDUAL,
                    FuncType.DAMAGE_NP_STATE_INDIVIDUAL,
                    FuncType.DAMAGE_NP_STATE_INDIVIDUAL_FIX,
                    FuncType.DAMAGE_NP_INDIVIDUAL_SUM,
                    FuncType.DAMAGE_NP_RARE,
                    FuncType.DAMAGE_NP_AND_CHECK_INDIVIDUALITY,
                }:
                    if i == 0:
                        text = "Rate"
                    elif i == 1:
                        text = "Value"
                    elif i == 2:
                        text = "Target"
                    elif i == 3:
                        text = "Correction"
                elif functype in {FuncType.ADD_STATE, FuncType.ADD_STATE_SHORT}:
                    if i == 0:
                        text = "Rate"
                    elif i == 1:
                        text = "Turn"
                    elif i == 2:
                        text = "Count"
                    elif i == 3:
                        text = "Value"
                    elif i == 4:
                        text = "UseRate"
                    elif i == 5:
                        text = "Value2"
                elif functype == FuncType.SUB_STATE:
                    if i == 0:
                        text = "Rate"
                    elif i == 1:
                        text = "Value"
                    elif i == 2:
                        text = "Value2"
                elif functype == FuncType.TRANSFORM_SERVANT:
                    if i == 0:
                        text = "Rate"
                    elif i == 1:
                        text = "Value"
                    elif i == 2:
                        text = "Target"
                    elif i == 3:
                        text = "SetLimitCount"
                elif functype in EVENT_FUNCTIONS:
                    if i == 0:
                        text = "Individuality"
                    elif i == 3:
                        text = "EventId"
                    else:
                        text = prefix + str(i)
                elif functype == FuncType.CLASS_DROP_UP:
                    if i == 2:
                        text = "EventId"
                    else:
                        text = prefix + str(i)
                elif functype == FuncType.ENEMY_PROB_DOWN:
                    if i == 0:
                        text = "Individuality"
                    elif i == 1:
                        text = "RateCount"
                    elif i == 2:
                        text = "EventId"
                elif functype in FRIEND_SUPPORT_FUNCTIONS:
                    if i == 2:
                        text = "Individuality"
                    else:
                        text = prefix + str(i)
                elif functype in {
                    FuncType.FRIEND_POINT_UP,
                    FuncType.FRIEND_POINT_UP_DUPLICATE,
                }:
                    if i == 0:
                        text = "AddCount"
                else:
                    if i == 0:
                        text = "Rate"
                    elif i == 1:
                        text = "Value"
                    elif i == 2:
                        text = "Target"

            except ValueError as e:
                value = str(arrayi)
                # array2 = re.split(r":\s*(?![^\[\]]*])", arrayi)
                # if len(array2) > 1:
                #     if array2[0] == "DependFuncId1":
                #         output["DependFuncId"] = int(remove_brackets(array2[1]))
                #     elif array2[0] == "DependFuncVals1":
                #         output["DependFuncVals"] = array2[1]
                #     elif array2[0] in LIST_DATAVALS:
                #         try:
                #             output[array2[0]] = [int(i) for i in array2[1].split("/")]
                #         except ValueError as ve:
                #             raise Exception(error_message) from ve
                #     else:
                #         try:
                #             text = array2[0]
                #             # value = int(array2[1])
                #             value = array2[1]
                #         except ValueError as ve:
                #             raise Exception(error_message) from ve
                # else:
                #     raise Exception(error_message) from e
            if text:
                output[text] = str(value)

        # if not any(key.startswith(prefix) for key in output):
            # if len(array) != len(output) and functype != FuncType.NONE:
                # print(
                #    f"Some datavals weren't parsed for func type {functype}: [{datavals}] => {output}"
                # )

    # Further parsing
    if functype in EVENT_FUNCTIONS:
        if output[f"{prefix}1"] == 1:
            output["AddCount"] = output[f"{prefix}2"]
        elif output[f"{prefix}1"] == 2:
            output["RateCount"] = output[f"{prefix}2"]
    elif functype in {FuncType.CLASS_DROP_UP} | FRIEND_SUPPORT_FUNCTIONS:
        if output[f"{prefix}0"] == 1:
            output["AddCount"] = output[f"{prefix}1"]
        elif output[f"{prefix}0"] == 2:
            output["RateCount"] = output[f"{prefix}1"]

    return dict({(key, str(value)) for key, value in output.items() if not key.startswith(prefix)})


def main(master_path: str, output: str) -> None:
    skill_lv_path = path.join(output, "parsed_mstSkillLv.json")
    treasure_device_lv_path = path.join(output, "parsed_mstTreasureDeviceLv.json")
    Path(output).mkdir(parents=True, exist_ok=True)

    master = Path(master_path).resolve()
    mstFunc = json.loads((master / "mstFunc.json").read_text(encoding="utf-8"))
    mstFuncId = {func["id"]: func for func in mstFunc}

    # if not Path(skill_lv_path).exists():
    if True:
        mstSkillLv = json.loads((master / "mstSkillLv.json").read_text(encoding="utf-8"))
        for skillLv in mstSkillLv:
            svalsParsed = []
            for funcId, datavals in zip(skillLv["funcId"], skillLv["svals"]):
                if funcId in mstFuncId:
                    funcType = mstFuncId[funcId]["funcType"]
                    parsedDatavals = parse_dataVals(datavals, funcType, mstFuncId)
                    svalsParsed.append(parsedDatavals)
                else:
                    svalsParsed.append({})
    
                skillLv["svalsParsed"] = svalsParsed
        with open(skill_lv_path, "w", encoding="utf-8") as fp:
            json.dump(mstSkillLv, fp)

    # if not Path(treasure_device_lv_path).exists():
    if True:
        mstTreasureDeviceLv = json.loads(
            (master / "mstTreasureDeviceLv.json").read_text(encoding="utf-8")
        )
        for tdLv in mstTreasureDeviceLv:
            for svals in ["svals", "svals2", "svals3", "svals4", "svals5"]:
                svalsParsed = []
                for funcId, datavals in zip(tdLv["funcId"], tdLv[svals]):
                    if funcId in mstFuncId:
                        funcType = mstFuncId[funcId]["funcType"]
                        parsedDatavals = parse_dataVals(datavals, funcType, mstFuncId)
                        svalsParsed.append(parsedDatavals)
                    else:
                        svalsParsed.append({})
                tdLv[f"{svals}Parsed"] = svalsParsed
    
        with open(treasure_device_lv_path, "w", encoding="utf-8") as fp:
            json.dump(mstTreasureDeviceLv, fp)


if __name__ == "__main__":
    start_time = time.perf_counter()

    parser = argparse.ArgumentParser()
    parser.add_argument("master", type=str, help="Path to master folder")
    parser.add_argument("output", type=str, help="Path to output folder")

    args = parser.parse_args()
    main(args.master, args.output)

    end_time = time.perf_counter()
    print(f"Time elapsed    : {end_time - start_time:.4f}s")
