public interface ICommand {
    public void Execute();
}

public class PrintMsg : ICommand{
    private string message;
    public PrintMsg(string message){
        this.message = message;
    }
    public void Execute() {
        IoC.Get<IPrintable>("stdout writer").Print(message);
    }
}

public interface IPrintable {
    public void Print(string message);
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

public class TestingProcedure : ICommand {
    public void Execute(){
        IoC.Set("stdout writer", (object[] args) => {
            return new StdOutPrintAdapter();
        });

        new PrintMsg("Hello world!").Execute();

        Thread.Sleep(1000 * 2);

        new StopApp().Execute();
    }
}