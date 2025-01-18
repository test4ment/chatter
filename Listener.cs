public class StartCmdListener : ICommand, IInterruptable{
    private readonly Queue<ICommand> queue;
    private readonly ICommand to_listen;
    private readonly IInterruptable interruptor;
    private bool interrupted = false;

    public StartCmdListener(Queue<ICommand> queue, ICommand to_listen, IInterruptable interruptor)
    {
        this.queue = queue;
        this.to_listen = to_listen;
        this.interruptor = interruptor;
    }

    public void Execute()
    {
        queue.Enqueue(to_listen);
        queue.Enqueue(this);
    }

    public void Interrupt() => interruptor.Interrupt();

    internal class EnqueueWithCheckForInterruption : ICommand
    {
        private readonly bool interrupted;
        private readonly ICommand to_add;
        private readonly Queue<ICommand> queue;

        internal EnqueueWithCheckForInterruption(ref bool interrupted, ICommand to_add, Queue<ICommand> queue)
        {
            this.interrupted = interrupted;
            this.to_add = to_add;
            this.queue = queue;
        }

        public void Execute()
        {
            if(!interrupted){
                queue.Enqueue(to_add);
            }
        }
    }
}

public interface IInterruptable{
    public void Interrupt();
}
