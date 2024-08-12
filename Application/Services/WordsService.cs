using Database;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Application.Services;

public interface IWordsService
{
    Task WriteAsync(Dictionary<string, int> wordsCounts, CancellationToken cancellationToken);
}

public class WordsService(FileToDbAppDbContext context, ILogger<IWordsService> logger) : IWordsService
{
    private readonly FileToDbAppDbContext _context = context;
    private readonly ILogger<IWordsService> _logger = logger;

    public async Task WriteAsync(Dictionary<string, int> wordsOccurrences, CancellationToken cancellationToken)
    {
        StringBuilder sb = new();

        foreach (var keyValuePair in wordsOccurrences)
        {
            sb.Append($"('{keyValuePair.Key}', {keyValuePair.Value}),");
        }

        if (sb.Length > 1)
        {
            sb.Remove(sb.Length - 1, 1);
        }

        string sbResult = sb.ToString();

        string queryTemplate =
            $@"
            USE WordsOccurrencesDb;

            BEGIN TRANSACTION;

            MERGE INTO Words AS target
            USING (VALUES
                {sbResult}) 
            AS source (Text, Occurrences)
            ON target.Text = source.Text
            WHEN MATCHED THEN 
                UPDATE SET target.Occurrences = target.Occurrences + source.Occurrences
            WHEN NOT MATCHED BY TARGET THEN
                INSERT (Text, Occurrences) VALUES (source.Text, source.Occurrences);

            COMMIT TRANSACTION;
            ";
        try
        {
            await _context.Database.ExecuteSqlRawAsync(queryTemplate, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw;
        }
    }
}
