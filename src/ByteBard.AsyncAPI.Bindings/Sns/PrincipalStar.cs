namespace ByteBard.AsyncAPI.Bindings.Sns;

using System;
using ByteBard.AsyncAPI.Writers;

public class PrincipalStar : Principal
{
    public string Value { get; private set; }

    public PrincipalStar()
    {
        this.Value = "*";
    }

    public override void Serialize(IAsyncApiWriter writer)
    {
        if (writer is null)
        {
            throw new ArgumentNullException(nameof(writer));
        }

        writer.WriteValue(this.Value);
    }
}