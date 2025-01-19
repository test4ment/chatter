public interface IInterruptable{
    public void Update(IInterruptor interruptable);
}

public interface IInterruptor{
    public void Subscribe(IInterruptable interruptor);
    public void Interrupt();
}

public class StartCmdListener : ICommand, IInterruptable{
    private readonly IContext ctx;
    private readonly ICommand to_listen;
    private readonly CheckForInterruptionCmd repeat_cmd;
    protected bool is_interrupted = false;

    public StartCmdListener(IContext ctx, ICommand to_listen)
    {
        this.ctx = ctx;
        this.to_listen = to_listen;
        this.repeat_cmd = new CheckForInterruptionCmd(this);
    }

    public void Execute()
    {
        ctx.Enqueue(to_listen);
        ctx.Enqueue(this.repeat_cmd);
    }

    protected void EnqueueMyself(){
        ctx.Enqueue(this);
    }

    public void Update(IInterruptor interruptable)
    {
        is_interrupted = true;
    }

    private class CheckForInterruptionCmd : ICommand
    {
        private readonly StartCmdListener inst;

        internal CheckForInterruptionCmd(StartCmdListener inst)
        {
            this.inst = inst;
        }

        public void Execute(){
            if(!inst.is_interrupted){
                inst.EnqueueMyself();
            }
        }
    }
}
