# AzureBatchDependenciesStorage
A sample app demonstration of Azure Batch Apps Task Decencies and specifying Intermediate output file as Required Files. For a full description of the sample and some of its code please refer to the corresponding article on this blog ( http://sarkar.azurewebsites.net/2015/05/18/deploy-azure-batch-apps-through-batch-apps-portal/ ).

Included Projects
-----------------

 AzureBatchDependenciesStorage includes only one project:

*  AzureBatchDependenciesStorage - Azure Batch Cloud Assembly. 

Building the Cloud Assembly
---------------------------
You need to specify the access details of Azure storage account  associated with your azure batch app service. You will find the details on how to find out this azure storage details here ( http://sarkar.azurewebsites.net/2015/03/16/uploading-large-executable-on-azure-batch-service-app-management-portal/ ).
To build the cloud assembly zip file:

1. Add Azure Storage access details to DownloadFile method in TaskProcessor.cs file in  the AzureBatchDependenciesStorage project.
2. Build the AzureBatchDependenciesStorage project.
2. Open the output folder of the AzureBatchDependenciesStorage project.
3. Select all the DLLs (and optionally PDB files) in the output folder.
4. Right-click and choose Send To > Compressed Folder.
                          

Getting the Application Image
------------------------------
To build the application image zip file:

1. The dummy test application image ( mri-processing-dummy.zip ) is included in this github repository.

Uploading the Application to Your Batch Apps Service
----------------------------------------------------
1. Open the Azure management portal (manage.windowsazure.com).
2. Select Batch Services in the left-hand menu.
3. Select your service in the list and click "Manage Batch Apps."  This opens the Batch Apps management 
   portal.
4. Select Services in the left-hand menu.
5. Select your service in the list and click View Details.
6. Choose the Manage Applications tab.
7. Click New Application.
8. Under "Select and upload a cloud assembly," choose your cloud assembly zip file and click Upload.
9. Under "Select and upload an application image," choose your application image zip file and click Upload.  
   (Be sure to leave the version as "default".)
10. Click Done.
                          
Running Jobs from Azure Batch Apps portal
----------------------------------------------------
1. Open the Azure management portal (manage.windowsazure.com).
2. Select Batch Services in the left-hand menu.
3. Select your service in the list and click "Manage Batch Apps." This opens the Batch Apps management portal.
4. Select Services in the left-hand menu.
5. Select your service in the list and click View Details.
6. Choose the Manage Applications tab.
7. Click Run Jobs.
8. Enter Job Name, select Job Type and enter any parameters your job. 
9. Under "Select the input files for your job" choose your input file. For simplicity, this sample project requires that these two files have these names (brain.nii and tissue.nii) . The GitHub project contains a zip file ( TestInputFiles.zip ) containing two input files and those files could be used here. 
10. Then you could start the job.

Expected Result of the Application once Job is finished
----------------------------------------------------
1. Final Job output is a file named completion.txt which indicates that job is done
2. Each of the four copies the content of input files to new output files ( resliced_tissue.nii, resliced_brain.nii ,skull-stripped_tissue.nii and skull-stripped_brain.nii  ).
 
