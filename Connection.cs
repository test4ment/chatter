// public class RegisterTcp : ICommand{
//     public void Execute(){
//         var tcp_server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
//         IoC.Set("TCP server", (object[] args) => {return tcp_server;});
//     }
// }

// public class StartListeningTcp : ICommand
// {
//     private int port;
//     public StartListeningTcp(int port = 25560){ // unhardcode
//         this.port = port;
//     }
//     public void Execute()
//     {
//         var q = IoC.Get<BlockingCollection<ICommand>>("Queue");

//         q.Add(new PrintLineMsg("Write local machine IP to listen connections (leave empty for localhost)"));
//         q.Add(new AwaitIoCVar(
//             "input",
//             new ExecuteOnException(
//                 new MacroCmd(
//                     new SetDefaultValueForInput<string>(
//                         (string str) => {
//                             return str != "";
//                         },
//                         "127.0.0.1"
//                     ),
//                     new BindAndListenTcp(port), 
//                     new ActionCommand(() => {
//                         q.Add(new MacroCmd(
//                                 new PrintMsg("Listening at "),
//                                 new PrintFromIoC("IP"), 
//                                 new PrintMsg($":{port}\n"),
//                                 new PrintFromIoC("Welcome message"),
//                                 new StartCmdListener(),
//                                 new AwaitOneClient()
//                             )
//                         );
//                         }
//                     )
//                 ),
//                 new MacroCmd(
//                     new AddCmdsToQueueCmd(new RethrowLatestExceptionCommand()),
//                     new NullifyIoCVar("input"),
//                     new StartListeningTcp(this.port)
//                 )
//             )
//             )
//         );
//     }
// }
// public class BindAndListenTcp : ICommand
// {
//     private int port;
//     public BindAndListenTcp(int port = 25560){
//         this.port = port;
//     }
//     public void Execute()
//     {
//         Socket server = IoC.Get<Socket>("TCP server");
//         IPAddress ip = IPAddress.Parse(IoC.Get<string>("Input.input"));
//         IoC.Set("IP", (object[] args) => ip.ToString());
//         server.Blocking = false;
//         server.Bind(new IPEndPoint(ip, port));
//         server.Listen(1);
//     }
// }

// public class TryAcceptOneClient : ICommand
// {
//     public void Execute()
//     {
//         Socket server = IoC.Get<Socket>("TCP server");
//         Socket client = server.Accept();
//         IoC.Set("Connected", (object[] args) => {return client;});
//     }
// }
// public class ForceReadMessage : ICommand
// {
//     private Action<string> action_on_message;
//     private int awaitms;
//     public ForceReadMessage(Action<string> action_on_message, int awaitms = 50)
//     {
//         this.action_on_message = action_on_message;
//         this.awaitms = awaitms;
//     }
//     public ForceReadMessage(int awaitms = 50)
//     {
//         this.action_on_message = (mess) => {
//             new AddCmdsToQueueCmd(
//                 new PrintLineMsgChat(mess)
//             ).Execute();
//         };
//         this.awaitms = awaitms;
//     }

//     public void Execute()
//     {
//         var connected = IoC.Get<Socket>("Connected");
//         var bytesRead = new byte[1024];
//         var waitUntil = DateTime.Now.AddMilliseconds(awaitms);

//         while (DateTime.Now <= waitUntil)
//         {
//             try
//             {
//                 if (connected.Receive(bytesRead) != 0){
//                     var encoding = IoC.Get<Encoding>("Encoding");
//                     string message = encoding.GetString(bytesRead.TakeWhile((byt) => byt != 0).ToArray()); // take all nonnull
//                     // string message = (string)encoding.GetString(bytesRead).TakeWhile((ch) => ch != (char)0x0); // utf16 support

//                     action_on_message(message); // Redo with ICommand
//                     break;
//                 }
//             }
//             catch (SocketException e)
//             {
//                 if(e.SocketErrorCode != SocketError.WouldBlock) throw;
//             }
//         }
//     }
// }
