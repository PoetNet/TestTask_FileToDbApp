using Bogus.DataSets;

namespace Application.Helpers;

public interface ITextGenerator
{
    string Generate(int wordsToGenerate); 
}
public class LoremGenerator : ITextGenerator
{
    private readonly Lorem _lorem = new Lorem();

    public string Generate(int wordsToGenerate) => string.Join(" ", _lorem.Words(wordsToGenerate));
}
