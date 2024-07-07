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