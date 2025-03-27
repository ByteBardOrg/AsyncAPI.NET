namespace ByteBard.AsyncAPI.Tests.Models
{
    using System;
    using ByteBard.AsyncAPI.Models;
    using NUnit.Framework;

    public class AsyncApiLicense_Should
    {
        [Test]
        public void V2_SerializeV2_WithNullWriter_Throws()
        {
            // Arrange
            var asyncApiLicense = new AsyncApiLicense();

            // Act
            // Assert
            Assert.Throws<ArgumentNullException>(() => { asyncApiLicense.SerializeV2(null); });
        }
    }
}
