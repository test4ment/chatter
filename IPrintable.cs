public interface IPrintable {
    public void Print(string message);
}

public class StdOutPrintLineAdapter : IPrintable{
    public void Print(string message){
        Console.WriteLine(message);
    }
}

public class StdOutPrintAdapter : IPrintable{
    public void Print(string message){
        Console.Write(message);
    }
}
