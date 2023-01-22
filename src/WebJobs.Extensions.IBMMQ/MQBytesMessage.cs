namespace Azure.WebJobs.Extensions.IBMMQ;

public class MQBytesMessage : MQMessage
{
    public byte[]? Bytes { get; set; }

    public MQBytesMessage() {}
    
    public MQBytesMessage(byte[]? bytes) => Bytes = bytes;
}