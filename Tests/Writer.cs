namespace chatter.Tests;

public abstract class Test : ICommand
{
    protected readonly IContext ctx;

    public Test(IContext ctx){
        this.ctx = ctx;
    }
    public abstract void Execute();
}

public class WriterTest : Test
{
    public WriterTest(IContext ctx) : base(ctx)
    {
    }

    public override void Execute()
    {
        ctx.Enqueue(
            new StartCmdListener(ctx, new TryWriteStdIn(
                new EchoHandle(
                    new StdOutPrintLineAdapter()
                )
            ))
        );
    }
}

public class WriterInterruptableTest : Test
{
    public WriterInterruptableTest(IContext ctx) : base(ctx)
    {
    }

    public override void Execute()
    {
        var handler = new EchoInterruptableHandle(new StdOutPrintLineAdapter());
        var listener = new StartCmdListener(ctx, new TryWriteStdIn(handler));

        ctx.Enqueue(
            listener    
        );

        handler.Subscribe(listener);
    }
}

public class EchoHandle : IInputHandler
{
    protected readonly IPrinter printer;

    public EchoHandle(IPrinter printer)
    {
        this.printer = printer;
    }

    public virtual void Handle(string input)
    {
        printer.Print(input);
    }
}

public class EchoInterruptableHandle : EchoHandle, IInterruptor
{
    public EchoInterruptableHandle(IPrinter printer) : base(printer) {}
    private IInterruptable? interruptable;

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
        interruptable?.Update(this);
    }

    public void Subscribe(IInterruptable interruptable)
    {
        this.interruptable = interruptable;
    }
}
