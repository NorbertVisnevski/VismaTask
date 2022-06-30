using System.Text.Json;
using System.Text.Json.Serialization;
using VismaTask.Models;

namespace VismaTask.Data;

public class DbContext
{
    private readonly string _filename;
    JsonSerializerOptions _options = new JsonSerializerOptions
    {
        WriteIndented = true,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };
    public List<Meeting> Meetings { get; set; }
    public DbContext(string filename)
    {
        _filename = filename;
        try
        {
            string json = File.ReadAllText(filename);
            Meetings = JsonSerializer.Deserialize<List<Meeting>>(json, _options) ?? new List<Meeting>();
        }
        catch(Exception)
        {
            Meetings = new List<Meeting>();
        }
    }
    public void SaveChanges()
    {
        using FileStream createStream = File.Create(_filename);
        JsonSerializer.Serialize(createStream, Meetings, _options);
        createStream.Dispose();
    }
}
