namespace spworlds.Types;

public class Item
{
    public string Name { get; set; }
    public int Count { get; set; }
    public int Price { get; set; }
    public string? Comment { get; set; } = null;
}
public class PaymentData
{
    public Item[] Items;
    public string RedirectUrl;
    public string WebHookUrl;
    public string Data;
}