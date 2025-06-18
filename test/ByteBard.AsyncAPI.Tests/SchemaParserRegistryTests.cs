namespace ByteBard.AsyncAPI.Tests;
using NUnit.Framework;
using Readers;

public class SchemaParserRegistryTests
{
    private SchemaParserRegistry registry;
    
    public SchemaParserRegistryTests()
    {
        this.registry = new SchemaParserRegistry();
    }

    [Test]
    public void GetParser_WithNullFormat_ReturnsJsonSchemaParser()
    {
        // Arrange
        // Act
        var result = this.registry.GetParser(null);

        // Assert
        Assert.IsNotNull(result);
        Assert.True(result is JsonSchemaParser);
    }

    [Test]
    public void GetParser_WithEmptyFormat_ReturnsJsonSchemaParser()
    {
        // Arrange
        // Act
        var result = this.registry.GetParser(string.Empty);

        // Assert
        Assert.IsNotNull(result);
        Assert.True(result is JsonSchemaParser);
    }

    [Test]
    public void GetParser_WithExactKeyMatch_ReturnsCorrectParser()
    {
        // Arrange
        // Act
        var result = this.registry.GetParser("application/vnd.aai.asyncapi+json");

        // Assert
        Assert.IsNotNull(result);
    }

    [Test]
    public void GetParser_WithPrefixMatch_ReturnsCorrectParser()
    {
        // Arrange
        // Act
        var result = this.registry.GetParser("application/vnd.aai.asyncapi+json;charset=utf-8");

        // Assert
        Assert.IsNotNull(result);
    }

    [Test]
    public void GetParser_WithUnknownFormat_ReturnsNull()
    {
        // Arrange
        // Act
        var result = this.registry.GetParser("application/unknown+format");

        // Assert
        Assert.IsNull(result);
    }

    [Test]
    public void GetParser_WithNonMatchingPrefix_ReturnsNull()
    {
        // Arrange
        // Act
        var result = this.registry.GetParser("text/plain");

        // Assert
        Assert.IsNull(result);
    }
}