using Microsoft.Extensions.Options;
using MySandbox.ClassLibrary;

namespace MySandbox.Tests;

public class TestBusinessRulesOptions
{
    public static IOptions<BusinessRulesOptions> Create(
        int dataRetentionDays = 30,
        string batchOperationPrefix = "Batch-")
    {
        return Options.Create(new BusinessRulesOptions
        {
            DataRetentionDays = dataRetentionDays,
            BatchOperationPrefix = batchOperationPrefix
        });
    }
}
