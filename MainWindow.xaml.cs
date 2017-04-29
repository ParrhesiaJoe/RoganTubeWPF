using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;


namespace RoganTubeWPF
{
   public partial class MainWindow : Window
   {
      public ObservableCollection<RoganResult> RoganResults { get { return (ObservableCollection<RoganResult>) GetValue(RoganResultsProperty); } set { SetValue(RoganResultsProperty, value); } }
      public static readonly DependencyProperty RoganResultsProperty = DependencyProperty.Register("RoganResults", 
         typeof(ObservableCollection<RoganResult>), typeof(MainWindow), new PropertyMetadata(default(ObservableCollection<RoganResult>)));

      public Dictionary<string, int> AppearancesByKeyword { get { return (Dictionary<string, int>) GetValue(AppearancesByKeywordProperty); } set { SetValue(AppearancesByKeywordProperty, value); } }
      public static readonly DependencyProperty AppearancesByKeywordProperty = DependencyProperty.Register("AppearancesByKeyword", 
         typeof(Dictionary<string, int>), typeof(MainWindow), new PropertyMetadata(default(Dictionary<string, int>)));

      public bool IsGoEnabled { get { return (bool) GetValue(IsGoEnabledProperty); } set { SetValue(IsGoEnabledProperty, value); } }
      public static readonly DependencyProperty IsGoEnabledProperty = DependencyProperty.Register("IsGoEnabled", 
         typeof(bool), typeof(MainWindow), new PropertyMetadata(default(bool)));

      public string CurrentListName { get { return (string) GetValue(CurrentListNameProperty); } set { SetValue(CurrentListNameProperty, value); } }
      public static readonly DependencyProperty CurrentListNameProperty = DependencyProperty.Register("CurrentListName", 
         typeof(string), typeof(MainWindow), new PropertyMetadata(default(string)));

      public string CurrentVideoName { get { return (string) GetValue(CurrentVideoNameProperty); } set { SetValue(CurrentVideoNameProperty, value); } }
      public static readonly DependencyProperty CurrentVideoNameProperty = DependencyProperty.Register("CurrentVideoName", 
         typeof(string), typeof(MainWindow), new PropertyMetadata(default(string)));

      private YouTubeService _service;


      public MainWindow()
      {
         InitializeComponent();
         Loaded += OnLoaded;
      }

      private void OnLoaded(object sender, RoutedEventArgs args)
      {
         ConnectToYouTube();
         IsGoEnabled = true;
      }

      private async void button_Click(object sender, RoutedEventArgs e)
      {
         IsGoEnabled = false;
         await Run();
      }

      private void ConnectToYouTube()
      {
         string apiKey = File.ReadAllText("apiKey.txt");
         var youtubeService = new YouTubeService(new BaseClientService.Initializer()
         {
            ApplicationName = "JoeRoganTube",
            ApiKey = apiKey,
         });
         _service = youtubeService;
      }

      private async Task Run()
      {
         // Joe Rogan's Playlist.
         var playlists = new[]
         {
            // 1 - 199
            "PLk1Sqn_f33KvtMA4mCQSnzGsZe8qsTdzV",
            // 200 - 349
            "PLk1Sqn_f33KtUuWHwNl3ndvbPKlQ6LJZB",
            // 350 - 499
            "PLk1Sqn_f33Kvv8T6ZESpJ2nvEHT9xBhlb",
            // 500 - 700
            "PLk1Sqn_f33KtVQWWnE_V6-sypm5zUMkU6",
            // 700 - now
            "PLk1Sqn_f33KuU_aJDvMPPAy_SoxXTt_ub",
            //"PLk1Sqn_f33KuS7ZSVMJqzFaqOyyl-esmG",
         };
         var roganResults = new ObservableCollection<RoganResult>();
         RoganResults = roganResults;
         var appearances = new Dictionary<string, int>();

         foreach (var playlist in playlists)
         {
            await AddPlaylistToResults(playlist, roganResults, appearances);
         }

         AppearancesByKeyword = appearances;
      }

      private async Task AddPlaylistToResults(string playlistId, ObservableCollection<RoganResult> resultCollection, Dictionary<string, int> appearances)
      {

         var playlistRequest = _service.Playlists.List("snippet");
         playlistRequest.Id = playlistId;

         var playlistResponse = await playlistRequest.ExecuteAsync();
         var playlist = playlistResponse.Items.First();
         CurrentListName = playlist.Snippet.Title;

         var playlistItemRequest = _service.PlaylistItems.List("contentDetails,snippet");
         playlistItemRequest.PlaylistId = playlistId;
         playlistItemRequest.MaxResults = 50;
         playlistItemRequest.PageToken = "";
         
         int index = resultCollection.Count;

         while (playlistItemRequest.PageToken != null)
         {
            var playlistItemResponse = await playlistItemRequest.ExecuteAsync();
            foreach (var playlistItem in playlistItemResponse.Items)
            {
               var videoId = playlistItem.ContentDetails.VideoId;
               var videoRequest = _service.Videos.List("snippet,statistics");
               videoRequest.Id = videoId;
               var videoResponse = await videoRequest.ExecuteAsync();
               if (!videoResponse.Items.Any())
               {
                  var privateEntry = new RoganResult()
                  {
                     Title = "Private: " + playlistItem.Snippet.Title
                  };
                  resultCollection.Add(privateEntry);
                  continue;
               }

               var video = videoResponse.Items.First();
               CurrentVideoName = video.Snippet.Title;
               var stats = video.Statistics;
               var newRogan = new RoganResult()
               {
                  Index = index++,
                  PublishDate = video.Snippet.PublishedAt ?? DateTime.Now,
                  Title = video.Snippet.Title,
                  Likes = (long) (stats.LikeCount ?? 0L),
                  Dislikes = (long) (stats.DislikeCount ?? 0L),
                  Views = (long) (stats.ViewCount ?? 0L),
                  Comments = (long) (stats.CommentCount ?? 0L),
                  Favorites = (long) (stats.FavoriteCount ?? 0L),
                  Tags = video.Snippet.Tags
               };
               AddTags(appearances, video.Snippet.Tags);
               resultCollection.Add(newRogan);
            }
            playlistItemRequest.PageToken = playlistItemResponse.NextPageToken;
         }
      }

      private void AddTags(Dictionary<string, int> appearances, IList<string> tags)
      {
         foreach (var tag in tags)
         {
            if (appearances.ContainsKey(tag))
            {
               appearances[tag]++;
            }
            else
            {
               appearances[tag] = 1;
            }
         }


      }
   }
}
