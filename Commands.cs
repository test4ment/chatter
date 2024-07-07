public interface ICommand {
    public void Execute();
}

public class PrintMsg : ICommand{
    private string message;
    public PrintMsg(string message){
        this.message = message;
    }
    public void Execute() {
        IoC.Get<IPrintable>("stdout writer").Print(message);
    }
}

public class StopApp : ICommand{
    public void Execute(){
        IoC.Set("IsRunning", (object[] args) => {return false;});
    }
}

public class ClearConsole : ICommand{
    public void Execute(){
        Console.Clear();
    }
}

public class MacroCmd : ICommand{
    private IEnumerable<ICommand> cmds;
    public MacroCmd(params ICommand[] cmds){
        this.cmds = cmds;
    }

    public void Execute(){
        foreach(var cmd in cmds){
            cmd.Execute();
        }
    }
}
