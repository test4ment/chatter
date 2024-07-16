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

public class HandleExceptionCmd : ICommand{
    ICommand cmd;
    Exception ex;

    public HandleExceptionCmd(ICommand cmd, Exception ex){
        this.cmd = cmd;
        this.ex = ex;
    }

    public void Execute(){
        ExceptionHandler.GetHandler(cmd.GetType(), ex.GetType())(cmd, ex);
    }
}

public class StartListener : ICommand{
    public void Execute()
    {
        throw new NotImplementedException();
    }
}

public class UserInputStdIn : ICommand{
    private string global_varname;

    public UserInputStdIn(string global_varname){
        this.global_varname = global_varname;
    }

    public void Execute(){
        var input = Console.ReadLine();
        input ??= "";
        IoC.Set("Input." + global_varname, (object[] args) => {return input;});
    }
}

public class RepeatCommand : ICommand{
    private string cmd_dep;
    
    public RepeatCommand(string cmd_dep){
        this.cmd_dep = cmd_dep;
    }

    public void Execute(){
        IoC.Get<ICommand>(cmd_dep).Execute();
        // queue.Add(new ContiniousCommand(cmd, queue));
        IoC.Get<ICollection<ICommand>>("Queue").Add(new RepeatCommand(cmd_dep));
    }
}

public class StartRepeating : ICommand
{
    private string dep_name;
    private ICommand cmd;
    public StartRepeating(string dep_name, ICommand cmd)
    {
        this.dep_name = dep_name;
        this.cmd = cmd;
    }

    public void Execute()
    {
        IoC.Set(dep_name, (object[] args) => {return cmd;});
    }
}

public class StopRepeating : ICommand
{
    private string dep_name;
    public StopRepeating(string dep_name) => this.dep_name = dep_name;

    public void Execute()
    {
        IoC.Set(dep_name, (object[] args) => {return null!;});
    }
}