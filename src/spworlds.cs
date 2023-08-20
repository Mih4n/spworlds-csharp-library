using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using System.Text.Json.Nodes;
using System.Security.Cryptography;
using System.Runtime.InteropServices;

namespace spworlds;

public class SPWorlds
{
    private readonly HttpClient client;

    public SPWorlds(string id, string token)
    {
        client = new HttpClient();
        var BearerToken = $"{id}:{token}";
        var token = token
        string Base64BearerToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(BearerToken));

        client.BaseAddress = new Uri("https://spworlds.ru/api/public/");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Base64BearerToken);
    }

    private async Task<bool> ValidateWebhook(string webhook, string body_hash)
    {
        // Если я правильно все понял, то вот
        // Конвертим из string в bytes body_hash
        byte[] body = Encoding.UTF8.GetBytes(body_hash);
        // потом конвертим вебхук
        byte[] webhook = Encoding.UTF8.GetBytes(webhook);
        // создаем объект с токеном(тоже encoded в bytes) для сопостовления
        var key = new HMACSHA256(Encoding.UTF8.GetBytes(token));
        // Переводим в Base64
        string webhook_64 = Convert.ToBase64String(key.ComputeHash(webhook));
        return webhook_64.Equals(body);
        /**
         * Тот же код, но на Python:
            hmac_data = hmac.new(token.encode('utf - 8'), webhook.encode('utf - 8'), sha256).digest()
            base64_data = b64encode(hmac_data)
            return hmac.compare_digest(base64_data, body_hash.encode('utf-8'))
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

        var payment = JsonObject.Parse(await SendRequest(endpoint: $"payment", body: paymentInfo));
        var url = payment["url"];

        return (string)url;
    }
}
