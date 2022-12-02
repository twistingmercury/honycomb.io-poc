using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace TM.Decorators.Abstractions;

public abstract class BaseUniversalWrapperAspect
{
    private static readonly ConcurrentDictionary<(MethodBase, Type), Lazy<Handler>> DelegateCache = new();

    private static readonly MethodInfo AsyncGenericHandler =
        typeof(BaseUniversalWrapperAttribute).GetMethod(nameof(BaseUniversalWrapperAttribute.WrapAsync),
            BindingFlags.NonPublic | BindingFlags.Instance)!;

    private static readonly MethodInfo SyncGenericHandler =
        typeof(BaseUniversalWrapperAttribute).GetMethod(nameof(BaseUniversalWrapperAttribute.WrapSync),
            BindingFlags.NonPublic | BindingFlags.Instance)!;

    private static readonly Type VoidTaskResult = Type.GetType("System.Threading.Tasks.VoidTaskResult")!;

    protected object BaseHandle(
        object instance,
        Type type,
        MethodBase method,
        Func<object[], object> target,
        string name,
        object[] args,
        Type returnType,
        Attribute[] triggers)
    {
        var eventArgs = new AspectEventArgs
        {
            Instance = instance,
            Type = type,
            Method = method,
            Name = name,
            Args = args,
            ReturnType = returnType,
            Triggers = triggers
        };

        var wrappers = triggers.OfType<BaseUniversalWrapperAttribute>().ToArray();

        var handler = GetMethodHandler(method, returnType, wrappers);
        return handler(target, args, eventArgs);
    }

    private Handler CreateMethodHandler(Type returnType, IReadOnlyList<BaseUniversalWrapperAttribute> wrappers)
    {
        var targetParam = Expression.Parameter(typeof(Func<object[], object>), "orig");
        var eventArgsParam = Expression.Parameter(typeof(AspectEventArgs), "event");

        MethodInfo wrapperMethod;

        if (typeof(Task).IsAssignableFrom(returnType))
        {
            var taskType = returnType.IsConstructedGenericType ? returnType.GenericTypeArguments[0] : VoidTaskResult;
            returnType = typeof(Task<>).MakeGenericType(taskType);

            wrapperMethod = AsyncGenericHandler.MakeGenericMethod(taskType);
        }
        else
        {
            if (returnType == typeof(void))
                returnType = typeof(object);

            wrapperMethod = SyncGenericHandler.MakeGenericMethod(returnType);
        }

        var converArgs = Expression.Parameter(typeof(object[]), "args");
        var next = Expression.Lambda(Expression.Convert(Expression.Invoke(targetParam, converArgs), returnType),
            converArgs);

        foreach (var wrapper in wrappers)
        {
            var argsParam = Expression.Parameter(typeof(object[]), "args");
            next = Expression.Lambda(
                Expression.Call(Expression.Constant(wrapper), wrapperMethod, next, argsParam, eventArgsParam),
                argsParam);
        }

        var origArgs = Expression.Parameter(typeof(object[]), "orig_args");
        var handler = Expression.Lambda<Handler>(Expression.Convert(Expression.Invoke(next, origArgs), typeof(object)),
            targetParam, origArgs, eventArgsParam);

        var handlerCompiled = handler.Compile();

        return handlerCompiled;
    }

    private Handler GetMethodHandler(MethodBase method, Type returnType,
        IReadOnlyList<BaseUniversalWrapperAttribute> wrappers)
    {
        var lazyHandler = DelegateCache.GetOrAdd((method, returnType),
            _ => new Lazy<Handler>(() => CreateMethodHandler(returnType, wrappers)));
        return lazyHandler.Value;
    }

    private delegate object Method(object[] args);

    private delegate object Wrapper(Func<object[], object> target, object[] args);

    private delegate object Handler(Func<object[], object> next, object[] args, AspectEventArgs eventArgs);
}