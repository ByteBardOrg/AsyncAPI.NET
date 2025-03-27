namespace ByteBard.AsyncAPI.Readers.Interface
{
    using ByteBard.AsyncAPI.Models;

    public interface IAsyncApiReader<TInput, TDiagnostic>
        where TDiagnostic : IDiagnostic
    {
        AsyncApiDocument Read(TInput input, out TDiagnostic diagnostic);
    }
}