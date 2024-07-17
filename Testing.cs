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
        var helpmsg = "help \t\t\t show this message \n" +
        "connect <ip>[:port] \t not implemented yet \n" +
        "cls \t\t\t clear screen \n" +
        "exit \t\t\t close app \n" +
        "whoami \t\t\t get username \n" +
        "setname <name> \t\t set new username \n"
        ;

        var commands = new Dictionary<string, Action<string[]>>(){
            {"connect", (argscmd) => {throw new NotImplementedException();}},
            {"help", (argscmd) => {new PrintLineMsg(helpmsg).Execute();}},
            {"cls", (argscmd) => {new ClearConsole().Execute();}},
            {"exit", (argscmd) => {new StopApp().Execute();}},
            {"whoami", (argscmd) => {new PrintMyName().Execute();}},
            {"setname", (argscmd) => {
                try{
                    var name = argscmd[0];
                    IoC.Set("Username", (object[] args) => name);
                }
                catch(IndexOutOfRangeException){
                    new PrintLineMsg("No argument given\nUsage: setname <name>").Execute();
                }
            }}
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
            // Console.WriteLine(ex.StackTrace);
        });

        ExceptionHandler.SetHandler(typeof(RepeatCommand), typeof(NullReferenceException), (a, b) => {});

        IoC.Set("Commands.Handler", (object[] args) => {
            string cmd = (string)args[0];

            if(cmd.Length < 2 || cmd[0] != '/') return new ActionCommand(() => {});
            
            cmd = cmd[1..];
            
            string[] cmdargs = cmd.Split(); // load to ioc?

            return new ActionCommand(() => {
                commands.GetValueOrDefault(cmdargs[0], 
                (argscmd) => {
                    new PrintLineMsg($"Unknown command {cmdargs[0]}").Execute();
                    }
                    )(cmdargs[1..]);
                }
            );
        }
        );
    }
}

public class HelloUser : ICommand
{
    public void Execute()
    {
        var q = IoC.Get<BlockingCollection<ICommand>>("Queue");
        q.Add(new PrintLineMsg("Hello user! Write your name:"));
        q.Add(new StartInputListener("input"));
        q.Add(new AwaitIoCVar("input", new MacroCmd(new RememberUsername(), new GreetUser(), new StartServer())));
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