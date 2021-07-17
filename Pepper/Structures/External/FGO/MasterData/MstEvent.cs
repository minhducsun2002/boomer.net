using MongoDB.Bson.Serialization.Attributes;

namespace Pepper.Structures.External.FGO.MasterData
{
    public class MstEvent : MasterDataEntity
    {
        [BsonElement("id")] public int ID;
        [BsonElement("baseEventId")] public int BaseEventId;
        [BsonElement("type")] public int Type;
        [BsonElement("openType")] public int OpenType;
        [BsonElement("name")] public string Name = string.Empty;
        [BsonElement("shortName")] public string ShortName = string.Empty;
        [BsonElement("detail")] public string Detail = string.Empty;
        [BsonElement("noticeBannerId")] public int NoticeBannerId;
        [BsonElement("bannerId")] public int BannerId;
        [BsonElement("iconId")] public int IconId;
        [BsonElement("bannerPriority")] public int BannerPriority;
        [BsonElement("openHours")] public int OpenHours;
        [BsonElement("intervalHours")] public int IntervalHours;
        [BsonElement("noticeAt")] public int NoticeAt;
        [BsonElement("startedAt")] public int StartedAt;
        [BsonElement("endedAt")] public int EndedAt;
        [BsonElement("finishedAt")] public int FinishedAt;
        [BsonElement("materialOpenedAt")] public int MaterialOpenedAt;
        [BsonElement("linkType")] public int LinkType;
        [BsonElement("linkBody")] public string LinkBody = string.Empty;
        [BsonElement("deviceType")] public int DeviceType;
        [BsonElement("myroomBgId")] public int MyroomBgId;
        [BsonElement("myroomBgmId")] public int MyroomBgmId;
        [BsonElement("createdAt")] public int CreatedAt;
    }
}