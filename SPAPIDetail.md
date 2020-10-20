Note: this API  hosted in http://40.70.16.29:8080/api/Pipflow/spcheckuser?uname=&pwd=pip@123

so replace localhost:56643 with 40.70.16.29:8080

1.
# Validate user from sharepoint site url

https://<IP>/api/Pipflow/spcheckuser?uname=<spm>&pwd=<pip@123>
  
  Method type : GET or POST
  
  Request parameters:
  
             uname: user name
             pwd : password of the user
  Response output:
  
              success: true
              fail:  false
  
# end 

2.
# Get the sharepont list details based on list name

https://<IP>/api/Pipflow/spgetListByName?Listname={Listname}&status={status}
  
  Method type : GET or POST
  
  Request parameters:
  
             Listname : Name of the list 
             status : FMR list status need to be send
             
  Response output:
  
              success: "[{\"id\":\"37\",\"title\":\"testsdf\",\"status\":\"\",\"remarks\":\"\",\"taskoutcome\":null,\"Modified_By\":null,\"Modified_By_id\":null,\"Created_By\":\"Microsoft.SharePoint.Client.FieldUserValue\",\"Created_By_id\":null,\"assigned_to\":\"Microsoft.SharePoint.Client.FieldUserValue\",\"assigned_to_id\":null,\"Modified_Date\":\"Microsoft.SharePoint.Client.FieldUserValue\"}
              fail:  null
  
  eg url path : http://localhost:56643/api/Pipflow/spgetListByName?listname=pipflow1
# end 

3.
# Set the sharepont list Item cretion based on TItle , remarks and listname

https://<IP>/api/Pipflow/spsetFMR?fmrid=<fmrid>&remarks=<remarks>&Listname=<Listname>
  
  Method type : GET or POST
  
  Request parameters:
  
             fmrid : Send details of FMR      
             remarks : Send remarks
             Listname : Name of the list 
             
  Response output:
  
              success: success
              fail:  exception string will send
  
  eg url path : http://localhost:56643/api/Pipflow/spsetfmr?fmrid=1.1.1.3&remarks=testindf%20&listname=pipflow1
# end 
4.
# Get the sharepont list Item  based on TItle and Item id

https://<IP>api/Pipflow/spgetListItemByID?listname=<pipflow1>&ListitemId=<43>
  
  Method type : GET or POST
  
  Request parameters:
  
             Listname : Name of the list 
             ListitemId : LIst item id
             
  Response output:
  
              success: "[{\"id\":null,\"title\":\"1.1.1.1\",\"status\":\"inprogress\",\"remarks\":\"teswting rom data\",\"taskoutcome\":null,\"Modified_By\":null,\"Modified_By_id\":null,\"Created_By\":\"Microsoft.SharePoint.Client.FieldUserValue\",\"Created_By_id\":null,\"assigned_to\":\"Microsoft.SharePoint.Client.FieldUserValue\",\"assigned_to_id\":null,\"Modified_Date\":\"Microsoft.SharePoint.Client.FieldUserValue\"}]"
              fail:  exception string will send
  
  eg url path : http://localhost:56643/api/Pipflow/spgetListItemByID?listname=pipflow1&ListitemId=43
# end 
5.
# Set the sharepont Task list Item   based on status,percentcomplete,comments,taskid and assignevent

https://<IP>api/Pipflow/spsetTaskItemByID?status=<Approved>&percentComplete=<1>&comments=<how%20r%20ou>&taskid=<449>&createdby=<created by user>&assignevent=<eventid>&assignedto=<assigned to user>&TASKTYPE=1
  
  Method type : GET or POST
  
  Request parameters:
  
             status : Approved or Rejected or Assign to MD 
             percentComplete : 1 for 100%
             comments : user comments
             createdby : current login user from UI or u can take from FMR LIST current assigni
             assignevent : send the event id as integer based on 9th API details. 
             assignedto : send the next approval user based on role to user mapping from UI. 
             TASKTYPE : 1 or 2 or 3 1 for normal flow,2 for additional flow,3 for ROP flow
             areviewuserTo: Need to be send for Aditional or ROP user 
             SPFmrID: send the current FMR ID 
             *Note  : createdby and assignedto parameters should be "user login name" which are mapped to Sharepoint users
 
  Response output: {"Message":"Success"}
             fail:  exception string will send
  
  eg url path : http://localhost:56643/api/Pipflow/spsetTaskItemByID?status=Approved&percentComplete=1&comments=how%20r%20ou&taskid=449&createdby=spm&assignevent=1assignedto=spm
  
  live url : http://40.70.16.29:8080/api/Pipflow/spsetTaskItemByID?status=Approved&percentComplete=1&comments=how%20r%20o&taskid=1008&createdby=md&assignevent=22&assignedto=spm
  
5.1 For Aditional review like sub task style
    
    only sub task assign to user
Eg link: 
http://localhost:44359/api/Pipflow/spsetTaskItemByID?status=approved&percentComplete=1&comments=testing&taskid=593&createdby=spm&assignevent=1&assignedto=&TASKTYPE=2&areviewuserTo=md&SPFmrID=193


    for multi users sub tasks 
Eg link: 
http://localhost:44359/api/Pipflow/spsetTaskItemByID?status=approved&percentComplete=0&comments=testing&taskid=593&createdby=spm&assignevent=1&assignedto=&TASKTYPE=2&areviewuserTo=md,snc,pdspoc,pdspoc2&SPFmrID=193

    For close the sub task plz send subtask taskid and percentC1ompleted send to 1
Eg link: 
http://localhost:44359/api/Pipflow/spsetTaskItemByID?status=approved&percentComplete=1&comments=testing jkhjkhkh &taskid=610&createdby=ca&assignevent=1&assignedto=&TASKTYPE=2&areviewuserTo=&SPFmrID=193


    For close the sub task and assign to respective user plz send subtask taskid and percentC1ompleted send to 1
Eg link: 
http://localhost:44359/api/Pipflow/spsetTaskItemByID?status=approved&percentComplete=1&comments=testing jkhjkhkh &taskid=603&createdby=md&assignevent=1&assignedto=&TASKTYPE=2&areviewuserTo=spm&SPFmrID=193


    FOr get the user wise sub tasks basded areviewuser tasktype 2 
Eg link: 
http://localhost:44359/api/Pipflow/spgetTaskDetails?listname&taskuser=spm&ReleatedItems=&status=Not%20Started&TaskType=2


# end 
6.
# Get the sharepont tasks  based on list and task item id

https://<IP>api/Pipflow/spgetTaskDetailsByuser?listname=<tasklist>&ListitemId=<449>
  
  Method type : GET or POST
  
  Request parameters:
  
             Listname : Name of the list 
             ListitemId : LIst item id
             
  Response output:
  
              success: "[{\"id\":null,\"title\":\"Task start and assing by i:0#.w|mylabsp\\\\spm\",\"status\":\"Approved\",\"remarks\":null,\"taskoutcome\":\"Approved\",\"Modified_By\":\"SPM\",\"Modified_By_id\":\"26\",\"Created_By\":\"SharePoint App\",\"Created_By_id\":\"1073741822\",\"assigned_to\":\"SPM\",\"assigned_to_id\":\"26\",\"Modified_Date\":null}]"
              fail:  exception string will send
  
  eg url path : http://localhost:56643/api/Pipflow/spgetTaskDetailsByuser?listname=tasklist&ListitemId=449
# end 
7.
# Get the sharepont Tasks list Item  based on TItle,Taskuser and ReleatedItems

http://<IP>api/Pipflow/spgetTaskDetails?listname=<tasklist>&Taskuser=<user name>&ReleatedItems=<, separate Main List IDs >&status={status}&TaskTYpe={TaskTYpe}
  
  Method type : GET or POST
  
  Request parameters:
  
             Listname : Name of the list 
             Taskuser : Pass user name
             ReleatedItems : ,(coma) separate Main List IDs or single id
             status : based on rejected/approved/not started task will be getting
             TaskTYpe: 1 for Normal ,2 for Addtional ,3 for ROP
  Response output:
  
              success: [{"id":"482","title":"Task start and assing by i:0#.w|mylabsp\\spm","status":"Not Started","remarks":null,"taskoutcome":"","RelatedItems":"61","Modified_By":"SharePoint App","Modified_By_id":"1073741822","Created_By":"SharePoint App","Created_By_id":"1073741822","assigned_to":"SPM","assigned_to_id":"26","Modified_Date":null},{"id":"483","title":"Task start and assing by i:0#.w|mylabsp\\spm","status":"Not Started","remarks":null,"taskoutcome":"","RelatedItems":"56","Modified_By":"SharePoint App","Modified_By_id":"1073741822","Created_By":"SharePoint App","Created_By_id":"1073741822","assigned_to":"SPM","assigned_to_id":"26","Modified_Date":null},{"id":"485","title":"Task start and assing by i:0#.w|mylabsp\\spm","status":"Not Started","remarks":null,"taskoutcome":"","RelatedItems":"51","Modified_By":"SharePoint App","Modified_By_id":"1073741822","Created_By":"SharePoint App","Created_By_id":"1073741822","assigned_to":"SPM","assigned_to_id":"26","Modified_Date":null}]
              fail:  exception string will send
  
  eg url path : http://localhost:56643/api/Pipflow/spgetTaskDetails?listname&taskuser=spm&ReleatedItems=61,51,56&status=not started
  
  7.1 For getting the Additional review details based on TaskTYpe 2
  
     FOr get the user wise sub tasks basded areviewuser tasktype 2
Eg link: 
      http://localhost:44359/api/Pipflow/spgetTaskDetails?listname&taskuser=spm&ReleatedItems=&status=Not%20Started&TaskType=2  
# end 
8.
# Get user info based on login details 
https://<IP>/api/Pipflow/spgetuserinfo?uname=<spm>&pwd=<pip@123>
  
  Method type : GET or POST
  
  Request parameters:
  
             uname: user name
             pwd : password of the user
  Response output:
  
              success: {"title":"SPM","Id":"26","LoginName":"i:0#.w|mylabsp\\spm","Emailid":null}
              fail:  Error message
Eg link: 
http://localhost:56643/api/Pipflow/spgetuserinfo?uname=spm&pwd=pip@123

# end 

9.
# Get user workflow events details based on user
https://<IP>/api/Pipflow/spgetWFEventDetailsByUser?listname&eventuser=snc
  
  Method type : GET or POST
  
  Request parameters:
  
             listname: optional set to null
             eventuser : send the user role name which is mapped in user creation.
             
  Response output:
  
              success: [{"id":"1","title":"SPM","arole":"MD","rrole":"SPM","flowevent":""}]
              fail:  Error message
Eg link: 
http://localhost:56643/api/Pipflow/spgetuserinfo?uname=spm&pwd=pip@123

live: http://52.172.200.35:2020/sppipapidev/api/Pipflow/spgetWFEventDetailsByUser?Listname=&Eventuser=spm
# end 
10.
# FMR list update for sataus and remarks api post
https://<IP>/api/Pipflow/spupdateFMR?Listname={Listname}&fmrSPid={fmrSPid}&remarks={remarks}&status={status}
  
  Method type : GET or POST
  
  Request parameters:
  
             listname: optional set to null
             fmrspid : send the sharepoint list item ID
             remarks : remarks send by user
             status : status updated by user smaple status are inprogress,completed,"RECALL SNC","RECALL MD"
             
  Response output:
  
              success: success
              fail:  Error message
Eg link: 
http://localhost:56643/api/Pipflow/spupdateFMR?Listname={Listname}&fmrSPid={fmrSPid}&remarks={remarks}&status={status}

live: http://52.172.200.35:2020/sppipapidev/Pipflow/spupdateFMR?Listname={Listname}&fmrSPid={fmrSPid}&remarks={remarks}&status={status}
# end 

11.
# For getting All state group by count based on status & roleids
https://<IP>http://localhost:44359/api/Pipflow/getGroupbyStates?status=not%20started&roleids=5,11
  
  Method type : GET 
  
  Request parameters Mandatory:
  
             status: status of the task 'not started' or rejected or approved
             roleids : roleids separate with , like 5,11, etc
             
             
  Response output:
 #    note: plz use stateid.COUNT.group for group count
              success: { "Row" : 
[{
"stateid": "1",
"stateid.": "1.00000000000000",
"roleid": "5",
"roleid.": "5.00000000000000",
"stateid.urlencoded": "%3B%231%2E00000000000000%3B%23",
"stateid.COUNT.group": "1326",
"stateid.newgroup": "1",
"stateid.groupindex": "1_",
"roleid.urlencoded": "%3B%231%2E00000000000000%3B%235%2E00000000000000%3B%23",
"roleid.COUNT.group2": "731",
"roleid.newgroup": "1",
"roleid.groupindex2": "1_"
}
,{
"stateid": "1",
"stateid.": "1.00000000000000",
"roleid": "11",
"roleid.": "11.0000000000000",
"stateid.urlencoded": "%3B%231%2E00000000000000%3B%23",
"stateid.COUNT.group": "1326",
"stateid.newgroup": "",
"stateid.groupindex": "1_",
"roleid.urlencoded": "%3B%231%2E00000000000000%3B%2311%2E0000000000000%3B%23",
"roleid.COUNT.group2": "595",
"roleid.newgroup": "1",
"roleid.groupindex2": "2_"
}],"FirstRow" : 1,
"LastRow" : 38
,"FilterLink" : "?"
,"ForceNoHierarchy" : "1"
,"HierarchyHasIndention" : ""

}    
fail:  Error message
Eg link: 
http://localhost:56643/api/Pipflow/spupdateFMR?Listname={Listname}&fmrSPid={fmrSPid}&remarks={remarks}&status={status}

live: http://52.172.200.35:2020/sppipapidev/Pipflow/spupdateFMR?Listname={Listname}&fmrSPid={fmrSPid}&remarks={remarks}&status={status}
# end 
12.
# satart bulkpush and call back for pip submit max 2000 fmrs
Bulk push for multiple urls and call back
https://api/Pipflow/BulkPushAPIS

Method type : POST

Request body json:

         [
{ "Title": "sample string 1", "url": "sample string 2", "callbackurl": "sample string 3" }, { "Title": "sample string 1", "url": "sample string 2", "callbackurl": "sample string 3" } ]

Response output:

          success: success
          fail:  Error messag
# end 

# Start Suplimentary only replace controller Pipflow to SupliPipflow
SUPLIMENTARY FMRS same as above all APIS so plz note all signature with replace of controller Pipflow to SupliPipflow
# END suplimentart

# Start Active directory USER start API

# for Active direcotory USER list , add & update APIS

1. api/Pipflow/getADUsers?OUNAMES={OUNAMES}

https://<IP>/api/Pipflow/getADUsers?OUNAMES={OUNAMES}
  
  Method type : GET
  
  Request parameters:
  
             OUNAMES: based on Active directory OUs state or national
             
             
  Response output:
  
              success       {"OU":"STATE","SubOU":null,"UserName":"brajesh","Password":null,"Mobileno":"96508XXXX","Emailid":"varun@dalabs.in","groups":null,"FirstName":"Dr. Beela Rajesh","LastName":"Rajesh","Department":"HFW","ReportingManager":null,"State":null}]
              fail:  Error message
Eg link: 
http://52.172.200.35:2020/sppipapidev/api/Pipflow/getADUsers?OUNAMES=state

2. <IP>/api/Pipflow/ADAddUser

 Method type : GET or POST
  
  Request parameters:
  
             JSON BODY :{"OU":"STATE","SubOU":null,"UserName":"brajesh","Password":null,"Mobileno":"96508XXXX","Emailid":"varun@dalabs.in","groups":null,"FirstName":"Dr. Beela Rajesh","LastName":"Rajesh","Department":"HFW","ReportingManager":null,"State":null}
             POST method 
             
  Response output:
  
              success: success
              fail:  Error message
Eg link: 
http://localhost:56643/api/Pipflow/ADAddUser

live: http://40.70.16.29:8080/api/Pipflow/spgetWFEventDetailsByUser?listname&eventuser=snc

3. <IP>/api/Pipflow/ADUpdateUser

 Method type : GET or POST
  
  Request parameters:
  
             JSON BODY :{"OU":"STATE","SubOU":null,"UserName":"brajesh","Mobileno":"96508XXXX","Emailid":"varun@dalabs.in","groups":null,"FirstName":"Dr. Beela Rajesh","LastName":"Rajesh","Department":"HFW","ReportingManager":null,"State":null}
             POST method 
             
  Response output:
  
              success: success
              fail:  Error message
Note: only mobile number and emailid updating this veriosn,UserName should be Same as user 
Eg link: 
http://localhost:56643/api/Pipflow/ADUpdateUser

live: http://40.70.16.29:8080/api/Pipflow/ADUpdateUser


