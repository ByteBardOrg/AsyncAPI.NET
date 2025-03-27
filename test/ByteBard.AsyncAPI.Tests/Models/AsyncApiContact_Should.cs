namespace ByteBard.AsyncAPI.Tests.Models
{
    using System;
    using ByteBard.AsyncAPI.Models;
    using NUnit.Framework;

    public class AsyncApiContact_Should
    {
        [Test]
        public void V2_SerializeV2_WithNullWriter_Throws()
        {
            // Arrange
            var asyncApiContact = new AsyncApiContact();

            // Act
            // Assert
            Assert.Throws<ArgumentNullException>(() => { asyncApiContact.SerializeV2(null); });
        }
    }
}
