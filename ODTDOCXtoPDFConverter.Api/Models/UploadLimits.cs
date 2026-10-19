namespace ODTDOCXtoPDFConverter.Api.Models
{
    public static class UploadLimits
    {
        public const long MaxDocumentBytes = 10 * 1024 * 1024;
        public const long MaxVariableDocumentBytes = 1 * 1024 * 1024;
        public const long MaxRequestBytes = 12 * 1024 * 1024;
    }
}