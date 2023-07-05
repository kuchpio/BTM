using System.Collections.Generic;

namespace BTM
{
    class FindCommand : KeywordConsumer, IHelpable
    {
        public FindCommand() : base(new List<CommandBase>()
        {
            new DisplayFilteredCollection<ILine>(new LineQuery()),
            new DisplayFilteredCollection<IStop>(new StopQuery()),
            new DisplayFilteredCollection<IBytebus>(new BytebusQuery()),
            new DisplayFilteredCollection<ITram>(new TramQuery()),
            new DisplayFilteredCollection<IVehicle>(new VehicleQuery()),
            new DisplayFilteredCollection<IDriver>(new DriverQuery())
        }, "find")
        { }

        public string HelpKeyword => "find";

        public string Help =>
@$"USAGE: find <collection> [(<attribute>(=|>|<)<value>)...]

collection: line|stop|bytebus|tram|vehicle|driver

Shows filtered entities listed in specified collection.
When necessary, <value> can be surrounded by double quotes ("").
";

        private class DisplayFilteredCollection<BTMBase> : CollectionSelector<BTMBase>
            where BTMBase : IBTMBase
        {
            public DisplayFilteredCollection(ICollectionQuery<BTMBase> query) :
                base(query)
            {
                subcommands.AddRange(query.CreateFilterAdders(subcommands));
                subcommands.Add(new FilteredCollectionPrinter<BTMBase>(query));
            }
        }
    }
}
