using System.Collections.Concurrent;
using Pepper.Services.FGO;
using Pepper.Structures;
using Pepper.Structures.Commands;

namespace Pepper.Commands.FGO
{
    [Category("FGO")]
    public abstract class FGOCommand : Command
    {
        public MasterDataService MasterDataService { get; set; }
        public ServantNamingService ServantNamingService { get; set; }
        public TraitService TraitService { get; set; }
        public ItemNamingService ItemNamingService { get; set; }

        public int[] AttributeList => MasterDataService.Connections[Region.JP].GetAttributeLists();
        public ConcurrentDictionary<long, ServantNaming> ServantNamings => ServantNamingService.Namings;
        public ConcurrentDictionary<long, string> ItemNamings => ItemNamingService.Namings;
    }
}