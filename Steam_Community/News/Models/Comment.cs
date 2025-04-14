using System;

namespace News
{
    public class Comment
    {
        public int CommentId { get; set; }
        public int PostId { get; set; }
        public int AuthorId { get; set; }
        public string Content { get; set; }
        public DateTime CommentDate { get; set; }
        public int NrLikes { get; set; }
        public int NrDislikes { get; set; }
    }
}