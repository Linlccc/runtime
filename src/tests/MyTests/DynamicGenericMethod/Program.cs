using System.Reflection;

public class Program
{
    static public int Main(string[] notUsed)
    {
        try
        {
            Console.WriteLine("Dynamic Generic Method Test");
            Console.WriteLine();
            Type[] types = [typeof(string), typeof(Foo2), typeof(int)];

            // 静态方法
            // {
            //     MethodInfo m1 = typeof(Program).GetMethod("TestMethod")!;

            //     foreach (Type t in types)
            //     {
            //         var runm1 = m1.MakeGenericMethod(t);
            //         runm1.Invoke(null, null);
            //         Console.WriteLine();
            //     }
            // }

            // 实例方法调用
            // {
            //     Foo1 foo1 = new();

            //     Console.WriteLine($"Result: {foo1.CallMethod1<string>()}");
            //     Console.WriteLine();

            //     Console.WriteLine($"Result: {foo1.CallMethod1<Foo2>()}");
            //     Console.WriteLine();

            //     Console.WriteLine($"Result: {foo1.CallMethod1<int>()}");
            //     Console.WriteLine();
            // }

            // 实例方法
            {
                Foo1 foo1 = new();
                MethodInfo m1 = foo1.GetType().GetMethod("CallMethod1")!;

                foreach (Type t in types)
                {
                    var runm1 = m1.MakeGenericMethod(t);
                    var result = runm1.Invoke(foo1, null);
                    Console.WriteLine($"Result: {result}");
                    Console.WriteLine();
                }
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
    public T? CallMethod1<T>()
    {
        T? defaultT = default(T);
        Console.WriteLine($"Call Method, Generic '{typeof(T).Name}'");
        Console.WriteLine($"Default value type: {defaultT?.GetType().Name ?? "[null]"}");
        return defaultT;
    }
}

public struct Foo2
{
    public Foo2(int myProperty = 42, float myProperty2 = 3.14f)
    {
        MyProperty = myProperty;
        MyProperty2 = myProperty2;
    }

    public int MyProperty { get; set; }

    public float MyProperty2 { get; set; }

    public override string ToString()
    {
        return $"Foo2: MyProperty = {MyProperty}, MyProperty2 = {MyProperty2}";
    }
}
