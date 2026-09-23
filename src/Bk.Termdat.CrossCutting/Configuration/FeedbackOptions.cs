using System.ComponentModel.DataAnnotations;

namespace Bk.Termdat.CrossCutting.Configuration;

public class FeedbackOptions
{
    public const string SectionKey = "Feedback";

    [Required]
    public string Email { get; set; }
}
