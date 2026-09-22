using CloudBackend.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClassLibrary
{
    public class EventRegistrations
    {
        public int StudentId { get; set; }
        public int EventId { get; set; }

        public Student Student { get; set; }
        public Event Event { get; set; }
        public bool CheckedIn { get; set; }
    }
}
