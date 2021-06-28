using Pepper.Services.FGO;
using Pepper.Structures.External.FGO.MasterData;

namespace Pepper.Structures.External.FGO.Renderer
{
    public abstract class EntityRenderer<T> where T : MasterDataEntity
    {
        protected MasterDataMongoDBConnection Connection;
        protected EntityRenderer(T entity, MasterDataMongoDBConnection connection)
        {
            Connection = connection;
        }
    }
}