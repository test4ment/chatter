public interface IPrintable {
    public void Print(string message);
}

public class StdOutPrintAdapter : IPrintable{
    public void Print(string message){
        Console.WriteLine(message);
    }
}
