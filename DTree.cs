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