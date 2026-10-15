using Castle.DynamicProxy;
using Core.CrossCuttingConcerns.Caching;
using Core.Utilities.Interceptors;
using Core.Utilities.IoC;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Aspects.Caching;

public class CacheAspect : MethodInterception
{
    private int _duration;
    private ICacheManager? _cacheManager;

    public CacheAspect(int duration = 60)
    {
        _duration = duration;
        _cacheManager = ServiceTool.ServiceProvider?.GetService<ICacheManager>();
    }

    public override void Intercept(IInvocation invocation)
    {
        _cacheManager ??= ServiceTool.ServiceProvider?.GetService<ICacheManager>();
        if (_cacheManager == null)
        {
            invocation.Proceed();
            return;
        }

        var reflectedType = invocation.Method.ReflectedType?.FullName ?? invocation.Method.DeclaringType?.FullName ?? "Unknown";
        var methodName = $"{reflectedType}.{invocation.Method.Name}";
        var arguments = invocation.Arguments.ToList();
        var key = $"{methodName}({string.Join(",", arguments.Select(x => x?.ToString() ?? "<Null>"))})";
        if (_cacheManager.IsAdd(key))
        {
            invocation.ReturnValue = _cacheManager.Get(key);
            return;
        }
        invocation.Proceed();
        if (invocation.ReturnValue != null)
        {
            _cacheManager.Add(key, invocation.ReturnValue, _duration);
        }
    }
}
