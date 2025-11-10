using Borm.Properties;
using System.Reflection;

namespace Borm.Tests;

public sealed class StringsTest
{
    [Fact]
    public void Strings_DoesNotThrowException()
    {
        // Arrange
        List<(MethodInfo, object[])> methodArgs = GetStaticMethods(typeof(Strings));
        List<Exception?> exceptions = [];

        // Act
        foreach ((MethodInfo method, object[] args) in methodArgs)
        {
            exceptions.Add(Record.Exception(() => method.Invoke(null, args)));
        }

        // Assert
        foreach (Exception? ex in exceptions.Where(ex => ex != null))
        {
            Assert.Fail(ex!.Message);
        }
    }

    private static List<(MethodInfo, object[])> GetStaticMethods(Type type)
    {
        List<(MethodInfo, object[])> ret = [];
        MethodInfo[] methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public);
        for (int i = 0; i < methods.Length; i++)
        {
            MethodInfo current = methods[i];
            ParameterInfo[] parameters = current.GetParameters();
            object[] args = new object[parameters.Length];
            for (int j = 0; j < args.Length; j++)
            {
                args[j] = default;
            }

            ret.Add((current, args));
        }

        return ret;
    }
}
