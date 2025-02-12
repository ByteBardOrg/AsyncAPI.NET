// Copyright (c) The LEGO Group. All rights reserved.

namespace LEGO.AsyncAPI.Models
{
    using LEGO.AsyncAPI.Attributes;

    public enum AsyncApiAction
    {
        [Display("send")]
        Send,

        [Display("receive")]
        Receive,
    }
}