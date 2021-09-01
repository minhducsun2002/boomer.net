using System;
using MongoDB.Bson.Serialization.Attributes;

namespace Pepper.Structures.External.FGO.MasterData
{
    public class MstQuest : MasterDataEntity
    {
        [BsonElement("beforeActionVals")] public string[] BeforeActionVals = Array.Empty<string>();
        [BsonElement("afterActionVals")] public string[] AfterActionVals = Array.Empty<string>();
        [BsonElement("id")] public int ID;
        [BsonElement("name")] public string Name = string.Empty;
        [BsonElement("nameRuby")] public string NameRuby = string.Empty;
        [BsonElement("type")] public int Type;
        [BsonElement("consumeType")] public int ConsumeType;
        [BsonElement("actConsume")] public int ActConsume;
        [BsonElement("chaldeaGateCategory")] public int ChaldeaGateCategory;
        [BsonElement("spotId")] public int SpotId;
        [BsonElement("giftId")] public int GiftId;
        [BsonElement("priority")] public int Priority;
        [BsonElement("bannerType")] public int BannerType;
        [BsonElement("bannerId")] public int BannerId;
        [BsonElement("iconId")] public int IconId;
        [BsonElement("charaIconId")] public int CharaIconId;
        [BsonElement("giftIconId")] public int GiftIconId;
        [BsonElement("forceOperation")] public int ForceOperation;
        [BsonElement("afterClear")] public int AfterClear;
        [BsonElement("displayHours")] public int DisplayHours;
        [BsonElement("intervalHours")] public int IntervalHours;
        [BsonElement("chapterId")] public int ChapterId;
        [BsonElement("chapterSubId")] public int ChapterSubId;
        [BsonElement("chapterSubStr")] public string ChapterSubStr = string.Empty;
        [BsonElement("recommendLv")] public string RecommendLv = string.Empty;
        [BsonElement("hasStartAction")] public int HasStartAction;
        [BsonElement("flag")] public long Flag;
        [BsonElement("scriptQuestId")] public int ScriptQuestId;
        [BsonElement("noticeAt")] public int NoticeAt;
        [BsonElement("openedAt")] public int OpenedAt;
        [BsonElement("closedAt")] public int ClosedAt;
    }
}