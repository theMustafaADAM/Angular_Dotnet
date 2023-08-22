using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModelService
{
	public class TokenModel
	{
		[Key]
		public int Id { get; set; }

		// The clientID, where it come from
		[Required]
		public string? ClientID { get; set; }

		// Value of the Token
		[Required]
		public string? Value { get; set; }

		// Get the Token Creation Date
		[Required]
		public DateTime CreatedDate { get; set; }

		// The UserID it was issued to
		[Required]
		public string? UserID { get; set; }

		[Required]
		public DateTime LastModifiedDate { get; set; }

		[Required]
		public DateTime ExpireTime { get; set; }

		[Required]
		public string? EncryptionKeyRt { get; set; }

		[Required]
		public string? EncryptionKeyJwt { get; set; }

		[ForeignKey("UserID")]
		public virtual ApplicationUser? User { get; set; }

	}
}
