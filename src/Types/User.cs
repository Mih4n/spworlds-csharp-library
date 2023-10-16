using spworlds;
using System.Text.Json.Nodes;
namespace spworlds.Types;

public class User
{
    private string Name { get; }
    private HttpClient client = new();
    private string nonSerializedUuid = await client.GetStringAsync($"https://api.mojang.com/users/profiles/minecraft/{Name}");
    private JsonNode uuid = JsonNode.Parse(nonSerializedUuid);
    private string Uuid { get; } = uuid["id"]
    string nonSerializedProfileId = await client.GetStringAsync($"https://sessionserver.mojang.com/session/minecraft/profile/{Uuid}");
    private JsonNode profile = JsonNode.Parse(nonSerializedProfileId);
    
    public async Task<string> GetSkinPart(SkinPart skinPart, string size)
    {
        return (string)$"https://visage.surgeplay.com/{skinPart}/{size}/{this.profile["profileId"]}"
    }
    public string GetName() => this.Name;
    public string GetUuid() => this.Uuid;
    public JsonNode GetProfile() => return this.profile;
    public bool IsPlayer() => this.Name != null : false;
}