namespace cxp.Models;

public class GameContext
{
    public string Command { get; set; } = string.Empty;
    public int Deletions { get; set; } = 0;
    public string CommitMessage { get; set; } = string.Empty;
    public DateTime CurrentTime { get; set; } = DateTime.Now;
}