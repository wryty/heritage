namespace HeritageApi.Data.Entities;

public class Exhibit
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }

    public byte[]? Image { get; set; }
    public string? ImageFileName { get; set; }

    //public string? ImagePath { get; set; } 
}