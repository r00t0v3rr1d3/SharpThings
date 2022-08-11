using System;
using System.Text;
using System.Collections.Generic;
using System.Security.Principal;
using System.DirectoryServices;

public class DSQuery
{
	public static void Main(string[] args)
	{
		int argumentsize = args.Length;
		int sizelimit = 100;
		string filter = "";
		string targetserver = "";
		string searchroot = "";
		bool adspathincluded = true;
		List<string> listAttrs = new List<string>();
		
		if (argumentsize > 1)
		{
			if (args[0].Equals("*"))
			{
				searchroot = "";
			}
			else
			{
				searchroot = args[0];
			}
		}
		else
		{
			Console.WriteLine("Usage: dsquery * -filter <filter> -attr <* or individual attrs separated by spaces> -limit <number> [-s <ip> or -d <name>]\nNo need to use -l, that is only output format.\n-s or -d is NOT required\nDefault limit is 100\n\nExample: dsquery * -filter \"(&(objectclass=group)(name=*admin*))\" -attr name adspath -limit 5 -s 10.10.1.10");
			System.Environment.Exit(-1);
		}
		
		try
		{
			int counter = 1;
			while (counter < argumentsize)
			{
				if (args[counter].Equals("-filter"))
				{
					filter = args[counter+1];
					counter +=2;
				}
				else if (args[counter].Equals("-attr"))
				{
					counter += 1;
					
					if (args[counter].Equals("*"))
					{
						listAttrs.Add("*");
						counter += 1;
					}
					else
					{
						int attrcounter = 0;
						for (int z = counter; z < argumentsize; z++)
						{
							if (args[z].IndexOf("-") != 0)
							{
								listAttrs.Add(args[z]);
								attrcounter += 1;
							}
							else
							{
								break;
							}
						}
						if (listAttrs.Contains("adspath"))
						{
							adspathincluded = true;
						}
						else
						{
							adspathincluded = false;
						}
						counter = counter + attrcounter;
					}
				}
				else if (args[counter].Equals("-limit"))
				{
					counter += 1;
					sizelimit = Int32.Parse(args[counter]);
					counter += 1;
				}
				else if (args[counter].Equals("-s"))
				{
					counter += 1;
					targetserver = args[counter];
					counter += 1;
				}
				else if (args[counter].Equals("-d"))
				{
					counter += 1;
					targetserver = args[counter];
					counter += 1;
				}
				else
				{
					Console.WriteLine("Error parsing arguments.");
					System.Environment.Exit(-1);
				}
			}
		}
		catch (System.IndexOutOfRangeException)
		{
			Console.WriteLine("Error parsing arguments.");
			System.Environment.Exit(-1);
		}
		
		string[] attrs = listAttrs.ToArray();
		
		DirectoryEntry de;
		if (targetserver.Equals(""))
		{
			de = new DirectoryEntry();
		}
		else
		{
			string ldapAddress = "LDAP://" + targetserver;
			de = new DirectoryEntry(ldapAddress);
		}
		
		DirectorySearcher ds = new DirectorySearcher(de);
		ds.PageSize = 1000;
		ds.Filter = filter;
		ds.SizeLimit = sizelimit;

		if (!attrs[0].Equals("*"))
		{
			foreach (string tempattr in attrs)
			{
				ds.PropertiesToLoad.Add(tempattr);
			}
		}
        ds.SearchScope = SearchScope.Subtree;
		try
		{
			SearchResultCollection src = ds.FindAll();
			string results = "";
			foreach (SearchResult sr in src)
			{
				ResultPropertyCollection myResultPropColl = sr.Properties;
				foreach (string myKey in myResultPropColl.PropertyNames)
				{
					foreach (Object myCollection in myResultPropColl[myKey])
					{
						if (myKey.Equals("objectsid"))
						{
							SecurityIdentifier si = new SecurityIdentifier((byte[])sr.Properties[myKey][0], 0);
							string bytesvalue = si.ToString();
							results += myKey + ": " + bytesvalue + "\n";
						}
						else if (myKey.Equals("objectguid"))
						{
							Guid guid = new Guid((byte[])sr.Properties[myKey][0]);
							string bytesvalue = guid.ToString();
							results += myKey + ": " + bytesvalue + "\n";
						}
						else if (myKey.Equals("adspath"))
						{
							if (adspathincluded)
							{
								results += myKey + ": " + myCollection + "\n";
							}
						}
						else
						{
							results += myKey + ": " + myCollection + "\n";
						}
					}
				}
			}
			
			if (src.Count == sizelimit && sizelimit != 0)
			{
				Console.WriteLine("\n" + results + "\nDsquery has reached the specified limit (" + sizelimit + ") on number of results to display; use a different value for the -limit option to display more results.");
			}
			else if (src.Count == 0)
			{
				Console.WriteLine("Invalid query or no results returned.");
			}
			else
			{
				Console.WriteLine("\n" + results);
			}
		}
		catch (System.Runtime.InteropServices.COMException excep)
		{
			string error = excep.ToString();
			if (error.Contains("unknown user name or bad password"))
			{
				Console.WriteLine("Logon Failure: unknown user name or bad password.");
			}
			else if (error.Contains("The server is not operational"))
			{
				Console.WriteLine("The server is not operational.");
			}
			else if (error.Contains("The specified domain either does not exist or could not be contacted"))
			{
				Console.WriteLine("The specified domain either does not exist or could not be contacted.");
			}
			else
			{
				Console.WriteLine(error);
			}
			System.Environment.Exit(-1);
		}
	}
}
