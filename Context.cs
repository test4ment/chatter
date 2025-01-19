public class DefaultContext : IContext
{
    private Queue<ICommand> queue = new();
    private bool running = true;

    public IUserIntefrace userIntefrace => new ConsoleIntefrace();

    public DefaultContext()
    {
    }

    public void Run(){
        ICommand cmd;

        while(running){
            if(queue.Count > 0){
                cmd = queue.Dequeue();
                try{
                    cmd.Execute();
                }
                catch(Exception e){
                    ExceptionHandler.GetHandler(cmd, e)(cmd, e);
                }
            }
        }
    }

    public void Exit()
    {
        running = false;
    }

    public void Enqueue(ICommand cmd) => queue.Enqueue(cmd);
}

public interface IContext{
    public void Exit();
    public void Run();
    public void Enqueue(ICommand cmd);
    public IUserIntefrace userIntefrace {get;}
}

public interface IUserIntefrace{
    public IPrinter printer {get;}
    public IPrinter printer_new_line {get;}
}

public class ConsoleIntefrace : IUserIntefrace
{
    public IPrinter printer {get;}

    public IPrinter printer_new_line {get;}

    public ConsoleIntefrace()
    {
        printer = new StdOutPrintAdapter();
        printer_new_line = new StdOutPrintLineAdapter();
    }
}
