using System.Reflection;

public class Program
{
    static public int Main(string[] notUsed)
    {
        try
        {
            Console.WriteLine("Dynamic Generic Method Test");
            Console.WriteLine();

            Foo1 foo1 = new();
            MethodInfo m1 = foo1.GetType().GetMethod("CallMethod1")!;

            Type[] types = [typeof(object), typeof(string), typeof(int)];

            foreach (Type t in types)
            {
                var runm1 = m1.MakeGenericMethod(t);
                runm1.Invoke(foo1, null);
                Console.WriteLine();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Test Failure: {e}");
            return 101;
        }

        return 100;
    }

    public static void TestMethod<T>()
    {
        Console.WriteLine($"Hello, World! {typeof(T).Name}");
    }
}


public class Foo1
{
    public void CallMethod1<T>()
    {
        Console.WriteLine($"Call Method, Generic '{typeof(T).Name}'");
    }
}
