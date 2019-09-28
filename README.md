# SharpHydrus
A CSharp API lib for the Hydrus Client API


# Example
	Hydrus.API_KEY = "API KEY";
	Hydrus.Server_Addr = "http://ServerIpOrDomain.com:45869";
	
	//basic api stuff
	Console.WriteLine("API Version: " + Hydrus.Get_Api_Version().Version);
	Console.WriteLine("Session Key: " + Hydrus.Get_Session_Key().Session_Key);
	int[] permissionarray = { 3 };
	Console.WriteLine("Requested session key with permission 3: " + Hydrus.Request_New_Permissions("Test Permission", permissionarray).Access_Key);

	//check what the heck we can do
	Console.WriteLine("Permissions array: [" + string.Join(",", Hydrus.Verify_Access_Key().Basic_permissions) + "]");
	Console.WriteLine("Permission Translation: " + Hydrus.Verify_Access_Key().Human_description);

	//TODO: Adding files 

	//TODO: Adding Tags 

	//TODO: Adding URLs 

	//TODO: Cookie handeling

	//TODO: Page managing

	//TODO: Searching Files, this is high critical work we do here son. The search never ends.
	Console.WriteLine("Thumb Name: " + Hydrus.Get_Files_Thumbnails(FILETYPE.File_ID, "7917947").FileName);
	Console.WriteLine(" File Name: " + Hydrus.Get_Files_File(FILETYPE.File_ID, "7917947").FileName);

	//That's all folks.
