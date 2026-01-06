using System;
using System.Collections.Generic;
using System.Text;

namespace Socigy.OpenSource.DB.Migrations
{
    public interface IMigration
    {
        public long Id { get; set; }
        public string HumanId { get; set; }

        public DateTime AppliedAt { get; set; }
        public bool IsRollback { get; set; }
        public string ExecutedBy { get; set; }
    }
}
