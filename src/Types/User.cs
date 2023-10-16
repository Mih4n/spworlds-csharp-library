using spworlds;
using System.Text.Json.Nodes;
namespace spworlds.Types;

public class User
{
    public readonly string Name 
    public readonly string Uuid
    public readonly JsonNode profile

    private HttpClient client = new();

    public bool IsPlayer() => Name != null ? true : false;

    public User(string name)
    {
      Uuid = JsonNode.Parse(client.GetStringAsync($"https://api.mojang.com/users/profiles/minecraft/{name}"))["id"];
      profile = JsonNode.Parse(client.GetStringAsync($"https://sessionserver.mojang.com/session/minecraft/profile/{Uuid}"));
    }
    
    public string GetSkinPart(SkinPart skinPart, string size = "64")
    {
        return (string)$"https://visage.surgeplay.com/{skinPart}/{size}/{this.profile["profileId"]}"
    }
}
