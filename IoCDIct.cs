public static class IoC{
    private static Dictionary<string, Func<object[], object>> Instance = new();

    public static T Get<T>(string key, params object[] args){
        return (T)Instance[key](args);
    }

    public static void Set(string key, Func<object[], object> obj){
        Instance[key] = obj;
    }
}