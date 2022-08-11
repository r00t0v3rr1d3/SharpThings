using System;
using Microsoft.Win32;
using System.ComponentModel;

namespace WindowsRegistry
{
    class Reg
    {
        static string[] parseHive(string path)
        {
            string[] results = new string[3];
            if (path.StartsWith("\\\\"))
            {
                string trimstart = path.Substring(2);
                int position = trimstart.IndexOf("\\");
                results[0] = trimstart.Substring(0, position);
                string nextchunk = trimstart.Substring(position+1);
                int slashposition = nextchunk.IndexOf("\\");
				if (slashposition == -1)
				{
					string hive = nextchunk;
					results[1] = hive.ToUpper();
					results[2] = "\\";
				}
				else
				{
					string hive = nextchunk.Substring(0, slashposition);
					results[1] = hive.ToUpper();
					results[2] = nextchunk.Substring(slashposition+1);
				}

            }
            else
            {
                results[0] = "local";
                int slashposition = path.IndexOf("\\");
				if (slashposition == -1)
				{
					string hive = path;
					results[1] = hive.ToUpper();
					results[2] = "\\";
				}
				else
				{
					string hive = path.Substring(0, slashposition);
					results[1] = hive.ToUpper();
					results[2] = path.Substring(slashposition+1);
				}
            }
            return results;
        }
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                System.Console.WriteLine("Invalid arguments");
                System.Environment.Exit(-1);
            }
            else
            {
                string action;
                if (args[0].ToUpper().Equals("QUERY"))
                {
                    string[] targetandhive = parseHive(args[1]);
					
					string regvalue = "";
					if (args.Length > 2)
					{
						if (args[2].ToUpper().Equals("/V"))
						{
							if (args.Length > 3)
							{
								regvalue = args[3];
							}
							else
							{
								Console.WriteLine("Invalid arguments: /v detected with no associated value");
								System.Environment.Exit(-1);
							}
						}
					}
					
                    try
                    {
                        string location = targetandhive[2];

                        RegistryKey key;
                       
					    if (targetandhive[1].Equals("HKCR"))
						{
							if (targetandhive[0].Equals("local"))
							{
								key = Registry.ClassesRoot.OpenSubKey(location);
							}
							else
							{
								key = RegistryKey.OpenRemoteBaseKey(RegistryHive.ClassesRoot, targetandhive[0]);
								key = key.OpenSubKey(location);
							}
						}
						else if (targetandhive[1].Equals("HKCU"))
						{
							if (targetandhive[0].Equals("local"))
							{
								key = Registry.CurrentUser.OpenSubKey(location);
							}
							else
							{
								key = RegistryKey.OpenRemoteBaseKey(RegistryHive.CurrentUser, targetandhive[0]);
								key = key.OpenSubKey(location);
							}
						}
						else if (targetandhive[1].Equals("HKLM"))
						{
							if (targetandhive[0].Equals("local"))
							{
								key = Registry.LocalMachine.OpenSubKey(location);
							}
							else
							{
								key = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, targetandhive[0]);
								key = key.OpenSubKey(location);
							}
						}
						else if (targetandhive[1].Equals("HKU"))
						{
							if (targetandhive[0].Equals("local"))
							{
								key = Registry.Users.OpenSubKey(location);
							}
							else
							{
								key = RegistryKey.OpenRemoteBaseKey(RegistryHive.Users, targetandhive[0]);
								key = key.OpenSubKey(location);
							}
						}
						else if (targetandhive[1].Equals("HKCC"))
						{
							if (targetandhive[0].Equals("local"))
							{
								key = Registry.CurrentConfig.OpenSubKey(location);
							}
							else
							{
								key = RegistryKey.OpenRemoteBaseKey(RegistryHive.CurrentConfig, targetandhive[0]);
								key = key.OpenSubKey(location);
							}
						}
						else
						{
							if (targetandhive[0].Equals("local"))
							{
								key = Registry.LocalMachine.OpenSubKey(location);
							}
							else
							{
								key = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, targetandhive[0]);
							}
						}
						string output = "";
						if (regvalue.Length == 0)
						{
							output += "\n" + targetandhive[1] + "\\" + location + "\n";
							foreach (var w in key.GetValueNames())
							{
								if (key.GetValueKind(w).ToString() == "Binary")
								{
									byte[] binarydata = (byte[]) key.GetValue(w);
									string binarydatastring = "";
									foreach (var b in binarydata)
									{
										int bytedata = Int32.Parse(b.ToString());
										string hexbytedata = bytedata.ToString("X");
										if (hexbytedata.Length == 1)
										{
											binarydatastring += "0";
											binarydatastring += hexbytedata;
										}
										else
										{
											binarydatastring += hexbytedata;
										}
									}
									output += "    " + w + "    " + key.GetValueKind(w) + "    " + binarydatastring + "\n";
								}
								else if (key.GetValueKind(w).ToString() == "MultiString")
								{
									string[] multistringdata = (string[]) key.GetValue(w);
									string multistringoutput = "";
									foreach (var s in multistringdata)
									{
										multistringoutput += s;
									}
									output += "    " + w + "    " + key.GetValueKind(w) + "    " + multistringoutput + "\n";
								}
								else
								{
									output += "    " + w + "    " + key.GetValueKind(w) + "    " + key.GetValue(w) + "\n";
								}
							}
							output += "\n";
							foreach (var v in key.GetSubKeyNames())
							{
								output += targetandhive[1] + "\\" + location + "\\" + v + "\n";
							}
						}
						else
						{
							output += "\n" + targetandhive[1] + "\\" + location + "\n";

							if (key.GetValueKind(regvalue).ToString() == "Binary")
							{
								byte[] binarydata = (byte[]) key.GetValue(regvalue);
								string binarydatastring = "";
								foreach (var b in binarydata)
								{
									int bytedata = Int32.Parse(b.ToString());
									string hexbytedata = bytedata.ToString("X");
									if (hexbytedata.Length == 1)
									{
										binarydatastring += "0";
										binarydatastring += hexbytedata;
									}
									else
									{
										binarydatastring += hexbytedata;
									}
								}
								output += "    " + regvalue + "    " + key.GetValueKind(regvalue) + "    " + binarydatastring + "\n";
							}
							else if (key.GetValueKind(regvalue).ToString() == "MultiString")
							{
								string[] multistringdata = (string[]) key.GetValue(regvalue);
								string multistringoutput = "";
								foreach (var s in multistringdata)
								{
									multistringoutput += s;
								}
								output += "    " + regvalue + "    " + key.GetValueKind(regvalue) + "    " + multistringoutput + "\n";
							}
							else
							{
								output += "    " + regvalue + "    " + key.GetValueKind(regvalue) + "    " + key.GetValue(regvalue) + "\n";
							}
						}
                        
                        key.Close();
						Console.WriteLine(output);
                    }
					catch (System.Security.SecurityException)
					{
						Console.WriteLine("Access denied.");
					}
					catch (System.NullReferenceException)
					{
						Console.WriteLine("Specified key does not exist.");
					}
					catch (System.UnauthorizedAccessException)
					{
						Console.WriteLine("Logon failure: unknown user name or bad password.");
					}
					catch (System.IO.IOException)
					{
						Console.WriteLine("ERROR: The network address is invalid.");
					}
                    catch (Exception ex)
                    {
						Console.WriteLine("Unexpected error.\n" + ex);
                    }
                }
                else if (args[0].ToUpper().Equals("ADD"))
                {
					if (args.Length != 6)
					{
						Console.WriteLine("Invalid arguments");
						System.Environment.Exit(-1);
					}
					else
					{
						if ((args[2].ToUpper().Equals("/V")) && (args[4].ToUpper().Equals("/D")))
						{
							action = "add";
							string[] targetandhive = parseHive(args[1]);
							try
							{
								string location = targetandhive[2];
								RegistryKey key;
								
								if (targetandhive[1].Equals("HKCR"))
								{
									if (targetandhive[0].Equals("local"))
									{
										key = RegistryKey.OpenBaseKey(RegistryHive.ClassesRoot, RegistryView.Default);
									}
									else
									{
										key = RegistryKey.OpenRemoteBaseKey(RegistryHive.ClassesRoot, targetandhive[0]);
									}
								}
								else if (targetandhive[1].Equals("HKCU"))
								{
									if (targetandhive[0].Equals("local"))
									{
										key = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default);
									}
									else
									{
										key = RegistryKey.OpenRemoteBaseKey(RegistryHive.CurrentUser, targetandhive[0]);
									}
								}
								else if (targetandhive[1].Equals("HKLM"))
								{
									if (targetandhive[0].Equals("local"))
									{
										key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default);
									}
									else
									{
										key = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, targetandhive[0]);
									}
								}
								else if (targetandhive[1].Equals("HKU"))
								{
									if (targetandhive[0].Equals("local"))
									{
										key = RegistryKey.OpenBaseKey(RegistryHive.Users, RegistryView.Default);
									}
									else
									{
										key = RegistryKey.OpenRemoteBaseKey(RegistryHive.Users, targetandhive[0]);
									}
								}
								else if (targetandhive[1].Equals("HKCC"))
								{
									if (targetandhive[0].Equals("local"))
									{
										key = RegistryKey.OpenBaseKey(RegistryHive.CurrentConfig, RegistryView.Default);
									}
									else
									{
										key = RegistryKey.OpenRemoteBaseKey(RegistryHive.CurrentConfig, targetandhive[0]);
									}
								}
								else
								{
									if (targetandhive[0].Equals("local"))
									{
										key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default);
									}
									else
									{
										key = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, targetandhive[0]);
									}
								}
								
								key = key.CreateSubKey(location);
								key.SetValue(args[3], args[5], RegistryValueKind.String);
								Console.WriteLine("The operation completed successfully.");
								key.Close();
							}
							catch (System.UnauthorizedAccessException)
							{
								Console.WriteLine("Access denied OR Logon failure: unknown user name or bad password.");
							}
							catch (System.IO.IOException)
							{
								Console.WriteLine("ERROR: The network address is invalid.");
							}
							catch (Exception ex)
							{
								Console.WriteLine("Unexpected error.\n" + ex);
							}
						}
						else
						{
							Console.WriteLine("Invalid arguments. Check /v and /d");
							System.Environment.Exit(-1);
						}
					}
                }
                else if (args[0].ToUpper().Equals("DELETE"))
                {
                    action = "delete";
					string[] targetandhive = parseHive(args[1]);
					string regvalue = "";
					if (args.Length > 2)
					{
						if (args[2].ToUpper().Equals("/V"))
						{
							if (args.Length > 3)
							{
								regvalue = args[3];
							}
							else
							{
								Console.WriteLine("Invalid arguments: /v detected with no associated value");
								System.Environment.Exit(-1);
							}
						}
					}
					
					try
                    {
                        string location = targetandhive[2];
						string subkey = "";
						if (regvalue.Length == 0)
						{
							int lastslash = location.LastIndexOf("\\");
							subkey = location.Substring(lastslash+1);
							location = location.Substring(0, lastslash);	
						}

                        RegistryKey key;
                        if (targetandhive[1].Equals("HKCR"))
                        {
                            if (targetandhive[0].Equals("local"))
                            {
                                key = Registry.ClassesRoot.OpenSubKey(location, true);
                            }
                            else
                            {
                                key = RegistryKey.OpenRemoteBaseKey(RegistryHive.ClassesRoot, targetandhive[0]);
                                key = key.OpenSubKey(location, true);
                            }
                        }
                        else if (targetandhive[1].Equals("HKCU"))
                        {
                            if (targetandhive[0].Equals("local"))
                            {
                                key = Registry.CurrentUser.OpenSubKey(location, true);
                            }
                            else
                            {
                                key = RegistryKey.OpenRemoteBaseKey(RegistryHive.CurrentUser, targetandhive[0]);
                                key = key.OpenSubKey(location, true);
                            }
                        }
                        else if (targetandhive[1].Equals("HKLM"))
                        {
                            if (targetandhive[0].Equals("local"))
                            {
                                key = Registry.LocalMachine.OpenSubKey(location, true);
                            }
                            else
                            {
                                key = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, targetandhive[0]);
                                key = key.OpenSubKey(location, true);
                            }
                        }
                        else if (targetandhive[1].Equals("HKU"))
                        {
                            if (targetandhive[0].Equals("local"))
                            {
                                key = Registry.Users.OpenSubKey(location, true);
                            }
                            else
                            {
                                key = RegistryKey.OpenRemoteBaseKey(RegistryHive.Users, targetandhive[0]);
                                key = key.OpenSubKey(location, true);
                            }
                        }
                        else if (targetandhive[1].Equals("HKCC"))
                        {
                            if (targetandhive[0].Equals("local"))
                            {
                                key = Registry.CurrentConfig.OpenSubKey(location, true);
                            }
                            else
                            {
                                key = RegistryKey.OpenRemoteBaseKey(RegistryHive.CurrentConfig, targetandhive[0]);
                                key = key.OpenSubKey(location, true);
                            }
                        }
                        else
                        {
                            if (targetandhive[0].Equals("local"))
                            {
                                key = Registry.LocalMachine.OpenSubKey(location, true);
                            }
                            else
                            {
                                key = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, targetandhive[0]);
                            }
                        }
						
						if (regvalue.Length > 0)
						{
							key.DeleteValue(regvalue);
						}
						else
						{
							key.DeleteSubKeyTree(subkey);
						}
						Console.WriteLine("The operation completed successfully.");
						key.Close();
					}
					catch (System.Security.SecurityException)
					{
						Console.WriteLine("Access denied.");
					}
					catch (System.ArgumentException)
					{
						Console.WriteLine("Specified key does not exist.");
					}
					catch (System.UnauthorizedAccessException)
					{
						Console.WriteLine("Logon failure: unknown user name or bad password.");
					}
					catch (System.IO.IOException)
					{
						Console.WriteLine("ERROR: The network address is invalid.");
					}
                    catch (Exception ex)
                    {
						Console.WriteLine("Unexpected error.\n" + ex);
                    }
                }
                else
                {
                    action = args[0];
                    Console.WriteLine("Invalid action:" + action);
                    System.Environment.Exit(-1);
                }

                System.Environment.Exit(0);
            }
        }
    }
}