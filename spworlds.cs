using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using System.Text.Json.Nodes;
using System.Security.Cryptography;
using System.Runtime.InteropServices;

using spworlds.Types;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;
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

    /// <summary>
    /// Validating wenhook from site
    /// </summary>
    /// <param name="requestBody">Body of request</param>
    /// <param name="base64Hash">X-Body-Hash</param>
    /// <returns></returns>
    public bool ValidateWebhook(string requestBody, string base64Hash)
    {
        byte[] requestData = Encoding.UTF8.GetBytes(requestBody);

        using (HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(token)))
        {
            byte[] hashBytes = hmac.ComputeHash(requestData);

            string computedHash = Convert.ToBase64String(hashBytes);

            return base64Hash.Equals(computedHash);
        }
    }

    private async Task<string> SendRequest(string endpoint, bool getResult = true, HttpMethod method = null, object body = null)
    {
        method ??= body == null ? HttpMethod.Get : HttpMethod.Post;
        HttpResponseMessage message;

        using (var requestMessage = new HttpRequestMessage(method, client.BaseAddress + endpoint))
        {
            requestMessage.Content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8, "application/json"
            );
            requestMessage.Headers.Authorization = client.DefaultRequestHeaders.Authorization;

            message = await client.SendAsync(requestMessage);
        }

        if (getResult)
            return await message.Content.ReadAsStringAsync();
        else
            return null;

    }


    /// <summary>
    /// Get card from spworlds
    /// </summary>
    /// <returns></returns>
    public async Task<Card> GetCard()
    => Deserialize.DeserializeObject<Card>(await SendRequest("card"));

    /// <summary>
    /// Create transaction
    /// </summary>
    /// <param name="receiver">receiver card</param>
    /// <param name="amount">amount of AR</param>
    /// <param name="comment">comment to transaction</param>
    /// <returns>balance of card</returns>
    public async Task<int> CreateTransaction(string receiver, int amount, string comment)
    {
        var transitionInfo = new Dictionary<string, object>
        {
            { "receiver", receiver },
            { "amount", amount },
            { "comment", comment }
        };

        var response = JsonObject.Parse(await SendRequest(endpoint: "transactions", body: transitionInfo, getResult: true));
        return (int)response["balance"];
    }

    /// <summary>
    /// Get user cards by nickname
    /// </summary>
    /// <param name="username">Username of player</param>
    /// <returns>Array of cards</returns>
    public async Task<UserCard[]> GetUserCardsAsync(string username)
    => Deserialize.DeserializeObject<UserCard[]>(await SendRequest($"accounts/{username}/cards"));



    /// <summary>
    /// Get user info from site
    /// </summary>
    /// <param name="discordId">Discord id of user</param>
    /// <returns></returns>
    public async Task<User> GetUser(string discordId)
    => Deserialize.DeserializeObject<User>(await SendRequest($"users/{discordId}"));
    
    /// <summary>
    /// Create payment url
    /// </summary>
    /// <param name="items">List of items</param>
    /// <param name="redirectUrl">User will be redirected to this url</param>
    /// <param name="webhookUrl">Webhook will be sended to this url</param>
    /// <param name="data">Data, returned with webhook</param>
    /// <returns></returns>
    public async Task<string> InitPayment(Item[] items, string redirectUrl, string webhookUrl, string data)
    {
        var paymentInfo = new Dictionary<string, object>
        {
            { "items", JsonSerializer.Serialize(items)},
            { "redirectUrl", redirectUrl },
            { "webhookUrl", webhookUrl },
            { "data", data }
        };

        var payment = JsonObject.Parse(await SendRequest(endpoint: $"payment", body: paymentInfo));
        var url = payment["url"];

        return (string)url;
    }

    /// <summary>
    /// Create payment url
    /// </summary>
    /// <param name="paymentData"></param>
    /// <returns></returns>
    public async Task<string> InitPayment(PaymentData paymentData)
    {
        var payment = JsonObject.Parse(await SendRequest(endpoint: $"payment", body: JsonSerializer.Serialize(paymentData)));
        return (string)payment["url"];
    }
    /// <summary>
    /// Setting up a webhook to card
    /// </summary>
    /// <param name="webhookUrl">Url of webhook</param>
    /// <returns></returns>
    public async Task<WebhookResponse> SetWebhook(string webhookUrl)
        => Deserialize.DeserializeObject<WebhookResponse>(
            await SendRequest(
                "card/webhook", 
                true, 
                HttpMethod.Put, 
                new Dictionary<string, string>() 
                { 
                    { "url", webhookUrl } 
                }
                )
            );
    public async Task<UserAccount> GetMeAsync() 
        => Deserialize.DeserializeObject<UserAccount>(await SendRequest("accounts/me"));
}

