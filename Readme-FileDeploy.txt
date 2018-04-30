1. Download PublishOutput folder from Git repository
2. Run IIS Manager > Find 2279 Site in the left pane > Stop site
3. Copy the contents of the PublishOutput folder to c:\2279 folder
4. Start 2279 Site in IIS Manager


DB Restore

After DB resore we can get error: Cannot open database requested by the login. The login failed.
Login failed for user ‘NT AUTHORITY\NETWORK SERVICE’

Please see link to fix it:
https://blog.sqlauthority.com/2009/08/20/sql-server-fix-error-cannot-open-database-requested-by-the-login-the-login-failed-login-failed-for-user-nt-authoritynetwork-service/
