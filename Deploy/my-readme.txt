

DEPLOYMENT

1.  IIS Manager and components must be installed (I installed on Win8)
	Control Panel > Programs > Programs and Features > Internet Information Services > Web Management Tools > IIS Management Console
	Control Panel > Programs > Programs and Features > Internet Information Services > World Wide Web Services (All)
	Control Panel > Programs > Programs and Features > Windows Process Activation Service
	Control Panel > Programs > Programs and Features > Internet Information Services > World Wide Web Services > Application Development Features > Check ASP.NET 4.5 and whatever else you might need

2.  Run IIS Manager (Win+x -> Control Panel -> Administrative Tools -> Internet Information Services (IIS) Manager)

3.  Right mouse click on Sites node in left panel -> Add Website

4.  Site name: 2279

5.  Select your phisical path (e.g. C:\2279)

6.  Host Name left empty

7.  Run Command Prompt(Admin) - with Admin rights

8.  TIP! Type CMD in Command Prompt and press Enter (it allow you to see output)

9.  Go to folder with .cmd file. Type or past/copy to Command Prompt: LRC-NET-Framework.deploy.cmd /T & Pause

10. Press Enter. Command Prompt will not be closed and you can check is everything Ok with [/T] test deployment

11. If you passed deploy in test mode (verified the output) run: LRC-NET-Framework.deploy.cmd /Y & Pause

12. Site will be deployed to your phisical path folder (e.g. C:\2279)



DB RESTORE

1.  Please download SQL Server Express (you can choose different versions of SQL Server Express) from Microsoft Download Center and install it.
	I used SQLEXPRESS 2014

2.  Download from the same place SQL Management Studio 2014.

3.  Run SQL Management Studio.

4.  Right click Databases on left pane (Object Explorer)

5.  Click Restore Database...

6.  Choose Device, click ..., and Add LRC.bak file

7.  Click OK, then OK again. LRC DB have to appear in the list of Databases

8.  Find XML Configuration File (web.config) and edit connection string:
    	<add name="LRCEntities" connectionString="metadata=res://*/DBModelContext.csdl|res://*/DBModelContext.ssdl|res://*/DBModelContext.msl;provider=System.Data.SqlClient;provider connection string=&quot;Data Source=THINKSERVER-PC\SQLEXPRESS
	Just change Data Source=THINKSERVER-PC\SQLEXPRESS to yours (e.g. Data Source=LENOVO-Y70\SQLEXPRESS)
	To get your Data Source go to SQL Management Studio. Expand Databases > Right click RLC > Properties > View Connection link > Copy Server Name field. This is your Data Source.

9. Browse site from IIS Manager
	NOTE: I got HTTP Error 500.19 - Internal Server Error.
	If you face this problem, here is how to solve it: http://www.winservermart.com/Howto/HTTP_Error_500_19_IIS_7.aspx
	Open IIS Manager > select root node (the hosting server) > in the middle panel double click Feature Delegation >
	In right panel select Custom Site Delegation... > In upper part of middle panel > 
	click the drop down list and select your site > In Action panel click Reset All Delegation.

10. Let me know if you face with page layout problem in your Explorer. (Bootstrap installation)
