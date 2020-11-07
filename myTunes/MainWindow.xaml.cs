using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace myTunes
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Point startPoint;
        private MusicLib musicLib = new MusicLib();

        public MainWindow()
        {
            InitializeComponent();

            MusicLib musicLib = new MusicLib();

            musicDataGrid.ItemsSource = musicLib.Songs.DefaultView;
            List<string> playlists = new List<string>();
            playlists.Add("All Music");
            playlists.AddRange(musicLib.Playlists);
            playListBox.ItemsSource = playlists;
        }

        private void playButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void musicDataGrid_MouseMove(object sender, MouseEventArgs e)
        {
            Point mousePos = e.GetPosition(null);
            Vector diff = startPoint - mousePos;

            // Start the drag-drop if mouse has moved far enough
            if (e.LeftButton == MouseButtonState.Pressed &&
                (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                //Here's where I am having issues. Can't quite figure out what arguments to give here so
                // so it gives the song Id to be added to the playlist. This may be correct, since I believe
                // this should be giving the contents of the first column, but I could be totally wrong 
                DragDrop.DoDragDrop(musicDataGrid, musicDataGrid.Columns[0], DragDropEffects.Copy);
                Console.WriteLine(musicDataGrid.Columns[0]);
            }
        }

        private void musicDataGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(null);
        }

        private void playListBox_Drop(object sender, DragEventArgs e)
        {
            //In the notes, this event listener is trigered when the mouse button is released
            //The notes also seem to indicate this is where the logic for adding a song to a playlist
            // should be performed. Not sure how this should work. I've tried some things and 
            // nothing has been correct so far.
        }

        private void playListBox_DragOver(object sender, DragEventArgs e)
        {
            //Notes point out that this handler is used to determine whether copying onto the target
            // is allowed or not. Dr. McCown has some pseudocode to help, but I'm struggling to figure out
            // how exactly to implement it. 


            //McCown's code from project pdf:
            Label playlist = sender as Label; 
            if (playlist != null)
            {    
                // Must be on top of a Label in the list box   
                // Get playlist name from playlist.Content} 
            }
        }

        private void PlayListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = e.AddedItems;
            string playlistName = selected[0] as string;

            if (playlistName == "All Music")
            {
                musicDataGrid.ItemsSource = musicLib.Songs.DefaultView;
            }
            else
            {
                if(musicLib.PlaylistExists(playlistName))
                {
                    musicDataGrid.ItemsSource = musicLib.SongsForPlaylist(playlistName).DefaultView;
                }
                else
                {
                    //Playlist doesn't exist, how did we even get here lmao
                }
            }
        }
    }

}
