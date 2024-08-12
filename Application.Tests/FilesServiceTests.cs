namespace Application.Tests;

using Application.Helpers;
using Application.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System.IO;
using Xunit;

public class FilesServiceTests
{
    private readonly Mock<IWordsService> _wordsServiceMock = new();
    private readonly Mock<ILogger<IFilesService>> _loggerMock = new();
    private readonly Mock<ITextGenerator> _textGeneratorMock = new();
    private const string filePath = "test.txt";

    [Fact]
    public void WriteWords_WritesCorrectNumberOfWordsToFile()
    {
        // Arrange
        _textGeneratorMock.Setup(t => t.Generate(It.IsAny<int>())).Returns("word1 word2 word3");

        var service = new FilesService(_wordsServiceMock.Object, _loggerMock.Object, _textGeneratorMock.Object);

        var totalWordsCount = 6;
        var wordsPerPart = 3;

        // Act
        service.WriteWords(filePath, totalWordsCount, wordsPerPart);

        // Assert
        var result = File.ReadAllText(filePath);
        Assert.Equal("word1 word2 word3word1 word2 word3", result);
    }

    [Fact]
    public void WriteWords_CorrectlySplitsWordsIntoParts()
    {
        // Arrange
        _textGeneratorMock.Setup(t => t.Generate(It.IsAny<int>())).Returns("word1 word2");

        var service = new FilesService(_wordsServiceMock.Object, _loggerMock.Object, _textGeneratorMock.Object);

        var totalWordsCount = 4;
        var wordsPerPart = 2;

        // Act
        service.WriteWords(filePath, totalWordsCount, wordsPerPart);

        // Assert
        var result = File.ReadAllText(filePath);
        Assert.Equal("word1 word2word1 word2", result);
    }

    [Fact]
    public void WriteWords_WorksWithMinimalWords()
    {
        // Arrange
        _textGeneratorMock.Setup(t => t.Generate(It.IsAny<int>())).Returns("word");

        var service = new FilesService(_wordsServiceMock.Object, _loggerMock.Object, _textGeneratorMock.Object);

        var totalWordsCount = 1;
        var wordsPerPart = 1;

        // Act
        service.WriteWords(filePath, totalWordsCount, wordsPerPart);

        // Assert
        var result = File.ReadAllText(filePath);
        Assert.Equal("word", result);
    }

    [Fact]
    public async Task ReadAsync_ReadsFileAndProcessesText()
    {
        // Arrange
        var service = new FilesService(_wordsServiceMock.Object, _loggerMock.Object, _textGeneratorMock.Object);

        var bufferSize = 50;
        File.WriteAllText(filePath, "word1 word2 word3");

        // Act
        await service.ReadAsync(filePath, bufferSize);

        // Assert
        _wordsServiceMock.Verify(w => w.WriteAsync(It.IsAny<Dictionary<string, int>>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task ReadAsync_ProcessesRemainingText()
    {
        // Arrange
        var service = new FilesService(_wordsServiceMock.Object, _loggerMock.Object, _textGeneratorMock.Object);

        var bufferSize = 15;
        File.WriteAllText(filePath, "word1 word2 word3word4");

        // Act
        await service.ReadAsync(filePath, bufferSize);

        // Assert
        _wordsServiceMock.Verify(w => w.WriteAsync(It.Is<Dictionary<string, int>>(d => d.ContainsKey("word3word4")), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ReadAsync_LogsErrorOnProcessingException()
    {
        // Arrange
        _wordsServiceMock.Setup(w => w.WriteAsync(It.IsAny<Dictionary<string, int>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Processing error"));

        var service = new FilesService(_wordsServiceMock.Object, _loggerMock.Object, _textGeneratorMock.Object);

        var bufferSize = 10;
        File.WriteAllText(filePath, "word1 word2");

        // Act
        await service.ReadAsync(filePath, bufferSize);

        // Assert
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Processing error"),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
            Times.Once);
    }

    [Fact]
    public void CheckFileSize_ThrowsExceptionForLargeFile()
    {
        // Arrange
        var service = new FilesService(_wordsServiceMock.Object, _loggerMock.Object, _textGeneratorMock.Object);
        var filePath = "largeFile.txt";
        File.WriteAllText(filePath, new string('a', 10000));

        // Act & Assert
        var exception = Record.Exception(() => service.CheckFileSize(filePath, 5000));
        Assert.IsType<ArgumentException>(exception);
    }

    [Fact]
    public void CheckFileSize_AllowsSmallFile()
    {
        // Arrange
        var service = new FilesService(_wordsServiceMock.Object, _loggerMock.Object, _textGeneratorMock.Object);
        var filePath = "smallFile.txt";
        File.WriteAllText(filePath, new string('a', 1000));

        // Act & Assert
        var exception = Record.Exception(() => service.CheckFileSize(filePath, 5000));
        Assert.Null(exception);
    }

    [Fact]
    public void CheckFileSize_ExitsApplicationForLargeFile()
    {
        // Arrange
        var service = new FilesService(_wordsServiceMock.Object, _loggerMock.Object, _textGeneratorMock.Object);
        var filePath = "largeFile.txt";
        File.WriteAllText(filePath, new string('a', 10000)); // Simulate large file

        // Act & Assert
        Assert.Throws<ArgumentException>(() => service.CheckFileSize(filePath, 5000));
    }

}
