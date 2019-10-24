using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using MyBase;
using System.Linq;

namespace RamPathFixer
{
    public class FolderHandler : SeriviceBase
    {
        // PROPERTY'S
        public override string this[string columnName] => throw new NotImplementedException();

        private string _serviceNaam;
        public override string ServiceNaam
        {
            get { return _serviceNaam; }
            set { _serviceNaam = value; }
        }

        public override event Action<string> ProcessFail;


        // METHODS
        public override void Start()
        {
            ServiceNaam = "CnPath";

            Init();

        }

        private void Init()
        {
            // Starten van Loop Thread
            Task.Run(Cycle);
        }

        private async Task Cycle()
        {
            // Starten van progress bar
            PandaSettings.ProgressBar = true; ;

            // FIRST CYCLE
            await Task.Factory.StartNew(DutyCyle);

            // Einde van actviteit, stop progress bar
            PandaSettings.ProgressBar = false;

            while (!_DisposeLoop)
            // Cyclus continue herhalen
            {
                // Doe het cyclus werk
                await Task.Factory.StartNew(DutyCyle);
                await Task.Delay(100);
            }
        }


        private async void DutyCyle()
        // Verwerken van alle bestanden.
        {
            try
            {
                Console.WriteLine($"{DateTime.Now} - MAIN CYLE -- START CYLE");
                Console.WriteLine($"{DateTime.Now} - MAIN CYLE -- START CYLE");
                Console.WriteLine($"{DateTime.Now} - MAIN CYLE -- START CYLE");

                Console.WriteLine($"{DateTime.Now} - MAIN CYLE -- DELETE OLD FILES < vandaag");
                DeleteOLdFiles();

                // 1) Maak lijst met bestanden
                FinfFiles();

                foreach (FileInfo item in ArchiveLog.FilesToRework)
                {
                    CncPathFixer c = new CncPathFixer(item.FullName);

                    if (c.RunFile())
                    // Als er file succesvol is verwerkt
                    {
                        // Mocht het bestand al bestaan in het archief verwijder het dan
                        ArchiveLog.FilesVerwerkt.RemoveAll(x => x.Name.Equals(item.Name));

                        // Lees opnieuw de laatste info van de file (= update time stamp)
                        FileInfo fi = new FileInfo(item.FullName);

                        // Voeg dit bestan toe aan het archief
                        ArchiveLog.FilesVerwerkt.Add(fi);
                    }
                    c.Dispose();
                }

                // Clear list met te verwerken items
                ArchiveLog.FilesToRework.Clear();

                Console.WriteLine($"{DateTime.Now} - MAIN CYLE ------------------------------------------ END");
                Console.WriteLine($"{DateTime.Now} - MAIN CYLE ------------------------------------------ END");
                Console.WriteLine($"{DateTime.Now} - MAIN CYLE ------------------------------------------ END");

            }
            catch (Exception ex)
            {
                ProcessFail?.Invoke(ex.Message);
            }
        }

        private void FinfFiles()
        {
            try
            {
                var directory = new DirectoryInfo(ArchiveLog.FolderPath);

                // Lees alle bestanden in die aanwezig zijn in de map
                List<FileInfo> hotFileList = directory.GetFiles().ToList();

                // Vergelijk of de bestanden al bestaan in de lijst met verwerkte bestanden
                // File name en timestamp moet overeenkomen

                foreach (FileInfo file in hotFileList)
                {
                    // Zoek naar bestand in archief met dezelfde naam en write-timestamp
                    FileInfo fl = ArchiveLog.FilesVerwerkt.Where(x => x.Name.Equals(file.Name) && x.LastWriteTime.Equals(file.LastWriteTime)).FirstOrDefault();

                    if (fl == null)
                    // HOT-list is verschillende van archief, bestand moet worden aangepast
                    {
                        if (file.Length > 0)
                        // Bestandsgrote moet meer zijn dan 0
                        {
                            // Voeg bestand toe aan lijst met te verwerken bestanden
                            ArchiveLog.FilesToRework.Add(file);
                        }
                        else
                        {

                        }
                    }
                    else
                    // Dit bestand is niet meer gewijzigd sinds de laatste aanpassingen, dus moet niet worden aangepast
                    {
                        // Doe niets
                    }
                }


                // CLEAN ARCHIVE LIST
                // CLEAN ARCHIVE LIST
                // CLEAN ARCHIVE LIST
                // Alle bestanden die voorkomen in het archief, maar niet in de HOT list moeten niet meer worden bijgehouden

                List<FileInfo> tmpArchive = new List<FileInfo>();

                foreach (FileInfo afile in ArchiveLog.FilesVerwerkt)
                {
                    FileInfo fl = hotFileList.Where(x => x.Name.Equals(afile.Name) && x.LastWriteTime.Equals(afile.LastWriteTime)).FirstOrDefault();

                    if (fl != null)
                    // Bestand bestaat in archief EN HOT list
                    {
                        tmpArchive.Add(fl);
                    }
                    else
                    {
                        Console.WriteLine($"{DateTime.Now} - File= {afile.Name} DELETED FROM ARCHIVE");
                    }
                }

                // Archief wisselen met HOT/ARCHIVE checklist
                ArchiveLog.FilesVerwerkt.Clear();
                ArchiveLog.FilesVerwerkt = tmpArchive;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"{DateTime.Now} - Uitlezen map met bestanden mislukt");
            }
        }

        private void DeleteOLdFiles()
        // Verwijder alle bestanden ouder dan 1 dag uit volgende mappen:
        // 1) Map met prgramma's (standaard folder)
        // 2) Map met kopie van standaard folders
        // 3) map met herwerkte bestanden
        {
            try
            {


                DateTime todayMidnight = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);



                List<FileInfo> programmaRoot = (new DirectoryInfo(ArchiveLog.FolderPath)).GetFiles().ToList().Where(x => x.LastWriteTime < todayMidnight).ToList();
                foreach (FileInfo fl in programmaRoot)
                {
                    File.Delete(fl.FullName);
                }



                List<FileInfo> KopieOrgFIles = (new DirectoryInfo(ArchiveLog.OrgHomagOptiPath)).GetFiles().ToList().Where(x => x.LastWriteTime < todayMidnight).ToList();
                foreach (FileInfo fl in KopieOrgFIles)
                {
                    File.Delete(fl.FullName);
                }


                List<FileInfo> NewAfterFixer = (new DirectoryInfo(ArchiveLog.NewAfterRamPathFixerPath)).GetFiles().ToList().Where(x => x.LastWriteTime < todayMidnight).ToList();
                foreach (FileInfo fl in NewAfterFixer)
                {
                    File.Delete(fl.FullName);
                }

                Console.WriteLine($"{DateTime.Now} - MAIN CYCLE - verwijderen van oude bestanden gelukt");
            }
            catch (Exception ex)
            {

                Console.WriteLine($"{DateTime.Now} - MAIN CYCLE - verwijderen van oude bestanden mislukt" + Environment.NewLine + Environment.NewLine + ex.Message);
            }

        }
    }
}
