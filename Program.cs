using CellcomClient;

string inputString;
Client port = new Client();

Console.WriteLine("You need to enter on  format <id>-<action>\t *To quit use id: 999");
Console.WriteLine("Available actions:\n1. NEW\n2. JOIN\n3. STOP\n4. QUIT");
Console.WriteLine("Enter to start conversation");
Console.ReadKey();
Console.WriteLine("Conversation started:");

while (true)
{
    inputString = Console.ReadLine();
    inputString = inputString.ToUpper();
    if (inputString.Split("-").Last() == "QUIT")
    {
        port.SendMessage(inputString);
        break;
    }
    if(CheckCorrectInput(inputString))
        port.SendMessage(inputString);
    else
        Console.WriteLine("Enter again your request by the correct format");   
}

bool CheckCorrectInput(string input)
{
    if (!input.Contains("-"))
        return false;
    if (!input.Contains("NEW") && !input.Contains("JOIN") && !input.Contains("STOP"))
        return false;
    return true;
}