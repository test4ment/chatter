using System.Runtime.CompilerServices;

public class TestingProcedure : ICommand {
    public void Execute(){
        IoC.Set("stdout writer", (object[] args) => {
            return new StdOutPrintAdapter();
        });

        IoC.Set("Exception.Handler", (object[] args) => {
            return new HandleExceptionCmd((ICommand)args[0], (Exception)args[1]);
        });

        ExceptionHandler.SetDefaultHandler((cmd, ex) => {
            Console.WriteLine($"Caught {ex.GetType()} in " + cmd.ToString());
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
        });

        new PrintMsg("Hello world!").Execute();

        var zero = 0;
        var a = 1/zero;

        Thread.Sleep(1000 * 2);
    }
}