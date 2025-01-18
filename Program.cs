global using System.Collections.Concurrent;
global using System.Net;
global using System.Net.Sockets;
global using System.Text;
global using System.Text.Json;
global using System.Text.Json.Nodes;
global using System.Runtime.InteropServices;
using chatter.Tests;


// queue.Add(new DefaultInit());
// queue.Add(new DebugExHandlerInit());
// queue.Add(new HelloUser());
Queue<ICommand> queue = new();
queue.Enqueue(new WriterTest(queue));

Thread t = new Thread(() => {
        ICommand cmd;
        // Queue<ICommand> queue = new();

        while(true){
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
);

t.Start();
