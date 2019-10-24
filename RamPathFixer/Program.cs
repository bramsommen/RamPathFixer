using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using MyBase;

namespace RamPathFixer
{
    class Program
    {
        static void Main(string[] args)
        {
            Run(); // Start APP

            // Screen Hold
            Console.ReadLine();
        }


        private static void Run()
        {
            try
            {
                using (StreamReader reader = new StreamReader("C:\\RamPathFixer\\Settings.txt"))
                {
                    // Lees PATH naar programma bestanden
                    // Lees PATH naar programma bestanden
                    // Lees PATH naar programma bestanden
                    string[] MainCNCResultFile = reader.ReadLine().Split(';');
                    ArchiveLog.FolderPath = MainCNCResultFile[0];
                    // TEST PATH
                    List<FileInfo> xxx = (new DirectoryInfo(ArchiveLog.FolderPath)).GetFiles().ToList();



                    // Lees PATH naar folder met kopies van originele homag bestanden
                    // Lees PATH naar folder met kopies van originele homag bestanden
                    // Lees PATH naar folder met kopies van originele homag bestanden
                    string[] OrgHomagOptiPath = reader.ReadLine().Split(';');
                    ArchiveLog.OrgHomagOptiPath = OrgHomagOptiPath[0];
                    // TEST PATH
                    xxx = (new DirectoryInfo(ArchiveLog.OrgHomagOptiPath)).GetFiles().ToList();



                    // Lees path naar folder met hetwerkte bestanden
                    // Lees path naar folder met hetwerkte bestanden
                    // Lees path naar folder met hetwerkte bestanden
                    string[] NewAfterRamPathFixerPath = reader.ReadLine().Split(';');
                    ArchiveLog.NewAfterRamPathFixerPath = NewAfterRamPathFixerPath[0];
                    // TEST PATH
                    xxx = (new DirectoryInfo(ArchiveLog.NewAfterRamPathFixerPath)).GetFiles().ToList();
                }

                // Als het bestand correct is uitgelezen
                // INstantie van initialisatie methode
                MyBase.ServiceShell.InitProcess = new ServiceHandler();

                // OPC UA verbinding overslaan
                MyBase.ServiceShell.SkipOpcUa = true;

                // Starten van service
                MyBase.ServiceShell.Start();

                Console.WriteLine("Path Fixer is started!");
            }
            catch (Exception ex)
            {

                Console.WriteLine("Read settingsfile FAIL - C:\\RamPathFixer\\Settings.txt" + Environment.NewLine + Environment.NewLine + ex.Message);
            }
        }
    }

    public class ServiceHandler : ProcessHandler
    {
        public override List<SeriviceBase> InitializeProcess()
        {
            try
            {
                List<SeriviceBase> Processes = new List<SeriviceBase>();
                FolderHandler fff = new FolderHandler();

                Processes.Add(fff);

                return Processes;

            }
            catch (Exception)
            {

                return null;
            }
        }

        public override bool DisposeProcess()
        {
            return true;
        }
    }
}
