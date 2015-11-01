using System;
using System.IO;


public static class Log
{
    /// <summary>
    /// Appends logMessage to the log.txt file. A new file is created if it doesn't already exist
    /// </summary>
    /// <param name="logMessage"></param>
    //public static void WriteLog(string logMessage)
    //{
    //    using (StreamWriter w = File.AppendText("log.txt"))
    //    {
    //        w.Write("\r\nLog Entry : ");
    //        w.WriteLine("{0} {1}\n  :", DateTime.Now.ToLongTimeString(), DateTime.Now.ToLongDateString());
    //        w.WriteLine("  :{0}", logMessage);
    //        w.WriteLine("-------------------------------");
    //    }
    //}

    /// <summary>
    /// Appends logMessage to filename (if supplied) else log.txt. A new file is created if it doesn't already exist
    /// </summary>
    /// <param name="logMessage"></param>
    public static void WriteLog(string logMessage, string filename = "log.txt")
    {
        using (StreamWriter w = File.AppendText(filename))
        {
            w.Write("\r\nLog Entry : ");
            w.WriteLine("{0} {1}\n  :", DateTime.Now.ToLongTimeString(), DateTime.Now.ToLongDateString());
            w.WriteLine("  :{0}", logMessage);
            w.WriteLine("-------------------------------");
        }
    }

    public static void Seperator( string filename = "log.txt")
    {
        using (StreamWriter w = File.AppendText(filename))
        {
            w.WriteLine("**********************************************************************************************");
        }
    }

    /// <summary>
    /// Dumps the contents of the StreamReader to the Console
    /// </summary>
    /// <param name="r"></param>
    public static void DumpLog(StreamReader r)
    {
        string line;
        while ((line = r.ReadLine()) != null)
        {
            Console.WriteLine(line);
        }
    }

    /// <summary>
    /// Appends the strings in the logMessage array to the ListBoxLMDlog.txt file. A new file is created if it doen't already exist
    /// </summary>
    /// <param name="logMessage"></param>
    public static void WriteListBoxLMDLog(params string[] logMessage)
    {
        using (StreamWriter w = File.AppendText("ListBoxLMDlog.txt"))
        {
           // w.Write("\r\nLog Entry : ");
            w.WriteLine("\r\nLog Entry : {0} {1}\n  :", DateTime.Now.ToLongTimeString(), DateTime.Now.ToLongDateString());
            foreach (var s in logMessage) w.WriteLine(s);
            w.WriteLine("  :\n-------------------------------");
        }
    }
}

