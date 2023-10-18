using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using System.Text.Json.Nodes;
using System.Security.Cryptography;
using System.Runtime.InteropServices;

using spworlds.Types;
namespace spworlds;

public class SPWorlds
{
    private readonly HttpClient client;
    private string token;

    public SPWorlds(string id, string token)
    {
        client = new HttpClient();
        var BearerToken = $"{id}:{token}";
        this.token = token;
        string Base64BearerToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(BearerToken));

        client.BaseAddress = new Uri("https://spworlds.ru/api/public/");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Base64BearerToken);
    }

    private async Task<bool> ValidateWebHook(string webHook, string bodyHash)
    {
        byte[] body = Encoding.UTF8.GetBytes(bodyHash);
        byte[] webhook = Encoding.UTF8.GetBytes(webHook);
        var key = new HMACSHA256(Encoding.UTF8.GetBytes(token));
        string webhook64 = Convert.ToBase64String(key.ComputeHash(webhook));
        return webhook64.Equals(body);
    }
    
    private async Task<string> SendRequest(string endpoint, Boolean getResult = true, Dictionary<string, object>? body = null)
    {
        string respond;
        string jsonBody;
        
        if (body == null)
        {
            return respond = client.GetAsync(endpoint).Result.Content.ReadAsStringAsync().Result;
        }
        else
        {
            jsonBody = JsonSerializer.Serialize(body);
            var payload = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            if (getResult)
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

    public async Task CreateTransaction(string receiver, int amount, string comment)
    {
        var transitionInfo = new Dictionary<string, object>
        {
            { "receiver", receiver },
            { "amount", amount },
            { "comment", comment }
        };

        await SendRequest(endpoint: "transactions", body: transitionInfo);
    }

    public async Task<User> GetUser(string discordId)
    {
        string userName = (string)JsonObject.Parse(await SendRequest($"users/{discordId}"))["username"];
        User user = await User.CreateUser(userName);
        return (User)user;
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

        var payment = JsonObject.Parse(await SendRequest(endpoint: $"payment", body: paymentInfo));
        var url = payment["url"];

        return (string)url;
    }

    public async Task<string> InitPayment(PaymentData paymentData)
    {
        var paymentInfo = new Dictionary<string, object>
        {
            { "amount", paymentData.Amount },
            { "redirectUrl", paymentData.RedirectUrl },
            { "webhookUrl", paymentData.WebHookUrl },
            { "data", paymentData.Data }
        };

        var payment = JsonObject.Parse(await SendRequest(endpoint: $"payment", body: paymentInfo));
        var url = payment["url"];

        return (string)url;
    }
}

