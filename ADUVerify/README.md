ADUVerify application is userd for windows AD(Active directory) user List/Login validate/Creaete windows users

<b>Active Directory info details</b> ref link: https://github.com/knarasimhar/ActiveDIrectoryAPPS/blob/master/README.md

<b>prerequisesions</b>:
windows NT server,
.net framework 4.5 above,
IIS 7,
Activedirectory initialize

Link as below : http://[domainurl]/aduverify

screenshort:
![](images/Screenshot%202020-05-08%20at%201.55.13%20PM.png)

Below are the Link details:

<b>UI Screen 1</b> : For Creating the Active directory user include Groups,manager and department updates

Url as : http://[domainurl]/ADUVerify/home/index

screenshort:

![](images/Screenshot%202020-05-14%20at%2011.23.54%20AM.png)

<b>UI Screen 2</b> : For List the all users based on OU configuration

Url as : http://[domainurl]/ADUVerify/Home/ListADUsers

screenshort:

![](images/Screenshot%202020-05-14%20at%2011.39.59%20AM.png)

<b>UI Screen 3</b> : For verifing the user in Active directory

Url as : http://[domainurl]/ADUVerify/Home/ValidateADUser

screenshort:

![](images/Screenshot%202020-05-14%20at%2011.41.37%20AM.png)



<b>API 1</b> : For list all users from Active directory 

Url as : http://[domainurl]/ADUVerify/Home/ListADUsers

screenshort:

![](images/Screenshot%202020-05-08%20at%202.08.26%20PM.png)

<b>API 2</b>: For validate the respective AD[active directory]  user  

Plz pass paramerters to this url like : [domainurl]/ValidateADUser?uname=xxxx&pwd=xxxx

parameter:
uname: pass the windows AD[active directory] user name
pwd: pass the windows AD[active directory] password

result:

True is for success/False is for not valid/active

screenshort:
![](images/Screenshot%202020-05-08%20at%202.10.11%20PM.png)

<b>API 3</b>: For creating user in remote windows NT server  Active directory 

Plz pass paramerters to this url like : [domainurl]/CreateADUser?ou=&dc=&_dc&uname=xxxx&pwd=xxxx

parameter:

ou: pass empty value
dc: pass empty value
uname: pass the windows AD[active directory] user name
pwd: pass the windows AD[active directory] password

result:

Windows server will give respective output base on username and password

note: Password policy should follow as windows authentication [one cap letter and lengh should be more 8 chars]

<b>Advanced APIS</b>: For user advance details like Group,permission and delegtion control enable in remote windows NT server  Active directory 

screenshort:
![](images/Screenshot%202020-05-14%20at%2011.56.19%20AM.png)

Ignore below:

<h2>Example of code</h2>
<pre>
    <div class="container">
        <div class="block two first">
            <h2>Your title</h2>
            <div class="wrap">
            //Narasimha content
            </div>
        </div>
    </div>
</pre>

