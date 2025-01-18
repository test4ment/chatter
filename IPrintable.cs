public interface IPrinter {
    public void Print(string message);
}

public class StdOutPrintLineAdapter : IPrinter{
    public void Print(string message){
        Console.WriteLine(message);
    }
}

public class StdOutPrintAdapter : IPrinter{
    public void Print(string message){
        Console.Write(message);
    }
}
