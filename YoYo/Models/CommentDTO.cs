using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace YoYo.Models
{
    public class CommentDTO
    {
        public int Id { get; set; }

        public string Text { get; set; }

        public string Username { get; set; }

        public string Photo { get; set; }
        

        public DateTime Date { get; set; }

        public string DateStr { get{

            return Date.ToShortDateString();

        }
            private set { }
        }
    }
}