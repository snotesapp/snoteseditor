using System.Text.Json.Serialization;

namespace BlazorApp1.Helpers
{

    public class GDriveFilesDto
    {
        public string kind { get; set; }
        public bool incompleteSearch { get; set; }
        public GFile[] files { get; set; }
    }

    public class GFile
    {
        public string kind { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string mimeType { get; set; }
    }


    public class GDriveFileId
    {
        public string kind { get; set; }
        public string space { get; set; }
        public string[] ids { get; set; }
    }

}
