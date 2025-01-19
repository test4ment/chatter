public interface ICommand {
    public void Execute();
}


public class StopApp : ICommand{
    private readonly IContext ctx;

    public StopApp(IContext ctx)
    {
        this.ctx = ctx;
    }

    public void Execute(){
        ctx.Exit();
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

public class ActionCommand : ICommand
{
    private Action action;
    public ActionCommand(Action action)
    {
        this.action = action;
    }

    public ActionCommand()
    {
        this.action = () => {};
    }

    public void Execute()
    {
        action();
    }
}

public class GreetUser : ICommand
{
    private readonly string username;
    private readonly IContext context;

    public GreetUser(string username, IContext context)
    {
        this.username = username;
        this.context = context;
    }

    public void Execute()
    {
        context.Enqueue(
            new PrintMsg($"Hello {username}!", context.userIntefrace.printer_new_line)
        );
    }
}

// public class PrintMyName : ICommand
// {
//     public void Execute()
//     {
//         new PrintLineMsg($"You are {IoC.Get<string>("Info.Username")}").Execute();
//     }
// }


// public class SetDefaultValueForInput<T> : ICommand
// where T: notnull
// {
//     Func<T, bool> condition;
//     T def;
//     public SetDefaultValueForInput(Func<T, bool> condition, T def)
//     {
//         this.condition = condition;
//         this.def = def;
//     }

//     public void Execute()
//     {
//         if(!condition(IoC.Get<T>("Input.input"))) IoC.Set("Input.input", (object[] args) => def);
//     }
// }


public class ThrowNewExceptionCmd : ICommand
{
    private Exception ex;
    public ThrowNewExceptionCmd(Exception ex)
    {
        this.ex = ex;
    }
    public ThrowNewExceptionCmd(string message){
        this.ex = new Exception(message);
    }

    public ThrowNewExceptionCmd(string message, Exception inner_ex){
        this.ex = new Exception(message, inner_ex);
    }

    public void Execute()
    {
        throw this.ex;
    }
}



// public class InitMessagingState : ICommand
// {
//     public void Execute()
//     {
//         IoC.Set("Message.Handler", (object[] args) => {
//             var mess = (string)args[0];
//             return new SendMessage(mess);
//         });

//         IoC.Set("Exit.Handler", (object[] args) => {
//             return new DeInitMessagingState();
//         });

//         IoC.Get<IDictionary<string, string>>("Help.Dict")["exit"] = "\t\t\t leave chat";

//         new AddCmdsToQueueCmd(new WriterWithYou()).Execute();
//     }
// }

// public class DeInitMessagingState : ICommand
// {
//     public void Execute()
//     {
//         var name = IoC.Get<string>("Info.Username");
//         using(var connected = IoC.Get<Socket>("Connected")){
//             new SendMessage(
//                 $"User {name} has disconnected; type /exit to leave chat"
//             ).Execute();
//             connected.Disconnect(false);
//         }

//         IoC.Set("Exit.Handler", (object[] args) => {
//             return new StopApp();
//         });

//         IoC.Set("Message.Handler", (object[] args) => {
//             return new ActionCommand(() => {});
//         });

//         IoC.Get<IDictionary<string, string>>("Help.Dict")["exit"] = "\t\t\t close app";

//         new AddCmdsToQueueCmd(
//             new NullifyIoCVar("Connected"),
//             new DefaultWriter(),
//             new ClearConsole(),
//             new PrintFromIoC("Welcome message")
//         ).Execute();
//     }
// }


// public class StartMessageListener : ICommand
// {
//     public void Execute()
//     {
//         IoC.Get<BlockingCollection<ICommand>>("Queue").Add(
//             new StartRepeating("Msg.Listener", new ForceReadMessage())
//         );
//     }
// }


// public class SendMessage : ICommand
// {
//     string message;
//     public SendMessage()
//     {
//         this.message = IoC.Get<string>("Input.input");
//     }

//     public SendMessage(string message)
//     {
//         this.message = message;
//     }

//     public void Execute()
//     {
//         var encoding = IoC.Get<Encoding>("Encoding");
//         IoC.Get<Socket>("Connected").Send(encoding.GetBytes(message));
//     }
// }

// public class MessagingStateCommand : ICommand
// {
//     public void Execute()
//     {
//         new InitMessagingState().Execute();
//         new StartMessageListener().Execute();
//     }
// }

// public class AwaitOneClient : ICommand
// {
//     private static ICommand macro = new MacroCmd(
//         new TryAcceptOneClient(),
//         new SendClientInfo(),
//         new ReceiveClientInfo(),
//         new StopRepeating("Await.Client"),
//         new ClearConsole(),
//         new ActionCommand(() => {
//             new PrintLineMsg(
//                 IoC.Get<string>("Connected.Username") + " (" +
//                 ((IPEndPoint)IoC.Get<Socket>("Connected").RemoteEndPoint!).Address.ToString() + ") connected"
//             ).Execute();
//         }),
//         new MessagingStateCommand()
//     );

//     public void Execute()
//     {
//         new StartRepeating("Await.Client",
//             new ExecuteOnException(macro, new ActionCommand(() => {}), typeof(SocketException))
//         ).Execute();
//     }
// }

// public class TryConnect : ICommand
// {
//     IPAddress ip;
//     int port;

//     public TryConnect(string ip, int port = 25560){
//         this.ip = IPAddress.Parse(ip);
//         this.port = port;
//     }
//     public TryConnect(IPAddress ip, int port = 25560){
//         this.ip = ip;
//         this.port = port;
//     }
//     public TryConnect(string ip, string port){
//         this.ip = IPAddress.Parse(ip);
//         this.port = int.Parse(port);
//     }

//     public void Execute()
//     {
//         Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
//         {
//             Blocking = false
//         };

//         try{
//             client.Connect(this.ip, this.port);
//         }
//         catch(SocketException ex){
//             if (ex.SocketErrorCode == SocketError.WouldBlock) while (!client.Poll(10000, SelectMode.SelectWrite)){} // WSAEWOULDBLOCK is expected, means connect is in progress 
//             else{
//                 throw;
//             }
//         }
//         IoC.Set("Connected", (object[] args) => client);

//         new SendClientInfo().Execute();
//         new ReceiveClientInfo().Execute();
//         new MessagingStateCommand().Execute();
//         new PrintLineMsg(
//             "Connected to " + 
//             IoC.Get<string>("Connected.Username") +
//             $" ({this.ip})"
//         ).Execute();
//     }
// }


// public class SendClientInfo : ICommand
// {
//     public void Execute()
//     {
//         var connected = IoC.Get<Socket>("Connected"); // unify all connected actions
//         var encoder = IoC.Get<Encoding>("Encoding");

//         var infoJson = new JsonObject
//         {
//             { "Username", IoC.Get<string>("Info.Username") } // get from storage
//         };

//         var bytes = encoder.GetBytes(JsonSerializer.Serialize(infoJson));
//         var waitUntil = DateTime.Now.AddMilliseconds(300);
//         while(DateTime.Now <= waitUntil){ // lin
//             try{
//                 if(connected.Send(bytes) != 0) break;
//             }
//             catch(SocketException e){
//                 Console.WriteLine(e.Message);
//             }
//         }; 
//         // connected.Send(bytes); // win
//     }
// }

// public class ReceiveClientInfo : ICommand
// {
//     public void Execute()
//     {
//         new ForceReadMessage(
//             (mess) => {
//                 var infoJson = JsonObject.Parse(mess);
                
//                 var res = JsonSerializer.Deserialize<Dictionary<string, string>>(infoJson); // dynamic type?

//                 res!.ToList().ForEach((kv) => {IoC.Set($"Connected.{kv.Key}", (object[] args) => kv.Value);});
//             },
//             200
//         ).Execute();
//     }
// }
