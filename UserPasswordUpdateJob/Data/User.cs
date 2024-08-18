using System;
using System.Collections.Generic;
using System.Text;

namespace UserPasswordUpdateJob.Data
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Status { get; set; }
        public DateTime LastUpdatePwd { get; set; }
    }
}
