using System;
using System.Collections.Generic;

namespace VaultTools.vault.util
{
    public class HierarchyItem : IComparable<HierarchyItem>
    {
        public long Id { get; set; }
        public string FileName { get; set; }
        public int Level { get; set; }
        public int Version { get; set; }
        public string Path { get; set; }
        public string CheckedInDate { get; set; }
        public string EntityIcon { get; set; }
        public string FileExtension { get; set; }
        public string Material { get; set; }
        public string RevNumber { get; set; }
        public string Status { get; set; }
        public string TotalVolume { get; set; }
        public string HasChild { get; set; } = "Nao";
        public string IsChild { get; set; } = "Nao";
        public List<HierarchyItem> Children { get; set; } = new List<HierarchyItem>();
        public List<string> ExtraCol { get; set; } = new List<string>();

        public int CompareTo(HierarchyItem other)
        {
            Children.Sort();
            return FileName.CompareTo(other.FileName);
        }

        public override string ToString()
        {
            string line = "";
            for (int i = 0; i < Level; i++)
            {
                line = line + ",";
            }

            line = line
                + "\"" + FileName + "\"";
            for (int i = 10; i > Level; i--)
            {
                line = line + ",";
            }
            line = line
                + ",\"" + Level + "\""
                + ",\"" + Version + "\""
                + ",\"" + Path + "\""
                + ",\"" + CheckedInDate + "\""
                + ",\"" + EntityIcon + "\""
                + ",\"" + FileExtension + "\""
                + ",\"" + Material + "\""
                + ",\"" + RevNumber + "\""
                + ",\"" + Status + "\""
                + ",\"" + TotalVolume + "\""
                ;
            foreach (string col in ExtraCol)
            {
                line = line + "," + col;
            }
            return line;
        }
    }
}
