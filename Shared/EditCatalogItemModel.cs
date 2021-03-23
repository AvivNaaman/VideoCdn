using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VideoCdn.Web.Shared
{
    public class EditCatalogItemModel
    {
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }

        public List<string> ResolutionsToRemove { get; set; }
    }
}
