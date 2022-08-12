// To Compile:
//   C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe /t:exe /out:wevtutil.exe wevtutil.cs

//TODO: Print output all at once, allow for writing out to files, allow for xml output format

using System;
using System.Linq;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;

public class ReadEventLog
{
    private static void PrintUsage() 
    {
        Console.WriteLine(@"Attempts to mimic/emulate wevtutil.exe behavior, but only for remote machine queries. Minimal syntax differences. Displays most recent logs first. Requires local admin on remote machine.
    
USAGE:
    wevtutil.exe <log name> /q <query - put in quotes> /r <remote system> [/c <count>]
    Security is the default eventlog, but it isn't optional. Please don't forget it.
    /c default is 5
    /r is required. You may use localhost if you want to run it against your local machine
    Note: There is no colon after any of the arguments, unlike like the real wevtutil.exe
    Note: Text is the only supported output format at the moment
    
EXAMPLES:
    wevtutil.exe Security /q ""*[System[EventID=4624] and EventData[Data[@Name='SubjectUserName'] and Data = 'entersamaccountnamehere']]"" /r DC.MYDOMAIN.LOCAL /c 3  
        - Displays the most recent 3 'Logon' events for the specified user from the Security log on DC.MYDOMAIN.LOCAL");
        Console.WriteLine("\nDONE");
    }
    
    public static void Main(string[] args)
    {
        string eventLogName = "";
        int count = 5;
        List<long> eventIDs = new List<long>();
        bool test;
        string targetSystem = "";
        string userQuery = "";
        
        // Parse arguments
        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];
        
            switch (arg.ToUpper())
            {
                case "/?": // Help
                    PrintUsage();
                    return;
                case "/H": // Help
                case "-H":
                    PrintUsage();
                    return;
                case "/C": // Count
                case "-C":
                    i++;
                    
                    // Catch error while attempting to parse the count to prevent exception
                    test = int.TryParse(args[i], out count);
                    if (test == false || count < 1) 
                    {
                        Console.WriteLine("Error: Invalid count");
                        Console.WriteLine("\nDONE");
                        return;
                    }
                    break;
                    
                case "-R": // Remote System
                case "/R":
                    i++;
                    targetSystem = args[i];
                    break;

                case "-Q": // Query
                case "/Q":
                    i++;
                    userQuery = args[i];
                    break;
       
                default: // eventLogName
                    eventLogName = arg;
                    break;
            }
        }
        
        if (eventLogName != "" && userQuery != "" && targetSystem != "")
        {

            string queryString = userQuery;
            EventLogSession myEventLogSession = new EventLogSession(targetSystem);
            EventLogQuery query = new EventLogQuery(eventLogName, PathType.LogName, queryString);
            query.ReverseDirection = true;
            query.TolerateQueryErrors = true;
            query.Session = myEventLogSession;

            try
            {
                EventLogReader logReader = new EventLogReader(query);
                // Display event info
                for (EventRecord eventInstance = logReader.ReadEvent(); null != eventInstance; eventInstance = logReader.ReadEvent())
                {
                    if (count != 0)
                    {
                        Console.WriteLine("----------------------------------------------------");
                        Console.WriteLine("Timestamp: {0}", eventInstance.TimeCreated);
                        Console.WriteLine("MachineName: {0}", eventInstance.MachineName);
                        Console.WriteLine("EventLogName: {0}", eventInstance.LogName);
                        Console.WriteLine("EventID: {0}", eventInstance.Id);  

                        try
                        {
                            Console.WriteLine("Description: {0}", eventInstance.FormatDescription());
                        }
                        catch (EventLogException ex)
                        {
                            Console.WriteLine("Description: {0}", ex.Message);
                        }

                        count = count -1;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            catch (EventLogException e)
            {
                Console.WriteLine("Could not query the remote computer! " + e.Message);
                return;
            }
        
            Console.WriteLine("\nDONE");

            }
        else
        {
            Console.WriteLine("Invalid arguments\nDONE");
        }
    }
}