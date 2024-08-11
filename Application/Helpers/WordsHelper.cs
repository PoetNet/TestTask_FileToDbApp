using System.Text.RegularExpressions;

namespace Application.Helpers;

public static class WordsHelper
{
    public static Dictionary<string, int> GetWordsWithCount(this string text)
    {
        Dictionary<string, int> wordsWithCount = new();

        string pattern = @"[^\w\s]";

        string[] words = Regex.Replace(text, pattern, "")
            .Split(' ')
            .Where(x => !string.IsNullOrEmpty(x) && x.Length >= 3 && x.Length <= 20)
            .ToArray();

        wordsWithCount = words
            .GroupBy(x => x)
            .ToDictionary(g => g.Key, g => g.Count());

        return wordsWithCount;
    }
}
