using System.ComponentModel;
using System.Text;

public interface ICommand {
    public void Execute();
}

public class PrintLineMsg : ICommand{
    private string message;
    public PrintLineMsg(string message){
        this.message = message;
    }
    public void Execute() {
        IoC.Get<IPrintable>("stdout writer").Print(message);
    }
}

public class PrintMsg : ICommand{
    private string message;
    public PrintMsg(string message){
        this.message = message;
    }
    public void Execute() {
        IoC.Get<IPrintable>("stdout writer no newline").Print(message);
    }
}

public class PrintFromIoC : ICommand
{
    private string dep_name;
    public PrintFromIoC(string dep_name)
    {
        this.dep_name = dep_name;
    }

    public void Execute()
    {
        IoC.Get<IPrintable>("stdout writer no newline").Print(
            IoC.Get<string>(dep_name)
        );
    }
}

public class StartInputListener : ICommand
{
    string var_to_write;
    private ICommand? on_success;
    public StartInputListener(string var_to_write, ICommand? on_success = null)
    {
        this.var_to_write = var_to_write;
        this.on_success = on_success;
    }

    public void Execute()
    {
        new StartRepeating("Listener." + var_to_write, new TryWriteStdIn(var_to_write, on_success)).Execute();
    }
}

public class StopInputListener : ICommand
{
    private string var_to_write;
    public StopInputListener(string var_to_write)
    {
        this.var_to_write = var_to_write;
    }

    public void Execute()
    {
        new StopRepeating("Listener." + var_to_write).Execute();
    }
}

public class TryWriteStdIn : ICommand
{
    private string var_to_write;
    private ICommand? on_success;
    public TryWriteStdIn(string var_to_write, ICommand? on_success = null){
        this.var_to_write = var_to_write;
        this.on_success = on_success;
    }

    public void Execute()
    {
        if (Console.KeyAvailable){
            new UserInputStdIn(var_to_write).Execute();
            on_success?.Execute();
        }
    }
}

public class TryReadIoCVar : ICommand
{
    internal string var_to_read;
    internal ICommand on_read;
    public TryReadIoCVar(string var_to_read, ICommand on_read)
    {
        this.var_to_read = var_to_read;
        this.on_read = on_read;
    }

    public void Execute()
    {
        // Console.WriteLine($"Trying to read {var_to_read}");
        try{
            if(IoC.Get<object>(var_to_read) is null) return;
            on_read.Execute();
        }
        catch(KeyNotFoundException){}
    }
}

public class NullifyIoCVar : ICommand
{
    private string varname;
    public NullifyIoCVar(string varname)
    {
        this.varname = varname;
    }

    public void Execute()
    {
        IoC.Set(varname, (object[] args) => {return null!;});
    }
}

public class AwaitIoCVar : TryReadIoCVar, ICommand
{
    public AwaitIoCVar(string var_to_read, ICommand on_read) : base(var_to_read, on_read){}

    public new void Execute()
    {
        var_to_read = "Input." + var_to_read;
        var dep = "Await." + var_to_read;
        new StartRepeating(
            dep, 
            new TryReadIoCVar(
                var_to_read,
                new MacroCmd(new StopRepeating(dep), on_read, new NullifyIoCVar(var_to_read))
            )
        ).Execute();
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

public class StartCmdListener : ICommand{
    public void Execute()
    {
        var q = IoC.Get<BlockingCollection<ICommand>>("Queue");
        var createmyself = new ActionCommand(() => {q.Add(new StartCmdListener());});
        var cmd = new MacroCmd(
            new HandleOneCommand(),
            createmyself
        );
        
        q.Add(new AwaitIoCVar("input", new ExecuteOnException(
            cmd,
            new MacroCmd(
                // new PrintLineMsg("An exception occured!"),
                new ActionCommand(() => {
                    new HandleExceptionCmd(new HandleOneCommand(), IoC.Get<Exception>("Latest exception")).Execute();
                }),
                createmyself
            )
        )));
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

        IoC.Get<BlockingCollection<ICommand>>("Queue").Add(new RepeatCommand(cmd_dep));
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
        var dep = "Repeat." + dep_name;
        cmd.Execute();
        IoC.Set(dep, (object[] args) => {return cmd;});
        new RepeatCommand(dep).Execute();
    }
}

public class StopRepeating : ICommand
{
    private string dep_name;
    public StopRepeating(string dep_name){
        if(dep_name.StartsWith("Repeat.")){
            this.dep_name = dep_name[7..];
        }
        else{
            this.dep_name = dep_name;
        }
    }

    public void Execute()
    {
        IoC.Set("Repeat." + dep_name, (object[] args) => {return null!;});
    }
}

public class ActionCommand : ICommand
{
    Action action;
    public ActionCommand(Action action)
    {
        this.action = action;
    }

    public void Execute()
    {
        action();
    }
}

public class GreetUser : ICommand
{
    public void Execute()
    {
        new PrintMsg("Hello ").Execute();
        new PrintFromIoC("Input.input").Execute();
        new PrintMsg("!\n").Execute();
    }
}

public class RememberUsername : ICommand
{
    public void Execute()
    {
        var name = IoC.Get<string>("Input.input");
        IoC.Set("Username", (object[] args) => {return name;});
    }
}

public class PrintMyName : ICommand
{
    public void Execute()
    {
        new PrintLineMsg($"You are {IoC.Get<string>("Username")}").Execute();
    }
}

public class RegisterTcp : ICommand{
    public void Execute(){
        var tcp_server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IoC.Set("TCP server", (object[] args) => {return tcp_server;});
    }
}

public class StartListeningTcp : ICommand
{
    private int port;
    public StartListeningTcp(int port = 25560){
        this.port = port;
    }
    public void Execute()
    {
        var q = IoC.Get<BlockingCollection<ICommand>>("Queue");

        q.Add(new PrintLineMsg("Write local machine IP to listen connections (leave empty for localhost)"));
        q.Add(new AwaitIoCVar(
            "input",
            new ExecuteOnException(
                new MacroCmd(
                    new SetDefaultValueForInput<string>(
                        (string str) => {
                            return str != "";
                        },
                        "127.0.0.1"
                    ),
                    new BindAndListenTcp(port), 
                    new ActionCommand(() => {
                        q.Add(new MacroCmd(
                                new PrintMsg("Listening at "),
                                new PrintFromIoC("IP"), 
                                new PrintMsg($":{port}\n"),
                                new PrintFromIoC("Welcome message"),
                                new StartCmdListener(),
                                new AwaitOneClient()
                            )
                        );
                        }
                    )
                ),
                new MacroCmd(
                    new NullifyIoCVar("input"),
                    new StartListeningTcp(this.port)
                )
            )
            )
        );
    }
}

public class SetDefaultValueForInput<T> : ICommand
where T: notnull
{
    Func<T, bool> condition;
    T def;
    public SetDefaultValueForInput(Func<T, bool> condition, T def)
    {
        this.condition = condition;
        this.def = def;
    }

    public void Execute()
    {
        if(!condition(IoC.Get<T>("Input.input"))) IoC.Set("Input.input", (object[] args) => def);
    }
}

public class ExecuteOnException : ICommand
{
    Type exception;
    ICommand to_exec;
    ICommand on_exception;
    public ExecuteOnException(ICommand to_exec, ICommand on_exception, Type? exception = null)
    {
        this.exception = exception ?? typeof(Exception);
        this.to_exec = to_exec;
        this.on_exception = on_exception;
    }

    public void Execute()
    {
        try{
            to_exec.Execute();
        }
        catch(Exception e){
            if(exception.IsInstanceOfType(e)){
                IoC.Set("Latest exception", (object[] args) => e);
                on_exception.Execute();
            }
            else{
                throw;
            }
        }
    }
}

public class BindAndListenTcp : ICommand
{
    private int port;
    public BindAndListenTcp(int port = 25560){
        this.port = port;
    }
    public void Execute()
    {
        Socket server = IoC.Get<Socket>("TCP server");
        IPAddress ip = IPAddress.Parse(IoC.Get<string>("Input.input"));
        IoC.Set("IP", (object[] args) => ip.ToString());
        server.Blocking = false;
        server.Bind(new IPEndPoint(ip, port));
        server.Listen(1);
    }
}

public class TryAcceptOneClient : ICommand
{
    public void Execute()
    {
        Socket server = IoC.Get<Socket>("TCP server");
        Socket client = server.Accept();
        IoC.Set("TCP client", (object[] args) => {return client;});
    }
}

public class InitMessagingState : ICommand
{
    public void Execute()
    {
        IoC.Set("Message.Handler", (object[] args) => {
            var mess = (string)args[0];
            return new SendMessage(mess);
        });
    }
}

public class TryReadMessage : ICommand
{
    public async void Execute()
    {
        var connected = IoC.Get<Socket>("Connected");
        var buffer = new List<byte>();
        var bytesRead = new byte[1];
        
        while(await connected.ReceiveAsync(bytesRead) > 0){
            buffer.Add(bytesRead[0]); // 192.168.191.246
        }
        
        if(buffer.Count == 0) return;

        var encoding = IoC.Get<Encoding>("Encoding");

        Console.WriteLine("added buf " + encoding.GetString(buffer.ToArray()));
        IoC.Get<BlockingCollection<ICommand>>("Queue").Add(
            new PrintLineMsg(encoding.GetString(buffer.ToArray()))
        );
    }
}

public class StartMessageListener : ICommand
{
    public void Execute()
    {
        IoC.Get<BlockingCollection<ICommand>>("Queue").Add(
            new StartRepeating("Msg.Listener", new TryReadMessage())
        );
    }
}

public class InitMessagingStateAsServer : ICommand
{
    public void Execute()
    {
        var connected = IoC.Get<Socket>("TCP client");
        IoC.Set("Connected", (object[] args) => connected);
    }
}

public class SendMessage : ICommand
{
    string message;
    public SendMessage()
    {
        this.message = IoC.Get<string>("Input.input");
    }

    public SendMessage(string message)
    {
        this.message = message;
    }

    public void Execute()
    {
        var encoding = IoC.Get<Encoding>("Encoding");
        IoC.Get<Socket>("Connected").Send(encoding.GetBytes(message));
    }
}

public class MessagingStateCommand : ICommand
{
    public void Execute()
    {
        new InitMessagingState().Execute();
        new StartMessageListener().Execute();
    }
}

public class AwaitOneClient : ICommand
{
    private static ICommand macro = new MacroCmd(
        new TryAcceptOneClient(),
        new StopRepeating("Awaut.Client"),
        new ClearConsole(),
        new ActionCommand(() => {new PrintLineMsg(((IPEndPoint)IoC.Get<Socket>("TCP client").RemoteEndPoint!).Address.ToString() + " connected").Execute();}),
        new InitMessagingStateAsServer(),
        new MessagingStateCommand()
    );

    public void Execute()
    {
        new StartRepeating("Awaut.Client",
            new ExecuteOnException(macro, new ActionCommand(() => {}), typeof(SocketException))
        ).Execute();
    }
}

public class TryConnect : ICommand
{
    IPAddress ip;
    int port;

    public TryConnect(string ip, int port = 25560){
        this.ip = IPAddress.Parse(ip);
        this.port = port;
    }
    public TryConnect(IPAddress ip, int port = 25560){
        this.ip = ip;
        this.port = port;
    }
    public TryConnect(string ip, string port){
        this.ip = IPAddress.Parse(ip);
        this.port = int.Parse(port);
    }

    public void Execute()
    {
        Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        {
            Blocking = false
        };

        try{
            client.Connect(this.ip, this.port);
        }
        catch(Win32Exception ex){
            if (ex.ErrorCode == 10035) while (!client.Poll(1000, SelectMode.SelectWrite)){} // WSAEWOULDBLOCK is expected, means connect is in progress 
        }

        IoC.Set("Connected", (object[] args) => client);
        new MessagingStateCommand().Execute();
        new PrintLineMsg("Connected to " + this.ip.ToString()).Execute();
    }
}

public class HandleOneCommand : ICommand
{
    public void Execute()
    {
        // new AwaitInputOnce("cmd").Execute();
        IoC.Get<ICommand>("Commands.Handler", IoC.Get<string>("Input.input")).Execute();
    }
}
