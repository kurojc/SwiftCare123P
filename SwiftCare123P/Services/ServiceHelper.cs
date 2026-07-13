namespace SwiftCare123P.Services;

public static class ServiceHelper
{
    public static T? GetService<T>() where T : class
    {
        if (Application.Current is App app)
        {
            var serviceProvider = app.Handler?.MauiContext?.Services;
            return serviceProvider?.GetService(typeof(T)) as T;
        }
        return null;
    }
}