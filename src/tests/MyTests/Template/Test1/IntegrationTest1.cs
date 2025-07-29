using System;
using Xunit;

namespace Test1.Tests;

public class IntegrationTest1
{
    [Fact]
    public static void Test1()
    {
        Console.WriteLine("IntegrationTest1.Test1");

        // Arrange
        var expected = 42;

        // Act
        var actual = 40 + 2;

        // Assert
        Assert.Equal(expected, actual);
    }
}
