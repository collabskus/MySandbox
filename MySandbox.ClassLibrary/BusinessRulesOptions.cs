namespace MySandbox.ClassLibrary;

public class BusinessRulesOptions
{
    public int DataRetentionDays { get; set; } = 30;
    public string BatchOperationPrefix { get; set; } = "Batch-";
}
