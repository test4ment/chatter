public interface IWriter{
    public string Input();
}

public class ConsoleWriter : IWriter
{
    public string Input()
    {
        var input = StdInHandle.readMethod();
        input ??= "";
        return input;
    }
}

public interface IInputHandler{
    public void Handle(string input);
}

public class TryWriteStdIn : ICommand
{
    private readonly IInputHandler handler;
    private readonly ConsoleWriter writer;

    public TryWriteStdIn(IInputHandler handler)
    {
        this.handler = handler;
        this.writer = new ConsoleWriter();
    }

    public void Execute()
    {
        if (Console.KeyAvailable){
            handler.Handle(
                writer.Input()
            );
        }
    }
}

[Obsolete("Move into start context")]
public static class StdInHandle{
    public delegate string? ReadLine();
    public static ReadLine readMethod {get; private set;}

    static StdInHandle(){
        if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)){
            readMethod = UnicodeReader.ReadLine;
        }
        else if(RuntimeInformation.IsOSPlatform(OSPlatform.Linux)){
            readMethod = Console.ReadLine;
        }
        else throw new Exception($"{RuntimeInformation.OSDescription} is not supported");
    }
}
