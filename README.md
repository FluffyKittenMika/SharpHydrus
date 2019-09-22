# SharpHydrus
A CSharp API lib for the Hydrus Client API


# Example
	Hydrus.API_KEY = "API KEY";
	Hydrus.Server_Addr = "http://ServerIpOrDomain.com:45869";
	
	//basic api stuff
	Console.WriteLine(Hydrus.Get_Api_Version().Result.Version);
	Console.WriteLine(Hydrus.Get_Session_Key().Result.Session_Key);
	Console.WriteLine("[" + string.Join(",", Hydrus.Verify_Access_Key().Result.Basic_permissions) + "]");
	Console.WriteLine(Hydrus.Verify_Access_Key().Result.Human_description);

	//TODO: Adding files 

	//TODO: Adding Tags 

	//TODO: Adding URLs 

	//TODO: Cookie handeling

	//TODO: Page managing

	//TODO: Searching Files, this is high critical work we do here son. The search never ends.
	Console.WriteLine(Hydrus.Get_Files_Thumbnails(FILETYPE.File_ID, "7917947").Result.FileName);
