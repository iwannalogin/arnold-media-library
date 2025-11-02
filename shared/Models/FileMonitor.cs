using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

namespace arnold.Models;

public class FileMonitor {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
    public long LibraryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public virtual FileLibrary Library { get; set; } = new FileLibrary();
    public string Directory { get; set; } = string.Empty;
    public bool Recurse { get; set; }
    public string Rule { get; set; } = "*";
    
    public bool IsInclusionRule { get; set; } = true;
    public bool IsExclusionRule => !IsInclusionRule;

    public override string ToString() => Name;

    [NotMapped]
    protected Regex? RuleRegex = null;
    public bool IsMatch( string fullFileName ) {
        if( RuleRegex is null ) RuleRegex = new Regex( Rule, RegexOptions.Compiled | RegexOptions.IgnoreCase );

        var matchesExpression = RuleRegex.IsMatch(fullFileName);
        return matchesExpression ^ IsExclusionRule;
    }

    [ModelBuilder]
    private static void OnModelCreating( ModelBuilder builder ) {
        builder.Entity<FileMonitor>()
            .HasOne( fm => fm.Library )
            .WithMany( fl => fl.Monitors )
            .HasForeignKey( fm => fm.LibraryId );
    }
}