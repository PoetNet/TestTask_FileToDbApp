using Application.Helpers;
using Application.Services;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

Env.TraversePath().Load(".env");
string connectionString = Env.GetString("MS_CONNECTION");
long maxFileSize = long.Parse(Env.GetString("MAX_FILE_SIZE"));
long totalWordsCount = long.Parse(Env.GetString("TOTAL_WORDS_COUNT"));
int wordsPerPart = int.Parse(Env.GetString("WORDS_PER_PART"));
long bufferSize = long.Parse(Env.GetString("BUFFER_SIZE"));

var serviceProvider = new ServiceCollection()
    .AddCustomDbContext(connectionString)
    .AddScoped<IWordsService, WordsService>()
    .AddScoped<IFilesService, FilesService>()
    .AddScoped<ITextGenerator, LoremGenerator>()
    .AddLogging()
    .BuildServiceProvider();

var filesService = serviceProvider.GetRequiredService<IFilesService>();

string parsedFileName = $"Lorem ipsum - {totalWordsCount} words.txt";
string filePath = Path.Combine("..", "..", "..", "Files to parse", parsedFileName);

filesService.WriteWords(filePath, totalWordsCount, wordsPerPart);
filesService.CheckFileSize(filePath, maxFileSize);
await filesService.ReadAsync(filePath, bufferSize);
