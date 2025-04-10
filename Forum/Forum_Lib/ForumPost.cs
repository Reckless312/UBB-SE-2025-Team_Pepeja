﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forum_Lib
{
    public class ForumPost
    {

        public uint Id { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public int Score { get; set; }
        public string TimeStamp { get; set; }
        public uint AuthorId { get; set; }
        public uint? GameId { get; set; }
    }
}
