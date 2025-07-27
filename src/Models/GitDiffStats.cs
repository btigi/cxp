namespace cxp.Models;

public class GitDiffStats
{
    public int TotalChanges { get; set; }
    public int Deletions { get; set; }
    public int FilesAdded { get; set; }
    public int FilesDeleted { get; set; }
}