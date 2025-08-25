namespace Azure.WebJobs.Extensions.IBMMQ;

public class MQTextMessage : MQMessage
{
    public string? Text { get; set; }

    public MQTextMessage() { }

    public MQTextMessage(string? text) => Text = text;
}