using Application.Helpers;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Application.Services;

public interface IFilesService
{
    void WriteWords(string filePath, long totalWordsCount, int wordsPerPart);
    Task ReadAsync(string filePath, long bufferSize);
    void CheckFileSize(string filePath, long maxFileSize);
}

public class FilesService(IWordsService wordsService, ILogger<IFilesService> logger, ITextGenerator textGenerator) : IFilesService
{
    private readonly IWordsService _wordsService = wordsService;
    private readonly ILogger<IFilesService> _logger = logger;
    private readonly ITextGenerator _textGenerator = textGenerator;

    public void WriteWords(string filePath, long totalWordsCount, int wordsPerPart)
    {
        using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
        using (var streamWriter = new StreamWriter(fileStream, Encoding.UTF8))
        {
            int totalParts = (int)Math.Ceiling((double)totalWordsCount / wordsPerPart);

            for (int part = 0; part < totalParts; part++)
            {
                int wordsToGenerate = (int)Math.Min(wordsPerPart, totalWordsCount - part * wordsPerPart);

                string words = _textGenerator.Generate(wordsToGenerate);
                streamWriter.Write(words);

                Console.WriteLine($"Part {part + 1}/{totalParts} written with {wordsToGenerate} words.");
            }
        }
    }

    public async Task ReadAsync(string filePath, long bufferSize)
    {
        using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
        using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
        {
            char[] buffer = new char[bufferSize];
            int charsRead;
            string leftover = string.Empty;

            int partsCounter = 0;

            while ((charsRead = streamReader.Read(buffer, 0, buffer.Length)) > 0)
            {
                string textToParse = leftover + new string(buffer, 0, charsRead);

                // Находим последний пробел в прочитанном тексте
                int lastSpaceIndex = textToParse.LastIndexOf(' ');

                if (lastSpaceIndex != -1)
                {
                    // Разбиваем текст на две части: до последнего пробела и оставшуюся часть
                    string completePart = textToParse.Substring(0, lastSpaceIndex);
                    leftover = textToParse.Substring(lastSpaceIndex + 1);

                    // Обрабатываем полные слова
                    if (!await TryProcessTextAsync(completePart))
                    {
                        return;
                    }
                    _logger.LogInformation($"Processed {partsCounter} part of file");
                    Console.WriteLine($"Processed {partsCounter++} part of file");
                }
                else
                {
                    // Если пробела не найдено, сохраняем всё в leftover
                    leftover = textToParse;
                }
            }

            // Обрабатываем оставшийся текст
            if (!string.IsNullOrEmpty(leftover))
            {
                if (!await TryProcessTextAsync(leftover))
                {
                    return;
                }
                _logger.LogInformation($"Processed {partsCounter} part of file");
                Console.WriteLine($"Processed {partsCounter++} part of file");
            }
        }
    }

    private async Task<bool> TryProcessTextAsync(string text)
    {
        try
        {
            await ProcessText(text);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            Console.WriteLine(ex.Message);
            return false;
        }
    }

    private async Task ProcessText(string text)
    {
        var wordsOccurrences = text.GetWordsWithCount();
        await _wordsService.WriteAsync(wordsOccurrences, default);
    }

    public void CheckFileSize(string filePath, long maxFileSize)
    {
        FileInfo fileInfo = new(filePath);
        var fileSize = fileInfo.Length;

        if (fileSize > maxFileSize)
        {
            _logger.LogError($"File with size {fileSize} is not processed");
            Console.WriteLine($"It's too big file, try another one. Less than {maxFileSize} bytes...");

            Environment.Exit(1);
        }
    }
}
