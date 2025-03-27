namespace ByteBard.AsyncAPI.Tests.Models
{
    using System;
    using System.Collections.Generic;
    using ByteBard.AsyncAPI.Models;
    using NUnit.Framework;

    public class AsyncApiSecurityRequirement_Should
    {
        [Test]
        public void SerializeV2_WithNullWriter_Throws()
        {
            // Arrange
            var asyncApiSecurityRequirement = new AsyncApiSecurityRequirement();

            // Act
            // Assert
            Assert.Throws<ArgumentNullException>(() => { asyncApiSecurityRequirement.SerializeV2(null); });
        }

        [Test]
        public void SerializeV2_Serializes()
        {
            var asyncApiSecurityRequirement = new AsyncApiSecurityRequirement();
            asyncApiSecurityRequirement.Add(new AsyncApiSecuritySchemeReference("apiKey"), new List<string> { "string" });

            var output = asyncApiSecurityRequirement.SerializeAsYaml(AsyncApiVersion.AsyncApi2_0);
        }
    }
}
