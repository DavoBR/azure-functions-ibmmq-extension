using IBM.XMS;

namespace Azure.WebJobs.Extensions.IBMMQ;

public abstract class MQMessage
{
    public string? Format { get; set; }
    
    public int? Encoding { get; set; }
    
    public int? CharacterSet { get; set; }
    
    public string? MessageId { get; set; }
    
    public string? CorrelationId { get; set; }
    
    public string? ReplyTo { get; set; }
    
    public string? ApplicationId { get; set; }
    
    public DateTime? Timestamp { get; set; }
    
    public bool Redelivered { get; set; }
    
    public string? SourceQueue { get; set; }
    
    //https://www.ibm.com/docs/en/ibm-mq/8.0?topic=applications-creating-destinations-in-jms-application

    public DeliveryMode? DeliveryMode { get; set; }

    public int? Priority { get; set; }
    
    public TimeSpan? Expiration { get; set; }
}