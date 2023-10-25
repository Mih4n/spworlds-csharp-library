using spworlds;
using System.Text.Json.Nodes;
namespace spworlds.Types;

public class User
{
    public string Name { get; }
    public string Uuid { get; }
    public JsonNode profile { get; }

    public bool IsPlayer() => Name != null ? true : false;

    public User(string name, string uuid, JsonNode profile)
    {
      Name = name;
      Uuid = uuid;
      this.profile = profile;
    }
    
    public static async Task<User> CreateUser(string name)
    {
      string uuid;
      JsonNode profile;
      using(HttpClient client = new())
      {
        uuid = (string)JsonNode.Parse(await client.GetStringAsync($"https://api.mojang.com/users/profiles/minecraft/{name}"))["id"];
        profile = JsonNode.Parse(await client.GetStringAsync($"https://sessionserver.mojang.com/session/minecraft/profile/{uuid}"));
      }
      User user = new(name, uuid, profile);
      return user;
    }

    public string GetSkinPart(SkinPart skinPart, string size = "64")
    {
        return (string)$"https://visage.surgeplay.com/{skinPart}/{size}/{this.profile["profileId"]}";
    }
}
