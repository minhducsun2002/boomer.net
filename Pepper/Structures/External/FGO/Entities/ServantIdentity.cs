namespace Pepper.Structures.External.FGO.Entities
{
    public class ServantIdentity
    {
        public int ServantId;
        public static implicit operator int(ServantIdentity servant) => servant.ServantId;
    }
}