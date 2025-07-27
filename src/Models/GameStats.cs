using System.Text.Json.Serialization;

namespace cxp.Models;

public class GameStats
{
    [JsonPropertyName("total_commits")]
    public int TotalCommits { get; set; } = 0;

    [JsonPropertyName("total_pushes")]
    public int TotalPushes { get; set; } = 0;

    [JsonPropertyName("last_commit_date")]
    public string LastCommitDate { get; set; } = "1970-01-01";

    [JsonPropertyName("last_push_date")]
    public string LastPushDate { get; set; } = "1970-01-01";

    [JsonPropertyName("consecutive_commit_days")]
    public int ConsecutiveCommitDays { get; set; } = 0;

    [JsonPropertyName("branches_created")]
    public int BranchesCreated { get; set; } = 0;

    [JsonPropertyName("merges_completed")]
    public int MergesCompleted { get; set; } = 0;

    [JsonPropertyName("log_views")]
    public int LogViews { get; set; } = 0;

    [JsonPropertyName("stash_uses")]
    public int StashUses { get; set; } = 0;

    [JsonPropertyName("tags_created")]
    public int TagsCreated { get; set; } = 0;

    [JsonPropertyName("reverts_used")]
    public int RevertsUsed { get; set; } = 0;

    [JsonPropertyName("commits_by_hour")]
    public int[] CommitsByHour { get; set; } = new int[24]; // Index 0 = midnight-1am, 1 = 1am-2am, etc.

    [JsonPropertyName("files_added")]
    public int FilesAdded { get; set; } = 0;

    [JsonPropertyName("files_deleted")]
    public int FilesDeleted { get; set; } = 0;
}