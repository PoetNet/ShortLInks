using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ShortLinks.Models
{
    public class Link
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string LinkPath { get; set; } = null!;
        public string ShortLink { get; set; } = null!;
        public byte[] Salt { get; set; } = null!;
        public byte[] PasswordToDel { get; set; } = null!;
    }

    public class LinkDto
    {
        [Required]
        public string LinkPath { get; set; } = null!;
        [Required]
        public string PasswordToDel { get; set; } = null!;
    }

    public class PublicLinkView
    {
        public string LinkPath { get; set; } = null!;
        public string ShortLink { get; set; } = null!;
    }

}
