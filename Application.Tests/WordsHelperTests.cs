using Application.Helpers;

namespace Application.Tests;

public class WordsHelperTests
{
    [Fact]
    public void GetWordsWithCount_SimpleText_SuccessResult()
    {
        // Arrange
        string text = "apple orange banana apple orange";
        var expected = new Dictionary<string, int>
        {
            { "apple", 2 },
            { "orange", 2 },
            { "banana", 1 }
        };

        // Act
        var result = text.GetWordsWithCount();

        //Assert
        Assert.Equal(expected, result);
    }    
    
    [Fact]
    public void GetWordsWithCount_SimpleRussianText_SuccessResult()
    {
        // Arrange
        string text = "€блоко апельсин банан €блоко апельсин";
        var expected = new Dictionary<string, int>
        {
            { "€блоко", 2 },
            { "апельсин", 2 },
            { "банан", 1 }
        };

        // Act
        var result = text.GetWordsWithCount();

        //Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetWordsWithCount_TextWithPunctuation_SuccessResult()
    {
        // Arrange
        string text = "hello, world! hello world.";
        Dictionary<string, int> expected = new Dictionary<string, int>
        {
            { "hello", 2 },
            { "world", 2 }
        };

        // Act
        var result = text.GetWordsWithCount();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetWordsWithCount_EmptyString_EmptyDictionary()
    {
        // Arrange
        string text = "";
        Dictionary<string, int> expected = new Dictionary<string, int>();

        // Act
        var result = text.GetWordsWithCount();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void GetWordsWithCount_TextWithSpecialCharactersInsideShortWords_SuccessResult()
    {
        // Arrange
        string text = "Csharp(#) is awesome. I love Csharp(#)!";
        Dictionary<string, int> expected = new Dictionary<string, int>
        {
            { "awesome", 1 },
            { "Csharp", 2 },
            { "love", 1 }
        };

        // Act
        var result = text.GetWordsWithCount();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetWordsWithCount_LongWords_SuccessResult()
    {
        // Arrange
        string text = "Csharp isawesomeIloveCsharpSoMuch(#), isawesomeIloveCsharpSoMuch!";
        Dictionary<string, int> expected = new Dictionary<string, int>
        {
            { "Csharp", 1 }
        };

        // Act
        var result = text.GetWordsWithCount();

        // Assert
        Assert.Equal(expected, result);
    }
}