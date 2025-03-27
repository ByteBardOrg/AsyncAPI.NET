namespace ByteBard.AsyncAPI.Tests.Models
{
    using System;
    using ByteBard.AsyncAPI.Models;
    using NUnit.Framework;

    public class AsyncApiInfo_Should
    {
        [Test]
        public void V2_SerializeV2_WithNullWriter_Throws()
        {
            // Arrange
            var asyncApiInfo = new AsyncApiInfo();

            // Act
            // Assert
            Assert.Throws<ArgumentNullException>(() => { asyncApiInfo.SerializeV2(null); });
        }
    }
}
