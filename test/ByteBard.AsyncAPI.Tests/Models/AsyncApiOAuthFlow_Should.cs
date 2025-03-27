namespace ByteBard.AsyncAPI.Tests.Models
{
    using System;
    using ByteBard.AsyncAPI.Models;
    using NUnit.Framework;

    public class AsyncApiOAuthFlow_Should
    {
        [Test]
        public void SerializeV2_WithNullWriter_Throws()
        {
            // Arrange
            var asyncApiOAuthFlow = new AsyncApiOAuthFlow();

            // Act
            // Assert
            Assert.Throws<ArgumentNullException>(() => { asyncApiOAuthFlow.SerializeV2(null); });
        }
    }
}
