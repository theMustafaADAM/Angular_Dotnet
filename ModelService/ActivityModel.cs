using System;
using System.ComponentModel.DataAnnotations;

namespace ModelService
{
	public class ActivityModel
	{
        [Key]
		public int ID { get; set; }
		public string? Type { get; set; }
		public string? IPAddress { get; set; }
		public string? Location { get; set; }
		public string? OperatingSystem { get; set; }
		public string? UserID { get; set; }
		public string? Color { get; set; }
		public string? Icon { get; set; }
        public DateTime Date { get; set; }

	}
}

