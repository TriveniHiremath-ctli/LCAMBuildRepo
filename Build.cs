using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Collections;
using System.Collections.ObjectModel;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Framework.Common;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.Build.Client;


namespace LCAMBuild
{
    public partial class frmBuild : Form
    {
        private string logLocation = string.Empty;
        private string releaseNumber;
        private string releaseBuildName;
        private string releaseName;
        private string releaseDirAppend;
        private List<buildProject> projectsToBuild;
        private StreamWriter logFile;
        private StringBuilder sbLog;
        private StringBuilder sbPop;
        private string changeset;
        private DataTable dtReleases;
        public frmBuild()
        {

            InitializeComponent();
            sbLog = new StringBuilder();

            String[] keys = ConfigurationManager.AppSettings.AllKeys;
            String[] projectMembers;
            ArrayList buildItems = new ArrayList();
            string listvalues = string.Empty;
            ArrayList categories = new ArrayList();
            string category = string.Empty;
            Hashtable catProjects = new Hashtable();
            sbPop = new StringBuilder();
           
            

            foreach (string thekey in keys)
            {
                if (thekey.IndexOf("Projects") >= 0)
                {
                    category = thekey.Substring(0, thekey.IndexOf("Projects"));
                    categories.Add(category);
                    listvalues = ConfigurationManager.AppSettings[thekey];
                    projectMembers = listvalues.Split('|');
                    foreach (string memberName in projectMembers)
                    {
                        buildItems.Add(memberName);
                        catProjects.Add(memberName, category);
                        lstBuildOutputs.Items.Add(category + " - " + memberName, true);

                    }
                }
            }
            
            string releaseInfo = ConfigurationManager.AppSettings["ReleaseNumbers"];
            string[] releases = releaseInfo.Split(',');
            dtReleases = new DataTable();
            dtReleases.Columns.Add("ReleaseNumber");
            dtReleases.Columns.Add("ReleaseName");
            dtReleases.Columns.Add("ReleaseBuildName");
            dtReleases.Columns.Add("BuildDirAppend");
            DataRow drRelease;
            
            foreach(string  release in releases)
            {
                drRelease = dtReleases.NewRow();
                string[] theRelease = release.Split('|');
                drRelease["ReleaseNumber"] = theRelease[2];
                drRelease["ReleaseName"] = theRelease[0];
                drRelease["ReleaseBuildName"] = theRelease[1];
                drRelease["BuildDirAppend"] = theRelease[3];
                ddReleases.Items.Add(theRelease[0]);
                dtReleases.Rows.Add(drRelease);
            }
            foreach(object item in ddReleases.Items)
            {
                if(item.ToString() == ConfigurationManager.AppSettings["ReleaseNumbers"])
                {
                    ddReleases.Text = item.ToString();
                }
            }
            

        }

        private void logResults(string logText)
        {
            sbLog.AppendLine(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + " - " + logText);
        }


        private void btnBuild_Click(object sender, EventArgs e)
        {
            try
            {
                sbPop.AppendLine("------------------------   Build For Release " + ddReleases.Text + "    -----------------------");
                sbPop.AppendLine("");
               lblMessage.Text = string.Empty;
                //set up to use the selected release
                bool found = false;
                foreach(DataRow drRelease in dtReleases.Rows )
                {
                    if(ddReleases.Text == drRelease["ReleaseName"].ToString())
                    {
                        found = true;
                        releaseBuildName = drRelease["ReleaseBuildName"].ToString();
                        releaseName = drRelease["ReleaseName"].ToString();
                        releaseNumber = drRelease["ReleaseNumber"].ToString();
                        releaseDirAppend = drRelease["BuildDirAppend"].ToString();
                        ConfigurationManager.AppSettings["Release"] = ddReleases.Text;
                    }
                }
                if(!found)
                {
                    throw new Exception("Release Build FAILED, release: " + ddReleases.Text + " is not a valid Release");
                }

                logResults("Build Process Started for release " + releaseNumber);

                if (BuildTFS())
                {
                    setupDirectories();
                    determineProjectsToBuild();
                    moveToTargets();
                    zipIt();
                    setMessage("Build Process Complete.");
                    logResults("Build Process Complete");
                    sbPop.AppendLine("");
                    sbPop.AppendLine("Build Process Complete");
                }
                writeLog();
            }
            catch (Exception ex)
            {
                logResults(ex.ToString());
                setMessage(ex.Message);
                sbPop.AppendLine(ex.Message);
                writeLog();
            }
            
            MessageBox.Show(sbPop.ToString(),"Build Results");
        }

        private void writeLog()
        {
            if (!Directory.Exists(@"e:\LCAMBuild\RLSE_" + releaseNumber + @"\logs"))
            {
                Directory.CreateDirectory(@"e:\LCAMBuild\RLSE_" + releaseNumber + @"\logs");
            }
            if (File.Exists(@"e:\LCAMBuild\RLSE_" + releaseNumber + @"\logs\BuildReport.log"))
            {
                File.Move(@"e:\LCAMBuild\RLSE_" + releaseNumber + @"\logs\BuildReport.log", @"e:\LCAMBuild\RLSE_" + releaseNumber + @"\logs\BuildReport_" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".log");
            }
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"e:\LCAMBuild\RLSE_" + releaseNumber + @"\logs\BuildReport.log", false))
            {
                file.Write(sbLog.ToString());
            }

        }
        private void frmBuild_Load(object sender, EventArgs e)
        {

        }
        private void setMessage(string messageText)
        {
            lblMessage.Visible = true;
            lblMessage.Text = messageText;

        }
        private bool BuildTFS()
        {
            // Connect to Team Foundation Server
            //     Server is the name of the server that is running the application tier for Team Foundation.
            //     Port is the port that Team Foundation uses. The default port is 8080.
            //     VDir is the virtual path to the Team Foundation application. The default path is tfs.
            Uri tfsUri = new Uri("http://ctomavd254:8080/tfs");
            int intReleaseNumber = int.Parse(releaseNumber.Replace(".",""));
           
            int intStartNum = int.Parse(releaseNumber.Substring(0,2).Replace(".", ""));
            sbPop.AppendLine("Release Number: " + intReleaseNumber + " Major Release: " + intStartNum);
            if (intReleaseNumber >= 1410 || intStartNum > 14)
                tfsUri = new Uri("http://ne1itcdwws04:8080/tfs");

            sbPop.AppendLine("TFS Uri: " + tfsUri);
            TfsConfigurationServer configurationServer =
                TfsConfigurationServerFactory.GetConfigurationServer(tfsUri);

            // Get the catalog of team project collections
            ReadOnlyCollection<CatalogNode> collectionNodes = configurationServer.CatalogNode.QueryChildren(
                new[] { CatalogResourceTypes.ProjectCollection },
                false, CatalogQueryOptions.None);

            // List the team project collections
            IBuildServer buildServer = null;
            string collectionName =  (intReleaseNumber >= 1410 || intStartNum > 14) ? "lcamweb" : "lcamwebctl";

            sbPop.AppendLine("Collection Name: " + collectionName);
            foreach (CatalogNode collectionNode in collectionNodes)
            {
                // Use the InstanceId property to get the team project collection
                Guid collectionId = new Guid(collectionNode.Resource.Properties["InstanceId"]);
                TfsTeamProjectCollection teamProjectCollection = configurationServer.GetTeamProjectCollection(collectionId);

                buildServer = (IBuildServer)teamProjectCollection.GetService(typeof(IBuildServer));
                //buildServer.TeamProjectCollection.DisplayName.IndexOf("TMPam")
               
                if (buildServer != null && buildServer.TeamProjectCollection.Name.ToLower().IndexOf(collectionName) > 0)
                    break;

                // Print the name of the team project collection
                //Console.WriteLine("Collection: " + teamProjectCollection.Name);
                string test = "Collection: " + teamProjectCollection.Name;
                // Get a catalog of team projects for the collection
                ReadOnlyCollection<CatalogNode> projectNodes = collectionNode.QueryChildren(
                    new[] { CatalogResourceTypes.TeamProject },
                    false, CatalogQueryOptions.None);


            }


            //IBuildServer buildServer = (IBuildServer)configurationServer.GetService(typeof(IBuildServer));

            if (buildServer != null)
            {

                IBuildDefinition buildDef = buildServer.GetBuildDefinition("LCAMWEBMAIN", releaseBuildName);
                string fullPath = buildDef.FullPath;

                string buildControllerName = (intReleaseNumber >= 1410 || intStartNum > 14) ? "NE1ITCDWWS04 - Controller" : "CTOMAVD254 - Controller";
                IBuildController buildController = buildServer.GetBuildController(buildControllerName);
                IBuildAgent buildAgent = (IBuildAgent)buildController.Agents[0];
                string buildPath = buildAgent.BuildDirectory;
                //buildPath = buildPath + releaseDirAppend;
                //buildAgent.BuildDirectory = buildPath;
              
                logResults(" ");

                logResults("Building Release: " + releaseBuildName);
                logResults("Build Controller " + buildController.Name);
                logResults("Build Agent " + buildAgent.Name);
                logResults("Build Definition " + buildDef.Name);
                logResults("Build Path " + buildPath);
                logResults(" ");
                logResults("Build Queued");

                IQueuedBuild QB = buildServer.QueueBuild(buildDef);
                
                while (QB.Status == QueueStatus.Queued || QB.Status == QueueStatus.Postponed)
                {
                    Thread.Sleep(5000);
                    QB.Refresh(QueryOptions.All);
                }
                IBuildDetail ibd = QB.Build;

                if (ibd != null)
                {
                    
                    logResults("Build Started");
                    while (QB.Status != QueueStatus.Completed && QB.Status != QueueStatus.Canceled
                    && (ibd.Status == BuildStatus.InProgress || ibd.Status == BuildStatus.NotStarted))
                    {
                        Thread.Sleep(5000);
                        QB.Refresh(QueryOptions.All);
                        ibd.RefreshAllDetails();

                    }
                }
                else
                {
                    setMessage("No Build created - Failed");
                    logResults("No Build created - Failed");
                    sbPop.AppendLine("No Build created - Failed");
                    return false;
                }


                logLocation = ibd.LogLocation.Replace("#", fullPath);



                if (ibd.CompilationStatus == BuildPhaseStatus.Failed)
                {
                    setMessage("Build Compile Failed");
                    setMessage("Change Set: " + ibd.SourceGetVersion);
                    logResults("Build Compile Failed");
                    logResults("Change Set: " + ibd.SourceGetVersion);
                    sbPop.AppendLine("Build Compile Failed, Change Set: " + ibd.SourceGetVersion);
                    return false;
                }
                else if (ibd.Status == BuildStatus.Succeeded)
                {
                    setMessage("Build completed Successfully");
                    setMessage("Change Set: " + ibd.SourceGetVersion);
                    logResults("Build completed Successfully");
                    logResults("Change Set: " + ibd.SourceGetVersion);
                    sbPop.AppendLine("Build completed Successfully, Change Set: " + ibd.SourceGetVersion);
                    return true;
                }
                else
                {
                    setMessage("Build Status: " + ibd.Status.ToString());
                    setMessage("Change Set: " + ibd.SourceGetVersion);
                    logResults("Build Status: " + ibd.Status.ToString());
                    logResults("Change Set: " + ibd.SourceGetVersion);
                    sbPop.AppendLine("Build Status: " + ibd.Status.ToString() + ", Change Set: " + ibd.SourceGetVersion);
                    return false;
                }
            }
            else
            {
                setMessage("No build service found");
                logResults("No build service found");
                sbPop.AppendLine("No build service found");
                return false;
            }

        }
        private void setupDirectories()
        {
            string targetDir = ConfigurationManager.AppSettings["TargetLocation"];
            string sourceDir = ConfigurationManager.AppSettings["SourceLocation"].Replace("#DirAppend", releaseDirAppend);
            
            if (Directory.Exists(targetDir + "RLSE_" + releaseNumber))
            {
                Directory.Move(targetDir + "RLSE_" + releaseNumber, targetDir + "RLSE_" + releaseNumber + "_Archive_" + DateTime.Now.ToString("yyyyMMddhhmmss"));
                logResults("Archived old build: " + targetDir + "RLSE_" + releaseNumber + "_Archive_" + DateTime.Now.ToString("yyyyMMddhhmmss"));
            }
            Directory.CreateDirectory(targetDir + "RLSE_" + releaseNumber + "/logs");
            File.Copy(sourceDir + "Alden.LCAM.Qwest.Development.log", targetDir + "RLSE_" + releaseNumber + "/logs/buildLog.txt");
            logResults("");
            logResults("Build Log: " + targetDir + "RLSE_" + releaseNumber + "/logs/buildLog.txt");

        }

        private void zipIt()
        {
            logResults("");
            logResults("Starting Zip Process");

            string buildDir = ConfigurationManager.AppSettings["TargetLocation"];
            string zipDir = ConfigurationManager.AppSettings["ZipLocation"].Replace("#relno", releaseNumber);

            if (!Directory.Exists(zipDir))
            {
                logResults("Create New zip Directory: " + zipDir);
                Directory.CreateDirectory(zipDir);
            }

            string targetDirectory = string.Empty;
            
            sbPop.AppendLine("");
            
            foreach (buildProject bp in projectsToBuild)
            {
                targetDirectory = bp.targetDirectory;
                string theFile = (zipDir + bp.zipPrefix + bp.zipProjectName + "." + bp.zipPostfix).Replace(".-", "-");
                logResults("     Zipping: " + targetDirectory + " to " + theFile);
                ZipFile.CreateFromDirectory(targetDirectory, theFile);
                sbPop.AppendLine(theFile);
            }
            
        }
        private void determineProjectsToBuild()
        {
            buildProject bp;
            string[] rename;
            string itemDetail = string.Empty;
            int dashInd = 0;
            projectsToBuild = new List<buildProject>();
            sbPop.AppendLine("");
            sbPop.AppendLine("Projects To Install:");
            logResults("");
            logResults("Projects To Build:");
            foreach (int item in lstBuildOutputs.CheckedIndices)
            {
                bp = new buildProject();
                itemDetail = lstBuildOutputs.Items[item].ToString();
                dashInd = itemDetail.IndexOf(" - ");
                bp.category = itemDetail.Substring(0, dashInd);
                bp.projectName = itemDetail.Substring(dashInd + 3);
                bp.zipPrefix = ConfigurationManager.AppSettings[bp.category + "ZipPrefix"];
                bp.zipPostfix = ConfigurationManager.AppSettings[bp.category + "ZipPostfix"].Replace("#relno", releaseNumber);
                bp.zipProjectName = bp.projectName;
                if(ConfigurationManager.AppSettings[bp.category + "ProjectRename"] != null)
                {
                    rename = ConfigurationManager.AppSettings[bp.category + "ProjectRename"].Split('|');
                    if (rename[0] == bp.projectName)
                        bp.zipProjectName = rename[1];
                }
                string[] fileTypes = ConfigurationManager.AppSettings[bp.category + "FileTypes"].Split('|');
                bp.fileTypes = fileTypes.ToList<string>();
                string[] folderExcludes = ConfigurationManager.AppSettings[bp.category + "FolderExclude"].Split('|');
                bp.folderExcludes = folderExcludes.ToList<string>();
                if (bp.projectName == "Service")
                    bp.sourceDirectory = ConfigurationManager.AppSettings[bp.category + "SourceDirectory"].Replace("#relno", releaseNumber).Replace("#DirAppend", releaseDirAppend) + "Alden.LCAM.Qwest.PresentationTier.WebService";
                else if (bp.projectName == "ManagedAssetsService")
                    bp.sourceDirectory = ConfigurationManager.AppSettings[bp.category + "SourceDirectory"].Replace("#relno", releaseNumber).Replace("#DirAppend", releaseDirAppend) + "CenturyLink.LCAM.WebServices";
                else if (bp.projectName == "basxx")
                    bp.sourceDirectory = ConfigurationManager.AppSettings[bp.category + "SourceDirectory"].Replace("#relno", releaseNumber).Replace("#DirAppend", releaseDirAppend) + "CenturyLink.LCAM.EWMRestService";
                else
                    bp.sourceDirectory = ConfigurationManager.AppSettings[bp.category + "SourceDirectory"].Replace("#relno", releaseNumber).Replace("#Project", bp.projectName).Replace("Listener", "Service").Replace("#DirAppend", releaseDirAppend);
                bp.targetDirectory = ConfigurationManager.AppSettings[bp.category + "TargetDirectory"].Replace("#relno", releaseNumber).Replace("#Project", bp.projectName);
                projectsToBuild.Add(bp);
                sbPop.AppendLine("    " + itemDetail);
                logResults("  Building Project: " + itemDetail);
                logResults("     Source Directory: " + bp.sourceDirectory);
                logResults("     Target Directory: " + bp.targetDirectory);
            }
            logResults("");
            logResults("Done-Projects to build");

        }
        private void moveToTargets()
        {
            logResults("");
            logResults("Copy files to targets:");
            logResults("");
            bool createSubs = false;
            foreach (buildProject bp in projectsToBuild)
            {
                createSubs = false;
                if (!Directory.Exists(bp.targetDirectory))
                    Directory.CreateDirectory(bp.targetDirectory);
                foreach (string type in bp.fileTypes)
                {
                    if (type == "folder")
                    {
                        createSubs = true;
                        break;
                    }
                }
                if (createSubs)
                {
                    writeFiles(bp.sourceDirectory, bp, "");
                    foreach (string dir in Directory.GetDirectories(bp.sourceDirectory, "*", SearchOption.AllDirectories))
                    {
                        logResults(bp.sourceDirectory.Length + " " + dir.Substring(dir.IndexOf(bp.sourceDirectory) + bp.sourceDirectory.Length));
                        writeFiles(dir, bp, dir.Substring(dir.IndexOf(bp.sourceDirectory) + bp.sourceDirectory.Length));
                    }
                }
                else
                {
                    foreach (string filename in Directory.GetFiles(bp.sourceDirectory, "*.*", SearchOption.AllDirectories))
                    {
                        if (copyFile(filename, bp))
                        {
                            logResults("copying File: " + filename + " to " + bp.targetDirectory + filename.Substring(filename.LastIndexOf('\\') + 1));
                            File.Copy(filename, bp.targetDirectory + filename.Substring(filename.LastIndexOf('\\') + 1));
                        }
                    }
                }
            }
        }

        private bool copyFile(string filename, buildProject bp)
        {
            foreach (string type in bp.fileTypes)
            {
                string teri = Path.GetExtension(filename);
                if (Path.GetExtension(filename) == "." + type)
                {
                    foreach (string folder in bp.folderExcludes)
                    {
                        if (filename.IndexOf("\\" + folder + "\\") >= 0)
                            return false;
                    }
                    return true;
                }
            }
            return false;
        }

        private void writeFiles(string dir, buildProject bp, string targetPortion)
        {
            foreach (string filename in Directory.GetFiles(dir, "*.*", SearchOption.TopDirectoryOnly))
            {
                if (copyFile(filename, bp))
                {
                    logResults("copying File: " + filename + " to " + bp.targetDirectory + targetPortion + filename.Substring(filename.LastIndexOf('\\') + 1));
                    if (!Directory.Exists(bp.targetDirectory + targetPortion))
                        Directory.CreateDirectory(bp.targetDirectory + targetPortion);
                    logResults("copying File: " + filename + " to " + bp.targetDirectory + targetPortion + "/" + Path.GetFileName(filename));
                    if(File.Exists(bp.targetDirectory + targetPortion + "/" + Path.GetFileName(filename)))
                    {
                        logResults("Target file exists:" + bp.targetDirectory + targetPortion + "/" + Path.GetFileName(filename) + " Created: " + File.GetCreationTime(bp.targetDirectory + targetPortion + "/" + Path.GetFileName(filename)));
                        try
                        {
                            File.Copy(filename, bp.targetDirectory + targetPortion + "/" + Path.GetFileName(filename), true);
                        }
                        catch { }
                    }
                    else
                        File.Copy(filename, bp.targetDirectory + targetPortion + "/" + Path.GetFileName(filename));
  
                   
                }

            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            foreach (int index in lstBuildOutputs.CheckedIndices)
            {
                lstBuildOutputs.SetItemChecked(index, false);
            }

        }

        private void btnDeleteArchives_Click(object sender, EventArgs e)
        {
            foreach (String dirName in Directory.GetDirectories(ConfigurationManager.AppSettings["TargetLocation"]))
            {
                if (dirName.IndexOf("_Archive") >= 0)
                {
                    Directory.Delete(dirName, true);
                }
            }
        }

    }
}
