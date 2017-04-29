using System;
using System.Collections.Generic;

namespace RoganTubeWPF
{
   public class RoganResult
   {
      public string Title { get; set; }
      public int Index { get; set; }
      public DateTime PublishDate { get; set; }
      public long Likes { get; set; }
      public long Dislikes { get; set; }
      public long TotalVotes => Likes + Dislikes;
      public double LikesToTotalVotes => (double)Likes / TotalVotes;

      public long Views { get; set; }
      public double ViewsToLikes => (double) Views/Likes;
      public double ViewsToDislikes => (double) Views/Dislikes;
      public double ViewsPerVote => (double) Views/TotalVotes;
      public double ViewsPerComment => (double)Views / Comments;

      public long Comments { get; set; }
      public double CommentsToVotes => (double) Comments/TotalVotes;
      public double CommentsToLikes => (double) Comments/Likes;
      public double CommentsToDislikes => (double) Comments/Dislikes;

      public long Favorites { get; set; }
      public IList<string> Tags { get; set; }
   }
}