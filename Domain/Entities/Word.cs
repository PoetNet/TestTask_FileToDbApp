namespace Domain.Entities;

public class Word(string text, long occurrences)
{
    public int Id { get; private set; }
    public string Text { get; set; } = text;
    public long Occurrences { get; set; } = occurrences;
}
