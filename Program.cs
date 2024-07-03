// rewrite into app command

using System.Collections.Concurrent;

BlockingCollection<ICommand> queue = new();
ICommand cmd;
IoC.Set("IsRunning", (object[] args) => {return true;});

queue.Add(new TestingProcedure());

Thread t = new Thread(() => {
        while(IoC.Get<bool>("IsRunning")){
            cmd = queue.Take();
            try{
                cmd.Execute();
            }
            catch(Exception e){
                IoC.Get<ICommand>("Exception.Handler", e, cmd).Execute();
            }
        }
    }
);

t.Start();