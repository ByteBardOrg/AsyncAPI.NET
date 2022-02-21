using System;
using System.Collections.Generic;
using LEGO.AsyncAPI.Models;
using LEGO.AsyncAPI.Models.Any;
using Xunit;
using String = LEGO.AsyncAPI.Models.Any.String;

namespace LEGO.AsyncAPI.E2E.Tests.readers.samples.AsyncApi.AsyncApiContactObject
{
    public class ShouldConsumeContact : ShouldConsumeProduceBase<Contact>
    {
        public ShouldConsumeContact() : base(typeof(ShouldConsumeContact))
        {
        }

        [Fact]
        public void ShouldConsumeMinimalSpec()
        {
            Assert.IsType<Contact>(_asyncApiAsyncApiReader.Read(GetStream("Minimal.json")));
        }

        [Fact]
        public void ShouldConsumeCompleteSpec()
        {
            var output = _asyncApiAsyncApiReader.Read(GetStreamWithMockedExtensions("Complete.json"));

            Assert.Equal("foo", output.Name);
            Assert.Equal(new Uri("https://lego.com"), output.Uri);
            Assert.Equal("asyncApiContactObject@lego.com", output.Email);
            var extensions = output.Extensions;
            Assert.IsAssignableFrom<IDictionary<string, IAny>>(extensions);
            var extension = (String)extensions["x-ext-string"];
            Assert.Equal("bar", (string) extension!);
        }
    }
}