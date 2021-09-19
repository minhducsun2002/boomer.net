using Pepper.Structures.External.FGO.Entities;
using Pepper.Structures.External.FGO.MasterData;

namespace Pepper.Structures.External.FGO
{
    public interface IServantDataProvider
    {
        public BaseServant GetServant(MstSvt svt) => GetServant(svt.ID, svt);
        public BaseServant GetServant(int id, MstSvt? hint = null);
        public MstSvt? GetServantEntityById(int id);
        public MstSvt? GetServantEntityByCollectionNo(int collectionNo);
        public ServantLimits GetServantLimits(int servantId);
        public MstSvt[] GetAllServantEntities();
    }
}