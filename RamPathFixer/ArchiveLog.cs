using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RamPathFixer
{
    public static class ArchiveLog
    {
        // Lijst van reeds verwerkte bestanden
        public static List<FileInfo> FilesVerwerkt = new List<FileInfo>();

        public static List<FileInfo> FilesToRework = new List<FileInfo>();

        // PAD waar de hoofd CNC programma's na HOMAG optimalisatie worden geplaatst
        public static string FolderPath { get; set; }

        // PAD waarnaar de originele CNC programma's worden gecompierd

        public static string OrgHomagOptiPath { get; set; }

        // PAD waar de bewerkte CNC programma's worden bewaard
        public static string NewAfterRamPathFixerPath { get; set; }
    }
}
