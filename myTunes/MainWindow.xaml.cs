using Microsoft.Win32;
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
        private MusicLib musicLib;
        private List<string> playlists = new List<string>();
        private MediaPlayer mediaPlayer;

        public MainWindow()
        {
            InitializeComponent();

            musicLib = new MusicLib();
            mediaPlayer = new MediaPlayer();

            musicDataGrid.ItemsSource = musicLib.Songs.DefaultView;
            
            playlists.Add("All Music");
            playlists.AddRange(musicLib.Playlists);
            playListBox.ItemsSource = playlists;
        }

        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            DataRowView selected = musicDataGrid.SelectedItem as DataRowView;
            if (selected != null)
            {
                int songID;
                if(selected.Row.ItemArray[0].GetType() == typeof(string))
                {
                    songID = Int32.Parse(selected.Row.ItemArray[0] as string);
                }
                else
                {
                    songID = (int)selected.Row.ItemArray[0];
                }

                Song song = musicLib.GetSong(songID);
                mediaPlayer.Open(new Uri(song.Filename));

                mediaPlayer.Play();
            }
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Stop();
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
                DataRowView selected = musicDataGrid.SelectedItem as DataRowView;
                if (selected != null)
                {
                    int songID;
                    if (selected.Row.ItemArray[0].GetType() == typeof(string))
                    {
                        songID = Int32.Parse(selected.Row.ItemArray[0] as string);
                    }
                    else
                    {
                        songID = (int)selected.Row.ItemArray[0];
                    }
                    //Here's where I am having issues. Can't quite figure out what arguments to give here so
                    // so it gives the song Id to be added to the playlist. This may be correct, since I believe
                    // this should be giving the contents of the first column, but I could be totally wrong 
                    DragDrop.DoDragDrop(musicDataGrid, songID, DragDropEffects.Copy);
                }
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

            int songID = (int)e.Data.GetData(typeof(int));
          
            Label playlist = sender as Label;
            if (playlist != null)
            {
                string playlistName = playlist.Content as string;
                if (musicLib.PlaylistExists(playlistName))
                {
                    musicLib.AddSongToPlaylist(songID, playlistName);

                    string selectedPlaylist = playListBox.SelectedItem as string;
                    if(selectedPlaylist != null && selectedPlaylist == playlistName)
                    {
                        musicDataGrid.ItemsSource = musicLib.SongsForPlaylist(playlistName).DefaultView;
                    }
                }
            }
        }

        private void playListBox_DragOver(object sender, DragEventArgs e)
        {
            //Notes point out that this handler is used to determine whether copying onto the target
            // is allowed or not. Dr. McCown has some pseudocode to help, but I'm struggling to figure out
            // how exactly to implement it. 

            e.Effects = DragDropEffects.None;

            //McCown's code from project pdf:
            Label playlist = sender as Label; 
            if (playlist != null)
            {
                // Must be on top of a Label in the list box   
                // Get playlist name from playlist.Content} 
                string playlistName = playlist.Content as string;
                if (musicLib.PlaylistExists(playlistName))
                {
                    e.Effects = DragDropEffects.Copy;
                }
            }
        }

        private void PlayListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = e.AddedItems;

            if (selected.Count == 0)
                return;

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

        private async void openButton_ClickMethodAsync(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Media files|*.mp3;*.m4a;*.wma;*.wav|MP3 (*.mp3)|*.mp3|M4A (*.m4a)|*.m4a|Windows Media Audio (*.wma)|*.wma|Wave files (*.wav)|*.wav|All files|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                await musicLib.AddSong(openFileDialog.FileName);
            }
        }

        private void newPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            AddPlaylistWindow addPlaylistWindow = new AddPlaylistWindow();
            if(addPlaylistWindow.ShowDialog() == false)
            {
                musicLib.AddPlaylist(addPlaylistWindow.playlistName);
                refreshPlaylists();
            }
        }

        private void refreshPlaylists()
        {
            playlists.Clear();
            playlists.Add("All Music");
            playlists.AddRange(musicLib.Playlists);
            playListBox.ItemsSource = null;
            playListBox.ItemsSource = playlists;
        }

        private void aboutButton_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow aboutWindow = new AboutWindow();
            aboutWindow.ShowDialog();
        }

        private void DeleteSong_Click(object sender, RoutedEventArgs e)
        {
            DataRowView selected = musicDataGrid.SelectedItem as DataRowView;
            int songID;
            if (selected.Row.ItemArray[0].GetType() == typeof(string))
            {
                songID = Int32.Parse(selected.Row.ItemArray[0] as string);
            }
            else
            {
                songID = (int)selected.Row.ItemArray[0];
            }

            string playlist = playListBox.SelectedItem as string;
            if(musicLib.PlaylistExists(playlist))
            {

                int songPosition;
                if (selected.Row.ItemArray[1].GetType() == typeof(string))
                {
                    songPosition = Int32.Parse(selected.Row.ItemArray[1] as string);
                }
                else
                {
                    songPosition = (int)selected.Row.ItemArray[1];
                }

                musicLib.RemoveSongFromPlaylist(songPosition, songID, playlist);
                musicDataGrid.ItemsSource = musicLib.SongsForPlaylist(playlist).DefaultView;
            }
            else if (playlist == "All Music" || playlist == null)
            {
                var dw = new DeleteWindow();
                var result = dw.ShowDialog();
                if (result == true)
                {

                    musicLib.DeleteSong(songID);
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            musicLib.Save();
        }
    }

}
