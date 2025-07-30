using System.Diagnostics;
using System.Reflection;

/* 当前示例测试在 aot 模式下 MakeGenericMethod 的行为
MakeGenericMethod 支持引用类型(object, string...)，不支持值类型(struct, int...)

引用可用原因：
    引用类型(本质是指向堆上对象的指针)泛型代码可以复用
    引用类型（如 class）在泛型方法实例化时，底层生成的机器码是可以共享的。因为引用类型的内存布局和处理方式在泛型内部是一致的，AOT 编译器可以只生成一份代码供所有引用类型泛型调用复用

值类型不支持原因：
    值类型泛型代码必须为每种类型单独生成
    值类型（如 int、float、struct）在泛型方法实例化时，底层机器码需要针对每种值类型分别生成，不能共享。每种值类型的内存布局、操作方式都不同，AOT 编译器必须提前知道所有可能被用到的值类型，才能一一生成对应的代码

不做支持原因：
    如果强制为所有值类型生成泛型代码，会导致二进制文件体积巨大且编译时间变长
*/

public class Program
{
    static public int Main(string[] notUsed)
    {
        try
        {
            Console.WriteLine("Dynamic Generic Method Test");
            Console.WriteLine();

            Type[] types = [typeof(string), typeof(Record1), typeof(Struct1), typeof(int)];

            MakeAndRunStaticMethod(types);

            MakeAndRunInstanceMethod(types);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Test Failure: {e}");
            return 101;
        }

        return 100;
    }

    public static void CallMethod1<T>() => Console.WriteLine($"Call Method, Generic '{typeof(T).Name}'");

    public static void MakeAndRunStaticMethod(Type[] types)
    {
        MethodInfo m1 = typeof(Program).GetMethod("CallMethod1")!;
        foreach (Type t in types)
        {
            try
            {
                m1.MakeGenericMethod(t).Invoke(null, null);
            }
            catch (NotSupportedException)
            {
                Console.WriteLine($"创建失败: Program.{m1.Name}<{t.Name}>");
            }
        }
        Console.WriteLine();
    }

    public static void MakeAndRunInstanceMethod(Type[] types)
    {
        Foo1 foo1 = new();
        MethodInfo m1 = foo1.GetType().GetMethod("CallMethod1")!;
        foreach (Type t in types)
        {
            try
            {
                m1.MakeGenericMethod(t).Invoke(foo1, null);
            }
            catch (NotSupportedException)
            {
                Console.WriteLine($"创建失败: foo1.{m1.Name}<{t.Name}>");
            }
        }
        Console.WriteLine();
    }
}


public class Foo1
{
    public void CallMethod1<T>() => Console.WriteLine($"Call Method, Generic '{typeof(T).Name}'");
}

public struct Struct1(int MyProperty)
{
    public override string ToString() => $"Struct1: {MyProperty}";
}

public record Record1(int MyProperty)
{
    public override string ToString() => $"Record1: {MyProperty}";
}
