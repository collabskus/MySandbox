using Microsoft.Extensions.Options;
using MySandbox.ClassLibrary;

namespace MySandbox.Tests;

public class TestSeedDataOptions
{
    public static IOptions<SeedDataOptions> Create(
        List<string>? products = null)
    {
        return Options.Create(new SeedDataOptions
        {
            Products = products ?? ["Alpha", "Beta", "Gamma"]
        });
    }
}
