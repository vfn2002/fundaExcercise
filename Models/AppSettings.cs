namespace Core.Models {
    public class AppSettings {
        public ConnectionStrings ConnectionStrings { get; set; }
        public int RequestPages { get; set; }
        public int RequestSize { get; set; }
        public int DebounceTimerMs { get; set; }
    }

    public class ConnectionStrings {
        public string FundaApi { get; set; }
    }
}