public class Program
{
    static public int Main(string[] notUsed)
    {
        try
        {
            TestMethod();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Test Failure: {e}");
            return 101;
        }

        return 100;
    }

    public static void TestMethod()
    {

        Console.WriteLine("Hello, World!");
    }
}
