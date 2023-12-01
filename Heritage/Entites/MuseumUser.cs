namespace Heritage.Entites;

public class MuseumUser
{
    public string UserId { get; set; }
    public string UserName { get; set; }
    public List<string> Roles { get; set; } = new List<string>();

}
