using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyBase;
using System.IO;

namespace RamPathFixer
{
    public class CncPathFixer : OOBase
    {
        // PROPERTY'S
        public string FilePath { get; set; }

        public String[] orgTxtLines { get; set; } // uitgelezen source file

        public List<RamPoint> OrgRampaPunten { get; set; } // Lijst met origineel ingelezen rampa punten
        public List<RamPoint> NewRampaPunten { get; set; } // Lijst met rampa punten in geoptimaliseerde volgorde

        public override string this[string columnName] => throw new NotImplementedException();

        private int _ramStartPointer = -1; // Regel Pointer in ORG Textfile vanaf waar de rampa's beschreven staan
        private int _ramEndPointer = -1; // Regel waar de laatste RAM bewerking staat

        // CONSTRUCTOR
        // CONSTRUCTOR
        // CONSTRUCTOR
        public CncPathFixer(string filePath)
        {
            OrgRampaPunten = new List<RamPoint>();
            NewRampaPunten = new List<RamPoint>();

            // Path nara bestand instellen
            FilePath = filePath;
        }

        // METHODS
        // METHODS
        // METHODS
        public bool RunFile()
        // File handling starten
        {
            try
            {
                // STart melding
                Console.WriteLine($"{DateTime.Now} - {FilePath} - START REWORK FILE");

                // Uitlezen van TEX FILE en rampa gegevens structureren
                TxtReader();

                // Optimaliseren van volgorde
                OptimizeTsp();

                // Schrijven van text file
                WriteResult();

                Console.WriteLine($"{DateTime.Now} - {FilePath} - END REWORK FILE");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{DateTime.Now} - {FilePath} - Algemene fout bij verwerken bestand");
                return false;
            }
        }

        private void TxtReader()
        // Inlezen van TXT file
        {
            try
            {
                // Inlezen van CNC Resultaatfile van machine
                orgTxtLines = File.ReadAllLines(FilePath);

                // Zoek alle ram punten
                RamSearch(orgTxtLines);
            }
            catch (Exception)
            {
                Console.WriteLine($"{DateTime.Now} - {FilePath} - Uitlezen bestand mislukt");
            }
        }

        private void OptimizeTsp()
        // Optimaliseer volgorde van punten
        {
            if (OrgRampaPunten != null)
            {
                if (OrgRampaPunten.Count != 0)
                {
                    // Maak lijst aan met X/Y punten voor TSP optimizer
                    double[,] tspValues = new double[2, OrgRampaPunten.Count + 1];

                    // START PUNTEN
                    int pointCounter = 0;
                    foreach (RamPoint ram in OrgRampaPunten)
                    {
                        // Het HOMAG eerste punt blijft ook het eerste punt

                        // EIND PUNT
                        tspValues[0, pointCounter] = ram.X__P41; // X EINDE
                        tspValues[1, pointCounter] = ram.Y__P42; // Y EINDE

                        pointCounter++;
                    }

                    // EIND PUNT
                    tspValues[0, pointCounter] = 3050*1000; // X EINDE ==> Eindpunt voor de berekening ligt factor 1000 buiten het bereik van de X-As.
                    tspValues[1, pointCounter] = 1250/2; // Y EINDE ==> Eindpunt ligt centraal in de Y richting

                    // Resultaat na optimalisatie
                    double[,] result = Tsp.do2opt(tspValues);


                    // Zoek naar punten in resultaat datebl en voeg deze toe

                    int joske = result.GetLength(1);

                    for (int i = 0; i < result.GetLength(1) - 1; i++)
                    // Eerste en laatste punt niet verwerken
                    // START op 1 ipv 0
                    // EINDE op lengte-1
                    {
                        double xValue = result[0, i];
                        double yValue = result[1, i];

                        // Zoek dit punt in lijst met originele punten
                        RamPoint ormp = OrgRampaPunten.Where(x => x.X__P41 == xValue && x.Y__P42 == yValue).FirstOrDefault();

                        if (ormp != null)
                        // Er is een rampa punt gevonden, voeg dit toe in de resultaten lijsten
                        {
                            NewRampaPunten.Add(ormp);
                        }
                    }
                }
            }
        }

        private void RamSearch(string[] sourceFileLines)
        // Overloop alle lijnen maak hiervan RAM Objecten
        {
            for (int i = 0; i < sourceFileLines.Length; i++)
            // Alle regels uitlzen en 
            {
                // Loop Line
                string line = sourceFileLines[i];

                if (line.IndexOf(".ram") >= 0)
                // ram staat in deze regel
                {
                    if (_ramStartPointer == -1)
                    // Eerste ram File
                    {
                        _ramStartPointer = i - 3;
                    }

                    // Laatste ram Pointer Instellen
                    _ramEndPointer = i;

                    // Ram Point aanmaken
                    RamPoint rp = new RamPoint(i - 2, sourceFileLines[i - 2], sourceFileLines[i - 1], sourceFileLines[i], sourceFileLines[i - 6], sourceFileLines[i - 5], sourceFileLines[i - 4]);

                    // VOeg punt toe aan lijst
                    OrgRampaPunten.Add(rp);
                }
                else
                // RAM staat niet in deze regel
                {

                }
            }

        }

        private void WriteResult()
        // Schrijven van resultaat
        // Werking:
        // 1) De gelezen textfile regel per regel schrijven tot je aan de rampamoeren bent
        // 2) rampamoeren toevoegen
        // 3) verder schrijven waar je gebleven bent
        // !!!! ER WORDT VERONDERSTELD DAT DE .RAM onderporgrammas steeds bij elkaar staan!!!
        {
            // Kopier origineel bestand
            try
            {
                // Zoek info van bestand
                FileInfo OrgFile = new FileInfo(FilePath);

                // Kopier origineel bestand naar archief
                File.Copy(OrgFile.FullName, $"{ArchiveLog.OrgHomagOptiPath}\\{OrgFile.Name}",true);

                using (StreamWriter txtWriter = new StreamWriter($"{ArchiveLog.NewAfterRamPathFixerPath}\\{OrgFile.Name}"))
                {
                    for (int arrayPointer = 0; arrayPointer < orgTxtLines.Length; arrayPointer++)
                    {
                        if ((arrayPointer >= _ramStartPointer) && (arrayPointer <= _ramEndPointer))
                        // Indeze zone bevinden zich de rampa moeren.. voeg alle rampa's toe
                        {
                            txtWriter.WriteLine("");
                            txtWriter.WriteLine("");
                            txtWriter.WriteLine("");
                            txtWriter.WriteLine("(** ----------  RAMPA // START \\ RAMPA ---------- **)");
                            txtWriter.WriteLine("(** ----------  RAMPA // START \\ RAMPA ---------- **)");
                            txtWriter.WriteLine("(** ----------  RAMPA // START \\ RAMPA ---------- **)");

                            foreach (RamPoint rmp in NewRampaPunten)
                            // Alle rampa punten toevoegen
                            {
                                txtWriter.WriteLine(rmp.PrePosLine1);
                                txtWriter.WriteLine(rmp.PrePosLine2);
                                txtWriter.WriteLine(rmp.PrePosProgrammaLine);

                                txtWriter.WriteLine(rmp.Tittle);
                                txtWriter.WriteLine(rmp.ParameterLine1);
                                txtWriter.WriteLine(rmp.ParameterLine2);
                                txtWriter.WriteLine(rmp.OnderProgrammaLine);
                            }

                            // JUMP naar einde rampa's
                            arrayPointer = _ramEndPointer;

                            txtWriter.WriteLine("(** ----------  RAMPA // EINDE \\ RAMPA ---------- **)");
                            txtWriter.WriteLine("(** ----------  RAMPA // EINDE \\ RAMPA ---------- **)");
                            txtWriter.WriteLine("(** ----------  RAMPA // EINDE\\ RAMPA ---------- **)");
                            txtWriter.WriteLine("");
                            txtWriter.WriteLine("");
                            txtWriter.WriteLine("");
                        }
                        else
                        // Alle andere punten, de lijnen gewoon kopieren van originele file
                        {
                            string line = orgTxtLines[arrayPointer];

                            txtWriter.WriteLine(line);
                        }
                    }
                }

                // SWAP FILE
                // Kopier het verwerkte bestand naar de originele map
                File.Copy($"{ArchiveLog.NewAfterRamPathFixerPath}\\{OrgFile.Name}", OrgFile.FullName, true);

            }
            catch (Exception ex)
            {

                Console.WriteLine($"{DateTime.Now} - {FilePath} - Schrijven naar bestand is mislutk" + Environment.NewLine + Environment.NewLine + ex.Message);
            }
        }

        public override string ToString()
        {
            return FilePath;
        }

        public override void Dispose()
        {
            base.Dispose();
            OrgRampaPunten = null;
            NewRampaPunten = null;
            orgTxtLines = null;
        }
    }

    public class RamPoint
    {
        // PROPERTY'S

        // Gelezen info ui Source File
        public int OrgStartLine { get; set; }

        // PRE POS VOOR V_PUNT
        private string _prePosLine1;
        public string PrePosLine1
        {
            get
            {
                // Zoek naar index van het programma PREPOS
                int idxPrepor = PrePosProgrammaLine.IndexOf("PREPOS");

                if (idxPrepor >= 0)
                // PREPOS BESTAAT ==> Regel moet worden toegevoegd
                {
                    return _prePosLine1;
                }
                else
                // Als er geen prepos wordt aangegeven, geen regel toevoegen,
                {
                    return "";
                }
            }
            set { _prePosLine1 = value; }
        }

        private string _prePosLine2;
        public string PrePosLine2
        {
            get
            {

                // Zoek naar index van het programma PREPOS
                int idxPrepor = PrePosProgrammaLine.IndexOf("PREPOS");

                if (idxPrepor >= 0)
                // PREPOS BESTAAT ==> Regel moet worden toegevoegd
                {
                    return _prePosLine2;
                }
                else
                // Als er geen prepos wordt aangegeven, geen regel toevoegen,
                {
                    return "";
                }
            }
            set { _prePosLine2 = value; }
        }

        private string _prePosProgrammaLine;
        public string PrePosProgrammaLine
        {
            get
            {
                // Zoek naar index van het programma PREPOS
                int idxPrepor = _prePosProgrammaLine.IndexOf("PREPOS");

                if (idxPrepor >= 0)
                // PREPOS BESTAAT ==> Regel moet worden toegevoegd
                {
                    return _prePosProgrammaLine;
                }
                else
                // Als er geen prepos wordt aangegeven, geen regel toevoegen,
                {
                    return "";
                }
            }
            set { _prePosProgrammaLine = value; }
        }


        // RAMPA PUNT
        public string Tittle
        {
            get { return $"(** SCHROEFAUTOMAAT // RampaPathFixer EDIT // ORIGINELE LIJN IN FILE: {OrgStartLine} **)"; }
        }

        public string ParameterLine1 { get; set; }
        public string ParameterLine2 { get; set; }
        public string OnderProgrammaLine { get; set; }


        // Positie
        public double X__P41 // X Positie 
        {
            get
            {
                if (!String.IsNullOrEmpty(ParameterLine1))
                // Als parameterline 1 NIET leeg is
                {
                    int idxP41 = ParameterLine1.IndexOf("P41");
                    int idxP42 = ParameterLine1.IndexOf("P42");

                    if (idxP41 >= 0)
                    // Als de parameter is gevonden er een index is gevonden
                    {
                        string valueString = ParameterLine1.Substring((idxP41 + 4), (idxP42 - (idxP41 + 4)) - 1);
                        valueString = valueString.Substring(0, valueString.IndexOf('.'));

                        if (double.TryParse(valueString, out double result))
                        {
                            // Resultaat * 1000 omdat de lengtes van de X beweging zeer nadelig zijn.
                            // Hier wordt de X value véél groter dan Y en zal de routine de Y route verkiezen ipv X
                            return result*1000;
                        }
                        return 0;

                    }
                    return 0;

                }
                return 0;
            }
        }

        public double Y__P42 // X Positie 
        {
            get
            {

                if (!String.IsNullOrEmpty(ParameterLine1))
                // Als parameterline 1 NIET leeg is
                {
                    int idxP42 = ParameterLine1.IndexOf("P42");
                    int idxP43 = ParameterLine1.IndexOf("P43");

                    if (idxP42 >= 0)
                    // Als de parameter is gevonden er een index is gevonden
                    {
                        string valueString = ParameterLine1.Substring((idxP42 + 4), (idxP43 - (idxP42 + 4)) - 1);
                        valueString = valueString.Substring(0, valueString.IndexOf('.'));

                        if (double.TryParse(valueString, out double result))
                        {
                            return result;
                        }
                        return 0;

                    }
                    return 0;

                }
                return 0;
            }
        }


        // CONSTRUCTOR
        public RamPoint(int orgStartLine, string parameterLine1, string parameterLine2, string onderProgrammaLine, string prePosLine1, string prePosLine2, string prePosProgrammaLine)
        {
            OrgStartLine = orgStartLine;
            ParameterLine1 = parameterLine1;
            ParameterLine2 = parameterLine2;
            OnderProgrammaLine = onderProgrammaLine;

            PrePosLine1 = prePosLine1;
            PrePosLine2 = prePosLine2;
            PrePosProgrammaLine = prePosProgrammaLine;
        }


        // METHODS
        public override string ToString()
        {
            return $"RAMPA: X: {X__P41}mm - Y: {Y__P42}mm";
        }
    }
}

