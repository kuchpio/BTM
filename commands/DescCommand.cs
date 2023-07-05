using System.Collections.Generic;

namespace BTM
{
    class DescCommand : KeywordConsumer, IHelpable
    {
        public DescCommand() :
            base(new List<CommandBase>() {
                new DisplayFieldDescription<ILine>(new LineQuery()),
                new DisplayFieldDescription<IStop>(new StopQuery()),
                new DisplayFieldDescription<IBytebus>(new BytebusQuery()),
                new DisplayFieldDescription<ITram>(new TramQuery()),
                new DisplayFieldDescription<IVehicle>(new VehicleQuery()),
                new DisplayFieldDescription<IDriver>(new DriverQuery()),
            }, "desc")
        { }

        public string HelpKeyword => "desc";

        public string Help =>
@"USAGE: desc <collection>

collection: line|stop|bytebus|tram|vehicle|driver

Shows avalible attributes for each collection with corresponding
value types. 
";

        private class DisplayFieldDescription<BTMBase> : KeywordConsumer
            where BTMBase : IBTMBase
        {
            public DisplayFieldDescription(ICollectionQuery<BTMBase> query) :
                base(new ConsoleLineWriter(query.FieldDescription), query.CollectionName)
            { }
        }
    }
}
