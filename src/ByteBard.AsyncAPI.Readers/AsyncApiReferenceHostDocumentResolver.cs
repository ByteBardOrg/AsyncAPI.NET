namespace ByteBard.AsyncAPI.Readers
{
    using System.Collections.Generic;
    using ByteBard.AsyncAPI.Models;
    using ByteBard.AsyncAPI.Models.Interfaces;
    using ByteBard.AsyncAPI.Services;

    internal class AsyncApiReferenceWorkspaceResolver : AsyncApiVisitorBase
    {
        private AsyncApiWorkspace workspace;

        public AsyncApiReferenceWorkspaceResolver(
            AsyncApiWorkspace workspace)
        {
            this.workspace = workspace;
        }

        public override void Visit(IAsyncApiReferenceable referenceable)
        {
            if (referenceable.Reference != null)
            {
                referenceable.Reference.Workspace = this.workspace;
            }
        }
    }
}