
**PIP Sharepoint Troubleshoot steps**
1. step 1 :
   1.  Login to sharepoint server
   2.  open respective console app location as [D:\Console**\CopyListItemsSsom\Release]
   3.  Refer below image and hilighted file name should not be in folder end with state id 
![](images/step1.PNG)

2. step 2 :

   1. Using below refer image 1 open the highlited path as configured 
   2. Enter into the folders name 'normal' and verfiy Today date json file ends with requested StateID
   3. If 2 step related stateid file not shown plz contact UI team or administrator.
  

![](images/step2.PNG)
![](images/step3.PNG)
![](images/step4.PNG)

3. step 3 :
    1. If step2 works fine verify the windows hilighted task sheduler in running condition or not  
    2. Hightlited task sheduler not working start sheule imediatly
    3. and recheck the step 1 if any state ID file is shown in folder

![](images/step6.PNG)

4. step 4 :
   Open the windows evernt viewer If any applications or system errors occures contact to system administrator refer the below image 
    
![](images/step7.PNG)

5. step 5 :
  Open the windows IIS server and verify all Application pools are starting or not, If not contact to system administrator refer the below image 
 
![](images/step8.PNG)

6. step 6 :

   Open the Sharepoint site is working or not refer below images and step 2 configuration 
 
  
![](images/step9.PNG)

7. step 7 :

   Open the Sharepoint site is working or not refer below images and step 2 configuration 
   
 **PIP UI server to Sharepoint Troubleshoot steps**
1. step 1 :
  Open the windows IIS server and verify all Application pools are starting or not, If not contact to system administrator refer the below image 
 
![](images/step10.PNG)
2. step 2 :
  Explore PIP UI web application folder and verfiy all web.config key value fields are cofigured properly or not, If not contact to system administrator refer the below image 
 
![](images/step11.PNG)
 
3. step 3 :
  Explore SPPIPAPI web application folder and verfiy all web.config key value fields are cofigured properly or not, If not contact to system administrator refer the below image 
 
  
![](images/step12.PNG)

 **PIP ACTIVE DIRECTORY server to Sharepoint Troubleshoot steps**
1. step 1 :
  Explore ADUVerify web application folder and verfiy all web.config key value fields are cofigured properly or not, If not contact to system administrator refer the below image 
 
  
![](images/Step17.PNG)
2. step 1 :
  Explore FileUPLOADAPI web application folder and verfiy all web.config key value fields are cofigured properly or not, If not contact to system administrator refer the below image 
 
  
![](images/step16.PNG)
        
    
    
    
    
    
    
    
    
    
