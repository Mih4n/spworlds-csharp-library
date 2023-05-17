using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using System.Text.Json.Nodes;

namespace spworlds;

public class SpWorlds
{
    private readonly HttpClient _client;
    JsonSerializerOptions options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };


    public SpWorlds(string id, string token)
    {
        var StringToken = $"{id}:{token}";
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(StringToken);
        var Base64Token = Convert.ToBase64String(bytes);

        _client = new HttpClient();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Base64Token);
    }

    private string SendRequest<T>(string path, T body, Boolean getResult)
    {
        string respond;
        string method = (body == null) ? "GET" : "POST"; 
        string jsonBody = null;
        
        var endpoint = new Uri($"https://spworlds.ru/api/public/{path}");
        if(method == "GET")
        {
            return respond = _client.GetAsync(endpoint).Result.Content.ReadAsStringAsync().Result;
        }
        else 
        {
            jsonBody = JsonSerializer.Serialize<T>(body);
            var payload = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            if(getResult)
                return respond = _client.PostAsync(endpoint, payload).Result.Content.ReadAsStringAsync().Result;
            else
                _client.PostAsync(endpoint, payload);
        }

        return jsonBody;
    }

    public int GetBalance() 
    {
        string respond = SendRequest<object>("card", null, true);

        var card = JsonObject.Parse(respond);
        var balance = card["balance"];
        return (int)balance;
    }

    public void CreatTransition(string reciverCardId, int amount, string comment)
    {
        TransitionInfo transitionInfo = new TransitionInfo();
            transitionInfo.amount = amount;
            transitionInfo.comment = comment;
            transitionInfo.receiver = reciverCardId;

        SendRequest("transactions", transitionInfo, false);
    }

    public string GetUser(string discordId)
    {
        var user = JsonObject.Parse(SendRequest<object>($"users/{discordId}", null, true));
        var userName = user["username"];
        return (string)userName;
    }

    public string InitPayment(int amount, string redirectUrl, string webhookUrl, string data)
    {
        PaymentInfo paymentInfo = new PaymentInfo();
            paymentInfo.amount = amount;
            paymentInfo.redirectUrl = redirectUrl;
            paymentInfo.webhookUrl = webhookUrl;
            paymentInfo.data = data;

        var jsonBody = JsonSerializer.Serialize(paymentInfo);

        Console.WriteLine(jsonBody);

        var payment = JsonObject.Parse(SendRequest($"payment", paymentInfo, true));
        var url = payment["url"];
        return (string) url;
    }

    class PaymentInfo
    {
        public int amount { get; set; }
        public string redirectUrl { get; set; }
        public string webhookUrl { get; set; }
        public string data { get; set; }
    }

    class TransitionInfo
    {
        public string receiver { get; set; }
        public int amount { get; set; }
        public string comment { get; set; }
    }
}
