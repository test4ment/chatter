// public class TestingProcedure : ICommand {
//     public void Execute(){
//         new PrintMsg("Hello world!", new StdOutPrintLineAdapter()).Execute();

//         var zero = 0;
//         var a = 1/zero;

//         Thread.Sleep(1000 * 2);
//     }
// }

public class DebugExHandlerInit : ICommand
{
    Action<ICommand, Exception> act;
    public DebugExHandlerInit(): this(false)
    {}

    public DebugExHandlerInit(bool stacktrace)
    {
        if(stacktrace){
                act = (cmd, ex) => {
                Console.WriteLine($"Caught {ex.GetType()} in " + cmd.ToString());
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            };
        }
        else{
                act = (cmd, ex) => {
                Console.WriteLine($"Caught {ex.GetType()} in " + cmd.ToString());
                Console.WriteLine(ex.Message);
            };
        }
    }

    public void Execute()
    {
        ExceptionHandler.SetDefaultHandler(act);
    }
}

// public class DefaultInit : ICommand
// {
//     public void Execute()
//     {
//         var helpDict = new Dictionary<string, string>(){
//             {"help", "\t\t\t show this message"},
//             {"connect", "<ip>[:port] \t connect to someone"},
//             {"cls", "\t\t\t clear screen"},
//             {"exit", "\t\t\t close app"},
//             {"whoami", "\t\t\t get username"},
//             {"setname", "<name> \t\t set new username"},
//             {"encode", "<string> \t debug"}
//         };

//         var welcome = @"Welcome to chatter v0.2! Type ""/help"" to list all commands" + "\n";

//         var commands = new Dictionary<string, Action<string[]>>(){
//             {"connect", (argscmd) => {
//                 try{
//                     string ip, port;
//                     if(argscmd[0].Contains(':')){
//                         string[] ipport = argscmd[0].Split(':');
//                         ip = ipport[0];
//                         port = ipport[1];
//                     }
//                     else{
//                         ip = argscmd[0];
//                         port = "25560";
//                     }
//                     new TryConnect(ip, port).Execute();
//                 }
//                 catch(IndexOutOfRangeException){
//                     new PrintLineMsg("No argument given\nUsage: connect <ip>[:port]").Execute();
//                 }
//             }},
//             {"help", (argscmd) => {
//                 IoC.Get<IDictionary<string, string>>("Help.Dict")
//                 .ToList()
//                 .ForEach((kv) => {
//                     new PrintLineMsg($"{kv.Key} {kv.Value}").Execute();
//                 });
//             }},
//             {"cls", (argscmd) => {new ClearConsole().Execute();}},
//             {"exit", (argscmd) => {
//                 IoC.Get<ICommand>("Exit.Handler").Execute();
//             }},
//             {"whoami", (argscmd) => {new PrintMyName().Execute();}},
//             {"setname", (argscmd) => {
//                 try{
//                     var name = string.Join(' ', argscmd);
//                     if(name.Length == 0) throw new IndexOutOfRangeException();
//                     IoC.Set("Info.Username", (object[] args) => name);
//                 }
//                 catch(IndexOutOfRangeException){
//                     new PrintLineMsg("No argument given\nUsage: setname <name>").Execute();
//                 }
//             }},
//             {"encode", (argscmd) => {
//                 var str = string.Join(' ', argscmd);
//                 var encoding = IoC.Get<Encoding>("Encoding");
//                 var encstr = encoding.GetBytes(str);
//                 encstr.ToList().ForEach((byt) => {Console.Write($"{byt} ");});
//                 Console.WriteLine();
//             }}
//         };

//         var encoding = Encoding.UTF8;

//         IoC.Set("Help.Dict", (object[] args) => helpDict);

//         IoC.Set("Encoding", (object[] args) => encoding);

//         IoC.Set("Welcome message", (object[] args) => welcome);

//         IoC.Set("stdout writer", (object[] args) => {
//             return new StdOutPrintLineAdapter();
//         });

//         IoC.Set("stdout writer no newline", (object[] args) => {
//             return new StdOutPrintAdapter();
//         });

//         new DefaultWriter().Execute();

//         IoC.Set("Exception.Handler", (object[] args) => {
//             return new HandleExceptionCmd((ICommand)args[0], (Exception)args[1]);
//         });

//         ExceptionHandler.SetDefaultHandler((cmd, ex) => {
//             Console.WriteLine(ex.Message);
//         });

//         ExceptionHandler.SetHandler(typeof(RepeatCommand), typeof(NullReferenceException), (a, b) => {});

//         IoC.Set("Message.Handler", (object[] args) => {
//             // var mess = (string)args[0];
//             return new ActionCommand(() => {});
//         });

//         IoC.Set("Exit.Handler", (object[] args) => {
//             return new StopApp();
//         });

//         IoC.Set("Commands.Handler", (object[] args) => {
//             string cmd = (string)args[0];

//             if(cmd.Length < 2 || cmd[0] != '/') return IoC.Get<ICommand>("Message.Handler", cmd);
            
//             cmd = cmd[1..];
            
//             string[] cmdargs = cmd.Split(); // load to ioc?

//             return new ActionCommand(() => {
//                 commands.GetValueOrDefault(cmdargs[0], 
//                 (argscmd) => {
//                     new PrintLineMsg($"Unknown command {cmdargs[0]}").Execute();
//                     }
//                     )(cmdargs[1..]);
//                 }
//             );
//         }
//         );
//     }
// }

// public class HelloUser : ICommand
// {
//     public void Execute()
//     {
//         var q = IoC.Get<BlockingCollection<ICommand>>("Queue");
//         q.Add(new PrintLineMsg("Hello user! Write your name:"));
//         q.Add(new StartInputListener("input"));
//         q.Add(new AwaitIoCVar("input", new MacroCmd(new RememberUsername(), new GreetUser(), new StartServer())));
//     }
// }

// public class StartServer : ICommand
// {
//     public void Execute()
//     {
//         var q = IoC.Get<BlockingCollection<ICommand>>("Queue");

//         q.Add(new RegisterTcp());
//         q.Add(new StartListeningTcp());
//     }
// }