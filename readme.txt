Release Notes for Script ChangeNamespaceOnComponents
================================

Version:
========
This script is intended only for Tridion 2013.

Description:
============
This is a script for:
- Updating component field namespaces when a schema's targetNamespace is changed.

Installation:
=============
.NET Environment Content Manager Server:
1. Ensure a backup of the Content Manager database has been taken.
2. Copy the distributed ChangeNamespaceOnComponents.exe and ChangeNamespaceOnComponents.exe.config inside the <TridionInstall>\Bin\Client folder.
3. After running the script, remove the two files from the <TridionInstall>\Bin\Client folder.

Distributed files:
==================
ChangeNamespaceOnComponents.exe
ChangeNamespaceOnComponents.exe.config

Dependencies:
=============
Potential:
------------
Tridion.Common.dll
Tridion.ContentManager.Core.Client.dll

Required:
------------
The script must be run on the Content Manager Server.
The user running the script must be an Administrator account in the Tridion CMS.
The user running the script must have permissions to write to the <TridionInstall>\Bin\client folder such that the log file can be created/edited.
All components being updated must be checked-in prior to running the script.

Fixed problems:
===============
Problem Description
-------------------
When changing a schema targetNamespace in 2013, the content fields of components based on the same schema do not automatically update with the new namespace when opening, saving and closing components. 

Script Description
------------------
This script will update component (and metadata) field namespaces if there was a change applied to the targetNamespace of the schema they are based on. 
The script will also update other items such as folders, pages, structure groups.
In this way each component is up to date and will allow Saving and Closing, and subsequent content updates.
The script runs as a console application and When running status is provided in both the console window as well as logged to a file created in the same folder. 

Script Use
------------------
1. Open a command prompt.
2. Navigate to the <TridionInstall>\bin\client folder.
3. Type the application name ChangeNamespaceOnComponents followed by the tcm uri of the schema with the namespace change.
For example (prompt>>>ChangeNamespaceOnComponents tcm:5-555-8).

The script will proceed to run and countdown all components based on the schema uri supplied.
The results are displayed in the window prompt as well as logged to ChangeNamespaceOnComponents.log file. 
The log is cumulative, it should be monitored for size and deleted when finished and if not needed.

** When verifying content updates in Tridion Content Manager Explorer, ensure the browser is refreshed by re-issuing an F5 command or closing and er-opening the client browser.


Disclaimer:
===========
Scripts and/or hotfixes are released at the discretion of Tridion based on technical
complexity, customer business requirements and schedules. Scripts and/or hotfixes are made
and tested only for the described problem on a particular environment/
configuration and therefore should only be installed if approved by Tridion
Customer Support. Scripts and/or hotfixes should be replaced as soon as possible by the
subsequent service pack where the problem is fixed.

(c) 2015 SDL Tridion B.V.