using Newtonsoft.Json;
using System;

namespace Blogbook.Models
{

    public class Post
    {
        public int Id { get; set; }
        public string Title { get; set; }
        [JsonProperty("publication_date")]
        public DateTime PublicationDate { get; set; }
        public string Description { get; set; }

        public Microsoft.AspNetCore.Identity.IdentityUser User { get; set; }
    }
}
