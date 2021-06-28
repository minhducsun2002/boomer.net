using System;
using MongoDB.Bson.Serialization.Attributes;

namespace Pepper.Structures.External.FGO.MasterData
{
    public class MstTreasureDeviceLv : MasterDataEntity
    {

        [BsonElement("funcId")] public int[] FuncId = Array.Empty<int>();
        [BsonElement("svals")] public string[] Svals = Array.Empty<string>();
        [BsonElement("svals2")] public string[] Svals2 = Array.Empty<string>();
        [BsonElement("svals3")] public string[] Svals3 = Array.Empty<string>();
        [BsonElement("svals4")] public string[] Svals4 = Array.Empty<string>();
        [BsonElement("svals5")] public string[] Svals5 = Array.Empty<string>();
        [BsonElement("treaureDeviceId")] public int TreaureDeviceId;
        [BsonElement("lv")] public int Lv;
        [BsonElement("gaugeCount")] public int GaugeCount;
        [BsonElement("detailId")] public int DetailId;
        [BsonElement("tdPoint")] public int TdPoint;
        [BsonElement("tdPointQ")] public int TdPointQ;
        [BsonElement("tdPointA")] public int TdPointA;
        [BsonElement("tdPointB")] public int TdPointB;
        [BsonElement("tdPointEx")] public int TdPointEx;
        [BsonElement("tdPointDef")] public int TdPointDef;
        [BsonElement("qp")] public int Qp;
    }
}