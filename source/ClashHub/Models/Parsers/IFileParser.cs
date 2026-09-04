using ClashHub.Domain.Entities;

namespace ClashHub.Models.Parsers;

public interface IFileParser<out T>
{
    IEnumerable<ClashTest> Parse(string filePath);
    string FormatName { get; }
}