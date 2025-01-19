
namespace chatter.Tests;

public class PrinterTest : Test
{
    public PrinterTest(IContext ctx) : base(ctx)
    {
    }

    public override void Execute()
    {
        ctx.Enqueue(
            new PrintMsg("Hello world", new StdOutPrintLineAdapter())
        );
        ctx.Enqueue(
            new PrintMsg("Hello world new line", new StdOutPrintLineAdapter())
        );
        ctx.Enqueue(new StopApp(ctx));
    }
}