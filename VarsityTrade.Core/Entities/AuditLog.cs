using System;
using System.Collections.Generic;
using System.Text;

namespace VarsityTrade.Core.Entities
{
    public class AuditLog
    {
        public int AuditLogId { get; set; }
        public int? UserId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string Entity { get; set; } = string.Empty;
        public int? EntityId { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;

        public User? User { get; set; }
    }
}
