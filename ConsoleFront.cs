public interface IPrinter {
    public void Print(string message);
}

public class StdOutPrintLineAdapter : IPrinter{
    public void Print(string message){
        Console.WriteLine(message);
    }
}

public class StdOutPrintAdapter : IPrinter{
    public void Print(string message){
        Console.Write(message);
    }
}

public class PrintMsg : ICommand{
    private readonly string message;
    private readonly IPrinter printer;

    public PrintMsg(string message, IPrinter printer){
        this.message = message;
        this.printer = printer;
    }
    public void Execute() {
        printer.Print(message);
    }
}

public class ColoredExecutionCmd : ICommand
{
    private ConsoleColor writeColor;
    private ICommand cmd;
    public ColoredExecutionCmd(ICommand cmd, ConsoleColor writeColor)
    {
        this.writeColor = writeColor;
        this.cmd = cmd;
    }

    public void Execute()
    {
        var color = Console.ForegroundColor;
        Console.ForegroundColor = writeColor;
        try{
            cmd.Execute();
        }
        catch{
            throw;
        }
        finally{
            Console.ForegroundColor = color;
        }
    }
}

public class ClearConsole : ICommand{
    public void Execute(){
        Console.Clear();
    }
}


// public class PrintLineMsgChat: ICommand
// {
//     private string mess;

//     public PrintLineMsgChat(string mess)
//     {
//         this.mess = mess;
//     }

//     public void Execute()
//     {
//         var name = IoC.Get<string>("Connected.Username");
//         var timestamp = TimeOnly.FromDateTime(DateTime.Now).ToShortTimeString();

//         new ColoredExecutionCmd(
//             new PrintMsg(timestamp + " "),
//             ConsoleColor.DarkGray
//         ).Execute();
//         new ColoredExecutionCmd(
//             new PrintMsg(name + ": "),
//             ConsoleColor.Yellow
//         ).Execute();
//         new PrintLineMsg(mess).Execute();
//     }
// }

public class ConsoleWriter : IWriter
{
    public string Input()
    {
        var input = StdInHandle.readMethod() ?? string.Empty;
        return input;
    }
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
