using StreamElementsLogger;

const string keyFileName = "key.txt";
if (!File.Exists(keyFileName))
{
    File.Create(keyFileName).Close();
    Console.Error.WriteLine("""
Created "key.txt", enter your StreamElements API key in this file.

Login to StreamElements and go to this link: https://streamelements.com/dashboard/account/channels
Click on 'Show secrets' and copy the 'JWT Token' to the "key.txt" file
""");
    Console.Error.WriteLine("\n\nPress any key to continue...");
    Console.ReadKey();
    return;
}

var key = File.ReadAllText(keyFileName);
if (key.Length <= 0)
{
    Console.Error.WriteLine("""
No key in "key.txt", enter your StreamElements API key in this file.

Login to StreamElements and go to this link: https://streamelements.com/dashboard/account/channels
Click on 'Show secrets' and copy the 'JWT Token' to the "key.txt" file
""");
    Console.Error.WriteLine("\n\nPress any key to continue...");
    Console.ReadKey();
    return;
}

new Dumper(key).Start();
