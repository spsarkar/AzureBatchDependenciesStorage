using System;
using System.IO;
using Microsoft.Azure.Batch.Apps.Cloud;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureBatchNiftiProcessing
{
    /// <summary>
    /// Processes a task.
    /// </summary>
    public class AzureBatchNiftiProcessingTaskProcessor : ParallelTaskProcessor
    {
        /// <summary>
        /// Executes the external process for processing the task
        /// </summary>
        /// <param name="task">The task to be processed.</param>
        /// <param name="settings">Contains information about the processing request.</param>
        /// <returns>The result of task processing.</returns>
        protected override TaskProcessResult RunExternalTaskProcess(ITask task,
            TaskExecutionSettings settings)
        {
            switch (task.TaskId)
            {
                case TaskIds.Reslice:
                    return RunResliceTask(task, "resliced_brain.nii");
                case TaskIds.Reslice1:
                    return RunResliceTask(task, "resliced_tissue.nii");
                case TaskIds.SkullStrip:
                   return RunSkullStripTask(task, "resliced_brain.nii");
                case TaskIds.SkullStrip1:
                    return RunSkullStripTask(task, "resliced_tissue.nii");
            }

            throw new ArgumentException(string.Format("No such task: {0}.", task), "task");
        }

        private TaskProcessResult RunResliceTask(ITask task, string strOutputFileName)
        {
            var originalInputFileName = task.RequiredFiles[0].Name;
            var inputFile = LocalPath(originalInputFileName);
            string commandPathStr =  ExecutablePath(@"mri-processing\niftiInit.bat");
            string strExecutableLocation = Path.GetDirectoryName(ExecutablesPath);
            var outputFile = Path.Combine(strExecutableLocation, strOutputFileName);

            string externalProcessArgs = string.Format("\"{0}\" \"{1}\"", inputFile.Replace(".nii", string.Empty), outputFile.Replace(".nii", string.Empty));

            var process = new ExternalProcess
            {
                CommandPath = commandPathStr,
                Arguments = externalProcessArgs,
                WorkingDirectory = LocalStoragePath
            };
            process.EnvironmentVariables["PATH"] = string.Format("{0};{1}", "%PATH%", ExecutablePath(@"mri-processing\bin"));

            //Log.Info("Calling '{0}' with Args '{1}' for Task '{2}' / Job '{3}' .", commandPathStr, externalProcessArgs, task.TaskId, task.JobId);

            var processOutput = process.Run();
            return TaskProcessResult.FromExternalProcessResult(processOutput, outputFile);           
        }

        private TaskProcessResult RunSkullStripTask(ITask task, string strInputFileName)
        {
            var originalInputFileName = task.RequiredFiles[0].Name;
            var outputFile = LocalPath("skull-stripped_tissue.nii");
            if(strInputFileName.Contains("brain"))
            {
                outputFile = LocalPath("skull-stripped_brain.nii");
            }
           
            string blobContainerName = "job-" + task.JobId.ToString();
            string inputFile = DownloadFile(blobContainerName, strInputFileName, LocalStoragePath);

            string commandPathStr = ExecutablePath(@"mri-processing\skullStrip.bat");
            string strExecutableLocation = Path.GetDirectoryName(ExecutablesPath);
            string externalProcessArgs = string.Format("\"{0}\" \"{1}\"", inputFile.Replace(".nii", string.Empty),outputFile.Replace(".nii", string.Empty)) ;
                        
            var process = new ExternalProcess
            {
                CommandPath = commandPathStr,
                Arguments = externalProcessArgs,
                WorkingDirectory = LocalStoragePath
            };
            process.EnvironmentVariables["PATH"] = string.Format("{0};{1}", "%PATH%", ExecutablePath(@"mri-processing\bin"));
            Log.Info("Calling '{0}' with Args '{1}' for Task '{2}' / Job '{3}' .", commandPathStr, externalProcessArgs, task.TaskId, task.JobId);

            var processOutput = process.Run();
            return TaskProcessResult.FromExternalProcessResult(processOutput, outputFile);
        }

        private string  DownloadFile(string blobContainerName, string blobName, string targetFolder)
        {
            Log.Info("blobContainerName = '{0}' ,blobName = '{1}' ,targetFile = '{2}'", blobContainerName, blobName, targetFolder);
            string strFileDownloaded = string.Empty;

            string storageAccountName = "[STORAGE ACCOUNT NAME ASSOCIATED WITH AZURE BATCH APP SERVICE]";
            string storageAccountKey = "[STORAGE ACCOUNT KEY]";
            string connectionString = string.Format(@"DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}",
            storageAccountName, storageAccountKey);

            //get a reference to the container where you want to put the files
            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(connectionString);
            CloudBlobClient cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
            CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference(blobContainerName);
            CloudBlockBlob blobSource = cloudBlobContainer.GetBlockBlobReference(blobName);

            if (blobSource.Exists())
            {
                //blob storage uses forward slashes, windows uses backward slashes; do a replace
                // so localPath will be right
                string localDestination = Path.Combine(targetFolder, blobSource.Name.Replace(@"/", @"\"));
                //if the directory path matching the "folders" in the blob name don't exist, create them
                string dirPath = Path.GetDirectoryName(localDestination);
                if (!Directory.Exists(localDestination))
                {
                    Directory.CreateDirectory(dirPath);
                }
                blobSource.DownloadToFile(localDestination, FileMode.Create);
                strFileDownloaded = localDestination;
            }

            Log.Info("File Downloaded to : {0}", strFileDownloaded);

            return strFileDownloaded;
        }

        private string  getDependentTaskID(int p)
        {
            string requiredFileName = string.Empty;
            switch (p)
            {
                case TaskIds.Reslice:
                case TaskIds.Reslice1:
                    break;
                case TaskIds.SkullStrip:
                    requiredFileName = "resliced_brain.nii";
                    break;
                case TaskIds.SkullStrip1:
                    requiredFileName = "resliced_tissue.nii";
                    break;
            }
            return requiredFileName;
        }

        protected override JobResult RunExternalMergeProcess(
            ITask mergeTask,
            TaskExecutionSettings settings)
        {
            var completionFile = LocalPath("completion.txt");

            File.WriteAllText(completionFile, "done");

            return new JobResult
            {
                OutputFile = completionFile
            };
        }
    }
}
