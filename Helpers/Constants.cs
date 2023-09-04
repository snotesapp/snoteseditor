namespace BlazorApp1.Helpers
{
    public static class FilePaths
    {
        public const string PackageDirectory = "Package";
        public const string MetaFilePath = "meta.json";
        public const string ProjectFilePath = "jsonFile.json";
        public const string NoteCardsFilePath = "notecards.json";
        public const string PackageFilePath = "package.zip";
       
    }

    public static class DialogTypes
    {
        public enum DialogType
        {
            GeneralSave,
            SaveNote

        }
    }
   
 
}
