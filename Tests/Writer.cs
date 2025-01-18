namespace chatter.Tests;

public class WriterTest : ICommand
{
    private readonly Queue<ICommand> queue;

    public WriterTest(Queue<ICommand> queue)
    {
        this.queue = queue;
    }

    public void Execute()
    {
        queue.Enqueue(
            new StartCmdListener(queue, new TryWriteStdIn(
                new EchoHandle(
                    new StdOutPrintLineAdapter()
                )
            ))
        );
    }
}

public class WriterInterruptableTest : ICommand
{
    private readonly Queue<ICommand> queue;

    public WriterInterruptableTest(Queue<ICommand> queue)
    {
        this.queue = queue;
    }

    public void Execute()
    {
        queue.Enqueue(
            new StartCmdListener(queue, new TryWriteStdIn(
                new EchoInterruptableHandle(
                    new StdOutPrintLineAdapter()
                )
            ))
        );
    }
}

public class EchoHandle : IInputHandler
{
    internal readonly IPrinter printer;

    public EchoHandle(IPrinter printer)
    {
        this.printer = printer;
    }

    public virtual void Handle(string input)
    {
        printer.Print(input);
    }
}

public class EchoInterruptableHandle : EchoHandle, IInterruptable
{
    public EchoInterruptableHandle(IPrinter printer) : base(printer) {}

    public override void Handle(string input)
    {
        if(input == string.Empty){
            printer.Print("Provide a string!");
        }
        else{
            Interrupt();
            printer.Print(input);
        }
    }

    public void Interrupt()
    {
        throw new NotImplementedException();
    }
}
