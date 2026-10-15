using System;
using System.Collections.Generic;

namespace WebUI.Areas.Tickets.Models
{
    public class FaqArticle
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Icon { get; set; } = "fa-circle-question";
        public string Summary { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public List<string> SolutionSteps { get; set; } = new();
        public List<string> Tags { get; set; } = new();
        public int ViewCount { get; set; }
        public int HelpfulCount { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }

    public class KnowledgeBaseViewModel
    {
        public string? SearchTerm { get; set; }
        public string? CategoryFilter { get; set; }
        public List<FaqArticle> Articles { get; set; } = new();
        public List<string> Categories { get; set; } = new();
        public int TotalArticles => Articles.Count;
    }
}
