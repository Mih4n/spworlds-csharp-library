using spworlds;
using System.Text.Json.Nodes;
namespace spworlds.Types;

public class User
{
    public string Name { get; }
    public string Uuid { get; }

    public bool IsPlayer() => Name != null ? true : false;

    public User(string name, string uuid)
    {
      Name = name;
      Uuid = uuid;
    }
    
    public static async Task<User> CreateUser(string name)
    {
      string uuid;
      using(HttpClient client = new())
      {
        uuid = (string)JsonNode.Parse(await client.GetStringAsync($"https://api.mojang.com/users/profiles/minecraft/{name}"))["id"];
      }
      User user = new(name, uuid);
      return user;
    }

    public string GetSkinPart(SkinPart skinPart, string size = "64")
    {
        return (string)$"https://visage.surgeplay.com/{skinPart}/{size}/{this.Uuid}";
    }
}
