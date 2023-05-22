using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using System.Text.Json.Nodes;

namespace spworlds;

public class SPWorlds
{
    private readonly HttpClient client;

    public SPWorlds(string id, string token)
    {
        client = new HttpClient();
        var BearerToken = $"{id}:{token}";
        string Base64BearerToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(BearerToken));

        client.BaseAddress = new Uri("https://spworlds.ru/api/public/");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Base64BearerToken);
    }

    private async Task<string> SendRequest(string endpoint, Boolean getResult = true, Dictionary<string, object>? body = null)
    {
        string respond;
        string jsonBody;
        
        if(body == null)
        {
            return respond = client.GetAsync(endpoint).Result.Content.ReadAsStringAsync().Result;
        }
        else 
        {
            jsonBody = JsonSerializer.Serialize(body);
            var payload = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            if(getResult)
                return respond = client.PostAsync(endpoint, payload).Result.Content.ReadAsStringAsync().Result;
            else
                await client.PostAsync(endpoint, payload);
        }

        return null;
    }

    public async Task<int> GetBalance() 
    {
        string respond = await SendRequest("card");

        var card = JsonObject.Parse(respond);
        var balance = card["balance"];

        return (int)balance;
    }

    public async Task CreatTransaction(string receiver, int amount, string comment)
    {
        var transitionInfo = new Dictionary<string, object>
        {
            { "receiver", receiver },
            { "amount", amount },
            { "comment", comment }
        };

        await SendRequest(endpoint: "transactions", body: transitionInfo);
    }

    public async Task<string> GetUser(string discordId)
    {
        var user = JsonObject.Parse(await SendRequest($"users/{discordId}"));
        var userName = user["username"];

        return (string)userName;
    }

    public async Task<string> InitPayment(int amount, string redirectUrl, string webhookUrl, string data)
    {
        var paymentInfo = new Dictionary<string, object>
        {
            { "amount", amount },
            { "redirectUrl", redirectUrl },
            { "webhookUrl", webhookUrl },
            { "data", data }
        };

        var payment = JsonObject.Parse(await SendRequest(endpoint: $"payment",body: paymentInfo));
        var url = payment["url"];

        return (string)url;
    }
}