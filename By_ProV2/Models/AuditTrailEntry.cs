using System;

namespace By_ProV2.Models
{
    public class AuditTrailEntry
    {
        public int Id { get; set; }
        public string TableName { get; set; }
        public int RecordId { get; set; }
        public string Operation { get; set; } // INSERT, UPDATE, DELETE
        public DateTime Timestamp { get; set; }
        public int? UserId { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string FieldName { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public string RecordDescription { get; set; }
    }
}