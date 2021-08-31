using Pepper.Services.FGO;
using Pepper.Structures.External.FGO.MasterData;

namespace Pepper.Structures.External.FGO.Renderer
{
    public abstract class EntityRenderer<T> where T : MasterDataEntity
    {
        protected IMasterDataProvider Connection;
        protected EntityRenderer(T _, IMasterDataProvider connection)
        {
            Connection = connection;
        }
    }
}