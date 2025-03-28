namespace ByteBard.AsyncAPI.Readers.Services
{
    using System.Collections.Generic;
    using ByteBard.AsyncAPI.Models.Interfaces;
    using ByteBard.AsyncAPI.Services;

    internal class AsyncApiReferenceCollector : AsyncApiVisitorBase
    {
        private readonly List<IAsyncApiReferenceable> references = new();
        private AsyncApiWorkspace workspace;

        public AsyncApiReferenceCollector(
            AsyncApiWorkspace workspace)
        {
            this.workspace = workspace;
        }

        /// <summary>
        /// List of all external references collected from AsyncApiDocument.
        /// </summary>
        public IEnumerable<IAsyncApiReferenceable> References
        {
            get
            {
                return this.references;
            }
        }

        /// <summary>
        /// Collect reference for each reference.
        /// </summary>
        /// <param name="referenceable"></param>
        public override void Visit(IAsyncApiReferenceable referenceable)
        {
            if (referenceable.Reference != null)
            {
                if (referenceable.Reference.Workspace == null)
                {
                    referenceable.Reference.Workspace = this.workspace;
                }

                this.references.Add(referenceable);
            }
        }
    }
}
