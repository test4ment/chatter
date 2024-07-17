public class TestingProcedure : ICommand {
    public void Execute(){
        new PrintLineMsg("Hello world!").Execute();

        var zero = 0;
        var a = 1/zero;

        Thread.Sleep(1000 * 2);
    }
}

public class DefaultInit : ICommand
{
    public void Execute()
    {
        var commands = new Dictionary<string, Action>(){
            {"connect", () => {throw new NotImplementedException();}},
            {"help", () => {Console.WriteLine("HEEELPP!");}}
        };

        IoC.Set("stdout writer", (object[] args) => {
            return new StdOutPrintLineAdapter();
        });

        IoC.Set("stdout writer no newline", (object[] args) => {
            return new StdOutPrintAdapter();
        });

        IoC.Set("Exception.Handler", (object[] args) => {
            return new HandleExceptionCmd((ICommand)args[0], (Exception)args[1]);
        });

        ExceptionHandler.SetDefaultHandler((cmd, ex) => {
            Console.WriteLine($"Caught {ex.GetType()} in " + cmd.ToString());
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
        });

        ExceptionHandler.SetHandler(typeof(RepeatCommand), typeof(NullReferenceException), (a, b) => {});

        IoC.Set("Commands.Handler", (object[] args) => {
            string cmd = (string)args[0];
            if(cmd.Length < 2 || cmd[0] != '/') return new ActionCommand(() => {});
            cmd = cmd[1..];
            string[] cmdargs = cmd.Split(); // load to ioc?

            return new ActionCommand(() => {
                commands.GetValueOrDefault(cmdargs[0], () => {Console.WriteLine("unkonw cmd");});
            });
        });
    }
}

public class HelloUser : ICommand
{
    public void Execute()
    {
        var q = IoC.Get<BlockingCollection<ICommand>>("Queue");
        q.Add(new PrintLineMsg("Hello user! Write your name:"));
        q.Add(new StartInputListener("input"));
        q.Add(new AwaitInputOnce("input", new GreetUser(), new StartServer()));
    }
}

public class StartServer : ICommand
{
    public void Execute()
    {
        var q = IoC.Get<BlockingCollection<ICommand>>("Queue");

        q.Add(new RegisterTcp());
        q.Add(new StartListeningTcp());
    }
}