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

https://<IP>/api/Pipflow/spgetListByName?Listname=<pipflow1>
  
  Method type : GET or POST
  
  Request parameters:
  
             Listname : Name of the list 
             
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

https://<IP>api/Pipflow/spsetTaskItemByID?status=<Approved>&percentComplete=<1>&comments=<how%20r%20ou>&taskid=<449>&createdby=<spm>&assignevent=<eventid>
  
  Method type : GET or POST
  
  Request parameters:
  
             status : Approved or Rejected or Assign to MD 
             percentComplete : 1 for 100%
             comments : user comments
             createdby : user login name
             assignevent : send the event id as integer based on 9th API details. 
             note smaple events details : (empty), assigntosnc , assignetojrsnc, assigntoprepdspoc, assigntoprepdspoc , assigntosnctopmcpdspoc, assigntosnctopmcpdspoc,assigntosnctopostpdsdspoc 
 
  Response output: {"Message":"Success"}
             fail:  exception string will send
  
  eg url path : http://localhost:56643/api/Pipflow/spsetTaskItemByID?status=Approved&percentComplete=1&comments=how%20r%20ou&taskid=449&createdby=spm&assignevent=1
  
  live url : http://40.70.16.29:8080/api/Pipflow/spsetTaskItemByID?status=Approved&percentComplete=1&comments=how%20r%20ou&taskid=743&createdby=spm&assignevent=1
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

http://<IP>api/Pipflow/spgetTaskDetails?listname=<tasklist>&Taskuser=<user name>&ReleatedItems=<, separate Main List IDs >
  
  Method type : GET or POST
  
  Request parameters:
  
             Listname : Name of the list 
             Taskuser : Pass user name
             ReleatedItems : ,(coma) separate Main List IDs or single id
             
  Response output:
  
              success: [{"id":"482","title":"Task start and assing by i:0#.w|mylabsp\\spm","status":"Not Started","remarks":null,"taskoutcome":"","RelatedItems":"61","Modified_By":"SharePoint App","Modified_By_id":"1073741822","Created_By":"SharePoint App","Created_By_id":"1073741822","assigned_to":"SPM","assigned_to_id":"26","Modified_Date":null},{"id":"483","title":"Task start and assing by i:0#.w|mylabsp\\spm","status":"Not Started","remarks":null,"taskoutcome":"","RelatedItems":"56","Modified_By":"SharePoint App","Modified_By_id":"1073741822","Created_By":"SharePoint App","Created_By_id":"1073741822","assigned_to":"SPM","assigned_to_id":"26","Modified_Date":null},{"id":"485","title":"Task start and assing by i:0#.w|mylabsp\\spm","status":"Not Started","remarks":null,"taskoutcome":"","RelatedItems":"51","Modified_By":"SharePoint App","Modified_By_id":"1073741822","Created_By":"SharePoint App","Created_By_id":"1073741822","assigned_to":"SPM","assigned_to_id":"26","Modified_Date":null}]
              fail:  exception string will send
  
  eg url path : http://localhost:56643/api/Pipflow/spgetTaskDetails?listname&taskuser=spm&ReleatedItems=61,51,56
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
             eventuser : send the sharepoint/login user name as parameter
             
  Response output:
  
              success: [{"id":"4","title":"SNC department flow","assigned_to":"SNC","assigned_to_id":"29","approved_to_id":"30","approved_to":"SND","rejected_to_id":"28","rejected_to":"CA","flowevent":""},{"id":"10","title":"SNC to MD flow","assigned_to":"SNC","assigned_to_id":"29","approved_to_id":"","approved_to":"","rejected_to_id":"27","rejected_to":"MD","flowevent":"reassign"},{"id":"11","title":"SNC to JR SNC","assigned_to":"SNC","assigned_to_id":"29","approved_to_id":"33","approved_to":"JR SNC","rejected_to_id":"29","rejected_to":"SNC","flowevent":"assignetojrsnc"},{"id":"12","title":"SNC to Pre PDSPOC","assigned_to":"SNC","assigned_to_id":"29","approved_to_id":"34","approved_to":"PrePD SPOC","rejected_to_id":"29","rejected_to":"SNC","flowevent":"assigntoprepdspoc"},{"id":"13","title":"SNC to PM COST PDSPOC ","assigned_to":"SNC","assigned_to_id":"29","approved_to_id":"35","approved_to":"PMCPD SPOC","rejected_to_id":"29","rejected_to":"SNC","flowevent":"assigntosnctopmcpdspoc"},{"id":"14","title":"SNC to Post COST PDSPOC","assigned_to":"SNC","assigned_to_id":"29","approved_to_id":"36","approved_to":"POSTPD SPOC","rejected_to_id":"29","rejected_to":"SNC","flowevent":"assigntosnctopostpdsdspoc"}]
              fail:  Error message
Eg link: 
http://localhost:56643/api/Pipflow/spgetuserinfo?uname=spm&pwd=pip@123

live: http://40.70.16.29:8080/api/Pipflow/spgetWFEventDetailsByUser?listname&eventuser=snc
# end 
