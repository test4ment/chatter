public class StdOutPrintAdapter : IPrintable{
    public void Print(string message){
        Console.WriteLine(message);
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