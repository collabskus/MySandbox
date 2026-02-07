namespace MySandbox.ClassLibrary;

public interface ICanDoStuff
{
    Task DoStuffAsync();
    Task ImportLargeDatasetAsync(int count = 1_000_000);
}
