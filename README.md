# chrome default instalation from power shell

$Path = $env:TEMP; $Installer = "chrome_installer.exe"; Invoke-WebRequest "http://dl.google.com/chrome/install/375.126/chrome_installer.exe" -OutFile $Path\$Installer; Start-Process -FilePath $Path\$Installer -Args "/silent /install" -Verb RunAs -Wait; Remove-Item $Path\$Installer

# sharepointDevelopment
for sharepoint development
# screen shorts for Create sharepoint server 2019

setup process: all are 2019 setups from microsoft 
Best Link for Adding Active directory & DNS server for windows server :

# shrepoint designer best link for setup workflow manager 2013

https://www.c-sharpcorner.com/article/how-to-enable-sharepoint-2013-workflow-in-sharepoint-designer-2013/

# end

https://www.youtube.com/watch?v=h3sxduUt5a8

download Latest sql server 2019

download latest sharepoint 2019   link to watch below link https://www.youtube.com/watch?v=-yYYZWLJ-RU
end of sharepoint setup

VPN Setup for windows server link:

https://www.youtube.com/watch?v=G_k5D-o7CrI

https://www.youtube.com/watch?v=eTzHH8CQX_8

azure setup for VPN

https://www.youtube.com/watch?v=7AAqhsNJWkw

azure setup for VPN 
Azure Basic Setup and Site to Point VPN Setup with Radius Authentication.

https://www.youtube.com/watch?v=mpBxkgtjGfE

screenshort 1 :
![](images/Screenshot%202020-05-15%20at%205.50.09%20PM.png)

screenshort 2 :
![](images/Screenshot%202020-05-15%20at%206.03.39%20PM.png)

screenshort 3 Admin Portal:

![](images/Screenshot%202020-05-15%20at%206.50.16%20PM.png)

screenshort 4 Client Portal:
![](images/Screenshot%202020-05-15%20at%206.42.57%20PM.png)

ENable ananymous access youtube link for public access

https://www.youtube.com/watch?v=s9tigJk95fo


IF active direcoty secronization not work use below link for setup in sharepoint

https://www.c-sharpcorner.com/article/install-and-configure-microsoft-identity-manager-2016-sp1-for-sharepoint-2016/


# very imported app start configuration missed in workflow I have wasted 2 days for implementation below is ref link

https://docs.microsoft.com/en-us/sharepoint/dev/general-development/create-a-workflow-with-elevated-permissions-by-using-the-sharepoint-workflo

# app configuration for Sharepoint apps administrator

https://www.youtube.com/watch?v=TAkQsEt71ns

Learn how to Setup SharePoint App Catalog in On-Premises and Office 365 SharePoint 

Steps in the Process in On-Premises
-----------------------------------------------------------
a) Configure the domain names in DNS
b) Configure the Subscription Settings and App Management service applications
c) Specify the app domain and app prefix 
d) Create new App Catalog site collection

Script for Subscription Settings and App Management service applications
-----------------------------------------------------------------------
$msa = Get-SPManagedAccount -Identity publicisgroupe\spadmin

$App = New-SPServiceApplicationPool -Name "AppManagementAppPool" –Account $msa

$Sub = New-SPSubscriptionSettingsServiceApplication -Name "Subscription Settings Service" -ApplicationPool $App

$SubSP = New-SPSubscriptionSettingsServiceApplicationProxy -ServiceApplication $Sub

More info on Site Collection App Catalog
------------------------------------------------------------------
https://docs.microsoft.com/en-us/shar...

#end flow

# end

# sahrepoint manager app from visual sutdio best link
https://www.youtube.com/watch?v=xBkikKlOIgE&t=5103s
# end 

# start DB maintanace for SHAREPOINT help its very imported
note: we stucked the application because of index rebuilding job not running daily in sharepoint server DB

Below links very helpfull to do this :
  
  https://www.mssqltips.com/sqlservertip/4331/sql-server-index-fragmentation-overview/
  
  https://techcommunity.microsoft.com/t5/core-infrastructure-and-security/sharepoint-database-maintenance-recommendations/ba-p/323370
  
# end 


