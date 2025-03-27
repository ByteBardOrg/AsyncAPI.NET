namespace ByteBard.AsyncAPI.Readers.ParseNodes
{
    using System;
    using System.Collections.Generic;

    public class FixedFieldMap<T> : Dictionary<string, Action<T, ParseNode>>
    {
    }
}
