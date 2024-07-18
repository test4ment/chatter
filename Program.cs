// rewrite into app command

global using System.Collections.Concurrent;
global using System.Net;
global using System.Net.Sockets;

BlockingCollection<ICommand> queue = new();
Queue<ICommand> queuePlanner = new();

var planner = new Planner(queuePlanner);

IoC.Set("Planner", (object[] args) => planner);
IoC.Set("MainQueue", (object[] args) => {return queue;});
IoC.Set("Queue", (object[] args) => {return queuePlanner;});
IoC.Set("IsRunning", (object[] args) => {return true;});

new Thread(planner.Start).Start();

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