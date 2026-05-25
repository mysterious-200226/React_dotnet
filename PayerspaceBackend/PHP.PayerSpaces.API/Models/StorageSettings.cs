namespace PHP.QARAdjustmentTool.API.Models
{  
        public class StorageSettings
        {
            public string ConnectionString { get; set; }

            public string AccountName { get; set; }

            public string AccountKey { get; set; }

            public string ContainerName { get; set; }
		public string BucketName { get; set; }
		public string Region { get; set; }
	}
}
