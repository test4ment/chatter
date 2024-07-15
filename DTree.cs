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
    private static readonly Dictionary<Type?, Dictionary<Type?, Action<Type, Type>>?> strategiesCmd = new();
    static ExceptionHandler(){
        throw new NotImplementedException();
    }

    public static void AddStrategy(Type cmd, Type exception, Action<Type, Type> strat){ // remake with nulls in dictionary!!
        try{
            strategiesCmd[cmd][exception] = strat;
        }
        catch(KeyNotFoundException){
            strategiesCmd[cmd] = new Dictionary<Type, Action<Type, Type>>(){
                {exception, strat}
            };
        }
    }
    public static void Handle(ICommand cmd, Exception exception){
        Type cmdType = cmd.GetType();
        Type exType = exception.GetType();

        Handle(cmdType, exType);
    }

    public static void Handle(Type cmdType, Type exType){

        // Dictionary<Type, Action<Type, Type>>? strats;
        if(strategiesCmd.ContainsKey(cmdType)){
            try{
                strategiesCmd[cmdType][exType](cmdType, exType);
            }
            catch (KeyNotFoundException){
                HandleUnknownException(cmdType, exType);
            }
        }
        else{
            HandleUnknownCmd(cmdType, exType);
        }
    }

    private static void HandleUnknownCmd(Type cmd, Type exception){

    }

    private static void HandleUnknownException(Type cmd, Type exception){

    }

    private static void DefaultHandle(){

    }
}