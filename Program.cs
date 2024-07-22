global using System.Collections.Concurrent;
global using System.Net;
global using System.Net.Sockets;
global using System.Text;
global using System.Text.Json;
global using System.Text.Json.Nodes;
global using System.Runtime.InteropServices;

BlockingCollection<ICommand> queue = new();

IoC.Set("Queue", (object[] args) => {return queue;});
IoC.Set("IsRunning", (object[] args) => {return true;});

queue.Add(new DefaultInit());
// queue.Add(new DebugExHandlerInit());
queue.Add(new HelloUser());

ICommand cmd;

Thread t = new Thread(() => {
        while(IoC.Get<bool>("IsRunning")){
            cmd = queue.Take();
            try{
                cmd.Execute();
            }
            catch(Exception e){
                IoC.Get<ICommand>("Exception.Handler", cmd, e).Execute();
            }
        }
    }
);

t.Start();