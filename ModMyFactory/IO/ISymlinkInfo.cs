namespace ModMyFactory.IO
{
    interface ISymlinkInfo
    {
        string Name { get; }

        string FullName { get; }

        string DestinationPath { get; set; }

        bool Exists { get; }

        void Delete();
    }
}
