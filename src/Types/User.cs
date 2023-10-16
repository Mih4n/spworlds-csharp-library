using spworlds;
using System.Text.Json.Nodes;
namespace spworlds.Types;

public class User
{
    public readonly string Name 
    public readonly string Uuid
    public readonly JsonNode profile

    private HttpClient client = new();

    public bool IsPlayer() => this.Name != null ? true : false;

    public User()
    {
      Uuid = JsonNode.Parse(await client.GetStringAsync($"https://api.mojang.com/users/profiles/minecraft/{Name}"))["id"];
      profile = JsonNode.Parse(await client.GetStringAsync($"https://sessionserver.mojang.com/session/minecraft/profile/{Uuid}"));
    }
    
    public async Task<string> GetSkinPart(SkinPart skinPart, string size = "64")
    {
        return (string)$"https://visage.surgeplay.com/{skinPart}/{size}/{this.profile["profileId"]}"
    }
}
