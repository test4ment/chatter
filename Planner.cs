public class Planner{
    public Dictionary<int, Queue<ICommand>> repeaters;

    public Planner(Queue<ICommand> queue, ushort freq = 1){
        if (freq == 0) throw new ArgumentException("Frequence cannot be zero!");
        
        repeaters = new(){
            {5 * freq, new Queue<ICommand>(1)},
            {4 * freq, new Queue<ICommand>(1)},
            {3 * freq, queue},
            {2 * freq, new Queue<ICommand>(1)},
            {freq, new Queue<ICommand>(1)}
        };
    }
    public void Start(){
        var q = IoC.Get<BlockingCollection<ICommand>>("MainQueue");
        
        while(IoC.Get<bool>("IsRunning")){
            foreach(var p_list in repeaters){
                var sub_q = p_list.Value;
                
                for(var _ = 0; _ < p_list.Key; _++){
                    if(sub_q.Count > 0){
                        q.Add(sub_q.Dequeue());
                    }
                }
            }
        }
    }
}

public enum Priority: byte{
    RealTime = 5,
    High = 4,
    Normal = 3,
    Low = 2,
    Lowest = 1
}

public class CmdWPriority: ICommand{
    internal Priority priority;
    private ICommand cmd;

    public CmdWPriority(ICommand cmd, Priority priority = Priority.Normal)
    {
        this.cmd = cmd;
        this.priority = priority;
    }

    public void Execute() => cmd.Execute();
}