public interface IMessage{
    public string Text {get;}
    // other data...
    // timestamp?
    // sender?
    public void PrintWith(IPrinter printer);
}

[Obsolete("IMessage awaiting impl...")]
public class WriterWithYou : ICommand
{
    private readonly string message;
    private readonly IPrinter printer;

    public WriterWithYou(string message, IPrinter printer)
    {
        this.message = message;
        this.printer = printer;
    }

    public void Execute()
    {
        printer.Print("you: " + message);
    }
}
