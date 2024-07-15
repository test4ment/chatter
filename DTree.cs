using System.Runtime.ExceptionServices;

public class DecisionTree<K, V> where K: notnull where V: notnull{
    private IDictionary<K, object> holder;

    public DecisionTree(){
        holder = new Dictionary<K, object>();
    }

    public V Get(K[] key){
        try{
            return Get(key[1..]);
        }
        catch (IndexOutOfRangeException){
            return (V)holder[key[0]];
        }
    }

    public void Set(K[] key, V value){
        IDictionary<K, object> currHolder = holder;

        foreach(K k in key){
            currHolder.TryAdd(k, new Dictionary<K, object>());
            currHolder = (IDictionary<K, object>)currHolder[k];
        }

        currHolder[key[^1]] = value;
    }
}

public static class ExceptionHandler{
    private static Dictionary<Type, Dictionary<Type, Action<ICommand, Exception>>> strategiesCmd = new();
    private static Dictionary<Type, Action<ICommand, Exception>> defaultCmdsHandles = new();
    private static Dictionary<Type, Action<ICommand, Exception>> defaultExepHandles = new(); 
    private static Action<ICommand, Exception> defaultHandler = (ICommand cmd, Exception ex) => {
        // ex.Data["cmd"] = cmd;
        ExceptionDispatchInfo.Capture(ex).Throw();
    }; 

    public static void SetHandler(ICommand cmd, Exception exception, Action<ICommand, Exception> strat){
        SetHandler(cmd.GetType(), exception.GetType(), strat);
    }

    public static void SetHandler(Type cmd, Type exception, Action<ICommand, Exception> strat){
        try{
            strategiesCmd[cmd][exception] = strat;
        }
        catch(KeyNotFoundException){
            strategiesCmd[cmd] = new Dictionary<Type, Action<ICommand, Exception>>(){
                {exception, strat}
            };
        }
    }

    public static void SetDefaultHandlerByInstance(ICommand cmd, Action<ICommand, Exception> strat){
        SetDefaultCmdHandler(cmd.GetType(), strat);
    }

    public static void SetDefaultHandlerByInstance(Exception ex, Action<ICommand, Exception> strat){
        SetDefaultExHandler(ex.GetType(), strat);
    }

    public static void SetDefaultCmdHandler(Type cmd, Action<ICommand, Exception> strat){
        defaultCmdsHandles[cmd] = strat;
    }

    public static void SetDefaultExHandler(Type ex, Action<ICommand, Exception> strat){
        defaultExepHandles[ex] = strat;
    }

    public static void SetDefaultHandler(Action<ICommand, Exception> strat){
        defaultHandler = strat;
    }

    public static Action<ICommand, Exception> GetHandler(ICommand cmd, Exception ex){
        return GetHandler(cmd.GetType(), ex.GetType());
    }

    public static Action<ICommand, Exception> GetHandler(Type cmdType, Type exType){
        Action<ICommand, Exception>? outvar;

        if(strategiesCmd.ContainsKey(cmdType)){
            if(strategiesCmd[cmdType].TryGetValue(exType, out outvar)) return outvar;
        }
        if(defaultCmdsHandles.TryGetValue(cmdType, out outvar) || defaultExepHandles.TryGetValue(exType, out outvar)){ // inconsistent?
            return outvar;
        }
        return defaultHandler;
    }
}