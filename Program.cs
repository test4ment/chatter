﻿// rewrite into app command

global using System.Collections.Concurrent;
global using System.Net;
global using System.Net.Sockets;

BlockingCollection<ICommand> queue = new();
IoC.Set("Queue", (object[] args) => {return queue;});

IoC.Set("IsRunning", (object[] args) => {return true;});

queue.Add(new DefaultInit());
// queue.Add(new TestingProcedure());
queue.Add(new HelloUser());
// queue.Add(new StopApp());

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