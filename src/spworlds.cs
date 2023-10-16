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
    

    // Полностью бесполезная функция, вебхук, возвращающийся от сайта по факту невозможно валидировать.
    private async Task<bool> ValidateWebHook(string webHook, string bodyHash)
    {
        // Если я правильно все понял, то вот
        // Конвертим из string в bytes body_hash
        byte[] body = Encoding.UTF8.GetBytes(bodyHash);
        // потом конвертим вебхук
        byte[] webhook = Encoding.UTF8.GetBytes(webHook);
        // создаем объект с токеном(тоже encoded в bytes) для сопостовления
        var key = new HMACSHA256(Encoding.UTF8.GetBytes(token));
        // Переводим в Base64
        string webhook64 = Convert.ToBase64String(key.ComputeHash(webhook));
        return webhook64.Equals(body);
        /**
         * Тот же код, но на Python:
            hmacData = hmac.new(token.encode('utf - 8'), webhook.encode('utf - 8'), sha256).digest()
            base64Data = b64encode(hmacData)
            return hmac.compare_digest(base64Data, bodyHash.encode('utf-8'))
        **/
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
        var userResponse = JsonObject.Parse(await SendRequest($"users/{discordId}"));
        var userName = userResponse["username"];
        User user = new() { Name = userName}
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
