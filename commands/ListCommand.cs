using System.Collections.Generic;

namespace BTM
{
    class ListCommand : KeywordConsumer, IHelpable
    {
        public ListCommand() : base(new List<CommandBase>()
        {
            new DisplayCollection<ILine>(new LineQuery()),
            new DisplayCollection<IStop>(new StopQuery()),
            new DisplayCollection<IBytebus>(new BytebusQuery()),
            new DisplayCollection<ITram>(new TramQuery()),
            new DisplayCollection<IVehicle>(new VehicleQuery()),
            new DisplayCollection<IDriver>(new DriverQuery())
        }, "list")
        { }

        public string HelpKeyword => "list";

        public string Help =>
@"USAGE: list <collection>

collection: line|stop|bytebus|tram|vehicle|driver

Shows all entities listed in specified collection.
";

        private class DisplayCollection<BTMBase> : CollectionSelector<BTMBase>
            where BTMBase : IBTMBase
        {
            public DisplayCollection(ICollectionQuery<BTMBase> query) :
                base(new FilteredCollectionPrinter<BTMBase>(query), query)
            { }
        }
    }
}
