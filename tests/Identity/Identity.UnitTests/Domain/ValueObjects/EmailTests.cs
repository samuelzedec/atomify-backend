using Bogus;
using Xunit;
using FluentAssertions;
using Identity.Domain.ValueObjects;
using BuildingBlocks.SharedKernel.Exceptions;

namespace Identity.UnitTests.Domain.ValueObjects;

public sealed class EmailTests
{
    [Fact]
    public void Create_WithValidValue_ShouldCreateEmail()
    {
        // Arrange
        var value = new Faker().Internet.Email()
            .ToLowerInvariant();

        // Act
        var result = Email.Create(value);

        // Assert
        result.Value.Should().Be(value);
    }

    [Fact]
    public void ImplicitConversionToString_ShouldReturnValue()
    {
        // Arrange
        var value = new Faker().Internet.Email()
            .ToLowerInvariant();

        // Act
        var result = Email.Create(value);
        string resultString = result;

        // Assert
        resultString.Should().Be(value);
    }

    [Fact]
    public void ToString_ShouldReturnEmailValue()
    {
        // Arrange
        var value = new Faker().Internet.Email()
            .ToLowerInvariant();

        // Act
        var result = Email.Create(value);

        // Assert
        result.ToString().Should().Be(value);
    }

    [Fact]
    public void Create_WithSpacesAroundEmail_ShouldTrimValue()
    {
        // Arrange
        var value = $"   {new Faker().Internet.Email().ToLowerInvariant()}   ";

        // Act
        var result = Email.Create(value);

        // Assert
        result.Value.Should().Be(value.Trim());
    }

    [Fact]
    public void Create_WithUppercaseEmail_ShouldNormalizeValue()
    {
        // Arrange
        var value = new Faker().Internet.Email().ToUpperInvariant();

        // Act
        var result = Email.Create(value);

        // Assert
        result.ToString().Should().Be(value.ToLowerInvariant());
    }

    [Theory]
    [InlineData("\n")]
    [InlineData("\t")]
    [InlineData("")]
    [InlineData("     ")]
    public void Create_WhenEmailIsEmpty_ShouldThrowAtomifyException(string value)
    {
        // Act
        var result = () => Email.Create(value);

        // Assert
        result.Should().Throw<AtomifyException>()
            .WithMessage("O email não pode ser vazio.");
    }

    [Fact]
    public void Create_WithEmailExceedingMaxLength_ShouldThrowAtomifyException()
    {
        // Arrange
        var value = new Faker().Random.String2(Email.MaxLength + 1);

        // Act
        var result = () => Email.Create(value);

        // Assert
        result.Should().Throw<AtomifyException>()
            .WithMessage($"O email deve ter no máximo {Email.MaxLength} caracteres.");
    }

    [Fact]
    public void Create_WithInvalidEmail_ShouldThrowAtomifyException()
    {
        // Arrange
        var value = new Faker().Internet.UserName();

        // Act
        var result = () => Email.Create(value);

        // Assert
        result.Should().Throw<AtomifyException>()
            .WithMessage("O email deve ter o formato válido.");
    }
}