using System.Collections.Generic;

namespace BTM
{
    abstract class CollectionQuery<BTMBase> : ICollectionQuery<BTMBase>
        where BTMBase : IBTMBase
    {
        private string collectionName, fieldDescription;
        private IBTMCollection<BTMBase> collection;
        private All<BTMBase> collectionFilter;
        private ActionSequence<BTMBase> editActions;

        public CollectionQuery(IBTMCollection<BTMBase> collection, string collectionName, string fieldDescription)
        {
            this.collection = collection;
            this.collectionFilter = new All<BTMBase>();
            this.editActions = new ActionSequence<BTMBase>();
            this.collectionName = collectionName;
            this.fieldDescription = fieldDescription;
        }

        public abstract IEnumerable<CommandBase> CreateFilterAdders(List<CommandBase> subcommands);
        public abstract IEnumerable<CommandBase> CreateSetterAdders(List<CommandBase> subcommands);
        public string CollectionName => collectionName;
        public string FieldDescription => fieldDescription;
        public IBTMCollection<BTMBase> Collection => collection;
        public All<BTMBase> Filter => collectionFilter;
        public ActionSequence<BTMBase> EditActions => editActions;
    }

    interface ICollectionQuery<BTMBase>
        where BTMBase : IBTMBase
    {
        IEnumerable<CommandBase> CreateFilterAdders(List<CommandBase> subcommands);
        IEnumerable<CommandBase> CreateSetterAdders(List<CommandBase> subcommands);
        string CollectionName { get; }
        string FieldDescription { get; }
        IBTMCollection<BTMBase> Collection { get; }
        All<BTMBase> Filter { get; }
        ActionSequence<BTMBase> EditActions { get; }
    }

    interface IBuildableCollectionQuery<BTMBase, BTMBaseBuilder> : ICollectionQuery<BTMBase>
        where BTMBase : IBTMBase
        where BTMBaseBuilder : IBTMBuilder<BTMBase>
    {
        public IEnumerable<CommandBase> CreateBuilderAdders(List<CommandBase> subcommands, BTMBaseBuilder builder);
    }
}
