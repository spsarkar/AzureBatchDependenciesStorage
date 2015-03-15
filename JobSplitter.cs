using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Azure.Batch.Apps.Cloud;

namespace AzureBatchNiftiProcessing
{
    /// <summary>
    /// Splits a job into tasks.
    /// </summary>
    public class AzureBatchNiftiProcessingJobSplitter : JobSplitter
    {
        /// <summary>
        /// Splits a job into more granular tasks to be processed in parallel.
        /// </summary>
        /// <param name="job">The job to be split.</param>
        /// <param name="settings">Contains information and services about the split request.</param>
        /// <returns>A sequence of tasks to be run on compute nodes.</returns>
        protected override IEnumerable<TaskSpecifier> Split(IJob job, JobSplitSettings settings)
        {
            var reorientTask = new TaskSpecifier
            {
                TaskId = TaskIds.Reslice,
                RequiredFiles = job.Files.Take(1).ToList(),
                Parameters = job.Parameters,                
            };

            var reorientTask2 = new TaskSpecifier
            {
                TaskId = TaskIds.Reslice1,
                RequiredFiles = job.Files.Take(2).ToList(),
                Parameters = job.Parameters,
            };

            var skullStripTask = new TaskSpecifier
            {
                TaskId = TaskIds.SkullStrip,
                Parameters = job.Parameters,
                DependsOn = TaskDependency.OnId(TaskIds.Reslice)
            }.RequiringAllJobFiles(job);

            var skullStripTask2 = new TaskSpecifier
            {
                TaskId = TaskIds.SkullStrip1,
                Parameters = job.Parameters,
                DependsOn = TaskDependency.OnId(TaskIds.Reslice1)
            }.RequiringAllJobFiles(job);

            return new List<TaskSpecifier> { reorientTask, reorientTask2, skullStripTask, skullStripTask2 };
        }
    }
}
