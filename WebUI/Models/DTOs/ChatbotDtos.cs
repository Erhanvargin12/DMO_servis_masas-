using System.Text.Json.Serialization;

namespace WebUI.Models.DTOs
{
    public class ChatbotRequestDto
    {
        [JsonPropertyName("question")]
        public string Question { get; set; } = string.Empty;

        // Alias for UI frontend flexibility
        public string Prompt
        {
            get => Question;
            set => Question = value;
        }

        [JsonPropertyName("client_request_id")]
        public string? ClientRequestId { get; set; }
    }

    public class CitationDto
    {
        [JsonPropertyName("evidence_id")]
        public string EvidenceId { get; set; } = string.Empty;

        [JsonPropertyName("citation_label")]
        public string CitationLabel { get; set; } = string.Empty;

        [JsonPropertyName("source_file")]
        public string SourceFile { get; set; } = string.Empty;

        [JsonPropertyName("source_url")]
        public string? SourceUrl { get; set; }

        [JsonPropertyName("chunk_id")]
        public string ChunkId { get; set; } = string.Empty;
    }

    public class ChatbotResponseDto
    {
        [JsonPropertyName("api_version")]
        public string ApiVersion { get; set; } = "v1";

        [JsonPropertyName("trace_id")]
        public string? TraceId { get; set; }

        [JsonPropertyName("client_request_id")]
        public string? ClientRequestId { get; set; }

        [JsonPropertyName("answer_type")]
        public string? AnswerType { get; set; }

        [JsonPropertyName("question")]
        public string? Question { get; set; }

        [JsonPropertyName("answer")]
        public string Answer { get; set; } = string.Empty;

        [JsonPropertyName("sufficient_evidence")]
        public bool SufficientEvidence { get; set; }

        [JsonPropertyName("citations")]
        public List<CitationDto> Citations { get; set; } = new();

        [JsonPropertyName("citation_compliant")]
        public bool CitationCompliant { get; set; }

        [JsonPropertyName("latency_ms")]
        public double LatencyMs { get; set; }

        [JsonPropertyName("generation_latency_ms")]
        public double GenerationLatencyMs { get; set; }

        [JsonPropertyName("model")]
        public string? Model { get; set; }

        [JsonPropertyName("index_id")]
        public string? IndexId { get; set; }

        // Proxy UI helper fields
        public bool Success { get; set; }
        public string Solution { get; set; } = string.Empty;
    }
}
